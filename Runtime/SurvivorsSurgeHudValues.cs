namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsSurgeHudState
    {
        public SurvivorsSurgeHudState(
            bool active,
            float remainingSeconds,
            int tier)
        {
            Active = active;
            RemainingSeconds = remainingSeconds;
            Tier = tier;
        }
        public bool Active { get; }
        public float RemainingSeconds { get; }
        public int Tier { get; }
    }

    internal readonly struct SurvivorsSurgeHudValues
    {
        public SurvivorsSurgeHudValues(
            SurvivorsSurgeHudState streak,
            SurvivorsSurgeHudState roamingCache,
            SurvivorsSurgeHudState arenaShrine,
            SurvivorsSurgeHudState waystoneFocus,
            SurvivorsSurgeHudState waystoneChain,
            SurvivorsSurgeHudState hordeRushClear,
            SurvivorsSurgeHudState weaponLoadout,
            SurvivorsSurgeHudState passiveLoadout,
            SurvivorsSurgeHudState bossRelic,
            SurvivorsSurgeHudState gem,
            SurvivorsSurgeHudState evolutionChain,
            SurvivorsSurgeHudState endless)
        {
            Streak = streak;
            RoamingCache = roamingCache;
            ArenaShrine = arenaShrine;
            WaystoneFocus = waystoneFocus;
            WaystoneChain = waystoneChain;
            HordeRushClear = hordeRushClear;
            WeaponLoadout = weaponLoadout;
            PassiveLoadout = passiveLoadout;
            BossRelic = bossRelic;
            Gem = gem;
            EvolutionChain = evolutionChain;
            Endless = endless;
        }
        public SurvivorsSurgeHudState Streak { get; }
        public SurvivorsSurgeHudState RoamingCache { get; }
        public SurvivorsSurgeHudState ArenaShrine { get; }
        public SurvivorsSurgeHudState WaystoneFocus { get; }
        public SurvivorsSurgeHudState WaystoneChain { get; }
        public SurvivorsSurgeHudState HordeRushClear { get; }
        public SurvivorsSurgeHudState WeaponLoadout { get; }
        public SurvivorsSurgeHudState PassiveLoadout { get; }
        public SurvivorsSurgeHudState BossRelic { get; }
        public SurvivorsSurgeHudState Gem { get; }
        public SurvivorsSurgeHudState EvolutionChain { get; }
        public SurvivorsSurgeHudState Endless { get; }
    }
}
