using HarmonyLib;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;

namespace TelepathyV2.Patches
{
    [HarmonyPatch(typeof(BarterManager), "BeginPlayerBarter")]
    public static class BarterPatch
    {
        // Oyun ticarət ekranını tam açmaq istəyəndə bu Prefix işə düşəcək
        public static void Prefix(BarterData args)
        {
            // Yalnız bizim Telepathy görüşü zamanı işləsin
            if (TelepathyBehaviour.MeetingInProgress)
            {
                // Əgər oyun hələ rəsmi bir qarşılaşma (Encounter) yaratmayıbsa
                if (PlayerEncounter.Current == null)
                {
                    // BarterData-dan tərəf partiyasını (OtherParty) götürürük
                    // Əgər OtherParty null-dursa (məsələn lord şəhərdədirsə), onun qəhrəman partiyasını götürürük
                    PartyBase opponentParty = args.OtherParty ?? args.OtherHero?.PartyBelongedTo?.Party;

                    if (opponentParty != null)
                    {
                        // SAXTA ENCOUNTER BAŞLATMAQ: 
                        // Oyunun daxili kodları "OpponentParty" axtaranda boşluğa düşməsin deyə
                        PlayerEncounter.Start();
                        PlayerEncounter.Current.SetupFields(PartyBase.MainParty, opponentParty);
                    }
                }
            }
        }
    }
}