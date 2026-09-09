namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Existing actor operations; health and status storage stay on the actor.</summary>
    internal interface ISurvivorsDamageAugmentTarget
    {
        bool IsAlive { get; }
        float HealthFraction { get; }
        string DisplayName { get; }
        void ApplyDamageOverTime(float totalDamage, float durationSeconds, string statusId, string source);
        bool ApplyMovementSlow(float multiplier, float durationSeconds);
        void ExecuteFromAugment(string source);
    }

    internal interface ISurvivorsDamageAugmentPort
    {
        SurvivorsDamageAugmentValues Values { get; }
        SurvivorsTemplateTuning Tuning { get; }
        bool IsPlayerBound { get; }
        void HealPlayer(float amount);
        void RestoreBarrier(float amount);
        bool IsEvolutionActive(string upgradeId);
        string ResolveUpgradeName(string upgradeId);
        string ResolveWeaponName(string weaponId);
    }
}
