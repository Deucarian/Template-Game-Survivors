namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsBuildMenuStatsValues
    {
        public SurvivorsBuildMenuStatsValues(
            float damageBonusTotal = 0,
            float surgeDamageBonus = 0,
            float weaponCooldownSeconds = 0,
            float playerMoveSpeed = 0,
            float currentHealth = 0,
            float maxHealth = 0,
            float barrierValue = 0,
            float barrierCapacity = 0,
            float contactInvulnerabilitySeconds = 0,
            float currentPickupAttractRange = 0,
            float currentPickupAttractionSpeed = 0,
            float experienceGainBonus = 0,
            float draftLuckBonus = 0,
            float areaRadiusBonus = 0,
            float orbitRadiusBonus = 0,
            float deathNovaDamage = 0,
            float deathNovaRadius = 0,
            float payloadExplosionRadiusBonus = 0,
            float payloadTriggerRadiusBonus = 0,
            float poisonDamageRatio = 0,
            float bleedDamageRatio = 0,
            float executeThresholdNormalized = 0,
            float lifestealRatio = 0,
            float criticalChanceNormalized = 0,
            float criticalDamageMultiplier = 0,
            int projectileFanBonus = 0,
            int projectilePierceBonus = 0,
            int projectileChainBonus = 0,
            int projectileForkBonus = 0,
            int projectileReturnBonus = 0,
            int payloadCountBonus = 0,
            string pickupPulseLabel = null)
        {
            DamageBonusTotal = damageBonusTotal;
            SurgeDamageBonus = surgeDamageBonus;
            WeaponCooldownSeconds = weaponCooldownSeconds;
            PlayerMoveSpeed = playerMoveSpeed;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            BarrierValue = barrierValue;
            BarrierCapacity = barrierCapacity;
            ContactInvulnerabilitySeconds = contactInvulnerabilitySeconds;
            CurrentPickupAttractRange = currentPickupAttractRange;
            CurrentPickupAttractionSpeed = currentPickupAttractionSpeed;
            ExperienceGainBonus = experienceGainBonus;
            DraftLuckBonus = draftLuckBonus;
            AreaRadiusBonus = areaRadiusBonus;
            OrbitRadiusBonus = orbitRadiusBonus;
            DeathNovaDamage = deathNovaDamage;
            DeathNovaRadius = deathNovaRadius;
            PayloadExplosionRadiusBonus = payloadExplosionRadiusBonus;
            PayloadTriggerRadiusBonus = payloadTriggerRadiusBonus;
            PoisonDamageRatio = poisonDamageRatio;
            BleedDamageRatio = bleedDamageRatio;
            ExecuteThresholdNormalized = executeThresholdNormalized;
            LifestealRatio = lifestealRatio;
            CriticalChanceNormalized = criticalChanceNormalized;
            CriticalDamageMultiplier = criticalDamageMultiplier;
            ProjectileFanBonus = projectileFanBonus;
            ProjectilePierceBonus = projectilePierceBonus;
            ProjectileChainBonus = projectileChainBonus;
            ProjectileForkBonus = projectileForkBonus;
            ProjectileReturnBonus = projectileReturnBonus;
            PayloadCountBonus = payloadCountBonus;
            PickupPulseLabel = pickupPulseLabel;
        }
        public float DamageBonusTotal { get; }
        public float SurgeDamageBonus { get; }
        public float WeaponCooldownSeconds { get; }
        public float PlayerMoveSpeed { get; }
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public float BarrierValue { get; }
        public float BarrierCapacity { get; }
        public float ContactInvulnerabilitySeconds { get; }
        public float CurrentPickupAttractRange { get; }
        public float CurrentPickupAttractionSpeed { get; }
        public float ExperienceGainBonus { get; }
        public float DraftLuckBonus { get; }
        public float AreaRadiusBonus { get; }
        public float OrbitRadiusBonus { get; }
        public float DeathNovaDamage { get; }
        public float DeathNovaRadius { get; }
        public float PayloadExplosionRadiusBonus { get; }
        public float PayloadTriggerRadiusBonus { get; }
        public float PoisonDamageRatio { get; }
        public float BleedDamageRatio { get; }
        public float ExecuteThresholdNormalized { get; }
        public float LifestealRatio { get; }
        public float CriticalChanceNormalized { get; }
        public float CriticalDamageMultiplier { get; }
        public int ProjectileFanBonus { get; }
        public int ProjectilePierceBonus { get; }
        public int ProjectileChainBonus { get; }
        public int ProjectileForkBonus { get; }
        public int ProjectileReturnBonus { get; }
        public int PayloadCountBonus { get; }
        public string PickupPulseLabel { get; }
    }

    internal readonly struct SurvivorsBuildMenuRunInfoValues
    {
        public SurvivorsBuildMenuRunInfoValues(
            bool isEndlessRun = false,
            string currentRunModeDisplayName = null,
            string pacingProfileLabel = null,
            float runTimeSeconds = 0,
            float survivalVictoryTimeSeconds = 0,
            string currentRunMilestoneHudLabel = null,
            string phaseLabel = null,
            int runEscalationLevel = 0,
            int level = 0,
            int killedCount = 0,
            int activeEnemyCount = 0,
            int currentEnemyMaximumAlive = 0,
            int activeEliteCount = 0,
            int activeMinibossCount = 0,
            int activeBossCount = 0,
            int currencyEarned = 0,
            int progressionEarned = 0,
            int draftRerollsRemaining = 0,
            int draftBanishesRemaining = 0,
            int draftSkipBloodShards = 0,
            int waystoneDiscoveryCount = 0,
            int roamingCacheDropCount = 0,
            int arenaShrineTrialCount = 0,
            long metaBloodShards = 0,
            long lifetimeLegacyExperience = 0,
            string currencyDisplayName = null,
            string progressionDisplayName = null,
            string currencyRewardLabel = null)
        {
            IsEndlessRun = isEndlessRun;
            CurrentRunModeDisplayName = currentRunModeDisplayName;
            PacingProfileLabel = pacingProfileLabel;
            RunTimeSeconds = runTimeSeconds;
            SurvivalVictoryTimeSeconds = survivalVictoryTimeSeconds;
            CurrentRunMilestoneHudLabel = currentRunMilestoneHudLabel;
            PhaseLabel = phaseLabel;
            RunEscalationLevel = runEscalationLevel;
            Level = level;
            KilledCount = killedCount;
            ActiveEnemyCount = activeEnemyCount;
            CurrentEnemyMaximumAlive = currentEnemyMaximumAlive;
            ActiveEliteCount = activeEliteCount;
            ActiveMinibossCount = activeMinibossCount;
            ActiveBossCount = activeBossCount;
            CurrencyEarned = currencyEarned;
            ProgressionEarned = progressionEarned;
            DraftRerollsRemaining = draftRerollsRemaining;
            DraftBanishesRemaining = draftBanishesRemaining;
            DraftSkipBloodShards = draftSkipBloodShards;
            WaystoneDiscoveryCount = waystoneDiscoveryCount;
            RoamingCacheDropCount = roamingCacheDropCount;
            ArenaShrineTrialCount = arenaShrineTrialCount;
            MetaBloodShards = metaBloodShards;
            LifetimeLegacyExperience = lifetimeLegacyExperience;
            CurrencyDisplayName = currencyDisplayName;
            ProgressionDisplayName = progressionDisplayName;
            CurrencyRewardLabel = currencyRewardLabel;
        }
        public bool IsEndlessRun { get; }
        public string CurrentRunModeDisplayName { get; }
        public string PacingProfileLabel { get; }
        public float RunTimeSeconds { get; }
        public float SurvivalVictoryTimeSeconds { get; }
        public string CurrentRunMilestoneHudLabel { get; }
        public string PhaseLabel { get; }
        public int RunEscalationLevel { get; }
        public int Level { get; }
        public int KilledCount { get; }
        public int ActiveEnemyCount { get; }
        public int CurrentEnemyMaximumAlive { get; }
        public int ActiveEliteCount { get; }
        public int ActiveMinibossCount { get; }
        public int ActiveBossCount { get; }
        public int CurrencyEarned { get; }
        public int ProgressionEarned { get; }
        public int DraftRerollsRemaining { get; }
        public int DraftBanishesRemaining { get; }
        public int DraftSkipBloodShards { get; }
        public int WaystoneDiscoveryCount { get; }
        public int RoamingCacheDropCount { get; }
        public int ArenaShrineTrialCount { get; }
        public long MetaBloodShards { get; }
        public long LifetimeLegacyExperience { get; }
        public string CurrencyDisplayName { get; }
        public string ProgressionDisplayName { get; }
        public string CurrencyRewardLabel { get; }
    }
}
