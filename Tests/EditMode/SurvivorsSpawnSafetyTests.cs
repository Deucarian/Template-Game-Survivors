using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsSpawnSafetyTests
    {
        [Test]
        public void MissingCameraUsesDeterministicRadialFallbackAndPreservesCenterHeight()
        {
            var port = new Projection { HasCamera = false };
            var safety = new SurvivorsSpawnSafety(port);
            Vector3 point = safety.ResolveSafeOffscreenPosition(new Vector3(4f, 7f, -3f), 3f, 6f, 0, -9f, 2f);
            Assert.That(point, Is.EqualTo(new Vector3(7f, 7f, -3f)));
            Assert.That(port.LastPadding, Is.Zero);
            Assert.That(safety.IsWorldPositionInsideCameraViewportForTest(point, -5f), Is.False);
            Assert.That(safety.GameplayEnemySpawnSafetyCheckCountForTest, Is.Zero);
        }

        [Test]
        public void CameraRectangleRoutesPolicyOutsideInclusiveBoundsWithoutChangingHeight()
        {
            var port = new Projection { Visible = new Rect(-10f, -10f, 20f, 20f) };
            var safety = new SurvivorsSpawnSafety(port);
            var point = safety.ResolveSafeOffscreenPosition(Vector3.up * 6f, 1f, 2f, 0, 2f, 3f);
            Assert.That(point.x, Is.EqualTo(10.15f).Within(0.00001f));
            Assert.That(point.y, Is.EqualTo(6f));
            Assert.That(point.z, Is.Zero);
            Assert.That(port.LastPadding, Is.EqualTo(2f));
            Assert.That(safety.IsWorldPositionInsideCameraViewportForTest(new Vector3(10f, 999f, 10f), 0f), Is.True);
            Assert.That(safety.IsWorldPositionInsideCameraViewportForTest(point, 0f), Is.False);
        }

        [Test]
        public void RadialResolverReadsLivePlayerAndTuningThroughTheSameSafetyPath()
        {
            var port = new Projection { HasCamera = false, PlayerPosition = new Vector3(3f, 4f, 5f) };
            port.Tuning.EnemySpawnRadius = 6f;
            port.Tuning.OffscreenSpawnPadding = 2f;
            var safety = new SurvivorsSpawnSafety(port);
            Assert.That(safety.ResolveRuntimeEnemySpawnPositionForResolver(0), Is.EqualTo(new Vector3(9f, 4f, 5f)));
            Assert.That(port.LastPadding, Is.EqualTo(2f));
            port.PlayerPosition = Vector3.zero;
            port.Tuning = new SurvivorsTemplateTuning { EnemySpawnRadius = -5f, OffscreenSpawnPadding = -1f, SpawnBandDepth = -2f };
            Assert.That(safety.ResolveRuntimeEnemySpawnPositionForResolver(0), Is.EqualTo(Vector3.right));
            Assert.That(port.LastPadding, Is.EqualTo(0.1f));
        }

        [Test]
        public void RoleDistancesAndRecyclePaddingKeepTheirOriginalClampPrecedence()
        {
            var port = new Projection();
            port.Tuning.MajorThreatCatchUpRadius = 14f;
            port.Tuning.SpawnBandDepth = 3f;
            port.Tuning.OffscreenSpawnPadding = 4f;
            port.Tuning.MajorThreatOffscreenSpawnPadding = 12f;
            port.Tuning.RecycledEnemyOffscreenSpawnPadding = -1f;
            var safety = new SurvivorsSpawnSafety(port);
            Assert.That(safety.ResolveGameplaySpawnMinimumDistance(SurvivorsEnemyRole.Swarm, -5f), Is.EqualTo(1f));
            Assert.That(safety.ResolveGameplaySpawnMinimumDistance(SurvivorsEnemyRole.Elite, 2f), Is.EqualTo(14f));
            Assert.That(safety.ResolveGameplaySpawnMaximumDistance(SurvivorsEnemyRole.Boss, 2f, 3f), Is.EqualTo(17f));
            Assert.That(safety.ResolveGameplaySpawnMaximumDistance(SurvivorsEnemyRole.Swarm, -5f, -10f), Is.EqualTo(-2f),
                "The coordinator preserves the supplied minimum here; radial geometry performs its own clamp later.");
            Assert.That(safety.ResolveOffscreenSpawnPadding(SurvivorsEnemyRole.Boss, "normal-recycle"), Is.EqualTo(0.1f));
            Assert.That(safety.ResolveOffscreenSpawnPadding(SurvivorsEnemyRole.Boss, "Normal-Recycle"), Is.EqualTo(12f));
            Assert.That(safety.ResolveOffscreenSpawnPadding(SurvivorsEnemyRole.Swarm, null), Is.EqualTo(4f));
        }

        [Test]
        public void DiagnosticsRetainLastFailureAcrossSafeChecksAndResetIndependentlyOfProjection()
        {
            var port = new Projection { Visible = new Rect(-2f, -2f, 4f, 4f) };
            var safety = new SurvivorsSpawnSafety(port);
            safety.RecordGameplaySpawnSafety(SurvivorsEnemyRole.Boss, Vector3.zero, " ");
            string failure = safety.LastGameplaySpawnSafetyFailureForTest;
            Assert.That(failure, Is.EqualTo($"runtime spawned Boss inside camera viewport at {Vector3.zero}"));
            Assert.That(safety.GameplaySpawnInsideCameraViewViolationCountForTest, Is.EqualTo(1));
            safety.RecordGameplaySpawnSafety(SurvivorsEnemyRole.Swarm, Vector3.right * 3f, "spawn");
            Assert.That(safety.GameplayEnemySpawnSafetyCheckCountForTest, Is.EqualTo(2));
            Assert.That(safety.GameplaySpawnInsideCameraViewViolationCountForTest, Is.EqualTo(1));
            Assert.That(safety.LastGameplaySpawnSafetyFailureForTest, Is.EqualTo(failure));
            Assert.That(safety.LastGameplaySpawnPositionForTest, Is.EqualTo(Vector3.right * 3f));
            Assert.That(safety.LastGameplaySpawnWasInsideCameraViewportForTest, Is.False);
            safety.ResetDiagnostics();
            Assert.That(safety.GameplayEnemySpawnSafetyCheckCountForTest, Is.Zero);
            Assert.That(safety.GameplaySpawnInsideCameraViewViolationCountForTest, Is.Zero);
            Assert.That(safety.LastGameplaySpawnSafetyFailureForTest, Is.Empty);
            Assert.That(safety.LastGameplaySpawnPositionForTest, Is.EqualTo(Vector3.zero));
            Assert.That(safety.LastGameplaySpawnPaddingForTest, Is.Zero);
            Assert.That(port.Calls, Is.EqualTo(2));
        }

        [Test]
        public void DefaultOverloadReadsPaddingAndBandFromCurrentTuning()
        {
            var port = new Projection { Visible = new Rect(-10f, -10f, 20f, 20f) };
            port.Tuning.OffscreenSpawnPadding = 7f;
            port.Tuning.SpawnBandDepth = 4f;
            var safety = new SurvivorsSpawnSafety(port);
            var center = new Vector3(1f, 2f, 3f);
            Vector3 explicitResult = safety.ResolveSafeOffscreenPosition(center, 2f, 5f, 91, 7f, 4f);
            Vector3 defaultResult = safety.ResolveSafeOffscreenPosition(center, 2f, 5f, 91);
            Assert.That(defaultResult, Is.EqualTo(explicitResult));
            Assert.That(port.LastPadding, Is.EqualTo(7f));
        }

        private sealed class Projection : ISurvivorsSpawnSafetyPort
        {
            public SurvivorsTemplateTuning Tuning { get; set; } = new SurvivorsTemplateTuning();
            public Vector3 PlayerPosition { get; set; }
            internal bool HasCamera = true;
            internal Rect Visible;
            internal float LastPadding;
            internal int Calls;
            public bool TryResolveCameraGroundRect(float padding, out Rect rect)
            {
                Calls++; LastPadding = padding; rect = Visible; return HasCamera;
            }
        }
    }
}
