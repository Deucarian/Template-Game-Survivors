using System;
using System.Linq;
using Deucarian.Progression;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsProgressionOwnerTests
    {
        [TestCase(SurvivorsEnemyRole.Elite, 2, 12, 1, 0, 0)]
        [TestCase(SurvivorsEnemyRole.DreadElite, 2, 12, 1, 0, 0)]
        [TestCase(SurvivorsEnemyRole.Miniboss, 4, 25, 0, 1, 0)]
        [TestCase(SurvivorsEnemyRole.Boss, 18, 120, 0, 0, 1)]
        [TestCase(SurvivorsEnemyRole.Swarm, 4, 25, 0, 1, 0)]
        public void MajorRewardsUseAuthoredAmountsAndRoleAccounting(SurvivorsEnemyRole role,
            int shards, int xp, int elite, int miniboss, int boss)
        {
            using (var host = new SurvivorsProgressionOwnerTestHost())
            {
                host.Rewards.GrantMajorEnemyReward(role);
                Assert.AreEqual(shards, host.Rewards.BonusBloodShards);
                Assert.AreEqual(xp, host.Rewards.BonusLegacyExperience);
                Assert.AreEqual(elite, host.Rewards.EliteRewardGrantCount);
                Assert.AreEqual(miniboss, host.Rewards.MinibossRewardGrantCount);
                Assert.AreEqual(boss, host.Rewards.BossRewardGrantCount);
                Assert.AreEqual(0, host.ProfileReads, "Encounter rewards accumulate before touching persistent state.");
            }
        }

        [Test]
        public void MissingAuthoredRewardDoesNotInventBonusOrDiagnostic()
        {
            using (var host = new SurvivorsProgressionOwnerTestHost())
            {
                var original = host.Definition;
                host.Definition = new SurvivorsMetaProgressionDefinition(original.BloodShardsCurrencyId,
                    original.LegacyExperienceTrackId, original.PersistentUpgrades, Array.Empty<SurvivorsRewardDefinition>());
                host.Rewards.GrantMajorEnemyReward(SurvivorsEnemyRole.Boss);
                Assert.AreEqual(0, host.Rewards.BonusBloodShards);
                Assert.AreEqual(0, host.Rewards.BonusLegacyExperience);
                Assert.AreEqual(0, host.Rewards.BossRewardGrantCount);
            }
        }

        [Test]
        public void VictoryGrantsScaledAccountBeforeUnlockAndSummaryAndSuppressesDuplicateTerminal()
        {
            using (var host = new SurvivorsProgressionOwnerTestHost())
            {
                host.Rewards.AddBloodShards(3);
                host.Rewards.GrantMajorEnemyReward(SurvivorsEnemyRole.Boss);
                host.Rewards.GrantRunRewards(true, new SurvivorsRunRewardInput(60f, 4, 2, 1, 2f));
                Assert.AreEqual(86, host.Rewards.BloodShardsEarned);
                Assert.AreEqual(576, host.Rewards.LegacyExperienceEarned);
                Assert.AreEqual(31, host.Rewards.BonusBloodShards);
                Assert.AreEqual(180, host.Rewards.BonusLegacyExperience);
                CollectionAssert.AreEqual(new[] { "unlock", "victory" }, host.Events);
                var result = host.Rewards.LastRunResult;
                var progression = host.EnsureProgression();
                Assert.AreEqual(86, progression.UnspentBloodShards);
                host.Rewards.GrantRunRewards(false, new SurvivorsRunRewardInput(999f, 40, 20, 10, 5f));
                Assert.AreSame(result, host.Rewards.LastRunResult);
                Assert.AreEqual(86, progression.UnspentBloodShards);
                Assert.AreEqual(2, host.Events.Count);
            }
        }

        [Test]
        public void RunResetPreservesProfileAndAnUnlockedClassHasNoSecondBonus()
        {
            using (var host = new SurvivorsProgressionOwnerTestHost())
            {
                host.Rewards.GrantRunRewards(true, new SurvivorsRunRewardInput(0f, 1, 0, 0, 1f));
                var progression = host.EnsureProgression();
                Assert.AreEqual(10, progression.UnspentBloodShards);
                host.Rewards.Reset();
                Assert.IsFalse(host.Rewards.Granted);
                Assert.IsNull(host.Rewards.LastRunResult);
                Assert.AreEqual(0, host.Rewards.BonusBloodShards);
                Assert.AreEqual(0, host.Rewards.ClassUnlockRewardCount);
                host.Events.Clear();
                host.Rewards.GrantRunRewards(true, new SurvivorsRunRewardInput(0f, 1, 0, 0, 1f));
                Assert.AreEqual(0, host.Rewards.BloodShardsEarned);
                Assert.AreEqual(75, host.Rewards.LegacyExperienceEarned);
                Assert.AreEqual(10, progression.UnspentBloodShards);
                CollectionAssert.AreEqual(new[] { "victory" }, host.Events);
            }
        }

        [Test]
        public void DefeatDoesNotLoadClassContentAndKeepsPickupBonuses()
        {
            using (var host = new SurvivorsProgressionOwnerTestHost())
            {
                host.Rewards.AddBloodShards(7);
                host.Rewards.GrantRunRewards(false, new SurvivorsRunRewardInput(40f, 2, 1, 0, 1f));
                Assert.AreEqual(13, host.Rewards.BloodShardsEarned);
                Assert.AreEqual(5, host.Rewards.LegacyExperienceEarned);
                Assert.AreEqual(0, host.ClassReads);
                CollectionAssert.AreEqual(new[] { "defeat" }, host.Events);
            }
        }

        [TestCase(-1f, 0, 0)]
        [TestCase(0f, 0, 0)]
        [TestCase(0.01f, 1, 1)]
        [TestCase(0.5f, 1, 2)]
        [TestCase(1f, 1, 3)]
        [TestCase(2f, 2, 6)]
        public void RewardScalingRetainsMinimumPositiveAndUnityRounding(float multiplier, int shards, int xp)
        {
            var summary = new SurvivorsRunRewardSummary { BloodShardsEarned = 1, LegacyExperienceEarned = 3 };
            SurvivorsRunRewards.ApplyRunRewardMultiplier(summary, multiplier);
            Assert.AreEqual(shards, summary.BloodShardsEarned);
            Assert.AreEqual(xp, summary.LegacyExperienceEarned);
            Assert.DoesNotThrow(() => SurvivorsRunRewards.ApplyRunRewardMultiplier(null, multiplier));
        }

        [Test]
        public void AffordableOptionsKeepAuthoredOrderAndPurchaseUsesLiveBalance()
        {
            using (var host = new SurvivorsProgressionOwnerTestHost())
            {
                Assert.IsEmpty(host.Persistent.ResolveResultMetaUpgradeOptions(0));
                Assert.AreEqual(0, host.ProfileReads);
                var progression = host.EnsureProgression();
                progression.GrantBloodShardsForDebug(5);
                var options = host.Persistent.ResolveResultMetaUpgradeOptions(3);
                CollectionAssert.AreEqual(new[] { BasicSurvivorsGame.ArcaneLegacyMetaUpgradeId.Value,
                    BasicSurvivorsGame.VitalWardMetaUpgradeId.Value, BasicSurvivorsGame.GemheartLegacyMetaUpgradeId.Value },
                    options.Select(x => x.Id.Value).ToArray());
                Assert.IsFalse(host.Persistent.TryPurchaseResultMetaUpgrade(-1, 3, true));
                Assert.IsTrue(host.Persistent.TryPurchaseResultMetaUpgrade(0, 3, true));
                Assert.AreEqual(0, progression.UnspentBloodShards);
                Assert.AreEqual(1, host.Persistent.MetaUpgradePurchaseCount);
                CollectionAssert.AreEqual(new[] { "apply", "purchase:" + BasicSurvivorsGame.ArcaneLegacyMetaUpgradeId.Value }, host.Events);
                Assert.IsFalse(host.Persistent.TryPurchaseResultMetaUpgrade(0, 3, true));
                Assert.AreEqual(1, host.BonusApplications);
                host.Persistent.ResetRunDiagnostics();
                Assert.AreEqual(0, host.Persistent.MetaUpgradePurchaseCount);
                Assert.AreEqual(1, progression.GetPersistentUpgradeRank(BasicSurvivorsGame.ArcaneLegacyMetaUpgradeId.Value));
            }
        }

        [Test]
        public void PurchasesOutsideRunDoNotApplyLiveModifiersAndMaximumRankLeavesOffers()
        {
            using (var host = new SurvivorsProgressionOwnerTestHost())
            {
                host.EnsureProgression().GrantBloodShardsForDebug(100);
                string id = BasicSurvivorsGame.ArcaneLegacyMetaUpgradeId.Value;
                for (int i = 0; i < 3; i++) Assert.IsTrue(host.Persistent.TryPurchasePersistentUpgrade(id, false));
                Assert.IsFalse(host.Persistent.TryPurchasePersistentUpgrade(id, false));
                Assert.AreEqual(3, host.Persistent.MetaUpgradePurchaseCount);
                Assert.AreEqual(0, host.BonusApplications);
                Assert.IsFalse(host.Persistent.ResolveResultMetaUpgradeOptions(100).Any(x => x.Id.Value == id));
            }
        }

        [TestCase(SurvivorsRunState.Playing)]
        [TestCase(SurvivorsRunState.LevelUp)]
        [TestCase(SurvivorsRunState.Booting)]
        public void StartedRunBlocksResultClassSelectionWithoutLoadingProfile(SurvivorsRunState state)
        {
            using (var host = new SurvivorsProgressionOwnerTestHost())
            {
                Assert.IsFalse(host.Persistent.TrySelectResultClass(0, 3, true, state));
                Assert.AreEqual(0, host.ProfileReads);
                Assert.IsEmpty(host.Events);
            }
        }

        [TestCase(SurvivorsRunState.GameOver)]
        [TestCase(SurvivorsRunState.Victory)]
        public void ResultSelectionKeepsLockedRowsAndPublishesSelectedClassBeforeFeedback(SurvivorsRunState state)
        {
            using (var host = new SurvivorsProgressionOwnerTestHost())
            {
                var choices = host.Persistent.ResolveResultClassOptions(100);
                int locked = choices.ToList().FindIndex(x => x.Id == BasicSurvivorsGame.EmberVanguardClassId);
                Assert.GreaterOrEqual(locked, 0);
                Assert.IsFalse(host.Persistent.TrySelectResultClass(locked, 100, true, state));
                host.EnsureProgression().UnlockClass(BasicSurvivorsGame.EmberVanguardClassId, host.Classes);
                Assert.IsTrue(host.Persistent.TrySelectResultClass(locked, 100, true, state));
                CollectionAssert.AreEqual(new[] { "selected", "class:" + BasicSurvivorsGame.EmberVanguardClassId }, host.Events);
                Assert.IsTrue(host.Persistent.TrySelectResultClass(locked, 100, true, state), "Existing same-class success semantics remain.");
                Assert.AreEqual(2, host.Persistent.ResultClassSelectionCount);
                Assert.IsFalse(host.Persistent.IsResultClassUnlocked(null));
            }
        }

        [Test]
        public void PersistentCostUsesLastAuthoredCostAndRejectsUnavailableRanks()
        {
            var upgrade = new SurvivorsPersistentUpgradeDefinition(new ResearchNodeId("test.cost"), "Cost", "", "", 4, new[] { 2, 7 }, 1f);
            Assert.AreEqual(2, SurvivorsPersistentProgression.ResolveNextPersistentUpgradeCost(upgrade, 0));
            Assert.AreEqual(7, SurvivorsPersistentProgression.ResolveNextPersistentUpgradeCost(upgrade, 3));
            Assert.AreEqual(0, SurvivorsPersistentProgression.ResolveNextPersistentUpgradeCost(upgrade, -1));
            Assert.AreEqual(0, SurvivorsPersistentProgression.ResolveNextPersistentUpgradeCost(upgrade, 4));
            Assert.AreEqual(0, SurvivorsPersistentProgression.ResolveNextPersistentUpgradeCost(null, 0));
        }
    }
}
