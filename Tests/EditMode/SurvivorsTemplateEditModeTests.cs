using Deucarian.Projectiles;
using Deucarian.RunUpgrades;
using Deucarian.WeaponSystems;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsTemplateEditModeTests
    {
        [Test]
        public void DefaultContentUsesStableDeucarianDescriptors()
        {
            WeaponDefinition weapon = BasicSurvivorsGame.CreateWeaponDefinition();
            ProjectileDefinition projectile = BasicSurvivorsGame.CreateProjectileDefinition();
            RunUpgradeCatalog upgrades = BasicSurvivorsGame.CreateRunUpgradeCatalog();

            Assert.AreEqual(BasicSurvivorsGame.ArcaneWandWeaponId, weapon.Id);
            Assert.AreEqual(WeaponFireMode.Projectile, weapon.FireMode);
            Assert.AreEqual(BasicSurvivorsGame.ArcaneBoltProjectileId, projectile.Id);
            Assert.AreEqual(BasicSurvivorsGame.ProjectileSpawnableId, projectile.SpawnableId);
            Assert.That(upgrades.Definitions.Count, Is.GreaterThanOrEqualTo(5));
            Assert.IsNotNull(BasicSurvivorsGame.CreateEncounterDefinition());
        }

        [Test]
        public void RunUpgradeDraftOffersThreeDeterministicChoices()
        {
            RunUpgradeCatalog catalog = BasicSurvivorsGame.CreateRunUpgradeCatalog();
            var state = new RunUpgradeState();

            RunUpgradeDraft first = RunUpgradeDraftService.Generate(catalog, state, new RunUpgradeDraftRequest(3, 20260624));
            RunUpgradeDraft second = RunUpgradeDraftService.Generate(catalog, state, new RunUpgradeDraftRequest(3, 20260624));

            Assert.AreEqual(3, first.Choices.Count);
            Assert.AreEqual(3, second.Choices.Count);
            for (int i = 0; i < first.Choices.Count; i++)
            {
                Assert.AreEqual(first.Choices[i].Id, second.Choices[i].Id);
            }
        }

        [Test]
        public void RadialSpawningCreatesEnemyAroundPlayer()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                controller.Simulate(1.1f);

                Assert.That(controller.SpawnedCount, Is.GreaterThanOrEqualTo(1));
                Assert.That(controller.ActiveEnemyCount, Is.GreaterThanOrEqualTo(1));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void WeaponProjectileCanKillEnemyAndDropExperience()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.2f, 0f, 0f), 1f);
                controller.FireWeaponForTest();
                Step(controller, 90, 1f / 60f);

                Assert.That(controller.ProjectileLaunchCount, Is.GreaterThanOrEqualTo(1));
                Assert.That(controller.KilledCount, Is.GreaterThanOrEqualTo(1));
                Assert.That(controller.ExperienceCollected + controller.ActivePickupCount, Is.GreaterThanOrEqualTo(1));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void ExperienceCollectionOpensLevelUpDraft()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(0.15f, 0f, 0.1f), controller.RequiredExperienceForNextLevel);
                Step(controller, 8, 1f / 30f);

                Assert.AreEqual(SurvivorsRunState.LevelUp, controller.State);
                Assert.AreEqual(3, controller.CurrentDraftChoices.Count);
                Assert.That(controller.ExperienceCollected, Is.GreaterThanOrEqualTo(5));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void SelectingUpgradeAppliesEffectAndResumesRun()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                controller.ForceLevelUp();
                float previousDamage = controller.ProjectileDamage;
                float previousMove = controller.PlayerMoveSpeed;
                float previousCooldown = controller.WeaponCooldownSeconds;
                float previousMaxHealth = controller.MaxHealth;

                Assert.IsTrue(controller.SelectUpgrade(0));

                Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
                Assert.AreEqual(1, controller.SelectedUpgradeCount);
                bool changed = controller.ProjectileDamage > previousDamage ||
                    controller.PlayerMoveSpeed > previousMove ||
                    controller.WeaponCooldownSeconds < previousCooldown ||
                    controller.MaxHealth > previousMaxHealth ||
                    controller.PickupRangeBonus > 0f;
                Assert.IsTrue(changed);
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void MagnetRecallPullsExperienceGemsIntoPlayer()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(8f, 0f, 0f), 1);
                controller.SpawnExperienceForTest(controller.PlayerPosition + new Vector3(-7f, 0f, 5f), 1);
                controller.SpawnMagnetForTest(controller.PlayerPosition + new Vector3(0.1f, 0f, 0.1f));
                Step(controller, 180, 1f / 60f);

                Assert.That(controller.MagnetRecallCount, Is.GreaterThanOrEqualTo(1));
                Assert.That(controller.ExperienceCollected, Is.GreaterThanOrEqualTo(2));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        private static SurvivorsTemplateController CreateController()
        {
            var root = new GameObject("Survivors Template EditMode Test");
            SurvivorsTemplateController controller = root.AddComponent<SurvivorsTemplateController>();
            controller.StartRun();
            return controller;
        }

        private static void Step(SurvivorsTemplateController controller, int frames, float deltaTime)
        {
            for (int i = 0; i < frames; i++)
            {
                controller.Simulate(deltaTime);
            }
        }

        private static void DestroyController(SurvivorsTemplateController controller)
        {
            if (controller != null)
            {
                Object.DestroyImmediate(controller.gameObject);
            }
        }
    }
}
