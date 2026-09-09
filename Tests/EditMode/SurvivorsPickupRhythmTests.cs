using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsPickupRhythmTests
    {
        [Test]
        public void KillRewardsKeepTheirDistinctCadencesAndSurgeTierCap()
        {
            var port = new StreakPort();
            var streak = new SurvivorsKillStreakRewards(port);
            for (int i = 0; i < 96; i++) streak.RegisterKillStreak(Vector3.zero);
            Assert.AreEqual(96, streak.BestKillStreak);
            Assert.AreEqual(12, streak.StreakBonusDropCount);
            Assert.AreEqual(6, streak.StreakHealthDropCount);
            Assert.AreEqual(4, streak.StreakMagnetDropCount);
            Assert.AreEqual(3, streak.StreakBloodShardDropCount);
            Assert.AreEqual(6, streak.StreakSurgeActivationCount);
            Assert.AreEqual(5, streak.StreakSurgeTier);
            Assert.AreEqual(5.5f, streak.StreakSurgeDamageBonus);
            Assert.AreEqual(10, port.FirstExperienceAmount);
        }

        [Test]
        public void FailedPickupCommandsStillAdvanceStreakAndTempoButDoNotClaimDrops()
        {
            var port = new StreakPort { SpawnSucceeds = false, HealthSucceeds = false };
            var streak = new SurvivorsKillStreakRewards(port);
            for (int i = 0; i < 32; i++) streak.RegisterKillStreak(Vector3.one);
            Assert.AreEqual(0, streak.StreakBonusDropCount);
            Assert.AreEqual(0, streak.StreakHealthDropCount);
            Assert.AreEqual(0, streak.StreakMagnetDropCount);
            Assert.AreEqual(0, streak.StreakBloodShardDropCount);
            Assert.AreEqual(2, streak.StreakSurgeActivationCount);
            Assert.AreEqual(2, port.Feedback.Count);
            Assert.AreEqual("32 Streak: Tempo Surge T2", port.Feedback[1]);
        }

        [Test]
        public void KillWindowAndTempoTimerExpireIndependentlyAndResetClearsDiagnostics()
        {
            var streak = new SurvivorsKillStreakRewards(new StreakPort());
            for (int i = 0; i < 16; i++) streak.RegisterKillStreak(Vector3.zero);
            streak.TickKillStreak(-1f);
            Assert.AreEqual(16, streak.CurrentKillStreak);
            streak.TickKillStreak(3.8f);
            Assert.AreEqual(0, streak.CurrentKillStreak);
            Assert.IsTrue(streak.IsStreakSurgeActive);
            streak.RegisterKillStreak(Vector3.zero);
            Assert.AreEqual(1, streak.CurrentKillStreak);
            Assert.AreEqual(16, streak.BestKillStreak);
            streak.TickStreakSurge(6f);
            Assert.IsFalse(streak.IsStreakSurgeActive);
            Assert.AreEqual(0, streak.StreakSurgeTier);
            Assert.AreEqual(0f, streak.StreakSurgePickupRangeBonus);
            streak.Reset();
            Assert.AreEqual(0, streak.BestKillStreak);
            Assert.AreEqual(0, streak.StreakSurgeActivationCount);
        }

        [Test]
        public void GemComboRefreshesRushButCountsOneActivationPerCombo()
        {
            var tuning = new SurvivorsTemplateTuning { GemRushDurationSeconds = 4f };
            var rhythm = new SurvivorsExperienceComboRewards(() => tuning);
            rhythm.RecordExperienceCombo(0);
            Assert.AreEqual(0, rhythm.CurrentExperienceComboPickupCount);
            rhythm.RecordExperienceCombo(1);
            rhythm.RecordExperienceCombo(2);
            Assert.AreEqual(0, rhythm.GemRushActivationCount);
            rhythm.RecordExperienceCombo(3);
            Assert.AreEqual(1, rhythm.GemRushActivationCount);
            Assert.AreEqual(6, rhythm.CurrentExperienceComboAmount);
            rhythm.TickGemRush(1f);
            rhythm.RecordExperienceCombo(4);
            Assert.AreEqual(4f, rhythm.GemRushRemainingSeconds);
            Assert.AreEqual(1, rhythm.GemRushActivationCount);
            Assert.AreEqual(2, rhythm.ExperienceComboFeedbackCount);
            Assert.That(rhythm.LastExperienceComboFeedbackLabel, Does.Contain("4 Gem Rush: +10 XP"));
        }

        [Test]
        public void ComboPresentationExpiryDoesNotConsumeThePausedGameplayRush()
        {
            var rhythm = new SurvivorsExperienceComboRewards(() => new SurvivorsTemplateTuning { GemRushDurationSeconds = 4f });
            for (int i = 0; i < 3; i++) rhythm.RecordExperienceCombo(1);
            rhythm.TickExperienceComboFeedback(2f);
            Assert.AreEqual(0, rhythm.CurrentExperienceComboPickupCount);
            Assert.AreEqual(0, rhythm.CurrentExperienceComboAmount);
            Assert.AreEqual(string.Empty, rhythm.ActiveExperienceComboFeedbackLabel);
            Assert.AreEqual(4f, rhythm.GemRushRemainingSeconds);
            for (int i = 0; i < 3; i++) rhythm.RecordExperienceCombo(1);
            Assert.AreEqual(2, rhythm.GemRushActivationCount);
            rhythm.Reset();
            Assert.AreEqual(0, rhythm.GemRushActivationCount);
            Assert.IsFalse(rhythm.IsGemRushActive);
            Assert.AreEqual(string.Empty, rhythm.LastGemRushFeedbackLabel);
        }

        [Test]
        public void DisabledRushStillShowsComboFeedbackAndActiveBonusesReadLiveClampedTuning()
        {
            var tuning = new SurvivorsTemplateTuning { GemRushDurationSeconds = 0f };
            var rhythm = new SurvivorsExperienceComboRewards(() => tuning);
            for (int i = 0; i < 3; i++) rhythm.RecordExperienceCombo(1);
            Assert.AreEqual(1, rhythm.ExperienceComboFeedbackCount);
            Assert.AreEqual(0, rhythm.GemRushActivationCount);
            Assert.IsNotEmpty(rhythm.ActiveExperienceComboFeedbackLabel);
            tuning.GemRushDurationSeconds = 3f;
            rhythm.RecordExperienceCombo(1);
            Assert.AreEqual(1, rhythm.GemRushActivationCount);
            tuning.GemRushDamageBonus = -2f;
            tuning.GemRushMoveSpeedBonus = -3f;
            tuning.GemRushPickupRangeBonus = -4f;
            tuning.GemRushCooldownMultiplierBonus = 0.5f;
            Assert.AreEqual(0f, rhythm.GemRushDamageBonus);
            Assert.AreEqual(0f, rhythm.GemRushMoveSpeedBonus);
            Assert.AreEqual(0f, rhythm.GemRushPickupRangeBonus);
            Assert.AreEqual(0f, rhythm.GemRushCooldownMultiplierBonus);
            tuning.GemRushDamageBonus = 2f;
            tuning.GemRushCooldownMultiplierBonus = -0.2f;
            Assert.AreEqual(2f, rhythm.GemRushDamageBonus);
            Assert.AreEqual(-0.2f, rhythm.GemRushCooldownMultiplierBonus);
            rhythm.TickGemRush(3f);
            Assert.AreEqual(0f, rhythm.GemRushDamageBonus);
        }

        private sealed class StreakPort : ISurvivorsStreakRewardPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning { EnemyExperienceReward = 3 };
            public string CurrencyLabel => "Shards";
            public bool SpawnSucceeds = true, HealthSucceeds = true;
            public int FirstExperienceAmount;
            public readonly List<string> Feedback = new List<string>();
            public bool SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount)
            {
                if (kind == SurvivorsPickupKind.Experience && FirstExperienceAmount == 0) FirstExperienceAmount = amount;
                return SpawnSucceeds;
            }
            public bool TryDropHealth(Vector3 position) => HealthSucceeds;
            public void ShowFeedback(string label, Color color) => Feedback.Add(label);
        }
    }
}
