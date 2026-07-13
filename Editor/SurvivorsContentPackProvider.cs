using System;
using System.Collections.Generic;
using System.Linq;
using Deucarian.GameContentAuthoring.Editor;
using UnityEditor;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    [InitializeOnLoad]
    public sealed class SurvivorsContentPackProvider :
        IGameContentAuthoringProvider,
        IGameContentAuthoringSurfaceProvider,
        IGameContentPackProvider,
        IGameContentPackEditProvider,
        IGameContentAuthoringProviderVisibility
    {
        public const string StableProviderId = "com.deucarian.template.game.survivors.content-packs";
        public const string OwningPackageId = "com.deucarian.template.game.survivors";
        public const string OpenSceneActionId = "open-scene";
        public const string PlayStandardActionId = "play-standard";
        public const string PlaySprintActionId = "play-sprint";
        public const string ValidateActionId = "validate";
        public const string BrowseActionId = "browse-content";
        public const string RevealActionId = "reveal-source";
        public const string OpenInstallerActionId = "open-package-installer";

        private static readonly SurvivorsContentPackProvider SharedInstance = new SurvivorsContentPackProvider();
        private readonly GameContentPackBrowserState _browserState = new GameContentPackBrowserState();
        private readonly Dictionary<string, SurvivorsContentPackIndex> _indexes =
            new Dictionary<string, SurvivorsContentPackIndex>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameContentPackManifest> _manifests =
            new Dictionary<string, GameContentPackManifest>(StringComparer.OrdinalIgnoreCase);
        private IReadOnlyList<GameContentPackDescriptor> _packs = Array.Empty<GameContentPackDescriptor>();

        private SurvivorsContentPackProvider()
        {
        }

        static SurvivorsContentPackProvider()
        {
            EnsureRegistered();
        }

        public static SurvivorsContentPackProvider Instance => SharedInstance;
        public string ProviderId => StableProviderId;
        public string DisplayName => "Content Packs";
        public string Description => "Browse, validate, and safely edit approved fields in imported Basic Survivors and Neon Arcana JSON packs.";
        public int SortOrder => 90;
        public bool Enabled => true;
        public bool VisibleInNavigation => false;

        public static void EnsureRegistered()
        {
            if (!GameContentAuthoringProviderRegistry.IsProviderRegistered(StableProviderId))
                GameContentAuthoringProviderRegistry.Register(SharedInstance);
        }

        public void OnSelected()
        {
            _browserState.Refresh(this);
        }

        public void Draw(GameContentAuthoringContext context)
        {
        }

        public void DrawPreview(GameContentAuthoringPreviewContext context)
        {
        }

        public void StopPreview()
        {
        }

        public void DrawCustomAuthoringSurface(GameContentAuthoringSurfaceContext context)
        {
            GameContentPackBrowser.Draw(context, this, _browserState);
        }

        public IReadOnlyList<GameContentPackDescriptor> GetContentPacks()
        {
            RefreshPacks();
            return _packs;
        }

        public IReadOnlyList<GameContentRecordDescriptor> GetRecords(string packId)
        {
            if (_packs.Count == 0) RefreshPacks();
            return _indexes.TryGetValue(packId ?? string.Empty, out SurvivorsContentPackIndex index)
                ? index.Records
                : Array.Empty<GameContentRecordDescriptor>();
        }

        public GameContentAuthoringValidationResult ValidatePack(string packId)
        {
            if (_packs.Count == 0) RefreshPacks();
            if (!_manifests.TryGetValue(packId ?? string.Empty, out GameContentPackManifest manifest))
                return new GameContentAuthoringValidationResult(new[]
                {
                    GameContentAuthoringValidationIssue.Error("Sample", "Import the Basic Survivors Game sample before validating a content pack.")
                });
            return SurvivorsContentPackIndex.ValidateSelectedSources(manifest);
        }

        public GameContentActionResult ExecuteAction(string packId, string actionId)
        {
            if (string.Equals(actionId, OpenInstallerActionId, StringComparison.OrdinalIgnoreCase))
            {
                return EditorApplication.ExecuteMenuItem("Tools/Deucarian/Package Installer")
                    ? GameContentActionResult.Success("Opened Package Installer. Import the Basic Survivors Game sample from the Survivors package.")
                    : GameContentActionResult.Failure("Package Installer is not installed. Install it, then import the Basic Survivors Game sample.");
            }

            if (!_manifests.TryGetValue(packId ?? string.Empty, out GameContentPackManifest manifest))
                return GameContentActionResult.Failure("The selected Survivors sample is not imported.");

            if (string.Equals(actionId, OpenSceneActionId, StringComparison.OrdinalIgnoreCase))
                return SurvivorsContentPackPlayLauncher.OpenScene(manifest);
            if (string.Equals(actionId, PlayStandardActionId, StringComparison.OrdinalIgnoreCase))
                return SurvivorsContentPackPlayLauncher.Play(manifest, SurvivorsPacingProfile.HumanPlaytest);
            if (string.Equals(actionId, PlaySprintActionId, StringComparison.OrdinalIgnoreCase))
                return SurvivorsContentPackPlayLauncher.Play(manifest, SurvivorsPacingProfile.SprintRun);
            if (string.Equals(actionId, ValidateActionId, StringComparison.OrdinalIgnoreCase))
            {
                GameContentAuthoringValidationResult validation = SurvivorsContentPackIndex.ValidateSelectedSources(manifest);
                RefreshPacks();
                return validation.ErrorCount == 0
                    ? GameContentActionResult.Success("Selected authored sources are valid.", validation)
                    : GameContentActionResult.Failure("Selected authored sources contain validation errors.", validation);
            }
            if (string.Equals(actionId, BrowseActionId, StringComparison.OrdinalIgnoreCase))
            {
                GameContentAuthoringWindow.Open();
                return GameContentActionResult.Success("Survivors content browser focused.");
            }
            if (string.Equals(actionId, RevealActionId, StringComparison.OrdinalIgnoreCase))
            {
                Selection.activeObject = manifest;
                EditorGUIUtility.PingObject(manifest);
                return GameContentActionResult.Success("Selected the " + manifest.DisplayName + " manifest.");
            }

            return GameContentActionResult.Failure("Unknown Survivors content-pack action '" + actionId + "'.");
        }

        public GameContentEditAvailability CanEdit(GameContentEditRequest request)
        {
            if (!TryResolveEditRequest(request, out SurvivorsEditableSource source, out string reason))
                return GameContentEditAvailability.ReadOnly(reason, StableProviderId);
            return GameContentEditAvailability.Editable(
                StableProviderId,
                source.Definition.EditableFieldCount,
                source.SourceTarget);
        }

        public IGameContentEditSession BeginEdit(GameContentEditRequest request)
        {
            if (!TryResolveEditRequest(request, out SurvivorsEditableSource source, out string reason))
                throw new InvalidOperationException(reason);
            if (!SurvivorsImportedSampleEditConsent.EnsureGranted())
                throw new InvalidOperationException("Editing was cancelled before changing the imported sample copy.");
            if (!TryResolveEditRequest(request, out source, out reason))
                throw new InvalidOperationException("The source changed while edit consent was shown: " + reason);
            return new SurvivorsContentEditSession(source, RefreshAfterEdit);
        }

        private void RefreshPacks()
        {
            _indexes.Clear();
            _manifests.Clear();
            GameContentPackDiscoveryReport discovery = GameContentPackDiscovery.Discover(StableProviderId);
            if (discovery.Entries.Count == 0)
            {
                _packs = new[] { CreateSampleNotImportedDescriptor() };
                return;
            }

            var packs = new List<GameContentPackDescriptor>(discovery.Entries.Count);
            for (int i = 0; i < discovery.Entries.Count; i++)
            {
                GameContentPackManifestEntry entry = discovery.Entries[i];
                SurvivorsContentPackIndex index = SurvivorsContentPackIndex.Build(entry.Manifest);
                GameContentAuthoringValidationResult validation = MergeValidation(entry.Validation, index.Validation);
                GameContentPackSourceState state = entry.SourceState == GameContentPackSourceState.Available && validation.ErrorCount > 0
                    ? GameContentPackSourceState.ValidationFailed
                    : entry.SourceState;
                bool usable = state == GameContentPackSourceState.Available && entry.Manifest.PlayableScene != null;
                packs.Add(new GameContentPackDescriptor(
                    entry.Manifest.PackId,
                    entry.Manifest.OwningPackageId,
                    entry.Manifest.ProviderId,
                    entry.Manifest.DisplayName,
                    entry.Manifest.Description,
                    entry.Manifest.SchemaVersion,
                    entry.Manifest.Tags,
                    entry.SourceKind,
                    state,
                    entry.ManifestPath,
                    entry.Manifest,
                    entry.Manifest.PlayableScene,
                    entry.Manifest.Preview,
                    entry.Manifest.Icon,
                    entry.Manifest.DefaultTheme,
                    index.Categories,
                    BuildActions(usable, entry.Manifest != null, state),
                    validation,
                    index.Records.Count,
                    BuildAccess(entry.Manifest, entry.SourceKind, state)));

                if (!_indexes.ContainsKey(entry.Manifest.PackId)) _indexes.Add(entry.Manifest.PackId, index);
                if (!_manifests.ContainsKey(entry.Manifest.PackId)) _manifests.Add(entry.Manifest.PackId, entry.Manifest);
            }

            _packs = packs;
        }

        private bool TryResolveEditRequest(
            GameContentEditRequest request,
            out SurvivorsEditableSource source,
            out string reason)
        {
            source = null;
            reason = string.Empty;
            if (request == null || !request.IsValid)
            {
                reason = "Select one existing record from an imported Survivors content pack.";
                return false;
            }
            if (string.Equals(request.SelectedPackKey, GameContentPackContext.AllPacksSelectionKey, StringComparison.Ordinal))
            {
                reason = "All Packs is a read-only browsing context. Select Basic Survivors or Neon Arcana.";
                return false;
            }
            if (!string.Equals(request.ProviderId, StableProviderId, StringComparison.OrdinalIgnoreCase))
            {
                reason = "The edit request does not target the registered Survivors provider.";
                return false;
            }

            RefreshPacks();
            GameContentPackDescriptor[] packMatches = _packs
                .Where(pack => string.Equals(pack.StableKey, request.SelectedPackKey, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (packMatches.Length == 0)
            {
                reason = "The selected Survivors pack is unavailable. Import one current Basic Survivors Game sample.";
                return false;
            }
            if (packMatches.Length > 1 || packMatches[0].SourceState == GameContentPackSourceState.DuplicateConflict)
            {
                reason = "Resolve duplicate imported pack manifests before editing.";
                return false;
            }
            GameContentPackDescriptor pack = packMatches[0];
            if (pack.SourceState != GameContentPackSourceState.Available)
            {
                reason = "Fix the selected pack's missing-source or strict validation errors before editing.";
                return false;
            }
            if (!pack.Access.CanEditExisting)
            {
                reason = string.IsNullOrWhiteSpace(pack.Access.DisabledReason)
                    ? "This pack source is read-only."
                    : pack.Access.DisabledReason;
                return false;
            }
            if (!_manifests.TryGetValue(request.RecordKey.PackId, out GameContentPackManifest manifest) ||
                !_indexes.TryGetValue(request.RecordKey.PackId, out SurvivorsContentPackIndex index))
            {
                reason = "The selected pack's manifest or content index is unavailable.";
                return false;
            }
            if (!string.Equals(manifest.StableKey, request.SelectedPackKey, StringComparison.OrdinalIgnoreCase))
            {
                reason = "The canonical record does not belong to the selected pack context.";
                return false;
            }
            GameContentRecordDescriptor record = index.Records.FirstOrDefault(candidate =>
                candidate.CanonicalKey.Equals(request.RecordKey));
            if (record == null)
            {
                reason = "Select a record owned by the current Basic Survivors or Neon Arcana index.";
                return false;
            }
            return SurvivorsEditableSource.TryCreate(manifest, record, true, out source, out reason);
        }

        private void RefreshAfterEdit()
        {
            RefreshPacks();
            _browserState.Refresh(this);
        }

        private static GameContentPackAccessDescriptor BuildAccess(
            GameContentPackManifest manifest,
            GameContentPackSourceKind sourceKind,
            GameContentPackSourceState state)
        {
            if (state == GameContentPackSourceState.Available &&
                SurvivorsEditableSource.CanAdvertiseEditing(manifest, sourceKind, out string reason))
            {
                return new GameContentPackAccessDescriptor(
                    GameContentPackBackendCapability.Read |
                    GameContentPackBackendCapability.Validate |
                    GameContentPackBackendCapability.RevealSource |
                    GameContentPackBackendCapability.EditExisting,
                    "Project-owned imported JSON with lossless field editing");
            }

            SurvivorsEditableSource.CanAdvertiseEditing(manifest, sourceKind, out string disabledReason);
            return new GameContentPackAccessDescriptor(
                GameContentPackBackendCapability.Read |
                GameContentPackBackendCapability.Validate |
                GameContentPackBackendCapability.RevealSource,
                "Read-only JSON source",
                string.IsNullOrWhiteSpace(disabledReason)
                    ? "Fix pack availability and import a project-owned sample before editing."
                    : disabledReason);
        }

        public static GameContentPackDescriptor CreateSampleNotImportedDescriptor()
        {
            const string reason = "Import the Basic Survivors Game sample before browsing or launching its content packs.";
            return new GameContentPackDescriptor(
                "sample-not-imported",
                OwningPackageId,
                StableProviderId,
                "Survivors Sample Not Imported",
                reason + " Package Installer owns sample import and freshness.",
                "1",
                new[] { "survivors", "sample" },
                GameContentPackSourceKind.Package,
                GameContentPackSourceState.SampleNotImported,
                "Packages/" + OwningPackageId + "/Samples~/BasicSurvivorsGame",
                null,
                null,
                null,
                null,
                null,
                Array.Empty<GameContentCategoryDescriptor>(),
                BuildActions(false, false, GameContentPackSourceState.SampleNotImported),
                new GameContentAuthoringValidationResult(new[]
                {
                    GameContentAuthoringValidationIssue.Warning("Sample", reason)
                }),
                0,
                new GameContentPackAccessDescriptor(
                    GameContentPackBackendCapability.None,
                    "Sample not imported",
                    reason));
        }

        private static IReadOnlyList<GameContentActionDescriptor> BuildActions(
            bool launchEnabled,
            bool manifestAvailable,
            GameContentPackSourceState state)
        {
            bool uniqueManifestAvailable = manifestAvailable && state != GameContentPackSourceState.DuplicateConflict;
            string launchReason = state == GameContentPackSourceState.SampleNotImported
                ? "Import the Basic Survivors Game sample first."
                : state == GameContentPackSourceState.DuplicateConflict
                    ? "Resolve duplicate imported pack manifests first."
                    : state == GameContentPackSourceState.ValidationFailed
                        ? "Fix selected authored-source validation errors first."
                        : "The imported playable scene is unavailable.";
            return new[]
            {
                new GameContentActionDescriptor(OpenSceneActionId, "Open Scene", "Open this pack's authored playable scene.", launchEnabled, launchReason, GameContentActionKind.OpenScene),
                new GameContentActionDescriptor(PlayStandardActionId, "Play Standard", "Start the actual strict 1800-second Standard / Human Playtest profile.", launchEnabled, launchReason, GameContentActionKind.Play),
                new GameContentActionDescriptor(PlaySprintActionId, "Play Sprint", "Start the actual strict 300-second Sprint profile.", launchEnabled, launchReason, GameContentActionKind.Play),
                new GameContentActionDescriptor(ValidateActionId, "Validate", "Validate this manifest's selected TextAssets.", uniqueManifestAvailable, state == GameContentPackSourceState.DuplicateConflict ? "Resolve duplicate imported pack manifests first." : "Import the sample first.", GameContentActionKind.Validate),
                new GameContentActionDescriptor(BrowseActionId, "Browse Content", "Focus this read-only content browser.", true, string.Empty, GameContentActionKind.Browse),
                new GameContentActionDescriptor(RevealActionId, "Reveal Source", "Select this pack manifest in the Project window.", uniqueManifestAvailable, state == GameContentPackSourceState.DuplicateConflict ? "Use the listed source locations to resolve the duplicate manifests." : "Import the sample first.", GameContentActionKind.RevealSource),
                new GameContentActionDescriptor(OpenInstallerActionId, "Open Package Installer", "Open Package Installer for sample import or package management.", true, string.Empty, GameContentActionKind.OpenPackageInstaller)
            };
        }

        private static GameContentAuthoringValidationResult MergeValidation(params GameContentAuthoringValidationResult[] results)
        {
            return new GameContentAuthoringValidationResult(results
                .Where(result => result != null)
                .SelectMany(result => result.Issues)
                .GroupBy(issue => issue.Severity + "|" + issue.Path + "|" + issue.Message, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToArray());
        }
    }
}
