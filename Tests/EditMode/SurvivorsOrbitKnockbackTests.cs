using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsOrbitKnockbackTests
    {
        [Test]
        public void NullDestroyedDeadAndMissingDefinitionReturnBeforeBorrowedStateReads()
        {
            var port = new Port();
            var diagnostics = new SurvivorsWeaponDiagnostics();
            var knockback = new SurvivorsOrbitKnockback(port, diagnostics);
            var instance = new GameObject("orbit-test");
            var enemy = instance.AddComponent<SurvivorsEnemyActor>();
            try
            {
                Assert.That(knockback.Apply(null, Definition()), Is.False);
                Assert.That(knockback.Apply(enemy, Definition()), Is.False);
                Initialize(enemy, SurvivorsEnemyRole.Swarm);
                Assert.That(knockback.Apply(enemy, null), Is.False);
                Object.DestroyImmediate(instance);
                Assert.That(knockback.Apply(enemy, Definition()), Is.False);
                Assert.That(port.Reads, Is.Empty);
                Assert.That(diagnostics.OrbitKnockbackCount, Is.Zero);
            }
            finally { if (instance != null) Object.DestroyImmediate(instance); }
        }

        [Test]
        public void EveryMajorRewardRoleRejectsMovementBeforeReadingTuning()
        {
            var port = new Port();
            var diagnostics = new SurvivorsWeaponDiagnostics();
            var knockback = new SurvivorsOrbitKnockback(port, diagnostics);
            var instance = new GameObject("orbit-test");
            var enemy = instance.AddComponent<SurvivorsEnemyActor>();
            try
            {
                foreach (SurvivorsEnemyRole role in new[] { SurvivorsEnemyRole.Elite, SurvivorsEnemyRole.DreadElite, SurvivorsEnemyRole.Miniboss, SurvivorsEnemyRole.Boss })
                {
                    Initialize(enemy, role);
                    Assert.That(knockback.Apply(enemy, Definition()), Is.False);
                }
                Assert.That(port.Reads, Is.Empty);
                Assert.That(instance.transform.position, Is.EqualTo(Vector3.zero));
                Assert.That(diagnostics.OrbitKnockbackCount, Is.Zero);
            }
            finally { Object.DestroyImmediate(instance); }
        }

        [TestCase(-0.3f)]
        [TestCase(0f)]
        public void NonpositiveDistanceDoesNotReadPlayerPoseOrChangeExistingHistory(float distance)
        {
            var port = new Port { Distance = distance };
            var diagnostics = new SurvivorsWeaponDiagnostics();
            diagnostics.RecordOrbitKnockback(false, "Earlier", "Runner");
            var knockback = new SurvivorsOrbitKnockback(port, diagnostics);
            var enemy = Enemy();
            try
            {
                Assert.That(knockback.Apply(enemy, Definition()), Is.False);
                Assert.That(port.Reads, Is.EqualTo(new[] { "distance" }));
                Assert.That(diagnostics.OrbitKnockbackCount, Is.EqualTo(1));
                Assert.That(diagnostics.LastOrbitKnockbackFeedbackLabel, Is.EqualTo("Earlier pushed Runner"));
            }
            finally { Object.DestroyImmediate(enemy.gameObject); }
        }

        [Test]
        public void NormalOrbitMovesPlanarAwayAndRotatesBeforeRecordingTheAuthoredLabel()
        {
            var port = new Port { Position = new Vector3(1f, -20f, 1f), Distance = 2f };
            var diagnostics = new SurvivorsWeaponDiagnostics();
            var knockback = new SurvivorsOrbitKnockback(port, diagnostics);
            var enemy = Enemy();
            enemy.transform.position = new Vector3(4f, 7f, 5f);
            try
            {
                Assert.That(knockback.Apply(enemy, Definition()), Is.True);
                Assert.That(Vector3.Distance(enemy.transform.position, new Vector3(5.2f, 7f, 6.6f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(enemy.transform.forward, new Vector3(0.6f, 0f, 0.8f)), Is.LessThan(0.0001f));
                Assert.That(port.Reads, Is.EqualTo(new[] { "distance", "position" }));
                Assert.That(diagnostics.OrbitKnockbackCount, Is.EqualTo(1));
                Assert.That(diagnostics.LastOrbitKnockbackFeedbackLabel, Is.EqualTo("Authored Orbit pushed Test Enemy"));
            }
            finally { Object.DestroyImmediate(enemy.gameObject); }
        }

        [TestCase(BasicSurvivorsGame.OrbitWardWeaponContentId, 0.2f, 0.8f, 0.8f)]
        [TestCase(BasicSurvivorsGame.ThornHaloWeaponContentId, 1f, 0.5f, 1f)]
        public void ActiveCrimsonOrbitUsesMaximumDistanceForBothSupportedIds(string id, float normal, float enhanced, float expected)
        {
            var port = new Port { Active = true, Distance = normal, EnhancedDistance = enhanced };
            var diagnostics = new SurvivorsWeaponDiagnostics();
            var knockback = new SurvivorsOrbitKnockback(port, diagnostics);
            var enemy = Enemy();
            enemy.transform.position = Vector3.right;
            try
            {
                Assert.That(knockback.Apply(enemy, Definition(id)), Is.True);
                Assert.That(enemy.transform.position.x, Is.EqualTo(1f + expected).Within(0.0001f));
                Assert.That(port.Reads, Is.EqualTo(new[] { "active", "distance", "enhanced", "position" }));
                Assert.That(diagnostics.LastOrbitKnockbackFeedbackLabel, Is.EqualTo("Crimson Aegis pushed Test Enemy"));
            }
            finally { Object.DestroyImmediate(enemy.gameObject); }
        }

        [Test]
        public void MissingEvolutionDoesNotReadEnhancedDistance()
        {
            var port = new Port { Active = false, Distance = 0.2f, EnhancedDistance = 9f };
            var diagnostics = new SurvivorsWeaponDiagnostics();
            var knockback = new SurvivorsOrbitKnockback(port, diagnostics);
            var enemy = Enemy();
            enemy.transform.position = Vector3.right;
            try
            {
                Assert.That(knockback.Apply(enemy, Definition(BasicSurvivorsGame.OrbitWardWeaponContentId)), Is.True);
                Assert.That(enemy.transform.position.x, Is.EqualTo(1.2f).Within(0.0001f));
                Assert.That(port.Reads, Is.EqualTo(new[] { "active", "distance", "position" }));
                Assert.That(diagnostics.LastOrbitKnockbackFeedbackLabel, Is.EqualTo("Authored Orbit pushed Test Enemy"));
            }
            finally { Object.DestroyImmediate(enemy.gameObject); }
        }

        [Test]
        public void CoincidentPlanarPositionsUseUnflattenedPlayerForward()
        {
            var port = new Port { Position = new Vector3(3f, -8f, 4f), Forward = new Vector3(1f, 2f, 0f), Distance = 2f };
            var diagnostics = new SurvivorsWeaponDiagnostics();
            var knockback = new SurvivorsOrbitKnockback(port, diagnostics);
            var enemy = Enemy();
            Vector3 original = enemy.transform.position = new Vector3(3f, 6f, 4f);
            try
            {
                Assert.That(knockback.Apply(enemy, Definition()), Is.True);
                Vector3 expected = original + port.Forward.normalized * 2f;
                Assert.That(Vector3.Distance(enemy.transform.position, expected), Is.LessThan(0.0001f));
                Assert.That(enemy.transform.position.y, Is.GreaterThan(original.y));
                Assert.That(port.Reads, Is.EqualTo(new[] { "distance", "position", "forward" }));
            }
            finally { Object.DestroyImmediate(enemy.gameObject); }
        }

        [Test]
        public void MissingFallbackDirectionKeepsPositionAndDiagnosticsUntouched()
        {
            var port = new Port { Forward = Vector3.zero };
            var diagnostics = new SurvivorsWeaponDiagnostics();
            var enemy = Enemy();
            try
            {
                Assert.That(new SurvivorsOrbitKnockback(port, diagnostics).Apply(enemy, Definition()), Is.False);
                Assert.That(enemy.transform.position, Is.EqualTo(Vector3.zero));
                Assert.That(port.Reads, Is.EqualTo(new[] { "distance", "position", "forward" }));
                Assert.That(diagnostics.OrbitKnockbackCount, Is.Zero);
                Assert.That(diagnostics.LastOrbitKnockbackFeedbackLabel, Is.Empty);
            }
            finally { Object.DestroyImmediate(enemy.gameObject); }
        }

        private static SurvivorsEnemyActor Enemy()
        {
            var enemy = new GameObject("orbit-test").AddComponent<SurvivorsEnemyActor>();
            Initialize(enemy, SurvivorsEnemyRole.Swarm);
            return enemy;
        }
        private static void Initialize(SurvivorsEnemyActor enemy, SurvivorsEnemyRole role)
            => enemy.Initialize(null, new SurvivorsEnemyProfile(role, "test.enemy", "Test Enemy", 10f, 1f, 0.4f, 1f, 1f, 1, Color.white));
        private static SurvivorsWeaponArchetypeDefinition Definition(string id = "test.orbit")
            => new SurvivorsWeaponArchetypeDefinition(id, "Authored Orbit", SurvivorsWeaponArchetype.Orbit, 1f, 1f, 2f, Color.white);
        private sealed class Port : ISurvivorsOrbitKnockbackPort
        {
            internal readonly List<string> Reads = new List<string>();
            internal bool Active;
            internal float Distance = 0.2f;
            internal float EnhancedDistance = 0.8f;
            internal Vector3 Position;
            internal Vector3 Forward = Vector3.forward;
            public bool CrimsonAegisActive { get { Reads.Add("active"); return Active; } }
            public float OrbitKnockbackDistance { get { Reads.Add("distance"); return Distance; } }
            public float CrimsonAegisOrbitKnockbackDistance { get { Reads.Add("enhanced"); return EnhancedDistance; } }
            public Vector3 PlayerPosition { get { Reads.Add("position"); return Position; } }
            public Vector3 PlayerForward { get { Reads.Add("forward"); return Forward; } }
        }
    }
}
