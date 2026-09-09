
namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Formats the ordered active momentum labels without owning their gameplay timers.</summary>
    internal static class SurvivorsSurgeHudModel
    {
        public static string Format(in SurvivorsSurgeHudValues values)
        {
            string label = string.Empty;
            if (values.Streak.Active)
            {
                label = $"   Surge T{values.Streak.Tier} {values.Streak.RemainingSeconds:0.0}s";
            }

            if (values.RoamingCache.Active)
            {
                string travel = $"Way {values.RoamingCache.RemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + travel : label + "   " + travel;
            }

            if (values.ArenaShrine.Active)
            {
                string shrine = $"Shrine {values.ArenaShrine.RemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + shrine : label + "   " + shrine;
            }

            if (values.WaystoneFocus.Active)
            {
                string focus = $"Focus {values.WaystoneFocus.RemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + focus : label + "   " + focus;
            }

            if (values.WaystoneChain.Active)
            {
                string chain = $"Chain {values.WaystoneChain.RemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + chain : label + "   " + chain;
            }

            if (values.HordeRushClear.Active)
            {
                string breaker = $"Breaker {values.HordeRushClear.RemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + breaker : label + "   " + breaker;
            }

            if (values.WeaponLoadout.Active)
            {
                string arsenal = $"Arsenal {values.WeaponLoadout.RemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + arsenal : label + "   " + arsenal;
            }

            if (values.PassiveLoadout.Active)
            {
                string harmony = $"Harmony {values.PassiveLoadout.RemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + harmony : label + "   " + harmony;
            }

            if (values.BossRelic.Active)
            {
                string relic = $"Relic {values.BossRelic.RemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + relic : label + "   " + relic;
            }

            if (values.Gem.Active)
            {
                string gem = $"Gem {values.Gem.RemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + gem : label + "   " + gem;
            }

            if (values.EvolutionChain.Active)
            {
                string legend = $"Legend {values.EvolutionChain.RemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + legend : label + "   " + legend;
            }

            if (values.Endless.Active)
            {
                string endless = $"Endless T{values.Endless.Tier} {values.Endless.RemainingSeconds:0.0}s";
                label = string.IsNullOrEmpty(label) ? "   " + endless : label + "   " + endless;
            }

            return label;
        }
    }
}
