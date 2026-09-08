namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Copied bar observations; no health, barrier or experience ownership.</summary>
    internal readonly struct SurvivorsHudVitals
    {
        internal SurvivorsHudVitals(float maxHealth, float currentHealth, float barrierCapacity, float barrierValue,
            int experience, int requiredExperience, int level)
        {
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
            BarrierCapacity = barrierCapacity;
            BarrierValue = barrierValue;
            Experience = experience;
            RequiredExperience = requiredExperience;
            Level = level;
        }
        internal float MaxHealth { get; }
        internal float CurrentHealth { get; }
        internal float BarrierCapacity { get; }
        internal float BarrierValue { get; }
        internal int Experience { get; }
        internal int RequiredExperience { get; }
        internal int Level { get; }
        internal float HealthRatio => MaxHealth <= 0f ? 0f : CurrentHealth / MaxHealth;
        internal float BarrierRatio => BarrierCapacity <= 0f ? 0f : BarrierValue / BarrierCapacity;
        internal float ExperienceRatio => Experience / (float)RequiredExperience;
        internal bool ShowPlayerBarrier => BarrierCapacity > 0.01f;
    }
}
