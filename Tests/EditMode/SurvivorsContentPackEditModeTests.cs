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
    public sealed class SurvivorsContentPackEditModeTests
    {
        private static readonly string[] ExpectedCategoryIds =
        {
            "weapons",
            "projectiles",
            "upgrades",
            "passives",
            "pickup-magnet",
            "mutations",
            "evolutions",
            "relics",
            "classes",
            "enemies",
            "elites",
            "minibosses",
            "bosses",
            "run-profiles",
            "waves-milestones",
            "rewards",
            "meta-upgrades",
            "progression",
            "themes",
            "audio-events",
            "tutorial"
        };

        [Test]
        public void Provider_RegistersExactlyOnceWithStableIdentity()
        {
            SurvivorsContentPackProvider.EnsureRegistered();
            SurvivorsContentPackProvider.EnsureRegistered();

            IGameContentAuthoringProvider[] matches = GameContentAuthoringProviderRegistry.Providers
                .Where(provider => string.Equals(
                    provider.ProviderId,
                    SurvivorsContentPackProvider.StableProviderId,
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();
            Assert.That(matches, Has.Length.EqualTo(1));
            Assert.That(matches[0], Is.SameAs(SurvivorsContentPackProvider.Instance));
            Assert.That(matches[0], Is.InstanceOf<IGameContentPackProvider>());
            Assert.That(matches[0], Is.InstanceOf<IGameContentAuthoringSurfaceProvider>());
        }

        [Test]
        public void PackageSample_ContainsExactlyTwoManifestAssets()
        {
            UnityEditor.PackageManager.PackageInfo package =
                UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(BasicSurvivorsGame).Assembly);
            Assert.IsNotNull(package);
            string contentPackRoot = Path.Combine(package.resolvedPath, "Samples~", "BasicSurvivorsGame", "ContentPacks");
            Assert.That(Directory.Exists(contentPackRoot), Is.True, contentPackRoot);

            string[] manifests = Directory.GetFiles(contentPackRoot, "*.asset", SearchOption.TopDirectoryOnly);
            Assert.That(manifests.Select(Path.GetFileName), Is.EquivalentTo(new[] { "BasicSurvivors.asset", "NeonArcana.asset" }));
        }

        [Test]
        public void ImportedManifests_DiscoverTwoValidIndependentPacks()
        {
            IReadOnlyList<GameContentPackManifestEntry> entries = GetImportedEntries();
            GameContentPackManifestEntry basic = entries.Single(entry => entry.Manifest.PackId == "basic-survivors");
            GameContentPackManifestEntry neon = entries.Single(entry => entry.Manifest.PackId == "neon-arcana");

            Assert.That(entries.Select(entry => entry.StableKey).Distinct(StringComparer.OrdinalIgnoreCase).Count(), Is.EqualTo(2));
            Assert.That(entries.All(entry => entry.SourceState == GameContentPackSourceState.Available), Is.True);
            Assert.That(entries.All(entry => entry.Validation.ErrorCount == 0), Is.True);
            Assert.That(basic.Manifest.DisplayName, Is.EqualTo("Basic Survivors"));
            Assert.That(neon.Manifest.DisplayName, Is.EqualTo("Neon Arcana"));
            Assert.That(basic.Manifest.OwningPackageId, Is.EqualTo(SurvivorsContentPackProvider.OwningPackageId));
            Assert.That(neon.Manifest.OwningPackageId, Is.EqualTo(SurvivorsContentPackProvider.OwningPackageId));
            Assert.That(basic.Manifest.PlayableScene, Is.Not.SameAs(neon.Manifest.PlayableScene));
            Assert.That(AssetDatabase.GetAssetPath(basic.Manifest.PlayableScene), Does.EndWith("/Scenes/BasicSurvivorsGame.unity"));
            Assert.That(AssetDatabase.GetAssetPath(neon.Manifest.PlayableScene), Does.EndWith("/Scenes/NeonArcana.unity"));
        }

        [Test]
        public void Manifests_ReferenceEveryRequiredSelectedSourceAndNeonOwnsItsGameplayTree()
        {
            IReadOnlyList<GameContentPackManifestEntry> entries = GetImportedEntries();
            foreach (GameContentPackManifestEntry entry in entries)
            {
                foreach (string sourceId in SurvivorsContentPackIndex.RequiredSourceIds)
                {
                    Assert.That(entry.Manifest.TryGetSource(sourceId, out GameContentPackSourceReference source), Is.True, entry.Manifest.PackId + ":" + sourceId);
                    Assert.IsNotNull(source.TextAsset, entry.Manifest.PackId + ":" + sourceId);
                }
            }

            GameContentPackManifest neon = entries.Single(entry => entry.Manifest.PackId == "neon-arcana").Manifest;
            string[] gameplaySources =
            {
                SurvivorsContentPackIndex.WeaponsSourceId,
                SurvivorsContentPackIndex.UpgradesSourceId,
                SurvivorsContentPackIndex.ProgressionSourceId,
                SurvivorsContentPackIndex.ClassesSourceId,
                SurvivorsContentPackIndex.EnemiesSourceId,
                SurvivorsContentPackIndex.RunFlowSourceId,
                SurvivorsContentPackIndex.RewardsSourceId,
                SurvivorsContentPackIndex.RelicsSourceId,
                SurvivorsContentPackIndex.PickupsSourceId
            };
            foreach (string sourceId in gameplaySources)
            {
                Assert.That(neon.TryGetSource(sourceId, out GameContentPackSourceReference source), Is.True);
                string path = AssetDatabase.GetAssetPath(source.TextAsset).Replace('\\', '/');
                Assert.That(path, Does.Contain("/Content/NeonArcana/"), sourceId + " unexpectedly points outside the Neon authored tree: " + path);
                Assert.That(path, Does.Not.Contain("/Content/Default"), sourceId);
            }
        }

        [Test]
        public void Index_EnumeratesAllSemanticCategoriesWithoutDuplicatingLogicalRecords()
        {
            foreach (GameContentPackManifestEntry entry in GetImportedEntries())
            {
                SurvivorsContentPackIndex index = SurvivorsContentPackIndex.Build(entry.Manifest);
                Assert.That(index.Validation.ErrorCount, Is.Zero, FormatIssues(index.Validation));
                Assert.That(index.Categories.Select(category => category.CategoryId), Is.EqualTo(ExpectedCategoryIds));
                Assert.That(index.Categories.All(category => category.RecordCount > 0), Is.True, entry.Manifest.PackId);
                Assert.That(index.Records.Select(record => record.PackScopedId).Distinct(StringComparer.OrdinalIgnoreCase).Count(), Is.EqualTo(index.Records.Count));
                Assert.That(index.Records.Any(record => record.IsInCategory("pickup-magnet") && record.IsInCategory("passives")), Is.True);
                Assert.That(index.Records.Any(record => record.IsInCategory("run-profiles") && record.IsInCategory("waves-milestones")), Is.True);
                Assert.That(index.Records.Count(record => record.IsInCategory("audio-events")), Is.GreaterThan(0));
                Assert.That(index.Records.Count(record => record.IsInCategory("tutorial")), Is.GreaterThan(0));
            }
        }

        [Test]
        public void Index_UsesPackScopedIdsAndResolvesInboundAndOutboundReferencesWithinPack()
        {
            foreach (GameContentPackManifestEntry entry in GetImportedEntries())
            {
                SurvivorsContentPackIndex index = SurvivorsContentPackIndex.Build(entry.Manifest);
                string prefix = entry.Manifest.PackId + "::";
                Assert.That(index.Records.All(record => record.PackScopedId.StartsWith(prefix, StringComparison.Ordinal)), Is.True);
                Assert.That(index.Records.SelectMany(record => record.OutboundReferences).Any(), Is.True);
                Assert.That(index.Records.SelectMany(record => record.InboundReferences).Any(), Is.True);
                foreach (GameContentRecordReferenceDescriptor reference in index.Records.SelectMany(record => record.OutboundReferences))
                {
                    Assert.That(reference.TargetPackId, Is.EqualTo(entry.Manifest.PackId));
                    Assert.That(reference.TargetRecordId, Does.StartWith(prefix));
                    Assert.That(reference.Valid, Is.True, reference.RelationshipLabel + " -> " + reference.TargetRecordId);
                    Assert.That(index.Records.Any(record => record.PackScopedId == reference.TargetRecordId), Is.True);
                }
            }
        }

        [Test]
        public void SelectedPackValidation_PassesIndependentlyAndProfilesRemainThirtyAndFiveMinutes()
        {
            foreach (GameContentPackManifestEntry entry in GetImportedEntries())
            {
                GameContentAuthoringValidationResult validation = SurvivorsContentPackIndex.ValidateSelectedSources(entry.Manifest);
                SurvivorsContentPackIndex index = SurvivorsContentPackIndex.Build(entry.Manifest);

                Assert.That(validation.ErrorCount, Is.Zero, FormatIssues(validation));
                Assert.That(index.TryGetRunProfile("HumanPlaytest", out float standardTarget, out float standardVictory), Is.True);
                Assert.That(index.TryGetRunProfile("SprintRun", out float sprintTarget, out float sprintVictory), Is.True);
                Assert.That(standardTarget, Is.EqualTo(1800f).Within(0.01f));
                Assert.That(standardVictory, Is.EqualTo(1800f).Within(0.01f));
                Assert.That(sprintTarget, Is.EqualTo(300f).Within(0.01f));
                Assert.That(sprintVictory, Is.EqualTo(300f).Within(0.01f));
            }
        }

        [Test]
        public void SelectedPackValidation_UsesManifestTextAssetsInsteadOfPackageDefaults()
        {
            GameContentPackManifest source = GetImportedEntries().Single(entry => entry.Manifest.PackId == "basic-survivors").Manifest;
            var invalidWeapons = new TextAsset("{\"weapons\":[],\"projectiles\":[]}");
            GameContentPackManifest fixture = CloneWithReplacement(source, SurvivorsContentPackIndex.WeaponsSourceId, invalidWeapons);
            try
            {
                GameContentAuthoringValidationResult result = SurvivorsContentPackIndex.ValidateSelectedSources(fixture);
                Assert.That(result.ErrorCount, Is.GreaterThan(0));
                Assert.That(result.Issues.Any(issue => issue.Message.IndexOf("weapon", StringComparison.OrdinalIgnoreCase) >= 0), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(fixture);
                UnityEngine.Object.DestroyImmediate(invalidWeapons);
            }
        }

        [Test]
        public void Index_BrokenReferenceFixtureIsVisibleOnTheOwningRecord()
        {
            GameContentPackManifest source = GetImportedEntries().Single(entry => entry.Manifest.PackId == "basic-survivors").Manifest;
            Assert.That(source.TryGetSource(SurvivorsContentPackIndex.ClassesSourceId, out GameContentPackSourceReference classSource), Is.True);
            string brokenJson = classSource.TextAsset.text.Replace(
                "weapon.survivors.arcane-wand",
                "weapon.survivors.missing-reference");
            var brokenClasses = new TextAsset(brokenJson);
            GameContentPackManifest fixture = CloneWithReplacement(source, SurvivorsContentPackIndex.ClassesSourceId, brokenClasses);
            try
            {
                SurvivorsContentPackIndex index = SurvivorsContentPackIndex.Build(fixture);
                GameContentRecordDescriptor broken = index.Records.Single(record => record.SourceRecordId == "class.survivors.arcane-initiate");
                Assert.That(index.Validation.ErrorCount, Is.GreaterThan(0));
                Assert.That(broken.HasBrokenReferences, Is.True);
                Assert.That(broken.OutboundReferences.Any(reference => !reference.Valid), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(fixture);
                UnityEngine.Object.DestroyImmediate(brokenClasses);
            }
        }

        [Test]
        public void ProviderActions_ResolveCorrectScenesAndMissingSampleStateIsActionable()
        {
            SurvivorsContentPackProvider provider = SurvivorsContentPackProvider.Instance;
            IReadOnlyList<GameContentPackDescriptor> packs = provider.GetContentPacks();
            Assert.That(packs.Count, Is.EqualTo(2));
            foreach (GameContentPackDescriptor pack in packs)
            {
                string scenePath = SurvivorsContentPackPlayLauncher.ResolveScenePath(pack.Manifest);
                Assert.That(scenePath, Does.EndWith(pack.PackId == "neon-arcana" ? "/Scenes/NeonArcana.unity" : "/Scenes/BasicSurvivorsGame.unity"));
                Assert.That(pack.Actions.Single(action => action.ActionId == SurvivorsContentPackProvider.OpenSceneActionId).Enabled, Is.True);
                Assert.That(pack.Actions.Single(action => action.ActionId == SurvivorsContentPackProvider.PlayStandardActionId).Enabled, Is.True);
                Assert.That(pack.Actions.Single(action => action.ActionId == SurvivorsContentPackProvider.PlaySprintActionId).Enabled, Is.True);
                Assert.That(pack.Actions.Single(action => action.ActionId == SurvivorsContentPackProvider.ValidateActionId).Enabled, Is.True);
                Assert.That(provider.ExecuteAction(pack.PackId, SurvivorsContentPackProvider.RevealActionId).Succeeded, Is.True);
                Assert.That(Selection.activeObject, Is.SameAs(pack.Manifest));
            }

            GameContentPackDescriptor missing = SurvivorsContentPackProvider.CreateSampleNotImportedDescriptor();
            Assert.That(missing.SourceState, Is.EqualTo(GameContentPackSourceState.SampleNotImported));
            Assert.That(missing.Actions.Single(action => action.ActionId == SurvivorsContentPackProvider.OpenSceneActionId).Enabled, Is.False);
            Assert.That(missing.Actions.Single(action => action.ActionId == SurvivorsContentPackProvider.PlayStandardActionId).Enabled, Is.False);
            Assert.That(missing.Actions.Single(action => action.ActionId == SurvivorsContentPackProvider.PlaySprintActionId).Enabled, Is.False);
            Assert.That(missing.Actions.Single(action => action.ActionId == SurvivorsContentPackProvider.OpenInstallerActionId).Enabled, Is.True);
        }

        [Test]
        public void ProviderActionDispatch_ReturnsSafeFailureForUnknownAction()
        {
            GameContentPackDescriptor basic = SurvivorsContentPackProvider.Instance.GetContentPacks()
                .Single(pack => pack.PackId == "basic-survivors");
            GameContentActionResult result = SurvivorsContentPackProvider.Instance.ExecuteAction(basic.PackId, "unknown-action");
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Message, Does.Contain("unknown-action"));
        }

        private static IReadOnlyList<GameContentPackManifestEntry> GetImportedEntries()
        {
            SurvivorsContentPackProvider.EnsureRegistered();
            GameContentPackDiscoveryReport report = GameContentPackDiscovery.Discover(SurvivorsContentPackProvider.StableProviderId);
            Assert.That(report.Entries.Count, Is.EqualTo(2),
                "Import exactly one current Basic Survivors Game sample before running content-pack integration tests. " + FormatIssues(report.Validation));
            return report.Entries;
        }

        private static GameContentPackManifest CloneWithReplacement(
            GameContentPackManifest source,
            string sourceId,
            TextAsset replacement)
        {
            GameContentPackSourceReference[] sources = source.ContentSources
                .Select(item => new GameContentPackSourceReference(
                    item.SourceId,
                    item.SourceKind,
                    string.Equals(item.SourceId, sourceId, StringComparison.OrdinalIgnoreCase) ? replacement : item.TextAsset,
                    item.DisplayLabel,
                    item.CategoryHint,
                    item.Required))
                .ToArray();
            GameContentPackManifest clone = ScriptableObject.CreateInstance<GameContentPackManifest>();
            clone.Configure(
                source.PackId,
                source.OwningPackageId,
                source.ProviderId,
                source.DisplayName,
                source.Description,
                source.SchemaVersion,
                source.Tags,
                source.PlayableScene,
                source.DefaultTheme,
                sources,
                source.Preview,
                source.Icon);
            return clone;
        }

        private static string FormatIssues(GameContentAuthoringValidationResult validation)
        {
            return validation == null
                ? "No validation result."
                : string.Join(Environment.NewLine, validation.Issues.Select(issue => issue.Path + ": " + issue.Message));
        }
    }
}
