namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsRunMetricsDraftValues
    {
        internal readonly int LevelDrafts, TotalDrafts, PendingLevels, Weapons, WeaponSlots, Passives, PassiveSlots, Evolutions;
        internal SurvivorsRunMetricsDraftValues(int levelDrafts, int totalDrafts, int pendingLevels, int weapons,
            int weaponSlots, int passives, int passiveSlots, int evolutions)
        {
            LevelDrafts = levelDrafts; TotalDrafts = totalDrafts; PendingLevels = pendingLevels; Weapons = weapons;
            WeaponSlots = weaponSlots; Passives = passives; PassiveSlots = passiveSlots; Evolutions = evolutions;
        }
    }

    internal readonly struct SurvivorsRunMetricsCombatValues
    {
        internal readonly int Kills, CollectedExperience, StoredExperience, NextLevelExperience, Overflow;
        internal readonly float DamageTaken;
        internal SurvivorsRunMetricsCombatValues(int kills, int collectedExperience, int storedExperience,
            int nextLevelExperience, int overflow, float damageTaken)
        {
            Kills = kills; CollectedExperience = collectedExperience; StoredExperience = storedExperience;
            NextLevelExperience = nextLevelExperience; Overflow = overflow; DamageTaken = damageTaken;
        }
    }

    internal readonly struct SurvivorsRunMetricsPickupValues
    {
        internal readonly float Range, Speed, PulseInterval;
        internal readonly int Markers, NormalRecycles, MajorRepositions;
        internal SurvivorsRunMetricsPickupValues(float range, float speed, float pulseInterval, int markers,
            int normalRecycles, int majorRepositions)
        {
            Range = range; Speed = speed; PulseInterval = pulseInterval; Markers = markers;
            NormalRecycles = normalRecycles; MajorRepositions = majorRepositions;
        }
    }
}
