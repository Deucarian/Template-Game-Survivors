using System;
using System.Collections;
using System.Collections.Generic;
using Deucarian.Persistence;
using Deucarian.RunUpgrades;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Deucarian.TemplateGameSurvivors.PlayModeTests
{
    public sealed class SurvivorsTemplatePlayModeTests
    {
        private const string FeedbackRootName = "Survivors Feedback Presentation";
        private const string SpawnPulseName = "Survivors Spawn Pulse";
        private const string FirePulseName = "Survivors Weapon Fire Pulse";
        private const string KillPulseName = "Survivors Kill Burst";
        private const string PickupPulseName = "Survivors Pickup Pulse";
        private const string LevelUpPulseName = "Survivors Level Up Pulse";
        private const string BossPulseName = "Survivors Boss Cue Pulse";
        private const string FeedbackAudioName = "Survivors Feedback Audio";
        private const string DirectionalLightName = "Survivors Directional Light";
        private const string ImportedPlayableScenePath = "Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity";
        private const string PlayableSceneMarkerName = "PLAYTEST_THIS_SCENE_OPEN_ME";

        [UnityTest]
        public IEnumerator FirstPlayableSliceBootsSpawnsKillsAndLevels()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.1f, 0f, 0f), 1f);
            controller.FireWeaponForTest();
            for (int i = 0; i < 120; i++)
            {
                controller.Simulate(1f / 60f);
                yield return null;
            }

            Assert.That(controller.ProjectileLaunchCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(1));

            controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(0.2f, 0f, 0.2f), controller.RequiredExperienceForNextLevel);
            for (int i = 0; i < 10; i++)
            {
                controller.Simulate(1f / 30f);
                yield return null;
            }

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
            Assert.That(controller.RewardCardPresentationCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(controller.LastRewardCardPresentationLabel, Does.Contain("Level Up"));
            Assert.IsTrue(controller.SelectUpgrade(0));
            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.AreEqual(1, controller.RewardSelectionFeedbackCount);
            Assert.That(controller.LastRewardSelectionFeedbackLabel, Does.Contain("Level Up"));
            Assert.That(controller.ActiveRewardFeedbackLabel, Does.Contain("Level Up"));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ImportedPlayableSampleSceneBootsIntoPlayingRun()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ImportedPlayableScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.IsNotNull(load);
            yield return WaitForAsyncOperation(load, 10f);

            Scene scene = SceneManager.GetSceneByPath(ImportedPlayableScenePath);
            Assert.IsTrue(scene.IsValid(), ImportedPlayableScenePath);
            Assert.IsTrue(scene.isLoaded, ImportedPlayableScenePath);
            Assert.IsNotNull(FindRootInScene(scene, PlayableSceneMarkerName));

            SurvivorsTemplateController controller = FindComponentInScene<SurvivorsTemplateController>(scene);
            Assert.IsNotNull(controller);

            float startingRunTime = controller.RunTimeSeconds;
            for (int i = 0; i < 120 && (!controller.IsPlaying || controller.RunTimeSeconds <= startingRunTime); i++)
            {
                yield return null;
            }

            Assert.IsTrue(controller.IsPlaying);
            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.That(controller.RunTimeSeconds, Is.GreaterThan(startingRunTime));
            Assert.That(controller.InfiniteArenaTileCountForTest, Is.GreaterThan(0));
            Assert.That(controller.ActiveWeaponCount, Is.GreaterThanOrEqualTo(1));

            AsyncOperation unload = SceneManager.UnloadSceneAsync(scene);
            if (unload != null)
            {
                yield return WaitForAsyncOperation(unload, 10f);
            }
        }

        [UnityTest]
        public IEnumerator EnemyDamageSpawnsShortLivedFeedbackNumbers()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            SurvivorsEnemyActor enemy = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(8f, 0f, 0f), 25f);
            Assert.AreEqual(0, controller.DamagePopupSpawnCount);
            Assert.AreEqual(0, controller.ActiveDamagePopupCount);
            Assert.AreEqual(0, controller.EnemyHitFlashFeedbackCount);

            enemy.ApplyDamage(7.2f, "test.status.damage-popup");
            yield return null;

            Assert.AreEqual(1, controller.DamagePopupSpawnCount);
            Assert.AreEqual(1, controller.ActiveDamagePopupCount);
            Assert.AreEqual(1, controller.EnemyHitFlashFeedbackCount);
            Assert.IsTrue(enemy.IsHitFlashActive);

            controller.Simulate(0.2f);
            yield return null;

            Assert.IsFalse(enemy.IsHitFlashActive);

            controller.ForceLevelUp();
            controller.Simulate(1.1f);
            yield return null;

            Assert.AreEqual(0, controller.ActiveDamagePopupCount);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator KeenEdgeMakesWeaponHitsCriticallyStrike()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.KeenEdgeUpgradeId));
            }

            Assert.AreEqual(1f, controller.CriticalChanceNormalized, 0.001f);
            Assert.That(controller.CriticalDamageMultiplier, Is.GreaterThan(1.5f));

            SurvivorsEnemyActor enemy = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(5f, 0f, 0f), 40f);
            float healthBefore = enemy.CurrentHealth;
            var damage = enemy.ApplyDamage(6f, "weapon.survivors.arcane-wand");
            yield return null;

            Assert.IsNotNull(damage);
            Assert.IsTrue(damage.Critical.IsCritical);
            Assert.That(damage.FinalDamage, Is.GreaterThan(6d));
            Assert.That(enemy.CurrentHealth, Is.LessThan(healthBefore - 6f));
            Assert.AreEqual(1, controller.CriticalHitFeedbackCount);
            Assert.AreEqual(1, controller.DamagePopupSpawnCount);
            Assert.AreEqual(1, controller.EnemyHitFlashFeedbackCount);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator EnemyDeathCreatesShortLivedWorldFeedback()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            SurvivorsEnemyActor enemy = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(8f, 0f, 0f), 4f);
            Assert.AreEqual(0, controller.EnemyDeathEffectCount);
            Assert.AreEqual(0, controller.ActiveEnemyDeathEffectCount);

            enemy.ApplyDamage(12f, "test.enemy-death-feedback");
            yield return null;

            Assert.AreEqual(1, controller.EnemyDeathEffectCount);
            Assert.AreEqual(1, controller.ActiveEnemyDeathEffectCount);

            controller.Simulate(0.5f);
            yield return null;

            Assert.AreEqual(0, controller.ActiveEnemyDeathEffectCount);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator SoulflareGlyphWeaponKillsCreateDeathNova()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.StartRun();
            yield return null;

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.SoulflareGlyphUpgradeId));
            Assert.That(controller.DeathNovaDamage, Is.GreaterThan(0f));
            Assert.That(controller.DeathNovaRadius, Is.GreaterThan(0f));

            Vector3 center = controller.PlayerPosition + new Vector3(5f, 0f, 0f);
            SurvivorsEnemyActor victim = controller.SpawnEnemyForTest(center, SurvivorsEnemyRole.Swarm, 1f);
            SurvivorsEnemyActor nearby = controller.SpawnEnemyForTest(center + new Vector3(1.1f, 0f, 0f), SurvivorsEnemyRole.Swarm, 4.5f);
            SurvivorsEnemyActor far = controller.SpawnEnemyForTest(center + new Vector3(5f, 0f, 0f), SurvivorsEnemyRole.Swarm, 4.5f);

            victim.ApplyDamage(20f, "weapon.survivors.arcane-wand");
            yield return null;

            Assert.AreEqual(1, controller.DeathNovaTriggerCount);
            Assert.AreEqual(1, controller.DeathNovaHitCount);
            Assert.AreEqual(2, controller.KilledCount);
            Assert.IsFalse(nearby.IsAlive);
            Assert.IsTrue(far.IsAlive);

            SurvivorsEnemyActor statusVictim = controller.SpawnEnemyForTest(center + new Vector3(0f, 0f, 6f), SurvivorsEnemyRole.Swarm, 1f);
            SurvivorsEnemyActor statusNearby = controller.SpawnEnemyForTest(center + new Vector3(1.1f, 0f, 6f), SurvivorsEnemyRole.Swarm, 4.5f);
            statusVictim.ApplyDamage(20f, "survivors.status.poison");
            yield return null;

            Assert.AreEqual(1, controller.DeathNovaTriggerCount);
            Assert.AreEqual(1, controller.DeathNovaHitCount);
            Assert.IsTrue(statusNearby.IsAlive);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator PlayerDamageShowsFeedbackAndLowHealthWarning()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.StartingBarrierCapacity = 0f;
            controller.StartRun();
            yield return null;

            Assert.IsFalse(controller.IsLowHealthWarningActive);

            controller.ApplyDamageToPlayer(controller.MaxHealth * 0.78f, "test.player-feedback");
            yield return null;

            Assert.AreEqual(1, controller.PlayerDamageFeedbackCount);
            Assert.AreEqual(1, controller.DamagePopupSpawnCount);
            Assert.AreEqual(1, controller.ActiveDamagePopupCount);
            Assert.IsTrue(controller.IsLowHealthWarningActive);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ArcStepDashMovesShovesAndBrieflyProtectsPlayer()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.CurrentTuning.EnemyMoveSpeed = 0f;
            controller.CurrentTuning.EnemyContactDamage = 0f;
            controller.CurrentTuning.StartingBarrierCapacity = 0f;
            controller.CurrentTuning.DashDistance = 3f;
            controller.CurrentTuning.DashCooldownSeconds = 0.5f;
            controller.CurrentTuning.DashInvulnerabilitySeconds = 0.35f;
            controller.CurrentTuning.DashKnockbackRadius = 1.25f;
            controller.CurrentTuning.DashKnockbackDistance = 1.1f;
            controller.CurrentTuning.DashDamage = 1f;
            controller.StartRun();
            yield return null;

            Vector3 start = controller.PlayerPosition;
            SurvivorsEnemyActor enemy = controller.SpawnEnemyForTest(start + new Vector3(1.4f, 0f, 0f), SurvivorsEnemyRole.Swarm, 20f);
            Assert.NotNull(enemy);
            Vector3 enemyBefore = enemy.transform.position;

            Assert.IsTrue(controller.DashForTest(Vector2.right));
            yield return null;

            Assert.AreEqual(1, controller.DashUseCount);
            Assert.That(controller.PlayerPosition.x, Is.GreaterThan(start.x + 2.9f));
            Assert.That(controller.DashCooldownRemainingSeconds, Is.GreaterThan(0f));
            Assert.IsTrue(controller.IsPlayerSafetyActive);
            Assert.That(controller.PlayerSafetyRemainingSeconds, Is.GreaterThan(0f));
            Assert.That(controller.DashEnemyShoveCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.DashDamageHitCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(enemy.transform.position.x, Is.GreaterThan(enemyBefore.x));
            Assert.That(controller.LastDashFeedbackLabel, Does.Contain("Arc Step"));
            Assert.That(controller.ActiveStreakRewardFeedbackLabel, Does.Contain("Arc Step"));

            float healthBefore = controller.CurrentHealth;
            controller.ApplyDamageToPlayer(9f, "test.arc-step.safety");
            Assert.AreEqual(healthBefore, controller.CurrentHealth);

            Assert.IsFalse(controller.DashForTest(Vector2.right));
            controller.Simulate(0.55f);
            yield return null;

            Assert.That(controller.DashCooldownRemainingSeconds, Is.EqualTo(0f).Within(0.01f));
            Assert.IsFalse(controller.IsPlayerSafetyActive);
            Assert.IsTrue(controller.DashForTest(Vector2.right));
            Assert.AreEqual(2, controller.DashUseCount);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RangedEnemyAttackTelegraphsBeforeDamage()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.StartingBarrierCapacity = 0f;
            controller.CurrentTuning.EnemyMaximumAlive = 1;
            controller.CurrentTuning.EnemyRangedAttackWindupSeconds = 0.28f;
            controller.StartRun();

            SurvivorsEnemyActor spitter = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(4.5f, 0f, 0f), SurvivorsEnemyRole.Spitter, 50f);
            Assert.NotNull(spitter);
            Assert.AreEqual(0, controller.EnemyRangedAttackFeedbackCount);
            Assert.AreEqual(0, controller.ActiveEnemyRangedAttackFeedbackCount);

            controller.Simulate(0.8f);
            yield return null;

            Assert.AreEqual(0, controller.PlayerDamageFeedbackCount);
            Assert.AreEqual(1, controller.EnemyRangedAttackFeedbackCount);
            Assert.AreEqual(1, controller.ActiveEnemyRangedAttackFeedbackCount);

            controller.Simulate(0.2f);
            yield return null;

            Assert.AreEqual(0, controller.PlayerDamageFeedbackCount);
            Assert.AreEqual(1, controller.ActiveEnemyRangedAttackFeedbackCount);

            controller.Simulate(0.12f);
            yield return null;

            Assert.That(controller.PlayerDamageFeedbackCount, Is.GreaterThanOrEqualTo(1));

            controller.Simulate(0.1f);
            yield return null;

            Assert.AreEqual(0, controller.ActiveEnemyRangedAttackFeedbackCount);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RangedEnemyAttackCanBeDodgedDuringWindup()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.StartingBarrierCapacity = 0f;
            controller.CurrentTuning.EnemyMaximumAlive = 1;
            controller.CurrentTuning.EnemyRangedAttackWindupSeconds = 0.28f;
            controller.StartRun();

            SurvivorsEnemyActor spitter = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(4.5f, 0f, 0f), SurvivorsEnemyRole.Spitter, 50f);
            Assert.NotNull(spitter);

            controller.Simulate(0.8f);
            yield return null;

            Assert.AreEqual(1, controller.EnemyRangedAttackFeedbackCount);
            Assert.AreEqual(0, controller.PlayerDamageFeedbackCount);

            controller.Simulate(0.4f, Vector2.left);
            yield return null;

            Assert.AreEqual(0, controller.PlayerDamageFeedbackCount);
            Assert.That(Vector3.Distance(controller.PlayerPosition, spitter.transform.position), Is.GreaterThan(5.8f));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator HealthPickupRestoresPlayerAndClearsLowHealthWarning()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.StartingBarrierCapacity = 0f;
            controller.StartRun();

            controller.ApplyDamageToPlayer(controller.MaxHealth * 0.78f, "test.health-pickup");
            float damagedHealth = controller.CurrentHealth;
            Assert.IsTrue(controller.IsLowHealthWarningActive);

            controller.SpawnHealthForTest(controller.PlayerPosition + new Vector3(0.15f, 0f, 0.15f), Mathf.CeilToInt(controller.MaxHealth));
            controller.Simulate(0.2f);
            yield return null;

            Assert.AreEqual(1, controller.HealthPickupCollectedCount);
            Assert.That(controller.HealthRestoredByPickups, Is.GreaterThan(0f));
            Assert.That(controller.CurrentHealth, Is.GreaterThan(damagedHealth));
            Assert.IsFalse(controller.IsLowHealthWarningActive);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator BloodShardPickupAddsRunCurrencyBonus()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.StartRun();

            int startingBonus = controller.BonusBloodShardsEarnedThisRun;
            controller.SpawnBloodShardForTest(controller.PlayerPosition + new Vector3(0.15f, 0f, 0.15f), 3);
            controller.Simulate(0.2f);
            yield return null;

            Assert.AreEqual(1, controller.BloodShardPickupCollectedCount);
            Assert.AreEqual(3, controller.BloodShardsCollectedFromPickups);
            Assert.AreEqual(startingBonus + 3, controller.BonusBloodShardsEarnedThisRun);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator MagnetRecallPulsesExperienceGemFeedback()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.StartRun();
            yield return null;

            SurvivorsPickupActor gem = controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(6f, 0f, 0f), 1);
            Assert.IsNotNull(gem);
            float baseScale = gem.transform.localScale.x;

            controller.TriggerMagnetRecall();
            controller.Simulate(1f / 30f);
            yield return null;

            Assert.AreEqual(1, controller.MagnetRecallFeedbackCount);
            Assert.That(controller.PickupAttractionFeedbackCount, Is.GreaterThanOrEqualTo(1));
            Assert.IsTrue(gem.IsGlobalRecallActive);
            Assert.IsTrue(gem.HasShownAttractionFeedback);
            Assert.That(gem.transform.localScale.x, Is.GreaterThan(baseScale));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator PickupRangeUpgradeAffectsExistingExperienceGems()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.StartRun();
            yield return null;

            float startingRange = controller.CurrentPickupAttractRange;
            SurvivorsPickupActor gem = controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(startingRange + 0.45f, 0f, 0f), 1);
            Assert.IsNotNull(gem);

            controller.Simulate(0.15f);
            yield return null;

            Assert.IsFalse(gem.HasShownAttractionFeedback);
            Assert.AreEqual(0, controller.PickupAttractionFeedbackCount);

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.gem-magnet"));
            Assert.That(controller.CurrentPickupAttractRange, Is.GreaterThan(startingRange + 0.45f));

            controller.Simulate(1f / 30f);
            yield return null;

            Assert.IsTrue(gem.HasShownAttractionFeedback);
            Assert.That(controller.PickupAttractionFeedbackCount, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RapidExperiencePickupShowsGemRushFeedback()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.StartRun();
            yield return null;

            controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(0.15f, 0f, 0.15f), 1);
            controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(-0.15f, 0f, 0.12f), 1);
            controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(0.08f, 0f, -0.15f), 1);

            controller.Simulate(0.2f);
            yield return null;

            Assert.AreEqual(3, controller.ExperiencePickupFeedbackCount);
            Assert.AreEqual(3, controller.CurrentExperienceComboPickupCount);
            Assert.AreEqual(3, controller.CurrentExperienceComboAmount);
            Assert.That(controller.ExperienceComboFeedbackCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.LastExperienceComboFeedbackLabel, Does.Contain("Gem Rush"));
            Assert.That(controller.ActiveExperienceComboFeedbackLabel, Does.Contain("Gem Rush"));
            Assert.That(controller.ExperienceComboFeedbackRemainingSeconds, Is.GreaterThan(0f));

            controller.Simulate(1.5f);
            yield return null;

            Assert.AreEqual(0, controller.CurrentExperienceComboPickupCount);
            Assert.AreEqual(0, controller.CurrentExperienceComboAmount);
            Assert.IsEmpty(controller.ActiveExperienceComboFeedbackLabel);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ControllerUsesHumanPlaytestPacingByDefault()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.AreEqual(SurvivorsPacingProfile.HumanPlaytest, controller.CurrentPacingProfile);
            Assert.IsTrue(controller.IsHumanPlaytestPacing);
            Assert.IsFalse(controller.IsDebugFastPacing);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.That(controller.CurrentTuning.RewardSelectionTimeoutSeconds, Is.LessThanOrEqualTo(0f));
            Assert.That(controller.CurrentEnemySpawnIntervalSeconds, Is.InRange(0.9f, 1.4f));
            Assert.That(controller.CurrentEnemyMaximumAlive, Is.InRange(24, 40));
            Assert.AreEqual(1f, controller.CurrentEnemySpeedMultiplier);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ImportedPlaytestSceneStartsControllerInHumanPlaytest()
        {
            const string scenePath = "Assets/Samples/com.deucarian.template.game.survivors/Basic Survivors Game/Scenes/PLAYTEST_THIS_SCENE_Survivors_Game.unity";
#if UNITY_EDITOR
            if (!System.IO.File.Exists(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), scenePath)))
            {
                Assert.Ignore("Imported playtest scene is not present in this project.");
            }

            AsyncOperation sceneLoad = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                scenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            Assert.NotNull(sceneLoad, "Imported playtest scene could not be loaded by path.");
            while (!sceneLoad.isDone)
            {
                yield return null;
            }
