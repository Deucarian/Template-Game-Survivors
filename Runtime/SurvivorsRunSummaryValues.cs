using System.Collections.Generic;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Copied observations for one rendered result; no gameplay or persistence commands.</summary>
    internal readonly struct SurvivorsRunSummaryValues
    {
        public SurvivorsRunSummaryValues(SurvivorsRunSummaryOutcome outcome, SurvivorsRunSummaryRewards rewards,
            SurvivorsRunSummaryCombat combat, SurvivorsRunSummaryCollection collection)
        { Outcome = outcome; Rewards = rewards; Combat = combat; Collection = collection; }
        public SurvivorsRunSummaryOutcome Outcome { get; }
        public SurvivorsRunSummaryRewards Rewards { get; }
        public SurvivorsRunSummaryCombat Combat { get; }
        public SurvivorsRunSummaryCollection Collection { get; }
    }

    internal readonly struct SurvivorsRunSummaryOutcome
    {
        public SurvivorsRunSummaryOutcome(
            bool victory = false,
            bool endlessContinuationEnabled = false,
            string themeTitle = null,
            string modeDisplayName = null,
            float runTimeSeconds = 0,
            float survivalVictoryTimeSeconds = 0,
            float targetDurationSeconds = 0,
            float runRewardMultiplier = 0,
            int level = 0,
            int experienceCollected = 0,
            int experience = 0,
            int requiredExperienceForNextLevel = 0)
        {
            Victory = victory;
            EndlessContinuationEnabled = endlessContinuationEnabled;
            ThemeTitle = themeTitle;
            ModeDisplayName = modeDisplayName;
            RunTimeSeconds = runTimeSeconds;
            SurvivalVictoryTimeSeconds = survivalVictoryTimeSeconds;
            TargetDurationSeconds = targetDurationSeconds;
            RunRewardMultiplier = runRewardMultiplier;
            Level = level;
            ExperienceCollected = experienceCollected;
            Experience = experience;
            RequiredExperienceForNextLevel = requiredExperienceForNextLevel;
        }
        public bool Victory { get; }
        public bool EndlessContinuationEnabled { get; }
        public string ThemeTitle { get; }
        public string ModeDisplayName { get; }
        public float RunTimeSeconds { get; }
        public float SurvivalVictoryTimeSeconds { get; }
        public float TargetDurationSeconds { get; }
        public float RunRewardMultiplier { get; }
        public int Level { get; }
        public int ExperienceCollected { get; }
        public int Experience { get; }
        public int RequiredExperienceForNextLevel { get; }
    }

    internal readonly struct SurvivorsRunSummaryRewards
    {
        public SurvivorsRunSummaryRewards(
            int bloodShardsEarnedThisRun = 0,
            int legacyExperienceEarnedThisRun = 0,
            string currencyRewardLabel = null,
            string progressionRewardLabel = null,
            long metaBloodShards = 0,
            long lifetimeLegacyExperience = 0,
            string currencyDisplayName = null,
            string progressionDisplayName = null)
        {
            BloodShardsEarnedThisRun = bloodShardsEarnedThisRun;
            LegacyExperienceEarnedThisRun = legacyExperienceEarnedThisRun;
            CurrencyRewardLabel = currencyRewardLabel;
            ProgressionRewardLabel = progressionRewardLabel;
            MetaBloodShards = metaBloodShards;
            LifetimeLegacyExperience = lifetimeLegacyExperience;
            CurrencyDisplayName = currencyDisplayName;
            ProgressionDisplayName = progressionDisplayName;
        }
        public int BloodShardsEarnedThisRun { get; }
        public int LegacyExperienceEarnedThisRun { get; }
        public string CurrencyRewardLabel { get; }
        public string ProgressionRewardLabel { get; }
        public long MetaBloodShards { get; }
        public long LifetimeLegacyExperience { get; }
        public string CurrencyDisplayName { get; }
        public string ProgressionDisplayName { get; }
    }

    internal readonly struct SurvivorsRunSummaryCombat
    {
        public SurvivorsRunSummaryCombat(
            int killedCount = 0,
            int eliteKilledCount = 0,
            int minibossKilledCount = 0,
            int bossKilledCount = 0,
            float damageTakenThisRun = 0,
            float currentHealth = 0,
            float maxHealth = 0,
            string bestMomentLabel = null,
            float firstEvolutionAcquiredTimeSeconds = -1f,
            string highestChosenRarityLabel = null,
            int bestKillStreak = 0)
        {
            KilledCount = killedCount;
            EliteKilledCount = eliteKilledCount;
            MinibossKilledCount = minibossKilledCount;
            BossKilledCount = bossKilledCount;
            DamageTakenThisRun = damageTakenThisRun;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            BestMomentLabel = bestMomentLabel;
            FirstEvolutionAcquiredTimeSeconds = firstEvolutionAcquiredTimeSeconds;
            HighestChosenRarityLabel = highestChosenRarityLabel;
            BestKillStreak = bestKillStreak;
        }
        public int KilledCount { get; }
        public int EliteKilledCount { get; }
        public int MinibossKilledCount { get; }
        public int BossKilledCount { get; }
        public float DamageTakenThisRun { get; }
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public string BestMomentLabel { get; }
        public float FirstEvolutionAcquiredTimeSeconds { get; }
        public string HighestChosenRarityLabel { get; }
        public int BestKillStreak { get; }
    }

    internal readonly struct SurvivorsRunSummaryCollection
    {
        public SurvivorsRunSummaryCollection(
            IReadOnlyList<string> activeWeaponIds = null,
            int activeWeaponCount = 0,
            int selectedRelicCount = 0,
            int totalRelicCount = 0,
            string selectedRelicLabel = null,
            float currentPickupAttractRange = 0,
            float currentPickupAttractionSpeed = 0,
            float currentPickupMagnetPulseIntervalSeconds = 0,
            int magnetRecallCount = 0,
            int classUnlockRewardCount = 0,
            string unlockedClassDisplayName = null)
        {
            ActiveWeaponIds = activeWeaponIds;
            ActiveWeaponCount = activeWeaponCount;
            SelectedRelicCount = selectedRelicCount;
            TotalRelicCount = totalRelicCount;
            SelectedRelicLabel = selectedRelicLabel;
            CurrentPickupAttractRange = currentPickupAttractRange;
            CurrentPickupAttractionSpeed = currentPickupAttractionSpeed;
            CurrentPickupMagnetPulseIntervalSeconds = currentPickupMagnetPulseIntervalSeconds;
            MagnetRecallCount = magnetRecallCount;
            ClassUnlockRewardCount = classUnlockRewardCount;
            UnlockedClassDisplayName = unlockedClassDisplayName;
        }
        public IReadOnlyList<string> ActiveWeaponIds { get; }
        public int ActiveWeaponCount { get; }
        public int SelectedRelicCount { get; }
        public int TotalRelicCount { get; }
        public string SelectedRelicLabel { get; }
        public float CurrentPickupAttractRange { get; }
        public float CurrentPickupAttractionSpeed { get; }
        public float CurrentPickupMagnetPulseIntervalSeconds { get; }
        public int MagnetRecallCount { get; }
        public int ClassUnlockRewardCount { get; }
        public string UnlockedClassDisplayName { get; }
    }
}
