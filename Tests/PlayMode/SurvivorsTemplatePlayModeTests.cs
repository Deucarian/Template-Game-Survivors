using System;
using System.Collections;
using Deucarian.Persistence;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Deucarian.TemplateGameSurvivors.PlayModeTests
{
    public sealed class SurvivorsTemplatePlayModeTests
    {
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
            Assert.AreEqual(1, controller.ActiveWeaponCount);
            Assert.IsTrue(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.ArcaneWandWeaponContentId));
            Assert.IsFalse(controller.HasWeaponInLoadoutForTest(BasicSurvivorsGame.StarBeamWeaponContentId));
            Assert.IsFalse(controller.IsUpgradeAvailableInRunForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId));

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
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.1f, 0f, 0f), 1f);
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.75f, 0f, 0f), 1f);

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
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.forked-bolts"));
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.1f, 0f, 0f), 1f);
            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.1f, 0f, 1.2f), 1f);

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
            SurvivorsTemplateController controller = CreateController();
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
            SurvivorsTemplateController controller = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            yield return null;

            controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(1.1f, 0f, 0f), 30f);

            Assert.IsTrue(controller.FireWeaponForTest(SurvivorsWeaponArchetype.Trap));
            yield return SimulateFrames(controller, 80);

            Assert.That(controller.PayloadPlacedCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.PayloadDetonationCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.PayloadExplosionHitCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(1));

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
        public IEnumerator MinibossDeathOpensBossRelicChoice()
        {
            SurvivorsTemplateController controller = CreateController();
            yield return null;

            SurvivorsEnemyActor miniboss = controller.SpawnMinibossForTest(controller.PlayerPosition + new Vector3(2.4f, 0f, 0f), 1f);
            miniboss.ApplyDamage(100f, "test.miniboss");
            yield return null;

            Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
            Assert.IsTrue(controller.IsRelicChoiceOpen);
            Assert.That(controller.BossRelicDraftOpenCount, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(3, controller.CurrentRelicChoices.Count);

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
            Assert.AreEqual(-1, IndexOfDraftChoice(defaultController, BasicSurvivorsGame.PrismaticBeamUpgradeId));
            Object.Destroy(defaultController.gameObject);
            yield return null;

            SurvivorsTemplateController emberController = CreateControllerWithClass(BasicSurvivorsGame.EmberVanguardClassId);
            yield return null;

            emberController.ForceLevelUpWithLockedChoiceForTest(BasicSurvivorsGame.PrismaticBeamUpgradeId);
            yield return null;

            int gatedChoiceIndex = IndexOfDraftChoice(emberController, BasicSurvivorsGame.PrismaticBeamUpgradeId);
            Assert.That(gatedChoiceIndex, Is.GreaterThanOrEqualTo(0));
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
            Assert.That(controller.ProjectileDamage, Is.GreaterThan(7f));

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
