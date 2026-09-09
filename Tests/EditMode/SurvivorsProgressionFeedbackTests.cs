using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsProgressionFeedbackTests
    {
        [Test]
        public void EvolutionGoalFallbackPublishesBeforePulseAndSurvivesBannerExpiry()
        {
            var h = new Host(); h.Owner.RecordEvolutionGoalFeedback(null, null);
            Assert.AreEqual("Evolution Goal: matching passive for Evolution", h.Evolution.Label);
            CollectionAssert.AreEqual(new[] { "pulse:22:1:0" }, h.Events);
            Assert.AreEqual(2.4f, h.Evolution.RemainingSeconds);
            h.Evolution.Tick(3);
            Assert.IsEmpty(h.Evolution.Label);
            Assert.AreEqual("Evolution Goal: matching passive for Evolution", h.Owner.LastEvolutionGoalFeedbackLabel);
        }

        [Test]
        public void EvolutionReadyRecordsMetricBeforeNameAndPublishesBeforePulse()
        {
            var h = new Host(); h.Owner.RecordEvolutionReadyFeedback(Upgrade("ready"));
            CollectionAssert.AreEqual(new[] { "metric", "name:ready", "pulse:36:0:1" }, h.Events);
            Assert.AreEqual("Evolution Ready: ready", h.Evolution.Label);
            Assert.AreEqual(Color.white, h.Evolution.Accent);
        }

        [Test]
        public void ThrowingNameLeavesMetricButDoesNotPublishReadyHistory()
        {
            var h = new Host { ThrowName = true };
            Assert.Throws<InvalidOperationException>(() => h.Owner.RecordEvolutionReadyFeedback(Upgrade("ready")));
            CollectionAssert.AreEqual(new[] { "metric", "name:ready" }, h.Events);
            Assert.AreEqual(0, h.Owner.EvolutionReadyFeedbackCount);
            Assert.IsEmpty(h.Evolution.Label);
        }

        [Test]
        public void PurchaseUsesLoadedAuthoredNameThenCurrentRankAndUnknownIdFallback()
        {
            var h = new Host(); var upgrade = h.Definition.PersistentUpgrades[0];
            h.Owner.RecordMetaUpgradePurchaseFeedback(upgrade.Id.Value);
            CollectionAssert.AreEqual(new[] { "profile", "definition", "rank:" + upgrade.Id.Value }, h.Events);
            Assert.AreEqual("Meta Upgrade: " + upgrade.DisplayName + " rank 3", h.Reward.Label);
            Assert.AreEqual(2.35f, h.Reward.RemainingSeconds);
            h.Owner.RecordMetaUpgradePurchaseFeedback("unknown");
            Assert.AreEqual("Meta Upgrade: unknown rank 3", h.Owner.LastMetaUpgradePurchaseFeedbackLabel);
        }

        [Test]
        public void ClassSelectionNullPreservesHistoryAndResetLeavesBorrowedBanner()
        {
            var h = new Host(); var selected = BasicSurvivorsGame.CreateClassLibraryDefinition().Classes[0];
            h.Owner.RecordResultClassSelectionFeedback(selected);
            string label = "Next Run Class: " + selected.DisplayName;
            Assert.AreEqual(label, h.Reward.Label);
            h.Owner.RecordResultClassSelectionFeedback(null);
            Assert.AreEqual(label, h.Owner.LastResultClassSelectionFeedbackLabel);
            h.Owner.ResetHistory();
            Assert.IsEmpty(h.Owner.LastResultClassSelectionFeedbackLabel);
            Assert.AreEqual(label, h.Reward.Label);
        }

        [Test]
        public void UnlockReadsClassBeforeRewardAndPublishesBeforePulse()
        {
            var h = new Host(); h.Owner.RecordClassUnlockRewardFeedback();
            CollectionAssert.AreEqual(new[] { "class", "definition", "unlock-pulse" }, h.Events);
            StringAssert.StartsWith("Class Unlocked: Authored class", h.ClassUnlock.Label);
            Assert.AreEqual(h.ClassUnlock.Label, h.Owner.LastClassUnlockRewardFeedbackLabel);
            Assert.AreEqual(2.65f, h.ClassUnlock.RemainingSeconds);
            h.Owner.ResetHistory();
            Assert.IsEmpty(h.Owner.LastClassUnlockRewardFeedbackLabel);
            Assert.IsNotEmpty(h.ClassUnlock.Label);
        }

        private static RunUpgradeDefinition Upgrade(string id) => new RunUpgradeDefinition(new RunUpgradeId(id),
            RunUpgradeRarity.Common, 1, 1, new[] { new RunUpgradeEffectDescriptor(new RunUpgradeEffectId("effect"), new RunUpgradeTargetId("target"), 1d) });
        private sealed class Host : ISurvivorsProgressionFeedbackPort
        {
            public readonly List<string> Events = new List<string>();
            public readonly SurvivorsFeedbackBannerPresenter Reward = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.Reward);
            public readonly SurvivorsFeedbackBannerPresenter Evolution = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.Evolution);
            public readonly SurvivorsFeedbackBannerPresenter ClassUnlock = new SurvivorsFeedbackBannerPresenter(SurvivorsFeedbackBannerKind.ClassUnlock);
            public readonly SurvivorsMetaProgressionDefinition Definition = BasicSurvivorsGame.CreateMetaProgressionDefinition();
            public readonly SurvivorsProgressionFeedback Owner;
            public bool ThrowName;
            public Host() => Owner = new SurvivorsProgressionFeedback(this, Reward, ClassUnlock, Evolution);
            public void EnsureProfile() => Events.Add("profile");
            public SurvivorsMetaProgressionDefinition MetaDefinition { get { Events.Add("definition"); return Definition; } }
            public int GetPersistentRank(string id) { Events.Add("rank:" + id); return 3; }
            public string ResolveUpgradeDisplayName(RunUpgradeId id) { Events.Add("name:" + id.Value); if (ThrowName) throw new InvalidOperationException(); return id.Value; }
            public string ResolveClassDisplayName(string id, string fallback) { Events.Add("class"); return "Authored class"; }
            public string CurrencyRewardLabel => "shards";
            public string ProgressionRewardLabel => "XP";
            public void RecordEvolutionEligibility() => Events.Add("metric");
            public void PlayEvolutionPulse(int count) => Events.Add($"pulse:{count}:{Owner.EvolutionGoalFeedbackCount}:{Owner.EvolutionReadyFeedbackCount}");
            public void PlayClassUnlockPulse() { Assert.AreEqual(ClassUnlock.Label, Owner.LastClassUnlockRewardFeedbackLabel); Events.Add("unlock-pulse"); }
        }
    }
}
