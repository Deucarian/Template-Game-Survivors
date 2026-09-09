namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsDebugBuildValues
    {
        public SurvivorsDebugBuildValues(
            int activeWeaponCount = 0,
            int maxWeaponSlots = 0,
            string activeWeaponList = null,
            int activePassiveCount = 0,
            int maxPassiveSlots = 0,
            int evolvedWeaponCount = 0,
            int selectedRelicCount = 0,
            int totalRelicCount = 0,
            string selectedRelicList = null,
            float damageBonus = 0f,
            float surgeDamageBonus = 0f,
            float criticalChanceNormalized = 0f,
            float criticalDamageMultiplier = 0f,
            float draftLuckBonus = 0f,
            float weaponCooldownSeconds = 0f,
            float playerMoveSpeed = 0f,
            float currentPickupAttractRange = 0f,
            float currentPickupAttractionSpeed = 0f,
            float currentPickupMagnetPulseIntervalSeconds = 0f,
            float totalExperienceGainBonus = 0f,
            int projectileFanBonus = 0,
            int projectilePierceBonus = 0,
            int projectileChainBonus = 0,
            int projectileForkBonus = 0,
            int projectileReturnBonus = 0,
            float areaRadiusBonus = 0f,
            float orbitRadiusBonus = 0f,
            int burstCountBonus = 0,
            int burstEchoBonus = 0,
            int payloadCountBonus = 0,
            float deathNovaDamage = 0f,
            float deathNovaRadius = 0f,
            float poisonDamageRatio = 0f,
            float bleedDamageRatio = 0f,
            float executeThresholdNormalized = 0f,
            float lifestealRatio = 0f)
        {
            ActiveWeaponCount = activeWeaponCount;
            MaxWeaponSlots = maxWeaponSlots;
            ActiveWeaponList = activeWeaponList;
            ActivePassiveCount = activePassiveCount;
            MaxPassiveSlots = maxPassiveSlots;
            EvolvedWeaponCount = evolvedWeaponCount;
            SelectedRelicCount = selectedRelicCount;
            TotalRelicCount = totalRelicCount;
            SelectedRelicList = selectedRelicList;
            DamageBonus = damageBonus;
            SurgeDamageBonus = surgeDamageBonus;
            CriticalChanceNormalized = criticalChanceNormalized;
            CriticalDamageMultiplier = criticalDamageMultiplier;
            DraftLuckBonus = draftLuckBonus;
            WeaponCooldownSeconds = weaponCooldownSeconds;
            PlayerMoveSpeed = playerMoveSpeed;
            CurrentPickupAttractRange = currentPickupAttractRange;
            CurrentPickupAttractionSpeed = currentPickupAttractionSpeed;
            CurrentPickupMagnetPulseIntervalSeconds = currentPickupMagnetPulseIntervalSeconds;
            TotalExperienceGainBonus = totalExperienceGainBonus;
            ProjectileFanBonus = projectileFanBonus;
            ProjectilePierceBonus = projectilePierceBonus;
            ProjectileChainBonus = projectileChainBonus;
            ProjectileForkBonus = projectileForkBonus;
            ProjectileReturnBonus = projectileReturnBonus;
            AreaRadiusBonus = areaRadiusBonus;
            OrbitRadiusBonus = orbitRadiusBonus;
            BurstCountBonus = burstCountBonus;
            BurstEchoBonus = burstEchoBonus;
            PayloadCountBonus = payloadCountBonus;
            DeathNovaDamage = deathNovaDamage;
            DeathNovaRadius = deathNovaRadius;
            PoisonDamageRatio = poisonDamageRatio;
            BleedDamageRatio = bleedDamageRatio;
            ExecuteThresholdNormalized = executeThresholdNormalized;
            LifestealRatio = lifestealRatio;
        }
        public int ActiveWeaponCount { get; }
        public int MaxWeaponSlots { get; }
        public string ActiveWeaponList { get; }
        public int ActivePassiveCount { get; }
        public int MaxPassiveSlots { get; }
        public int EvolvedWeaponCount { get; }
        public int SelectedRelicCount { get; }
        public int TotalRelicCount { get; }
        public string SelectedRelicList { get; }
        public float DamageBonus { get; }
        public float SurgeDamageBonus { get; }
        public float CriticalChanceNormalized { get; }
        public float CriticalDamageMultiplier { get; }
        public float DraftLuckBonus { get; }
        public float WeaponCooldownSeconds { get; }
        public float PlayerMoveSpeed { get; }
        public float CurrentPickupAttractRange { get; }
        public float CurrentPickupAttractionSpeed { get; }
        public float CurrentPickupMagnetPulseIntervalSeconds { get; }
        public float TotalExperienceGainBonus { get; }
        public int ProjectileFanBonus { get; }
        public int ProjectilePierceBonus { get; }
        public int ProjectileChainBonus { get; }
        public int ProjectileForkBonus { get; }
        public int ProjectileReturnBonus { get; }
        public float AreaRadiusBonus { get; }
        public float OrbitRadiusBonus { get; }
        public int BurstCountBonus { get; }
        public int BurstEchoBonus { get; }
        public int PayloadCountBonus { get; }
        public float DeathNovaDamage { get; }
        public float DeathNovaRadius { get; }
        public float PoisonDamageRatio { get; }
        public float BleedDamageRatio { get; }
        public float ExecuteThresholdNormalized { get; }
        public float LifestealRatio { get; }
    }
}
