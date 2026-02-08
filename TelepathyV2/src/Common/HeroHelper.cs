using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TelepathyV2
{
    public static class HeroHelper
    {
        public static Vec2? TryGetHeroPosition(Hero hero)
        {
            if (hero == null)
                return null;

            var mapPoint = hero.GetMapPoint();
            if (mapPoint != null)
                return mapPoint.Position.ToVec2();

            if (hero.CurrentSettlement != null)
                return hero.CurrentSettlement.Position.ToVec2();

            if (hero.HomeSettlement != null)
                return hero.HomeSettlement.Position.ToVec2();

            return null;
        }

        public static Settlement GetClosestSettlement(Hero hero)
        {
            if (hero == null)
                return null;

            var pos = TryGetHeroPosition(hero);
            if (pos == null)
                return hero.HomeSettlement;

            var p = pos.Value;
            Settlement best = null;
            float bestDist = float.MaxValue;

            foreach (var s in Settlement.All)
            {
                if (s == null)
                    continue;

                if (!s.IsTown && !s.IsCastle && !s.IsVillage)
                    continue;

                float d = s.Position.ToVec2().DistanceSquared(p);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = s;
                }
            }

            return best;
        }
    }
}
