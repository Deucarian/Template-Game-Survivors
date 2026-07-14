using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.RunUpgrades;
using Deucarian.TemplateGameSurvivors.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsUpgradeEffectEditingEditModeTests
    {
        private sealed class Fixture
        {
            public SurvivorsContentPackProvider Provider;
            public GameContentPackDescriptor Pack;
            public GameContentPackManifest Manifest;
            public SurvivorsContentPackIndex Index;
            public GameContentRecordDescriptor Record;
            public SurvivorsEditableSource Source;
            public GameContentEditRequest Request;
        }

        [OneTimeSetUp]
        public void GrantImportedCopyConsent()
        {
            SurvivorsImportedSampleEditConsent.GrantForTests();
        }

        [OneTimeTearDown]
        public void ClearImportedCopyConsent()
        {
            SurvivorsImportedSampleEditConsent.ClearForTests();
        }

        [TestCase("basic-survivors")]
        [TestCase("neon-arcana")]
        public void Descriptor_ExposesEmbeddedEffectsWithoutCreatingCanonicalChildren(string packId)
        {
            Fixture fixture = ResolveEditable(packId, "upgrade.survivors.keen-edge");
            byte[] before = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
            int canonicalCount = fixture.Index.Records.Count;
            int upgradeCount = fixture.Index.Records.Count(record =>
                record.CanonicalKey.SourceId == SurvivorsContentPackIndex.UpgradesSourceId &&
                record.HasCapability(GameContentRecordCapabilities.Upgrade));

            using (IGameContentEditSession session = fixture.Provider.BeginEdit(fixture.Request))
            {
                Assert.That(session, Is.InstanceOf<IGameContentStructuredCollectionEditSession>());
                Assert.That(session.Fields.Where(field => !field.IsReadOnly).Select(field => field.FieldId),
                    Is.EqualTo(new[] { SurvivorsUpgradeEffectEditing.CollectionFieldId }));
                Assert.That(session.Fields.Where(field => field.IsReadOnly).Select(field => field.FieldId),
                    Is.EqualTo(new[] { "id" }));

                GameContentFieldDescriptor field = EffectsField(session);
                GameContentStructuredCollectionFieldDescriptor collection = field.StructuredCollection;
                GameContentStructuredRowDescriptor row = collection.RowDescriptor;
                Assert.That(field.FieldType, Is.EqualTo(GameContentFieldType.OrderedStructuredCollection));
                Assert.That(field.DisplayName, Is.EqualTo("Effects"));
                Assert.That(field.FieldId, Is.EqualTo("effects"));
                Assert.That(collection.MinimumCount, Is.Zero);
                Assert.That(collection.MaximumCount, Is.Null);
                Assert.That(collection.DuplicatePolicy, Is.EqualTo(GameContentStructuredRowDuplicatePolicy.Allow));
                Assert.That(collection.PermittedOperations,
                    Is.EqualTo(GameContentStructuredCollectionPermittedOperations.All));
                Assert.That(collection.OrderingSemantics, Does.Contain("authored array order"));
                Assert.That(collection.RuntimeImpact,
                    Is.EqualTo(GameContentReferenceRuntimeImpact.Refresh |
                               GameContentReferenceRuntimeImpact.Rebind |
                               GameContentReferenceRuntimeImpact.Restart));
                Assert.That(row.RowSchemaId, Is.EqualTo(SurvivorsUpgradeEffectEditing.RowSchemaId));
                Assert.That(row.NativeKey, Is.Null);
                Assert.That(row.RepresentsIndependentCanonicalRecord, Is.False);
                Assert.That(row.SupportsAdd, Is.True);
                Assert.That(row.SupportsRemove, Is.True);
                Assert.That(row.SupportsMove, Is.True);
                Assert.That(row.SupportsRowFieldReplacement, Is.True);
                Assert.That(row.Fields.Select(candidate => candidate.FieldId),
                    Is.EqualTo(new[] { "effect", "target", "amount" }));
                Assert.That(row.Fields.Select(candidate => candidate.FieldType),
                    Is.EqualTo(new[]
                    {
                        GameContentFieldType.Enum,
                        GameContentFieldType.Enum,
                        GameContentFieldType.Number
                    }));
                Assert.That(row.Fields.All(candidate => candidate.Required && !candidate.IsReadOnly), Is.True);
                Assert.That(row.Fields.All(candidate => candidate.FieldType != GameContentFieldType.String &&
                                                       candidate.FieldType != GameContentFieldType.RecordReference), Is.True);

                GameContentOrderedStructuredCollectionValue original = EffectiveEffects(session);
                Assert.That(original.Rows.Select(candidate => candidate.OriginalIndex), Is.EqualTo(new[] { 0, 1 }));
                Assert.That(original.Rows.Select(candidate => candidate.RowKey).Distinct().Count(), Is.EqualTo(2));
                GameContentStructuredCollectionOperationResult added = Structured(session).ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.AddRow(CopyFields(original.Rows[0])));
                Assert.That(added.Succeeded, Is.True, added.Message);
                Assert.That(added.RowKey, Is.Not.Null);
                Assert.That(EffectiveEffects(session).Count, Is.EqualTo(3));
                Assert.That(Structured(session).ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.RemoveRow(added.RowKey)).Succeeded, Is.True);
                Assert.That(EffectiveEffects(session).Count, Is.EqualTo(2));
                Assert.That(session.Rollback().Succeeded, Is.True);
            }

            SurvivorsContentPackIndex after = SurvivorsContentPackIndex.Build(fixture.Manifest);
            Assert.That(after.Records.Count, Is.EqualTo(canonicalCount));
            Assert.That(after.Records.Count(record =>
                record.CanonicalKey.SourceId == SurvivorsContentPackIndex.UpgradesSourceId &&
                record.HasCapability(GameContentRecordCapabilities.Upgrade)), Is.EqualTo(upgradeCount));
            Assert.That(after.Records.Any(record => record.CategoryId == "effects"), Is.False);
            Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(before));
        }

        [TestCase("basic-survivors")]
        [TestCase("neon-arcana")]
        public void Descriptor_ClosedOptionsCoverEveryStrictAuthoredEffectAndKnownTarget(string packId)
        {
            Fixture fixture = ResolveEditable(packId, "upgrade.survivors.keen-edge");
            Assert.That(TryBindStrict(fixture.Manifest, out SurvivorsAuthoredContentDefinition authored, out string error),
                Is.True,
                error);
            string[] authoredEffects = authored.RunUpgradeCatalog.Definitions
                .SelectMany(definition => definition.Effects)
                .Select(effect => effect.EffectId.Value)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] options = SurvivorsUpgradeEffectEditing.SupportedEffectIds
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Assert.That(authoredEffects.All(options.Contains), Is.True);
            Assert.That(options, Does.Contain("survivors.critical.damage"));
            Assert.That(options, Does.Contain(BasicSurvivorsGame.CriticalDamageEffect.Value));
            Assert.That(options.All(value => value.StartsWith("survivors.", StringComparison.Ordinal)), Is.True);

            string[] knownTargets = BasicSurvivorsGame.CreateKnownUpgradeTargets()
                .Select(target => target.Value)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Assert.That(SurvivorsUpgradeEffectEditing.SupportedTargetIds.OrderBy(value => value, StringComparer.Ordinal),
                Is.EqualTo(knownTargets));

            using (IGameContentEditSession session = fixture.Provider.BeginEdit(fixture.Request))
            {
                GameContentStructuredRowDescriptor row = EffectsField(session).StructuredCollection.RowDescriptor;
                Assert.That(row.FindField("effect").EnumOptions.Select(option => option.Token),
                    Is.EqualTo(options));
                Assert.That(row.FindField("target").EnumOptions.Select(option => option.Token),
                    Is.EqualTo(knownTargets));
                Assert.That(session.Rollback().Succeeded, Is.True);
            }
        }

        [Test]
        public void Availability_IsPackScopedAndOnlyDirectSupportedUpgradeEffectsAreEditable()
        {
            Fixture basic = ResolveEditable("basic-survivors", "upgrade.survivors.keen-edge");
            Fixture neon = ResolveEditable("neon-arcana", "upgrade.survivors.keen-edge");
            Assert.That(basic.Record.CanonicalKey, Is.Not.EqualTo(neon.Record.CanonicalKey));
            Assert.That(basic.Source.SourcePath.FullPath, Is.Not.EqualTo(neon.Source.SourcePath.FullPath));
            Assert.That(basic.Source.SourceTarget.LockKey, Is.Not.EqualTo(neon.Source.SourceTarget.LockKey));
            Assert.That(basic.Provider.CanEdit(basic.Request).IsEditable, Is.True);
            Assert.That(neon.Provider.CanEdit(neon.Request).IsEditable, Is.True);

            Fixture missingArray = ResolveRecord("basic-survivors", "upgrade.survivors.arcane-damage");
            GameContentEditAvailability missing = missingArray.Provider.CanEdit(missingArray.Request);
            Assert.That(missing.IsEditable, Is.False);
            Assert.That(missing.DisabledReason, Does.Contain("Effects array"));

            Fixture enemy = ResolveRecord("basic-survivors", "enemy.survivors.swarm");
            using (IGameContentEditSession session = enemy.Provider.BeginEdit(enemy.Request))
            {
                Assert.That(session.Fields.Any(field => field.FieldId == "effects"), Is.False);
                Assert.That(session.Rollback().Succeeded, Is.True);
            }

            var allPacks = new GameContentEditRequest(
                GameContentPackContext.AllPacksSelectionKey,
                basic.Record.CanonicalKey,
                basic.Provider.ProviderId);
            Assert.That(basic.Provider.CanEdit(allPacks).DisabledReason, Does.Contain("All Packs"));
            Assert.That(SurvivorsEditableSource.CanAdvertiseEditing(
                basic.Manifest,
                GameContentPackSourceKind.Package,
                out string packageReason), Is.False);
            Assert.That(packageReason, Does.Contain("Package source"));
        }

        [Test]
        public void StructuredOperations_AreStageOnlyValidatedKeyedAndUndoable()
        {
            Fixture fixture = ResolveEditable("basic-survivors", "upgrade.survivors.keen-edge");
            Fixture other = ResolveEditable("neon-arcana", "upgrade.survivors.keen-edge");
            byte[] before = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
            byte[] otherBefore = File.ReadAllBytes(other.Source.SourcePath.FullPath);
            using (IGameContentEditSession session = fixture.Provider.BeginEdit(fixture.Request))
            {
                IGameContentStructuredCollectionEditSession structured = Structured(session);
                GameContentOrderedStructuredCollectionValue baseline = EffectiveEffects(session);
                GameContentStructuredRowKey firstKey = baseline.Rows[0].RowKey;
                GameContentStructuredRowKey secondKey = baseline.Rows[1].RowKey;

                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.AddRow(new[]
                    {
                        new GameContentStructuredRowFieldValue("effect", GameContentFieldValue.FromEnum("survivors.damage.flat"))
                    })).Succeeded, Is.False);
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.AddRow(Fields(
                        "survivors.not-supported",
                        "survivors.player",
                        1d))).Succeeded, Is.False);
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.AddRow(Fields(
                        "survivors.damage.flat",
                        "survivors.not-a-target",
                        1d))).Succeeded, Is.False);
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.AddRow(Fields(
                        "survivors.damage.flat",
                        "survivors.player",
                        0d))).Succeeded, Is.False);
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.AddRow(Fields(
                        "survivors.damage.flat",
                        "survivors.player",
                        double.NaN))).Succeeded, Is.False);
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.AddRow(Fields(
                        "survivors.damage.flat",
                        "survivors.player",
                        double.PositiveInfinity))).Succeeded, Is.False);

                GameContentStructuredCollectionOperationResult add = structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.AddRow(CopyFields(baseline.Rows[0])));
                Assert.That(add.Succeeded, Is.True, add.Message);
                Assert.That(add.RowKey, Is.Not.Null);
                Assert.That(EffectiveEffects(session).Rows.Last().IsAdded, Is.True);
                Assert.That(EffectiveEffects(session).Rows.Last().RowKey, Is.EqualTo(add.RowKey));

                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.ReplaceRowField(
                        add.RowKey,
                        "effect",
                        GameContentFieldValue.FromEnum("survivors.area.radius"))).Succeeded, Is.True);
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.ReplaceRowField(
                        add.RowKey,
                        "target",
                        GameContentFieldValue.FromEnum("survivors.area"))).Succeeded, Is.True);
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.ReplaceRowField(
                        add.RowKey,
                        "amount",
                        GameContentFieldValue.FromNumber(0.75d))).Succeeded, Is.True);
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.MoveRow(secondKey, 0)).Succeeded, Is.True);
                Assert.That(EffectiveEffects(session).Rows[0].RowKey, Is.EqualTo(secondKey));
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.RestoreOriginalOrder()).Succeeded, Is.True);
                Assert.That(EffectiveEffects(session).Rows.Take(2).Select(row => row.RowKey),
                    Is.EqualTo(new[] { firstKey, secondKey }));

                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.RemoveRow(add.RowKey)).Succeeded, Is.True);
                Assert.That(EffectiveEffects(session).Count, Is.EqualTo(2));
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.RemoveRow(GameContentStructuredRowKey.CreateSessionKey())).Succeeded,
                    Is.False);

                double originalAmount = baseline.Rows[0].FieldValues.Single(value => value.FieldId == "amount").Value.NumberValue;
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.ReplaceRowField(
                        firstKey,
                        "amount",
                        GameContentFieldValue.FromNumber(originalAmount + 0.05d))).Succeeded, Is.True);
                Assert.That(session.CanUndo, Is.True);
                Assert.That(session.Undo().Succeeded, Is.True);
                Assert.That(EffectiveEffects(session).Rows[0].FieldValues.Single(value => value.FieldId == "amount").Value.NumberValue,
                    Is.EqualTo(originalAmount));
                Assert.That(session.Redo().Succeeded, Is.True);
                Assert.That(EffectiveEffects(session).Rows[0].RowKey, Is.EqualTo(firstKey));
                Assert.That(session.Apply("effects", session.Changes.Single().ProposedValue).Succeeded, Is.False);

                GameContentReferenceEvaluation noReference = structured.EvaluateStructuredRowReference(
                    "effects",
                    firstKey,
                    "target",
                    other.Record.CanonicalKey);
                Assert.That(noReference.IsValid, Is.False);
                Assert.That(noReference.Reason, Does.Contain("closed gameplay target tokens"));
                Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(before));
                Assert.That(File.ReadAllBytes(other.Source.SourcePath.FullPath), Is.EqualTo(otherBefore));
                Assert.That(SurvivorsContentPackIndex.ValidateSelectedSources(other.Manifest).ErrorCount, Is.Zero);
                Assert.That(session.Rollback().Succeeded, Is.True);
            }
            Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(before));
            Assert.That(File.ReadAllBytes(other.Source.SourcePath.FullPath), Is.EqualTo(otherBefore));
        }

        [Test]
        public void EvolutionSession_MixesCanonicalReferenceAndStructuredHistoryWithoutLosingRowKeys()
        {
            Fixture fixture = ResolveEditable("basic-survivors", "upgrade.survivors.evolution.arcane-storm");
            byte[] before = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
            GameContentRecordDescriptor target = fixture.Index.Records.Single(record =>
                record.SourceRecordId == "upgrade.survivors.swift-steps");
            using (IGameContentEditSession session = fixture.Provider.BeginEdit(fixture.Request))
            {
                GameContentOrderedStructuredCollectionValue baseline = EffectiveEffects(session);
                GameContentStructuredRowValue first = baseline.Rows[0];
                double originalAmount = RowAmount(first);
                GameContentRecordReferenceValue originalReference = session.Snapshot
                    .FieldValues["requiredPassiveUpgradeId"]
                    .RecordReferenceValue;
                var replacement = GameContentRecordReferenceValue.Resolved(
                    target.CanonicalKey,
                    target.DisplayName,
                    SurvivorsContentPackIndex.UpgradesSourceId);

                Assert.That(session.Apply(
                    "requiredPassiveUpgradeId",
                    GameContentFieldValue.FromRecordReference(replacement)).Succeeded, Is.True);
                Assert.That(Structured(session).ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.ReplaceRowField(
                        first.RowKey,
                        "amount",
                        GameContentFieldValue.FromNumber(originalAmount + 1d))).Succeeded, Is.True);
                Assert.That(EffectiveEffects(session).Rows[0].RowKey, Is.EqualTo(first.RowKey));
                Assert.That(session.Undo().Succeeded, Is.True);
                Assert.That(RowAmount(EffectiveEffects(session).Rows[0]), Is.EqualTo(originalAmount));
                Assert.That(session.Undo().Succeeded, Is.True);
                Assert.That(session.Changes, Is.Empty);
                Assert.That(session.Redo().Succeeded, Is.True);
                Assert.That(session.Changes.Single().ProposedValue.RecordReferenceValue.TargetKey,
                    Is.EqualTo(target.CanonicalKey));
                Assert.That(session.Redo().Succeeded, Is.True);
                Assert.That(EffectiveEffects(session).Rows[0].RowKey, Is.EqualTo(first.RowKey));
                Assert.That(RowAmount(EffectiveEffects(session).Rows[0]), Is.EqualTo(originalAmount + 1d));
                Assert.That(originalReference.TargetKey, Is.Not.EqualTo(target.CanonicalKey));
                Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(before));
                Assert.That(session.Rollback().Succeeded, Is.True);
            }
            Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(before));
        }

        [Test]
        public void RemovingAllEffectRows_UsesAuthoritativeZeroMinimumAndStillPassesStrictPreview()
        {
            Fixture fixture = ResolveEditable("basic-survivors", "upgrade.survivors.keen-edge");
            byte[] before = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
            using (IGameContentEditSession session = fixture.Provider.BeginEdit(fixture.Request))
            {
                IGameContentStructuredCollectionEditSession structured = Structured(session);
                GameContentStructuredRowKey[] keys = EffectiveEffects(session).Rows.Select(row => row.RowKey).ToArray();
                foreach (GameContentStructuredRowKey key in keys)
                {
                    GameContentStructuredCollectionOperationResult remove = structured.ApplyStructuredOperation(
                        "effects",
                        GameContentStructuredCollectionOperation.RemoveRow(key));
                    Assert.That(remove.Succeeded, Is.True, remove.Message);
                }
                Assert.That(EffectiveEffects(session).Count, Is.Zero);
                GameContentValidationPreview preview = session.Preview();
                Assert.That(preview.CanCommit, Is.True, FormatPreview(preview));
                Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(before));
                Assert.That(session.Rollback().Succeeded, Is.True);
            }
        }

        [Test]
        public void StructuredSession_UsesPhysicalSourceLockAndReleasesItAfterCancel()
        {
            Fixture firstFixture = ResolveEditable("basic-survivors", "upgrade.survivors.keen-edge");
            Fixture secondFixture = ResolveEditable("basic-survivors", "upgrade.survivors.soulflare-glyph");
            var coordinator = new GameContentEditSessionCoordinator();
            try
            {
                GameContentPackContext context = Context(firstFixture.Pack.StableKey);
                GameContentRecordDescriptor first = context.ResolveRecord(firstFixture.Record.CanonicalKey);
                GameContentRecordDescriptor second = context.ResolveRecord(secondFixture.Record.CanonicalKey);
                GameContentEditBeginResult opened = coordinator.BeginEdit(context, first, "first-effects");
                Assert.That(opened.Succeeded, Is.True, opened.Message);
                GameContentEditBeginResult blocked = coordinator.BeginEdit(context, second, "second-effects");
                Assert.That(blocked.Succeeded, Is.False);
                Assert.That(blocked.Message, Does.Contain("source"));
                Assert.That(coordinator.Cancel(opened.Session).Succeeded, Is.True);

                GameContentEditBeginResult reopened = coordinator.BeginEdit(context, second, "second-effects");
                Assert.That(reopened.Succeeded, Is.True, reopened.Message);
                Assert.That(coordinator.Cancel(reopened.Session).Succeeded, Is.True);
            }
            finally
            {
                coordinator.Dispose();
            }
        }

        [Test]
        public void StructuredSession_ExternalRefreshBecomesStaleAndCannotOverwriteSource()
        {
            Fixture fixture = ResolveEditable("basic-survivors", "upgrade.survivors.keen-edge");
            byte[] original = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
            IGameContentEditSession session = fixture.Provider.BeginEdit(fixture.Request);
            try
            {
                GameContentStructuredRowValue first = EffectiveEffects(session).Rows[0];
                double amount = first.FieldValues.Single(value => value.FieldId == "amount").Value.NumberValue;
                Assert.That(Structured(session).ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.ReplaceRowField(
                        first.RowKey,
                        "amount",
                        GameContentFieldValue.FromNumber(amount + 0.05d))).Succeeded, Is.True);

                byte[] external = original.Concat(new[] { (byte)' ' }).ToArray();
                File.WriteAllBytes(fixture.Source.SourcePath.FullPath, external);
                AssetDatabase.ImportAsset(
                    fixture.Source.SourcePath.AssetPath,
                    ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

                Assert.That(session.CheckStale().IsStale, Is.True);
                Assert.That(session.Preview().CanCommit, Is.False);
                Assert.That(session.Commit(true).Succeeded, Is.False);
                Assert.That(session.State, Is.EqualTo(GameContentEditSessionState.Stale));
                Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(external));
            }
            finally
            {
                session.Dispose();
                RestoreExact(fixture, original);
            }
        }

        [TestCase("basic-survivors", "neon-arcana")]
        [TestCase("neon-arcana", "basic-survivors")]
        public void Commit_RuntimeConsumesMovedEditedEffects_ThenRollbackRestoresExactPackBytes(
            string editedPackId,
            string otherPackId)
        {
            Fixture edited = ResolveEditable(editedPackId, "upgrade.survivors.keen-edge");
            Fixture other = ResolveEditable(otherPackId, "upgrade.survivors.keen-edge");
            byte[] original = File.ReadAllBytes(edited.Source.SourcePath.FullPath);
            byte[] otherOriginal = File.ReadAllBytes(other.Source.SourcePath.FullPath);
            int originalUpgradeCount = edited.Index.Records.Count(record =>
                record.CanonicalKey.SourceId == SurvivorsContentPackIndex.UpgradesSourceId &&
                record.HasCapability(GameContentRecordCapabilities.Upgrade));
            SurvivorsJsonStructuredCollectionToken originalToken =
                edited.Source.Definition.StructuredCollectionTokens["effects"];
            SurvivorsTemplateController controller = null;
            IGameContentEditSession session = edited.Provider.BeginEdit(edited.Request);
            try
            {
                IGameContentStructuredCollectionEditSession structured = Structured(session);
                GameContentOrderedStructuredCollectionValue baseline = EffectiveEffects(session);
                GameContentStructuredRowValue criticalChance = baseline.Rows.Single(row =>
                    row.FieldValues.Single(value => value.FieldId == "effect").Value.StringValue ==
                    BasicSurvivorsGame.CriticalChanceEffect.Value);
                GameContentStructuredRowValue criticalDamage = baseline.Rows.Single(row =>
                    row.RowKey != criticalChance.RowKey);
                double originalAmount = criticalChance.FieldValues.Single(value => value.FieldId == "amount").Value.NumberValue;
                double proposedAmount = originalAmount + 0.05d;

                GameContentStructuredCollectionOperationResult temporary = structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.AddRow(Fields(
                        "survivors.area.radius",
                        "survivors.area",
                        0.2d)));
                Assert.That(temporary.Succeeded, Is.True, temporary.Message);
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.RemoveRow(temporary.RowKey)).Succeeded, Is.True);
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.ReplaceRowField(
                        criticalChance.RowKey,
                        "amount",
                        GameContentFieldValue.FromNumber(proposedAmount))).Succeeded, Is.True);
                Assert.That(structured.ApplyStructuredOperation(
                    "effects",
                    GameContentStructuredCollectionOperation.MoveRow(criticalDamage.RowKey, 0)).Succeeded, Is.True);

                GameContentOrderedStructuredCollectionValue expected = EffectiveEffects(session);
                Assert.That(expected.Rows.Select(row => row.RowKey),
                    Is.EqualTo(new[] { criticalDamage.RowKey, criticalChance.RowKey }));
                GameContentValidationPreview preview = session.Preview();
                Assert.That(preview.CanCommit, Is.True, FormatPreview(preview));
                GameContentCommitResult commit = session.Commit(true);
                Assert.That(commit.Succeeded, Is.True, commit.Message);
                Assert.That(commit.RequiresRefresh, Is.True);
                Assert.That(commit.RequiresRebind, Is.True);
                Assert.That(File.ReadAllBytes(other.Source.SourcePath.FullPath), Is.EqualTo(otherOriginal));
                Assert.That(SurvivorsContentPackIndex.ValidateSelectedSources(edited.Manifest).ErrorCount, Is.Zero);
                Assert.That(SurvivorsContentPackIndex.ValidateSelectedSources(other.Manifest).ErrorCount, Is.Zero);

                byte[] committedBytes = File.ReadAllBytes(edited.Source.SourcePath.FullPath);
                Assert.That(SurvivorsLosslessJsonDocument.TryParse(
                    committedBytes,
                    out SurvivorsLosslessJsonDocument committedDocument,
                    out string parseError), Is.True, parseError);
                Assert.That(SurvivorsJsonRecordNavigator.TryLocateRecord(
                    committedDocument,
                    edited.Source.Definition.Locator,
                    out SurvivorsJsonNode committedRecord,
                    out string locateError), Is.True, locateError);
                Assert.That(SurvivorsJsonRecordNavigator.TryReadDirectStructuredCollection(
                    committedDocument,
                    committedRecord,
                    EffectsField(session),
                    true,
                    out SurvivorsJsonStructuredCollectionToken committedToken,
                    out string tokenError), Is.True, tokenError);
                Assert.That(committedDocument.Text.Substring(0, committedToken.Node.Start),
                    Is.EqualTo(edited.Source.Document.Text.Substring(0, originalToken.Node.Start)));
                Assert.That(committedDocument.Text.Substring(committedToken.Node.End),
                    Is.EqualTo(edited.Source.Document.Text.Substring(originalToken.Node.End)));
                Assert.That(committedToken.Value.OrderedStructuredCollectionValue.Rows.Select(RowEffect),
                    Is.EqualTo(expected.Rows.Select(RowEffect)));
                Assert.That(committedToken.Value.OrderedStructuredCollectionValue.Rows.Select(RowAmount),
                    Is.EqualTo(expected.Rows.Select(RowAmount)));

                Assert.That(TryBindStrict(
                    edited.Manifest,
                    out SurvivorsAuthoredContentDefinition authored,
                    out string bindError), Is.True, bindError);
                RunUpgradeDefinition upgrade = authored.RunUpgradeCatalog.Definitions.Single(definition =>
                    definition.Id.Value == edited.Record.SourceRecordId);
                Assert.That(upgrade.Effects.Select(effect => effect.EffectId.Value),
                    Is.EqualTo(expected.Rows.Select(RowEffect)));
                Assert.That(upgrade.Effects.Select(effect => effect.TargetId.Value),
                    Is.EqualTo(expected.Rows.Select(RowTarget)));
                Assert.That(upgrade.Effects.Select(effect => effect.Amount),
                    Is.EqualTo(expected.Rows.Select(RowAmount)));
                Assert.That(authored.IsStrictSample, Is.True);
                Assert.That(authored.UsesBuiltInFallbacks, Is.False);

                var root = new GameObject("Survivors Effect Row Runtime Proof");
                controller = root.AddComponent<SurvivorsTemplateController>();
                Assert.That(ConfigureStrict(controller, edited.Manifest), Is.True, controller.AuthoredContentStatus);
                Assert.That(controller.IsStrictAuthoredSample, Is.True);
                Assert.That(controller.IsFallbackContentActive, Is.False);
                Assert.That(controller.ApplyUpgradeByIdForTest(edited.Record.SourceRecordId), Is.True);
                Assert.That(controller.CriticalChanceBonus, Is.EqualTo((float)proposedAmount).Within(0.0001f));

                GameContentRollbackResult rollback = session.Rollback();
                Assert.That(rollback.Succeeded, Is.True, rollback.Message);
                Assert.That(File.ReadAllBytes(edited.Source.SourcePath.FullPath), Is.EqualTo(original));
                Assert.That(File.ReadAllBytes(other.Source.SourcePath.FullPath), Is.EqualTo(otherOriginal));
                SurvivorsContentPackIndex restored = SurvivorsContentPackIndex.Build(edited.Manifest);
                Assert.That(restored.Records.Count(record =>
                    record.CanonicalKey.SourceId == SurvivorsContentPackIndex.UpgradesSourceId &&
                    record.HasCapability(GameContentRecordCapabilities.Upgrade)), Is.EqualTo(originalUpgradeCount));
                Assert.That(SurvivorsContentPackIndex.ValidateSelectedSources(edited.Manifest).ErrorCount, Is.Zero);
            }
            finally
            {
                if (controller != null) UnityEngine.Object.DestroyImmediate(controller.gameObject);
                session.Dispose();
                RestoreExact(edited, original);
                RestoreExact(other, otherOriginal);
            }
        }

        private static Fixture ResolveEditable(string packId, string recordId)
        {
            Fixture fixture = ResolveRecord(packId, recordId);
            Assert.That(SurvivorsEditableSource.TryCreate(
                fixture.Manifest,
                fixture.Record,
                true,
                out SurvivorsEditableSource source,
                out string error), Is.True, error);
            fixture.Source = source;
            return fixture;
        }

        private static Fixture ResolveRecord(string packId, string recordId)
        {
            SurvivorsContentPackProvider provider = SurvivorsContentPackProvider.Instance;
            GameContentPackDescriptor pack = provider.GetContentPacks().Single(candidate => candidate.PackId == packId);
            SurvivorsContentPackIndex index = SurvivorsContentPackIndex.Build(pack.Manifest);
            Assert.That(index.Validation.ErrorCount, Is.Zero, FormatIssues(index.Validation));
            GameContentRecordDescriptor record = index.Records.Single(candidate => candidate.SourceRecordId == recordId);
            return new Fixture
            {
                Provider = provider,
                Pack = pack,
                Manifest = pack.Manifest,
                Index = index,
                Record = record,
                Request = new GameContentEditRequest(pack.StableKey, record.CanonicalKey, provider.ProviderId)
            };
        }

        private static GameContentPackContext Context(string packStableKey)
        {
            SurvivorsContentPackProvider provider = SurvivorsContentPackProvider.Instance;
            GameContentPackCatalog catalog = GameContentPackCatalog.Build(new[] { provider });
            return new GameContentPackSelectionState().Select(catalog, packStableKey);
        }

        private static IGameContentStructuredCollectionEditSession Structured(IGameContentEditSession session)
        {
            Assert.That(session, Is.InstanceOf<IGameContentStructuredCollectionEditSession>());
            return (IGameContentStructuredCollectionEditSession)session;
        }

        private static GameContentFieldDescriptor EffectsField(IGameContentEditSession session)
        {
            return session.Fields.Single(field => field.FieldId == SurvivorsUpgradeEffectEditing.CollectionFieldId);
        }

        private static GameContentOrderedStructuredCollectionValue EffectiveEffects(IGameContentEditSession session)
        {
            GameContentProposedChange change = session.Changes.LastOrDefault(candidate =>
                candidate.FieldId == SurvivorsUpgradeEffectEditing.CollectionFieldId);
            return (change == null
                    ? session.Snapshot.FieldValues[SurvivorsUpgradeEffectEditing.CollectionFieldId]
                    : change.ProposedValue)
                .OrderedStructuredCollectionValue;
        }

        private static IReadOnlyList<GameContentStructuredRowFieldValue> Fields(
            string effect,
            string target,
            double amount)
        {
            return new[]
            {
                new GameContentStructuredRowFieldValue("effect", GameContentFieldValue.FromEnum(effect)),
                new GameContentStructuredRowFieldValue("target", GameContentFieldValue.FromEnum(target)),
                new GameContentStructuredRowFieldValue("amount", GameContentFieldValue.FromNumber(amount))
            };
        }

        private static IReadOnlyList<GameContentStructuredRowFieldValue> CopyFields(
            GameContentStructuredRowValue row)
        {
            return row.FieldValues.Select(value =>
                new GameContentStructuredRowFieldValue(value.FieldId, value.Value)).ToArray();
        }

        private static string RowEffect(GameContentStructuredRowValue row)
        {
            return row.FieldValues.Single(value => value.FieldId == "effect").Value.StringValue;
        }

        private static string RowTarget(GameContentStructuredRowValue row)
        {
            return row.FieldValues.Single(value => value.FieldId == "target").Value.StringValue;
        }

        private static double RowAmount(GameContentStructuredRowValue row)
        {
            return row.FieldValues.Single(value => value.FieldId == "amount").Value.NumberValue;
        }

        private static bool TryBindStrict(
            GameContentPackManifest manifest,
            out SurvivorsAuthoredContentDefinition definition,
            out string error)
        {
            return SurvivorsAuthoredContentDefinition.TryCreate(
                SourceText(manifest, SurvivorsContentPackIndex.WeaponsSourceId),
                SourceText(manifest, SurvivorsContentPackIndex.UpgradesSourceId),
                SourceText(manifest, SurvivorsContentPackIndex.RelicsSourceId),
                SourceText(manifest, SurvivorsContentPackIndex.ClassesSourceId),
                SourceText(manifest, SurvivorsContentPackIndex.ProgressionSourceId),
                SourceText(manifest, SurvivorsContentPackIndex.EnemiesSourceId),
                SourceText(manifest, SurvivorsContentPackIndex.RunFlowSourceId),
                SourceText(manifest, SurvivorsContentPackIndex.RewardsSourceId),
                SurvivorsAuthoredContentBindingPolicy.StrictSample,
                out definition,
                out error);
        }

        private static string SourceText(GameContentPackManifest manifest, string sourceId)
        {
            Assert.That(manifest.TryGetSource(sourceId, out GameContentPackSourceReference source), Is.True, sourceId);
            Assert.That(source.TextAsset, Is.Not.Null, sourceId);
            return source.TextAsset.text;
        }

        private static TextAsset SourceAsset(GameContentPackManifest manifest, string sourceId)
        {
            Assert.That(manifest.TryGetSource(sourceId, out GameContentPackSourceReference source), Is.True, sourceId);
            Assert.That(source.TextAsset, Is.Not.Null, sourceId);
            return source.TextAsset;
        }

        private static bool ConfigureStrict(SurvivorsTemplateController controller, GameContentPackManifest manifest)
        {
            return controller.ConfigureStrictSampleContent(
                SourceAsset(manifest, SurvivorsContentPackIndex.WeaponsSourceId),
                SourceAsset(manifest, SurvivorsContentPackIndex.UpgradesSourceId),
                SourceAsset(manifest, SurvivorsContentPackIndex.RelicsSourceId),
                SourceAsset(manifest, SurvivorsContentPackIndex.ClassesSourceId),
                SourceAsset(manifest, SurvivorsContentPackIndex.ProgressionSourceId),
                SourceAsset(manifest, SurvivorsContentPackIndex.EnemiesSourceId),
                SourceAsset(manifest, SurvivorsContentPackIndex.PickupsSourceId),
                SourceAsset(manifest, SurvivorsContentPackIndex.RunFlowSourceId),
                SourceAsset(manifest, SurvivorsContentPackIndex.RewardsSourceId),
                SourceAsset(manifest, SurvivorsContentPackIndex.PrimaryThemeSourceId),
                SourceAsset(manifest, SurvivorsContentPackIndex.AlternateThemeSourceId));
        }

        private static void RestoreExact(Fixture fixture, byte[] bytes)
        {
            if (fixture?.Source == null || bytes == null || !File.Exists(fixture.Source.SourcePath.FullPath)) return;
            if (File.ReadAllBytes(fixture.Source.SourcePath.FullPath).SequenceEqual(bytes)) return;
            FileAttributes attributes = File.GetAttributes(fixture.Source.SourcePath.FullPath);
            if ((attributes & FileAttributes.ReadOnly) != 0)
                File.SetAttributes(fixture.Source.SourcePath.FullPath, attributes & ~FileAttributes.ReadOnly);
            File.WriteAllBytes(fixture.Source.SourcePath.FullPath, bytes);
            AssetDatabase.ImportAsset(
                fixture.Source.SourcePath.AssetPath,
                ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            fixture.Provider.GetContentPacks();
        }

        private static string FormatIssues(GameContentAuthoringValidationResult validation)
        {
            return validation == null
                ? "No validation result."
                : string.Join(Environment.NewLine, validation.Issues.Select(issue => issue.Path + ": " + issue.Message));
        }

        private static string FormatPreview(GameContentValidationPreview preview)
        {
            return preview == null
                ? "No preview."
                : string.Join(Environment.NewLine, preview.Issues.Select(issue => issue.Path + ": " + issue.Message));
        }
    }
}
