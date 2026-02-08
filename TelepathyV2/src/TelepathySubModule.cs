using Bannerlord.UIExtenderEx;
using HarmonyLib;
using MCM.Abstractions.Base.Global;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TelepathyV2
{
    public sealed class TelepathySubModule : MBSubModuleBase
    {
        private Harmony _harmony;
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            // Force settings initialization so MCM can discover it reliably.
            _ = GlobalSettings<TelepathySettings>.Instance;

            _harmony = new Harmony("TelepathyV2.Harmony");
            _harmony.PatchAll(typeof(TelepathySubModule).Assembly);

            var extender = UIExtender.Create("TelepathyV2");
            extender.Register(typeof(TelepathySubModule).Assembly);
            extender.Enable();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (!(game.GameType is Campaign))
                return;

            if (gameStarterObject is CampaignGameStarter starter)
            {
                starter.AddBehavior(new TelepathyBehaviour());
            }
        }
    }
}
