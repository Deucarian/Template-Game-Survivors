using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    [SetCulture("en-US")]
    public sealed class SurvivorsRemainingHudTests
    {
        [TestCase(200, 270f)]
        [TestCase(332, 300f)]
        [TestCase(1600, 340f)]
        public void PlayerLayoutRetainsWidthClampsAndOnlyFirstFourRows(int width, float expected)
        {
            var layout = new SurvivorsPlayerHudLayout(width, 7);
            Assert.That(layout.Panel, Is.EqualTo(new Rect(12f, 58f, expected, 202f)));
            Assert.That(layout.VisibleLineCount, Is.EqualTo(4));
            Assert.That(layout.Row(0), Is.EqualTo(new Rect(26f, 168f, expected - 28f, 19f)));
            Assert.That(layout.Row(3).y, Is.EqualTo(225f));
            Assert.That(new SurvivorsPlayerHudLayout(width, 0).Panel.height, Is.EqualTo(126f));
            Assert.That(new SurvivorsPlayerHudLayout(width, 2).VisibleLineCount, Is.EqualTo(2));
        }

        [Test]
        public void BarObservationsRetainHealthBarrierThresholdsAndUnclampedExperienceRatio()
        {
            var vitals = new SurvivorsHudVitals(100f, 25f, 20f, 15f, 80, 40, 7);
            Assert.That(vitals.HealthRatio, Is.EqualTo(0.25f));
            Assert.That(vitals.BarrierRatio, Is.EqualTo(0.75f));
            Assert.That(vitals.ExperienceRatio, Is.EqualTo(2f));
            Assert.That(vitals.ShowPlayerBarrier, Is.True);
            vitals = new SurvivorsHudVitals(0f, 25f, 0.01f, 0.02f, 1, 0, 1);
            Assert.That(vitals.HealthRatio, Is.Zero);
            Assert.That(vitals.BarrierRatio, Is.EqualTo(2f));
            Assert.That(vitals.ShowPlayerBarrier, Is.False);
            Assert.That(float.IsPositiveInfinity(vitals.ExperienceRatio), Is.True, "The existing bar renderer owns clamping, not this read snapshot.");
            Assert.That(new SurvivorsHudVitals(-1f, 4f, -1f, 4f, 0, 1, 1).BarrierRatio, Is.Zero);
        }

        [TestCase(400, " ", 146f, 424f, 13)]
        [TestCase(1200, "Evolution goal", 264f, 448f, 14)]
        public void DebugLayoutPreservesOptionalEvolutionRowAndFirstFourLabelStyles(int height, string evolution,
            float y, float panelHeight, int rowCount)
        {
            var values = new SurvivorsDebugHudValues(evolutionObjective: evolution, dashLabel: "Dash");
            var rows = SurvivorsDebugHudModel.BuildRows(values);
            var layout = new SurvivorsDebugHudLayout(height, evolution);
            Assert.That(layout.Panel, Is.EqualTo(new Rect(12f, y, 356f, panelHeight)));
            Assert.That(rows.Count, Is.EqualTo(rowCount));
            Assert.That(rows[rows.Count - 1], Is.EqualTo("Dash"));
            Assert.That(layout.Row(rows.Count - 1).y, Is.EqualTo(y + (rowCount == 13 ? 400f : 422f)));
            for (int i = 0; i < rows.Count; i++) Assert.That(SurvivorsDebugHudLayout.UsesLabelStyle(i), Is.EqualTo(i < 4));
        }

        [Test]
        public void DebugRowsRetainEveryLabelOrderPrecisionAndLongCurrencyObservation()
        {
            var values = new SurvivorsDebugHudValues(
                vitals: new SurvivorsHudVitals(100f, 20f, 10f, 3f, 10, 20, 7),
                evolutionObjective: "Ready Solar Lens", waystoneCompass: "Waystone northeast", runTimeSeconds: 125.8f,
                survivalVictoryTimeSeconds: 250f, runPhaseLabel: "Endless", runEscalationLevel: 3, milestoneLabel: "Next Boss",
                activeEnemyCount: 4, enemyMaximumAlive: 90, killedCount: 81, splitterCount: 1, summonerCount: 2,
                eliteCount: 3, minibossCount: 4, bossCount: 5, currencyDisplayName: "Prisms", metaBloodShards: 3000000000L,
                poisonDamageRatio: 0.126f, bleedDamageRatio: 0.251f, executeThresholdNormalized: 0.22f,
                weaponLabel: "Authored Wand", modeDisplayName: "Neon", pacingProfile: SurvivorsPacingProfile.SprintRun,
                enemySpawnIntervalSeconds: 0.234f, enemySpeedMultiplier: 1.25f, currentKillStreak: 6, bestKillStreak: 12,
                streakBonusDropCount: 9, surgeLabel: "   Surge T2 1.0s", rewardSelectionTimeoutSeconds: 4.5f,
                draftRerollsRemaining: 2, draftBanishesRemaining: 1, buildSlotLabel: "Slots", dashLabel: "Arc Step Ready");
            Assert.That(SurvivorsDebugHudModel.BuildRows(values), Is.EqualTo(new[]
            {
                "LV 7   Time 02:05   Phase Endless +3", "Next Boss", "Enemies 4/90   Kills 81",
                "Split 1   Call 2   Elite 3   Mini 4   Boss 5", "Prisms 3000000000   Poison 0.13   Bleed 0.25   Execute 22%",
                "Weapons: Authored Wand", "Mode Neon   Profile Sprint Run", "Spawn 0.23s   Enemy Speed x1.25",
                "Streak 6   Best 12   Bonus Drops 9   Surge T2 1.0s", "Reward Timeout 4.5s   Reroll 2   Banish 1",
                "Slots", "Waystone northeast", "Ready Solar Lens", "Arc Step Ready"
            }));
            Assert.That(SurvivorsDebugHudModel.RunRatio(values), Is.EqualTo(125.8f / 250f));
            Assert.That(SurvivorsDebugHudModel.RunRatio(new SurvivorsDebugHudValues(runTimeSeconds: 4f)), Is.EqualTo(1f));
            Assert.That(SurvivorsDebugHudModel.RunRatio(new SurvivorsDebugHudValues(runTimeSeconds: -2f)), Is.Zero);
        }

        [TestCase(200, 240f)]
        [TestCase(332, 300f)]
        [TestCase(1600, 360f)]
        public void TopTimerKeepsCenteredWidthClampsAndFixedHeight(int width, float expected)
        {
            Assert.That(SurvivorsTopTimerHud.Panel(width), Is.EqualTo(new Rect(width * 0.5f - expected * 0.5f, 12f, expected, 36f)));
        }

        [TestCase(false, false, 300f, 61.9f, "Authored", "")]
        [TestCase(true, false, 300f, 61.9f, "Authored", "Authored  TIME 01:01  LEFT 03:58")]
        [TestCase(true, true, 300f, 61.9f, "Authored", "Authored  TIME 01:01  ENDLESS")]
        [TestCase(true, false, 0f, 61.9f, " ", "Sprint Run  TIME 01:01  ENDLESS")]
        [TestCase(true, false, 300f, 361f, "Authored", "Authored  TIME 06:01  LEFT 00:00")]
        public void TimerLabelsRetainStoppedEndlessFallbackAndExpiredTargetBranches(bool started, bool endless,
            float target, float elapsed, string mode, string expected)
        {
            var values = new SurvivorsTopTimerValues(started, endless, target, elapsed, mode, SurvivorsPacingProfile.SprintRun);
            Assert.That(SurvivorsTopTimerHud.Label(values), Is.EqualTo(expected));
        }

        [Test]
        public void WeaponLabelsKeepLiveAuthoredNamesInputOrderFourItemLimitAndDuplicates()
        {
            var build = new SurvivorsRunBuildState(new BuildPort());
            var labels = new SurvivorsCompactHudLabels(new SurvivorsBuildContentLabels(build));
            Assert.That(labels.Weapons(Array.Empty<string>()), Is.EqualTo("none"));
            var weapons = new[] { BasicSurvivorsGame.ArcaneWandWeaponContentId, "custom", "custom", "other", "hidden" };
            Assert.That(labels.Weapons(weapons), Is.EqualTo("Wand, custom, custom, other +1"));
            build.Initialize(new RunUpgradeCatalog(new[] { Upgrade("wand.upgrade") }), new[]
            {
                new SurvivorsRunUpgradeMetadata("wand.upgrade", "Authored Wand", SurvivorsRunUpgradeCategory.Weapon,
                    SurvivorsRunBuildSlotKind.Weapon, BasicSurvivorsGame.ArcaneWandWeaponContentId, "wand")
            }, null, null);
            Assert.That(labels.Weapons(weapons), Is.EqualTo("Authored Wand, custom, custom, other +1"));
            Assert.That(build.State.GetRank(new RunUpgradeId("wand.upgrade")), Is.Zero);
        }

        [Test]
        public void RelicLabelsKeepThreeSelectionsUnknownFallbackAndHiddenCount()
        {
            Assert.That(SurvivorsCompactHudLabels.Relics(Array.Empty<SurvivorsRelicDefinition>()), Is.EqualTo("none"));
            var selected = new List<SurvivorsRelicDefinition> { null, Relic("", ""), Relic("r", "Authored Relic"), Relic("r2", "Hidden") };
            Assert.That(SurvivorsCompactHudLabels.Relics(selected), Is.EqualTo("Unknown Relic, Unknown Relic, Authored Relic +1"));
            selected.RemoveAt(0);
            Assert.That(SurvivorsCompactHudLabels.Relics(selected), Is.EqualTo("Unknown Relic, Authored Relic, Hidden"));
        }

        [Test]
        public void WaystoneLabelsKeepDisabledUndiscoveredDiscoveredAndMissingPrecedence()
        {
            Assert.That(SurvivorsCompactHudLabels.Waystone(false, 3, true, 12.4f, Vector3.right, true), Is.EqualTo("Explore Waystones off   Found 3"));
            Assert.That(SurvivorsCompactHudLabels.Waystone(true, 3, true, 12.4f, new Vector3(1f, 9f, 1f), true), Is.EqualTo("Explore Waystone NE 12m   Found 3"));
            Assert.That(SurvivorsCompactHudLabels.Waystone(true, 3, false, 0f, Vector3.zero, true), Is.EqualTo("Explore Waystone new grid   Found 3"));
            Assert.That(SurvivorsCompactHudLabels.Waystone(true, 3, false, 0f, Vector3.zero, false), Is.EqualTo("Explore Waystone --   Found 3"));
        }

        private static SurvivorsRelicDefinition Relic(string id, string name)
            => new SurvivorsRelicDefinition(id, name, "player", "damage", SurvivorsRelicEffectKind.DamageBonus, 1f, 1);
        private static RunUpgradeDefinition Upgrade(string id) => new RunUpgradeDefinition(new RunUpgradeId(id), RunUpgradeRarity.Common,
            1, 3, new[] { new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 1) });
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
