using HarmonyLib;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;

namespace TelepathyV2.Patches
{
    [HarmonyPatch(typeof(BarterManager), "BeginPlayerBarter")]
    public static class BarterPatch
    {
        public static void Prefix(BarterData args)
        {
            if (TelepathyBehaviour.MeetingInProgress)
            {
                if (PlayerEncounter.Current == null)
                {
                    PartyBase opponentParty = args.OtherParty ?? args.OtherHero?.PartyBelongedTo?.Party;

                    if (opponentParty != null)
                    {
                        PlayerEncounter.Start();
                        PlayerEncounter.Current.SetupFields(PartyBase.MainParty, opponentParty);
                    }
                }
            }
        }
    }
}