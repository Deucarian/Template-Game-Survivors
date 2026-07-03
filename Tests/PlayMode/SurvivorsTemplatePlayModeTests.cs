using System;
using System.Collections;
using Deucarian.Persistence;
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
            Assert.IsTrue(controller.SelectUpgrade(0));
            Assert.AreEqual(SurvivorsRunState.Playing, controller.State);

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
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Assert.Ignore("Imported playtest scene is not in this project's build settings.");
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
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
            Assert.IsFalse(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId));

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

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.bigger-booms"));
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(0.8f, 0f, 0f), 100f);

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Grenade));
            yield return SimulateFrames(controller, 80);

            Assert.That(controller.PayloadExplosionRadiusBonus, Is.GreaterThan(0f));
            Assert.That(controller.PayloadDetonationCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.PayloadExplosionHitCount, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator RunFlowCanSpawnMinibossAndBossOverTime()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            controller.Simulate(controller.CurrentTuning.MinibossSpawnTimeSeconds + 0.1f);
            yield return null;

            Assert.That(controller.MinibossSpawnCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.ActiveMinibossCount, Is.GreaterThanOrEqualTo(1));

            float bossDelta = controller.CurrentTuning.BossSpawnTimeSeconds - controller.RunTimeSeconds + 0.1f;
            controller.Simulate(bossDelta);
            yield return null;

            Assert.That(controller.BossSpawnCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.ActiveBossCount, Is.GreaterThanOrEqualTo(1));

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
            int evolutionChoiceIndex = IndexOfDraftChoice(controller, BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId);
            Assert.That(evolutionChoiceIndex, Is.GreaterThanOrEqualTo(0));

            Assert.IsTrue(controller.SelectUpgrade(evolutionChoiceIndex));
            yield return null;

            Assert.IsTrue(controller.HasEvolvedUpgradeForTest(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId));
            Assert.AreEqual(1, controller.SelectedRewardUpgradeCount);
            Assert.AreEqual(SurvivorsRunState.Victory, controller.State);
            Assert.IsTrue(controller.IsVictory);
            Assert.That(controller.BossRewardGrantCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.BloodShardsEarnedThisRun, Is.GreaterThanOrEqualTo(18));
            Assert.AreEqual(1, controller.MetaBossVictories);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator BossCanDieAndTriggerVictory()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            SurvivorsEnemyActor boss = controller.SpawnBossForTest(controller.PlayerPosition + new Vector3(3f, 0f, 0f), 1f);
            boss.ApplyDamage(100f, "test.boss");
            yield return null;

            Assert.That(controller.BossSpawnCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.BossKilledCount, Is.GreaterThanOrEqualTo(1));
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
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            controller.Simulate(controller.CurrentTuning.SurvivalVictoryTimeSeconds + 0.1f);
            yield return null;

            Assert.AreEqual(SurvivorsRunState.Victory, controller.State);
            Assert.IsTrue(controller.IsVictory);

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

            SurvivorsEnemyActor boss = first.SpawnBossForTest(first.PlayerPosition + new Vector3(3f, 0f, 0f), 1f);
            boss.ApplyDamage(100f, "test.boss");
            yield return null;
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

            SurvivorsEnemyActor boss = first.SpawnBossForTest(first.PlayerPosition + new Vector3(3f, 0f, 0f), 1f);
            boss.ApplyDamage(100f, "test.boss");
            yield return null;

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

            SurvivorsEnemyActor boss = controller.SpawnBossForTest(controller.PlayerPosition + new Vector3(3f, 0f, 0f), 1f);
            boss.ApplyDamage(100f, "test.boss");
            yield return null;

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

            SurvivorsEnemyActor boss = controller.SpawnBossForTest(controller.PlayerPosition + new Vector3(3f, 0f, 0f), 1f);
            boss.ApplyDamage(100f, "test.boss");
            yield return null;

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

            SurvivorsEnemyActor boss = controller.SpawnBossForTest(controller.PlayerPosition + new Vector3(3f, 0f, 0f), 1f);
            boss.ApplyDamage(100f, "test.boss");
            yield return null;

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
        public IEnumerator ClassSpecificUpgradeAppearsOnlyWhenValid()
        {
            SurvivorsTemplateController defaultController = CreateController();
            yield return null;

            defaultController.ForceLevelUpWithLockedChoiceForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId);
            yield return null;

            Assert.IsFalse(defaultController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId));
            Assert.IsTrue(defaultController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.ArcaneThesisUpgradeId));
            Assert.IsFalse(defaultController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.EmberForgeHeartUpgradeId));
            Assert.AreEqual(-1, IndexOfDraftChoice(defaultController, BasicSurvivorsGame.PrismaticBeamUpgradeId));
            Object.Destroy(defaultController.gameObject);
            yield return null;

            SurvivorsTemplateController emberController = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            yield return null;

            emberController.ForceLevelUpWithLockedChoiceForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId);
            yield return null;

            int gatedChoiceIndex = IndexOfDraftChoice(emberController, BasicSurvivorsGame.PrismaticBeamUpgradeId);
            Assert.That(gatedChoiceIndex, Is.GreaterThanOrEqualTo(0));
            Assert.IsFalse(emberController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.ArcaneThesisUpgradeId));
            Assert.IsTrue(emberController.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.EmberForgeHeartUpgradeId));
            Assert.IsTrue(emberController.SelectUpgrade(gatedChoiceIndex));
            Assert.That(emberController.HitscanPierceBonus, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(emberController.gameObject);
        }

        [UnityTest]
        public IEnumerator ClassUnlockPersistsAfterVictory()
        {
            var storage = new InMemoryTextStorage();
            const string slot = "play-class-unlock";
            SurvivorsTemplateController first = CreateController(storage, slot);
            yield return null;

            SurvivorsEnemyActor boss = first.SpawnBossForTest(first.PlayerPosition + new Vector3(3f, 0f, 0f), 1f);
            boss.ApplyDamage(100f, "test.boss");
            yield return null;

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
        public IEnumerator PersistentUpgradeAffectsNewRunDamage()
        {
            var storage = new InMemoryTextStorage();
            SurvivorsTemplateController controller = CreateController(storage, "play-meta-upgrade");
            yield return null;

            SurvivorsEnemyActor boss = controller.SpawnBossForTest(controller.PlayerPosition + new Vector3(3f, 0f, 0f), 1f);
            boss.ApplyDamage(100f, "test.boss");
            yield return null;

            Assert.IsTrue(controller.TryPurchasePersistentUpgradeForTest(BasicSurvivorsGame.ArcaneLegacyMetaUpgradeId.Value));
            Assert.AreEqual(1, controller.GetPersistentUpgradeRankForTest(BasicSurvivorsGame.ArcaneLegacyMetaUpgradeId.Value));

            controller.RestartRun();
            yield return null;

            Assert.That(controller.PersistentDamageBonus, Is.GreaterThan(0f));
            Assert.That(controller.ProjectileDamage, Is.GreaterThan(controller.CurrentTuning.ProjectileDamage));

            Object.Destroy(controller.gameObject);
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
