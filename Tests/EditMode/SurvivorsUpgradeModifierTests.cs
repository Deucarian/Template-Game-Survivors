using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsUpgradeModifierTests
    {
        [Test]
        public void PersistentReapplicationUsesDeltasAndPreservesRunUpgrades()
        {
            var sink = new EffectSink();
            var modifiers = new SurvivorsUpgradeModifiers(sink);
            modifiers.Apply(Upgrade(Effect(BasicSurvivorsGame.DamageBonusEffect, 3)));
            var bonuses = new SurvivorsPersistentBonuses(5f, 10f, 2f, 0.2f, 2f);
            modifiers.ApplyPersistent(bonuses);
            modifiers.ApplyPersistent(bonuses);
            Assert.AreEqual(8f, modifiers.DamageBonus);
            Assert.AreEqual(2f, modifiers.PickupRangeBonus);
            Assert.AreEqual(0.2f, modifiers.ExperienceGainMultiplierBonus);
            Assert.AreEqual(2, modifiers.PersistentDraftRerollBonus);
            CollectionAssert.AreEqual(new[] { "health:10" }, sink.Events);
            modifiers.ApplyPersistent(new SurvivorsPersistentBonuses(6f, 15f, 3f, 0.4f, 3f));
            Assert.AreEqual(9f, modifiers.DamageBonus);
            CollectionAssert.AreEqual(new[] { "health:10", "health:5" }, sink.Events);
            modifiers.Reset();
            Assert.AreEqual(0f, modifiers.DamageBonus);
            Assert.AreEqual(0f, modifiers.PersistentMaxHealthBonus);
        }

        [Test]
        public void UpgradeEffectsRetainAuthoredOrderAndObserveUpdatedModifiers()
        {
            var sink = new EffectSink();
            var modifiers = new SurvivorsUpgradeModifiers(sink);
            sink.Modifiers = modifiers;
            modifiers.Apply(Upgrade(
                Effect(BasicSurvivorsGame.MagnetPulseEffect, 2),
                Effect(BasicSurvivorsGame.MaxHealthEffect, 4),
                Effect(BasicSurvivorsGame.BarrierCapacityEffect, 6)));
            CollectionAssert.AreEqual(new[] { "magnet:2", "health:4", "barrier:6:6" }, sink.Events);
        }

        [Test]
        public void ModifierCapsAndMinimumCountsMatchTheExistingGameplayPolicy()
        {
            var modifiers = new SurvivorsUpgradeModifiers(new EffectSink());
            modifiers.Apply(Upgrade(
                Effect(BasicSurvivorsGame.FireRateEffect, -2),
                Effect(BasicSurvivorsGame.CriticalChanceEffect, 2),
                Effect(BasicSurvivorsGame.ExecuteEffect, 2),
                Effect(BasicSurvivorsGame.ProjectilePierceEffect, 0.1),
                Effect(BasicSurvivorsGame.OrbitRadiusEffect, 2),
                Effect(BasicSurvivorsGame.MagnetSpeedEffect, -3)));
            Assert.AreEqual(-0.75f, modifiers.WeaponCooldownMultiplierBonus);
            Assert.AreEqual(1f, modifiers.CriticalChanceBonus);
            Assert.AreEqual(1f, modifiers.ExecuteThresholdNormalized);
            Assert.AreEqual(1, modifiers.ProjectilePierceBonus);
            Assert.AreEqual(2f, modifiers.OrbitRadiusBonus);
            Assert.AreEqual(1, modifiers.OrbitBladeBonus);
            Assert.AreEqual(0f, modifiers.PickupAttractionSpeedBonus);
        }

        private static RunUpgradeDefinition Upgrade(params RunUpgradeEffectDescriptor[] effects)
        {
            return new RunUpgradeDefinition(new RunUpgradeId("composition-test"), RunUpgradeRarity.Common, 1, 1, effects);
        }

        private static RunUpgradeEffectDescriptor Effect(RunUpgradeEffectId id, double amount)
        {
            return new RunUpgradeEffectDescriptor(id, BasicSurvivorsGame.PlayerTarget, amount);
        }

        private sealed class EffectSink : ISurvivorsUpgradeEffectSink
        {
            public readonly List<string> Events = new List<string>();
            public SurvivorsUpgradeModifiers Modifiers;
            public void IncreaseMaximumHealth(double amount) => Events.Add("health:" + amount);
            public void RestoreBarrier(float amount) => Events.Add("barrier:" + amount + ":" + Modifiers.BarrierCapacityBonus);
            public void ScheduleMagnetPulse() => Events.Add("magnet:" + Modifiers.PickupMagnetPulseIntervalReductionBonus);
        }
    }
}
