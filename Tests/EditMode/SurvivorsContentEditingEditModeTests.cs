using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.TemplateGameSurvivors.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsContentEditingEditModeTests
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

        [Test]
        public void Provider_EnablesOnlyImportedBasicAndNeonExistingRecordEditing()
        {
            SurvivorsContentPackProvider provider = SurvivorsContentPackProvider.Instance;
            GameContentPackDescriptor[] packs = provider.GetContentPacks().ToArray();

            Assert.That(provider, Is.InstanceOf<IGameContentPackEditProvider>());
            Assert.That(packs.Select(pack => pack.PackId), Is.EquivalentTo(new[] { "basic-survivors", "neon-arcana" }));
            Assert.That(packs.All(pack => pack.SourceKind == GameContentPackSourceKind.ImportedSample), Is.True);
            Assert.That(packs.All(pack => pack.Access.CanEditExisting), Is.True);
            Assert.That(packs.All(pack => !pack.Access.CanCreate && !pack.Access.CanDuplicate && !pack.Access.CanDelete && !pack.Access.CanClonePack), Is.True);

            GameContentPackDescriptor missing = SurvivorsContentPackProvider.CreateSampleNotImportedDescriptor();
            Assert.That(missing.Access.CanEditExisting, Is.False);
        }

        [Test]
        public void Provider_ExposesApprovedFieldsAndKeepsIdentityReferencesAndRolesReadOnly()
        {
            AssertFields(
                Resolve("basic-survivors", "weapon.survivors.arcane-wand"),
                new[] { "displayName", "cooldownSeconds", "damage", "range", "projectileRadius" },
                new[] { "id", "fireMode", "projectileId" });
            AssertFields(
                Resolve("basic-survivors", "projectile.survivors.arcane-bolt"),
                new[] { "speed", "lifetimeSeconds" },
                new[] { "id" });
            AssertFields(
                Resolve("neon-arcana", "enemy.survivors.swarm"),
                new[] { "displayName", "health", "contactDamage", "contactIntervalSeconds", "moveSpeed", "radius", "experienceDrop" },
                new[] { "id", "role" });
        }

        [TestCase("basic-survivors")]
        [TestCase("neon-arcana")]
        public void Provider_ExposesOnlyEvolutionPassiveReferenceWithCanonicalConstraints(string packId)
        {
            Fixture fixture = Resolve(packId, "upgrade.survivors.evolution.arcane-storm");
            using (IGameContentEditSession session = fixture.Provider.BeginEdit(fixture.Request))
            {
                Assert.That(session, Is.InstanceOf<IGameContentRecordReferenceEditSession>());
                Assert.That(session.Fields.Where(field => !field.IsReadOnly).Select(field => field.FieldId),
                    Is.EqualTo(new[] { "requiredPassiveUpgradeId" }));
                Assert.That(session.Fields.Where(field => field.IsReadOnly).Select(field => field.FieldId),
                    Is.EqualTo(new[] { "id" }));

                GameContentFieldDescriptor field = session.Fields.Single(candidate => !candidate.IsReadOnly);
                Assert.That(field.FieldType, Is.EqualTo(GameContentFieldType.RecordReference));
                Assert.That(field.Required, Is.True);
                Assert.That(field.RecordReference.AllowClear, Is.False);
                Assert.That(field.RecordReference.PackPolicy, Is.EqualTo(GameContentReferencePackPolicy.SameSelectedPack));
                Assert.That(field.RecordReference.RequiredCapabilities,
                    Is.EqualTo(new[] { GameContentRecordCapabilities.Upgrade, GameContentRecordCapabilities.Passive }));
                Assert.That(field.RecordReference.RuntimeImpact,
                    Is.EqualTo(GameContentReferenceRuntimeImpact.Refresh | GameContentReferenceRuntimeImpact.Rebind));

                GameContentRecordReferenceValue current = session.Snapshot
                    .FieldValues["requiredPassiveUpgradeId"]
                    .RecordReferenceValue;
                Assert.That(current.IsResolved, Is.True);
                Assert.That(current.TargetKey.SourceRecordId, Is.EqualTo("upgrade.survivors.arcane-thesis"));
                Assert.That(current.TargetKey.PackId, Is.EqualTo(packId));
                Assert.That(session.Rollback().Succeeded, Is.True);
            }
        }

        [Test]
        public void ReferenceCandidates_AreSamePackPassiveOnlyAndRejectCraftedTargetsAndNone()
        {
            Fixture fixture = Resolve("basic-survivors", "upgrade.survivors.evolution.arcane-storm");
            Fixture neon = Resolve("neon-arcana", "upgrade.survivors.swift-steps");
            var coordinator = new GameContentEditSessionCoordinator();
            try
            {
                GameContentPackContext context = Context(fixture.Pack.StableKey);
                GameContentRecordDescriptor record = context.ResolveRecord(fixture.Record.CanonicalKey);
                GameContentEditBeginResult begin = coordinator.BeginEdit(context, record, "upgrade-lens");
                Assert.That(begin.Succeeded, Is.True, begin.Message);

                GameContentReferenceCandidateSet candidates = coordinator.GetReferenceCandidates(
                    begin.Session,
                    "requiredPassiveUpgradeId");
                Assert.That(candidates.Candidates, Is.Not.Empty);
                Assert.That(candidates.Candidates.All(candidate =>
                    candidate.Record.CanonicalKey.PackId == "basic-survivors" &&
                    candidate.Record.HasCapability(GameContentRecordCapabilities.Upgrade) &&
                    candidate.Record.HasCapability(GameContentRecordCapabilities.Passive)), Is.True);
                Assert.That(candidates.Candidates.Select(candidate => candidate.Record.SourceRecordId),
                    Does.Contain("upgrade.survivors.swift-steps"));

                GameContentRecordDescriptor nonPassive = fixture.Index.Records.Single(candidate =>
                    candidate.SourceRecordId == "upgrade.survivors.arcane-damage");
                GameContentReferenceEvaluation wrongCategory = coordinator.EvaluateReferenceTarget(
                    begin.Session,
                    "requiredPassiveUpgradeId",
                    nonPassive.CanonicalKey);
                GameContentReferenceEvaluation crossPack = coordinator.EvaluateReferenceTarget(
                    begin.Session,
                    "requiredPassiveUpgradeId",
                    neon.Record.CanonicalKey);
                var missingKey = new GameContentRecordKey(
                    fixture.Record.CanonicalKey.OwningPackageId,
                    fixture.Record.CanonicalKey.PackId,
                    "upgrade.survivors.missing-passive",
                    SurvivorsContentPackIndex.UpgradesSourceId);
                GameContentReferenceEvaluation missing = coordinator.EvaluateReferenceTarget(
                    begin.Session,
                    "requiredPassiveUpgradeId",
                    missingKey);
                GameContentEditOperationResult none = coordinator.Apply(
                    begin.Session,
                    "requiredPassiveUpgradeId",
                    GameContentFieldValue.FromRecordReference(GameContentRecordReferenceValue.None()));

                Assert.That(wrongCategory.IsValid, Is.False);
                Assert.That(wrongCategory.RequiredCapabilitiesSatisfied, Is.False);
                Assert.That(crossPack.IsValid, Is.False);
                Assert.That(crossPack.SamePackPolicySatisfied, Is.False);
                Assert.That(missing.IsValid, Is.False);
                Assert.That(missing.SourceClaimValid, Is.False);
                Assert.That(none.Succeeded, Is.False);
                Assert.That(begin.Session.State, Is.EqualTo(GameContentEditSessionState.Clean));
                Assert.That(coordinator.Cancel(begin.Session).Succeeded, Is.True);
            }
            finally
            {
                coordinator.Dispose();
            }
        }

        [Test]
        public void Provider_DisablesAllPacksUnsupportedCategoriesAndArbitrarySources()
        {
            Fixture upgrade = Resolve("basic-survivors", "upgrade.survivors.arcane-damage");
            GameContentEditAvailability unsupported = upgrade.Provider.CanEdit(upgrade.Request);
            Assert.That(unsupported.IsEditable, Is.False);
            Assert.That(unsupported.DisabledReason, Does.Contain("evolution prerequisite"));

            var allPacksRequest = new GameContentEditRequest(
                GameContentPackContext.AllPacksSelectionKey,
                upgrade.Record.CanonicalKey,
                SurvivorsContentPackProvider.StableProviderId);
            GameContentEditAvailability allPacks = upgrade.Provider.CanEdit(allPacksRequest);
            Assert.That(allPacks.IsEditable, Is.False);
            Assert.That(allPacks.DisabledReason, Does.Contain("All Packs"));

            Assert.That(SurvivorsEditableSource.CanAdvertiseEditing(
                upgrade.Manifest,
                GameContentPackSourceKind.Package,
                out string packageSourceReason), Is.False);
            Assert.That(packageSourceReason, Does.Contain("Package source"));

            TextAsset packageAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Packages/com.deucarian.template.game.survivors/package.json");
            Assert.That(packageAsset, Is.Not.Null);
            var packageSource = new GameContentPackSourceReference("weapons", "json", packageAsset, "Package source", "weapons");
            Assert.That(SurvivorsProjectOwnedSourcePolicy.TryResolve(
                upgrade.Manifest,
                packageSource,
                out _,
                out string packagePathError), Is.False);
            Assert.That(packagePathError, Does.Contain("outside the imported sample's declared Content root"));

            var transient = new TextAsset("{}");
            try
            {
                var arbitrary = new GameContentPackSourceReference("weapons", "json", transient, "Arbitrary", "weapons");
                Assert.That(SurvivorsProjectOwnedSourcePolicy.TryResolve(
                    upgrade.Manifest,
                    arbitrary,
                    out _,
                    out string pathError), Is.False);
                Assert.That(pathError, Does.Contain("safe project-relative asset paths"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(transient);
            }
        }

        [Test]
        public void Provider_DisablesReadOnlyImportedSourceWithoutChangingBytes()
        {
            Fixture fixture = Resolve("basic-survivors", "enemy.survivors.swarm");
            byte[] before = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
            FileAttributes attributes = File.GetAttributes(fixture.Source.SourcePath.FullPath);
            try
            {
                File.SetAttributes(fixture.Source.SourcePath.FullPath, attributes | FileAttributes.ReadOnly);
                GameContentEditAvailability availability = fixture.Provider.CanEdit(fixture.Request);
                Assert.That(availability.IsEditable, Is.False);
                Assert.That(availability.DisabledReason, Does.Contain("read-only"));
                Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(before));
            }
            finally
            {
                File.SetAttributes(fixture.Source.SourcePath.FullPath, attributes);
            }
        }

        [Test]
        public void Coordinator_SharesCanonicalSessionConflictsSecondRecordAndReleasesLockOnCancel()
        {
            Fixture fixture = Resolve("basic-survivors", "weapon.survivors.arcane-wand");
            byte[] before = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
            var coordinator = new GameContentEditSessionCoordinator();
            try
            {
                GameContentPackContext context = Context(fixture.Pack.StableKey);
                GameContentRecordDescriptor record = context.ResolveRecord(fixture.Record.CanonicalKey);
                GameContentEditBeginResult first = coordinator.BeginEdit(context, record, "weapon-lens");
                Assert.That(first.Succeeded, Is.True, first.Message);
                Assert.That(first.AttachedExisting, Is.False);

                double healthOrDamage = first.Session.GetEffectiveValue("damage").NumberValue;
                Assert.That(coordinator.Apply(first.Session, "damage", GameContentFieldValue.FromNumber(healthOrDamage + 0.5d)).Succeeded, Is.True);
                Assert.That(first.Session.State, Is.EqualTo(GameContentEditSessionState.Dirty));
                Assert.That(coordinator.Undo(first.Session).Succeeded, Is.True);
                Assert.That(first.Session.State, Is.EqualTo(GameContentEditSessionState.Clean));
                Assert.That(coordinator.Redo(first.Session).Succeeded, Is.True);
                Assert.That(coordinator.Preview(first.Session).CanCommit, Is.True);

                GameContentEditBeginResult attached = coordinator.BeginEdit(context, record, "attack-lens");
                Assert.That(attached.Succeeded, Is.True, attached.Message);
                Assert.That(attached.AttachedExisting, Is.True);
                Assert.That(attached.Session, Is.SameAs(first.Session));

                GameContentRecordDescriptor secondRecord = context.Records.First(candidate =>
                    candidate.CategoryId == "weapons" && !candidate.CanonicalKey.Equals(record.CanonicalKey));
                GameContentEditBeginResult conflict = coordinator.BeginEdit(context, secondRecord, "weapon-lens");
                Assert.That(conflict.Succeeded, Is.False);
                Assert.That(conflict.Message, Does.Contain("already being edited"));

                GameContentRollbackResult cancelled = coordinator.Cancel(first.Session);
                Assert.That(cancelled.Succeeded, Is.True, cancelled.Message);
                Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(before));
                Assert.That(coordinator.GetAvailability(context, secondRecord, "weapon-lens").IsEditable, Is.True);
            }
            finally
            {
                coordinator.Dispose();
                RestoreExact(fixture, before);
            }
        }

        [Test]
        public void Session_RejectsReadOnlyWrongTypeAndOutOfConstraintValues()
        {
            Fixture fixture = Resolve("basic-survivors", "enemy.survivors.swarm");
            using (IGameContentEditSession session = fixture.Provider.BeginEdit(fixture.Request))
            {
                Assert.That(session.Apply("id", GameContentFieldValue.FromString("changed-id")).Succeeded, Is.False);
                Assert.That(session.Apply("role", GameContentFieldValue.FromString("Boss")).Succeeded, Is.False);
                Assert.That(session.Apply("health", GameContentFieldValue.FromString("many")).Succeeded, Is.False);
                Assert.That(session.Apply("health", GameContentFieldValue.FromNumber(-1d)).Succeeded, Is.False);
                Assert.That(session.Apply("health", GameContentFieldValue.FromNumber(double.PositiveInfinity)).Succeeded, Is.False);
                Assert.That(session.State, Is.EqualTo(GameContentEditSessionState.Clean));
                Assert.That(session.Rollback().Succeeded, Is.True);
            }
        }

        [Test]
        public void ProposedPackValidation_BlocksInvalidScalarWhileLeavingDiskUntouched()
        {
            Fixture fixture = Resolve("basic-survivors", "enemy.survivors.swarm");
            byte[] before = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
            var changes = new Dictionary<string, GameContentFieldValue>(StringComparer.Ordinal)
            {
                ["health"] = GameContentFieldValue.FromNumber(-1d)
            };
            Assert.That(SurvivorsLosslessJsonPatcher.TryPatch(
                fixture.Source.Document,
                fixture.Source.Definition.Tokens,
                changes,
                out string proposed,
                out _,
                out string patchError), Is.True, patchError);

            GameContentAuthoringValidationResult validation = SurvivorsContentPackIndex.ValidateSelectedSources(
                fixture.Manifest,
                SurvivorsContentPackIndex.EnemiesSourceId,
                proposed);

            Assert.That(validation.ErrorCount, Is.GreaterThan(0));
            Assert.That(validation.Issues.Any(issue => issue.Message.Contains("health above zero")), Is.True);
            Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(before));
        }

        [Test]
        public void Session_DetectsExternalByteChangeAndStaleCommitCannotOverwriteIt()
        {
            Fixture fixture = Resolve("basic-survivors", "enemy.survivors.swarm");
            byte[] before = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
            var session = new SurvivorsContentEditSession(fixture.Source, () => fixture.Provider.GetContentPacks());
            try
            {
                double health = session.Snapshot.FieldValues["health"].NumberValue;
                Assert.That(session.Apply("health", GameContentFieldValue.FromNumber(health + 1d)).Succeeded, Is.True);
                byte[] external = before.Concat(new byte[] { (byte)' ' }).ToArray();
                File.WriteAllBytes(fixture.Source.SourcePath.FullPath, external);
                AssetDatabase.ImportAsset(fixture.Source.SourcePath.AssetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

                Assert.That(session.CheckStale().IsStale, Is.True);
                GameContentCommitResult commit = session.Commit(true);
                Assert.That(commit.Succeeded, Is.False);
                Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(external));
                Assert.That(session.Rollback().Succeeded, Is.True);
                Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(external));
            }
            finally
            {
                session.Dispose();
                RestoreExact(fixture, before);
            }
        }

        [Test]
        public void SourceRevision_ChangesWhenManifestSourceListChanges()
        {
            Fixture fixture = Resolve("basic-survivors", "enemy.survivors.swarm");
            GameContentPackSourceReference[] reversed = fixture.Manifest.ContentSources
                .Reverse()
                .Select(source => new GameContentPackSourceReference(
                    source.SourceId,
                    source.SourceKind,
                    source.TextAsset,
                    source.DisplayLabel,
                    source.CategoryHint,
                    source.Required))
                .ToArray();
            GameContentPackManifest clone = ScriptableObject.CreateInstance<GameContentPackManifest>();
            try
            {
                clone.Configure(
                    fixture.Manifest.PackId,
                    fixture.Manifest.OwningPackageId,
                    fixture.Manifest.ProviderId,
                    fixture.Manifest.DisplayName,
                    fixture.Manifest.Description,
                    fixture.Manifest.SchemaVersion,
                    fixture.Manifest.Tags,
                    fixture.Manifest.PlayableScene,
                    fixture.Manifest.DefaultTheme,
                    reversed,
                    fixture.Manifest.Preview,
                    fixture.Manifest.Icon);
                GameContentSourceRevision changed = SurvivorsContentSourceRevision.Create(
                    clone,
                    fixture.Record.CanonicalKey,
                    fixture.Source.SourcePath,
                    fixture.Source.ExactBytes);
                Assert.That(changed, Is.Not.EqualTo(fixture.Source.Revision));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(clone);
            }
        }

        [Test]
        public void AtomicWriteFailure_PreservesDestinationAndSessionCanCancelCleanly()
        {
            Fixture fixture = Resolve("basic-survivors", "enemy.survivors.swarm");
            byte[] before = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
            var hooks = new SurvivorsEditTransactionHooks
            {
                BeforeAtomicReplace = () => throw new IOException("simulated write failure")
            };
            var session = new SurvivorsContentEditSession(fixture.Source, () => fixture.Provider.GetContentPacks(), hooks);
            try
            {
                double health = session.Snapshot.FieldValues["health"].NumberValue;
                Assert.That(session.Apply("health", GameContentFieldValue.FromNumber(health + 1d)).Succeeded, Is.True);
                GameContentCommitResult result = session.Commit(true);
                Assert.That(result.Succeeded, Is.False);
                Assert.That(result.Recovery, Is.Null);
                Assert.That(session.State, Is.EqualTo(GameContentEditSessionState.Dirty));
                Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(before));
                Assert.That(session.Rollback().Succeeded, Is.True);
            }
            finally
            {
                session.Dispose();
                RestoreExact(fixture, before);
            }
        }

        [Test]
        public void ImportFailure_AutomaticallyRestoresAndVerifiesExactOriginalBytes()
        {
            Fixture fixture = Resolve("basic-survivors", "enemy.survivors.swarm");
            byte[] before = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
            int importAttempts = 0;
            var hooks = new SurvivorsEditTransactionHooks
            {
                BeforeImport = () =>
                {
                    if (importAttempts++ == 0) throw new IOException("simulated import failure");
                }
            };
            var session = new SurvivorsContentEditSession(fixture.Source, () => fixture.Provider.GetContentPacks(), hooks);
            try
            {
                double health = session.Snapshot.FieldValues["health"].NumberValue;
                Assert.That(session.Apply("health", GameContentFieldValue.FromNumber(health + 1d)).Succeeded, Is.True);
                GameContentCommitResult result = session.Commit(true);
                Assert.That(result.Succeeded, Is.False);
                Assert.That(result.Recovery, Is.Null);
                Assert.That(session.State, Is.EqualTo(GameContentEditSessionState.RolledBack));
                Assert.That(importAttempts, Is.EqualTo(2));
                Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(before));
            }
            finally
            {
                session.Dispose();
                RestoreExact(fixture, before);
            }
        }

        [Test]
        public void Rollback_RefusesToOverwriteAChangedCommittedSource()
        {
            Fixture fixture = Resolve("basic-survivors", "enemy.survivors.swarm");
            byte[] original = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
            IGameContentEditSession session = fixture.Provider.BeginEdit(fixture.Request);
            try
            {
                double health = session.Snapshot.FieldValues["health"].NumberValue;
                Assert.That(session.Apply("health", GameContentFieldValue.FromNumber(health + 1d)).Succeeded, Is.True);
                Assert.That(session.Commit(true).Succeeded, Is.True);

                byte[] committed = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
                byte[] external = committed.Concat(new byte[] { (byte)' ' }).ToArray();
                File.WriteAllBytes(fixture.Source.SourcePath.FullPath, external);
                AssetDatabase.ImportAsset(
                    fixture.Source.SourcePath.AssetPath,
                    ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

                GameContentRollbackResult rollback = session.Rollback();
                Assert.That(rollback.Succeeded, Is.False);
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
        public void Commit_RuntimeConsumesEditedEnemy_ThenRollbackRestoresExactBytesAndOtherPackIsUntouched(
            string editedPackId,
            string otherPackId)
        {
            Fixture edited = Resolve(editedPackId, "enemy.survivors.swarm");
            Fixture other = Resolve(otherPackId, "enemy.survivors.swarm");
            byte[] original = File.ReadAllBytes(edited.Source.SourcePath.FullPath);
            byte[] otherOriginal = File.ReadAllBytes(other.Source.SourcePath.FullPath);
            DateTime transactionStart = DateTime.UtcNow;
            IGameContentEditSession session = edited.Provider.BeginEdit(edited.Request);
            try
            {
                double baseline = session.Snapshot.FieldValues["health"].NumberValue;
                double proposed = baseline + 2.75d;
                Assert.That(session.Apply("health", GameContentFieldValue.FromNumber(proposed)).Succeeded, Is.True);
                Assert.That(session.Preview().CanCommit, Is.True);

                GameContentCommitResult committed = session.Commit(true);
                Assert.That(committed.Succeeded, Is.True, committed.Message);
                Assert.That(session.State, Is.EqualTo(GameContentEditSessionState.Committed));
                Assert.That(File.ReadAllBytes(edited.Source.SourcePath.FullPath), Is.Not.EqualTo(original));
                Assert.That(File.ReadAllBytes(other.Source.SourcePath.FullPath), Is.EqualTo(otherOriginal));
                Assert.That(SurvivorsContentPackIndex.ValidateSelectedSources(edited.Manifest).ErrorCount, Is.Zero);
                Assert.That(SurvivorsContentPackIndex.ValidateSelectedSources(other.Manifest).ErrorCount, Is.Zero);

                Assert.That(TryBindStrict(edited.Manifest, out SurvivorsAuthoredContentDefinition authored, out string bindError), Is.True, bindError);
                Assert.That(authored.IsStrictSample, Is.True);
                Assert.That(authored.UsesBuiltInFallbacks, Is.False);
                Assert.That(authored.TryGetEnemyProfile(SurvivorsEnemyRole.Swarm, out SurvivorsEnemyProfile swarm), Is.True);
                Assert.That(swarm.MaxHealth, Is.EqualTo((float)proposed).Within(0.001f));

                string sourceRecoveryDirectory = Path.Combine(
                    SurvivorsRecoveryStore.RootPath,
                    SurvivorsContentEditHash.Sha256(edited.Source.SourcePath.FullPath));
                Assert.That(Directory.Exists(sourceRecoveryDirectory), Is.True);
                Assert.That(Directory.GetFiles(sourceRecoveryDirectory, "*.backup")
                    .Any(path => File.GetLastWriteTimeUtc(path) >= transactionStart.AddSeconds(-1)), Is.True);

                GameContentRollbackResult rolledBack = session.Rollback();
                Assert.That(rolledBack.Succeeded, Is.True, rolledBack.Message);
                Assert.That(File.ReadAllBytes(edited.Source.SourcePath.FullPath), Is.EqualTo(original));
                Assert.That(File.ReadAllBytes(other.Source.SourcePath.FullPath), Is.EqualTo(otherOriginal));
                Assert.That(SurvivorsContentPackIndex.ValidateSelectedSources(edited.Manifest).ErrorCount, Is.Zero);
            }
            finally
            {
                session.Dispose();
                RestoreExact(edited, original);
                RestoreExact(other, otherOriginal);
            }
        }

        [Test]
        public void BasicAndNeonSameRecordIdResolveToDifferentPhysicalSourcesAndPackScopedKeys()
        {
            Fixture basic = Resolve("basic-survivors", "enemy.survivors.swarm");
            Fixture neon = Resolve("neon-arcana", "enemy.survivors.swarm");

            Assert.That(basic.Record.CanonicalKey, Is.Not.EqualTo(neon.Record.CanonicalKey));
            Assert.That(basic.Source.SourcePath.AssetGuid, Is.Not.EqualTo(neon.Source.SourcePath.AssetGuid));
            Assert.That(basic.Source.SourcePath.FullPath, Is.Not.EqualTo(neon.Source.SourcePath.FullPath));
            Assert.That(basic.Source.SourceTarget.LockKey, Is.Not.EqualTo(neon.Source.SourceTarget.LockKey));
        }

        [TestCase("basic-survivors", "neon-arcana")]
        [TestCase("neon-arcana", "basic-survivors")]
        public void Commit_EvolutionPassiveChangesOneStringTokenDrivesStrictRuntimeAndRollsBackExactly(
            string editedPackId,
            string otherPackId)
        {
            Fixture edited = Resolve(editedPackId, "upgrade.survivors.evolution.arcane-storm");
            Fixture other = Resolve(otherPackId, "upgrade.survivors.evolution.arcane-storm");
            GameContentRecordDescriptor target = edited.Index.Records.Single(candidate =>
                candidate.SourceRecordId == "upgrade.survivors.swift-steps");
            byte[] original = File.ReadAllBytes(edited.Source.SourcePath.FullPath);
            byte[] otherOriginal = File.ReadAllBytes(other.Source.SourcePath.FullPath);
            var storageChange = new Dictionary<string, GameContentFieldValue>(StringComparer.Ordinal)
            {
                ["requiredPassiveUpgradeId"] = GameContentFieldValue.FromString(target.SourceRecordId)
            };
            Assert.That(SurvivorsLosslessJsonPatcher.TryPatch(
                edited.Source.Document,
                edited.Source.Definition.Tokens,
                storageChange,
                out _,
                out byte[] expectedBytes,
                out string expectedError), Is.True, expectedError);

            IGameContentEditSession session = edited.Provider.BeginEdit(edited.Request);
            try
            {
                var reference = GameContentRecordReferenceValue.Resolved(
                    target.CanonicalKey,
                    target.DisplayName,
                    SurvivorsContentPackIndex.UpgradesSourceId);
                Assert.That(session.Apply(
                    "requiredPassiveUpgradeId",
                    GameContentFieldValue.FromRecordReference(reference)).Succeeded, Is.True);
                Assert.That(session.Preview().CanCommit, Is.True);

                GameContentCommitResult commit = session.Commit(true);
                Assert.That(commit.Succeeded, Is.True, commit.Message);
                Assert.That(commit.RequiresRefresh, Is.True);
                Assert.That(commit.RequiresRebind, Is.True);
                Assert.That(File.ReadAllBytes(edited.Source.SourcePath.FullPath), Is.EqualTo(expectedBytes));
                Assert.That(File.ReadAllBytes(other.Source.SourcePath.FullPath), Is.EqualTo(otherOriginal));
                Assert.That(SurvivorsContentPackIndex.ValidateSelectedSources(edited.Manifest).ErrorCount, Is.Zero);

                Assert.That(TryBindStrict(edited.Manifest, out SurvivorsAuthoredContentDefinition authored, out string bindError),
                    Is.True,
                    bindError);
                SurvivorsRunUpgradeMetadata evolution = authored.RunUpgradeMetadata.Single(candidate =>
                    candidate.UpgradeId == edited.Record.SourceRecordId);
                Assert.That(evolution.RequiredPassiveUpgradeId, Is.EqualTo(target.SourceRecordId));
                Assert.That(authored.IsStrictSample, Is.True);
                Assert.That(authored.UsesBuiltInFallbacks, Is.False);

                GameContentRollbackResult rollback = session.Rollback();
                Assert.That(rollback.Succeeded, Is.True, rollback.Message);
                Assert.That(File.ReadAllBytes(edited.Source.SourcePath.FullPath), Is.EqualTo(original));
                Assert.That(File.ReadAllBytes(other.Source.SourcePath.FullPath), Is.EqualTo(otherOriginal));
            }
            finally
            {
                session.Dispose();
                RestoreExact(edited, original);
                RestoreExact(other, otherOriginal);
            }
        }

        [Test]
        public void ProposedEvolutionReference_RejectsNonPassiveAndExternalTargetDisappearanceBlocksCommit()
        {
            Fixture fixture = Resolve("basic-survivors", "upgrade.survivors.evolution.arcane-storm");
            GameContentRecordDescriptor validTarget = fixture.Index.Records.Single(candidate =>
                candidate.SourceRecordId == "upgrade.survivors.swift-steps");
            byte[] original = File.ReadAllBytes(fixture.Source.SourcePath.FullPath);
            var nonPassiveChange = new Dictionary<string, GameContentFieldValue>(StringComparer.Ordinal)
            {
                ["requiredPassiveUpgradeId"] = GameContentFieldValue.FromString("upgrade.survivors.arcane-damage")
            };
            Assert.That(SurvivorsLosslessJsonPatcher.TryPatch(
                fixture.Source.Document,
                fixture.Source.Definition.Tokens,
                nonPassiveChange,
                out string invalidText,
                out _,
                out string patchError), Is.True, patchError);
            GameContentAuthoringValidationResult invalid = SurvivorsContentPackIndex.ValidateSelectedSources(
                fixture.Manifest,
                SurvivorsContentPackIndex.UpgradesSourceId,
                invalidText);
            Assert.That(invalid.ErrorCount, Is.GreaterThan(0));
            Assert.That(invalid.Issues.Any(issue => issue.Message.Contains("must reference a Passive upgrade record")), Is.True);

            IGameContentEditSession session = fixture.Provider.BeginEdit(fixture.Request);
            try
            {
                Assert.That(session.Apply(
                    "requiredPassiveUpgradeId",
                    GameContentFieldValue.FromRecordReference(GameContentRecordReferenceValue.Resolved(validTarget.CanonicalKey))).Succeeded,
                    Is.True);

                var targetLocator = new SurvivorsJsonRecordLocator(
                    SurvivorsContentPackIndex.UpgradesSourceId,
                    "upgrades",
                    validTarget.SourceRecordId,
                    "Passive");
                Assert.That(SurvivorsJsonRecordNavigator.TryLocateRecord(
                    fixture.Source.Document,
                    targetLocator,
                    out SurvivorsJsonNode targetNode,
                    out string locateError), Is.True, locateError);
                Assert.That(SurvivorsJsonRecordNavigator.TryReadDirectScalar(
                    fixture.Source.Document,
                    targetNode,
                    "id",
                    GameContentFieldType.String,
                    true,
                    out SurvivorsJsonScalarToken idToken,
                    out string tokenError), Is.True, tokenError);
                var idTokens = new Dictionary<string, SurvivorsJsonScalarToken>(StringComparer.Ordinal)
                {
                    ["id"] = idToken
                };
                var disappeared = new Dictionary<string, GameContentFieldValue>(StringComparer.Ordinal)
                {
                    ["id"] = GameContentFieldValue.FromString("upgrade.survivors.disappeared-passive")
                };
                Assert.That(SurvivorsLosslessJsonPatcher.TryPatch(
                    fixture.Source.Document,
                    idTokens,
                    disappeared,
                    out _,
                    out byte[] externalBytes,
                    out string disappearError), Is.True, disappearError);
                File.WriteAllBytes(fixture.Source.SourcePath.FullPath, externalBytes);
                AssetDatabase.ImportAsset(
                    fixture.Source.SourcePath.AssetPath,
                    ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

                GameContentCommitResult blocked = session.Commit(true);
                Assert.That(blocked.Succeeded, Is.False);
                Assert.That(session.State, Is.EqualTo(GameContentEditSessionState.Stale));
                Assert.That(File.ReadAllBytes(fixture.Source.SourcePath.FullPath), Is.EqualTo(externalBytes));
            }
            finally
            {
                session.Dispose();
                RestoreExact(fixture, original);
            }
        }

        private static void AssertFields(Fixture fixture, IEnumerable<string> editable, IEnumerable<string> readOnly)
        {
            using (IGameContentEditSession session = fixture.Provider.BeginEdit(fixture.Request))
            {
                Assert.That(session.Fields.Where(field => !field.IsReadOnly).Select(field => field.FieldId), Is.EquivalentTo(editable));
                Assert.That(session.Fields.Where(field => field.IsReadOnly).Select(field => field.FieldId), Is.EquivalentTo(readOnly));
                Assert.That(session.Fields.All(field => session.Snapshot.FieldValues.ContainsKey(field.FieldId)), Is.True);
                Assert.That(session.Rollback().Succeeded, Is.True);
            }
        }

        private static Fixture Resolve(string packId, string recordId)
        {
            SurvivorsContentPackProvider provider = SurvivorsContentPackProvider.Instance;
            GameContentPackDescriptor pack = provider.GetContentPacks().Single(candidate => candidate.PackId == packId);
            GameContentPackManifest manifest = pack.Manifest;
            SurvivorsContentPackIndex index = SurvivorsContentPackIndex.Build(manifest);
            Assert.That(index.Validation.ErrorCount, Is.Zero, FormatIssues(index.Validation));
            GameContentRecordDescriptor record = index.Records.Single(candidate => candidate.SourceRecordId == recordId);
            SurvivorsEditableSource source = null;
            bool supported = (record.CategoryId == "weapons" || record.CategoryId == "projectiles") &&
                             record.CanonicalKey.SourceId == SurvivorsContentPackIndex.WeaponsSourceId ||
                             record.CategoryId == "enemies" &&
                             record.CanonicalKey.SourceId == SurvivorsContentPackIndex.EnemiesSourceId ||
                             record.CategoryId == "evolutions" &&
                             record.CanonicalKey.SourceId == SurvivorsContentPackIndex.UpgradesSourceId;
            if (supported)
                Assert.That(SurvivorsEditableSource.TryCreate(manifest, record, true, out source, out string error), Is.True, error);
            return new Fixture
            {
                Provider = provider,
                Pack = pack,
                Manifest = manifest,
                Index = index,
                Record = record,
                Source = source,
                Request = new GameContentEditRequest(pack.StableKey, record.CanonicalKey, provider.ProviderId)
            };
        }

        private static GameContentPackContext Context(string packStableKey)
        {
            SurvivorsContentPackProvider provider = SurvivorsContentPackProvider.Instance;
            GameContentPackCatalog catalog = GameContentPackCatalog.Build(new[] { provider });
            return new GameContentPackSelectionState().Select(catalog, packStableKey);
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

        private static void RestoreExact(Fixture fixture, byte[] bytes)
        {
            if (fixture == null || bytes == null || !File.Exists(fixture.Source.SourcePath.FullPath)) return;
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
    }
}
