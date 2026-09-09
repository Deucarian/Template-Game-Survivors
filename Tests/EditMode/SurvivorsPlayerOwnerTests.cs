using System;
using System.Collections.Generic;
using Deucarian.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsPlayerOwnerTests
    {
        [Test]
        public void BarrierAbsorptionStartsContactSafetyBeforeHealthDamage()
        {
            var port = new DamagePort();
            var player = new SurvivorsPlayerVitals(port);
            player.Initialize(100f);
            player.SetBarrier(30f);
            player.ApplyDamageToPlayer(20f, "enemy");
            Assert.AreEqual(100f, player.CurrentHealth);
            Assert.AreEqual(10f, player.BarrierValue);
            Assert.AreEqual(0.5f, player.SafetyRemaining);
            player.ApplyDamageToPlayer(20f, "enemy");
            Assert.AreEqual(10f, player.BarrierValue);
            CollectionAssert.AreEqual(new[] { "absorbed", "invulnerable" }, port.Events);
            player.TickSafety(0.5f);
            player.ApplyDamageToPlayer(20f, "enemy");
            Assert.AreEqual(90f, player.CurrentHealth);
            Assert.AreEqual(0f, player.BarrierValue);
            Assert.AreEqual(10f, player.DamageTaken);
            player.ApplyDamageToPlayer(-1f, null);
            Assert.AreEqual("absorbed", port.Events[port.Events.Count - 1], "Zero incoming damage still has its original blocked cue.");
        }

        [Test]
        public void ClutchTriggersOnceOnThresholdCrossingAndExtendsSafety()
        {
            var port = new DamagePort();
            var player = new SurvivorsPlayerVitals(port);
            player.Initialize(100f);
            player.ApplyDamageToPlayer(70f, "enemy");
            Assert.AreEqual(30f, player.CurrentHealth);
            Assert.AreEqual(1, player.LowHealthClutchPulseCount);
            Assert.AreEqual(2, player.LowHealthClutchPulseHitCount);
            Assert.AreEqual(3f, player.SafetyRemaining);
            CollectionAssert.AreEqual(new[] { "damage", "pulse", "clutch" }, port.Events);
            player.Heal(40f);
            player.TickSafety(4f);
            player.ApplyDamageToPlayer(50f, "enemy");
            Assert.AreEqual(20f, player.CurrentHealth);
            Assert.AreEqual(1, player.LowHealthClutchPulseCount);
            Assert.AreEqual(120f, player.DamageTaken);
            player.Initialize(100f);
            Assert.AreEqual(0f, player.SafetyRemaining);
            Assert.AreEqual(0f, player.DamageTaken);
            Assert.AreEqual(0, player.LowHealthClutchPulseCount);
            Assert.AreEqual(string.Empty, player.LastLowHealthClutchPulseFeedbackLabel);
        }

        [Test]
        public void LethalDamageRecordsResolutionBeforeDefeatAndEndsFurtherDamage()
        {
            var port = new DamagePort { State = SurvivorsRunState.LevelUp };
            var player = new SurvivorsPlayerVitals(port);
            player.Initialize(100f);
            player.ApplyDamageToPlayer(200f, string.Empty);
            Assert.AreEqual(0f, player.CurrentHealth);
            CollectionAssert.AreEqual(new[] { "damage", "defeat" }, port.Events);
            Assert.AreEqual(0, player.LowHealthClutchPulseCount);
            player.ApplyDamageToPlayer(20f, "enemy");
            Assert.AreEqual(2, port.Events.Count);
        }

        [Test]
        public void HealingAndBarrierRegenerationKeepActualRestoredAmountsAndCapacity()
        {
            var port = new DamagePort();
            var player = new SurvivorsPlayerVitals(port);
            player.Initialize(100f);
            player.RestoreHealthFromPickup(10);
            player.RestoreHealthFromPickup(0);
            player.ApplyDamageToPlayer(20f, "enemy");
            player.RestoreHealthFromPickup(15);
            player.RestoreHealthFromPickup(15);
            Assert.AreEqual(3, player.HealthPickupCollectedCount);
            Assert.AreEqual(20f, player.HealthRestoredByPickups);
            player.IncreaseMaximumHealth(0.125d);
            Assert.AreEqual(100.125f, player.MaxHealth);
            Assert.AreEqual(player.MaxHealth, player.CurrentHealth);
            player.TickBarrier(2f);
            Assert.AreEqual(10f, player.BarrierValue);
            player.RestoreBarrier(-10f);
            Assert.AreEqual(10f, player.BarrierValue);
        }

        [Test]
        public void MovementPreservesAnalogMagnitudeAndCapsDiagonalInput()
        {
            var port = new MotionPort();
            var motion = new SurvivorsPlayerMotion(port);
            motion.MovePlayer(new Vector2(0.5f, 0f), 2f);
            Assert.AreEqual(new Vector3(4f, 1f, 0f), port.Position);
            motion.MovePlayer(new Vector2(3f, 4f), 1f);
            Assert.That(Vector3.Distance(new Vector3(6.4f, 1f, 3.2f), port.Position), Is.LessThan(0.001f));
            Assert.AreEqual(2, port.Travel.Count);
            port.HasPlayer = false;
            motion.MovePlayer(Vector2.one, 10f);
            Assert.AreEqual(2, port.Travel.Count);
        }

        [Test]
        public void DashPreservesTravelSafetyPressureOrderAndCooldownDiagnostics()
        {
            var port = new MotionPort { Forward = Vector3.up };
            var motion = new SurvivorsPlayerMotion(port);
            port.Events.Clear();
            Assert.IsTrue(motion.TryDash(Vector2.zero));
            Assert.AreEqual(new Vector3(0f, 1f, 3f), port.Position);
            CollectionAssert.AreEqual(new[] { "position", "facing", "travel", "safety", "pressure", "message" }, port.Events);
            Assert.AreEqual(0.05f, motion.CooldownRemaining);
            Assert.AreEqual(1, motion.DashUseCount);
            Assert.AreEqual(3, motion.DashEnemyShoveCount);
            Assert.AreEqual(2, motion.DashDamageHitCount);
            Assert.IsFalse(motion.TryDash(Vector2.right));
            motion.TickCooldown(0.05f);
            port.State = SurvivorsRunState.LevelUp;
            Assert.IsFalse(motion.TryDash(Vector2.right));
            port.State = SurvivorsRunState.Playing;
            Assert.IsTrue(motion.TryDash(Vector2.right));
            Assert.AreEqual(4, motion.DashDamageHitCount);
            motion.Reset();
            Assert.AreEqual(0, motion.DashUseCount);
            Assert.AreEqual(0f, motion.CooldownRemaining);
            Assert.AreEqual(string.Empty, motion.LastDashFeedbackLabel);
        }

        [Test]
        public void DashSegmentProjectionClampsEndsAndHandlesDegeneratePath()
        {
            Vector3 start = new Vector3(0f, 1f, 0f);
            Vector3 end = new Vector3(0f, 1f, 4f);
            Assert.AreEqual(start, SurvivorsDashPressure.ClosestPointOnSegment(start, end, new Vector3(2f, 5f, -1f)));
            Assert.AreEqual(end, SurvivorsDashPressure.ClosestPointOnSegment(start, end, new Vector3(2f, 5f, 9f)));
            Assert.AreEqual(new Vector3(0f, 1f, 2f), SurvivorsDashPressure.ClosestPointOnSegment(start, end, new Vector3(2f, 5f, 2f)));
            Assert.AreEqual(start, SurvivorsDashPressure.ClosestPointOnSegment(start, start, Vector3.one));
        }

        private sealed class DamagePort : ISurvivorsPlayerDamagePort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning
            {
                PlayerContactInvulnerabilitySeconds = 0.5f, LowHealthClutchSafetySeconds = 3f,
                LowHealthClutchPulseRadius = 5f, LowHealthClutchPulseDamage = 10f, BaseBarrierRegenPerSecond = 2f
            };
            public SurvivorsRunState State { get; set; } = SurvivorsRunState.Playing;
            public Vector3 PlayerPosition => Vector3.zero;
            public CombatCatalog CombatCatalog { get; } = BasicSurvivorsGame.CreateCombatCatalog();
            public float BarrierCapacity => 10f;
            public float BarrierRegenPerSecondBonus => 3f;
            public readonly List<string> Events = new List<string>();
            public int DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source) { Events.Add("pulse"); return 2; }
            public void ShowBlockedDamage(bool invulnerable) => Events.Add(invulnerable ? "invulnerable" : "absorbed");
            public void RecordDamage(DamageResult damage, Vector3 position) => Events.Add("damage");
            public void Defeat() { Events.Add("defeat"); State = SurvivorsRunState.GameOver; }
            public void ShowClutch(string label, int hitCount) => Events.Add("clutch");
            public void ShowHurt() => Events.Add("hurt");
        }

        private sealed class MotionPort : ISurvivorsPlayerMotionPort
        {
            private Vector3 _position = Vector3.up;
            private Vector3 _forward = Vector3.forward;
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning { DashDistance = 3f, DashCooldownSeconds = 0f, DashInvulnerabilitySeconds = 2f };
            public SurvivorsRunState State { get; set; } = SurvivorsRunState.Playing;
            public bool HasPlayer { get; set; } = true;
            public Vector3 Position { get => _position; set { _position = value; Events.Add("position"); } }
            public Vector3 Forward { get => _forward; set { _forward = value; Events.Add("facing"); } }
            public float MoveSpeed => 4f;
            public readonly List<Vector3> Travel = new List<Vector3>();
            public readonly List<string> Events = new List<string>();
            public void RecordTravel(Vector3 delta) { Travel.Add(delta); Events.Add("travel"); }
            public void ExtendSafety(float seconds) { Assert.AreEqual(2f, seconds); Events.Add("safety"); }
            public int ApplyDashPressure(Vector3 start, Vector3 end, Vector3 direction, Action onDamageHit)
            {
                Events.Add("pressure");
                onDamageHit();
                onDamageHit();
                return 3;
            }
            public void ShowDash(Vector3 position, string label, int shoved) => Events.Add("message");
        }
    }
}
