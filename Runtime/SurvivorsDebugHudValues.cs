namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Copied observations for one debug draw, never authoritative runtime state.</summary>
    internal readonly struct SurvivorsDebugHudValues
    {
        internal SurvivorsDebugHudValues(
            SurvivorsHudVitals vitals = default,
            string evolutionObjective = default,
            string waystoneCompass = default,
            float runTimeSeconds = default,
            float survivalVictoryTimeSeconds = default,
            string runPhaseLabel = default,
            int runEscalationLevel = default,
            string milestoneLabel = default,
            int activeEnemyCount = default,
            int enemyMaximumAlive = default,
            int killedCount = default,
            int splitterCount = default,
            int summonerCount = default,
            int eliteCount = default,
            int minibossCount = default,
            int bossCount = default,
            string currencyDisplayName = default,
            long metaBloodShards = default,
            float poisonDamageRatio = default,
            float bleedDamageRatio = default,
            float executeThresholdNormalized = default,
            string weaponLabel = default,
            string modeDisplayName = default,
            SurvivorsPacingProfile pacingProfile = default,
            float enemySpawnIntervalSeconds = default,
            float enemySpeedMultiplier = default,
            int currentKillStreak = default,
            int bestKillStreak = default,
            int streakBonusDropCount = default,
            string surgeLabel = default,
            float rewardSelectionTimeoutSeconds = default,
            int draftRerollsRemaining = default,
            int draftBanishesRemaining = default,
            string buildSlotLabel = default,
            string dashLabel = default)
        {
            Vitals = vitals;
            EvolutionObjective = evolutionObjective;
            WaystoneCompass = waystoneCompass;
            RunTimeSeconds = runTimeSeconds;
            SurvivalVictoryTimeSeconds = survivalVictoryTimeSeconds;
            RunPhaseLabel = runPhaseLabel;
            RunEscalationLevel = runEscalationLevel;
            MilestoneLabel = milestoneLabel;
            ActiveEnemyCount = activeEnemyCount;
            EnemyMaximumAlive = enemyMaximumAlive;
            KilledCount = killedCount;
            SplitterCount = splitterCount;
            SummonerCount = summonerCount;
            EliteCount = eliteCount;
            MinibossCount = minibossCount;
            BossCount = bossCount;
            CurrencyDisplayName = currencyDisplayName;
            MetaBloodShards = metaBloodShards;
            PoisonDamageRatio = poisonDamageRatio;
            BleedDamageRatio = bleedDamageRatio;
            ExecuteThresholdNormalized = executeThresholdNormalized;
            WeaponLabel = weaponLabel;
            ModeDisplayName = modeDisplayName;
            PacingProfile = pacingProfile;
            EnemySpawnIntervalSeconds = enemySpawnIntervalSeconds;
            EnemySpeedMultiplier = enemySpeedMultiplier;
            CurrentKillStreak = currentKillStreak;
            BestKillStreak = bestKillStreak;
            StreakBonusDropCount = streakBonusDropCount;
            SurgeLabel = surgeLabel;
            RewardSelectionTimeoutSeconds = rewardSelectionTimeoutSeconds;
            DraftRerollsRemaining = draftRerollsRemaining;
            DraftBanishesRemaining = draftBanishesRemaining;
            BuildSlotLabel = buildSlotLabel;
            DashLabel = dashLabel;
        }
        internal SurvivorsHudVitals Vitals { get; }
        internal string EvolutionObjective { get; }
        internal string WaystoneCompass { get; }
        internal float RunTimeSeconds { get; }
        internal float SurvivalVictoryTimeSeconds { get; }
        internal string RunPhaseLabel { get; }
        internal int RunEscalationLevel { get; }
        internal string MilestoneLabel { get; }
        internal int ActiveEnemyCount { get; }
        internal int EnemyMaximumAlive { get; }
        internal int KilledCount { get; }
        internal int SplitterCount { get; }
        internal int SummonerCount { get; }
        internal int EliteCount { get; }
        internal int MinibossCount { get; }
        internal int BossCount { get; }
        internal string CurrencyDisplayName { get; }
        internal long MetaBloodShards { get; }
        internal float PoisonDamageRatio { get; }
        internal float BleedDamageRatio { get; }
        internal float ExecuteThresholdNormalized { get; }
        internal string WeaponLabel { get; }
        internal string ModeDisplayName { get; }
        internal SurvivorsPacingProfile PacingProfile { get; }
        internal float EnemySpawnIntervalSeconds { get; }
        internal float EnemySpeedMultiplier { get; }
        internal int CurrentKillStreak { get; }
        internal int BestKillStreak { get; }
        internal int StreakBonusDropCount { get; }
        internal string SurgeLabel { get; }
        internal float RewardSelectionTimeoutSeconds { get; }
        internal int DraftRerollsRemaining { get; }
        internal int DraftBanishesRemaining { get; }
        internal string BuildSlotLabel { get; }
        internal string DashLabel { get; }
    }
}
