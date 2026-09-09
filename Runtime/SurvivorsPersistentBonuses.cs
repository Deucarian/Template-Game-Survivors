namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Current profile bonuses projected into a run without giving modifiers storage access.</summary>
    internal readonly struct SurvivorsPersistentBonuses
    {
        public SurvivorsPersistentBonuses(float damage, float maxHealth, float pickupRange, float experienceGain, float draftRerolls)
        {
            Damage = damage;
            MaxHealth = maxHealth;
            PickupRange = pickupRange;
            ExperienceGain = experienceGain;
            DraftRerolls = draftRerolls;
        }

        public float Damage { get; }
        public float MaxHealth { get; }
        public float PickupRange { get; }
        public float ExperienceGain { get; }
        public float DraftRerolls { get; }
    }
}
