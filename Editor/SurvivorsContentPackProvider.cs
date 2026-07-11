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
        public string Description => "Browse and validate imported Basic Survivors and Neon Arcana authored JSON packs.";
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
                    GameContentPackAccessDescriptor.ReadOnlyJson));

                if (!_indexes.ContainsKey(entry.Manifest.PackId)) _indexes.Add(entry.Manifest.PackId, index);
                if (!_manifests.ContainsKey(entry.Manifest.PackId)) _manifests.Add(entry.Manifest.PackId, entry.Manifest);
            }

            _packs = packs;
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