#else
            const string sceneName = "PLAYTEST_THIS_SCENE_Survivors_Game";
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Assert.Ignore("Imported playtest scene is not in this project's build settings.");
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
#endif
            SurvivorsTemplateController controller = null;
            float deadline = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < deadline)
            {
                controller = Object.FindFirstObjectByType<SurvivorsTemplateController>();
                if (controller != null && controller.State == SurvivorsRunState.Playing)
                {
                    break;
                }

                yield return null;
            }

            Assert.NotNull(controller, "Imported playtest scene did not create a SurvivorsTemplateController.");
            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.AreEqual(SurvivorsPacingProfile.HumanPlaytest, controller.CurrentPacingProfile);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.That(controller.CurrentEnemySpawnIntervalSeconds, Is.InRange(0.9f, 1.4f));
            Assert.That(controller.CurrentEnemyMaximumAlive, Is.InRange(24, 40));

            for (int i = 0; i < 240 && controller.SpawnedCount <= 0; i++)
            {
                controller.Simulate(1f / 60f, Vector2.right);
                yield return null;
            }

            Assert.That(controller.SpawnedCount, Is.GreaterThan(0), "Imported playtest scene did not advance into horde spawning.");
            Assert.That(controller.ActiveEnemyCount, Is.GreaterThan(0));

            int startingLevel = controller.Level;
            controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(0.2f, 0f, 0.2f), controller.RequiredExperienceForNextLevel);
            for (int i = 0; i < 30 && controller.State != SurvivorsRunState.LevelUp; i++)
            {
                controller.Simulate(1f / 60f);
                yield return null;
            }

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State, "Imported playtest scene did not open a level-up draft after XP collection.");
            Assert.That(controller.Level, Is.GreaterThan(startingLevel));
            Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
            Assert.That(controller.RewardCardPresentationCount, Is.GreaterThanOrEqualTo(3));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator DebugFastPacingRequiresExplicitProfileSwitch()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            Assert.AreEqual(SurvivorsPacingProfile.HumanPlaytest, controller.CurrentPacingProfile);

            controller.ApplyPacingProfileForTest(SurvivorsPacingProfile.DebugFast);
            controller.StartRun();
            yield return null;

            SurvivorsTemplateTuning human = BasicSurvivorsGame.CreateDefaultTuning();
            Assert.AreEqual(SurvivorsPacingProfile.DebugFast, controller.CurrentPacingProfile);
            Assert.IsTrue(controller.IsDebugFastPacing);
            Assert.That(controller.CurrentTuning.EnemySpawnIntervalSeconds, Is.LessThan(human.EnemySpawnIntervalSeconds));
            Assert.That(controller.CurrentTuning.RewardSelectionTimeoutSeconds, Is.GreaterThan(human.RewardSelectionTimeoutSeconds));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator HumanPlaytestRewardChoicesWaitForPlayer()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            controller.ForceLevelUp();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.AreEqual(0f, controller.RewardSelectionRemainingSeconds);

            controller.Simulate(120f);
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.AreEqual(0, controller.SelectedUpgradeCount);
            Assert.AreEqual(0, controller.RewardAutoSelectCount);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator OpeningHumanPlaytestCanOpenFirstDraftWithinOneMinute()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            const float deltaTime = 0.1f;
            float elapsed = 0f;
            int steps = 0;
            while (elapsed < 60f && !controller.IsRunUpgradeDraftOpen)
            {
                Vector2 movement = new Vector2(Mathf.Sin(elapsed * 0.75f), Mathf.Cos(elapsed * 0.55f));
                controller.Simulate(deltaTime, movement);
                elapsed += deltaTime;
                steps++;
                if (steps % 10 == 0)
                {
                    yield return null;
                }
            }

            string openingState = $"elapsed={elapsed:0.0}s, kills={controller.KilledCount}, xpCollected={controller.ExperienceCollected}, xp={controller.Experience}/{controller.RequiredExperienceForNextLevel}, state={controller.State}";
            Assert.IsTrue(controller.IsRunUpgradeDraftOpen, openingState);
            Assert.That(elapsed, Is.LessThanOrEqualTo(60f), openingState);
            Assert.AreEqual(3, controller.CurrentDraftChoices.Count, openingState);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator OpeningHumanPlaytestCollectsFirstExperienceWithinSeconds()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            const float deltaTime = 0.1f;
            float elapsed = 0f;
            int steps = 0;
            while (elapsed < 20f && controller.ExperienceCollected <= 0)
            {
                Vector2 movement = new Vector2(Mathf.Sin(elapsed * 0.65f), Mathf.Cos(elapsed * 0.8f));
                controller.Simulate(deltaTime, movement);
                elapsed += deltaTime;
                steps++;
                if (steps % 10 == 0)
                {
                    yield return null;
                }
            }

            string openingState = $"elapsed={elapsed:0.0}s, kills={controller.KilledCount}, activePickups={controller.ActivePickupCount}, xpCollected={controller.ExperienceCollected}, state={controller.State}";
            Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(1), openingState);
            Assert.That(controller.ExperienceCollected, Is.GreaterThan(0), openingState);
            Assert.That(elapsed, Is.LessThanOrEqualTo(20f), openingState);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator SpawnTicksCreateHordePacksAndRespectMaxAlive()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 0.1f;
            controller.CurrentTuning.MinimumEnemySpawnIntervalSeconds = 0.05f;
            controller.CurrentTuning.EnemyMaximumAlive = 5;
            controller.CurrentTuning.EnemySpawnPackBaseCount = 3;
            controller.CurrentTuning.EnemySpawnPackMaxCount = 3;
            controller.CurrentTuning.EnemySpawnPackIncreaseEveryEscalations = 1;
            controller.CurrentTuning.EnemyContactDamage = 0f;
            controller.StartRun();

            Assert.AreEqual(3, controller.CurrentEnemySpawnPackSize);

            controller.Simulate(0.01f);
            yield return null;

            Assert.AreEqual(3, controller.SpawnedCount);
            Assert.AreEqual(3, controller.ActiveEnemyCount);

            controller.Simulate(0.11f);
            yield return null;

            Assert.AreEqual(5, controller.SpawnedCount);
            Assert.AreEqual(5, controller.ActiveEnemyCount);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator CrowdedSwarmEnemiesSeparateWithoutLosingPressure()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.CurrentTuning.EnemyMoveSpeed = 2f;
            controller.CurrentTuning.EnemyContactDamage = 0f;
            controller.CurrentTuning.EnemySeparationRadius = 1.35f;
            controller.CurrentTuning.EnemySeparationStrength = 2f;
            controller.CurrentTuning.EnemySeparationMaxNeighbors = 4;
            controller.CurrentTuning.ProjectileDamage = 0f;
            controller.CurrentTuning.OrbitDamage = 0f;
            controller.CurrentTuning.MeleeDamage = 0f;
            controller.CurrentTuning.BurstDamage = 0f;
            controller.CurrentTuning.HitscanDamage = 0f;
            controller.CurrentTuning.GrenadeDamage = 0f;
            controller.CurrentTuning.PlacedPayloadDamage = 0f;
            controller.StartRun();
            yield return null;

            Vector3 player = controller.PlayerPosition;
            Vector3 center = player + new Vector3(4f, 0f, 0f);
            SurvivorsEnemyActor upper = controller.SpawnEnemyForTest(center + new Vector3(0f, 0f, 0.05f), SurvivorsEnemyRole.Swarm, 30f);
            SurvivorsEnemyActor lower = controller.SpawnEnemyForTest(center + new Vector3(0f, 0f, -0.05f), SurvivorsEnemyRole.Swarm, 30f);
            Assert.NotNull(upper);
            Assert.NotNull(lower);

            float startingPairDistance = Vector3.Distance(upper.transform.position, lower.transform.position);
            float startingUpperPlayerDistance = Vector3.Distance(upper.transform.position, player);
            float startingLowerPlayerDistance = Vector3.Distance(lower.transform.position, player);

            for (int i = 0; i < 10; i++)
            {
                controller.Simulate(0.08f);
                yield return null;
            }

            float finalPairDistance = Vector3.Distance(upper.transform.position, lower.transform.position);
            Assert.That(finalPairDistance, Is.GreaterThan(startingPairDistance + 0.45f));
            Assert.That(Vector3.Distance(upper.transform.position, player), Is.LessThan(startingUpperPlayerDistance));
            Assert.That(Vector3.Distance(lower.transform.position, player), Is.LessThan(startingLowerPlayerDistance));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator TimedHordeRushWarnsAndSpawnsArenaBurst()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.CurrentTuning.EnemyMaximumAlive = 40;
            controller.CurrentTuning.EnemyContactDamage = 0f;
            controller.CurrentTuning.HordeRushFirstTimeSeconds = 0.5f;
            controller.CurrentTuning.HordeRushIntervalSeconds = 1.1f;
            controller.CurrentTuning.HordeRushWarningLeadSeconds = 0.25f;
            controller.CurrentTuning.HordeRushBaseEnemyCount = 6;
            controller.CurrentTuning.HordeRushEnemyCountIncreasePerRush = 2;
            controller.CurrentTuning.HordeRushMaxEnemyCount = 10;
            controller.CurrentTuning.HordeRushExtraAliveAllowance = 12;
            controller.CurrentTuning.HordeRushSpawnRadius = 5.5f;
            controller.CurrentTuning.HordeRushClearExperienceGemCount = 4;
            controller.CurrentTuning.HordeRushClearMagnetEveryRush = 1;
            controller.CurrentTuning.HordeRushClearBloodShardEveryRush = 1;
            controller.StartRun();

            Assert.AreEqual(0, controller.HordeRushSpawnCount);
            Assert.That(controller.NextHordeRushTimeSecondsForTest, Is.EqualTo(0.5f).Within(0.001f));

            controller.Simulate(0.3f);
            yield return null;

            Assert.IsTrue(controller.IsHordeRushWarningActive);
            Assert.AreEqual(1, controller.HordeRushWarningCount);
            Assert.That(controller.CurrentHordeRushWarningLabel, Does.Contain("HORDE RUSH"));
            Assert.AreEqual(0, controller.HordeRushSpawnCount);
            int activeBeforeRush = controller.ActiveEnemyCount;

            controller.Simulate(0.25f);
            yield return null;

            Assert.IsFalse(controller.IsHordeRushWarningActive);
            Assert.AreEqual(1, controller.HordeRushSpawnCount);
            Assert.AreEqual(6, controller.HordeRushEnemySpawnCount);
            Assert.AreEqual(activeBeforeRush + 6, controller.ActiveEnemyCount);
            Assert.AreEqual(6, controller.ActiveHordeRushEnemyCount);
            Assert.That(controller.ActiveRunnerCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.LastHordeRushFeedbackLabel, Does.Contain("Horde Rush"));
            Assert.That(controller.LastStreakRewardFeedbackLabel, Does.Contain("Horde Rush"));
            Assert.That(controller.NextHordeRushTimeSecondsForTest, Is.GreaterThan(controller.RunTimeSeconds));

            int pickupCountBeforeClear = controller.ActivePickupCount;
            Assert.AreEqual(6, controller.KillActiveHordeRushEnemiesForTest());
            yield return null;

            Assert.AreEqual(0, controller.ActiveHordeRushEnemyCount);
            Assert.AreEqual(activeBeforeRush, controller.ActiveEnemyCount);
            Assert.AreEqual(1, controller.HordeRushClearRewardCount);
            Assert.AreEqual(4, controller.HordeRushClearExperienceGemDropCount);
            Assert.AreEqual(2, controller.HordeRushClearSpecialDropCount);
            Assert.That(controller.ActivePickupCount, Is.GreaterThanOrEqualTo(pickupCountBeforeClear + 12));
            Assert.That(controller.LastHordeRushClearFeedbackLabel, Does.Contain("Cleared"));
            Assert.That(controller.LastHordeRushFeedbackLabel, Does.Contain("Cleared"));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator SpawnPackSizeScalesWithRunEscalation()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.CurrentTuning.EnemyMaximumAlive = 20;
            controller.CurrentTuning.EnemySpawnPackBaseCount = 1;
            controller.CurrentTuning.EnemySpawnPackMaxCount = 4;
            controller.CurrentTuning.EnemySpawnPackIncreaseEveryEscalations = 1;
            controller.CurrentTuning.RunEscalationIntervalSeconds = 1f;
            controller.CurrentTuning.EnemyContactDamage = 0f;
            controller.StartRun();

            Assert.AreEqual(1, controller.CurrentEnemySpawnPackSize);

            controller.Simulate(2.1f);
            yield return null;

            Assert.That(controller.RunEscalationLevel, Is.GreaterThanOrEqualTo(2));
            Assert.AreEqual(3, controller.CurrentEnemySpawnPackSize);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator LevelUpChoiceAutoSelectsAfterTimeout()
        {
            SurvivorsTemplateController controller = CreateController();
            controller.CurrentTuning.RewardSelectionTimeoutSeconds = 0.05f;
            yield return null;

            controller.ForceLevelUp();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.That(controller.RewardSelectionRemainingSeconds, Is.GreaterThan(0f));

            controller.Simulate(0.1f);
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.AreEqual(1, controller.SelectedUpgradeCount);
            Assert.AreEqual(1, controller.RewardAutoSelectCount);
            Assert.AreEqual(0f, controller.RewardSelectionRemainingSeconds);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator LevelUpDraftPausesCombatButKeepsSelectionTimeout()
        {
            SurvivorsTemplateController controller = CreateController();
            controller.CurrentTuning.RewardSelectionTimeoutSeconds = 2f;
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            yield return null;

            SurvivorsEnemyActor enemy = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(3f, 0f, 0f), 100f);
            Vector3 playerPosition = controller.PlayerPosition;
            Vector3 enemyPosition = enemy.transform.position;
            float runTime = controller.RunTimeSeconds;

            controller.ForceLevelUp();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            float timeoutStart = controller.RewardSelectionRemainingSeconds;

            controller.Simulate(0.5f, Vector2.right);
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.AreEqual(runTime, controller.RunTimeSeconds);
            Assert.That((controller.PlayerPosition - playerPosition).sqrMagnitude, Is.LessThan(0.0001f));
            Assert.That((enemy.transform.position - enemyPosition).sqrMagnitude, Is.LessThan(0.0001f));
            Assert.That(controller.RewardSelectionRemainingSeconds, Is.LessThan(timeoutStart));
            Assert.That(controller.RewardSelectionRemainingSeconds, Is.GreaterThan(0f));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator EmptyLevelUpDraftAutoSkipsAndResumes()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.DraftChoiceCount = 0;
            controller.StartRun();
            yield return null;

            int skipReward = controller.DraftSkipBloodShards;
            controller.ForceLevelUp();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.AreEqual(0, controller.PendingLevelUps);
            Assert.AreEqual(0, controller.CurrentDraftChoices.Count);
            Assert.AreEqual(0, controller.SelectedUpgradeCount);
            Assert.AreEqual(1, controller.DraftSkipCount);
            Assert.That(controller.BonusBloodShardsEarnedThisRun, Is.GreaterThanOrEqualTo(skipReward));
            Assert.That(controller.LastRewardSelectionFeedbackLabel, Does.Contain("Level Up skipped"));
            Assert.That(controller.ActiveRewardFeedbackLabel, Does.Contain("skipped"));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator EarlyLevelUpDraftSuppressesLegendaryRarity()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            controller.ForceLevelUp();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
            for (int i = 0; i < controller.CurrentDraftChoices.Count; i++)
            {
                Assert.AreNotEqual(RunUpgradeRarity.Legendary, controller.CurrentDraftChoices[i].Rarity);
            }

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator FateLensBiasesFutureDraftsTowardHigherRarities()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            int commonBefore = controller.GetNormalMidDraftRarityWeightForTest(RunUpgradeRarity.Common);
            int rareBefore = controller.GetNormalMidDraftRarityWeightForTest(RunUpgradeRarity.Rare);
            int epicBefore = controller.GetNormalMidDraftRarityWeightForTest(RunUpgradeRarity.Epic);
            int legendaryBefore = controller.GetNormalMidDraftRarityWeightForTest(RunUpgradeRarity.Legendary);

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.FateLensUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.FateLensUpgradeId));

            Assert.That(controller.DraftLuckBonus, Is.GreaterThan(0f));
            Assert.That(controller.GetNormalMidDraftRarityWeightForTest(RunUpgradeRarity.Common), Is.LessThan(commonBefore));
            Assert.That(controller.GetNormalMidDraftRarityWeightForTest(RunUpgradeRarity.Rare), Is.GreaterThan(rareBefore));
            Assert.That(controller.GetNormalMidDraftRarityWeightForTest(RunUpgradeRarity.Epic), Is.GreaterThan(epicBefore));
            Assert.AreEqual(legendaryBefore, controller.GetNormalMidDraftRarityWeightForTest(RunUpgradeRarity.Legendary));
            Assert.That(string.Join("\n", controller.DebugDescribeCurrentBuild()), Does.Contain("luck +"));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator MidLevelDraftLocksRareOrBetterFirstChoiceWhenEligible()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            controller.CurrentTuning.DraftMidRarityLevel = 1;
            controller.CurrentTuning.DraftLateRarityLevel = 99;
            controller.CurrentTuning.NormalMidCommonWeight = 1000;
            controller.CurrentTuning.NormalMidUncommonWeight = 1000;
            controller.CurrentTuning.NormalMidRareWeight = 1;
            controller.CurrentTuning.NormalMidEpicWeight = 1;
            controller.CurrentTuning.NormalMidLegendaryWeight = 0;

            controller.ForceLevelUp();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
            Assert.That((int)controller.CurrentDraftChoices[0].Rarity, Is.GreaterThanOrEqualTo((int)RunUpgradeRarity.Rare));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator LateLevelDraftLocksEpicOrBetterFirstChoiceWhenEligible()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            controller.CurrentTuning.DraftMidRarityLevel = 1;
            controller.CurrentTuning.DraftLateRarityLevel = 1;
            controller.CurrentTuning.NormalLateCommonWeight = 1000;
            controller.CurrentTuning.NormalLateUncommonWeight = 1000;
            controller.CurrentTuning.NormalLateRareWeight = 1000;
            controller.CurrentTuning.NormalLateEpicWeight = 1;
            controller.CurrentTuning.NormalLateLegendaryWeight = 0;

            controller.ForceLevelUp();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
            Assert.That((int)controller.CurrentDraftChoices[0].Rarity, Is.GreaterThanOrEqualTo((int)RunUpgradeRarity.Epic));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator DraftRerollRefreshesChoicesWithoutConsumingLevel()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            controller.ForceLevelUp();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.AreEqual(1, controller.PendingLevelUps);
            Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
            int startingRerolls = controller.DraftRerollsRemaining;

            Assert.IsTrue(controller.RerollCurrentDraft());
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.AreEqual(1, controller.PendingLevelUps);
            Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
            Assert.AreEqual(1, controller.DraftRerollCount);
            Assert.AreEqual(startingRerolls - 1, controller.DraftRerollsRemaining);
            Assert.AreEqual(0, controller.SelectedUpgradeCount);
            Assert.IsTrue(controller.SkipCurrentDraft());

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator DraftChoiceCardsShowCurrentAndNextRank()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.arcane-damage"));
            controller.ForceLevelUpWithLockedChoiceForTest("upgrade.survivors.arcane-damage");
            yield return null;

            int choiceIndex = IndexOfDraftChoice(controller, "upgrade.survivors.arcane-damage");
            Assert.That(choiceIndex, Is.GreaterThanOrEqualTo(0));
            string label = controller.GetCurrentDraftChoiceLabelForTest(choiceIndex);
            Assert.That(label, Does.Contain("Arcane Damage"));
            Assert.That(label, Does.Contain("Weapon Upgrade"));
            Assert.That(label, Does.Contain("Rank 1->2/8"));
            Assert.That(label, Does.Contain("Affects Wand"));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator DraftBanishRemovesChoiceFromFutureDrafts()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            controller.ForceLevelUpWithLockedChoiceForTest("upgrade.survivors.arcane-damage");
            yield return null;

            int arcaneChoiceIndex = IndexOfDraftChoice(controller, "upgrade.survivors.arcane-damage");
            Assert.That(arcaneChoiceIndex, Is.GreaterThanOrEqualTo(0));
            int startingBanishes = controller.DraftBanishesRemaining;

            Assert.IsTrue(controller.BanishDraftChoice(arcaneChoiceIndex));
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.AreEqual(1, controller.DraftBanishCount);
            Assert.AreEqual(startingBanishes - 1, controller.DraftBanishesRemaining);
            Assert.AreEqual(-1, IndexOfDraftChoice(controller, "upgrade.survivors.arcane-damage"));
            Assert.IsFalse(controller.ApplyUpgradeByIdForTest("upgrade.survivors.arcane-damage"));
            Assert.IsTrue(controller.SkipCurrentDraft());

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator DraftSkipGrantsSmallRunRewardAndResumes()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            controller.ForceLevelUp();
            yield return null;

            int skipReward = controller.DraftSkipBloodShards;
            Assert.That(skipReward, Is.GreaterThanOrEqualTo(1));
            Assert.IsTrue(controller.SkipCurrentDraft());
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.AreEqual(0, controller.PendingLevelUps);
            Assert.AreEqual(0, controller.SelectedUpgradeCount);
            Assert.AreEqual(1, controller.DraftSkipCount);
            Assert.That(controller.BonusBloodShardsEarnedThisRun, Is.GreaterThanOrEqualTo(skipReward));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RuntimeDebuggerActionsExposeBuildDraftEvolutionAndCurrency()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            long startingShards = controller.MetaBloodShards;
            Assert.IsTrue(controller.DebugGrantBloodShards(25));
            Assert.AreEqual(startingShards + 25L, controller.MetaBloodShards);

            SurvivorsEnemyActor elite = controller.DebugSpawnElite(3f);
            SurvivorsEnemyActor dreadElite = controller.DebugSpawnDreadElite(4f);
            SurvivorsEnemyActor boss = controller.DebugSpawnBoss(5f);
            Assert.IsNotNull(elite);
            Assert.IsNotNull(dreadElite);
            Assert.IsNotNull(boss);
            Assert.AreEqual(SurvivorsEnemyRole.Elite, elite.Role);
            Assert.AreEqual(SurvivorsEnemyRole.DreadElite, dreadElite.Role);
            Assert.AreEqual(SurvivorsEnemyRole.Boss, boss.Role);
            Assert.That(controller.ActiveEliteCount, Is.GreaterThanOrEqualTo(2));
            Assert.AreEqual(1, controller.ActiveBossCount);

            controller.CurrentTuning.HordeRushBaseEnemyCount = 3;
            controller.CurrentTuning.HordeRushMaxEnemyCount = 3;
            controller.CurrentTuning.HordeRushExtraAliveAllowance = 8;
            Assert.AreEqual(3, controller.DebugTriggerHordeRush());
            Assert.AreEqual(3, controller.ActiveHordeRushEnemyCount);
            Assert.That(controller.LastHordeRushFeedbackLabel, Does.Contain("Horde Rush"));
            Assert.AreEqual(3, controller.DebugClearActiveHordeRush());
            yield return null;
            Assert.AreEqual(0, controller.ActiveHordeRushEnemyCount);
            Assert.AreEqual(1, controller.HordeRushClearRewardCount);

            var buildLines = controller.DebugDescribeCurrentBuild();
            string buildSummary = string.Join("\n", buildLines);
            Assert.That(buildSummary, Does.Contain("Weapons"));
            Assert.That(buildSummary, Does.Contain("Stats:"));

            var evolutionLines = controller.DebugDescribeEligibleEvolutionPool();
            Assert.That(evolutionLines.Count, Is.GreaterThanOrEqualTo(1));

            controller.ForceLevelUp();
            yield return null;

            var draftLines = controller.DebugDescribeCurrentDraftPool();
            Assert.That(string.Join("\n", draftLines), Does.Contain("Level Up"));
            Assert.That(draftLines.Count, Is.GreaterThanOrEqualTo(4));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator BossRelicChoiceAutoSelectsAfterTimeout()
        {
            SurvivorsTemplateController controller = CreateController();
            controller.CurrentTuning.RewardSelectionTimeoutSeconds = 0.05f;
            yield return null;

            Assert.IsTrue(controller.OpenBossRelicDraftForTest());
            yield return null;

            Assert.IsTrue(controller.IsRelicChoiceOpen);
            Assert.That(controller.RewardSelectionRemainingSeconds, Is.GreaterThan(0f));

            controller.Simulate(0.1f);
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.AreEqual(1, controller.SelectedRelicCount);
            Assert.AreEqual(1, controller.RewardAutoSelectCount);
            Assert.AreEqual(0f, controller.RewardSelectionRemainingSeconds);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RuntimePresentationCreatesFeedbackParticlesAudioAndProjectileTrails()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.IsNotNull(GameObject.Find(FeedbackRootName));
            Assert.IsNotNull(GameObject.Find(SpawnPulseName));
            Assert.IsNotNull(GameObject.Find(FirePulseName));
            Assert.IsNotNull(GameObject.Find(KillPulseName));
            Assert.IsNotNull(GameObject.Find(PickupPulseName));
            Assert.IsNotNull(GameObject.Find(LevelUpPulseName));
            Assert.IsNotNull(GameObject.Find(BossPulseName));
            Assert.IsNotNull(GameObject.Find(FeedbackAudioName));
            Assert.IsNotNull(GameObject.Find(DirectionalLightName));
            Assert.That(Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Length, Is.GreaterThanOrEqualTo(6));
            Assert.That(Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length, Is.GreaterThanOrEqualTo(1));
            Assert.IsTrue(Array.Exists(Object.FindObjectsByType<Light>(FindObjectsSortMode.None), light => light.type == LightType.Directional && light.name == DirectionalLightName));

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.1f, 0f, 0f), 20f);
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Projectile));
            yield return null;

            Assert.That(Object.FindObjectsByType<TrailRenderer>(FindObjectsSortMode.None).Length, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator InfiniteArenaTilesFollowLongPlayerTravel()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.That(controller.InfiniteArenaTileCountForTest, Is.GreaterThanOrEqualTo(25));
            Vector3 firstTileBefore = controller.FirstInfiniteArenaTilePositionForTest;

            yield return SimulateFrames(controller, 240, Vector2.right);

            Assert.That(controller.PlayerPosition.x, Is.GreaterThan(18f));
            Assert.That(controller.ArenaPresentationCenterForTest.x, Is.EqualTo(controller.PlayerPosition.x).Within(0.05f));
            Assert.That(Mathf.Abs(controller.FirstInfiniteArenaTilePositionForTest.x - firstTileBefore.x), Is.GreaterThan(0.5f));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RoamingArenaTravelDropsRewardCaches()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.CurrentTuning.ExperienceRequiredBase = 999;
            controller.StartRun();
            yield return null;

            yield return SimulateFrames(controller, 660, Vector2.right);

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.That(controller.RoamingCacheDropCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(controller.RoamingCacheExperienceGemDropCount, Is.GreaterThanOrEqualTo(9));
            Assert.That(controller.RoamingCacheMagnetDropCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.LastRoamingCacheFeedbackLabel, Does.Contain("Roaming Cache"));
            Assert.That(controller.ActiveStreakRewardFeedbackLabel, Does.Contain("Roaming Cache"));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RoamingArenaCachesCanTriggerSpecialDropsAndAmbushes()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.CurrentTuning.EnemyMoveSpeed = 0f;
            controller.CurrentTuning.EnemyContactDamage = 0f;
            controller.CurrentTuning.ProjectileDamage = 0f;
            controller.CurrentTuning.OrbitDamage = 0f;
            controller.CurrentTuning.MeleeDamage = 0f;
            controller.CurrentTuning.BurstDamage = 0f;
            controller.CurrentTuning.HitscanDamage = 0f;
            controller.CurrentTuning.GrenadeDamage = 0f;
            controller.CurrentTuning.PlacedPayloadDamage = 0f;
            controller.CurrentTuning.ExperienceRequiredBase = 999;
            controller.CurrentTuning.RoamingCacheTravelInterval = 2f;
            controller.CurrentTuning.RoamingCacheExperienceGemCount = 1;
            controller.CurrentTuning.RoamingCacheMagnetInterval = 2;
            controller.CurrentTuning.RoamingCacheBloodShardInterval = 2;
            controller.CurrentTuning.RoamingCacheAmbushStartCache = 2;
            controller.CurrentTuning.RoamingCacheAmbushInterval = 1;
            controller.CurrentTuning.RoamingCacheAmbushBaseEnemyCount = 1;
            controller.CurrentTuning.RoamingCacheAmbushMaxEnemyCount = 2;
            controller.CurrentTuning.RoamingCacheAmbushExtraAliveAllowance = 4;
            controller.CurrentTuning.RoamingCacheAmbushRadius = 2f;
            controller.StartRun();
            yield return null;

            yield return SimulateFrames(controller, 90, Vector2.right);

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.That(controller.RoamingCacheDropCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(controller.RoamingCacheBloodShardDropCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.RoamingCacheAmbushCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.RoamingCacheAmbushEnemySpawnCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.ActiveRoamingCacheAmbushEnemyCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.LastRoamingCacheFeedbackLabel, Does.Contain("Roaming Cache"));
            Assert.That(controller.LastRoamingCacheFeedbackLabel, Does.Contain("Ambush"));

            int killed = controller.KillActiveRoamingCacheAmbushEnemiesForTest();
            yield return null;

            Assert.That(killed, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(0, controller.ActiveRoamingCacheAmbushEnemyCount);
            Assert.AreEqual(1, controller.RoamingCacheAmbushClearRewardCount);
            Assert.That(controller.RoamingCacheAmbushClearExperienceGemDropCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.LastRoamingCacheAmbushClearFeedbackLabel, Does.Contain("Roaming Ambush Cleared"));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator DeepRoamingCachesTriggerWayfinderSurge()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.CurrentTuning.EnemyMoveSpeed = 0f;
            controller.CurrentTuning.EnemyContactDamage = 0f;
            controller.CurrentTuning.ProjectileDamage = 0f;
            controller.CurrentTuning.OrbitDamage = 0f;
            controller.CurrentTuning.MeleeDamage = 0f;
            controller.CurrentTuning.BurstDamage = 0f;
            controller.CurrentTuning.HitscanDamage = 0f;
            controller.CurrentTuning.GrenadeDamage = 0f;
            controller.CurrentTuning.PlacedPayloadDamage = 0f;
            controller.CurrentTuning.ExperienceRequiredBase = 999;
            controller.CurrentTuning.RoamingCacheTravelInterval = 2f;
            controller.CurrentTuning.RoamingCacheExperienceGemCount = 1;
            controller.CurrentTuning.RoamingCacheMagnetInterval = 0;
            controller.CurrentTuning.RoamingCacheBloodShardInterval = 0;
            controller.CurrentTuning.RoamingCacheAmbushStartCache = 99;
            controller.CurrentTuning.RoamingCacheSurgeInterval = 2;
            controller.CurrentTuning.RoamingCacheSurgeBonusGemCount = 2;
            controller.CurrentTuning.RoamingCacheSurgeDurationSeconds = 4f;
            controller.CurrentTuning.RoamingCacheSurgeDamageBonus = 2f;
            controller.CurrentTuning.RoamingCacheSurgeMoveSpeedBonus = 0.5f;
            controller.CurrentTuning.RoamingCacheSurgeCooldownMultiplierBonus = -0.1f;
            controller.CurrentTuning.RoamingCacheSurgePickupRangeBonus = 0.8f;
            controller.StartRun();
            yield return null;

            float startingDamage = controller.ProjectileDamage;
            float startingCooldown = controller.WeaponCooldownSeconds;
            float startingMoveSpeed = controller.PlayerMoveSpeed;
            float startingPickupRange = controller.CurrentPickupAttractRange;

            yield return SimulateFrames(controller, 90, Vector2.right);

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.That(controller.RoamingCacheDropCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(controller.RoamingCacheSurgeActivationCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.RoamingCacheSurgeBonusExperienceGemDropCount, Is.GreaterThanOrEqualTo(2));
            Assert.IsTrue(controller.IsRoamingCacheSurgeActive);
            Assert.That(controller.RoamingCacheSurgeRemainingSeconds, Is.GreaterThan(0f));
            Assert.That(controller.ProjectileDamage, Is.GreaterThan(startingDamage));
            Assert.That(controller.WeaponCooldownSeconds, Is.LessThan(startingCooldown));
            Assert.That(controller.PlayerMoveSpeed, Is.GreaterThan(startingMoveSpeed));
            Assert.That(controller.CurrentPickupAttractRange, Is.GreaterThan(startingPickupRange));
            Assert.That(controller.LastRoamingCacheFeedbackLabel, Does.Contain("Wayfinder Surge"));
            Assert.That(controller.LastRoamingCacheSurgeFeedbackLabel, Does.Contain("Wayfinder Surge"));
            Assert.That(controller.ActiveStreakRewardFeedbackLabel, Does.Contain("Wayfinder Surge"));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RapidKillsCreateStreakBonusDropsAndMagnet()
        {
            SurvivorsTemplateController controller = CreateController();
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            yield return null;

            controller.ApplyDamageToPlayer(controller.MaxHealth * 0.45f, "test.streak-health");
            Assert.That(controller.CurrentHealth, Is.LessThan(controller.MaxHealth));
            float startingDamage = controller.ProjectileDamage;
            float startingCooldown = controller.WeaponCooldownSeconds;
            float startingMoveSpeed = controller.PlayerMoveSpeed;
            float startingPickupRange = controller.CurrentPickupAttractRange;

            for (int i = 0; i < 32; i++)
            {
                SurvivorsEnemyActor enemy = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(7f + i * 0.08f, 0f, 0f), 1f);
                Assert.NotNull(enemy);
                enemy.ApplyDamage(100f, "test.streak");
            }

            Assert.AreEqual(32, controller.CurrentKillStreak);
            Assert.AreEqual(32, controller.BestKillStreak);
            Assert.That(controller.StreakBonusDropCount, Is.GreaterThanOrEqualTo(4));
            Assert.That(controller.StreakHealthDropCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.StreakMagnetDropCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.StreakBloodShardDropCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.StreakRewardFeedbackCount, Is.GreaterThanOrEqualTo(6));
            Assert.AreEqual(2, controller.StreakSurgeTier);
            Assert.AreEqual(2, controller.StreakSurgeActivationCount);
            Assert.IsTrue(controller.IsStreakSurgeActive);
            Assert.That(controller.StreakSurgeDamageBonus, Is.GreaterThan(0f));
            Assert.That(controller.ProjectileDamage, Is.GreaterThan(startingDamage));
            Assert.That(controller.WeaponCooldownSeconds, Is.LessThan(startingCooldown));
            Assert.That(controller.PlayerMoveSpeed, Is.GreaterThan(startingMoveSpeed));
            Assert.That(controller.CurrentPickupAttractRange, Is.GreaterThan(startingPickupRange));
            Assert.That(controller.LastStreakRewardFeedbackLabel, Does.Contain("Tempo Surge"));
            Assert.That(controller.ActiveStreakRewardFeedbackLabel, Does.Contain("Tempo Surge"));
            Assert.That(controller.StreakRewardFeedbackRemainingSeconds, Is.GreaterThan(0f));
            Assert.That(controller.ActivePickupCount, Is.GreaterThanOrEqualTo(37));

            controller.Simulate(6.5f);
            yield return null;

            Assert.AreEqual(0, controller.CurrentKillStreak);
            Assert.AreEqual(0, controller.StreakSurgeTier);
            Assert.IsFalse(controller.IsStreakSurgeActive);
            Assert.IsEmpty(controller.ActiveStreakRewardFeedbackLabel);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator PlayerCanDieAndRestart()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            controller.KillPlayerForTest();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.GameOver, controller.State);
            Assert.That(controller.LastRunSummaryLines.Count, Is.GreaterThanOrEqualTo(5));
            string defeatSummary = string.Join("\n", controller.LastRunSummaryLines);
            Assert.That(defeatSummary, Does.Contain("Defeat"));
            Assert.That(defeatSummary, Does.Contain("Rewards"));
            Assert.That(defeatSummary, Does.Contain("Meta"));

            controller.RestartRun();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.That(controller.CurrentHealth, Is.GreaterThan(0f));
            Assert.AreEqual(0, controller.LastRunSummaryLines.Count);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator DefaultClassStartsWithExpectedLoadout()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.AreEqual(BasicSurvivorsGame.DefaultClassId, controller.SelectedClassId);
            Assert.That(controller.ActiveWeaponCount, Is.GreaterThanOrEqualTo(5));
            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.ArcaneWandWeaponContentId));
            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.FrostFanWeaponContentId));
            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.OrbitWardWeaponContentId));
            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.ThornHaloWeaponContentId));
            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.StarNovaWeaponContentId));
            Assert.IsFalse(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.StarBeamWeaponContentId));
            Assert.IsTrue(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.FrostFanUnlockUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.FrostFanUnlockUpgradeId));
            Assert.AreEqual(SurvivorsRunUpgradeCategory.Weapon, controller.GetUpgradeCategoryForTest(BasicSurvivorsGame.FrostFanUnlockUpgradeId));
            Assert.IsTrue(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.OrbitWardUnlockUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.OrbitWardUnlockUpgradeId));
            Assert.IsTrue(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.StarNovaUnlockUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.StarNovaUnlockUpgradeId));
            Assert.IsFalse(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.MoonSlashUnlockUpgradeId));
            Assert.IsTrue(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.StarBeamUnlockUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.StarBeamUnlockUpgradeId));
            Assert.AreEqual(SurvivorsRunUpgradeCategory.Weapon, controller.GetUpgradeCategoryForTest(BasicSurvivorsGame.StarBeamUnlockUpgradeId));
            Assert.IsTrue(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.StarFocusUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.StarFocusUpgradeId));
            Assert.IsTrue(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator DraftedWeaponUnlockAddsLiveWeaponAndUnlocksItsPath()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.IsFalse(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.StarBeamWeaponContentId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.StarBeamUnlockUpgradeId));
            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.StarBeamWeaponContentId));
            Assert.AreEqual(controller.MaxWeaponSlots, controller.ActiveWeaponCount);
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.GravityGrenadeUnlockUpgradeId));
            Assert.IsFalse(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.GravityGrenadeUnlockUpgradeId));

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(3f, 0f, 0f), 1f);
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Hitscan));
            yield return null;

            Assert.That(controller.HitscanFireCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.HitscanHitCount, Is.GreaterThanOrEqualTo(1));

            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.StarFocusUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.StarPulseUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.StarFocusUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.StarFocusUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.StarPulseUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.StarFocusUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.StarPulseUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.StarPulseUpgradeId));

            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.TempestPrismEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.StarFocusUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.StarFocusUpgradeId));
            Assert.AreEqual(5, controller.GetRunUpgradeRankForTest(BasicSurvivorsGame.StarFocusUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.TempestPrismEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.TwinCharmUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.TempestPrismEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.TempestPrismEvolutionUpgradeId));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.TempestPrismEvolutionUpgradeId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator DraftedPayloadWeaponUnlockOpensPayloadPathAndEvolution()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.IsFalse(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.GravityGrenadeWeaponContentId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.ExtraPayloadUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.BiggerBoomsUpgradeId));

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.GravityGrenadeUnlockUpgradeId));
            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.GravityGrenadeWeaponContentId));
            Assert.AreEqual(controller.MaxWeaponSlots, controller.ActiveWeaponCount);
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.StarBeamUnlockUpgradeId));

            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.ExtraPayloadUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ExtraPayloadUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.BiggerBoomsUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ExtraPayloadUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.BiggerBoomsUpgradeId));

            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.BiggerBoomsUpgradeId));
            }

            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.GravefieldEngineEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.GiantRuneUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.GravefieldEngineEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.GravefieldEngineEvolutionUpgradeId));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.GravefieldEngineEvolutionUpgradeId));

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(0.8f, 0f, 0f), 100f);
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Grenade));
            yield return SimulateFrames(controller, 80);

            Assert.That(controller.PayloadCountBonus, Is.GreaterThanOrEqualTo(4));
            Assert.That(controller.PayloadExplosionRadiusBonus, Is.GreaterThan(0f));
            Assert.That(controller.PayloadDetonationCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.PayloadExplosionHitCount, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator PassiveSlotsBlockNewPassivesButAllowOwnedRanks()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            string[] passives =
            {
                "upgrade.survivors.swift-steps",
                "upgrade.survivors.gem-magnet",
                "upgrade.survivors.iron-blood",
                "upgrade.survivors.distilled-poison",
                "upgrade.survivors.hemorrhage-edge",
                "upgrade.survivors.execution-rite"
            };

            for (int i = 0; i < passives.Length; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(passives[i]));
            }

            Assert.AreEqual(controller.MaxPassiveSlots, controller.ActivePassiveCount);
            Assert.AreEqual(SurvivorsRunUpgradeCategory.PassiveUpgrade, controller.GetUpgradeCategoryForTest("upgrade.survivors.swift-steps"));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest("upgrade.survivors.sanguine-feast"));
            Assert.IsFalse(controller.ApplyUpgradeByIdForTest("upgrade.survivors.sanguine-feast"));

            int previousRank = controller.GetRunUpgradeRankForTest("upgrade.survivors.swift-steps");
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest("upgrade.survivors.swift-steps"));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.swift-steps"));
            Assert.AreEqual(controller.MaxPassiveSlots, controller.ActivePassiveCount);
            Assert.AreEqual(previousRank + 1, controller.GetRunUpgradeRankForTest("upgrade.survivors.swift-steps"));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator TunedSlotLimitsBlockNewSlotsButAllowOwnedRanks()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            controller.CurrentTuning.MaxWeaponSlots = controller.ActiveWeaponCount;
            Assert.AreEqual(controller.ActiveWeaponCount, controller.MaxWeaponSlots);
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.StarBeamUnlockUpgradeId));
            Assert.IsFalse(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.StarBeamUnlockUpgradeId));

            controller.CurrentTuning.MaxPassiveSlots = 1;
            Assert.AreEqual(1, controller.MaxPassiveSlots);
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.swift-steps"));
            Assert.AreEqual(1, controller.ActivePassiveCount);
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest("upgrade.survivors.gem-magnet"));
            Assert.IsFalse(controller.ApplyUpgradeByIdForTest("upgrade.survivors.gem-magnet"));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest("upgrade.survivors.swift-steps"));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.swift-steps"));
            Assert.AreEqual(1, controller.ActivePassiveCount);
            Assert.AreEqual(2, controller.GetRunUpgradeRankForTest("upgrade.survivors.swift-steps"));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator WeaponEvolutionRequiresRankedWeaponPathAndPassive()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.AreEqual(SurvivorsRunUpgradeCategory.Evolution, controller.GetUpgradeCategoryForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            Assert.That(controller.GetUpgradeDescriptionForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId), Does.Contain("Evolution"));
            Assert.AreEqual(0, controller.EvolutionReadyFeedbackCount);

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.arcane-damage"));
            }

            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            Assert.AreEqual(0, controller.EvolutionReadyFeedbackCount);
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ArcaneThesisUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            Assert.AreEqual(1, controller.EvolutionReadyFeedbackCount);
            Assert.That(controller.LastEvolutionReadyFeedbackLabel, Does.Contain("Evolution Ready"));
            Assert.That(controller.LastEvolutionReadyFeedbackLabel, Does.Contain("Arcane Storm"));
            Assert.That(controller.ActiveEvolutionReadyFeedbackLabel, Does.Contain("Arcane Storm"));
            Assert.That(controller.EvolutionReadyFeedbackRemainingSeconds, Is.GreaterThan(0f));

            float previousDamage = controller.DamageBonus;
            int previousChains = controller.ProjectileChainBonus;
            int previousForks = controller.ProjectileForkBonus;
            controller.CurrentTuning.EvolutionSurgeDamage = 12f;
            controller.CurrentTuning.EvolutionSurgeRadius = 4f;
            SurvivorsEnemyActor surgeTargetA = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(1.4f, 0f, 0f), SurvivorsEnemyRole.Swarm, 5f);
            SurvivorsEnemyActor surgeTargetB = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(0f, 0f, 2.2f), SurvivorsEnemyRole.Runner, 5f);
            SurvivorsEnemyActor protectedElite = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(1.8f, 0f, 1.6f), SurvivorsEnemyRole.Elite, 30f);
            SurvivorsEnemyActor outsideTarget = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(6f, 0f, 0f), SurvivorsEnemyRole.Swarm, 5f);
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            Assert.AreEqual(1, controller.EvolutionReadyFeedbackCount);

            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            Assert.AreEqual(1, controller.EvolvedWeaponCount);
            Assert.AreEqual(1, controller.WeaponEvolutionSurgeCount);
            Assert.AreEqual(2, controller.WeaponEvolutionSurgeHitCount);
            Assert.That(controller.LastWeaponEvolutionSurgeFeedbackLabel, Does.Contain("Arcane Storm"));
            Assert.IsFalse(surgeTargetA.IsAlive);
            Assert.IsFalse(surgeTargetB.IsAlive);
            Assert.IsTrue(protectedElite.IsAlive);
            Assert.IsTrue(outsideTarget.IsAlive);
            Assert.That(controller.DamageBonus, Is.GreaterThan(previousDamage));
            Assert.That(controller.ProjectileChainBonus, Is.GreaterThan(previousChains));
            Assert.That(controller.ProjectileForkBonus, Is.GreaterThan(previousForks));
            controller.Simulate(3f);
            Assert.IsEmpty(controller.ActiveEvolutionReadyFeedbackLabel);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ArcaneStormEvolutionAddsRadialBoltRing()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            ConfigureProjectileModifierOnlyTuning(controller.CurrentTuning);
            controller.StartRun();
            yield return null;

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(4f, 0f, 0f), 100f);
            int beforeBaselineFire = controller.ProjectileLaunchCount;
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Projectile));
            int baselineLaunches = controller.ProjectileLaunchCount - beforeBaselineFire;
            Assert.That(baselineLaunches, Is.GreaterThanOrEqualTo(1));

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.arcane-damage"));
            }

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ArcaneThesisUpgradeId));
            int beforeEvolutionFeedback = controller.WeaponEvolutionFeedbackCount;
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            Assert.AreEqual(beforeEvolutionFeedback + 1, controller.WeaponEvolutionFeedbackCount);

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(4.4f, 0f, 0f), 100f);
            int beforeStormFire = controller.ProjectileLaunchCount;
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Projectile));
            int evolvedLaunches = controller.ProjectileLaunchCount - beforeStormFire;

            Assert.That(evolvedLaunches, Is.GreaterThanOrEqualTo(baselineLaunches + 8));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator BlizzardCrownEvolutionAddsRadialShardCrown()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            ConfigureProjectileModifierOnlyTuning(controller.CurrentTuning);
            controller.StartRun();
            yield return null;

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.FrostFanUpgradeId));
            }

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.FrostNeedleworkUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId));

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(4f, 0f, 0f), 100f);
            int beforeBaselineFire = controller.ProjectileLaunchCount;
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Projectile));
            int baselineLaunches = controller.ProjectileLaunchCount - beforeBaselineFire;
            Assert.That(baselineLaunches, Is.GreaterThanOrEqualTo(1));

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId));

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(4.4f, 0f, 0f), 100f);
            int beforeCrownFire = controller.ProjectileLaunchCount;
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Projectile));
            int evolvedLaunches = controller.ProjectileLaunchCount - beforeCrownFire;

            Assert.That(evolvedLaunches, Is.GreaterThanOrEqualTo(baselineLaunches + 10));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator InfernoHeartEvolutionAddsSatelliteNovaCoverage()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            ConfigureBurstOnlyTuning(controller.CurrentTuning);
            controller.StartRun();
            yield return null;

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.NovaEchoUpgradeId));
            }

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.CinderScriptUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.InfernoHeartEvolutionUpgradeId));

            float radius = controller.CurrentTuning.BurstRadius + controller.AreaRadiusBonus;
            Vector3 anchorPosition = controller.PlayerPosition + new Vector3(radius * 0.55f, 0f, 0f);
            Vector3 outsideCentralBurst = anchorPosition + new Vector3(radius * 1.18f, 0f, 0f);
            SurvivorsEnemyActor anchor = controller.SpawnEnemyForTest(anchorPosition, 100f);
            SurvivorsEnemyActor satelliteTarget = controller.SpawnEnemyForTest(outsideCentralBurst, 100f);
            Assert.NotNull(anchor);
            Assert.NotNull(satelliteTarget);

            int beforeBaselineHits = controller.BurstHitCount;
            int beforeBaselinePulses = controller.BurstPulseCount;
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Burst));
            int baselineHits = controller.BurstHitCount - beforeBaselineHits;
            int baselinePulses = controller.BurstPulseCount - beforeBaselinePulses;
            Assert.AreEqual(1, baselineHits);
            Assert.AreEqual(1, baselinePulses);

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.InfernoHeartEvolutionUpgradeId));

            int beforeInfernoHits = controller.BurstHitCount;
            int beforeInfernoPulses = controller.BurstPulseCount;
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Burst));
            int infernoHits = controller.BurstHitCount - beforeInfernoHits;
            int infernoPulses = controller.BurstPulseCount - beforeInfernoPulses;

            Assert.That(infernoHits, Is.GreaterThanOrEqualTo(2));
            Assert.That(infernoPulses, Is.GreaterThanOrEqualTo(baselinePulses + 6));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.InfernoHeartEvolutionUpgradeId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator TempestPrismEvolutionAddsPrismSideBeams()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            ConfigureHitscanOnlyTuning(controller.CurrentTuning);
            controller.StartRun();
            yield return null;

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.StarBeamUnlockUpgradeId));
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.StarFocusUpgradeId));
            }

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.TwinCharmUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.TempestPrismEvolutionUpgradeId));

            Vector3 forwardTarget = controller.PlayerPosition + new Vector3(3.8f, 0f, 0f);
            Vector3 sideDirection = Quaternion.Euler(0f, 22f, 0f) * Vector3.right;
            Vector3 sideTarget = controller.PlayerPosition + sideDirection * 4.8f;
            SurvivorsEnemyActor anchor = controller.SpawnEnemyForTest(forwardTarget, 100f);
            SurvivorsEnemyActor prismTarget = controller.SpawnEnemyForTest(sideTarget, 100f);
            Assert.NotNull(anchor);
            Assert.NotNull(prismTarget);

            int beforeBaselineHits = controller.HitscanHitCount;
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Hitscan));
            int baselineHits = controller.HitscanHitCount - beforeBaselineHits;
            Assert.AreEqual(1, baselineHits);

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.TempestPrismEvolutionUpgradeId));

            int beforePrismHits = controller.HitscanHitCount;
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Hitscan));
            int prismHits = controller.HitscanHitCount - beforePrismHits;

            Assert.That(prismHits, Is.GreaterThanOrEqualTo(2));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.TempestPrismEvolutionUpgradeId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator CrimsonAegisEvolutionAddsCounterRotatingShieldRing()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.OrbitingFocusUpgradeId));
            }

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.BloodRingCanticleUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId));

            controller.Simulate(1f / 60f);
            yield return null;
            int beforeEvolutionCount = controller.ActiveOrbitBladeCount;
            Assert.That(beforeEvolutionCount, Is.GreaterThanOrEqualTo(2));

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId));

            controller.Simulate(1f / 60f);
            yield return null;
            int evolvedCount = controller.ActiveOrbitBladeCount;

            Assert.That(evolvedCount, Is.GreaterThanOrEqualTo(beforeEvolutionCount + 10));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator EclipseWaltzEvolutionAddsBackSweep()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            ConfigureMeleeOnlyTuning(controller.CurrentTuning);
            Assert.IsTrue(controller.UnlockClassForTest(BasicSurvivorsGame.EmberVanguardClassId));
            Assert.IsTrue(controller.TrySelectClassForTest(BasicSurvivorsGame.EmberVanguardClassId));
            controller.StartRun();
            yield return null;

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.MoonlitEdgeUpgradeId));
            }

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.MoonOathUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.EclipseWaltzEvolutionUpgradeId));

            SurvivorsEnemyActor forwardTarget = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(1.65f, 0f, 0f), 100f);
            SurvivorsEnemyActor backTarget = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(-2f, 0f, 0f), 100f);
            Assert.NotNull(forwardTarget);
            Assert.NotNull(backTarget);

            int beforeBaselineHits = controller.MeleeHitCount;
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Melee));
            int baselineHits = controller.MeleeHitCount - beforeBaselineHits;
            Assert.AreEqual(1, baselineHits);

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.EclipseWaltzEvolutionUpgradeId));

            int beforeWaltzHits = controller.MeleeHitCount;
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Melee));
            int waltzHits = controller.MeleeHitCount - beforeWaltzHits;

            Assert.That(waltzHits, Is.GreaterThanOrEqualTo(2));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.EclipseWaltzEvolutionUpgradeId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator DefaultWeaponPathsRequireFiveRanksBeforeEvolution()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.FrostSplinterUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.FrostRicochetUpgradeId));
            for (int i = 0; i < 2; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.FrostFanUpgradeId));
            }

            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.FrostSplinterUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.FrostRicochetUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.FrostFanUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.FrostRicochetUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.FrostSplinterUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.FrostRicochetUpgradeId));

            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.FrostFanUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.FrostFanUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.FrostNeedleworkUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId));

            int previousFan = controller.ProjectileFanBonus;
            int previousPierce = controller.ProjectilePierceBonus;
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId));
            Assert.That(controller.ProjectileFanBonus, Is.GreaterThan(previousFan));
            Assert.That(controller.ProjectilePierceBonus, Is.GreaterThan(previousPierce));

            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.CinderEchoUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.TargetedSigilUpgradeId));
            for (int i = 0; i < 2; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.NovaEchoUpgradeId));
            }

            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.CinderEchoUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.TargetedSigilUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.NovaEchoUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.TargetedSigilUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.CinderEchoUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.TargetedSigilUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.InfernoHeartEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.NovaEchoUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.NovaEchoUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.InfernoHeartEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.CinderScriptUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.InfernoHeartEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.InfernoHeartEvolutionUpgradeId));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.InfernoHeartEvolutionUpgradeId));

            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.SerratedOrbitUpgradeId));
            for (int i = 0; i < 2; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.OrbitingFocusUpgradeId));
            }

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.OrbitingFocusUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.SerratedOrbitUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.SerratedOrbitUpgradeId));

            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.BrambleGuardUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.ThornHaloUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.HaloSpiralUpgradeId));
            for (int i = 0; i < 2; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.BrambleGuardUpgradeId));
            }

            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.ThornHaloUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.HaloSpiralUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.BrambleGuardUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.HaloSpiralUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ThornHaloUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.HaloSpiralUpgradeId));

            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.OrbitingFocusUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.OrbitingFocusUpgradeId));
            Assert.AreEqual(5, controller.GetRunUpgradeRankForTest(BasicSurvivorsGame.OrbitingFocusUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.BloodRingCanticleUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId));

            int previousOrbitBlades = controller.OrbitBladeBonus;
            float previousOrbitRadius = controller.OrbitRadiusBonus;
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId));
            Assert.That(controller.OrbitBladeBonus, Is.GreaterThan(previousOrbitBlades));
            Assert.That(controller.OrbitRadiusBonus, Is.GreaterThan(previousOrbitRadius));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator OrbitWeaponCanDamageAndKillEnemy()
        {
            SurvivorsTemplateController controller = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            yield return null;

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.1f, 0f, 0f), 1f);
            for (int i = 0; i < 20; i++)
            {
                controller.Simulate(1f / 60f);
                yield return null;
            }

            Assert.That(controller.ActiveOrbitBladeCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.OrbitHitCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator MeleeAndBurstWeaponsCanDamageAndKillEnemies()
        {
            SurvivorsTemplateController controller = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            yield return null;

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(1.4f, 0f, 0f), 1f);
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Melee));
            yield return null;

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2f, 0f, 0f), 1f);
            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Burst));
            yield return null;

            Assert.That(controller.MeleeSwingCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.MeleeHitCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.BurstPulseCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.BurstHitCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(2));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RunUpgradeCanAffectNewWeaponArchetype()
        {
            SurvivorsTemplateController controller = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            yield return null;

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.1f, 0f, 0f), 20f);
            controller.Simulate(1f / 60f);
            yield return null;
            int baseline = controller.ActiveOrbitBladeCount;

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.orbiting-focus"));
            controller.Simulate(1f / 60f);
            yield return null;

            Assert.That(baseline, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.OrbitBladeBonus, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.ActiveOrbitBladeCount, Is.GreaterThan(baseline));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator HitscanWeaponCanDamageAndKillEnemy()
        {
            SurvivorsTemplateController controller = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            yield return null;

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(3f, 0f, 0f), 1f);

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Hitscan));
            yield return null;

            Assert.That(controller.HitscanFireCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.HitscanHitCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator PiercingProjectileCanHitMultipleEnemies()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.piercing-bolts"));
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(3.6f, 0f, 0f), 1f);
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(4.25f, 0f, 0f), 1f);

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Projectile));
            yield return SimulateFrames(controller, 90);

            Assert.That(controller.ProjectilePierceBonus, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.ProjectilePierceHitCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(2));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator FastProjectileDamagesEnemyCrossedBetweenFrames()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.CurrentTuning.ProjectileSpeed = 80f;
            controller.CurrentTuning.ProjectileRadius = 0.12f;
            controller.StartRun();
            yield return null;

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(4f, 0f, 0f), 1f);

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Projectile));
            controller.Simulate(0.1f);
            yield return null;

            Assert.That(controller.ProjectileLaunchCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ChainedProjectileCanRetargetNearbyEnemy()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.chain-bolts"));
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(1.4f, 0f, 0f), 30f);
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(1.4f, 0f, 1.15f), 30f);

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Projectile));
            yield return SimulateFrames(controller, 120);

            Assert.That(controller.ProjectileChainBonus, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.ProjectileChainHitCount, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ForkedProjectileCanSpawnSecondaryProjectile()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            ConfigureProjectileModifierOnlyTuning(controller.CurrentTuning);
            controller.StartRun();
            yield return null;

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.forked-bolts"));
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(3.6f, 0f, 0f), 1f);
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(3.6f, 0f, 1.2f), 1f);

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Projectile));
            yield return SimulateFrames(controller, 120);

            Assert.That(controller.ProjectileForkBonus, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.ProjectileForkSpawnCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(2));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ReturningProjectileCanBoomerangAfterHit()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            ConfigureProjectileModifierOnlyTuning(controller.CurrentTuning);
            controller.StartRun();
            yield return null;

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.returning-bolts"));
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.1f, 0f, 0f), 1f);

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Projectile));
            yield return SimulateFrames(controller, 90);

            Assert.That(controller.ProjectileReturnBonus, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.ProjectileReturnStartCount, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator GrenadePayloadCanDetonateDamageAndKillEnemy()
        {
            SurvivorsTemplateController controller = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            yield return null;

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(0.8f, 0f, 0f), 100f);

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Grenade));
            yield return SimulateFrames(controller, 80);

            Assert.That(controller.PayloadThrowCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.PayloadDetonationCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.PayloadExplosionHitCount, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator TrapPayloadCanArmTriggerAndKillEnemy()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            ConfigureTrapOnlyTuning(controller.CurrentTuning);
            Assert.IsTrue(controller.UnlockClassForTest(BasicSurvivorsGame.EmberVanguardClassId));
            Assert.IsTrue(controller.TrySelectClassForTest(BasicSurvivorsGame.EmberVanguardClassId));
            controller.StartRun();
            yield return null;

            SurvivorsEnemyActor enemy = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(6f, 0f, 0f), SurvivorsEnemyRole.Bruiser, 6.5f);

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Trap));
            yield return SimulateFrames(controller, 80);

            string payloadState = $"placed={controller.PayloadPlacedCount}, detonations={controller.PayloadDetonationCount}, hits={controller.PayloadExplosionHitCount}, kills={controller.KilledCount}, activeEnemies={controller.ActiveEnemyCount}, enemyHealth={(enemy == null ? -1f : enemy.CurrentHealth)}";
            Assert.That(controller.PayloadPlacedCount, Is.GreaterThanOrEqualTo(1), payloadState);
            Assert.That(controller.PayloadDetonationCount, Is.GreaterThanOrEqualTo(1), payloadState);
            Assert.That(controller.PayloadExplosionHitCount, Is.GreaterThanOrEqualTo(1), payloadState);
            Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(1), payloadState);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator PayloadUpgradeCanAffectPayloadBehavior()
        {
            SurvivorsTemplateController controller = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            yield return null;

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ExtraPayloadUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ExtraPayloadUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.BiggerBoomsUpgradeId));
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(0.8f, 0f, 0f), 100f);

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Grenade));
            yield return SimulateFrames(controller, 80);

            Assert.That(controller.PayloadExplosionRadiusBonus, Is.GreaterThan(0f));
            Assert.That(controller.PayloadDetonationCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.PayloadExplosionHitCount, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator GravefieldEngineEvolutionMakesGravityGrenadeLeaveSatelliteHazards()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            ConfigureGrenadeOnlyTuning(controller.CurrentTuning);
            controller.StartRun();
            yield return null;

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.GravityGrenadeUnlockUpgradeId));
            for (int i = 0; i < 2; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ExtraPayloadUpgradeId));
            }

            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.BiggerBoomsUpgradeId));
            }

            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.GravefieldEngineEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.GiantRuneUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.GravefieldEngineEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.GravefieldEngineEvolutionUpgradeId));

            int hazardsBefore = Object.FindObjectsByType<SurvivorsPayloadHazardActor>(FindObjectsSortMode.None).Length;
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(0.8f, 0f, 0f), 1000f);

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Grenade));
            yield return SimulateFrames(controller, 95);

            int hazardsAfter = Object.FindObjectsByType<SurvivorsPayloadHazardActor>(FindObjectsSortMode.None).Length;
            Assert.That(hazardsAfter - hazardsBefore, Is.GreaterThanOrEqualTo(7));
            Assert.That(controller.PayloadDetonationCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.PayloadHazardTickCount, Is.GreaterThanOrEqualTo(1));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.GravefieldEngineEvolutionUpgradeId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator EmberPayloadHazardPathRequiresRanksBeforeEvolution()
        {
            SurvivorsTemplateController defaultController = CreateController();
            yield return null;

            Assert.IsFalse(defaultController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.RuneLatticeUpgradeId));
            Assert.IsFalse(defaultController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.SnaringRunesUpgradeId));
            Object.Destroy(defaultController.gameObject);
            yield return null;

            SurvivorsTemplateController controller = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            yield return null;

            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.RuneTrapWeaponContentId));
            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.AetherMineWeaponContentId));
            Assert.IsTrue(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.RuneLatticeUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.RuneLatticeUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.SnaringRunesUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.AetherBloomUpgradeId));

            for (int i = 0; i < 2; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.RuneLatticeUpgradeId));
            }

            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.SnaringRunesUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.AetherBloomUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.RuneLatticeUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.AetherBloomUpgradeId));

            Assert.That(controller.PayloadCountBonus, Is.GreaterThanOrEqualTo(3));
            float previousTrigger = controller.PayloadTriggerRadiusBonus;
            float previousRadius = controller.PayloadExplosionRadiusBonus;
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.SnaringRunesUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.AetherBloomUpgradeId));
            Assert.That(controller.PayloadTriggerRadiusBonus, Is.GreaterThan(previousTrigger));
            Assert.That(controller.PayloadExplosionRadiusBonus, Is.GreaterThan(previousRadius));

            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.AetherfieldMatrixEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.RuneLatticeUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.RuneLatticeUpgradeId));
            Assert.AreEqual(5, controller.GetRunUpgradeRankForTest(BasicSurvivorsGame.RuneLatticeUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.AetherfieldMatrixEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.SiegePayloadsUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.AetherfieldMatrixEvolutionUpgradeId));

            int previousPayloads = controller.PayloadCountBonus;
            previousTrigger = controller.PayloadTriggerRadiusBonus;
            previousRadius = controller.PayloadExplosionRadiusBonus;
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.AetherfieldMatrixEvolutionUpgradeId));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.AetherfieldMatrixEvolutionUpgradeId));
            Assert.That(controller.PayloadCountBonus, Is.GreaterThan(previousPayloads));
            Assert.That(controller.PayloadTriggerRadiusBonus, Is.GreaterThan(previousTrigger));
            Assert.That(controller.PayloadExplosionRadiusBonus, Is.GreaterThan(previousRadius));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator AetherfieldMatrixEvolutionMakesAetherMineLeaveHazards()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            ConfigureTrapOnlyTuning(controller.CurrentTuning);
            Assert.IsTrue(controller.UnlockClassForTest(BasicSurvivorsGame.EmberVanguardClassId));
            Assert.IsTrue(controller.TrySelectClassForTest(BasicSurvivorsGame.EmberVanguardClassId));
            controller.StartRun();
            yield return null;

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.RuneLatticeUpgradeId));
            }

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.SiegePayloadsUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.AetherfieldMatrixEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.AetherfieldMatrixEvolutionUpgradeId));

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(0f, 0f, 1.8f), 100f);

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Mine));
            yield return SimulateFrames(controller, 150);

            Assert.That(controller.PayloadDetonationCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.PayloadHazardTickCount, Is.GreaterThanOrEqualTo(1));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.AetherfieldMatrixEvolutionUpgradeId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ExperienceAndAreaPassivesCreateReadablePowerGains()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.GiantRuneUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.GiantRuneUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.GiantRuneUpgradeId));
            Assert.That(controller.AreaRadiusBonus, Is.GreaterThan(0.5f));

            float baseBurstRadius = controller.CurrentTuning.BurstRadius;
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(1f, 0f, 0f), 100f);
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(baseBurstRadius + controller.AreaRadiusBonus - 0.08f, 0f, 0f), 100f);
            int hitsBeforeBurst = controller.BurstHitCount;

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Burst));
            yield return null;

            Assert.That(controller.BurstHitCount - hitsBeforeBurst, Is.GreaterThanOrEqualTo(2));

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ScholarsLensUpgradeId));
            Assert.That(controller.ExperienceGainMultiplierBonus, Is.GreaterThan(0f));

            int experienceBeforePickup = controller.ExperienceCollected;
            controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(0.15f, 0f, 0.15f), 10);
            yield return SimulateFrames(controller, 8);

            Assert.That(controller.ExperienceCollected - experienceBeforePickup, Is.GreaterThan(10));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator SplitterDeathSpawnsSwarmChildren()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.StartRun();
            yield return null;

            SurvivorsEnemyActor splitter = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(4f, 0f, 0f), SurvivorsEnemyRole.Splitter, 1f);
            Assert.AreEqual(1, controller.ActiveSplitterCount);
            int activeBeforeKill = controller.ActiveEnemyCount;

            splitter.ApplyDamage(100f, "test.splitter");
            yield return null;

            Assert.AreEqual(0, controller.ActiveSplitterCount);
            Assert.AreEqual(2, controller.SplitterChildSpawnCount);
            Assert.AreEqual(activeBeforeKill + 1, controller.ActiveEnemyCount);
            Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator SummonerPeriodicallyCallsSupportEnemies()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.CurrentTuning.EnemyMaximumAlive = 1;
            controller.CurrentTuning.SummonerSupportInitialDelaySeconds = 0.05f;
            controller.CurrentTuning.SummonerSupportIntervalSeconds = 0.2f;
            controller.CurrentTuning.SummonerSupportCount = 2;
            controller.CurrentTuning.SummonerSupportExtraAliveAllowance = 4;
            controller.StartRun();
            yield return null;

            int activeBeforeSummoner = controller.ActiveEnemyCount;
            int activeSummonersBefore = controller.ActiveSummonerCount;
            SurvivorsEnemyActor summoner = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(5f, 0f, 0f), SurvivorsEnemyRole.Summoner, 30f);
            Assert.NotNull(summoner);
            Assert.AreEqual(activeSummonersBefore + 1, controller.ActiveSummonerCount);
            Assert.AreEqual(activeBeforeSummoner + 1, controller.ActiveEnemyCount);

            controller.Simulate(0.06f);
            yield return null;

            Assert.AreEqual(2, controller.SummonerSupportSpawnCount);
            Assert.AreEqual(activeSummonersBefore + 1, controller.ActiveSummonerCount);
            Assert.AreEqual(activeBeforeSummoner + 3, controller.ActiveEnemyCount);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RunFlowCanSpawnTimedElitesMinibossAndBossOverTime()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.CurrentTuning.MajorThreatWarningLeadSeconds = 0.5f;
            controller.CurrentTuning.FirstEliteSpawnTimeSeconds = 2f;
            controller.CurrentTuning.FirstDreadEliteSpawnTimeSeconds = 4f;
            controller.CurrentTuning.MinibossSpawnTimeSeconds = 6f;
            controller.CurrentTuning.BossSpawnTimeSeconds = 8f;
            controller.CurrentTuning.SurvivalVictoryTimeSeconds = 12f;
            controller.StartRun();
            yield return null;

            controller.Simulate(1.5f);
            yield return null;

            Assert.IsTrue(controller.IsMajorThreatWarningActive);
            Assert.That(controller.CurrentMajorThreatWarningLabel, Does.Contain("ELITE"));
            Assert.AreEqual(1, controller.MajorThreatWarningCount);
            Assert.AreEqual(0, controller.ActiveEliteCount);

            controller.Simulate(controller.CurrentTuning.FirstEliteSpawnTimeSeconds - controller.RunTimeSeconds + 0.1f);
            yield return null;

            Assert.That(controller.ActiveEliteCount, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(0, controller.ActiveDreadEliteCount);
            Assert.AreEqual(0, controller.MinibossSpawnCount);
            Assert.IsFalse(controller.IsMajorThreatWarningActive);

            controller.Simulate(controller.CurrentTuning.FirstDreadEliteSpawnTimeSeconds - controller.CurrentTuning.MajorThreatWarningLeadSeconds - controller.RunTimeSeconds);
            yield return null;

            Assert.IsTrue(controller.IsMajorThreatWarningActive);
            Assert.That(controller.CurrentMajorThreatWarningLabel, Does.Contain("DREAD ELITE"));
            Assert.AreEqual(2, controller.MajorThreatWarningCount);

            controller.Simulate(controller.CurrentTuning.FirstDreadEliteSpawnTimeSeconds - controller.RunTimeSeconds + 0.1f);
            yield return null;

            Assert.That(controller.ActiveEliteCount, Is.GreaterThanOrEqualTo(2));
            Assert.AreEqual(1, controller.ActiveDreadEliteCount);
            Assert.IsFalse(controller.IsMajorThreatWarningActive);

            controller.Simulate(controller.CurrentTuning.MinibossSpawnTimeSeconds - controller.CurrentTuning.MajorThreatWarningLeadSeconds - controller.RunTimeSeconds);
            yield return null;

            Assert.IsTrue(controller.IsMajorThreatWarningActive);
            Assert.That(controller.CurrentMajorThreatWarningLabel, Does.Contain("MINIBOSS"));
            Assert.AreEqual(3, controller.MajorThreatWarningCount);

            controller.Simulate(controller.CurrentTuning.MinibossSpawnTimeSeconds - controller.RunTimeSeconds + 0.1f);
            yield return null;

            Assert.That(controller.MinibossSpawnCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.ActiveMinibossCount, Is.GreaterThanOrEqualTo(1));
            Assert.IsFalse(controller.IsMajorThreatWarningActive);

            controller.Simulate(controller.CurrentTuning.BossSpawnTimeSeconds - controller.CurrentTuning.MajorThreatWarningLeadSeconds - controller.RunTimeSeconds);
            yield return null;

            Assert.IsTrue(controller.IsMajorThreatWarningActive);
            Assert.That(controller.CurrentMajorThreatWarningLabel, Does.Contain("FINAL BOSS"));
            Assert.AreEqual(4, controller.MajorThreatWarningCount);

            controller.Simulate(controller.CurrentTuning.BossSpawnTimeSeconds - controller.RunTimeSeconds + 0.1f);
            yield return null;

            Assert.That(controller.BossSpawnCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.ActiveBossCount, Is.GreaterThanOrEqualTo(1));
            Assert.IsFalse(controller.IsMajorThreatWarningActive);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RecurringTimedElitesKeepMidRunRewardPressure()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.CurrentTuning.EnemyMaximumAlive = 12;
            controller.CurrentTuning.MajorThreatWarningLeadSeconds = 0.2f;
            controller.CurrentTuning.FirstEliteSpawnTimeSeconds = 0.5f;
            controller.CurrentTuning.EliteSpawnIntervalSeconds = 1.2f;
            controller.CurrentTuning.FirstDreadEliteSpawnTimeSeconds = 2.6f;
            controller.CurrentTuning.DreadEliteSpawnIntervalSeconds = 2f;
            controller.CurrentTuning.MinibossSpawnTimeSeconds = 5f;
            controller.CurrentTuning.BossSpawnTimeSeconds = 6f;
            controller.CurrentTuning.SurvivalVictoryTimeSeconds = 8f;
            controller.StartRun();
            yield return null;

            controller.Simulate(0.31f);
            yield return null;

            Assert.IsTrue(controller.IsMajorThreatWarningActive);
            Assert.That(controller.CurrentMajorThreatWarningLabel, Does.Contain("ELITE"));
            Assert.AreEqual(1, controller.MajorThreatWarningCount);

            controller.Simulate(0.25f);
            yield return null;

            Assert.That(controller.ActiveEliteCount, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(0, controller.ActiveDreadEliteCount);
            Assert.IsFalse(controller.IsMajorThreatWarningActive);
            int warningsAfterFirstElite = controller.MajorThreatWarningCount;

            controller.Simulate(1.6f - controller.RunTimeSeconds);
            yield return null;

            Assert.IsTrue(controller.IsMajorThreatWarningActive);
            Assert.That(controller.CurrentMajorThreatWarningLabel, Does.Contain("ELITE"));
            Assert.That(controller.MajorThreatWarningCount, Is.GreaterThan(warningsAfterFirstElite));

            controller.Simulate(1.82f - controller.RunTimeSeconds);
            yield return null;

            Assert.That(controller.ActiveEliteCount, Is.GreaterThanOrEqualTo(2));
            Assert.AreEqual(0, controller.ActiveDreadEliteCount);
            int warningsAfterRecurringElite = controller.MajorThreatWarningCount;

            controller.Simulate(2.41f - controller.RunTimeSeconds);
            yield return null;

            Assert.IsTrue(controller.IsMajorThreatWarningActive);
            Assert.That(controller.CurrentMajorThreatWarningLabel, Does.Contain("DREAD ELITE"));
            Assert.That(controller.MajorThreatWarningCount, Is.GreaterThan(warningsAfterRecurringElite));

            controller.Simulate(2.7f - controller.RunTimeSeconds);
            yield return null;

            Assert.That(controller.ActiveEliteCount, Is.GreaterThanOrEqualTo(2));
            Assert.AreEqual(1, controller.ActiveDreadEliteCount);
            Assert.IsFalse(controller.IsMajorThreatWarningActive);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator MajorThreatHealthReadoutPrioritizesBossAndTracksDamage()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            ConfigureProjectileModifierOnlyTuning(controller.CurrentTuning);
            controller.StartRun();
            yield return null;

            Assert.IsFalse(controller.IsMajorThreatHealthVisible);
            Assert.AreEqual(0f, controller.CurrentMajorThreatHealthFraction);
            Assert.AreEqual(string.Empty, controller.CurrentMajorThreatHealthLabel);

            SurvivorsEnemyActor elite = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(3f, 0f, 0f), SurvivorsEnemyRole.Elite, 100f);
            yield return null;

            Assert.IsTrue(controller.IsMajorThreatHealthVisible);
            Assert.That(controller.CurrentMajorThreatHealthLabel, Does.Contain("Blood Warden"));
            Assert.AreEqual(1f, controller.CurrentMajorThreatHealthFraction);

            SurvivorsEnemyActor miniboss = controller.SpawnMinibossForTest(controller.PlayerPosition + new Vector3(4f, 0f, 0f), 120f);
            yield return null;

            Assert.That(controller.CurrentMajorThreatHealthLabel, Does.Contain("Bloodbound Miniboss"));

            SurvivorsEnemyActor boss = controller.SpawnBossForTest(controller.PlayerPosition + new Vector3(5f, 0f, 0f), 200f);
            yield return null;

            Assert.That(controller.CurrentMajorThreatHealthLabel, Does.Contain("Eclipse Boss"));
            boss.ApplyDamage(50f, "test.major-threat-health");
            yield return null;

            Assert.AreEqual(0.75f, controller.CurrentMajorThreatHealthFraction, 0.001f);
            Assert.IsTrue(elite.IsAlive);
            Assert.IsTrue(miniboss.IsAlive);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator MajorThreatSlamTelegraphsBeforeDamagingPlayer()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.CurrentTuning.BossContactDamage = 0f;
            controller.CurrentTuning.StartingBarrierCapacity = 0f;
            controller.CurrentTuning.MajorThreatSlamIntervalSeconds = 0.15f;
            controller.CurrentTuning.MajorThreatSlamTelegraphSeconds = 0.1f;
            controller.CurrentTuning.MajorThreatSlamRadius = 3.2f;
            controller.CurrentTuning.MajorThreatSlamDamage = 8f;
            controller.StartRun();
            yield return null;

            SurvivorsEnemyActor boss = controller.SpawnBossForTest(controller.PlayerPosition + new Vector3(2.25f, 0f, 0f), 200f);
            Assert.NotNull(boss);
            float healthBeforeTelegraph = controller.CurrentHealth;

            controller.Simulate(0.16f);
            yield return null;

            Assert.AreEqual(1, controller.MajorThreatSlamWarningCount);
            Assert.AreEqual(0, controller.MajorThreatSlamCastCount);
            Assert.AreEqual(0, controller.MajorThreatSlamHitCount);
            Assert.AreEqual(1, controller.MajorThreatSlamTelegraphEffectCount);
            Assert.AreEqual(1, controller.ActiveMajorThreatSlamTelegraphEffectCount);
            Assert.AreEqual(healthBeforeTelegraph, controller.CurrentHealth);
            Assert.That(controller.LastMajorThreatSlamFeedbackLabel, Does.Contain("winding slam"));
            Assert.That(controller.ActiveStreakRewardFeedbackLabel, Does.Contain("winding slam"));

            controller.Simulate(0.11f);
            yield return null;

            Assert.AreEqual(1, controller.MajorThreatSlamCastCount);
            Assert.AreEqual(1, controller.MajorThreatSlamHitCount);
            Assert.That(controller.CurrentHealth, Is.LessThan(healthBeforeTelegraph));
            Assert.That(controller.LastMajorThreatSlamFeedbackLabel, Does.Contain("slam hit"));
            Assert.AreEqual(1, controller.ActiveMajorThreatSlamTelegraphEffectCount);

            controller.CurrentTuning.MajorThreatSlamRadius = 0.1f;
            controller.Simulate(0.2f);
            yield return null;

            Assert.AreEqual(0, controller.ActiveMajorThreatSlamTelegraphEffectCount);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator MajorThreatEnrageSpawnsOneSupportWaveAtLowHealth()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            ConfigureProjectileModifierOnlyTuning(controller.CurrentTuning);
            controller.CurrentTuning.MajorThreatEnrageHealthThreshold = 0.75f;
            controller.CurrentTuning.MajorThreatEnrageBossSupportCount = 5;
            controller.CurrentTuning.MajorThreatEnrageExtraAliveAllowance = 8;
            controller.CurrentTuning.MajorThreatEnrageSupportRadius = 2.8f;
            controller.StartRun();
            yield return null;

            SurvivorsEnemyActor boss = controller.SpawnBossForTest(controller.PlayerPosition + new Vector3(5f, 0f, 0f), 100f);
            yield return null;

            int aliveBeforeEnrage = controller.ActiveEnemyCount;
            boss.ApplyDamage(20f, "test.major-threat-enrage");
            yield return null;

            Assert.AreEqual(0, controller.MajorThreatEnrageCount);
            Assert.AreEqual(0, controller.MajorThreatEnrageSupportSpawnCount);
            Assert.AreEqual(aliveBeforeEnrage, controller.ActiveEnemyCount);

            boss.ApplyDamage(10f, "test.major-threat-enrage");
            yield return null;

            Assert.AreEqual(1, controller.MajorThreatEnrageCount);
            Assert.AreEqual(5, controller.MajorThreatEnrageSupportSpawnCount);
            Assert.AreEqual(aliveBeforeEnrage + 5, controller.ActiveEnemyCount);
            Assert.That(controller.LastMajorThreatEnrageFeedbackLabel, Does.Contain("Eclipse Boss"));
            Assert.That(controller.LastMajorThreatEnrageFeedbackLabel, Does.Contain("+5 support"));

            boss.ApplyDamage(5f, "test.major-threat-enrage");
            yield return null;

            Assert.AreEqual(1, controller.MajorThreatEnrageCount);
            Assert.AreEqual(5, controller.MajorThreatEnrageSupportSpawnCount);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator MinibossCanDieAndDropExperience()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            SurvivorsEnemyActor miniboss = controller.SpawnMinibossForTest(controller.PlayerPosition + new Vector3(2.4f, 0f, 0f), 1f);
            miniboss.ApplyDamage(100f, "test.miniboss");
            yield return null;

            Assert.That(controller.MinibossSpawnCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.MinibossKilledCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.MinibossRewardGrantCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.BonusBloodShardsEarnedThisRun, Is.GreaterThanOrEqualTo(4));
            Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.ActivePickupCount, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(1, controller.MajorRewardDropFeedbackCount);
            Assert.AreEqual(1, controller.ActiveMajorRewardDropFeedbackCount);
            Assert.That(controller.LastMajorRewardDropFeedbackLabel, Does.Contain("Miniboss"));

            controller.Simulate(1.4f);
            yield return null;

            Assert.AreEqual(0, controller.ActiveMajorRewardDropFeedbackCount);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator EliteDeathOpensEliteUpgradeRewardChoice()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            SurvivorsEnemyActor elite = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.8f, 0f, 0f), SurvivorsEnemyRole.Elite, 1f);
            elite.ApplyDamage(100f, "test.elite");
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.IsTrue(controller.IsUpgradeRewardChoiceOpen);
            Assert.IsFalse(controller.IsRelicChoiceOpen);
            Assert.That(controller.EliteKilledCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.EliteRewardGrantCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.EliteUpgradeDraftOpenCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.BonusBloodShardsEarnedThisRun, Is.GreaterThanOrEqualTo(2));
            Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
            Assert.That(controller.RewardCardPresentationCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(controller.LastRewardCardPresentationLabel, Does.Contain("Elite Reward"));
            Assert.AreEqual(1, controller.MajorRewardDropFeedbackCount);
            Assert.AreEqual(1, controller.ActiveMajorRewardDropFeedbackCount);
            Assert.That(controller.LastMajorRewardDropFeedbackLabel, Does.Contain("Elite"));
            Assert.AreEqual(1, controller.MajorRewardCacheDropCount);
            Assert.That(controller.MajorRewardCacheExperienceGemDropCount, Is.GreaterThanOrEqualTo(5));
            Assert.That(controller.MajorRewardCacheSpecialDropCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.ActivePickupCount, Is.GreaterThanOrEqualTo(7));
            Assert.That(controller.LastMajorRewardCacheFeedbackLabel, Does.Contain("Elite"));
            Assert.That(controller.LastMajorRewardCacheFeedbackLabel, Does.Contain("Cache"));
            Assert.That(controller.LastMajorRewardCacheFeedbackLabel, Does.Contain("pull"));
            Assert.That(controller.MajorRewardCacheAttractedPickupCount, Is.GreaterThanOrEqualTo(6));
            Assert.That(controller.ActiveMajorRewardCacheAttractedPickupCount, Is.GreaterThanOrEqualTo(6));
            Assert.That(controller.PickupAttractionFeedbackCount, Is.GreaterThanOrEqualTo(5));
            Assert.That(controller.LastStreakRewardFeedbackLabel, Does.Contain("Cache"));
            int experienceBeforeCachePull = controller.ExperienceCollected;
            int activeCachePullBefore = controller.ActiveMajorRewardCacheAttractedPickupCount;

            controller.Simulate(1.4f);
            yield return null;

            Assert.AreEqual(0, controller.ActiveMajorRewardDropFeedbackCount);
            Assert.IsTrue(controller.SkipCurrentDraft());
            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.That(controller.LastRewardSelectionFeedbackLabel, Does.Contain("Elite Reward"));
            Assert.That(controller.ActiveRewardFeedbackLabel, Does.Contain("skipped"));

            controller.Simulate(0.65f);
            yield return null;

            Assert.That(controller.ExperienceCollected, Is.GreaterThan(experienceBeforeCachePull));
            Assert.That(controller.ActiveMajorRewardCacheAttractedPickupCount, Is.LessThan(activeCachePullBefore));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator DreadEliteCountsAsEliteAndOpensRewardChoice()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.StartRun();

            int activeEliteBefore = controller.ActiveEliteCount;
            SurvivorsEnemyActor dreadElite = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(5f, 0f, 0f), SurvivorsEnemyRole.DreadElite, 1f);

            Assert.AreEqual(activeEliteBefore + 1, controller.ActiveEliteCount);
            Assert.AreEqual(1, controller.ActiveDreadEliteCount);

            dreadElite.ApplyDamage(100f, "test.dread-elite");
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.IsTrue(controller.IsUpgradeRewardChoiceOpen);
            Assert.That(controller.EliteKilledCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.EliteRewardGrantCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.EliteUpgradeDraftOpenCount, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
            Assert.AreEqual(1, controller.MajorRewardDropFeedbackCount);
            Assert.That(controller.LastMajorRewardDropFeedbackLabel, Does.Contain("Dread Elite"));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator MinibossDeathChainsUpgradeRewardIntoBossRelicChoice()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            float previousRelicDamage = controller.RelicDamageBonus;
            float previousRelicCooldown = controller.RelicCooldownMultiplierBonus;
            float previousRelicPickup = controller.RelicPickupRangeBonus;
            SurvivorsEnemyActor miniboss = controller.SpawnMinibossForTest(controller.PlayerPosition + new Vector3(2.4f, 0f, 0f), 1f);
            miniboss.ApplyDamage(100f, "test.miniboss");
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.IsTrue(controller.IsUpgradeRewardChoiceOpen);
            Assert.IsFalse(controller.IsRelicChoiceOpen);
            Assert.That(controller.EliteUpgradeDraftOpenCount, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
            Assert.IsTrue(controller.SelectUpgrade(0));
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.IsTrue(controller.IsRelicChoiceOpen);
            Assert.That(controller.BossRelicDraftOpenCount, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(3, controller.CurrentRelicChoices.Count);
            Assert.AreEqual(1, controller.SelectedRewardUpgradeCount);
            Assert.IsTrue(controller.SelectRelicForTest(0));
            yield return null;

            Assert.IsFalse(controller.IsRelicChoiceOpen);
            Assert.That(controller.State, Is.EqualTo(SurvivorsRunState.Playing).Or.EqualTo(SurvivorsRunState.LevelUp));
            if (controller.State == SurvivorsRunState.LevelUp)
            {
                Assert.IsTrue(controller.IsRunUpgradeDraftOpen);
            }

            Assert.AreEqual(1, controller.SelectedRelicCount);
            bool relicChanged = controller.RelicDamageBonus != previousRelicDamage ||
                controller.RelicCooldownMultiplierBonus != previousRelicCooldown ||
                controller.RelicPickupRangeBonus != previousRelicPickup;
            Assert.IsTrue(relicChanged);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator SelectedBossRelicAffectsCurrentRun()
        {
            SurvivorsTemplateController controller = CreateController();
            controller.CurrentTuning.BossRelicSurgeDamage = 12f;
            controller.CurrentTuning.BossRelicSurgeRadius = 4f;
            yield return null;

            SurvivorsEnemyActor surgeTargetA = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(1.4f, 0f, 0f), 1f);
            SurvivorsEnemyActor surgeTargetB = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(-1.2f, 0f, 0f), 1f);
            SurvivorsEnemyActor majorTarget = controller.SpawnBossForTest(controller.PlayerPosition + new Vector3(1.8f, 0f, 1.2f), 100f);
            int killedBeforeRelic = controller.KilledCount;

            Assert.IsTrue(controller.OpenBossRelicDraftForTest());
            SurvivorsRelicDefinition selectedRelic = controller.CurrentRelicChoices[0];
            float previousRelicDamage = controller.RelicDamageBonus;
            float previousRelicCooldown = controller.RelicCooldownMultiplierBonus;
            float previousRelicPickup = controller.RelicPickupRangeBonus;

            Assert.IsTrue(controller.SelectRelicForTest(0));
            yield return null;

            Assert.IsFalse(controller.IsRelicChoiceOpen);
            Assert.That(controller.State, Is.EqualTo(SurvivorsRunState.Playing).Or.EqualTo(SurvivorsRunState.LevelUp));
            if (controller.State == SurvivorsRunState.LevelUp)
            {
                Assert.IsTrue(controller.IsRunUpgradeDraftOpen);
            }

            Assert.AreEqual(1, controller.SelectedRelicCount);
            bool relicChanged = controller.RelicDamageBonus != previousRelicDamage ||
                controller.RelicCooldownMultiplierBonus != previousRelicCooldown ||
                controller.RelicPickupRangeBonus != previousRelicPickup;
            Assert.IsTrue(relicChanged);
            Assert.AreEqual(1, controller.BossRelicSurgeCount);
            Assert.AreEqual(2, controller.BossRelicSurgeHitCount);
            Assert.IsFalse(surgeTargetA.IsAlive);
            Assert.IsFalse(surgeTargetB.IsAlive);
            Assert.IsTrue(majorTarget.IsAlive);
            Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(killedBeforeRelic + 2));
            Assert.That(controller.LastBossRelicSurgeFeedbackLabel, Does.Contain(selectedRelic.DisplayName));
            Assert.That(controller.LastBossRelicSurgeFeedbackLabel, Does.Contain("2 enemies hit"));
            Assert.That(controller.ActiveStreakRewardFeedbackLabel, Does.Contain("Surge"));
            Assert.That(controller.RewardCardPresentationCount, Is.GreaterThanOrEqualTo(3));
            Assert.AreEqual(1, controller.RewardSelectionFeedbackCount);
            Assert.That(controller.LastRewardSelectionFeedbackLabel, Does.Contain("Boss Relic"));
            Assert.That(controller.ActiveRewardFeedbackLabel, Does.Contain("Boss Relic"));
            string buildSummary = string.Join("\n", controller.DebugDescribeCurrentBuild());
            Assert.That(buildSummary, Does.Contain("Relics 1/"));
            Assert.That(buildSummary, Does.Contain(selectedRelic.DisplayName));

            controller.KillPlayerForTest();
            yield return null;

            string runSummary = string.Join("\n", controller.LastRunSummaryLines);
            Assert.That(runSummary, Does.Contain("1/"));
            Assert.That(runSummary, Does.Contain("relics"));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator BossRelicDraftsDoNotRepeatSelectedRelics()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            int expectedRelicCount = BasicSurvivorsGame.CreateRelicDefinitions().Count;
            var selectedRelicIds = new HashSet<string>(StringComparer.Ordinal);
            int openedDraftCount = 0;
            while (controller.OpenBossRelicDraftForTest())
            {
                openedDraftCount++;
                Assert.That(controller.CurrentRelicChoices.Count, Is.GreaterThan(0));
                Assert.That(controller.CurrentRelicChoices.Count, Is.LessThanOrEqualTo(3));

                for (int i = 0; i < controller.CurrentRelicChoices.Count; i++)
                {
                    SurvivorsRelicDefinition relic = controller.CurrentRelicChoices[i];
                    Assert.IsNotNull(relic);
                    Assert.IsFalse(selectedRelicIds.Contains(relic.Id), $"Relic repeated in draft: {relic.Id}");
                }

                string selectedRelicId = controller.CurrentRelicChoices[0].Id;
                selectedRelicIds.Add(selectedRelicId);
                Assert.IsTrue(controller.SelectRelicForTest(0));
                yield return null;

                Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
                Assert.AreEqual(selectedRelicIds.Count, controller.SelectedRelicCount);
                Assert.That(openedDraftCount, Is.LessThanOrEqualTo(expectedRelicCount));
            }

            Assert.AreEqual(expectedRelicCount, selectedRelicIds.Count);
            Assert.AreEqual(expectedRelicCount, controller.SelectedRelicCount);
            Assert.IsFalse(controller.IsRelicChoiceOpen);
            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator EligibleBossEvolutionRewardDelaysVictoryUntilSelected()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.arcane-damage"));
            }

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ArcaneThesisUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));

            SurvivorsEnemyActor boss = controller.SpawnBossForTest(controller.PlayerPosition + new Vector3(3f, 0f, 0f), 1f);
            boss.ApplyDamage(100f, "test.boss.evolution");
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.IsTrue(controller.IsUpgradeRewardChoiceOpen);
            Assert.IsFalse(controller.IsVictory);
            Assert.That(controller.BossUpgradeDraftOpenCount, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(1, controller.MajorRewardDropFeedbackCount);
            Assert.AreEqual(1, controller.ActiveMajorRewardDropFeedbackCount);
            Assert.That(controller.LastMajorRewardDropFeedbackLabel, Does.Contain("Boss"));
            int evolutionChoiceIndex = IndexOfDraftChoice(controller, BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId);
            Assert.That(evolutionChoiceIndex, Is.GreaterThanOrEqualTo(0));

            Assert.IsTrue(controller.SelectUpgrade(evolutionChoiceIndex));
            yield return null;

            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            Assert.AreEqual(1, controller.SelectedRewardUpgradeCount);
            Assert.AreEqual(1, controller.RewardSelectionFeedbackCount);
            Assert.That(controller.LastRewardSelectionFeedbackLabel, Does.Contain("Boss Reward"));
            Assert.That(controller.LastRewardSelectionFeedbackLabel, Does.Contain("Legendary"));
            Assert.AreEqual(SurvivorsRunState.Victory, controller.State);
            Assert.IsTrue(controller.IsVictory);
            Assert.That(controller.BossRewardGrantCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.BloodShardsEarnedThisRun, Is.GreaterThanOrEqualTo(18));
            Assert.AreEqual(1, controller.MetaBossVictories);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator BossDeathOpensFallbackRewardThenTriggersVictory()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            SurvivorsEnemyActor boss = controller.SpawnBossForTest(controller.PlayerPosition + new Vector3(3f, 0f, 0f), 1f);
            boss.ApplyDamage(100f, "test.boss");
            yield return null;

            Assert.That(controller.BossSpawnCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.BossKilledCount, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.IsTrue(controller.IsUpgradeRewardChoiceOpen);
            Assert.That(controller.BossUpgradeDraftOpenCount, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(1, controller.MajorRewardDropFeedbackCount);
            Assert.That(controller.LastMajorRewardDropFeedbackLabel, Does.Contain("Boss"));
            Assert.AreEqual(1, controller.MajorRewardCacheDropCount);
            Assert.That(controller.MajorRewardCacheExperienceGemDropCount, Is.GreaterThanOrEqualTo(12));
            Assert.That(controller.MajorRewardCacheSpecialDropCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(controller.LastMajorRewardCacheFeedbackLabel, Does.Contain("Boss"));
            Assert.That(controller.LastMajorRewardCacheFeedbackLabel, Does.Contain("Cache"));
            Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
            Assert.AreEqual(-1, IndexOfDraftChoice(controller, BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            for (int i = 0; i < controller.CurrentDraftChoices.Count; i++)
            {
                Assert.That((int)controller.CurrentDraftChoices[i].Rarity, Is.GreaterThanOrEqualTo((int)RunUpgradeRarity.Rare));
            }

            Assert.IsTrue(controller.SkipCurrentDraft());
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Victory, controller.State);
            Assert.IsTrue(controller.IsVictory);
            Assert.That(controller.BossRewardGrantCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.BloodShardsEarnedThisRun, Is.GreaterThanOrEqualTo(18));
            Assert.That(controller.LegacyExperienceEarnedThisRun, Is.GreaterThanOrEqualTo(120));
            Assert.That(controller.MetaBloodShards, Is.GreaterThanOrEqualTo(18));
            Assert.AreEqual(1, controller.MetaBossVictories);
            Assert.That(controller.ClassUnlockRewardCount, Is.GreaterThanOrEqualTo(1));
            Assert.IsTrue(controller.IsClassUnlockedForTest(BasicSurvivorsGame.EmberVanguardClassId));
            Assert.That(controller.LastRunSummaryLines.Count, Is.GreaterThanOrEqualTo(6));
            string victorySummary = string.Join("\n", controller.LastRunSummaryLines);
            Assert.That(victorySummary, Does.Contain("Victory"));
            Assert.That(victorySummary, Does.Contain("Bosses 1"));
            Assert.That(victorySummary, Does.Contain("Build"));
            Assert.That(victorySummary, Does.Contain("Class unlocked"));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator SurvivalDurationCanTriggerVictory()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.SurvivalVictoryTimeSeconds = 0.5f;
            controller.StartRun();
            yield return null;

            controller.Simulate(controller.CurrentTuning.SurvivalVictoryTimeSeconds + 0.1f);
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Victory, controller.State);
            Assert.IsTrue(controller.IsVictory);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator VictoryCanContinueIntoEndlessEscalation()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.SurvivalVictoryTimeSeconds = 0.5f;
            controller.StartRun();
            yield return null;

            int baseMaximumAlive = controller.CurrentEnemyMaximumAlive;
            float baseSpawnInterval = controller.CurrentEnemySpawnIntervalSeconds;

            controller.Simulate(0.6f);
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Victory, controller.State);
            Assert.IsTrue(controller.HasClearedVictoryThisRun);
            Assert.IsFalse(controller.IsEndlessRun);
            long shardsAfterVictory = controller.MetaBloodShards;
            int completedRunsAfterVictory = controller.MetaCompletedRuns;

            Assert.IsTrue(controller.ContinueAfterVictory());
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.IsFalse(controller.IsVictory);
            Assert.IsTrue(controller.IsEndlessRun);
            Assert.That(controller.CurrentEnemyMaximumAlive, Is.GreaterThan(baseMaximumAlive));
            Assert.That(controller.CurrentEnemySpawnIntervalSeconds, Is.LessThan(baseSpawnInterval));

            float timeBefore = controller.RunTimeSeconds;
            controller.Simulate(0.25f, Vector2.right);
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.That(controller.RunTimeSeconds, Is.GreaterThan(timeBefore));
            Assert.That(controller.PlayerPosition.x, Is.GreaterThan(0f));
            Assert.AreEqual(shardsAfterVictory, controller.MetaBloodShards);
            Assert.AreEqual(completedRunsAfterVictory, controller.MetaCompletedRuns);
            Assert.IsFalse(controller.ContinueAfterVictory());

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator EndlessContinuationSpawnsRecurringMajorThreats()
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            controller.CurrentTuning.PlayerMaxHealth = 999f;
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            controller.CurrentTuning.EnemyMaximumAlive = 1;
            controller.CurrentTuning.SurvivalVictoryTimeSeconds = 0.4f;
            controller.CurrentTuning.MajorThreatWarningLeadSeconds = 0.2f;
            controller.CurrentTuning.EndlessEliteSpawnIntervalSeconds = 0.5f;
            controller.CurrentTuning.EndlessMinibossSpawnIntervalSeconds = 0.8f;
            controller.CurrentTuning.EndlessBossSpawnIntervalSeconds = 1.2f;
            controller.StartRun();
            yield return null;

            controller.Simulate(0.45f);
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Victory, controller.State);
            Assert.IsTrue(controller.ContinueAfterVictory());
            Assert.IsTrue(controller.IsEndlessRun);
            Assert.AreEqual(0, controller.EndlessThreatSpawnCount);

            int warningCount = controller.MajorThreatWarningCount;
            controller.Simulate(0.31f);
            yield return null;

            Assert.That(controller.MajorThreatWarningCount, Is.GreaterThan(warningCount));
            Assert.IsTrue(controller.IsMajorThreatWarningActive);
            Assert.That(controller.CurrentMajorThreatWarningLabel, Does.Contain("ELITE"));
            Assert.AreEqual(0, controller.EndlessThreatSpawnCount);

            controller.Simulate(0.2f);
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.That(controller.EndlessThreatSpawnCount, Is.GreaterThanOrEqualTo(1));

            controller.Simulate(0.35f);
            yield return null;

            Assert.That(controller.EndlessThreatSpawnCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(controller.MinibossSpawnCount, Is.GreaterThanOrEqualTo(1));

            controller.Simulate(0.45f);
            yield return null;

            Assert.That(controller.EndlessThreatSpawnCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(controller.BossSpawnCount, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator MinibossRewardIsGrantedWhenRunEndsInDefeat()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            SurvivorsEnemyActor miniboss = controller.SpawnMinibossForTest(controller.PlayerPosition + new Vector3(2.2f, 0f, 0f), 1f);
            miniboss.ApplyDamage(100f, "test.miniboss");
            yield return null;
            controller.KillPlayerForTest();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.GameOver, controller.State);
            Assert.That(controller.MinibossRewardGrantCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.BloodShardsEarnedThisRun, Is.GreaterThanOrEqualTo(4));
            Assert.That(controller.LegacyExperienceEarnedThisRun, Is.GreaterThanOrEqualTo(25));
            Assert.That(controller.MetaBloodShards, Is.GreaterThanOrEqualTo(4));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator MetaCurrencyPersistsAcrossControllerInstances()
        {
            var storage = new InMemoryTextStorage();
            const string slot = "play-persistence";
            SurvivorsTemplateController first = CreateController(storage, slot);
            yield return null;

            yield return DefeatBossAndResolveReward(first);
            long savedBloodShards = first.MetaBloodShards;

            Object.Destroy(first.gameObject);
            yield return null;

            SurvivorsTemplateController second = CreateController(storage, slot);
            yield return null;

            Assert.That(savedBloodShards, Is.GreaterThanOrEqualTo(18));
            Assert.AreEqual(savedBloodShards, second.MetaBloodShards);
            Assert.AreEqual(1, second.MetaBossVictories);

            Object.Destroy(second.gameObject);
        }

        [UnityTest]
        public IEnumerator NormalPlayModeStartDoesNotWipeMetaProgression()
        {
            var storage = new InMemoryTextStorage();
            const string slot = "play-no-reset-start";
            SurvivorsTemplateController first = CreateController(storage, slot);
            yield return null;

            yield return DefeatBossAndResolveReward(first);

            long savedBloodShards = first.MetaBloodShards;
            long lifetimeBloodShards = first.LifetimeBloodShards;
            int completedRuns = first.MetaCompletedRuns;
            int bossVictories = first.MetaBossVictories;
            Assert.That(savedBloodShards, Is.GreaterThan(0));
            Assert.That(lifetimeBloodShards, Is.GreaterThan(0));
            Assert.That(completedRuns, Is.GreaterThan(0));

            Object.Destroy(first.gameObject);
            yield return null;

            SurvivorsTemplateController second = CreateController(storage, slot);
            yield return null;

            Assert.AreEqual(savedBloodShards, second.MetaBloodShards);
            Assert.AreEqual(lifetimeBloodShards, second.LifetimeBloodShards);
            Assert.AreEqual(completedRuns, second.MetaCompletedRuns);
            Assert.AreEqual(bossVictories, second.MetaBossVictories);

            Object.Destroy(second.gameObject);
        }

        [UnityTest]
        public IEnumerator ApplyingNormalPacingDoesNotWipeMetaProgression()
        {
            var storage = new InMemoryTextStorage();
            SurvivorsTemplateController controller = CreateController(storage, "play-no-reset-normal-profile");
            yield return null;

            yield return DefeatBossAndResolveReward(controller);

            long savedBloodShards = controller.MetaBloodShards;
            long lifetimeBloodShards = controller.LifetimeBloodShards;
            int completedRuns = controller.MetaCompletedRuns;
            int bossVictories = controller.MetaBossVictories;
            bool emberUnlocked = controller.IsClassUnlockedForTest(BasicSurvivorsGame.EmberVanguardClassId);

            controller.ApplyPacingProfileForTest(SurvivorsPacingProfile.Normal, restartRun: true);
            yield return null;

            Assert.AreEqual(SurvivorsPacingProfile.Normal, controller.CurrentPacingProfile);
            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.AreEqual(savedBloodShards, controller.MetaBloodShards);
            Assert.AreEqual(lifetimeBloodShards, controller.LifetimeBloodShards);
            Assert.AreEqual(completedRuns, controller.MetaCompletedRuns);
            Assert.AreEqual(bossVictories, controller.MetaBossVictories);
            Assert.AreEqual(emberUnlocked, controller.IsClassUnlockedForTest(BasicSurvivorsGame.EmberVanguardClassId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RestartingRunDoesNotWipeMetaProgression()
        {
            var storage = new InMemoryTextStorage();
            SurvivorsTemplateController controller = CreateController(storage, "play-no-reset-restart");
            yield return null;

            yield return DefeatBossAndResolveReward(controller);

            long savedBloodShards = controller.MetaBloodShards;
            long lifetimeBloodShards = controller.LifetimeBloodShards;
            int completedRuns = controller.MetaCompletedRuns;
            int bossVictories = controller.MetaBossVictories;
            bool emberUnlocked = controller.IsClassUnlockedForTest(BasicSurvivorsGame.EmberVanguardClassId);

            controller.RestartRun();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.AreEqual(savedBloodShards, controller.MetaBloodShards);
            Assert.AreEqual(lifetimeBloodShards, controller.LifetimeBloodShards);
            Assert.AreEqual(completedRuns, controller.MetaCompletedRuns);
            Assert.AreEqual(bossVictories, controller.MetaBossVictories);
            Assert.AreEqual(emberUnlocked, controller.IsClassUnlockedForTest(BasicSurvivorsGame.EmberVanguardClassId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ExplicitResetClearsPersistentMetaProgression()
        {
            var storage = new InMemoryTextStorage();
            SurvivorsTemplateController controller = CreateController(storage, "play-explicit-reset");
            yield return null;

            yield return DefeatBossAndResolveReward(controller);

            Assert.That(controller.MetaBloodShards, Is.GreaterThan(0));
            Assert.That(controller.LifetimeBloodShards, Is.GreaterThan(0));
            Assert.That(controller.MetaCompletedRuns, Is.GreaterThan(0));
            Assert.IsTrue(controller.IsClassUnlockedForTest(BasicSurvivorsGame.EmberVanguardClassId));

            controller.DebugResetMetaProgression();
            yield return null;

            Assert.AreEqual(0, controller.MetaBloodShards);
            Assert.AreEqual(0, controller.LifetimeBloodShards);
            Assert.AreEqual(0, controller.LifetimeLegacyExperience);
            Assert.AreEqual(0, controller.MetaCompletedRuns);
            Assert.AreEqual(0, controller.MetaBossVictories);
            Assert.IsFalse(controller.IsClassUnlockedForTest(BasicSurvivorsGame.EmberVanguardClassId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator SelectedClassAffectsStartingState()
        {
            var storage = new InMemoryTextStorage();
            SurvivorsTemplateController controller = CreateController(storage, "play-class-start", startRun: false);

            Assert.IsTrue(controller.UnlockClassForTest(BasicSurvivorsGame.EmberVanguardClassId));
            Assert.IsTrue(controller.TrySelectClassForTest(BasicSurvivorsGame.EmberVanguardClassId));
            controller.StartRun();
            yield return null;

            Assert.AreEqual(BasicSurvivorsGame.EmberVanguardClassId, controller.SelectedClassId);
            Assert.That(controller.PlayerMoveSpeed, Is.GreaterThan(controller.CurrentTuning.PlayerMoveSpeed));
            Assert.That(controller.ProjectileDamage, Is.GreaterThan(controller.CurrentTuning.ProjectileDamage));
            Assert.That(controller.MaxHealth, Is.GreaterThan(controller.CurrentTuning.PlayerMaxHealth));
            Assert.That(controller.ActiveWeaponCount, Is.GreaterThan(1));
            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.StarBeamWeaponContentId));
            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.GravityGrenadeWeaponContentId));
            Assert.IsTrue(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator EmberVanguardMoonSlashPathRequiresRanksBeforeEvolution()
        {
            SurvivorsTemplateController defaultController = CreateController();
            yield return null;

            Assert.IsFalse(defaultController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.MoonlitEdgeUpgradeId));
            Assert.IsFalse(defaultController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.MoonOathUpgradeId));
            Assert.IsFalse(defaultController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.MoonSlashUnlockUpgradeId));
            Object.Destroy(defaultController.gameObject);
            yield return null;

            SurvivorsTemplateController controller = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            yield return null;

            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.MoonSlashWeaponContentId));
            Assert.IsTrue(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.MoonSlashUnlockUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.MoonSlashUnlockUpgradeId));
            Assert.IsTrue(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.MoonlitEdgeUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.MoonlitEdgeUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.CrescentChainUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.LunarTempoUpgradeId));

            for (int i = 0; i < 2; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.MoonlitEdgeUpgradeId));
            }

            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.CrescentChainUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.LunarTempoUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.MoonlitEdgeUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.LunarTempoUpgradeId));

            int previousTargets = controller.MeleeTargetBonus;
            float previousCooldown = controller.WeaponCooldownMultiplierBonus;
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.CrescentChainUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.LunarTempoUpgradeId));
            Assert.That(controller.MeleeTargetBonus, Is.GreaterThan(previousTargets));
            Assert.That(controller.WeaponCooldownMultiplierBonus, Is.LessThan(previousCooldown));

            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.EclipseWaltzEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.MoonlitEdgeUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.MoonlitEdgeUpgradeId));
            Assert.AreEqual(5, controller.GetRunUpgradeRankForTest(BasicSurvivorsGame.MoonlitEdgeUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.EclipseWaltzEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.MoonOathUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.EclipseWaltzEvolutionUpgradeId));

            previousTargets = controller.MeleeTargetBonus;
            previousCooldown = controller.WeaponCooldownMultiplierBonus;
            float previousDamage = controller.DamageBonus;
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.EclipseWaltzEvolutionUpgradeId));
            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.EclipseWaltzEvolutionUpgradeId));
            Assert.That(controller.MeleeTargetBonus, Is.GreaterThan(previousTargets));
            Assert.That(controller.WeaponCooldownMultiplierBonus, Is.LessThan(previousCooldown));
            Assert.That(controller.DamageBonus, Is.GreaterThan(previousDamage));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ClassSpecificUpgradeAppearsOnlyWhenValid()
        {
            SurvivorsTemplateController defaultController = CreateController();
            yield return null;

            defaultController.ForceLevelUpWithLockedChoiceForTest(BasicSurvivorsGame.EmberForgeHeartUpgradeId);
            yield return null;

            Assert.IsTrue(defaultController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId));
            Assert.IsFalse(defaultController.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId));
            Assert.IsTrue(defaultController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.ArcaneThesisUpgradeId));
            Assert.IsFalse(defaultController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.EmberForgeHeartUpgradeId));
            Assert.AreEqual(-1, IndexOfDraftChoice(defaultController, BasicSurvivorsGame.EmberForgeHeartUpgradeId));
            Object.Destroy(defaultController.gameObject);
            yield return null;

            SurvivorsTemplateController emberController = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            yield return null;

            emberController.ForceLevelUpWithLockedChoiceForTest(BasicSurvivorsGame.EmberForgeHeartUpgradeId);
            yield return null;

            int gatedChoiceIndex = IndexOfDraftChoice(emberController, BasicSurvivorsGame.EmberForgeHeartUpgradeId);
            Assert.That(gatedChoiceIndex, Is.GreaterThanOrEqualTo(0));
            Assert.IsFalse(emberController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.ArcaneThesisUpgradeId));
            Assert.IsTrue(emberController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.EmberForgeHeartUpgradeId));
            float previousDamage = emberController.DamageBonus;
            Assert.IsTrue(emberController.SelectUpgrade(gatedChoiceIndex));
            Assert.That(emberController.DamageBonus, Is.GreaterThan(previousDamage));

            Object.Destroy(emberController.gameObject);
        }

        [UnityTest]
        public IEnumerator ClassUnlockPersistsAfterVictory()
        {
            var storage = new InMemoryTextStorage();
            const string slot = "play-class-unlock";
            SurvivorsTemplateController first = CreateController(storage, slot);
            yield return null;

            yield return DefeatBossAndResolveReward(first);

            Assert.IsTrue(first.IsClassUnlockedForTest(BasicSurvivorsGame.EmberVanguardClassId));
            Object.Destroy(first.gameObject);
            yield return null;

            SurvivorsTemplateController second = CreateController(storage, slot, startRun: false);
            Assert.IsTrue(second.IsClassUnlockedForTest(BasicSurvivorsGame.EmberVanguardClassId));
            Assert.IsTrue(second.TrySelectClassForTest(BasicSurvivorsGame.EmberVanguardClassId));
            second.StartRun();
            yield return null;

            Assert.AreEqual(BasicSurvivorsGame.EmberVanguardClassId, second.SelectedClassId);

            Object.Destroy(second.gameObject);
        }

        [UnityTest]
        public IEnumerator ResultClassChoiceSelectsUnlockedClassForNextRun()
        {
            var storage = new InMemoryTextStorage();
            SurvivorsTemplateController controller = CreateController(storage, "play-result-class-choice");
            yield return null;

            yield return DefeatBossAndResolveReward(controller);

            Assert.AreEqual(SurvivorsRunState.Victory, controller.State);
            Assert.AreEqual(BasicSurvivorsGame.DefaultClassId, controller.SelectedClassId);
            Assert.IsTrue(controller.IsClassUnlockedForTest(BasicSurvivorsGame.EmberVanguardClassId));

            IReadOnlyList<string> classOptions = controller.GetResultClassOptionLabelsForTest();
            Assert.That(classOptions.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(classOptions[0], Does.Contain("Arcane Initiate"));
            Assert.That(classOptions[0], Does.Contain("Selected"));
            Assert.That(classOptions[1], Does.Contain("Ember Vanguard"));
            Assert.That(classOptions[1], Does.Contain("Unlocked"));

            Assert.IsTrue(controller.TrySelectResultClassForTest(1));
            Assert.AreEqual(BasicSurvivorsGame.EmberVanguardClassId, controller.SelectedClassId);
            Assert.AreEqual(1, controller.ResultClassSelectionCount);
            Assert.That(controller.LastResultClassSelectionFeedbackLabel, Does.Contain("Ember Vanguard"));

            controller.RestartRun();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.AreEqual(BasicSurvivorsGame.EmberVanguardClassId, controller.SelectedClassId);
            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.StarBeamWeaponContentId));
            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.GravityGrenadeWeaponContentId));
            Assert.IsTrue(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.EmberForgeHeartUpgradeId));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator PersistentUpgradesAffectNewRunStats()
        {
            var storage = new InMemoryTextStorage();
            SurvivorsTemplateController controller = CreateController(storage, "play-meta-upgrade");
            yield return null;

            yield return DefeatBossAndResolveReward(controller);
            Assert.IsTrue(controller.DebugGrantBloodShards(40));

            Assert.IsTrue(controller.TryPurchasePersistentUpgradeForTest(BasicSurvivorsGame.ArcaneLegacyMetaUpgradeId.Value));
            Assert.AreEqual(1, controller.GetPersistentUpgradeRankForTest(BasicSurvivorsGame.ArcaneLegacyMetaUpgradeId.Value));
            Assert.IsTrue(controller.TryPurchasePersistentUpgradeForTest(BasicSurvivorsGame.VitalWardMetaUpgradeId.Value));
            Assert.AreEqual(1, controller.GetPersistentUpgradeRankForTest(BasicSurvivorsGame.VitalWardMetaUpgradeId.Value));
            Assert.IsTrue(controller.TryPurchasePersistentUpgradeForTest(BasicSurvivorsGame.GemheartLegacyMetaUpgradeId.Value));
            Assert.AreEqual(1, controller.GetPersistentUpgradeRankForTest(BasicSurvivorsGame.GemheartLegacyMetaUpgradeId.Value));
            Assert.IsTrue(controller.TryPurchasePersistentUpgradeForTest(BasicSurvivorsGame.ScholarIndexMetaUpgradeId.Value));
            Assert.AreEqual(1, controller.GetPersistentUpgradeRankForTest(BasicSurvivorsGame.ScholarIndexMetaUpgradeId.Value));
            Assert.IsTrue(controller.TryPurchasePersistentUpgradeForTest(BasicSurvivorsGame.PreparedDraftMetaUpgradeId.Value));
            Assert.AreEqual(1, controller.GetPersistentUpgradeRankForTest(BasicSurvivorsGame.PreparedDraftMetaUpgradeId.Value));

            controller.RestartRun();
            yield return null;

            Assert.That(controller.PersistentDamageBonus, Is.GreaterThan(0f));
            Assert.That(controller.ProjectileDamage, Is.GreaterThan(controller.CurrentTuning.ProjectileDamage));
            Assert.That(controller.PersistentMaxHealthBonus, Is.GreaterThan(0f));
            Assert.That(controller.MaxHealth, Is.GreaterThan(controller.CurrentTuning.PlayerMaxHealth));
            Assert.That(controller.PersistentPickupRangeBonus, Is.GreaterThan(0f));
            Assert.That(controller.PickupRangeBonus, Is.GreaterThan(0f));
            Assert.That(controller.PersistentExperienceGainMultiplierBonus, Is.GreaterThan(0f));
            Assert.That(controller.ExperienceGainMultiplierBonus, Is.GreaterThan(0f));
            Assert.That(controller.PersistentDraftRerollBonus, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.TotalDraftRerollCharges, Is.GreaterThan(controller.CurrentTuning.DraftRerollCharges));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RunResultMetaUpgradeOptionsSpendEarnedShardsBeforeRestart()
        {
            var storage = new InMemoryTextStorage();
            SurvivorsTemplateController controller = CreateController(storage, "play-result-meta-upgrade");
            yield return null;

            float baseProjectileDamage = controller.ProjectileDamage;
            controller.SpawnBloodShardForTest(controller.PlayerPosition + new Vector3(0.15f, 0f, 0.15f), 5);
            yield return SimulateFrames(controller, 8);
            Assert.That(controller.BonusBloodShardsEarnedThisRun, Is.GreaterThanOrEqualTo(5));

            controller.KillPlayerForTest();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.GameOver, controller.State);
            Assert.That(controller.MetaBloodShards, Is.GreaterThanOrEqualTo(5));
            IReadOnlyList<string> options = controller.GetResultMetaUpgradeOptionLabelsForTest();
            Assert.That(options.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(options[0], Does.StartWith("1."));
            Assert.That(options[0], Does.Contain("Arcane Legacy"));

            long shardsBeforePurchase = controller.MetaBloodShards;
            Assert.IsTrue(controller.TryPurchaseResultMetaUpgradeForTest(0));
            Assert.AreEqual(1, controller.GetPersistentUpgradeRankForTest(BasicSurvivorsGame.ArcaneLegacyMetaUpgradeId.Value));
            Assert.AreEqual(shardsBeforePurchase - 5, controller.MetaBloodShards);
            Assert.AreEqual(1, controller.MetaUpgradePurchaseCount);
            Assert.That(controller.LastMetaUpgradePurchaseFeedbackLabel, Does.Contain("Arcane Legacy"));

            controller.RestartRun();
            yield return null;

            Assert.That(controller.PersistentDamageBonus, Is.GreaterThan(0f));
            Assert.That(controller.ProjectileDamage, Is.GreaterThan(baseProjectileDamage));

            Object.Destroy(controller.gameObject);
        }

        private static IEnumerator DefeatBossAndResolveReward(SurvivorsTemplateController controller)
        {
            SurvivorsEnemyActor boss = controller.SpawnBossForTest(controller.PlayerPosition + new Vector3(3f, 0f, 0f), 1f);
            boss.ApplyDamage(100f, "test.boss");
            yield return null;

            if (controller.IsUpgradeRewardChoiceOpen)
            {
                Assert.IsTrue(controller.SkipCurrentDraft());
                yield return null;
            }
        }

        private static SurvivorsTemplateController CreateController(InMemoryTextStorage storage = null, string slot = null, bool startRun = true)
        {
            var root = new GameObject("Survivors Template PlayMode Test");
            SurvivorsTemplateController controller = root.AddComponent<SurvivorsTemplateController>();
            controller.ConfigureMetaPersistenceForTest(
                new PersistenceService(storage ?? new InMemoryTextStorage()),
                new SaveSlotId(string.IsNullOrWhiteSpace(slot) ? "play-" + Guid.NewGuid().ToString("N") : slot));
            if (startRun)
            {
                controller.StartRun();
            }

            return controller;
        }

        private static IEnumerator WaitForAsyncOperation(AsyncOperation operation, float timeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + Mathf.Max(0.1f, timeoutSeconds);
            while (operation != null && !operation.isDone && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.IsTrue(operation == null || operation.isDone);
        }

        private static T FindComponentInScene<T>(Scene scene)
            where T : Component
        {
            if (!scene.IsValid())
            {
                return null;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                T component = roots[i] == null ? null : roots[i].GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static GameObject FindRootInScene(Scene scene, string name)
        {
            if (!scene.IsValid() || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && string.Equals(roots[i].name, name, StringComparison.Ordinal))
                {
                    return roots[i];
                }
            }

            return null;
        }

        private static SurvivorsTemplateController CreateControllerWithClass(string classId)
        {
            SurvivorsTemplateController controller = CreateController(startRun: false);
            Assert.IsTrue(controller.UnlockClassForTest(classId));
            Assert.IsTrue(controller.TrySelectClassForTest(classId));
            controller.StartRun();
            return controller;
        }

        private static void ConfigureTrapOnlyTuning(SurvivorsTemplateTuning tuning)
        {
            tuning.EnemySpawnIntervalSeconds = 999f;
            tuning.EnemyMoveSpeed = 1.45f;
            tuning.ProjectileDamage = 0f;
            tuning.OrbitDamage = 0f;
            tuning.MeleeDamage = 0f;
            tuning.BurstDamage = 0f;
            tuning.HitscanDamage = 0f;
            tuning.GrenadeDamage = 0f;
            tuning.PlacedPayloadDamage = 6.8f;
            tuning.PlacedPayloadRange = 6.2f;
            tuning.PlacedPayloadArmingSeconds = 0.7f;
            tuning.PlacedPayloadLifetimeSeconds = 4.4f;
            tuning.PlacedPayloadTriggerRadius = 1.35f;
            tuning.PlacedPayloadExplosionRadius = 2.15f;
        }

        private static void ConfigureGrenadeOnlyTuning(SurvivorsTemplateTuning tuning)
        {
            tuning.EnemySpawnIntervalSeconds = 999f;
            tuning.EnemyMoveSpeed = 0f;
            tuning.EnemyContactDamage = 0f;
            tuning.ProjectileDamage = 0f;
            tuning.OrbitDamage = 0f;
            tuning.MeleeDamage = 0f;
            tuning.BurstDamage = 0f;
            tuning.HitscanDamage = 0f;
            tuning.GrenadeDamage = 8.5f;
            tuning.GrenadeCooldownSeconds = 2.4f;
            tuning.GrenadePayloadTravelSpeed = 9f;
            tuning.GrenadePayloadArmingSeconds = 0.7f;
            tuning.GrenadePayloadExplosionRadius = 2.35f;
            tuning.PlacedPayloadDamage = 0f;
        }

        private static void ConfigureProjectileModifierOnlyTuning(SurvivorsTemplateTuning tuning)
        {
            tuning.EnemySpawnIntervalSeconds = 999f;
            tuning.EnemyMoveSpeed = 0f;
            tuning.EnemyContactDamage = 0f;
            tuning.OrbitDamage = 0f;
            tuning.MeleeDamage = 0f;
            tuning.BurstDamage = 0f;
            tuning.HitscanDamage = 0f;
            tuning.GrenadeDamage = 0f;
            tuning.PlacedPayloadDamage = 0f;
        }

        private static void ConfigureBurstOnlyTuning(SurvivorsTemplateTuning tuning)
        {
            tuning.EnemySpawnIntervalSeconds = 999f;
            tuning.EnemyMoveSpeed = 0f;
            tuning.EnemyContactDamage = 0f;
            tuning.ProjectileDamage = 0f;
            tuning.OrbitDamage = 0f;
            tuning.MeleeDamage = 0f;
            tuning.BurstDamage = 6f;
            tuning.HitscanDamage = 0f;
            tuning.GrenadeDamage = 0f;
            tuning.PlacedPayloadDamage = 0f;
        }

        private static void ConfigureHitscanOnlyTuning(SurvivorsTemplateTuning tuning)
        {
            tuning.EnemySpawnIntervalSeconds = 999f;
            tuning.EnemyMoveSpeed = 0f;
            tuning.EnemyContactDamage = 0f;
            tuning.ProjectileDamage = 0f;
            tuning.OrbitDamage = 0f;
            tuning.MeleeDamage = 0f;
            tuning.BurstDamage = 0f;
            tuning.HitscanDamage = 6f;
            tuning.HitscanRange = 9.5f;
            tuning.HitscanWidth = 0.24f;
            tuning.GrenadeDamage = 0f;
            tuning.PlacedPayloadDamage = 0f;
        }

        private static void ConfigureMeleeOnlyTuning(SurvivorsTemplateTuning tuning)
        {
            tuning.EnemySpawnIntervalSeconds = 999f;
            tuning.EnemyMoveSpeed = 0f;
            tuning.EnemyContactDamage = 0f;
            tuning.ProjectileDamage = 0f;
            tuning.OrbitDamage = 0f;
            tuning.MeleeDamage = 6f;
            tuning.MeleeRange = 3.2f;
            tuning.MeleeArcDegrees = 100f;
            tuning.BurstDamage = 0f;
            tuning.HitscanDamage = 0f;
            tuning.GrenadeDamage = 0f;
            tuning.PlacedPayloadDamage = 0f;
        }

        private static int IndexOfDraftChoice(SurvivorsTemplateController controller, string upgradeId)
        {
            if (controller == null || string.IsNullOrWhiteSpace(upgradeId))
            {
                return -1;
            }

            for (int i = 0; i < controller.CurrentDraftChoices.Count; i++)
            {
                if (string.Equals(controller.CurrentDraftChoices[i].Id.Value, upgradeId, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private static IEnumerator SimulateFrames(SurvivorsTemplateController controller, int frames)
        {
            return SimulateFrames(controller, frames, default);
        }

        private static IEnumerator SimulateFrames(SurvivorsTemplateController controller, int frames, Vector2 movement)
        {
            for (int i = 0; i < frames; i++)
            {
                controller.Simulate(1f / 60f, movement);
                yield return null;
            }
        }
    }
}
