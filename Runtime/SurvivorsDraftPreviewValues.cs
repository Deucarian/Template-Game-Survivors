namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>A per-card copy of displayed stats, including active bonuses. Never authoritative run state.</summary>
    internal struct SurvivorsDraftPreviewValues
    {
        public float ProjectileDamage;
        public float WeaponCooldownSeconds;
        public float PlayerMoveSpeed;
        public float CurrentPickupAttractRange;
        public float CurrentPickupAttractionSpeed;
        public float CurrentPickupMagnetPulseIntervalSeconds;
        public float MaxHealth;
        public float OrbitRadiusBonus;
        public float PayloadExplosionRadiusBonus;
        public float PayloadTriggerRadiusBonus;
        public float PoisonDamageRatio;
        public float BleedDamageRatio;
        public float ExecuteThresholdNormalized;
        public float CriticalChanceNormalized;
        public float CriticalDamageMultiplier;
        public float DraftLuckBonus;
        public float DeathNovaDamage;
        public float DeathNovaRadius;
        public float LifestealRatio;
        public float BarrierCapacity;
        public float BarrierRegenPerSecondBonus;
        public float BarrierOnDamageRatio;
        public float ExperienceGainMultiplierBonus;
        public float AreaRadiusBonus;
        public float PickupMagnetPulseBaseIntervalSeconds;
        public float PickupMagnetPulseMinimumIntervalSeconds;
        public int OrbitBladeBonus;
        public int MeleeTargetBonus;
        public int BurstCountBonus;
        public int BurstEchoBonus;
        public int TargetedBurstSigilBonus;
        public int ProjectileFanBonus;
        public int ProjectilePierceBonus;
        public int ProjectileChainBonus;
        public int ProjectileForkBonus;
        public int ProjectileReturnBonus;
        public int HitscanPierceBonus;
        public int PayloadCountBonus;
    }
}
