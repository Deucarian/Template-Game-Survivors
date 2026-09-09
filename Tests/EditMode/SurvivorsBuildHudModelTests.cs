using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    [SetCulture("en-US")]
    public sealed class SurvivorsBuildHudModelTests
    {
        [Test]
        public void EmptyBuildKeepsSectionOrderAndQueriesEvolutionFallbackOnlyForEmptyRows()
        {
            var build = new SurvivorsRunBuildState(new BuildPort());
            int objectiveReads = 0;
            var model = new SurvivorsBuildHudModel(build, new SurvivorsBuildContentLabels(build), () =>
            {
                objectiveReads++;
                return "Goal Lens -> Radiance";
            });
            CollectionAssert.AreEqual(new[]
            {
                "Weapons 0/6", "  none", "Passives 0/6", "  none", "Evolutions 0",
                "  Goal Lens -> Radiance", "Pickup range 2.5   Pull 4.2   Pulse not yet", "Relics none"
            }, model.BuildLines(new SurvivorsBuildHudValues(Array.Empty<string>(), 0, 2.5f, 4.2f, "not yet", "none")));
            Assert.AreEqual(1, objectiveReads);
            Assert.IsNull(build.State, "Projecting a missing build must not initialize rank state.");
        }

        [Test]
        public void WeaponRowsPreserveLoadoutOrderThreeCatalogFragmentsDuplicatesAndHiddenCount()
        {
            var definitions = new[] { Upgrade("w.beta"), Upgrade("e"), Upgrade("b"), Upgrade("a"), Upgrade("w.alpha"), Upgrade("d"), Upgrade("c") };
            var metadata = new[]
            {
                Metadata("w.beta", "Beta Starter", SurvivorsRunUpgradeCategory.Weapon, "beta"),
                Metadata("w.alpha", "Alpha Starter", SurvivorsRunUpgradeCategory.Weapon, "alpha"),
                Metadata("a", "Shared", SurvivorsRunUpgradeCategory.WeaponUpgrade, "beta"),
                Metadata("b", "Shared", SurvivorsRunUpgradeCategory.WeaponUpgrade, "beta"),
                Metadata("c", "Spiral", SurvivorsRunUpgradeCategory.Mutation, "beta"),
                Metadata("d", "Fourth", SurvivorsRunUpgradeCategory.WeaponUpgrade, "beta"),
                Metadata("e", "Fifth", SurvivorsRunUpgradeCategory.WeaponUpgrade, "beta")
            };
            var build = Build(definitions, metadata);
            foreach (string id in new[] { "e", "d", "c", "b", "a" })
                Assert.IsTrue(build.State.Select(build.Catalog, new RunUpgradeId(id)).Succeeded);
            var model = new SurvivorsBuildHudModel(build, new SurvivorsBuildContentLabels(build), () => string.Empty);
            var weapons = new List<string> { "beta", "alpha" };
            var values = new SurvivorsBuildHudValues(weapons, 2, 3f, 5f, "00:12", "Prism, Bell +2");
            IReadOnlyList<string> first = model.BuildLines(values);
            Assert.AreEqual("Weapons 2/6", first[0]);
            Assert.AreEqual("  Beta Starter: Shared 1/3, Shared 1/3, Spiral 1/3, +2", first[1]);
            Assert.AreEqual("  Alpha Starter", first[2]);
            Assert.AreEqual("Relics Prism, Bell +2", first[first.Count - 1]);
            CollectionAssert.AreEqual(new[] { "beta", "alpha" }, weapons);
            Assert.AreEqual(1, build.State.GetRank(new RunUpgradeId("a")));
            Assert.AreEqual(0, build.State.GetRank(new RunUpgradeId("w.beta")), "Authored weapon labels are available before acquiring their upgrade.");

            Assert.IsTrue(build.State.Select(build.Catalog, new RunUpgradeId("a")).Succeeded);
            Assert.That(model.BuildLines(values)[1], Does.Contain("Shared 2/3"));
            Assert.That(first[1], Does.Contain("Shared 1/3"), "Previously produced rows are values, not mutable rank views.");
            weapons.Reverse();
            Assert.AreEqual("  Alpha Starter", model.BuildLines(values)[1]);
        }

        [Test]
        public void EverySelectedPassiveAndEvolutionIsListedWithoutSummaryTruncation()
        {
            var definitions = new List<RunUpgradeDefinition>();
            var metadata = new List<SurvivorsRunUpgradeMetadata>();
            for (int i = 0; i < 6; i++)
            {
                definitions.Add(Upgrade("p" + i));
                metadata.Add(Metadata("p" + i, "Passive " + i, SurvivorsRunUpgradeCategory.Passive,
                    BasicSurvivorsGame.PlayerTarget.Value, SurvivorsRunBuildSlotKind.Passive));
            }
            definitions.Add(Upgrade("evolution"));
            metadata.Add(Metadata("evolution", "Radiance", SurvivorsRunUpgradeCategory.Evolution, "weapon"));
            var build = Build(definitions, metadata);
            foreach (RunUpgradeDefinition definition in definitions)
            {
                Assert.IsTrue(build.State.Select(build.Catalog, definition.Id).Succeeded);
                build.RecordRunBuildSelection(definition);
            }
            var model = new SurvivorsBuildHudModel(build, new SurvivorsBuildContentLabels(build),
                () => throw new InvalidOperationException("Owned evolution rows must suppress the goal/ready query."));
            IReadOnlyList<string> lines = model.BuildLines(new SurvivorsBuildHudValues(Array.Empty<string>(), 0, 1f, 2f, "00:00", "none"));
            Assert.AreEqual("Passives 6/6", lines[2]);
            for (int i = 0; i < 6; i++)
                Assert.AreEqual("  Passive " + i + " 1/3 - Player", lines[i + 3]);
            Assert.AreEqual("Evolutions 1", lines[9]);
            Assert.AreEqual("  Radiance 1/3", lines[10]);
            Assert.AreEqual(13, lines.Count);
            Assert.AreEqual(6, build.ActivePassiveCount);
            Assert.AreEqual(1, build.EvolutionIds.Count);
        }

        [Test]
        public void AuthoredWeaponNamesUseFirstCatalogMatchAndDefaultNamesKeepCompactFallbacks()
        {
            string id = BasicSurvivorsGame.ArcaneWandUnlockUpgradeId;
            var build = Build(new[] { Upgrade(id) }, new[]
            {
                Metadata(id, "Arcane Wand", SurvivorsRunUpgradeCategory.Weapon, BasicSurvivorsGame.ArcaneWandWeaponContentId)
            });
            var labels = new SurvivorsBuildContentLabels(build);
            Assert.AreEqual("Wand", labels.ShortWeaponName(BasicSurvivorsGame.ArcaneWandWeaponContentId));
            Assert.AreEqual("Arcane Wand", labels.ResolveWeaponBuildDisplayName(BasicSurvivorsGame.ArcaneWandWeaponContentId));
            Assert.AreEqual("unknown", labels.ShortWeaponName(null));
            Assert.AreEqual("unrecognized.content", labels.ShortWeaponName("unrecognized.content"));
            Assert.AreEqual("Missing", labels.FormatBuildRankFragment(null, 1));

            build.Initialize(new RunUpgradeCatalog(new[] { Upgrade(id), Upgrade("aaa.first") }), new[]
            {
                Metadata(id, "Later catalog name", SurvivorsRunUpgradeCategory.Weapon, BasicSurvivorsGame.ArcaneWandWeaponContentId),
                Metadata("aaa.first", "Neon Needle", SurvivorsRunUpgradeCategory.Weapon, BasicSurvivorsGame.ArcaneWandWeaponContentId)
            }, null, null);
            Assert.AreEqual("Neon Needle", labels.ShortWeaponName(BasicSurvivorsGame.ArcaneWandWeaponContentId));
            Assert.IsTrue(labels.TryResolveWeaponUpgradeDisplayName(BasicSurvivorsGame.ArcaneWandWeaponContentId, out string name, out RunUpgradeId resolved));
            Assert.AreEqual("Neon Needle", name);
            Assert.AreEqual("aaa.first", resolved.Value);
            Assert.AreEqual(0, build.State.GetRank(resolved));
        }

        [Test]
        public void NarrowHudSkipsTextProjectionAndFirstVisibleWidthKeepsOriginalAnchor()
        {
            int reads = 0;
            var presenter = new SurvivorsBuildHudPresenter(() =>
            {
                reads++;
                return new[] { "one" };
            }, new SurvivorsHudStyles());
            Assert.IsFalse(presenter.TryPrepare(691, out _, out IReadOnlyList<string> hiddenLines));
            Assert.IsNull(hiddenLines);
            Assert.AreEqual(0, reads);
            Assert.IsTrue(presenter.TryPrepare(692, out SurvivorsBuildHudPanelLayout layout, out IReadOnlyList<string> lines));
            Assert.AreEqual(1, reads);
            Assert.AreEqual(new Rect(380f, 12f, 300f, 61f), layout.Panel);
            CollectionAssert.AreEqual(new[] { "one" }, lines);
        }

        [TestCase(0, 0, 0, 42f)]
        [TestCase(18, 18, 0, 384f)]
        [TestCase(19, 18, 1, 403f)]
        [TestCase(25, 18, 7, 403f)]
        public void WideHudCapsWidthAndRowsAndReservesOneOverflowRow(int total, int visible, int hidden, float height)
        {
            var rows = new string[total];
            for (int i = 0; i < total; i++) rows[i] = "row " + i;
            var presenter = new SurvivorsBuildHudPresenter(() => rows, new SurvivorsHudStyles());
            Assert.IsTrue(presenter.TryPrepare(3840, out SurvivorsBuildHudPanelLayout layout, out IReadOnlyList<string> prepared));
            Assert.AreEqual(new Rect(3446f, 12f, 382f, height), layout.Panel);
            Assert.AreEqual(visible, layout.VisibleLineCount);
            Assert.AreEqual(hidden, layout.HiddenLineCount);
            Assert.AreSame(rows, prepared);
        }

        private static SurvivorsRunBuildState Build(IReadOnlyList<RunUpgradeDefinition> definitions, IReadOnlyList<SurvivorsRunUpgradeMetadata> metadata)
        {
            var build = new SurvivorsRunBuildState(new BuildPort());
            build.Initialize(new RunUpgradeCatalog(definitions), metadata, null, null);
            return build;
        }

        private static RunUpgradeDefinition Upgrade(string id) => new RunUpgradeDefinition(new RunUpgradeId(id), RunUpgradeRarity.Common, 100, 3,
            new[] { new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 1) });

        private static SurvivorsRunUpgradeMetadata Metadata(string id, string name, SurvivorsRunUpgradeCategory category, string target,
            SurvivorsRunBuildSlotKind slot = SurvivorsRunBuildSlotKind.None) => new SurvivorsRunUpgradeMetadata(id, name, category, slot, target, name);

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
