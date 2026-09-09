using System;
using Deucarian.RunUpgrades;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    [SetCulture("en-US")]
    public sealed class SurvivorsRunStatusModelTests
    {
        [Test]
        public void EvolutionGoalUsesLivePrerequisitesAndPrecedesAnAlreadyReadyEvolution()
        {
            var port = new BuildPort();
            var build = new SurvivorsRunBuildState(port);
            build.Initialize(new RunUpgradeCatalog(new[] { Upgrade("0.ready", 1), Upgrade("a.evo", 1), Upgrade("rank"), Upgrade("passive") }), new[]
            {
                new SurvivorsRunUpgradeMetadata("0.ready", "Ready First", SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None, "weapon", "ready"),
                new SurvivorsRunUpgradeMetadata("a.evo", "Solar Spear", SurvivorsRunUpgradeCategory.Evolution, SurvivorsRunBuildSlotKind.None,
                    "weapon", "evolve", requiredUpgradeId: "rank", requiredUpgradeRank: 2, requiredPassiveUpgradeId: "passive"),
                new SurvivorsRunUpgradeMetadata("passive", "Authored Lens", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, "weapon", "lens")
            }, null, null);
            var model = EvolutionModel(build, port);
            Assert.IsTrue(build.State.Select(build.Catalog, new RunUpgradeId("rank")).Succeeded);
            Assert.AreEqual(string.Empty, model.Goal());
            Assert.AreEqual("Ready Ready First -> elite/boss reward", model.Ready());
            port.Tuning.EvolutionRequiredRankReduction = 1;
            Assert.AreEqual("Goal Authored Lens -> Solar Spear", model.Goal());
            Assert.AreEqual(model.Goal(), model.Objective(), "Goal takes priority even when an earlier catalog entry is ready.");
            Assert.AreEqual(0, build.State.GetRank(new RunUpgradeId("passive")));
            Assert.AreEqual(0, build.EvolutionIds.Count);
            Assert.IsTrue(build.State.Select(build.Catalog, new RunUpgradeId("passive")).Succeeded);
            Assert.AreEqual(string.Empty, model.Goal());
            Assert.AreEqual("Ready Ready First -> elite/boss reward", model.Objective());
            Assert.IsTrue(build.State.Select(build.Catalog, new RunUpgradeId("0.ready")).Succeeded);
            Assert.AreEqual("Ready Solar Spear -> elite/boss reward", model.Ready());
        }

        [Test]
        public void MissingCatalogHasNoEvolutionLabelsAndDoesNotInitializeBuild()
        {
            var port = new BuildPort();
            var build = new SurvivorsRunBuildState(port);
            var model = EvolutionModel(build, port);
            Assert.AreEqual(string.Empty, model.Goal());
            Assert.AreEqual(string.Empty, model.Ready());
            Assert.AreEqual(string.Empty, model.Objective());
            Assert.IsNull(build.State);
        }

        [Test]
        public void MilestonesKeepHordeFirstTieToleranceAndIgnoreExpiredOrDisabledCandidates()
        {
            var normal = new SurvivorsNormalMilestoneTimes(true, 20f, 19.9995f, 40f, 50f, 60f);
            var values = new SurvivorsRunMilestoneValues(true, SurvivorsRunState.Playing, 10f, false, 20f, normal, default);
            Assert.IsTrue(SurvivorsRunMilestoneModel.TryResolve(values, out string name, out float target, out float remaining));
            Assert.AreEqual("Horde Rush", name);
            Assert.AreEqual(20f, target);
            Assert.AreEqual(10f, remaining);
            Assert.AreEqual("Next Horde Rush in 00:10", SurvivorsRunMilestoneModel.Label(values));
            normal = new SurvivorsNormalMilestoneTimes(true, 10f, 0f, 19.9f, -1f, 60f);
            values = new SurvivorsRunMilestoneValues(true, SurvivorsRunState.Playing, 10f, false, 20f, normal, default);
            Assert.IsTrue(SurvivorsRunMilestoneModel.TryResolve(values, out name, out _, out _));
            Assert.AreEqual("Miniboss", name);
        }

        [TestCase(SurvivorsEnemyRole.Elite, "Endless Elite")]
        [TestCase(SurvivorsEnemyRole.DreadElite, "Endless Dread Elite")]
        public void EndlessMilestonesUseTheirOwnScheduleAndEliteRole(SurvivorsEnemyRole role, string expected)
        {
            var normal = new SurvivorsNormalMilestoneTimes(true, 1f, 2f, 3f, 4f, 5f);
            var endless = new SurvivorsEndlessMilestoneTimes(role, 20f, 30f, 40f);
            var values = new SurvivorsRunMilestoneValues(true, SurvivorsRunState.Playing, 0f, true, 50f, normal, endless);
            Assert.IsTrue(SurvivorsRunMilestoneModel.TryResolve(values, out string name, out float target, out _));
            Assert.AreEqual(expected, name);
            Assert.AreEqual(20f, target);
        }

        [TestCase(false, SurvivorsRunState.Victory, "Next Objective: survive", false)]
        [TestCase(true, SurvivorsRunState.Victory, "Victory Clear - continue or restart", true)]
        [TestCase(true, SurvivorsRunState.GameOver, "Run Ended - restart to try again", true)]
        [TestCase(true, SurvivorsRunState.Playing, "Next Objective: survive", false)]
        public void StoppedTerminalAndMissingScheduleStatesKeepOriginalMilestonePrecedence(bool started, SurvivorsRunState state, string label, bool hasMilestone)
        {
            var values = new SurvivorsRunMilestoneValues(started, state, 20f, false, 0f, default, default);
            Assert.AreEqual(hasMilestone, SurvivorsRunMilestoneModel.TryResolve(values, out _, out float target, out float remaining));
            Assert.AreEqual(0f, target);
            Assert.AreEqual(0f, remaining);
            Assert.AreEqual(label, SurvivorsRunMilestoneModel.Label(values));
        }

        [Test]
        public void SurgeLabelsPreserveAllTwelvePositionsSpacingAndExplicitActivityFlags()
        {
            var values = new SurvivorsSurgeHudValues(
                new SurvivorsSurgeHudState(true, 1f, 2), new SurvivorsSurgeHudState(true, 2f, 0),
                new SurvivorsSurgeHudState(true, 3f, 0), new SurvivorsSurgeHudState(true, 4f, 0),
                new SurvivorsSurgeHudState(true, 5f, 0), new SurvivorsSurgeHudState(true, 6f, 0),
                new SurvivorsSurgeHudState(true, 7f, 0), new SurvivorsSurgeHudState(true, 8f, 0),
                new SurvivorsSurgeHudState(true, 9f, 0), new SurvivorsSurgeHudState(true, 10f, 0),
                new SurvivorsSurgeHudState(true, 11f, 0), new SurvivorsSurgeHudState(true, 12f, 3));
            Assert.AreEqual("   Surge T2 1.0s   Way 2.0s   Shrine 3.0s   Focus 4.0s   Chain 5.0s   Breaker 6.0s   Arsenal 7.0s   Harmony 8.0s   Relic 9.0s   Gem 10.0s   Legend 11.0s   Endless T3 12.0s", SurvivorsSurgeHudModel.Format(values));
            values = new SurvivorsSurgeHudValues(new SurvivorsSurgeHudState(false, 20f, 5), default, default, default,
                default, default, default, default, default, default, default, new SurvivorsSurgeHudState(true, 0f, 3));
            Assert.AreEqual("   Endless T3 0.0s", SurvivorsSurgeHudModel.Format(values));
            Assert.AreEqual(string.Empty, SurvivorsSurgeHudModel.Format(default));
        }

        [Test]
        public void ClockMetricDashAndBuildLabelsPreserveDistinctThresholdsAndPrecision()
        {
            Assert.AreEqual("00:00", SurvivorsRunText.FormatRunTime(-1f));
            Assert.AreEqual("01:59", SurvivorsRunText.FormatRunTime(119.99f));
            Assert.AreEqual("100:01", SurvivorsRunText.FormatRunTime(6001f));
            Assert.AreEqual("not yet", SurvivorsRunText.FormatMetricTime(-0.1f));
            Assert.AreEqual("00:00", SurvivorsRunText.FormatMetricTime(0f));
            Assert.AreEqual("Off", SurvivorsRunText.FormatRewardTimeout(0f));
            Assert.AreEqual("0.5s", SurvivorsRunText.FormatRewardTimeout(0.5f));
            Assert.AreEqual("Arc Step Ready   Safe 0.0s", SurvivorsRunText.FormatDash(0.01f, true, 0f));
            Assert.AreEqual("Arc Step 0.0s", SurvivorsRunText.FormatDash(0.0101f, false, 3f));
            Assert.AreEqual("Endless", SurvivorsRunText.FormatPhase(true, (SurvivorsRunPhase)999));
            Assert.AreEqual("999", SurvivorsRunText.FormatPhase(false, (SurvivorsRunPhase)999));
            Assert.AreEqual("Build W 2/6   P 3/5   Evo 1   Relic 2/4", SurvivorsRunText.FormatBuildSlots(2, 6, 3, 5, 1, 2, 4));
        }

        private static SurvivorsEvolutionHudModel EvolutionModel(SurvivorsRunBuildState build, BuildPort port) =>
            new SurvivorsEvolutionHudModel(build, new SurvivorsDraftCatalogPolicy(build,
                new SurvivorsDraftRarityPolicy(() => port.Tuning, () => 0f), () => port.Tuning));
        private static RunUpgradeDefinition Upgrade(string id, int maxRank = 3) => new RunUpgradeDefinition(new RunUpgradeId(id), RunUpgradeRarity.Common, 1, maxRank,
            new[] { new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 1) });
        private sealed class BuildPort : ISurvivorsRunBuildPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning();
            public int WeaponCount => 0;
            public bool HasWeapon(string id) => true;
            public void AddWeapon(string id) { }
            public void PassiveAdded(RunUpgradeDefinition upgrade) { }
            public void RecordEvolutionTime() { }
            public void EvolutionAdded(RunUpgradeDefinition upgrade) { }
        }
    }
}
