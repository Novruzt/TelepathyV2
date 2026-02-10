using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MCM.Abstractions.Base.Global;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TelepathyV2
{
    public sealed class TelepathyBehaviour : CampaignBehaviorBase
    {
        private static readonly LinkedList<Call> _queue = new LinkedList<Call>();

        private static PlayerEncounter _meetingEncounter;
        private static Hero _meetingHero;

        private static PlayerEncounter _savedEncounter;
        private static LocationEncounter _savedLocation;
        private static Settlement _savedSettlement;

        private static bool _meetingLock;

        private static TelepathySettings Settings => GlobalSettings<TelepathySettings>.Instance;

        public static bool MeetingInProgress => _meetingEncounter != null;

        // Public API (used by UI button / VM mixin)
        public static void CallToTalk(Hero hero)
        {
            if (!CanRequestConversation(hero))
                return;

            if (IsAlreadyQueued(hero))
                return;

            bool pigeon = Settings.PigeonPostMode;

            if (!MeetsRelationshipRequirement(hero, pigeon))
                return;

            Call call = pigeon ? (Call)new PigeonPostCall(hero) : new DelayedCall(hero);

            _queue.AddLast(call);
        }

        public static bool IsAlreadyQueued(Hero hero)
        {
            if (hero == null)
                return false;

            for (var n = _queue.First; n != null; n = n.Next)
            {
                var c = n.Value;
                if (c != null && c.Hero == hero)
                    return true;
            }

            return false;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
            CampaignEvents.ConversationEnded.AddNonSerializedListener(this, OnConversationEnded);
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnGameLoaded(CampaignGameStarter _)
        {
            _meetingLock = false;

            _queue.Clear();
            _meetingEncounter = null;
            _meetingHero = null;

            _savedEncounter = null;
            _savedLocation = null;
            _savedSettlement = null;
        }

        private void OnSessionLaunched(CampaignGameStarter game)
        {
            // Dialog lines used only during a telepathy meeting
            game.AddPlayerLine(
                "tpv2_ask",
                "hero_main_options",
                "tpv2_answer",
                new TextObject("{=TPV2_Ask}I want to ask you something...").ToString(),
                () => MeetingInProgress,
                null,
                100);

            game.AddDialogLine(
                "tpv2_answer",
                "tpv2_answer",
                "tpv2_ask_2",
                new TextObject("{=TPV2_WhatIsIt}What is it?").ToString(),
                null,
                null,
                100);

            // Where are you?
            game.AddPlayerLine(
                "tpv2_ask_where",
                "tpv2_ask_2",
                "tpv2_tell_where",
                new TextObject("{=TPV2_Where}Where are you?").ToString(),
                () => MeetingInProgress && _meetingHero != null,
                null,
                100);

            game.AddDialogLine(
                "tpv2_tell_where",
                "tpv2_tell_where",
                "hero_main_options",
                "{TPV2_LORD_LOCATION_ANSWER}",
                () =>
                {
                    var answer = BuildLocationAnswer();
                    MBTextManager.SetTextVariable("TPV2_LORD_LOCATION_ANSWER", answer, false);
                    return true;
                },
                null,
                100);

            // What are you doing?
            game.AddPlayerLine(
                "tpv2_ask_doing",
                "tpv2_ask_2",
                "tpv2_tell_doing",
                new TextObject("{=TPV2_Doing}What are you doing?").ToString(),
                () => MeetingInProgress && _meetingHero != null,
                null,
                100);

            game.AddDialogLine(
                "tpv2_tell_doing",
                "tpv2_tell_doing",
                "hero_main_options",
                "{TPV2_LORD_OBJECTIVE_ANSWER}",
                () =>
                {
                    var answer = BuildObjectiveAnswer();
                    MBTextManager.SetTextVariable("TPV2_LORD_OBJECTIVE_ANSWER", answer, false);
                    return true;
                },
                null,
                100);

            // Optional: hide vanilla quest lines while meeting is running
            if (Settings.HideQuestDialogLines)
            {
                BlockConversationSentences(game,
                    () => !MeetingInProgress,
                    "hero_give_issue",
                    "hero_task_given",
                    "caravan_create_conversation_1",
                    "main_option_discussions_1");
            }
        }

        private void OnHourlyTick()
        {
            if (!IsPlayerFree())
                return;

            // Advance all queued calls
            for (var n = _queue.First; n != null; n = n.Next)
                n.Value?.HourlyTick();

            // Pick first ready call
            Call ready = null;
            for (var n = _queue.First; n != null; n = n.Next)
            {
                var c = n.Value;
                if (c != null && c.Ready)
                {
                    ready = c;
                    break;
                }
            }

            if (ready == null)
                return;

            if (!MeetsRelationshipRequirement(ready.Hero, ready.IsPigeon))
            {
                _queue.Remove(ready);
                return;
            }

            // settings: dead restriction
            if (!CanTalkToDead(ready.Hero))
            {
                _queue.Remove(ready);
                return;
            }

            if (!CanStartMeetingNow(ready.Hero))
                return;

            _queue.Remove(ready);
            StartMeeting(ready.Hero);
        }

        private void OnConversationEnded(IEnumerable<CharacterObject> _)
        {
            if (_meetingEncounter == null)
                return;

            try { PlayerEncounter.Finish(false); } catch { }

            _meetingEncounter = null;
            _meetingHero = null;

            try
            {
                if (Campaign.Current != null)
                {
                    AccessTools.Property(typeof(Campaign), "PlayerEncounter")?.SetValue(Campaign.Current, _savedEncounter);
                    _savedEncounter = null;

                    AccessTools.Property(typeof(Campaign), "LocationEncounter")?.SetValue(Campaign.Current, _savedLocation);
                    _savedLocation = null;

                    if (Hero.MainHero != null && Hero.MainHero.PartyBelongedTo != null)
                        Hero.MainHero.PartyBelongedTo.CurrentSettlement = _savedSettlement;

                    _savedSettlement = null;
                }
            }
            catch
            {
                _savedEncounter = null;
                _savedLocation = null;
                _savedSettlement = null;
            }
            finally
            {
                _meetingLock = false;

                ForceExitMenuIfNeeded();
            }
        }

        private static void ForceExitMenuIfNeeded()
        {
            try
            {
                if (Campaign.Current?.CurrentMenuContext != null)
                {
                    var mapState = TaleWorlds.Core.Game.Current?.GameStateManager?.ActiveState as TaleWorlds.CampaignSystem.GameState.MapState;
                    mapState?.ExitMenuMode();
                }
            }
            catch { }
        }


        // ========================= RULES / SETTINGS =========================

        public static bool CanRequestConversation(Hero hero)
        {
            if (hero == null) return false;
            if (Hero.MainHero == null) return false;
            if (Campaign.Current == null) return false;

            if (hero == Hero.MainHero) return false;

            if (MeetingInProgress) return false;

            // settings: dead restriction
            var canTalkToDead = CanTalkToDead(hero);

            if (!canTalkToDead)
                return false;

            // settings: met restriction
            bool hasMet;
            try { hasMet = hero.HasMet; }
            catch { hasMet = true; }

            if (!hasMet && Settings.PreventTalkingToHeroesHaveNotMetBefore)
                return false;

            // If hero is a prisoner or in a map event right now, don't allow scheduling
            try
            {
                if (hero.IsPrisoner)
                    return false;

                var hp = hero.PartyBelongedTo;
                if (hp != null && hp.MapEvent != null)
                    return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static bool CanTalkToDead(Hero hero)
        {
            bool alive;
            try
            {
                alive = hero.IsAlive;
            }
            catch
            {
                alive = false;
            }

            if (!alive && Settings.PreventTalkingToDead)
                alive = false;
            else
                alive = true;

            return alive;
        }

        private static bool IsPlayerFree()
        {
            if (Hero.MainHero == null)
                return false;

            try
            {
                if (Hero.MainHero.IsPrisoner)
                    return false;

                var mp = Hero.MainHero.PartyBelongedTo;
                if (mp != null && mp.MapEvent != null)
                    return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static bool CanStartMeetingNow(Hero hero)
        {
            // Re-check settings at meeting time too (hero can die / become not-met / etc)
            if (!CanRequestConversation(hero))
                return false;

            // Meeting requires a valid player party
            try
            {
                var playerParty = Hero.MainHero.PartyBelongedTo?.Party;
                if (playerParty == null)
                    return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        // ========================= MEETING CORE =========================

        private void StartMeeting(Hero hero)
        {
            if (_meetingLock)
                return;

            if (hero == null || Hero.MainHero == null)
                return;

            _meetingLock = true;

            // Əgər player town / menu içindədirsə – məcburi çıx
            ForceExitMenuIfNeeded();

            try
            {
                var playerParty = PartyBase.MainParty;
                if (playerParty == null)
                {
                    _meetingLock = false;
                    return;
                }

                var playerData = new ConversationCharacterData(
                    CharacterObject.PlayerCharacter,
                    playerParty,
                    false, false, false, false, false, false
                );

                var heroData = new ConversationCharacterData(
                    hero.CharacterObject,
                    playerParty,   // 👈 HERO-nun party-si vacib deyil
                    false, false, false, false, false, false
                );

                Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
                Campaign.Current.CurrentConversationContext = ConversationContext.Default;

                _meetingHero = hero;

                CampaignMission.OpenConversationMission(
                    playerData,
                    heroData,
                    "",
                    "",
                    false
                );
            }
            catch
            {
                _meetingHero = null;
            }
            finally
            {
                _meetingLock = false;
            }
        }


        // ========================= DIALOG HELPERS =========================

        private static TextObject BuildLocationAnswer()
        {
            if (_meetingHero == null)
                return new TextObject("{=TPV2_Unknown}...");

            if (ShouldRefuseAnswer())
                return new TextObject("{=TPV2_NotYourBusiness}It's not your business!");

            var loc = HeroHelper.GetClosestSettlement(_meetingHero);

            if (loc == null)
                return new TextObject("{=TPV2_Lost}I'm lost.");

            var answer = _meetingHero.CurrentSettlement != null
                ? new TextObject("{=TPV2_In}I'm in {Settlement}.")
                : new TextObject("{=TPV2_Near}I'm near {Settlement}.");

            answer.SetTextVariable("Settlement", loc.EncyclopediaLinkWithName);
            return answer;
        }

        private static string BuildObjectiveAnswer()
        {
            if (_meetingHero == null)
                return new TextObject("{=TPV2_Unknown}...").ToString();

            if (ShouldRefuseAnswer())
                return new TextObject("{=TPV2_NotYourBusiness}It's not your business!").ToString();

            try
            {
                if (_meetingHero.PartyBelongedTo == null)
                    return new TextObject("{=TPV2_Nothing}Nothing actually.").ToString();
            }
            catch
            {
                return new TextObject("{=TPV2_Nothing}Nothing actually.").ToString();
            }

            return new TextObject("{=TPV2_Moving}I'm on the move.").ToString();
        }

        private static bool ShouldRefuseAnswer()
        {
            if (_meetingHero == null || Hero.MainHero == null)
                return true;

            try
            {
                if (Hero.MainHero.IsEnemy(_meetingHero))
                {
                    Hero kingdomLeader = null;

                    var clan = _meetingHero.Clan;
                    if (clan != null)
                    {
                        var kingdom = clan.Kingdom;
                        if (kingdom != null && kingdom.RulingClan != null)
                            kingdomLeader = kingdom.RulingClan.Leader;
                    }

                    if (kingdomLeader != Hero.MainHero)
                        return true;
                }

                return FactionManager.IsAtWarAgainstFaction(_meetingHero.MapFaction, Hero.MainHero.MapFaction)
                       && !Hero.MainHero.IsFriend(_meetingHero);
            }
            catch
            {
                return true;
            }
        }

        // ========================= CONVERSATION BLOCKING =========================

        private static void BlockConversationSentences(CampaignGameStarter starter, Func<bool> condition, params string[] sentenceIds)
        {
            try
            {
                var conversationManager = AccessTools.Field(typeof(CampaignGameStarter), "_conversationManager")?.GetValue(starter);
                if (conversationManager == null)
                    return;

                var sentencesObj = AccessTools.Field(conversationManager.GetType(), "_sentences")?.GetValue(conversationManager);
                var sentences = sentencesObj as List<ConversationSentence>;
                if (sentences == null)
                    return;

                foreach (var id in sentenceIds)
                {
                    var sentence = sentences.SingleOrDefault(s => s != null && s.Id == id);
                    if (sentence == null)
                        continue;

                    var original = sentence.OnCondition;
                    sentence.OnCondition = () =>
                    {
                        if (!condition())
                            return false;

                        return original == null || original();
                    };
                }
            }
            catch
            {
                // Intentionally ignored: sentence blocking is optional.
            }
        }

        private static bool MeetsRelationshipRequirement(Hero hero, bool isPigeonCall)
        {
            if (hero == null || Hero.MainHero == null)
                return false;

            // If the setting is disabled, always pass.
            if (!Settings.RequireMinimumRelationship)
                return true;

            // Direct telepathy ALWAYS uses the rule.
            // Pigeon uses it only if enabled.
            if (isPigeonCall && !Settings.ApplyRelationshipRuleToPigeon)
                return true;

            int minRel = Settings.MinimumRelationshipToTalk;

            // No requirement? Always pass.
            if (minRel <= -100)
                return true;

            int rel;
            try
            {
                // This is the common API in Bannerlord for hero relations.
                rel = Hero.MainHero.GetRelation(hero);
            }
            catch
            {
                // If relation API is unavailable in your version, fail safe (don't allow).
                return false;
            }

            return rel >= minRel;
        }

        private static bool IsBusyForConversation(Hero hero)
        {
            if (hero == null) return true;

            try
            {
                if (hero.IsPrisoner) return true;

                var mp = hero.PartyBelongedTo;
                if (mp != null && mp.MapEvent != null)
                    return true;
            }
            catch
            {
                return true;
            }

            return false;
        }


        // ========================= CALL TYPES =========================

        private abstract class Call
        {
            protected Call(Hero hero)
            {
                Hero = hero;
                Ready = false;
            }

            public Hero Hero { get; private set; }
            public bool Ready { get; protected set; }

            public abstract bool IsPigeon { get; }
            public abstract void HourlyTick();
        }

        private sealed class DelayedCall : Call
        {
            private int _hoursLeft;
            public override bool IsPigeon => false;

            public DelayedCall(Hero hero) : base(hero)
            {
                _hoursLeft = Math.Max(0, Settings.MinDelayHours);
                Ready = _hoursLeft == 0;
            }

            public override void HourlyTick()
            {
                if (Ready)
                    return;

                _hoursLeft--;
                if (_hoursLeft <= 0)
                    Ready = true;
            }
        }

        private sealed class PigeonPostCall : Call
        {
            public override bool IsPigeon => true;
            private Vec2 _position;
            private bool _returning;

            public PigeonPostCall(Hero hero) : base(hero)
            {
                var start = HeroHelper.TryGetHeroPosition(Hero.MainHero);
                _position = start ?? Vec2.Zero;
                _returning = false;

                // Apply minimum delay even in pigeon mode
                _minDelayLeft = Math.Max(0, Settings.MinDelayHours);
            }

            private int _minDelayLeft;

            public override void HourlyTick()
            {
                if (Ready)
                    return;

                // First: minimum delay gate
                if (_minDelayLeft > 0)
                {
                    _minDelayLeft--;
                    if (_minDelayLeft > 0)
                        return;
                }

                float speed = Math.Max(1f, Settings.PigeonSpeedPerHour);

                Vec2 target;
                if (!_returning)
                {
                    var heroPos = HeroHelper.TryGetHeroPosition(Hero);
                    target = heroPos ?? _position;
                }
                else
                {
                    var playerPos = HeroHelper.TryGetHeroPosition(Hero.MainHero);
                    target = playerPos ?? _position;
                }

                var diff = target - _position;
                var dist = diff.Length;

                if (dist <= speed)
                {
                    _position = target;

                    if (!_returning)
                    {
                        _returning = true;
                        return;
                    }

                    Ready = true;
                    return;
                }

                _position += diff.Normalized() * speed;
            }
        }
    }
}
