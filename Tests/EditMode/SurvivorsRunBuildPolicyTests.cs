using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsRunBuildPolicyTests
    {
        [Test]
        public void ClassFilteringUsesFirstMatchingGateAndRetainsWholeCatalogWhenEverythingIsRejected()
        {
            var port = new BuildPort();
            var build = new SurvivorsRunBuildState(port);
            RunUpgradeDefinition a = Upgrade("a");
            RunUpgradeDefinition b = Upgrade("b");
            var catalog = new RunUpgradeCatalog(new[] { a, b });
            var blocked = new SurvivorsClassUpgradeGateDefinition("a", new[] { "other-class" });
            build.Initialize(catalog, Array.Empty<SurvivorsRunUpgradeMetadata>(), null, new[] { blocked });
            Assert.AreEqual(1, build.Catalog.Definitions.Count);
            Assert.AreEqual(b.Id, build.Catalog.Definitions[0].Id);
            build.Initialize(catalog, Array.Empty<SurvivorsRunUpgradeMetadata>(), null, new[] {
                new SurvivorsClassUpgradeGateDefinition("a", Array.Empty<string>()), blocked });
            Assert.AreEqual(2, build.Catalog.Definitions.Count);
            build.Initialize(catalog, Array.Empty<SurvivorsRunUpgradeMetadata>(), null, new[] {
                blocked, new SurvivorsClassUpgradeGateDefinition("b", new[] { "other-class" }) });
            Assert.AreSame(catalog, build.Catalog, "The existing all-rejected fallback is part of the template contract.");
        }

        [Test]
        public void FirstMetadataWinsAndRankedPassivesDoNotConsumeAnotherSlot()
        {
            var port = new BuildPort();
            var build = new SurvivorsRunBuildState(port);
            RunUpgradeDefinition a = Upgrade("a");
            RunUpgradeDefinition b = Upgrade("b");
            var first = Metadata("a", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive, "First");
            build.Initialize(new RunUpgradeCatalog(new[] { a, b }), new[] {
                null, first, Metadata("a", SurvivorsRunUpgradeCategory.Weapon, SurvivorsRunBuildSlotKind.Weapon, "Ignored"),
                Metadata("b", SurvivorsRunUpgradeCategory.Passive, SurvivorsRunBuildSlotKind.Passive) }, null, null);
            Assert.AreEqual("First", build.ResolveUpgradeDisplayName(a.Id));
            Assert.IsTrue(build.IsUpgradeEligibleForCurrentBuild(a));
            Assert.IsTrue(build.State.Select(build.Catalog, a.Id).Succeeded);
            build.RecordRunBuildSelection(a);
            Assert.AreEqual(1, build.ActivePassiveCount);
            Assert.IsTrue(build.IsUpgradeEligibleForCurrentBuild(a));
            Assert.IsFalse(build.IsUpgradeEligibleForCurrentBuild(b));
            Assert.AreEqual(SurvivorsRunUpgradeCategory.PassiveUpgrade, build.ResolveCurrentUpgradeCategory(a));
            Assert.IsTrue(build.State.Select(build.Catalog, a.Id).Succeeded);
            build.RecordRunBuildSelection(a);
            CollectionAssert.AreEqual(new[] { "passive" }, port.Events);
            build.ClearOwnedSelections();
            Assert.AreEqual(2, build.State.GetRank(a.Id), "World release preserves rank diagnostics until the next run.");
            build.Initialize(new RunUpgradeCatalog(new[] { a }), new[] { first }, null, null);
            Assert.AreEqual(0, build.State.GetRank(a.Id));
            Assert.AreEqual(0, build.ActivePassiveCount);
        }

        [Test]
        public void WeaponSlotsAndExistingLoadoutPreventDuplicateAcquisition()
        {
            var port = new BuildPort();
            var build = new SurvivorsRunBuildState(port);
            RunUpgradeDefinition weapon = Upgrade("weapon-upgrade");
            var metadata = new SurvivorsRunUpgradeMetadata(weapon.Id.Value, "Bolt", SurvivorsRunUpgradeCategory.Weapon,
                SurvivorsRunBuildSlotKind.Weapon, "bolt", "Adds bolt");
            build.Initialize(new RunUpgradeCatalog(new[] { weapon }), new[] { metadata }, null, null);
            port.Weapons.Add("bolt");
            Assert.IsFalse(build.IsUpgradeEligibleForCurrentBuild(weapon));
            port.Weapons.Clear();
            Assert.IsTrue(build.IsUpgradeEligibleForCurrentBuild(weapon));
            Assert.IsTrue(build.State.Select(build.Catalog, weapon.Id).Succeeded);
            build.RecordRunBuildSelection(weapon);
            Assert.IsTrue(port.Weapons.Contains("bolt"));
            Assert.IsTrue(build.IsUpgradeEligibleForCurrentBuild(weapon), "An owned weapon upgrade may rank up with a full loadout.");
            Assert.AreEqual(SurvivorsRunUpgradeCategory.WeaponUpgrade, build.ResolveCurrentUpgradeCategory(weapon));
            port.Tuning.MaxWeaponSlots = 0;
            port.Tuning.MaxPassiveSlots = -1;
            Assert.AreEqual(6, build.MaxWeaponSlots);
            Assert.AreEqual(6, build.MaxPassiveSlots);
        }

        [Test]
        public void EvolutionRequiresWeaponReducedRankAndPassiveThenRecordsOwnershipBeforeEffects()
        {
            var port = new BuildPort();
            port.Tuning.EvolutionRequiredRankReduction = 2;
            var build = new SurvivorsRunBuildState(port);
            RunUpgradeDefinition rank = Upgrade("rank");
            RunUpgradeDefinition passive = Upgrade("passive");
            RunUpgradeDefinition evolution = Upgrade("evolution");
            var metadata = new SurvivorsRunUpgradeMetadata("evolution", "Evolved", SurvivorsRunUpgradeCategory.Evolution,
                SurvivorsRunBuildSlotKind.None, "bolt", "Evolve", "bolt", "rank", 3, "passive");
            build.Initialize(new RunUpgradeCatalog(new[] { rank, passive, evolution }), new[] { metadata }, null, null);
            Assert.IsFalse(build.IsUpgradeEligibleForCurrentBuild(evolution));
            port.Weapons.Add("bolt");
            Assert.IsFalse(build.IsUpgradeEligibleForCurrentBuild(evolution));
            Assert.IsTrue(build.State.Select(build.Catalog, rank.Id).Succeeded);
            Assert.IsFalse(build.IsUpgradeEligibleForCurrentBuild(evolution));
            Assert.IsTrue(build.State.Select(build.Catalog, passive.Id).Succeeded);
            Assert.IsTrue(build.IsUpgradeEligibleForCurrentBuild(evolution));
            Assert.AreEqual(1, build.ResolveRequiredUpgradeRank(metadata));
            port.OnTime = () => Assert.IsFalse(build.HasEvolution("evolution"));
            port.OnEvolution = () => {
                Assert.IsTrue(build.HasEvolution("evolution"));
                Assert.AreEqual(1, build.WeaponEvolutionFeedbackCount);
            };
            build.RecordRunBuildSelection(evolution);
            CollectionAssert.AreEqual(new[] { "time", "evolved" }, port.Events);
            Assert.IsTrue(build.State.Banish(evolution.Id));
            Assert.IsFalse(build.IsUpgradeEligibleForCurrentBuild(evolution));
        }

        [Test]
        public void RarityProfileUsesLateBeforeMidAndRespondsToLiveTuning()
        {
            var tuning = new SurvivorsTemplateTuning { DraftMidRarityLevel = 5, DraftLateRarityLevel = 10 };
            var rarity = new SurvivorsDraftRarityPolicy(() => tuning, () => 0f);
            Assert.AreEqual(DraftRarityProfile.NormalEarly, rarity.ResolveNormalDraftRarityProfile(4));
            Assert.AreEqual(DraftRarityProfile.NormalMid, rarity.ResolveNormalDraftRarityProfile(5));
            Assert.AreEqual(DraftRarityProfile.NormalLate, rarity.ResolveNormalDraftRarityProfile(10));
            tuning.DraftMidRarityLevel = 0;
            tuning.DraftLateRarityLevel = 0;
            Assert.AreEqual(DraftRarityProfile.NormalLate, rarity.ResolveNormalDraftRarityProfile(1));
        }

        [Test]
        public void LuckDoesNotReviveDisabledRaritiesAndKeepsCommonFloor()
        {
            float luck = 0.5f;
            var tuning = new SurvivorsTemplateTuning { NormalMidCommonWeight = 1000, NormalMidRareWeight = 100, NormalMidEpicWeight = 0 };
            var rarity = new SurvivorsDraftRarityPolicy(() => tuning, () => luck);
            Assert.AreEqual(150, rarity.ResolveDraftRarityWeight(DraftRarityProfile.NormalMid, RunUpgradeRarity.Rare));
            Assert.AreEqual(0, rarity.ResolveDraftRarityWeight(DraftRarityProfile.NormalMid, RunUpgradeRarity.Epic));
            luck = 10f;
            Assert.AreEqual(350, rarity.ResolveDraftRarityWeight(DraftRarityProfile.NormalMid, RunUpgradeRarity.Common));
            luck = -1f;
            Assert.AreEqual(1000, rarity.ResolveDraftRarityWeight(DraftRarityProfile.NormalMid, RunUpgradeRarity.Common));
        }

        [Test]
        public void WeightedCatalogKeepsUpgradeEffectsAndDropsOnlyDisabledRarities()
        {
            var tuning = new SurvivorsTemplateTuning { BossRareWeight = 150, BossCommonWeight = 0 };
            var rarity = new SurvivorsDraftRarityPolicy(() => tuning, () => 0f);
            var effect = new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 2.25);
            var rare = new RunUpgradeDefinition(new RunUpgradeId("rare"), RunUpgradeRarity.Rare, 250, 3, new[] { effect });
            var weighted = rarity.CreateWeightedDraftCatalog(new[] { null, rare, Upgrade("common") }, DraftRarityProfile.Boss);
            Assert.AreEqual(1, weighted.Definitions.Count);
            RunUpgradeDefinition copy = weighted.Definitions[0];
            Assert.AreEqual(375, copy.Weight);
            Assert.AreEqual(rare.Id, copy.Id);
            Assert.AreEqual(rare.MaxRank, copy.MaxRank);
            CollectionAssert.AreEqual(rare.Effects, copy.Effects);
            CollectionAssert.AreEqual(rare.Prerequisites, copy.Prerequisites);
            CollectionAssert.AreEqual(rare.Exclusions, copy.Exclusions);
            Assert.AreEqual(250, rare.Weight);
            Assert.IsNull(rarity.CreateWeightedDraftCatalog(new[] { Upgrade("disabled") }, DraftRarityProfile.Boss));
        }

        private static RunUpgradeDefinition Upgrade(string id) => new RunUpgradeDefinition(new RunUpgradeId(id), RunUpgradeRarity.Common, 100, 3,
            new[] { new RunUpgradeEffectDescriptor(BasicSurvivorsGame.DamageBonusEffect, BasicSurvivorsGame.PlayerTarget, 1d) });
        private static SurvivorsRunUpgradeMetadata Metadata(string id, SurvivorsRunUpgradeCategory category, SurvivorsRunBuildSlotKind slot, string label = null) =>
            new SurvivorsRunUpgradeMetadata(id, label ?? id, category, slot, id, "Test metadata");

        private sealed class BuildPort : ISurvivorsRunBuildPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning { MaxWeaponSlots = 1, MaxPassiveSlots = 1 };
            public readonly HashSet<string> Weapons = new HashSet<string>(StringComparer.Ordinal);
            public readonly List<string> Events = new List<string>();
            public Action OnTime;
            public Action OnEvolution;
            public int WeaponCount => Weapons.Count;
            public bool HasWeapon(string id) => Weapons.Contains(id);
            public void AddWeapon(string id) => Weapons.Add(id);
            public void PassiveAdded(RunUpgradeDefinition upgrade) => Events.Add("passive");
            public void RecordEvolutionTime() { Events.Add("time"); OnTime?.Invoke(); }
            public void EvolutionAdded(RunUpgradeDefinition upgrade) { Events.Add("evolved"); OnEvolution?.Invoke(); }
        }
    }
}
