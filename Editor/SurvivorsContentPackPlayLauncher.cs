using System;
using System.Linq;
using Deucarian.GameContentAuthoring.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    [InitializeOnLoad]
    public static class SurvivorsContentPackPlayLauncher
    {
        private const string PendingKey = "Deucarian.Survivors.ContentPackLaunch.Pending";
        private const string ScenePathKey = "Deucarian.Survivors.ContentPackLaunch.ScenePath";
        private const string ProfileKey = "Deucarian.Survivors.ContentPackLaunch.Profile";
        private const string FailureKey = "Deucarian.Survivors.ContentPackLaunch.Failure";
        private static double _deadline;

        static SurvivorsContentPackPlayLauncher()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (EditorApplication.isPlaying && SessionState.GetBool(PendingKey, false)) BeginApplyPendingLaunch();
        }

        public static string LastLaunchFailure => SessionState.GetString(FailureKey, string.Empty);

        public static string ResolveScenePath(GameContentPackManifest manifest)
        {
            return manifest == null || manifest.PlayableScene == null
                ? string.Empty
                : AssetDatabase.GetAssetPath(manifest.PlayableScene);
        }

        public static GameContentActionResult OpenScene(GameContentPackManifest manifest)
        {
            string scenePath = ResolveScenePath(manifest);
            if (string.IsNullOrWhiteSpace(scenePath))
                return GameContentActionResult.Failure("The selected content pack has no imported playable scene.");
            if (Application.isBatchMode)
                return GameContentActionResult.Failure("Scenes cannot be opened interactively in batch mode.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return GameContentActionResult.Failure("Scene opening was cancelled because modified scenes were not saved.");

            try
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                return GameContentActionResult.Success("Opened " + manifest.DisplayName + " scene.");
            }
            catch (Exception exception)
            {
                return GameContentActionResult.Failure("Could not open the selected scene: " + exception.GetBaseException().Message);
            }
        }

        public static GameContentActionResult Play(GameContentPackManifest manifest, SurvivorsPacingProfile profile)
        {
            if (profile != SurvivorsPacingProfile.HumanPlaytest && profile != SurvivorsPacingProfile.SprintRun)
                return GameContentActionResult.Failure("Only the strict Standard and Sprint profiles can be launched from content authoring.");
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return GameContentActionResult.Failure("Unity is already playing or changing Play Mode state.");

            GameContentActionResult opened = OpenScene(manifest);
            if (!opened.Succeeded) return opened;

            string scenePath = ResolveScenePath(manifest);
            SessionState.SetString(ScenePathKey, scenePath);
            SessionState.SetInt(ProfileKey, (int)profile);
            SessionState.SetString(FailureKey, string.Empty);
            SessionState.SetBool(PendingKey, true);
            EditorApplication.isPlaying = true;
            string label = profile == SurvivorsPacingProfile.SprintRun ? "Sprint Run" : "Standard / Human Playtest";
            return GameContentActionResult.Success("Entering Play Mode and starting " + label + ".");
        }

        public static bool SelectProfile(SurvivorsTemplateController controller, SurvivorsPacingProfile profile)
        {
            if (controller == null) return false;
            if (profile == SurvivorsPacingProfile.HumanPlaytest) return controller.SelectStandardRun();
            if (profile == SurvivorsPacingProfile.SprintRun) return controller.SelectSprintRun();
            return false;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(PendingKey, false))
            {
                BeginApplyPendingLaunch();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= ApplyPendingLaunch;
            }
        }

        private static void BeginApplyPendingLaunch()
        {
            _deadline = EditorApplication.timeSinceStartup + 10d;
            EditorApplication.update -= ApplyPendingLaunch;
            EditorApplication.update += ApplyPendingLaunch;
        }

        private static void ApplyPendingLaunch()
        {
            if (!EditorApplication.isPlaying || !SessionState.GetBool(PendingKey, false))
            {
                EditorApplication.update -= ApplyPendingLaunch;
                return;
            }

            string scenePath = SessionState.GetString(ScenePathKey, string.Empty);
            SurvivorsPacingProfile profile = (SurvivorsPacingProfile)SessionState.GetInt(
                ProfileKey,
                (int)SurvivorsPacingProfile.HumanPlaytest);
            SurvivorsTemplateController controller = UnityEngine.Object
                .FindObjectsByType<SurvivorsTemplateController>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate != null &&
                                             candidate.gameObject.scene.IsValid() &&
                                             string.Equals(candidate.gameObject.scene.path, scenePath, StringComparison.OrdinalIgnoreCase));
            if (controller != null && SelectProfile(controller, profile))
            {
                ClearPendingLaunch();
                return;
            }

            if (EditorApplication.timeSinceStartup < _deadline) return;
            SessionState.SetString(
                FailureKey,
                controller == null
                    ? "The selected scene did not create a SurvivorsTemplateController."
                    : "Strict authored binding prevented the selected run profile from starting.");
            ClearPendingLaunch(clearFailure: false);
        }

        private static void ClearPendingLaunch(bool clearFailure = true)
        {
            SessionState.SetBool(PendingKey, false);
            SessionState.EraseString(ScenePathKey);
            SessionState.EraseInt(ProfileKey);
            if (clearFailure) SessionState.SetString(FailureKey, string.Empty);
            EditorApplication.update -= ApplyPendingLaunch;
        }
    }
}
