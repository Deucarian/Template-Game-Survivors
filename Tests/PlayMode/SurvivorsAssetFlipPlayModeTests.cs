using System;
using System.Collections;
using System.Linq;
using Deucarian.Persistence;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Deucarian.TemplateGameSurvivors.PlayModeTests
{
    public sealed class SurvivorsAssetFlipPlayModeTests
    {
        private const string SceneMarkerName = "PLAYTEST_THIS_SCENE_NEON_ARCANA";
        private Scene _loadedScene;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (!_loadedScene.IsValid() || !_loadedScene.isLoaded)
            {
                yield break;
            }

            AsyncOperation unload = SceneManager.UnloadSceneAsync(_loadedScene);
            while (unload != null && !unload.isDone)
            {
                yield return null;
            }

            _loadedScene = default;
        }

        [UnityTest]
        public IEnumerator ImportedNeonArcanaSceneStartsStrictSprintWithAlternateRuntimeContent()
        {
            yield return LoadScene(result => _loadedScene = result);
            SurvivorsTemplateController controller = FindComponentInScene<SurvivorsTemplateController>(_loadedScene);
            Assert.IsNotNull(controller);
            yield return WaitUntil(() => controller.IsRunModeSelectionOpen || controller.IsRunStarted, 120);

            Assert.IsTrue(controller.IsRunModeSelectionOpen);
            Assert.IsFalse(controller.IsRunStarted);
            Assert.IsTrue(controller.IsStrictAuthoredSample, controller.AuthoredContentStatus);
            Assert.IsTrue(controller.IsAuthoredContentBound, controller.AuthoredContentStatus);
            Assert.IsFalse(controller.IsFallbackContentActive, controller.AuthoredContentStatus);
            Assert.AreEqual("Neon Arcana", controller.CurrentUiThemeName);
            Assert.AreEqual("Run The Circuit", controller.CurrentTutorialStepTitle);
            Assert.That(controller.AvailableUiThemesForTest.Select(theme => theme.themeName), Is.EquivalentTo(new[] { "Neon Arcana", "Neon Arcana: Afterglow" }));

            ConfigureIsolatedMetaPersistence(controller, "neon-sprint");
            Assert.IsTrue(controller.SelectSprintRun());
            yield return null;
            if (controller.IsTutorialOverlayOpen)
            {
                Assert.IsTrue(controller.SkipTutorialForTest());
            }

            yield return WaitUntil(() => controller.IsPlaying, 120);
            yield return WaitUntil(
                () => FindGameObjectInScene(_loadedScene, "Survivors Player") != null,
                120);

            GameObject player = FindGameObjectInScene(_loadedScene, "Survivors Player");
            Assert.AreEqual(new Color32(92, 255, 241, 255), (Color32)player.GetComponentInChildren<Renderer>().sharedMaterial.color);
            Assert.AreEqual(SurvivorsPacingProfile.SprintRun, controller.CurrentPacingProfile);
            Assert.AreEqual("Neon Arcana Sprint", controller.CurrentRunModeDisplayName);
            Assert.AreEqual(300f, controller.CurrentTuning.SurvivalVictoryTimeSeconds);
            Assert.That(controller.TopCenterTimerHudLabel, Does.Contain("Neon Arcana Sprint"));
            Assert.AreEqual(1, controller.ActiveWeaponCount);
            Assert.That(controller.CurrentBuildHudLinesForTest()[1], Does.Contain("Neon Pulse"));
            Assert.AreEqual("Pulse Runner", controller.SelectedClass.DisplayName);

            controller.ForceLevelUpWithLockedChoiceForTest("upgrade.survivors.arcane-damage");
            yield return WaitUntil(() => controller.State == SurvivorsRunState.LevelUp, 120);
            int pulseChoiceIndex = Enumerable.Range(0, controller.CurrentDraftChoices.Count)
                .FirstOrDefault(index => controller.GetCurrentDraftChoiceLabelForTest(index).Contains("Pulse Voltage"));
            string pulseChoiceLabel = controller.GetCurrentDraftChoiceLabelForTest(pulseChoiceIndex);
            Assert.That(pulseChoiceLabel, Does.Contain("Pulse Voltage"));
            Assert.That(pulseChoiceLabel, Does.Contain("Affects Neon Pulse"));
            Assert.That(pulseChoiceLabel, Does.Not.Contain("Arcane Damage"));
            Assert.IsTrue(controller.SelectUpgrade(pulseChoiceIndex));
            yield return WaitUntil(() => controller.IsPlaying, 120);

            SurvivorsEnemyActor swarm = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(9f, 0f, 0f), SurvivorsEnemyRole.Swarm, 100f);
            SurvivorsEnemyActor boss = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(-10f, 0f, 0f), SurvivorsEnemyRole.Boss, 100f);
            Assert.AreEqual("Static Wisp", swarm.DisplayName);
            Assert.AreEqual("Blacklight Sovereign", boss.DisplayName);

            controller.FireWeaponForTest();
            SurvivorsProjectileActor projectile = FindComponentsInScene<SurvivorsProjectileActor>(_loadedScene).FirstOrDefault(item => item.IsActive);
            Assert.IsNotNull(projectile);
            Assert.AreEqual(new Color32(0, 229, 255, 255), (Color32)projectile.GetComponentInChildren<Renderer>().sharedMaterial.color);

            Assert.IsTrue(controller.DebugGrantBloodShards(10));
            Assert.That(controller.GetResultMetaUpgradeOptionLabelsForTest()[0], Does.Contain("Pulse Legacy"));
            Assert.That(controller.GetResultMetaUpgradeOptionLabelsForTest()[0], Does.Contain("Neon Fragments"));
            Assert.That(controller.GetResultClassOptionLabelsForTest(), Has.Some.Contains("Reactor Knight"));
        }

        [UnityTest]
        public IEnumerator ImportedNeonArcanaScenePreservesStandardThirtyMinuteProfile()
        {
            yield return LoadScene(result => _loadedScene = result);
            SurvivorsTemplateController controller = FindComponentInScene<SurvivorsTemplateController>(_loadedScene);
            Assert.IsNotNull(controller);
            yield return WaitUntil(() => controller.IsRunModeSelectionOpen || controller.IsRunStarted, 120);

            ConfigureIsolatedMetaPersistence(controller, "neon-standard");
            Assert.IsTrue(controller.SelectStandardRun());
            yield return null;
            if (controller.IsTutorialOverlayOpen)
            {
                Assert.IsTrue(controller.SkipTutorialForTest());
            }

            yield return WaitUntil(() => controller.IsPlaying, 120);
            Assert.AreEqual(SurvivorsPacingProfile.HumanPlaytest, controller.CurrentPacingProfile);
            Assert.AreEqual("Neon Arcana Standard", controller.CurrentRunModeDisplayName);
            Assert.AreEqual(1800f, controller.CurrentTuning.SurvivalVictoryTimeSeconds);
            Assert.That(controller.TopCenterTimerHudLabel, Does.Contain("Neon Arcana Standard"));
            Assert.AreEqual(1, controller.ActiveWeaponCount);
        }

        private static void ConfigureIsolatedMetaPersistence(SurvivorsTemplateController controller, string slotPrefix)
        {
            controller.ConfigureMetaPersistenceForTest(
                new PersistenceService(new InMemoryTextStorage()),
                new SaveSlotId(slotPrefix + "-" + Guid.NewGuid().ToString("N")));
        }

        private static IEnumerator LoadScene(Action<Scene> assign)
        {
            string scenePath = SurvivorsImportedSampleSceneResolver.ResolveOrIgnore(SurvivorsImportedSampleSceneKind.NeonArcana);

            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                scenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.IsNotNull(load);
            while (!load.isDone)
            {
                yield return null;
            }

            Scene scene = SceneManager.GetSceneByPath(scenePath);
            Assert.IsTrue(scene.IsValid(), scenePath);
            Assert.IsTrue(scene.isLoaded, scenePath);
            Assert.IsNotNull(FindGameObjectInScene(scene, SceneMarkerName));
            assign(scene);
        }

        private static IEnumerator WaitUntil(Func<bool> condition, int maximumFrames)
        {
            for (int frame = 0; frame < maximumFrames && !condition(); frame++)
            {
                yield return null;
            }

            Assert.IsTrue(condition());
        }

        private static T FindComponentInScene<T>(Scene scene) where T : Component
        {
            return FindComponentsInScene<T>(scene).FirstOrDefault();
        }

        private static T[] FindComponentsInScene<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .Where(root => root != null)
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static GameObject FindGameObjectInScene(Scene scene, string name)
        {
            Transform match = scene.GetRootGameObjects()
                .Where(root => root != null)
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(item => string.Equals(item.name, name, StringComparison.Ordinal));
            return match == null ? null : match.gameObject;
        }
    }
}
