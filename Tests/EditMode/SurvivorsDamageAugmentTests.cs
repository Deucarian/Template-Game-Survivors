using System;
using System.Linq;
using Deucarian.Combat;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsDamageAugmentTests
    {
        [Test]
        public void HealthDamageDrivesHealingBarrierStatusesThenExecuteInExistingOrder()
        {
            var host = new SurvivorsDamageAugmentTestHost { Values = new SurvivorsDamageAugmentValues(0.2f, 0.3f, 0.4f, 0.5f, 0.2f) };
            var augments = new SurvivorsDamageAugments(host);
            augments.ApplyDamageAugmentsToEnemy(host, Damage(10f), "weapon.test");
            CollectionAssert.AreEqual(new[] { "heal", "barrier", "status.survivors.poison", "status.survivors.bleed", "execute:weapon.test" }, host.Events);
            Assert.AreEqual(2f, host.Healed);
            Assert.AreEqual(3f, host.Barrier);
            CollectionAssert.AreEqual(new[] { 4f, 5f }, host.StatusAmounts);
            CollectionAssert.AreEqual(new[] { 4f, 2f }, host.StatusDurations);
            CollectionAssert.AreEqual(new[] { "weapon.test", "weapon.test" }, host.StatusSources);
            Assert.IsFalse(host.IsAlive);
        }

        [TestCase("weapon.status.poison")]
        [TestCase("weapon.augment.execute")]
        [TestCase("combatant.survivors.enemy.12")]
        public void RecursiveAndEnemyDamageCannotTriggerAnyAugment(string source)
        {
            var host = new SurvivorsDamageAugmentTestHost { Values = new SurvivorsDamageAugmentValues(1f, 1f, 1f, 1f, 1f) };
            new SurvivorsDamageAugments(host).ApplyDamageAugmentsToEnemy(host, Damage(10f), source);
            Assert.IsEmpty(host.Events);
            Assert.IsTrue(host.IsAlive);
        }

        [Test]
        public void MissingDeadAndZeroHealthDamageTargetsDoNotApplyAugments()
        {
            var host = new SurvivorsDamageAugmentTestHost { Values = new SurvivorsDamageAugmentValues(1f, 1f, 1f, 1f, 1f) };
            var augments = new SurvivorsDamageAugments(host);
            augments.ApplyDamageAugmentsToEnemy(null, Damage(10f), null);
            augments.ApplyDamageAugmentsToEnemy(host, null, null);
            augments.ApplyDamageAugmentsToEnemy(host, Damage(0f), null);
            host.IsAlive = false;
            augments.ApplyDamageAugmentsToEnemy(host, Damage(10f), null);
            Assert.IsEmpty(host.Events);
        }

        [Test]
        public void LiveModifierChangesAndPlayerBindingOnlyGateTheRelevantEffects()
        {
            var host = new SurvivorsDamageAugmentTestHost { IsPlayerBound = false };
            var augments = new SurvivorsDamageAugments(host);
            augments.ApplyDamageAugmentsToEnemy(host, Damage(10f), null);
            Assert.IsEmpty(host.Events);
            host.Values = new SurvivorsDamageAugmentValues(0.5f, 0.2f, 0f, 0f, 0f);
            augments.ApplyDamageAugmentsToEnemy(host, Damage(10f), null);
            Assert.AreEqual(0f, host.Healed);
            Assert.AreEqual(2f, host.Barrier);
            host.IsPlayerBound = true;
            augments.ApplyDamageAugmentsToEnemy(host, Damage(10f), null);
            Assert.AreEqual(5f, host.Healed);
            Assert.AreEqual(4f, host.Barrier);
        }

        [Test]
        public void FrostUsesLiveEvolutionAndAuthoredNamesWithoutRequiringDamageResult()
        {
            var host = new SurvivorsDamageAugmentTestHost();
            var augments = new SurvivorsDamageAugments(host);
            var frost = Weapon(BasicSurvivorsGame.FrostFanWeaponContentId);
            augments.ApplyWeaponStatusEffectsToEnemy(host, frost, null);
            Assert.AreEqual(0.68f, host.SlowMultiplier);
            Assert.AreEqual(1.65f, host.SlowDuration);
            Assert.AreEqual("Authored weapon " + frost.Id + " chilled Target", augments.LastFrostFanSlowFeedbackLabel);
            host.Evolutions.Add(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId);
            augments.ApplyWeaponStatusEffectsToEnemy(host, frost, null);
            Assert.AreEqual(0.52f, host.SlowMultiplier);
            Assert.AreEqual(2.35f, host.SlowDuration);
            Assert.AreEqual("Authored evolution " + BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId + " chilled Target", augments.LastFrostFanSlowFeedbackLabel);
            Assert.AreEqual(2, augments.FrostFanSlowApplicationCount);
            host.AcceptSlow = false;
            augments.ApplyWeaponStatusEffectsToEnemy(host, frost, null);
            Assert.AreEqual(2, augments.FrostFanSlowApplicationCount);
            augments.Reset();
            Assert.AreEqual(0, augments.FrostFanSlowApplicationCount);
            Assert.AreEqual(string.Empty, augments.LastFrostFanSlowFeedbackLabel);
        }

        [TestCase(false, 2.6f, 3f, "Cinder Burst burned Target")]
        [TestCase(true, 4.2f, 4.35f, "Inferno Heart burned Target")]
        public void BurnUsesHealthDamageAndEvolvedDurationWithoutGenericPoisonRatios(bool evolved, float damage, float duration, string label)
        {
            var host = new SurvivorsDamageAugmentTestHost { Values = new SurvivorsDamageAugmentValues(5f, 5f, 5f, 5f, 5f) };
            if (evolved) host.Evolutions.Add(BasicSurvivorsGame.InfernoHeartEvolutionUpgradeId);
            var augments = new SurvivorsDamageAugments(host);
            var weapon = Weapon(BasicSurvivorsGame.StarNovaWeaponContentId);
            augments.ApplyWeaponStatusEffectsToEnemy(host, weapon, null);
            augments.ApplyWeaponStatusEffectsToEnemy(host, weapon, Damage(0f));
            Assert.AreEqual(0, augments.CinderBurnApplicationCount);
            augments.ApplyWeaponStatusEffectsToEnemy(host, weapon, Damage(10f));
            CollectionAssert.AreEqual(new[] { "status.survivors.burn" }, host.Events);
            Assert.AreEqual(damage, host.StatusAmounts.Single(), 0.00001f);
            Assert.AreEqual(duration, host.StatusDurations.Single(), 0.00001f);
            Assert.AreEqual(weapon.Id, host.StatusSources.Single());
            Assert.AreEqual(label, augments.LastCinderBurnFeedbackLabel);
            Assert.AreEqual(1, augments.CinderBurnApplicationCount);
            augments.Reset();
            Assert.AreEqual(0, augments.CinderBurnApplicationCount);
            Assert.AreEqual(string.Empty, augments.LastCinderBurnFeedbackLabel);
        }

        [Test]
        public void StatusAndExplicitlyDisabledCriticalHitsDoNotConsumeSeededRandomStream()
        {
            var first = new SurvivorsEnemyDamage(() => 0.5f, () => 2f);
            var second = new SurvivorsEnemyDamage(() => 0.5f, () => 2f);
            first.Reset(123);
            second.Reset(123);
            for (int i = 0; i < 12; i++)
            {
                var status = first.ResolveEnemyDamage(Health(), 10f, "weapon.status.poison", true);
                var disabled = first.ResolveEnemyDamage(Health(), 10f, "weapon", false);
                Assert.IsFalse(status.Critical.ConsumedRandom);
                Assert.IsFalse(disabled.Critical.ConsumedRandom);
                Assert.IsFalse(status.Critical.IsCritical);
                Assert.AreEqual(10d, status.HealthDamage);
                var actual = first.ResolveEnemyDamage(Health(), 10f, "weapon", true);
                var expected = second.ResolveEnemyDamage(Health(), 10f, "weapon", true);
                Assert.IsTrue(actual.Critical.ConsumedRandom);
                Assert.AreEqual(expected.Critical.Roll, actual.Critical.Roll);
                Assert.AreEqual(expected.HealthDamage, actual.HealthDamage);
            }
            first.Reset(123);
            second.Reset(123);
            Assert.AreEqual(second.ResolveEnemyDamage(Health(), 10f, null, true).Critical.Roll,
                first.ResolveEnemyDamage(Health(), 10f, null, true).Critical.Roll);
        }

        [Test]
        public void DamageResolutionReadsLiveCriticalValuesAndMutatesOnlySuppliedHealth()
        {
            float chance = 0f;
            float multiplier = 2f;
            var damage = new SurvivorsEnemyDamage(() => chance, () => multiplier);
            Assert.IsNull(damage.Catalog);
            Assert.IsNull(damage.ResolveEnemyDamage(null, 10f, "weapon", true));
            damage.Reset(7);
            Assert.IsNotNull(damage.Catalog);
            Assert.AreEqual(10d, damage.ResolveEnemyDamage(Health(), 10f, "weapon", true).HealthDamage);
            chance = 1f;
            multiplier = 3f;
            var health = Health();
            var result = damage.ResolveEnemyDamage(health, 10f, "  ", true);
            Assert.IsTrue(result.Critical.IsCritical);
            Assert.AreEqual(30d, result.HealthDamage);
            Assert.AreEqual(70d, health.CurrentHealth);
            Assert.IsTrue(SurvivorsEnemyDamage.CanApplyDamageAugments("STATUS.survivors.poison"), "Existing ordinal substring contract is case-sensitive.");
            Assert.IsTrue(SurvivorsEnemyDamage.CanApplyDamageAugments(null));
        }

        [Test]
        public void ControllerBoundaryRejectsDestroyedUnityTargetsBeforeInterfaceDispatch()
        {
            var root = new UnityEngine.GameObject("augment-boundary");
            root.SetActive(false);
            var target = new UnityEngine.GameObject("augment-target");
            target.SetActive(false);
            try
            {
                var controller = root.AddComponent<SurvivorsTemplateController>();
                var enemy = target.AddComponent<SurvivorsEnemyActor>();
                enemy.Initialize(null, BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Swarm, new SurvivorsTemplateTuning()));
                UnityEngine.Object.DestroyImmediate(target);
                Assert.IsTrue(enemy == null);
                Assert.DoesNotThrow(() => controller.ApplyDamageAugmentsToEnemy(enemy, Damage(10f), "weapon"));
                Assert.DoesNotThrow(() => controller.ApplyWeaponStatusEffectsToEnemy(enemy, Weapon(BasicSurvivorsGame.FrostFanWeaponContentId), null));
                Assert.AreEqual(0, controller.FrostFanSlowApplicationCount);
                Assert.AreEqual(0, controller.CinderBurnApplicationCount);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                if (target != null) UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static HealthState Health() => new HealthState(new CombatantId("enemy.test"), 100d, 100d);
        private static SurvivorsWeaponArchetypeDefinition Weapon(string id) =>
            BasicSurvivorsGame.CreateWeaponArchetypeDefinitions(new SurvivorsTemplateTuning()).First(x => x.Id == id);
        private static DamageResult Damage(float amount) => new DamageResult(CombatStatus.Success, default,
            Array.Empty<DamageComponentResult>(), 40d, 30d, 20d, amount, 0d, default, default, Array.Empty<StatusApplicationResult>());
    }
}
