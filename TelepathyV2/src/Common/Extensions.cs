using HarmonyLib;
using MCM.Abstractions.Base.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;

namespace TelepathyV2
{
    public static class Extensions
    {
        public static bool CanTalkTo(this Hero hero)
        {
            if (hero == null) return false;
            if (hero == Hero.MainHero) return false;

            if (hero.IsDead || Hero.MainHero.IsDead) return false;

            var settings = GlobalSettings<TelepathySettings>.Instance;

            if (hero.IsPrisoner && settings.PreventTalkingToPrisoners)
                return false;

            if (!hero.HasMet && settings.PreventTalkingToHeroesHaveNotMetBefore)
                return false;

            return true;
        }

        public static ConversationSentence GetSentence(this CampaignGameStarter starter, string id)
        {
            var cm = AccessTools.Field(typeof(CampaignGameStarter), "_conversationManager").GetValue(starter) as ConversationManager;
            var sentences = AccessTools.Field(typeof(ConversationManager), "_sentences").GetValue(cm) as List<ConversationSentence>;
            return sentences?.SingleOrDefault(x => x.Id == id);
        }

        public static void BlockSentences(this CampaignGameStarter starter, Func<bool> condition, params string[] sentenceIds)
        {
            foreach (var sentenceId in sentenceIds)
            {
                var s = starter.GetSentence(sentenceId);
                if (s == null)
                    continue;

                var prev = s.OnCondition;
                s.OnCondition = () => condition() && (prev == null || prev());
            }
        }
    }
}
