using HarmonyLib;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;

namespace TelepathyV2.Patches
{
    [HarmonyPatch(typeof(FlattenedTroopRoster), "GenerateUniqueNoFromParty")]
    public static class FlattenedTroopRosterPatch
    {
        public static bool Prefix(MobileParty party, int troopIndex, ref int __result)
        {
            int partyIndex = 1;

            try
            {
                if (party?.Party != null)
                    partyIndex = party.Party.Index;
            }
            catch
            {
                partyIndex = 1;
            }

            __result = (partyIndex * 999983 + troopIndex * 100003) % 616841;
            return false;
        }
    }
}
