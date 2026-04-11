using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using MCM.Abstractions.Base.Global;
using System;
using TaleWorlds.CampaignSystem.Map;

namespace TelepathyV2
{
    public static class HeroHelper
    {
        public static TelepathySettings Settings => GlobalSettings<TelepathySettings>.Instance;

        public static CampaignVec2? TryGetHeroPosition(Hero hero)
        {
            if (hero == null)
                return null;

            var mapPoint = hero.GetMapPoint();
            if (mapPoint != null)
                return mapPoint.Position;

            if (hero.CurrentSettlement != null)
                return hero.CurrentSettlement.Position;

            if (hero.HomeSettlement != null)
                return hero.HomeSettlement.Position;

            return null;
        }

        /// <summary>
        /// Calculates the pigeon delivery cost based on distance and MCM settings.
        /// Formula: Max(BaseCost, Distance * CostPerUnit * Multiplier)
        /// </summary>
        public static int CalculatePigeonCost(Hero hero)
        {
            // Get user-defined parameters from settings
            int minCost = Settings.PigeonBaseCost;
            float multiplier = Settings.PigeonDistanceMultiplier;
            float costPerUnit = Settings.CostPerDistance;

            // If multiplier is 0, it acts as a flat fee regardless of distance
            if (multiplier <= 0f)
                return minCost;

            var playerPos = TryGetHeroPosition(Hero.MainHero);
            var targetPos = TryGetHeroPosition(hero);

            if (playerPos == null || targetPos == null)
                return minCost;

            // Calculate map distance
            float distance = (playerPos.Value - targetPos.Value).Length; 

            // Calculate dynamic cost based on distance
            long calculatedCost = (long)(distance * costPerUnit * multiplier);

            if(calculatedCost > int.MaxValue)
                return int.MaxValue;

            // The final price is the calculated amount, but never lower than the Base Cost
            return Math.Max(minCost, (int)calculatedCost);
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

                float d = s.Position.DistanceSquared(p);
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