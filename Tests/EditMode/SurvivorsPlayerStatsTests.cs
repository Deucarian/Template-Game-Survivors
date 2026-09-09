using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsPlayerStatsTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void BaseThenUpgradeThenSurgesPreserveFloatAdditionAndReadOrder(bool movement)
        {
            var port = new Port();
            var stats = new SurvivorsPlayerStats(port);
            port.TuningValue.PlayerMoveSpeed = port.TuningValue.PickupAttractRange = 16777216f;
            port.Add(movement ? BasicSurvivorsGame.MoveSpeedEffect : BasicSurvivorsGame.MagnetRangeEffect, -16777216d);
            port.Movement = port.Pickup = new SurvivorsPlayerSurgeValues(streak: 1f);
            Assert.That(movement ? stats.PlayerMoveSpeed : stats.CurrentPickupAttractRange, Is.EqualTo(1f));
            Assert.That(port.Events, Is.EqualTo(new[] { "tuning", "modifiers", movement ? "movement" : "pickup" }));
        }

        [Test]
        public void EverySurgeTermContributesAndQueriesReadChangedValues()
        {
            var port = new Port();
            var stats = new SurvivorsPlayerStats(port);
            port.TuningValue.PlayerMoveSpeed = port.TuningValue.PickupAttractRange = 10f;
            port.Add(BasicSurvivorsGame.MoveSpeedEffect, 5d);
            port.Add(BasicSurvivorsGame.MagnetRangeEffect, 5d);
            port.Movement = port.Pickup = new SurvivorsPlayerSurgeValues(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);
            Assert.That(stats.PlayerMoveSpeed, Is.EqualTo(93f));
            Assert.That(stats.CurrentPickupAttractRange, Is.EqualTo(93f));
            port.Movement = new SurvivorsPlayerSurgeValues(endless: 2f);
            port.Pickup = new SurvivorsPlayerSurgeValues(endless: 3f);
            port.Add(BasicSurvivorsGame.MoveSpeedEffect, 1d);
            Assert.That(stats.PlayerMoveSpeed, Is.EqualTo(18f));
            Assert.That(stats.CurrentPickupAttractRange, Is.EqualTo(18f));
        }

        [Test]
        public void MovementRemainsUnclampedWhilePickupRangeAndPullKeepDistinctFloors()
        {
            var port = new Port();
            var stats = new SurvivorsPlayerStats(port);
            port.TuningValue.PlayerMoveSpeed = port.TuningValue.PickupAttractRange = 0f;
            port.Add(BasicSurvivorsGame.MoveSpeedEffect, -3d);
            port.Add(BasicSurvivorsGame.MagnetRangeEffect, -3d);
            port.Movement = port.Pickup = new SurvivorsPlayerSurgeValues(streak: -2f);
            Assert.That(stats.PlayerMoveSpeed, Is.EqualTo(-5f));
            Assert.That(stats.CurrentPickupAttractRange, Is.Zero);
            port.TuningValue.PickupAttractionSpeed = -5f;
            port.Add(BasicSurvivorsGame.MagnetSpeedEffect, 2d);
            Assert.That(stats.CurrentPickupAttractionSpeed, Is.EqualTo(0.1f));
            port.TuningValue.PickupAttractionSpeed = 7f;
            Assert.That(stats.CurrentPickupAttractionSpeed, Is.EqualTo(9f));
        }

        [Test]
        public void CriticalValuesReadAuthoritativeModifiersAndRetainBaseAndUpperClamps()
        {
            var port = new Port();
            var stats = new SurvivorsPlayerStats(port);
            Assert.That(stats.CriticalChanceNormalized, Is.Zero);
            Assert.That(stats.CriticalDamageMultiplier, Is.EqualTo(1.5f));
            port.Add(BasicSurvivorsGame.CriticalChanceEffect, 0.4d);
            port.Add(BasicSurvivorsGame.CriticalDamageEffect, 0.5d);
            Assert.That(stats.CriticalChanceNormalized, Is.EqualTo(0.4f));
            Assert.That(stats.CriticalDamageMultiplier, Is.EqualTo(2f));
            port.Add(BasicSurvivorsGame.CriticalChanceEffect, 2d);
            port.Add(BasicSurvivorsGame.CriticalDamageEffect, 1000d);
            Assert.That(stats.CriticalChanceNormalized, Is.EqualTo(1f));
            Assert.That(stats.CriticalDamageMultiplier, Is.EqualTo(100f));
            Assert.That(port.EffectCommands, Is.Zero);
        }

        [Test]
        public void DisabledNovaReadsOnlyDamageAndEnabledNovaUsesRadiusThenHalfArea()
        {
            var port = new Port();
            var stats = new SurvivorsPlayerStats(port);
            port.Add(BasicSurvivorsGame.DeathNovaRadiusEffect, 2d);
            port.Add(BasicSurvivorsGame.AreaRadiusEffect, 4d);
            Assert.That(stats.DeathNovaRadius, Is.Zero);
            Assert.That(port.Events, Is.EqualTo(new[] { "modifiers" }));
            port.Add(BasicSurvivorsGame.DeathNovaDamageEffect, 4d);
            port.Events.Clear();
            Assert.That(stats.DeathNovaRadius, Is.EqualTo(5.65f).Within(0.000001f));
            Assert.That(port.Events, Is.EqualTo(new[] { "modifiers", "modifiers", "modifiers" }));
            Assert.That(stats.DeathNovaDamage, Is.EqualTo(4f));
        }

        [Test]
        public void BarrierReadsBaseBeforeLiveModifierWithoutReplayingRestoreEffects()
        {
            var port = new Port();
            var stats = new SurvivorsPlayerStats(port);
            port.Add(BasicSurvivorsGame.BarrierCapacityEffect, 6d);
            int commands = port.EffectCommands;
            port.TuningValue.StartingBarrierCapacity = -10f;
            Assert.That(stats.BarrierCapacity, Is.Zero);
            Assert.That(port.Events, Is.EqualTo(new[] { "tuning", "modifiers" }));
            port.TuningValue.StartingBarrierCapacity = 10f;
            Assert.That(stats.BarrierCapacity, Is.EqualTo(16f));
            Assert.That(port.EffectCommands, Is.EqualTo(commands));
        }

        [TestCase(SurvivorsRunState.Booting)]
        [TestCase(SurvivorsRunState.Victory)]
        [TestCase(SurvivorsRunState.GameOver)]
        public void InactiveWarningsStopBeforeHealthReads(SurvivorsRunState state)
        {
            var port = new Port { StateValue = state };
            var stats = new SurvivorsPlayerStats(port);
            Assert.That(stats.IsLowHealthWarningActive, Is.False);
            Assert.That(port.Events, Is.EqualTo(new[] { "state", "state" }));
        }

        [TestCase(SurvivorsRunState.Playing)]
        [TestCase(SurvivorsRunState.LevelUp)]
        public void ActiveWarningsRetainInclusiveThresholdAndRepeatedMaximumRead(SurvivorsRunState state)
        {
            var port = new Port { StateValue = state, Maximum = 10f, Current = 3f };
            var stats = new SurvivorsPlayerStats(port);
            Assert.That(stats.IsLowHealthWarningActive, Is.True);
            var expected = new List<string> { "state" };
            if (state == SurvivorsRunState.LevelUp) expected.Add("state");
            expected.AddRange(new[] { "maximum", "current", "maximum" });
            Assert.That(port.Events, Is.EqualTo(expected));
            port.Current = 3.01f;
            Assert.That(stats.IsLowHealthWarningActive, Is.False);
        }

        [Test]
        public void NonpositiveMaximumStopsBeforeCurrentHealthRead()
        {
            var port = new Port { StateValue = SurvivorsRunState.Playing, Maximum = 0f };
            Assert.That(new SurvivorsPlayerStats(port).IsLowHealthWarningActive, Is.False);
            Assert.That(port.Events, Is.EqualTo(new[] { "state", "maximum" }));
        }

        [Test]
        public void UnboundRecoveryStopsBeforeHealAmountAndHealthReads()
        {
            var port = new Port { Bound = false };
            Assert.That(new SurvivorsHealthPickupDrops(port).TryDropHealthPickup(Vector3.one), Is.False);
            Assert.That(port.Events, Is.EqualTo(new[] { "bound" }));
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void DisabledHealAmountStopsBeforeCurrentHealthRead(int amount)
        {
            var port = new Port { Heal = amount };
            Assert.That(new SurvivorsHealthPickupDrops(port).TryDropHealthPickup(Vector3.one), Is.False);
            Assert.That(port.Events, Is.EqualTo(new[] { "bound", "heal" }));
        }

        [TestCase(10f, 10f, false)]
        [TestCase(10f, 9.995f, false)]
        [TestCase(0.02f, 0.01f, false)]
        [TestCase(10f, 9.98f, true)]
        public void RecoveryRetainsNearFullHealthThreshold(float maximum, float current, bool expected)
        {
            // Exact cancellation tests equality without depending on intermediate float precision.
            var port = new Port { Current = current, Maximum = maximum };
            Assert.That(new SurvivorsHealthPickupDrops(port).TryDropHealthPickup(Vector3.up), Is.EqualTo(expected));
            Assert.That(port.SpawnCalls, Is.EqualTo(expected ? 1 : 0));
        }

        [Test]
        public void EligibleRecoveryReadsHealAmountAgainAfterHealthBeforeSpawning()
        {
            var port = new Port { Heal = 3, Current = 5f, Maximum = 10f };
            port.OnRead = name => { if (name == "maximum") port.Heal = 7; };
            Vector3 position = new Vector3(1f, 2f, 3f);
            Assert.That(new SurvivorsHealthPickupDrops(port).TryDropHealthPickup(position), Is.True);
            Assert.That(port.Events, Is.EqualTo(new[] { "bound", "heal", "current", "maximum", "heal", "spawn" }));
            Assert.That(port.SpawnedAmount, Is.EqualTo(7));
            Assert.That(port.SpawnedPosition, Is.EqualTo(position));
        }

        [Test]
        public void FailedRecoverySpawnReturnsFailureWithoutApplyingHealthOrOtherCommands()
        {
            var port = new Port { SpawnResult = false, Current = 2f, Maximum = 10f };
            Assert.That(new SurvivorsHealthPickupDrops(port).TryDropHealthPickup(Vector3.one), Is.False);
            Assert.That(port.SpawnCalls, Is.EqualTo(1));
            Assert.That(port.Current, Is.EqualTo(2f));
            Assert.That(port.EffectCommands, Is.Zero);
        }

        private sealed class Port : ISurvivorsPlayerStatReadPort, ISurvivorsHealthPickupDropPort, ISurvivorsUpgradeEffectSink
        {
            internal readonly List<string> Events = new List<string>();
            internal readonly SurvivorsTemplateTuning TuningValue = new SurvivorsTemplateTuning();
            internal readonly SurvivorsUpgradeModifiers ModifierValues;
            internal SurvivorsPlayerSurgeValues Movement, Pickup;
            internal SurvivorsRunState StateValue;
            internal float Maximum = 10f, Current = 5f;
            internal int Heal = 3, SpawnCalls, SpawnedAmount, EffectCommands;
            internal Vector3 SpawnedPosition;
            internal bool Bound = true, SpawnResult = true;
            internal Action<string> OnRead;
            internal Port() { ModifierValues = new SurvivorsUpgradeModifiers(this); }
            private T Read<T>(string name, T value) { Events.Add(name); OnRead?.Invoke(name); return value; }
            public SurvivorsTemplateTuning Tuning => Read("tuning", TuningValue);
            public SurvivorsUpgradeModifiers Modifiers => Read("modifiers", ModifierValues);
            public SurvivorsPlayerSurgeValues MovementSurges => Read("movement", Movement);
            public SurvivorsPlayerSurgeValues PickupSurges => Read("pickup", Pickup);
            public SurvivorsRunState State => Read("state", StateValue);
            public float MaxHealth => Read("maximum", Maximum);
            public float CurrentHealth => Read("current", Current);
            public bool IsHealthBound => Read("bound", Bound);
            public int HealAmount => Read("heal", Heal);
            public bool SpawnHealthPickup(Vector3 position, int amount)
            { Events.Add("spawn"); SpawnCalls++; SpawnedPosition = position; SpawnedAmount = amount; return SpawnResult; }
            public void IncreaseMaximumHealth(double amount) => EffectCommands++;
            public void RestoreBarrier(float amount) => EffectCommands++;
            public void ScheduleMagnetPulse() => EffectCommands++;
            internal void Add(RunUpgradeEffectId effect, double amount)
                => ModifierValues.Apply(new RunUpgradeDefinition(new RunUpgradeId("fixture"), RunUpgradeRarity.Common, 1, 1,
                    new[] { new RunUpgradeEffectDescriptor(effect, BasicSurvivorsGame.PlayerTarget, amount) }));
        }
    }
}
