using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    [SetCulture("en-US")]
    public sealed class SurvivorsDebugViewTests
    {
        [Test]
        public void MissingChoicesPreserveSeparateDebugAndMenuFallbacks()
        {
            var host = new Host();
            Assert.AreEqual("Missing upgrade", host.Formatter.FormatDebugRankLine(null, 1));
            Assert.AreEqual("Missing upgrade", host.Formatter.FormatDebugUpgradeLine(-1, null));
            Assert.AreEqual("3. Missing upgrade", host.Formatter.FormatDebugUpgradeLine(2, null));
            Assert.AreEqual("3. Missing Choice", host.Formatter.FormatUpgradeChoiceLabel(2, null));
            Assert.AreEqual("3. Missing Relic", host.Formatter.FormatRelicChoiceLabel(2, null));
            var lines = new List<string>();
            host.Formatter.AppendSelectedUpgradeRankLines(null);
            host.Formatter.AppendSelectedUpgradeRankLines(lines);
            Assert.IsEmpty(lines);
        }

        [Test]
        public void LiveRankChangesCategoryWithoutApplyingGameplayCommands()
        {
            var upgrade = Upgrade("passive");
            var host = new Host(new[] { upgrade }, new[] { Metadata(upgrade, "Authored passive", SurvivorsRunUpgradeCategory.Passive) });
            Assert.AreEqual("1. Authored passive [Passive/Rare] rank 0->1/3 - target: Authored copy", host.Formatter.FormatDebugUpgradeLine(0, upgrade));
            host.Build.State.Select(host.Build.Catalog, upgrade.Id);
            Assert.AreEqual("Authored passive [PassiveUpgrade] rank 1/3 (Rare) - target", host.Formatter.FormatDebugRankLine(upgrade, 1));
            StringAssert.Contains("Passive Upgrade", host.Formatter.FormatUpgradeChoiceLabel(0, upgrade));
            Assert.AreEqual(1, host.Build.State.GetRank(upgrade.Id));
            Assert.AreEqual(0, host.Build.ActivePassiveCount);
            Assert.AreEqual(0, host.Commands);
        }

        [Test]
        public void RankRowsUseCatalogOrderAndExactlyOneHeader()
        {
            var first = Upgrade("first"); var second = Upgrade("second");
            var host = new Host(new[] { first, second }, new[] { Metadata(first, "First"), Metadata(second, "Second") });
            host.Build.State.Select(host.Build.Catalog, second.Id);
            host.Build.State.Select(host.Build.Catalog, first.Id);
            var lines = new List<string> { "Existing" };
            host.Formatter.AppendSelectedUpgradeRankLines(lines);
            Assert.AreEqual(4, lines.Count);
            Assert.AreEqual("Ranks", lines[1]);
            StringAssert.StartsWith("  First ", lines[2]);
            StringAssert.StartsWith("  Second ", lines[3]);
        }

        [Test]
        public void MissingMetadataUsesBuildFallbackAndNextRankCapsAtMaximum()
        {
            var choice = Upgrade("custom");
            var host = new Host(new[] { choice });
            for (int i = 0; i < 3; i++) host.Build.State.Select(host.Build.Catalog, choice.Id);
            string debug = host.Formatter.FormatDebugUpgradeLine(-1, choice);
            StringAssert.Contains("rank 3->3/3 - Build:", debug);
            StringAssert.Contains("Rank 3->3/3", host.Formatter.FormatUpgradeChoiceLabel(0, choice));
            Assert.AreEqual(3, host.Build.State.GetRank(choice.Id));
        }

        [Test]
        public void EmptyAndMissingCatalogEvolutionQueriesHaveDistinctResults()
        {
            var missing = new Host();
            Assert.IsEmpty(missing.Drafts.DebugDescribeEligibleEvolutionPool());
            var bound = new Host(new[] { Upgrade("ordinary") });
            CollectionAssert.AreEqual(new[] { "No eligible evolutions yet. Max a weapon path and own its matching passive." }, bound.Drafts.DebugDescribeEligibleEvolutionPool());
            Assert.AreEqual(0, bound.Commands);
        }

        [Test]
        public void EligibleEvolutionsPreserveCatalogOrderAndCurrentPrerequisiteState()
        {
            var passive = Upgrade("passive"); var later = Upgrade("later"); var first = Upgrade("first");
            var host = new Host(new[] { later, passive, first }, new[] {
                Metadata(later, "Later", SurvivorsRunUpgradeCategory.Evolution, "passive"),
                Metadata(first, "First", SurvivorsRunUpgradeCategory.Evolution) });
            CollectionAssert.AreEqual(new[] { first, later, passive }, host.Build.Catalog.Definitions);
            var before = host.Drafts.DebugDescribeEligibleEvolutionPool();
            Assert.AreEqual(1, before.Count); StringAssert.StartsWith("First ", before[0]);
            host.Build.State.Select(host.Build.Catalog, passive.Id);
            var after = host.Drafts.DebugDescribeEligibleEvolutionPool();
            Assert.AreEqual(2, after.Count);
            StringAssert.StartsWith("First ", after[0]); StringAssert.StartsWith("Later ", after[1]);
            Assert.AreEqual(0, host.Build.State.GetRank(first.Id));
            Assert.AreEqual(0, host.Commands);
        }

        [Test]
        public void DraftAndRelicPoolsBothRenderInSourceOrderWithIndependentNumbering()
        {
            var choice = Upgrade("choice");
            var host = new Host(new[] { choice }, new[] { Metadata(choice, "Choice") });
            host.CurrentDraft = new RunUpgradeDraft(new[] { choice, null });
            host.CurrentRelicDraft = new SurvivorsRelicDraft(new[] { null, Relic() });
            var lines = host.Drafts.DebugDescribeCurrentDraftPool();
            Assert.AreEqual(6, lines.Count);
            Assert.AreEqual("Authored reward title", lines[0]);
            StringAssert.StartsWith("1. Choice ", lines[1]);
            Assert.AreEqual("2. Missing upgrade", lines[2]);
            Assert.AreEqual("Boss Relics", lines[3]);
            Assert.AreEqual("1. Missing relic", lines[4]);
            Assert.AreEqual("2. Relic [DamageBonus] +1.25 target", lines[5]);
            Assert.AreEqual(1, host.TitleReads);
            Assert.AreEqual(0, host.Commands);
        }

        [Test]
        public void EmptyDraftDoesNotQueryOverlayTitleAndRelicUsesBorrowedPreview()
        {
            var host = new Host();
            CollectionAssert.AreEqual(new[] { "No draft or reward pool is currently open." }, host.Drafts.DebugDescribeCurrentDraftPool());
            Assert.AreEqual(0, host.TitleReads);
            host.CurrentRelicDraft = new SurvivorsRelicDraft(new[] { Relic() });
            host.Drafts.DebugDescribeCurrentDraftPool();
            Assert.AreEqual(0, host.TitleReads);
            Assert.AreEqual("2. Boss Relic: Relic\n" + host.Cards.FormatRelicEffectSummary(Relic()), host.Formatter.FormatRelicChoiceLabel(1, Relic()));
        }

        [Test]
        public void BuildSnapshotKeepsRowsThenAppendsObjectiveBeforeLiveRankQuery()
        {
            var choice = Upgrade("choice");
            var host = new Host(new[] { choice }, new[] { Metadata(choice, "Choice") });
            var model = new SurvivorsDebugBuildModel(host.Formatter, () =>
            {
                host.Build.State.Select(host.Build.Catalog, choice.Id);
                return "Evolution objective";
            });
            var values = new SurvivorsDebugBuildValues(activeWeaponCount: 2, maxWeaponSlots: 6,
                activeWeaponList: "Wand, Frost", activePassiveCount: 1, maxPassiveSlots: 4,
                selectedRelicCount: 1, totalRelicCount: 3, selectedRelicList: "Relic",
                damageBonus: 2.5f, surgeDamageBonus: 1.5f, currentPickupMagnetPulseIntervalSeconds: -1f,
                projectileFanBonus: 2, projectilePierceBonus: 3, deathNovaDamage: 5f, deathNovaRadius: 2f);
            var lines = model.Describe(values);
            Assert.AreEqual(10, lines.Count);
            Assert.AreEqual("Weapons 2/6: Wand, Frost", lines[0]);
            Assert.AreEqual("Passives 1/4, Evolutions 0", lines[1]);
            Assert.AreEqual("Relics 1/3: Relic", lines[2]);
            StringAssert.Contains("damage +2.5 surge +1.5", lines[3]);
            StringAssert.Contains("pulse not yet", lines[3]);
            StringAssert.Contains("fan +2, pierce +3", lines[4]);
            StringAssert.Contains("death nova 5/2", lines[5]);
            Assert.AreEqual("Evolution objective", lines[7]);
            Assert.AreEqual("Ranks", lines[8]);
            StringAssert.Contains("rank 1/3", lines[9]);
        }

        [Test]
        public void BuildResultsAreSeparateListsAndBlankObjectiveAddsNoRow()
        {
            var host = new Host();
            int reads = 0;
            var model = new SurvivorsDebugBuildModel(host.Formatter, () => { reads++; return " "; });
            var first = model.Describe(default);
            var second = model.Describe(default);
            Assert.AreNotSame(first, second);
            Assert.AreEqual(7, first.Count);
            Assert.AreEqual(2, reads);
        }

        private static RunUpgradeDefinition Upgrade(string id) => new RunUpgradeDefinition(new RunUpgradeId(id), RunUpgradeRarity.Rare, 10, 3,
            new[] { new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 1d) });
        private static SurvivorsRunUpgradeMetadata Metadata(RunUpgradeDefinition choice, string name,
            SurvivorsRunUpgradeCategory category = SurvivorsRunUpgradeCategory.WeaponUpgrade, string passive = null) =>
            new SurvivorsRunUpgradeMetadata(choice.Id.Value, name, category, SurvivorsRunBuildSlotKind.None, "target", "Authored copy", requiredPassiveUpgradeId: passive);
        private static SurvivorsRelicDefinition Relic() => new SurvivorsRelicDefinition("relic", "Relic", "target", "effect", SurvivorsRelicEffectKind.DamageBonus, 1.25f, 1);

        private sealed class Host : ISurvivorsRunBuildPort, ISurvivorsDebugDraftPort
        {
            public readonly SurvivorsRunBuildState Build;
            public readonly SurvivorsDraftCardFactory Cards;
            public readonly SurvivorsDebugUpgradeFormatter Formatter;
            public readonly SurvivorsDebugDraftModel Drafts;
            public int Commands, TitleReads;
            public Host(RunUpgradeDefinition[] choices = null, SurvivorsRunUpgradeMetadata[] metadata = null)
            {
                Build = new SurvivorsRunBuildState(this);
                if (choices != null) Build.Initialize(new RunUpgradeCatalog(choices), metadata ?? Array.Empty<SurvivorsRunUpgradeMetadata>(), null, null);
                var labels = new SurvivorsBuildContentLabels(Build);
                Cards = new SurvivorsDraftCardFactory(Build, labels.ShortWeaponName);
                Formatter = new SurvivorsDebugUpgradeFormatter(Build, labels, Cards);
                var rarity = new SurvivorsDraftRarityPolicy(() => Tuning, () => 0f);
                Drafts = new SurvivorsDebugDraftModel(Build, new SurvivorsDraftCatalogPolicy(Build, rarity, () => Tuning), Formatter, labels, this);
            }
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning();
            public int WeaponCount => 0;
            public bool HasWeapon(string id) => false;
            public void AddWeapon(string id) => Commands++;
            public void PassiveAdded(RunUpgradeDefinition upgrade) => Commands++;
            public void RecordEvolutionTime() => Commands++;
            public void EvolutionAdded(RunUpgradeDefinition upgrade) => Commands++;
            public RunUpgradeDraft CurrentDraft { get; set; }
            public SurvivorsRelicDraft CurrentRelicDraft { get; set; }
            public string RewardOverlayTitle { get { TitleReads++; return "Authored reward title"; } }
        }
    }
}
