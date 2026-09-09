using System;
using System.Collections.Generic;
using System.Linq;
using Deucarian.RunUpgrades;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsDraftOfferPolicyTests
    {
        [Test]
        public void RewardPoolUsesPreferredRaritiesOnlyWhenTheyFillTheAuthoredChoiceCount()
        {
            var f = new Fixture();
            f.Tuning.DraftChoiceCount = 1;
            CollectionAssert.AreEqual(new[] { "epic" }, Ids(f.Offers.Catalogs.CreateEligibleRewardDraftCatalog(SurvivorsEnemyRole.Boss, false)));
            f.Tuning.DraftChoiceCount = 3;
            CollectionAssert.AreEquivalent(new[] { "rank", "passive", "epic" }, Ids(f.Offers.Catalogs.CreateEligibleRewardDraftCatalog(SurvivorsEnemyRole.Boss, false)));
            Assert.IsNull(f.Offers.Catalogs.CreateEligibleRewardDraftCatalog(SurvivorsEnemyRole.Boss, true));
        }

        [Test]
        public void NormalGuaranteesPrioritizeRarityThenPrimerAndDeduplicateTheEarlyPassive()
        {
            var f = new Fixture();
            f.Select("rank");
            CollectionAssert.AreEqual(new[] { new RunUpgradeId("epic"), new RunUpgradeId("passive") },
                f.Offers.Guarantees.CreateNormalDraftChoiceLocks(DraftRarityProfile.NormalLate, 0));
            f.Tuning.DraftChoiceCount = 1;
            CollectionAssert.AreEqual(new[] { new RunUpgradeId("epic") },
                f.Offers.Guarantees.CreateNormalDraftChoiceLocks(DraftRarityProfile.NormalLate, 0));
            f.Tuning.DraftChoiceCount = 0;
            Assert.IsEmpty(f.Offers.Guarantees.CreateNormalDraftChoiceLocks(DraftRarityProfile.NormalLate, 0));
        }

        [Test]
        public void MissingPassiveQueryRequiresRankAndAnEligibleNewPassive()
        {
            var f = new Fixture();
            RunUpgradeDefinition evolution = f.Get("evolution");
            Assert.IsFalse(f.Offers.Catalogs.TryResolveEvolutionMissingPassive(evolution, out _));
            f.Select("rank");
            Assert.IsTrue(f.Offers.Catalogs.TryResolveEvolutionMissingPassive(evolution, out var passive));
            Assert.AreEqual("passive", passive.Id.Value);
            Assert.IsFalse(f.Build.IsUpgradeEligibleForCurrentBuild(evolution));
            f.Build.State.Banish(passive.Id);
            Assert.IsFalse(f.Offers.Catalogs.TryResolveEvolutionMissingPassive(evolution, out passive));
            Assert.IsNull(passive);
        }

        [Test]
        public void EvolutionLocksReplaceRarityFallbackAndRespectCatalogOrderAndLimit()
        {
            var f = new Fixture();
            CollectionAssert.AreEqual(new[] { new RunUpgradeId("epic") },
                f.Offers.Guarantees.CreateRewardDraftChoiceLocks(SurvivorsEnemyRole.Boss, 3, 0));
            f.Select("rank");
            f.Select("passive");
            f.Port.Weapons.Add("bolt");
            CollectionAssert.AreEqual(new[] { new RunUpgradeId("evolution"), new RunUpgradeId("evolution-two") },
                f.Offers.Guarantees.CreateRewardDraftChoiceLocks(SurvivorsEnemyRole.Boss, 3, 0));
            CollectionAssert.AreEqual(new[] { new RunUpgradeId("evolution") }, f.Offers.Guarantees.CreateEligibleEvolutionChoiceLocks(1));
            Assert.IsEmpty(f.Offers.Guarantees.CreateEligibleEvolutionChoiceLocks(0));
        }

        [Test]
        public void GenerationPreservesExplicitNormalLocksButDerivesRewardLocksFromTheBuild()
        {
            var f = new Fixture();
            f.Tuning.DraftChoiceCount = 1;
            var explicitLock = new[] { new RunUpgradeId("rank") };
            Assert.IsTrue(f.Offers.TryGenerate(SurvivorsRewardSelectionKind.LevelUp, -1, explicitLock, out var normal));
            Assert.AreEqual("rank", normal.Choices[0].Id.Value);
            Assert.IsTrue(f.Offers.TryGenerate(SurvivorsRewardSelectionKind.BossUpgrade, -1, explicitLock, out var boss));
            Assert.AreEqual("epic", boss.Choices[0].Id.Value);
            Assert.AreEqual(0, f.Build.State.GetRank(explicitLock[0]), "Generating an offer never spends a rank.");
            Assert.IsFalse(f.Offers.TryGenerate(SurvivorsRewardSelectionKind.BossRelic, 0, null, out var unsupported));
            Assert.IsNull(unsupported);
        }

        [Test]
        public void RepeatedGenerationIsDeterministicAndDisabledPoolsProduceNoDraft()
        {
            var f = new Fixture();
            Assert.IsTrue(f.Offers.TryGenerate(SurvivorsRewardSelectionKind.LevelUp, 2, null, out var first));
            Assert.IsTrue(f.Offers.TryGenerate(SurvivorsRewardSelectionKind.LevelUp, 2, null, out var second));
            CollectionAssert.AreEqual(first.Choices.Select(c => c.Id), second.Choices.Select(c => c.Id));
            f.Tuning.NormalEarlyCommonWeight = 0;
            f.Tuning.NormalEarlyEpicWeight = 0;
            Assert.IsFalse(f.Offers.TryGenerate(SurvivorsRewardSelectionKind.LevelUp, 0, null, out var empty));
            Assert.IsNull(empty);
        }

        [Test]
        public void ProgressSeedRetainsSeparateRewardSaltsAndAllRunProgressInputs()
        {
            var progress = new SurvivorsDraftProgress(100, 7, 9, 2, 3);
            Assert.AreEqual(151, progress.ResolveSeed(SurvivorsRewardSelectionKind.LevelUp));
            Assert.AreEqual(324, progress.ResolveSeed(SurvivorsRewardSelectionKind.EliteUpgrade));
            Assert.AreEqual(464, progress.ResolveSeed(SurvivorsRewardSelectionKind.BossUpgrade));
        }

        private static string[] Ids(RunUpgradeCatalog catalog) => catalog.Definitions.Select(d => d.Id.Value).ToArray();
        private sealed class Fixture
        {
            public readonly BuildPort Port = new BuildPort();
            public SurvivorsTemplateTuning Tuning => Port.Tuning;
            public readonly SurvivorsRunBuildState Build;
            public readonly SurvivorsDraftOfferGenerator Offers;
            public Fixture()
            {
                var definitions = new[] { Upgrade("rank"), Upgrade("passive"), Upgrade("epic", RunUpgradeRarity.Epic),
                    Upgrade("evolution"), Upgrade("evolution-two") };
                var metadata = new[] {
                    new SurvivorsRunUpgradeMetadata("passive", "Passive", SurvivorsRunUpgradeCategory.Passive,
                        SurvivorsRunBuildSlotKind.Passive, "passive", "Test passive"), Evolution("evolution"), Evolution("evolution-two") };
                Build = new SurvivorsRunBuildState(Port);
                Build.Initialize(new RunUpgradeCatalog(definitions), metadata, null, null);
                var rarity = new SurvivorsDraftRarityPolicy(() => Tuning, () => 0f);
                Offers = new SurvivorsDraftOfferGenerator(Build, rarity, () => Tuning, () => new SurvivorsDraftProgress(101, 1, 0, 0, 0));
            }
            public RunUpgradeDefinition Get(string id) { Build.TryGetRunUpgrade(id, out var definition); return definition; }
            public void Select(string id) => Assert.IsTrue(Build.State.Select(Build.Catalog, new RunUpgradeId(id)).Succeeded);
            private static SurvivorsRunUpgradeMetadata Evolution(string id) => new SurvivorsRunUpgradeMetadata(id, id,
                SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None, "bolt", "Test evolution", "bolt", "rank", 1, "passive");
            private static RunUpgradeDefinition Upgrade(string id, RunUpgradeRarity rarity = RunUpgradeRarity.Common) =>
                new RunUpgradeDefinition(new RunUpgradeId(id), rarity, 100, 3,
                    new[] { new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 1d) });
        }
        private sealed class BuildPort : ISurvivorsRunBuildPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning {
                DraftChoiceCount = 3, NormalEarlyCommonWeight = 100, NormalEarlyEpicWeight = 100,
                NormalLateCommonWeight = 100, NormalLateEpicWeight = 100, BossCommonWeight = 100, BossEpicWeight = 100 };
            public readonly HashSet<string> Weapons = new HashSet<string>();
            public int WeaponCount => Weapons.Count;
            public bool HasWeapon(string id) => Weapons.Contains(id);
            public void AddWeapon(string id) => Weapons.Add(id);
            public void PassiveAdded(RunUpgradeDefinition upgrade) { }
            public void RecordEvolutionTime() { }
            public void EvolutionAdded(RunUpgradeDefinition upgrade) { }
        }
    }
}
