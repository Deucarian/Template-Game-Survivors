using System;
using System.Collections;
using Deucarian.Persistence;
using Deucarian.RunUpgrades;
using NUnit.Framework;
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
            const string sceneName = "PLAYTEST_THIS_SCENE_Survivors_Game";
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
        public IEnumerator RapidKillsCreateStreakBonusDropsAndMagnet()
        {
            SurvivorsTemplateController controller = CreateController();
            controller.CurrentTuning.EnemySpawnIntervalSeconds = 999f;
            yield return null;

            for (int i = 0; i < 24; i++)
            {
                SurvivorsEnemyActor enemy = controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(7f + i * 0.08f, 0f, 0f), 1f);
                Assert.NotNull(enemy);
                enemy.ApplyDamage(100f, "test.streak");
            }

            Assert.AreEqual(24, controller.CurrentKillStreak);
            Assert.AreEqual(24, controller.BestKillStreak);
            Assert.That(controller.StreakBonusDropCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(controller.StreakMagnetDropCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.ActivePickupCount, Is.GreaterThanOrEqualTo(28));

            controller.Simulate(4.5f);
            yield return null;

            Assert.AreEqual(0, controller.CurrentKillStreak);

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
            controller.RestartRun();
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.That(controller.CurrentHealth, Is.GreaterThan(0f));

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
        public IEnumerator WeaponEvolutionRequiresRankedWeaponPathAndPassive()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.AreEqual(SurvivorsRunUpgradeCategory.Evolution, controller.GetUpgradeCategoryForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            Assert.That(controller.GetUpgradeDescriptionForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId), Does.Contain("Evolution"));

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.arcane-damage"));
            }

            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ArcaneThesisUpgradeId));
            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));

            float previousDamage = controller.DamageBonus;
            int previousChains = controller.ProjectileChainBonus;
            int previousForks = controller.ProjectileForkBonus;
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));

            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            Assert.AreEqual(1, controller.EvolvedWeaponCount);
            Assert.That(controller.DamageBonus, Is.GreaterThan(previousDamage));
            Assert.That(controller.ProjectileChainBonus, Is.GreaterThan(previousChains));
            Assert.That(controller.ProjectileForkBonus, Is.GreaterThan(previousForks));

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

            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.ThornHaloUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.HaloSpiralUpgradeId));
            for (int i = 0; i < 2; i++)
            {
                Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.OrbitingFocusUpgradeId));
            }

            Assert.IsTrue(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.ThornHaloUpgradeId));
            Assert.IsFalse(controller.IsUpgradeEligibleInCurrentBuildForTest(BasicSurvivorsGame.HaloSpiralUpgradeId));
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicSurvivorsGame.OrbitingFocusUpgradeId));
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

            controller.Simulate(1.4f);
            yield return null;

            Assert.AreEqual(0, controller.ActiveMajorRewardDropFeedbackCount);
            Assert.IsTrue(controller.SkipCurrentDraft());
            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.That(controller.LastRewardSelectionFeedbackLabel, Does.Contain("Elite Reward"));
            Assert.That(controller.ActiveRewardFeedbackLabel, Does.Contain("skipped"));

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
        public IEnumerator MinibossDeathOpensEliteUpgradeRewardChoice()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            SurvivorsEnemyActor miniboss = controller.SpawnMinibossForTest(controller.PlayerPosition + new Vector3(2.4f, 0f, 0f), 1f);
            miniboss.ApplyDamage(100f, "test.miniboss");
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.IsTrue(controller.IsUpgradeRewardChoiceOpen);
            Assert.IsFalse(controller.IsRelicChoiceOpen);
            Assert.That(controller.EliteUpgradeDraftOpenCount, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
            Assert.IsTrue(controller.SelectUpgrade(0));
            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.AreEqual(1, controller.SelectedRewardUpgradeCount);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator SelectedBossRelicAffectsCurrentRun()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.IsTrue(controller.OpenBossRelicDraftForTest());
            float previousRelicDamage = controller.RelicDamageBonus;
            float previousRelicCooldown = controller.RelicCooldownMultiplierBonus;
            float previousRelicPickup = controller.RelicPickupRangeBonus;

            Assert.IsTrue(controller.SelectRelicForTest(0));
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
            Assert.AreEqual(1, controller.SelectedRelicCount);
            bool relicChanged = controller.RelicDamageBonus != previousRelicDamage ||
                controller.RelicCooldownMultiplierBonus != previousRelicCooldown ||
                controller.RelicPickupRangeBonus != previousRelicPickup;
            Assert.IsTrue(relicChanged);
            Assert.That(controller.RewardCardPresentationCount, Is.GreaterThanOrEqualTo(3));
            Assert.AreEqual(1, controller.RewardSelectionFeedbackCount);
            Assert.That(controller.LastRewardSelectionFeedbackLabel, Does.Contain("Boss Relic"));
            Assert.That(controller.ActiveRewardFeedbackLabel, Does.Contain("Boss Relic"));

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
            Object.Destroy(defaultController.gameObject);
            yield return null;

            SurvivorsTemplateController controller = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            yield return null;

            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.MoonSlashWeaponContentId));
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
