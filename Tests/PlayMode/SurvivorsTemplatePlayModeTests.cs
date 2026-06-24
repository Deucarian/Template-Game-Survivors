using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
        public IEnumerator OrbitWeaponCanDamageAndKillEnemy()
        {
            SurvivorsTemplateController controller = CreateController();
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
            SurvivorsTemplateController controller = CreateController();
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
            SurvivorsTemplateController controller = CreateController();
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
            SurvivorsTemplateController controller = CreateController();
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
            SurvivorsTemplateController controller = CreateController();
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
            SurvivorsTemplateController controller = CreateController();
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
            SurvivorsTemplateController controller = CreateController();
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

        private static SurvivorsTemplateController CreateController()
        {
            var root = new GameObject("Survivors Template PlayMode Test");
            SurvivorsTemplateController controller = root.AddComponent<SurvivorsTemplateController>();
            controller.StartRun();
            return controller;
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
