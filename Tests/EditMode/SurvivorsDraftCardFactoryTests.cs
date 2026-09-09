using System;
using Deucarian.RunUpgrades;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    [SetCulture("en-US")]
    public sealed class SurvivorsDraftCardFactoryTests
    {
        [Test]
        public void MissingCardsRetainReadablePlaceholdersAndThemeFallbacks()
        {
            var factory = CreateFactory(out _, Upgrade("unused"));
            var theme = new SurvivorsUiTheme
            {
                categoryStyles = new[] { new CategoryStyleToken("MetaReward", "Supplies", "supply-icon") }
            };
            SurvivorsDraftCard upgrade = factory.CreateUpgradeCard(2, null, theme, default);
            Assert.AreEqual(2, upgrade.Index);
            Assert.AreEqual("3", upgrade.Hotkey);
            Assert.AreEqual("Missing Choice", upgrade.Name);
            Assert.AreEqual("Missing", upgrade.RarityLabel);
            Assert.AreEqual("MetaReward", upgrade.CategoryId);
            Assert.AreEqual("Supplies", upgrade.CategoryLabel);
            Assert.AreEqual("supply-icon", upgrade.IconId);
            Assert.AreEqual("Common", upgrade.StyleToken);
            Assert.AreEqual("Affects Build", upgrade.AffectedLabel);
            Assert.AreEqual("No rank", upgrade.RankLabel);
            Assert.AreEqual("Missing upgrade definition.", upgrade.Description);
            Assert.AreEqual("No effect preview.", upgrade.EffectPreview);
            Assert.AreEqual(string.Empty, upgrade.RequirementHint);
            Assert.AreEqual(Color.white, upgrade.AccentColor);

            SurvivorsDraftCard relic = factory.CreateRelicCard(0, null, theme);
            Assert.AreEqual("Boss Relic", relic.Name);
            Assert.AreEqual("Relic", relic.CategoryId);
            Assert.AreEqual("Run relic", relic.RankLabel);
            Assert.AreEqual("Missing relic definition.", relic.Description);
            Assert.AreEqual("No effect preview.", relic.EffectPreview);
            Assert.AreEqual("Unique boss relic for this run.", relic.RequirementHint);
            Assert.AreEqual(Color.white, relic.AccentColor);
        }

        [Test]
        public void CardReadsCurrentRanksAndThemeWithoutAcquiringOrApplyingRewards()
        {
            RunUpgradeDefinition choice = Upgrade("passive");
            var metadata = Metadata(choice, SurvivorsRunUpgradeCategory.Passive, "Authored passive");
            var port = new BuildPort();
            var build = Build(port, choice, metadata, Metadata(choice, SurvivorsRunUpgradeCategory.Weapon, "Ignored duplicate"));
            var factory = new SurvivorsDraftCardFactory(build, id => "Named " + id);
            var theme = new SurvivorsUiTheme();
            var values = new SurvivorsDraftPreviewValues { ProjectileDamage = 20f };
            SurvivorsDraftCard first = factory.CreateUpgradeCard(0, choice, theme, values);
            Assert.AreEqual("Authored passive", first.Name);
            Assert.AreEqual("Passive", first.CategoryId);
            Assert.AreEqual("Rank 0->1/3", first.RankLabel);
            Assert.AreEqual("Damage: 20 -> 21", first.EffectPreview);
            Assert.AreEqual("Affects Named target", first.AffectedLabel);
            Assert.AreEqual(0, build.State.GetRank(choice.Id));
            Assert.AreEqual(0, build.ActivePassiveCount);
            Assert.AreEqual(0, port.CommandCount);

            Assert.IsTrue(build.State.Select(build.Catalog, choice.Id).Succeeded);
            values.ProjectileDamage = 70f;
            theme = new SurvivorsUiTheme
            {
                rarityStyles = new[] { new RarityStyleToken("Common", "Neon Tier", "neon-frame", "#123456", "unused") },
                categoryStyles = new[] { new CategoryStyleToken("PassiveUpgrade", "Neon passive", "neon-passive") }
            };
            SurvivorsDraftCard next = factory.CreateUpgradeCard(0, choice, theme, values);
            Assert.AreEqual("Rank 1->2/3", next.RankLabel);
            Assert.AreEqual("PassiveUpgrade", next.CategoryId);
            Assert.AreEqual("Neon passive", next.CategoryLabel);
            Assert.AreEqual("neon-passive", next.IconId);
            Assert.AreEqual("Neon Tier", next.RarityLabel);
            Assert.AreEqual("neon-frame", next.StyleToken);
            Assert.AreEqual(new Color(18f / 255f, 52f / 255f, 86f / 255f), next.AccentColor);
            Assert.AreEqual("Damage: 70 -> 71", next.EffectPreview);
            Assert.AreEqual(1, build.State.GetRank(choice.Id));
            Assert.AreEqual(0, port.CommandCount);
            Assert.IsTrue(build.State.Select(build.Catalog, choice.Id).Succeeded);
            Assert.IsTrue(build.State.Select(build.Catalog, choice.Id).Succeeded);
            Assert.AreEqual("Rank 3->3/3", factory.CreateUpgradeCard(0, choice, theme, values).RankLabel);
        }

        [Test]
        public void EvolutionCardUsesAuthoredNameAndLiveReducedPrerequisiteRank()
        {
            RunUpgradeDefinition choice = Upgrade("evolution", Effect(BasicSurvivorsGame.WeaponUnlockEffect, 1));
            var metadata = new SurvivorsRunUpgradeMetadata(choice.Id.Value, "Solar Lance", SurvivorsRunUpgradeCategory.Evolution,
                SurvivorsRunBuildSlotKind.None, "lance", "Turns bolts into rays", "lance", "rank", 5, "passive");
            var port = new BuildPort();
            var build = Build(port, choice, metadata,
                new SurvivorsRunUpgradeMetadata("rank", "Bolt Rank", SurvivorsRunUpgradeCategory.WeaponUpgrade, SurvivorsRunBuildSlotKind.None, "lance", "rank"),
                new SurvivorsRunUpgradeMetadata("passive", "Lens", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, "lance", "passive"));
            var factory = new SurvivorsDraftCardFactory(build, id => "Authored " + id);
            var theme = new SurvivorsUiTheme();
            SurvivorsDraftCard normal = factory.CreateUpgradeCard(0, choice, theme, default);
            Assert.IsTrue(normal.IsEvolution);
            Assert.AreEqual("Evolution", normal.RarityLabel);
            Assert.AreEqual(new Color(1f, 0.38f, 0.56f), normal.AccentColor);
            Assert.AreEqual("Evolves into Solar Lance; Unlocks Solar Lance", normal.EffectPreview);
            Assert.AreEqual("Evolution path: Authored lance needs Bolt Rank rank 5 plus Lens.", normal.RequirementHint);
            port.Tuning.EvolutionRequiredRankReduction = 4;
            Assert.AreEqual("Evolution path: Authored lance needs Bolt Rank rank 1 plus Lens.",
                factory.CreateUpgradeCard(0, choice, theme, default).RequirementHint);
            Assert.AreEqual(RunUpgradeRarity.Common, choice.Rarity);
            Assert.IsFalse(build.HasEvolution(choice.Id.Value));
        }

        [TestCase("rank", "passive", "weapon", "Requires Rank Name rank 2.")]
        [TestCase(null, "passive", "weapon", "Requires Passive Name.")]
        [TestCase(null, null, "weapon", "Requires Named weapon.")]
        [TestCase(null, null, null, "Pickup build: improves gem reach without bypassing draft pacing.")]
        public void RequirementHintsPreservePriorityOverCollectorHint(string rank, string passive, string weapon, string expected)
        {
            RunUpgradeDefinition choice = Upgrade("collector", Effect(BasicSurvivorsGame.MagnetRangeEffect, 1));
            var metadata = new SurvivorsRunUpgradeMetadata(choice.Id.Value, "Collector", SurvivorsRunUpgradeCategory.Passive,
                SurvivorsRunBuildSlotKind.Passive, "collector", "Collect", weapon, rank, 2, passive);
            var build = Build(new BuildPort(), choice, metadata,
                new SurvivorsRunUpgradeMetadata("rank", "Rank Name", SurvivorsRunUpgradeCategory.WeaponUpgrade, SurvivorsRunBuildSlotKind.None, "weapon", "rank"),
                new SurvivorsRunUpgradeMetadata("passive", "Passive Name", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, "weapon", "passive"));
            var factory = new SurvivorsDraftCardFactory(build, id => "Named " + id);
            SurvivorsDraftCard card = factory.CreateUpgradeCard(0, choice, new SurvivorsUiTheme(), default);
            Assert.AreEqual("PickupMagnet", card.CategoryId);
            Assert.AreEqual(expected, card.RequirementHint);
        }

        [Test]
        public void PreviewKeepsFirstTwoRawEffectsIncludingDuplicatesAndRemainingCount()
        {
            RunUpgradeEffectDescriptor damage = Effect(BasicSurvivorsGame.DamageBonusEffect, 2);
            RunUpgradeDefinition choice = Upgrade("four-effects", damage, damage,
                Effect(BasicSurvivorsGame.MoveSpeedEffect, 3), Effect(BasicSurvivorsGame.MaxHealthEffect, 5));
            var factory = CreateFactory(out var build, choice);
            var values = new SurvivorsDraftPreviewValues { ProjectileDamage = 10f };
            Assert.AreEqual("Damage: 10 -> 12; Damage: 10 -> 12; +2 more",
                factory.CreateUpgradeCard(0, choice, new SurvivorsUiTheme(), values).EffectPreview);
            Assert.AreEqual(0, build.State.GetRank(choice.Id));
            Assert.AreEqual(10f, values.ProjectileDamage);
        }

        [Test]
        public void CollectorPreviewPreservesInactivePulseFloorAndUnknownCollectorEffects()
        {
            RunUpgradeDefinition choice = Upgrade("collector", Effect(BasicSurvivorsGame.MagnetPulseEffect, 50),
                Effect(new RunUpgradeEffectId("custom.collector"), 1));
            var factory = CreateFactory(out _, choice);
            var values = new SurvivorsDraftPreviewValues
            {
                PickupMagnetPulseBaseIntervalSeconds = 5f,
                PickupMagnetPulseMinimumIntervalSeconds = 0.5f
            };
            SurvivorsDraftCard card = factory.CreateUpgradeCard(0, choice, new SurvivorsUiTheme(), values);
            Assert.AreEqual("Pulse Interval: inactive -> 1s; Improves collector effects.", card.EffectPreview);
            Assert.AreEqual("PickupMagnet", card.CategoryId);
            Assert.AreEqual(string.Empty, card.RequirementHint, "Missing metadata still suppresses the collector requirement hint.");
            values.CurrentPickupMagnetPulseIntervalSeconds = 2.5f;
            Assert.AreEqual("Pulse Interval: 2.5s -> 1s; Improves collector effects.",
                factory.CreateUpgradeCard(0, choice, new SurvivorsUiTheme(), values).EffectPreview);
        }

        [Test]
        public void ComparisonPreviewsKeepCooldownFloorsIntegerBonusesAndSignedPercentRules()
        {
            var values = new SurvivorsDraftPreviewValues
            {
                WeaponCooldownSeconds = 0.4f, ProjectileFanBonus = 2,
                ExecuteThresholdNormalized = 0.2f, CriticalChanceNormalized = 0.9f,
                CurrentPickupAttractionSpeed = 4f
            };
            AssertPreview(values, BasicSurvivorsGame.FireRateEffect, -9, "Cooldown: 0.40s -> 0.12s");
            AssertPreview(values, BasicSurvivorsGame.ProjectileFanEffect, -2, "Projectiles: 3 -> 4");
            AssertPreview(values, BasicSurvivorsGame.ExecuteEffect, -1, "Execute Threshold: 20% -> 0%");
            AssertPreview(values, BasicSurvivorsGame.CriticalChanceEffect, -1, "Crit Chance: 90% -> 90%");
            AssertPreview(values, BasicSurvivorsGame.CriticalChanceEffect, 1, "Crit Chance: 90% -> 100%");
            AssertPreview(values, BasicSurvivorsGame.MagnetSpeedEffect, -3, "Magnet Pull Speed: 4 -> 4");
        }

        [Test]
        public void WeaponUnlockUsesAuthoredMetadataBeforeTargetNameAndKeepsOneTimeLabel()
        {
            RunUpgradeDefinition choice = new RunUpgradeDefinition(new RunUpgradeId("new-weapon"), RunUpgradeRarity.Rare, 100, 1,
                new[] { Effect(BasicSurvivorsGame.WeaponUnlockEffect, 1) });
            var metadata = Metadata(choice, SurvivorsRunUpgradeCategory.Weapon, "Moon Needle");
            var build = Build(new BuildPort(), choice, metadata);
            var factory = new SurvivorsDraftCardFactory(build, id => "Target Name");
            SurvivorsDraftCard card = factory.CreateUpgradeCard(0, choice, new SurvivorsUiTheme(), default);
            Assert.AreEqual("NewWeapon", card.CategoryId);
            Assert.AreEqual("Weapon", card.CategoryLabel);
            Assert.AreEqual("One-time unlock", card.RankLabel);
            Assert.AreEqual("Unlocks Moon Needle", card.EffectPreview);
            build.Initialize(build.Catalog, Array.Empty<SurvivorsRunUpgradeMetadata>(), null, null);
            Assert.AreEqual("Unlocks Target Name", factory.CreateUpgradeCard(0, choice, new SurvivorsUiTheme(), default).EffectPreview);
        }

        [TestCase(SurvivorsRelicEffectKind.DamageBonus, 2.26f, "+2.3 damage to Named weapon", "+2.3 damage while this relic is held")]
        [TestCase(SurvivorsRelicEffectKind.CooldownMultiplier, -0.25f, "25% faster cooldown on Named weapon", "25% faster attacks for Named weapon")]
        [TestCase(SurvivorsRelicEffectKind.PickupRange, 3f, "+3 pickup range", "+3 pickup radius and relic surge momentum")]
        public void RelicCardsUseEffectSpecificCopyAndLiveThemedStyle(SurvivorsRelicEffectKind kind, float amount, string description, string preview)
        {
            var factory = CreateFactory(out _, Upgrade("unused"));
            var relic = new SurvivorsRelicDefinition("relic", "Prism", "weapon", "effect", kind, amount, 1);
            var theme = new SurvivorsUiTheme
            {
                rarityStyles = new[] { new RarityStyleToken("Relic", "Artifact", "artifact-style", "#FF00FF", "unused") },
                categoryStyles = new[] { new CategoryStyleToken("Relic", "Treasures", "treasure") }
            };
            SurvivorsDraftCard card = factory.CreateRelicCard(1, relic, theme);
            Assert.AreEqual("2", card.Hotkey);
            Assert.AreEqual("Prism", card.Name);
            Assert.AreEqual("Artifact", card.RarityLabel);
            Assert.AreEqual("Treasures", card.CategoryLabel);
            Assert.AreEqual("treasure", card.IconId);
            Assert.AreEqual("artifact-style", card.StyleToken);
            Assert.AreEqual(Color.magenta, card.AccentColor);
            Assert.AreEqual("Affects Named weapon", card.AffectedLabel);
            Assert.AreEqual(description, card.Description);
            Assert.AreEqual(preview, card.EffectPreview);
        }

        private static void AssertPreview(SurvivorsDraftPreviewValues values, RunUpgradeEffectId effect, double amount, string expected)
        {
            RunUpgradeDefinition choice = Upgrade("preview", Effect(effect, amount));
            var factory = CreateFactory(out _, choice);
            Assert.AreEqual(expected, factory.CreateUpgradeCard(0, choice, new SurvivorsUiTheme(), values).EffectPreview);
        }

        private static SurvivorsDraftCardFactory CreateFactory(out SurvivorsRunBuildState build, RunUpgradeDefinition choice)
        {
            build = Build(new BuildPort(), choice);
            return new SurvivorsDraftCardFactory(build, id => "Named " + id);
        }

        private static SurvivorsRunBuildState Build(BuildPort port, RunUpgradeDefinition choice, params SurvivorsRunUpgradeMetadata[] metadata)
        {
            var build = new SurvivorsRunBuildState(port);
            build.Initialize(new RunUpgradeCatalog(new[] { choice }), metadata, null, null);
            return build;
        }

        private static SurvivorsRunUpgradeMetadata Metadata(RunUpgradeDefinition choice, SurvivorsRunUpgradeCategory category, string name) =>
            new SurvivorsRunUpgradeMetadata(choice.Id.Value, name, category, SurvivorsRunBuildSlotKind.None, "target", "Authored description");

        private static RunUpgradeEffectDescriptor Effect(RunUpgradeEffectId effect, double amount) =>
            new RunUpgradeEffectDescriptor(effect, BasicSurvivorsGame.PlayerTarget, amount);

        private static RunUpgradeDefinition Upgrade(string id, params RunUpgradeEffectDescriptor[] effects) =>
            new RunUpgradeDefinition(new RunUpgradeId(id), RunUpgradeRarity.Common, 100, 3,
                effects.Length == 0 ? new[] { Effect(BasicSurvivorsGame.DamageBonusEffect, 1) } : effects);

        private sealed class BuildPort : ISurvivorsRunBuildPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning();
            public int CommandCount;
            public int WeaponCount => 0;
            public bool HasWeapon(string id) => false;
            public void AddWeapon(string id) => CommandCount++;
            public void PassiveAdded(RunUpgradeDefinition upgrade) => CommandCount++;
            public void RecordEvolutionTime() => CommandCount++;
            public void EvolutionAdded(RunUpgradeDefinition upgrade) => CommandCount++;
        }
    }
}
