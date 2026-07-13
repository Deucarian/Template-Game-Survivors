using System;
using System.Collections.Generic;
using System.Linq;
using Deucarian.Attacks.Editor;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.RunUpgrades.Editor;
using Deucarian.TemplateGameSurvivors.Editor;
using Deucarian.WeaponSystems.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsContentLensEditModeTests
    {
        [SetUp]
        public void SetUp()
        {
            SurvivorsContentPackProvider.EnsureRegistered();
            SurvivorsContentLensAdapters.EnsureRegistered();
        }

        [Test]
        public void BasicAndNeon_ExposeIndependentRealRecordsToEveryRequiredLens()
        {
            IReadOnlyList<GameContentPackManifestEntry> entries = ImportedEntries();
            Assert.That(entries.Select(entry => entry.Manifest.PackId), Is.EquivalentTo(new[] { "basic-survivors", "neon-arcana" }));
            foreach (GameContentPackManifestEntry entry in entries)
            {
                SurvivorsContentPackIndex index = SurvivorsContentPackIndex.Build(entry.Manifest);
                Assert.That(index.Records.Count, Is.EqualTo(251), entry.Manifest.PackId);
                Assert.That(index.Records.Count(record => record.HasCapability(GameContentRecordCapabilities.Attack)), Is.EqualTo(10));
                Assert.That(index.Records.Count(record => record.HasCapability(GameContentRecordCapabilities.Weapon)), Is.EqualTo(10));
                Assert.That(index.Records.Count(record => record.HasCapability(GameContentRecordCapabilities.Enemy)), Is.EqualTo(10));
                Assert.That(index.Records.Count(record => record.HasCapability(GameContentRecordCapabilities.Encounter)), Is.EqualTo(5));
                Assert.That(index.Records.Count(record => record.HasCapability(GameContentRecordCapabilities.Upgrade)), Is.EqualTo(82));
                Assert.That(index.Records.Count(record => record.HasCapability(GameContentRecordCapabilities.Projectile)), Is.EqualTo(1));
                Assert.That(index.Records.Any(record => record.HasCapability(GameContentRecordCapabilities.Passive)), Is.True);
                Assert.That(index.Records.Any(record => record.HasCapability(GameContentRecordCapabilities.PickupMagnet)), Is.True);
                Assert.That(index.Records.Any(record => record.HasCapability(GameContentRecordCapabilities.Mutation)), Is.True);
                Assert.That(index.Records.Any(record => record.HasCapability(GameContentRecordCapabilities.Evolution)), Is.True);
                Assert.That(index.Records.Any(record => record.HasCapability(GameContentRecordCapabilities.MetaUpgrade)), Is.True);
                Assert.That(index.Records.Any(record => record.HasCapability(GameContentRecordCapabilities.Boss)), Is.True);
                Assert.That(index.Records.Any(record => record.HasCapability(GameContentRecordCapabilities.Elite)), Is.True);
                Assert.That(index.Records.Any(record => record.HasCapability(GameContentRecordCapabilities.Tower)), Is.False);
                Assert.That(index.Records.Select(record => record.CanonicalKey).Distinct().Count(), Is.EqualTo(index.Records.Count));
                Assert.That(index.Records.All(record => record.CanonicalKey.PackId == entry.Manifest.PackId), Is.True);
            }
        }

        [Test]
        public void ArcaneWand_IsOneCanonicalRecordInAttackAndWeaponLenses()
        {
            SurvivorsContentPackIndex index = Index("basic-survivors");
            GameContentRecordDescriptor wand = index.Records.Single(record =>
                record.SourceRecordId == "weapon.survivors.arcane-wand");

            Assert.That(wand.HasCapability(GameContentRecordCapabilities.Attack), Is.True);
            Assert.That(wand.HasCapability(GameContentRecordCapabilities.Weapon), Is.True);
            Assert.That(GameContentRecordProjectionRegistry<AttackContentRecordProjection>.TryProject(
                wand,
                out AttackContentRecordProjection attack), Is.True);
            Assert.That(GameContentRecordProjectionRegistry<WeaponContentRecordProjection>.TryProject(
                wand,
                out WeaponContentRecordProjection weapon), Is.True);
            Assert.That(attack.Record, Is.SameAs(wand));
            Assert.That(weapon.Record, Is.SameAs(wand));
            Assert.That(attack.Record.CanonicalKey, Is.EqualTo(weapon.Record.CanonicalKey));
            Assert.That(attack.Damage, Is.EqualTo(7f).Within(0.001f));
            Assert.That(attack.CooldownSeconds, Is.EqualTo(0.52f).Within(0.001f));
            Assert.That(attack.Range, Is.EqualTo(14f).Within(0.001f));
            Assert.That(weapon.IsTower, Is.False);
            Assert.That(weapon.PayloadRecordId, Is.EqualTo("projectile.survivors.arcane-bolt"));
        }

        [Test]
        public void EnemyAndBossSemanticViews_KeepOneIdentityAndAuthoredStats()
        {
            SurvivorsContentPackIndex index = Index("basic-survivors");
            GameContentRecordDescriptor boss = index.Records.Single(record => record.HasCapability(GameContentRecordCapabilities.Boss));
            TextAsset source = boss.SourceAsset as TextAsset;
            Assert.That(source, Is.Not.Null);
            string sourceTextBefore = source.text;

            Assert.That(boss.HasCapability(GameContentRecordCapabilities.Enemy), Is.True);
            Assert.That(GameContentRecordProjectionRegistry<EnemyContentRecordProjection>.TryProject(
                boss,
                out EnemyContentRecordProjection projection), Is.True);
            Assert.That(projection.Record.CanonicalKey, Is.EqualTo(boss.CanonicalKey));
            Assert.That(projection.Health, Is.GreaterThan(0f));
            Assert.That(projection.MoveSpeed, Is.GreaterThan(0f));
            Assert.That(projection.ContactDamage, Is.GreaterThan(0f));
            Assert.That(projection.MajorThreat, Is.True);
            Assert.That(projection.LifeBarBehavior, Does.Contain("boss").IgnoreCase);
            Assert.That(projection.GameSpecificSummary, Does.Contain("leash").IgnoreCase);
            Assert.That(source.text, Is.EqualTo(sourceTextBefore));

            foreach (string role in new[] { "Splitter", "Summoner" })
            {
                GameContentRecordDescriptor specialist = index.Records.Single(record =>
                    record.PlayerFacingMetadata.Any(value => value.Label == "Role" && value.Value == role));
                Assert.That(GameContentRecordProjectionRegistry<EnemyContentRecordProjection>.TryProject(
                    specialist,
                    out EnemyContentRecordProjection specialistProjection), Is.True);
                Assert.That(specialistProjection.GameSpecificSummary, Does.Contain(
                    role == "Splitter" ? "child" : "support").IgnoreCase);
            }
        }

        [Test]
        public void EncounterProjection_UsesAuthoredDurationsAndPackSafeReferences()
        {
            foreach (string packId in new[] { "basic-survivors", "neon-arcana" })
            {
                SurvivorsContentPackIndex index = Index(packId);
                GameContentRecordDescriptor standard = index.Records.Single(record =>
                    record.SourceRecordId == "HumanPlaytest" && record.HasCapability(GameContentRecordCapabilities.Encounter));
                GameContentRecordDescriptor sprint = index.Records.Single(record =>
                    record.SourceRecordId == "SprintRun" && record.HasCapability(GameContentRecordCapabilities.Encounter));
                Assert.That(GameContentRecordProjectionRegistry<EncounterContentRecordProjection>.TryProject(
                    standard,
                    out EncounterContentRecordProjection standardProjection), Is.True);
                Assert.That(GameContentRecordProjectionRegistry<EncounterContentRecordProjection>.TryProject(
                    sprint,
                    out EncounterContentRecordProjection sprintProjection), Is.True);
                Assert.That(standardProjection.DurationSeconds, Is.EqualTo(1800f).Within(0.01f));
                Assert.That(standardProjection.VictoryTimeSeconds, Is.EqualTo(1800f).Within(0.01f));
                Assert.That(sprintProjection.DurationSeconds, Is.EqualTo(300f).Within(0.01f));
                Assert.That(sprintProjection.VictoryTimeSeconds, Is.EqualTo(300f).Within(0.01f));
                Assert.That(standardProjection.EnemyKeys, Is.Not.Empty);
                Assert.That(standardProjection.RewardKeys, Is.Not.Empty);
                Assert.That(standard.OutboundReferences.All(reference =>
                    reference.TargetRecordKey == null ||
                    reference.TargetRecordKey.PackId == packId), Is.True);
                Assert.That(standard.OutboundReferences.All(reference =>
                    reference.TargetRecordKey == null ||
                    reference.TargetRecordKey.OwningPackageId == SurvivorsContentPackProvider.OwningPackageId), Is.True);
            }
        }

        [Test]
        public void UpgradeSubtypes_UseOneRecordAndExposeAuthoredComparisonData()
        {
            SurvivorsContentPackIndex index = Index("basic-survivors");
            GameContentRecordDescriptor evolution = index.Records.First(record =>
                record.HasCapability(GameContentRecordCapabilities.Evolution));
            Assert.That(evolution.HasCapability(GameContentRecordCapabilities.Upgrade), Is.True);
            Assert.That(GameContentRecordProjectionRegistry<UpgradeContentRecordProjection>.TryProject(
                evolution,
                out UpgradeContentRecordProjection projection), Is.True);
            Assert.That(projection.Record, Is.SameAs(evolution));
            Assert.That(projection.MaxRank, Is.GreaterThan(0));
            Assert.That(projection.EffectKind, Is.Not.Empty);
            Assert.That(projection.ComparisonSummary, Is.Not.Empty);

            GameContentRecordDescriptor gatedUpgrade = index.Records.First(record =>
                record.HasCapability(GameContentRecordCapabilities.Upgrade) &&
                record.PlayerFacingMetadata.Any(value => value.Label == "Class Gate"));
            Assert.That(GameContentRecordProjectionRegistry<UpgradeContentRecordProjection>.TryProject(
                gatedUpgrade,
                out UpgradeContentRecordProjection gatedProjection), Is.True);
            Assert.That(gatedProjection.ClassGateSummary, Is.Not.Empty);
        }

        [Test]
        public void PackBackends_EditExistingOnlyAndBasicNeverResolvesAgainstNeon()
        {
            SurvivorsContentPackProvider provider = SurvivorsContentPackProvider.Instance;
            IReadOnlyList<GameContentPackDescriptor> packs = provider.GetContentPacks();
            Assert.That(packs, Has.Count.EqualTo(2));
            Assert.That(packs.All(pack => pack.Access.CanRead), Is.True);
            Assert.That(packs.All(pack => pack.Access.CanValidate), Is.True);
            Assert.That(packs.All(pack => pack.Access.CanEditExisting), Is.True);
            Assert.That(packs.All(pack => !pack.Access.CanCreate && !pack.Access.CanDuplicate && !pack.Access.CanDelete && !pack.Access.CanClonePack), Is.True);

            GameContentPackCatalog catalog = GameContentPackCatalog.Build(new[] { provider });
            GameContentPackDescriptor basicPack = packs.Single(pack => pack.PackId == "basic-survivors");
            GameContentPackContext basic = new GameContentPackSelectionState().Select(catalog, basicPack.StableKey);
            GameContentRecordDescriptor neonWand = provider.GetRecords("neon-arcana").Single(record =>
                record.SourceRecordId == "weapon.survivors.arcane-wand");
            Assert.That(basic.ResolveRecord(neonWand.CanonicalKey), Is.Null);
        }

        private static SurvivorsContentPackIndex Index(string packId)
        {
            GameContentPackManifestEntry entry = ImportedEntries().Single(value => value.Manifest.PackId == packId);
            SurvivorsContentPackIndex index = SurvivorsContentPackIndex.Build(entry.Manifest);
            Assert.That(index.Validation.ErrorCount, Is.Zero, packId);
            return index;
        }

        private static IReadOnlyList<GameContentPackManifestEntry> ImportedEntries()
        {
            GameContentPackDiscoveryReport report = GameContentPackDiscovery.Discover(SurvivorsContentPackProvider.StableProviderId);
            Assert.That(report.Entries.Count, Is.EqualTo(2));
            return report.Entries;
        }
    }
}
