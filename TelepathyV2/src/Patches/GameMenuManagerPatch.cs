using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;

namespace TelepathyV2.Patches
{
    [HarmonyPatch(typeof(GameMenuManager), "ExitToLast")]
    public static class GameMenuManagerPatch
    {
        public static bool Prefix()
        {
            try
            {
                if (Campaign.Current?.CurrentMenuContext != null)
                {
                    var mapState = Game.Current?.GameStateManager?.ActiveState as MapState;
                    mapState?.ExitMenuMode();
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }
    }
}
