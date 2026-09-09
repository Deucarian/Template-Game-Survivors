using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsDraftHeaderTests
    {
        [Test]
        public void RelicHeaderHasPriorityWithoutReadingUpgradeDraftOrCategories()
        {
            var port = new Port { RelicOpen = true, RejectDraftReads = true };
            var header = new SurvivorsDraftHeader(port);
            Assert.That(header.ResolveAccent(), Is.EqualTo(new Color(1f, 0.84f, 0.42f)));
            Assert.That(port.Events, Is.EqualTo(new[] { "relic", "theme" }));
            port.Theme = Theme("Relic", "#123456");
            Assert.That(header.ResolveAccent(), Is.EqualTo(new Color(18f / 255f, 52f / 255f, 86f / 255f)));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingOrEmptyDraftUsesWhiteCommonFallbackAndReadsThemeAfterGuard(bool empty)
        {
            var port = new Port { Draft = empty ? new RunUpgradeDraft(null) : null };
            Assert.That(new SurvivorsDraftHeader(port).ResolveAccent(), Is.EqualTo(Color.white));
            Assert.That(port.Events, Is.EqualTo(empty ? new[] { "relic", "draft", "draft", "theme" } : new[] { "relic", "draft", "theme" }));
            port.Theme = Theme("Common", "#00FF00");
            Assert.That(new SurvivorsDraftHeader(port).ResolveAccent(), Is.EqualTo(Color.green));
        }

        [Test]
        public void FirstEvolutionWinsOverLegendaryAndStopsCategoryReadsBeforeResolvingTheme()
        {
            var first = Upgrade("legendary", RunUpgradeRarity.Legendary);
            var evolution = Upgrade("evolution");
            var port = new Port { Draft = new RunUpgradeDraft(new[] { null, first, evolution, Upgrade("unread") }) };
            port.Category = choice =>
            {
                Assert.That(choice.Id.Value, Is.Not.EqualTo("unread"));
                if (choice == evolution) port.Theme = Theme("Evolution", "#FF0000");
                return choice == evolution ? SurvivorsRunUpgradeCategory.Evolution : SurvivorsRunUpgradeCategory.Passive;
            };
            Assert.That(new SurvivorsDraftHeader(port).ResolveAccent(), Is.EqualTo(Color.red));
            Assert.That(port.CategoryIds, Is.EqualTo(new[] { "legendary", "evolution" }));
            Assert.That(port.Events[port.Events.Count - 1], Is.EqualTo("theme"));
        }

        [Test]
        public void OrdinaryDraftUsesHighestRarityWhileAllNullEntriesKeepNonemptyCommonFallback()
        {
            var port = new Port { Draft = new RunUpgradeDraft(new[] { Upgrade("rare", RunUpgradeRarity.Rare), null, Upgrade("epic", RunUpgradeRarity.Epic), Upgrade("common") }) };
            var header = new SurvivorsDraftHeader(port);
            Assert.That(header.ResolveAccent(), Is.EqualTo(new Color(0.9f, 0.46f, 1f)));
            Assert.That(port.CategoryIds, Is.EqualTo(new[] { "rare", "epic", "common" }));
            port.Theme = Theme("Epic", "#0000FF");
            Assert.That(header.ResolveAccent(), Is.EqualTo(Color.blue));
            port.Theme = Theme("Common", "invalid-color");
            port.Draft = new RunUpgradeDraft(new RunUpgradeDefinition[] { null });
            Assert.That(header.ResolveAccent(), Is.EqualTo(new Color(0.78f, 0.84f, 0.88f)), "Nonempty missing entries retain the ordinary rarity fallback, distinct from an empty draft.");
        }

        [Test]
        public void CategoryCallbackChangesAreVisibleToSubsequentDraftAndThemeReads()
        {
            var original = Upgrade("original");
            var port = new Port { Draft = new RunUpgradeDraft(new[] { original, Upgrade("stale") }) };
            var replacement = new RunUpgradeDraft(new[] { Upgrade("new", RunUpgradeRarity.Legendary) });
            port.Category = choice =>
            {
                Assert.That(choice, Is.SameAs(original));
                port.Draft = replacement;
                port.Theme = Theme("Legendary", "#00FF00");
                return SurvivorsRunUpgradeCategory.Passive;
            };
            Assert.That(new SurvivorsDraftHeader(port).ResolveAccent(), Is.EqualTo(Color.green));
            Assert.That(port.CategoryIds, Is.EqualTo(new[] { "original" }));
            Assert.That(port.Draft, Is.SameAs(replacement));
        }

        [Test]
        public void TitlesRetainAllSelectionLabelsAndUnknownKindFallback()
        {
            Assert.That(SurvivorsDraftHeader.Title(SurvivorsRewardSelectionKind.BossRelic), Is.EqualTo("Choose a Boss Relic"));
            Assert.That(SurvivorsDraftHeader.Title(SurvivorsRewardSelectionKind.EliteUpgrade), Is.EqualTo("Elite Reward"));
            Assert.That(SurvivorsDraftHeader.Title(SurvivorsRewardSelectionKind.BossUpgrade), Is.EqualTo("Boss Evolution Reward"));
            Assert.That(SurvivorsDraftHeader.Title(SurvivorsRewardSelectionKind.LevelUp), Is.EqualTo("Level Up"));
            Assert.That(SurvivorsDraftHeader.Title(SurvivorsRewardSelectionKind.None), Is.EqualTo("Level Up"));
            Assert.That(SurvivorsDraftHeader.Title((SurvivorsRewardSelectionKind)99), Is.EqualTo("Level Up"));
        }

        private static SurvivorsUiTheme Theme(string rarity, string color) => new SurvivorsUiTheme
        { rarityStyles = new[] { new RarityStyleToken(rarity, rarity, rarity, color, "unused") } };
        private static RunUpgradeDefinition Upgrade(string id, RunUpgradeRarity rarity = RunUpgradeRarity.Common)
            => new RunUpgradeDefinition(new RunUpgradeId(id), rarity, 1, 3,
                new[] { new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 1) });

        private sealed class Port : ISurvivorsDraftHeaderReadPort
        {
            internal readonly List<string> Events = new List<string>();
            internal readonly List<string> CategoryIds = new List<string>();
            internal bool RelicOpen, RejectDraftReads;
            internal RunUpgradeDraft Draft;
            internal SurvivorsUiTheme Theme = new SurvivorsUiTheme();
            internal Func<RunUpgradeDefinition, SurvivorsRunUpgradeCategory> Category = _ => SurvivorsRunUpgradeCategory.Passive;
            public bool IsRelicChoiceOpen { get { Events.Add("relic"); return RelicOpen; } }
            public RunUpgradeDraft CurrentDraft { get { Assert.That(RejectDraftReads, Is.False); Events.Add("draft"); return Draft; } }
            public SurvivorsRunUpgradeCategory ResolveCategory(RunUpgradeDefinition choice)
            { Events.Add("category:" + choice.Id.Value); CategoryIds.Add(choice.Id.Value); return Category(choice); }
            public SurvivorsUiTheme ActiveTheme { get { Events.Add("theme"); return Theme; } }
        }
    }
}
