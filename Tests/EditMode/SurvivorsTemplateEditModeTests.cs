using System;
using System.IO;
using Deucarian.Projectiles;
using Deucarian.RunUpgrades;
using Deucarian.WeaponSystems;
using NUnit.Framework;
using UnityEditor.PackageManager;
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
            var archetypes = BasicSurvivorsGame.CreateWeaponArchetypeDefinitions();

            Assert.AreEqual(BasicSurvivorsGame.ArcaneWandWeaponId, weapon.Id);
            Assert.AreEqual(WeaponFireMode.Projectile, weapon.FireMode);
            Assert.AreEqual(BasicSurvivorsGame.ArcaneBoltProjectileId, projectile.Id);
            Assert.AreEqual(BasicSurvivorsGame.ProjectileSpawnableId, projectile.SpawnableId);
            Assert.That(upgrades.Definitions.Count, Is.GreaterThanOrEqualTo(16));
            Assert.That(archetypes.Count, Is.EqualTo(8));
            Assert.That(archetypes[0].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Projectile));
            Assert.That(archetypes[1].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Orbit));
            Assert.That(archetypes[2].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Melee));
            Assert.That(archetypes[3].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Burst));
            Assert.That(archetypes[4].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Hitscan));
            Assert.That(archetypes[5].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Grenade));
            Assert.That(archetypes[6].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Trap));
            Assert.That(archetypes[7].Archetype, Is.EqualTo(SurvivorsWeaponArchetype.Mine));
            Assert.IsNotNull(BasicSurvivorsGame.CreateEncounterDefinition());
        }

        [Test]
        public void RuntimeContentValidationPassesForDefaultCatalogs()
        {
            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateRuntimeContent(
                BasicSurvivorsGame.CreateWeaponArchetypeDefinitions(),
                BasicSurvivorsGame.CreateRunUpgradeCatalog(),
                BasicSurvivorsGame.CreateKnownUpgradeTargets());

            Assert.IsTrue(result.Succeeded, string.Join(Environment.NewLine, result.Errors));
        }

        [Test]
        public void SampleContentJsonLoadsAndValidates()
        {
            string sampleRoot = GetSampleRoot();
            string weaponJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultWeapons", "weapons.json"));
            string upgradeJson = File.ReadAllText(Path.Combine(sampleRoot, "Content", "DefaultUpgrades", "upgrades.json"));

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson);

            Assert.IsTrue(result.Succeeded, string.Join(Environment.NewLine, result.Errors));
        }

        [Test]
        public void InvalidSampleContentReportsDuplicateAndMissingReferences()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.dup\",\"fireMode\":\"Projectile\",\"projectileId\":\"projectile.missing\"},{\"id\":\"weapon.dup\",\"fireMode\":\"NoSuchArchetype\"}],\"projectiles\":[]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.dup\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"target.missing\"},{\"id\":\"upgrade.dup\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"survivors.player\"}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("Duplicate weapon id", errors);
            StringAssert.Contains("references missing projectile id", errors);
            StringAssert.Contains("unknown archetype", errors);
            StringAssert.Contains("Duplicate upgrade id", errors);
            StringAssert.Contains("unknown target", errors);
        }

        [Test]
        public void InvalidPayloadSampleContentReportsPayloadErrors()
        {
            string weaponJson = "{\"weapons\":[{\"id\":\"weapon.payload.bad\",\"fireMode\":\"Grenade\",\"payloadCount\":0,\"payloadTravelSpeed\":0,\"payloadArmingSeconds\":0,\"payloadLifetimeSeconds\":0,\"payloadTriggerRadius\":0,\"payloadExplosionRadius\":0,\"payloadLeavesHazard\":true,\"payloadHazardDurationSeconds\":0,\"payloadHazardTickIntervalSeconds\":0,\"payloadHazardDamageRatio\":-1}],\"projectiles\":[]}";
            string upgradeJson = "{\"upgrades\":[{\"id\":\"upgrade.valid\",\"rarity\":\"Common\",\"effect\":\"effect.test\",\"target\":\"survivors.weapon.payloads\"}]}";

            SurvivorsContentValidationResult result = SurvivorsContentValidator.ValidateSampleJson(weaponJson, upgradeJson);
            string errors = string.Join(Environment.NewLine, result.Errors);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("payload count", errors);
            StringAssert.Contains("payload travel speed", errors);
            StringAssert.Contains("payload arming time", errors);
            StringAssert.Contains("payload lifetime", errors);
            StringAssert.Contains("payload trigger radius", errors);
            StringAssert.Contains("payload explosion radius", errors);
            StringAssert.Contains("hazard duration", errors);
            StringAssert.Contains("hazard tick interval", errors);
            StringAssert.Contains("negative hazard damage ratio", errors);
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
                int previousProjectilePierce = controller.ProjectilePierceBonus;
                int previousProjectileChain = controller.ProjectileChainBonus;
                int previousProjectileFork = controller.ProjectileForkBonus;
                int previousProjectileReturn = controller.ProjectileReturnBonus;
                int previousHitscanPierce = controller.HitscanPierceBonus;
                int previousPayloadCount = controller.PayloadCountBonus;
                float previousPayloadRadius = controller.PayloadExplosionRadiusBonus;
                float previousPayloadTriggerRadius = controller.PayloadTriggerRadiusBonus;

                Assert.IsTrue(controller.SelectUpgrade(0));

                Assert.AreEqual(SurvivorsRunState.Playing, controller.State);
                Assert.AreEqual(1, controller.SelectedUpgradeCount);
                bool changed = controller.ProjectileDamage > previousDamage ||
                    controller.PlayerMoveSpeed > previousMove ||
                    controller.WeaponCooldownSeconds < previousCooldown ||
                    controller.MaxHealth > previousMaxHealth ||
                    controller.PickupRangeBonus > 0f ||
                    controller.ProjectilePierceBonus > previousProjectilePierce ||
                    controller.ProjectileChainBonus > previousProjectileChain ||
                    controller.ProjectileForkBonus > previousProjectileFork ||
                    controller.ProjectileReturnBonus > previousProjectileReturn ||
                    controller.HitscanPierceBonus > previousHitscanPierce ||
                    controller.PayloadCountBonus > previousPayloadCount ||
                    controller.PayloadExplosionRadiusBonus > previousPayloadRadius ||
                    controller.PayloadTriggerRadiusBonus > previousPayloadTriggerRadius;
                Assert.IsTrue(changed);
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void OrbitUpgradeAddsBladeToLocalWeaponRuntime()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                controller.SpawnEnemyForTest(controller.PlayerPosition + new Vector3(2.1f, 0f, 0f), 20f);
                Step(controller, 1, 1f / 60f);
                int baseline = controller.ActiveOrbitBladeCount;

                Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.orbiting-focus"));
                Step(controller, 1, 1f / 60f);

                Assert.That(baseline, Is.GreaterThanOrEqualTo(1));
                Assert.That(controller.OrbitBladeBonus, Is.GreaterThanOrEqualTo(1));
                Assert.That(controller.ActiveOrbitBladeCount, Is.GreaterThan(baseline));
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void ProjectileModifierUpgradeAppliesLocalBonus()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                Assert.AreEqual(0, controller.ProjectilePierceBonus);

                Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.piercing-bolts"));

                Assert.That(controller.ProjectilePierceBonus, Is.GreaterThanOrEqualTo(1));
                Assert.AreEqual(1, controller.SelectedUpgradeCount);
            }
            finally
            {
                DestroyController(controller);
            }
        }

        [Test]
        public void PayloadUpgradeAppliesLocalBonus()
        {
            SurvivorsTemplateController controller = CreateController();
            try
            {
                Assert.AreEqual(0f, controller.PayloadExplosionRadiusBonus);

                Assert.IsTrue(controller.ApplyUpgradeByIdForTest("upgrade.survivors.bigger-booms"));

                Assert.That(controller.PayloadExplosionRadiusBonus, Is.GreaterThan(0f));
                Assert.AreEqual(1, controller.SelectedUpgradeCount);
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

        private static string GetSampleRoot()
        {
            var packageInfo = PackageInfo.FindForAssembly(typeof(BasicSurvivorsGame).Assembly);
            Assert.IsNotNull(packageInfo);
            string sampleRoot = Path.Combine(packageInfo.resolvedPath, "Samples~", "BasicSurvivorsGame");
            Assert.IsTrue(Directory.Exists(sampleRoot), "Sample root missing: " + sampleRoot);
            return sampleRoot;
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
                UnityEngine.Object.DestroyImmediate(controller.gameObject);
            }
        }
    }
}
