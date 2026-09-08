using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    [SetCulture("en-US")]
    public sealed class SurvivorsRunSummaryModelTests
    {
        [Test]
        public void ResultRowsPreserveCopyOrderAuthoredLabelsLongBalancesAndStableHistoryIdentity()
        {
            var build = Build();
            Select(build, "passive");
            Select(build, "evolution");
            var model = new SurvivorsRunSummaryModel(build, new SurvivorsBuildContentLabels(build));
            IReadOnlyList<string> retained = model.Lines;
            var weapons = new List<string> { BasicSurvivorsGame.ArcaneWandWeaponContentId, BasicSurvivorsGame.FrostFanWeaponContentId };
            var values = new SurvivorsRunSummaryValues(
                new SurvivorsRunSummaryOutcome(true, true, "Neon Ledger", "Sprint", 65.9f, 120f, 180f, 1.25f, 7, 80, 4, 12),
                new SurvivorsRunSummaryRewards(9, 11, "Shards", "Legacy", 5000000000L, 6000000000L, "Shard Bank", "Legacy Bank"),
                new SurvivorsRunSummaryCombat(42, 3, 2, 1, 3.2f, 9.5f, 20f, "Chosen moment", 30.9f, "Legendary", 8),
                new SurvivorsRunSummaryCollection(weapons, 2, 1, 4, "Prism", 2.5f, 4.2f, 12f, 3, 1, "Authored Vanguard"));
            model.Rebuild(values);
            Assert.AreSame(retained, model.Lines);
            Assert.AreEqual("Neon Ledger - Victory", model.Title);
            CollectionAssert.AreEqual(new[]
            {
                "Run mode: Sprint",
                "Result: Victory - Endless continuation available",
                "Run time: 01:05 / target 02:00",
                "Level reached: 7   XP collected: 80   Stored XP: 4/12",
                "Kills: 42   Elites: 3   Minibosses: 2   Bosses: 1",
                "Rewards earned: +9 Shards, +11 Legacy",
                "Reward profile: target 03:00, multiplier x1.25",
                "Meta bank: 5000000000 Shard Bank, 6000000000 Legacy Bank",
                "Damage taken: 3.2   Final health: 9.5/20",
                "Weapons 2/6: Wand, Frost",
                "Passives 1/6: Lens 1/3",
                "Evolutions 1: Radiance",
                "Relics 1/4: Prism",
                "Pickup build: radius 2.5, magnet range 2.5, pull speed 4.2, pulse 00:12, recalls 3",
                "Top weapon by damage: not tracked yet",
                "Top weapon by kills: not tracked yet",
                "Best moment: Chosen moment",
                "Class unlocked: Authored Vanguard"
            }, retained);
            weapons.Clear();
            Assert.AreEqual("Weapons 2/6: Wand, Frost", retained[9]);
            Assert.AreEqual(1, build.State.GetRank(new RunUpgradeId("passive")));
            model.Clear();
            Assert.AreSame(retained, model.Lines);
            Assert.AreEqual(0, retained.Count);
            Assert.AreEqual(string.Empty, model.Title);
            model.Rebuild(values);
            Assert.AreEqual("Weapons 2/6: None", retained[9], "Each rebuild reads the current supplied loadout without retaining it.");
        }

        [TestCase("Recorded", 12f, "Epic", 9, "Recorded")]
        [TestCase(" ", 12.9f, "Epic", 9, "First evolution at 00:12")]
        [TestCase(null, -1f, "Epic", 9, "Highest rarity chosen: Epic")]
        [TestCase(null, -1f, " ", 9, "Best streak 9")]
        [TestCase(null, -1f, null, 0, "First run data captured")]
        public void DefeatSummaryPreservesBestMomentPrecedenceAndOmitsClassRow(string recorded, float evolutionTime, string rarity, int streak, string expected)
        {
            var build = Build();
            var model = new SurvivorsRunSummaryModel(build, new SurvivorsBuildContentLabels(build));
            model.Rebuild(new SurvivorsRunSummaryValues(
                new SurvivorsRunSummaryOutcome(themeTitle: "Ledger", modeDisplayName: "Standard"),
                default,
                new SurvivorsRunSummaryCombat(bestMomentLabel: recorded, firstEvolutionAcquiredTimeSeconds: evolutionTime,
                    highestChosenRarityLabel: rarity, bestKillStreak: streak),
                default));
            Assert.AreEqual("Ledger - Defeat", model.Title);
            Assert.AreEqual("Result: Defeat", model.Lines[1]);
            Assert.AreEqual("Best moment: " + expected, model.Lines[16]);
            Assert.AreEqual(17, model.Lines.Count);
        }

        [Test]
        public void UpgradeSummaryUsesOrdinalCatalogOrderAndRankFloorWithoutMutatingBuild()
        {
            var build = Build();
            var model = new SurvivorsRunSummaryModel(build, new SurvivorsBuildContentLabels(build));
            Assert.AreEqual("Radiance 1/3, Lens 1/3", model.FormatRunSummaryUpgradeList(new[] { "passive", "evolution", "passive" }, true));
            Assert.AreEqual("Radiance, Lens", model.FormatRunSummaryUpgradeList(new[] { "passive", "evolution" }, false));
            Assert.AreEqual("None yet", model.FormatRunSummaryUpgradeList(new[] { "PASSIVE", "missing" }, true));
            Assert.AreEqual("None yet", model.FormatRunSummaryUpgradeList(null, true));
            Assert.AreEqual(0, build.State.GetRank(new RunUpgradeId("passive")));
            Assert.AreEqual(0, build.ActivePassiveCount);
        }

        private static SurvivorsRunBuildState Build()
        {
            var build = new SurvivorsRunBuildState(new BuildPort());
            build.Initialize(new RunUpgradeCatalog(new[] { Upgrade("passive"), Upgrade("evolution") }), new[]
            {
                new SurvivorsRunUpgradeMetadata("passive", "Lens", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, "player", "Lens"),
                new SurvivorsRunUpgradeMetadata("evolution", "Radiance", SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None, "weapon", "Radiance")
            }, null, null);
            return build;
        }
        private static RunUpgradeDefinition Upgrade(string id) => new RunUpgradeDefinition(new RunUpgradeId(id), RunUpgradeRarity.Common, 1, 3,
            new[] { new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 1) });
        private static void Select(SurvivorsRunBuildState build, string id)
        {
            Assert.IsTrue(build.Catalog.TryGet(new RunUpgradeId(id), out RunUpgradeDefinition definition));
            Assert.IsTrue(build.State.Select(build.Catalog, definition.Id).Succeeded);
            build.RecordRunBuildSelection(definition);
        }
        private sealed class BuildPort : ISurvivorsRunBuildPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning();
            public int WeaponCount => 0;
            public bool HasWeapon(string id) => false;
            public void AddWeapon(string id) { }
            public void PassiveAdded(RunUpgradeDefinition upgrade) { }
            public void RecordEvolutionTime() { }
            public void EvolutionAdded(RunUpgradeDefinition upgrade) { }
        }
    }
}
