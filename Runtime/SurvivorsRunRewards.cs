using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns run bonus accounting, terminal reward idempotence and victory unlock ordering.</summary>
    internal sealed class SurvivorsRunRewards
    {
        private readonly ISurvivorsRunRewardPort _port;

        public SurvivorsRunRewards(ISurvivorsRunRewardPort port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
        }

        public bool Granted { get; private set; }
        public int BonusBloodShards { get; private set; }
        public int BonusLegacyExperience { get; private set; }
        public int BloodShardsEarned { get; private set; }
        public int LegacyExperienceEarned { get; private set; }
        public int EliteRewardGrantCount { get; private set; }
        public int MinibossRewardGrantCount { get; private set; }
        public int BossRewardGrantCount { get; private set; }
        public int ClassUnlockRewardCount { get; private set; }
        public SurvivorsRunRewardSummary LastRunResult { get; private set; }

        public void Reset()
        {
            Granted = false;
            BonusBloodShards = 0;
            BonusLegacyExperience = 0;
            BloodShardsEarned = 0;
            LegacyExperienceEarned = 0;
            EliteRewardGrantCount = 0;
            MinibossRewardGrantCount = 0;
            BossRewardGrantCount = 0;
            ClassUnlockRewardCount = 0;
            LastRunResult = null;
        }

        public void AddBloodShards(int amount) => BonusBloodShards += amount;

        public void GrantMajorEnemyReward(SurvivorsEnemyRole role)
        {
            string rewardId = BasicSurvivorsGame.MinibossRewardId;
            if (role == SurvivorsEnemyRole.Boss) rewardId = BasicSurvivorsGame.BossRewardId;
            else if (IsEliteRole(role)) rewardId = BasicSurvivorsGame.EliteRewardId;
            if (!_port.ProgressionDefinition.TryGetReward(rewardId, out SurvivorsRewardDefinition reward)) return;
            AddReward(reward);
            if (role == SurvivorsEnemyRole.Boss) BossRewardGrantCount++;
            else if (IsEliteRole(role)) EliteRewardGrantCount++;
            else MinibossRewardGrantCount++;
        }

        public void GrantRunRewards(bool victory, SurvivorsRunRewardInput input)
        {
            if (Granted) return;
            SurvivorsMetaProgressionService progression = _port.EnsureProgression();
            bool grantClassUnlock = ShouldGrantVictoryClassUnlockReward(victory, progression);
            if (grantClassUnlock && _port.ProgressionDefinition.TryGetReward(
                BasicSurvivorsGame.EmberVanguardUnlockRewardId, out SurvivorsRewardDefinition reward))
            {
                AddReward(reward);
            }

            LastRunResult = SurvivorsRunRewardCalculator.Calculate(input.Duration, input.Level,
                input.MinibossKills, input.BossKills, victory, BonusBloodShards, BonusLegacyExperience);
            ApplyRunRewardMultiplier(LastRunResult, input.Multiplier);
            BloodShardsEarned = LastRunResult.BloodShardsEarned;
            LegacyExperienceEarned = LastRunResult.LegacyExperienceEarned;
            if (progression.GrantRunRewards(LastRunResult).Succeeded)
            {
                Granted = true;
                if (grantClassUnlock)
                {
                    SurvivorsClassLibraryDefinition library = _port.EnsureClasses();
                    if (progression.UnlockClass(BasicSurvivorsGame.EmberVanguardClassId, library))
                    {
                        ClassUnlockRewardCount++;
                        _port.ShowClassUnlock();
                    }
                }
            }

            _port.ShowRunSummary(victory);
        }

        public static void ApplyRunRewardMultiplier(SurvivorsRunRewardSummary summary, float multiplier)
        {
            if (summary == null) return;
            float resolvedMultiplier = Mathf.Max(0f, multiplier);
            if (Mathf.Approximately(resolvedMultiplier, 1f)) return;
            summary.BloodShardsEarned = ScaleReward(summary.BloodShardsEarned, resolvedMultiplier);
            summary.LegacyExperienceEarned = ScaleReward(summary.LegacyExperienceEarned, resolvedMultiplier);
        }

        private bool ShouldGrantVictoryClassUnlockReward(bool victory, SurvivorsMetaProgressionService progression)
        {
            if (!victory) return false;
            SurvivorsClassLibraryDefinition library = _port.EnsureClasses();
            return progression != null && library != null &&
                !progression.IsClassUnlocked(BasicSurvivorsGame.EmberVanguardClassId, library);
        }

        private void AddReward(SurvivorsRewardDefinition reward)
        {
            BonusBloodShards += reward.CurrencyAmount;
            BonusLegacyExperience += reward.TrackAmount;
        }

        private static int ScaleReward(int amount, float multiplier)
        {
            if (amount <= 0 || multiplier <= 0f) return 0;
            return Mathf.Max(1, Mathf.RoundToInt(amount * multiplier));
        }

        private static bool IsEliteRole(SurvivorsEnemyRole role) =>
            role == SurvivorsEnemyRole.Elite || role == SurvivorsEnemyRole.DreadElite;
    }
}
