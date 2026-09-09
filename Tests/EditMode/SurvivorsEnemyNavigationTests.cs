using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsEnemyNavigationTests
    {
        [Test]
        public void NearestUsesFullDistanceAndLastEqualCandidateFromLiveMembership()
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            var first = host.Add(Vector3.left);
            var last = host.Add(Vector3.right);
            host.Add(Vector3.up * 2f);
            var dead = host.Add(Vector3.zero);
            SurvivorsEnemyNavigationTestHost.SetCurrentHealth(dead, 0f);
            var destroyed = host.Add(Vector3.zero);
            Object.DestroyImmediate(destroyed.gameObject);
            host.Enemies.Add(null);
            Assert.That(host.Queries.FindNearestEnemy(Vector3.zero, 1f), Is.SameAs(last));
            host.Enemies.Remove(last);
            Assert.That(host.Queries.FindNearestEnemy(Vector3.zero, -1f), Is.SameAs(first),
                "The original query squares its supplied range, including negative values.");
            host.Enemies.Remove(first);
            Assert.That(host.Queries.FindNearestEnemy(Vector3.zero, 1f), Is.Null);
        }

        [Test]
        public void RadiusClearsResultsPreservesSourceOrderAndUsesFullDistance()
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            var first = host.Add(Vector3.right);
            var second = host.Add(Vector3.left);
            var elevated = host.Add(Vector3.up * 1.01f);
            var dead = host.Add(Vector3.zero);
            SurvivorsEnemyNavigationTestHost.SetCurrentHealth(dead, 0f);
            var results = new List<SurvivorsEnemyActor> { elevated, dead };
            host.Queries.CollectEnemiesWithinRadius(Vector3.zero, -1f, results);
            Assert.That(results, Is.EqualTo(new[] { first, second }));
            host.Queries.CollectEnemiesWithinRadius(Vector3.zero, 0f, results);
            Assert.That(results, Is.Empty);
            Assert.DoesNotThrow(() => host.Queries.CollectEnemiesWithinRadius(Vector3.zero, 1f, null));
        }

        [Test]
        public void CrowdUsesHorizontalPhysicalRadiiAndFirstEligibleNeighborLimit()
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            var actor = host.Add(Vector3.zero);
            var dead = host.Add(Vector3.zero);
            SurvivorsEnemyNavigationTestHost.SetCurrentHealth(dead, 0f);
            host.Add(Vector3.right * 20f);
            host.Add(new Vector3(1f, 100f, 0f));
            host.Add(Vector3.left);
            host.Tuning.EnemySeparationRadius = 0.1f;
            host.Tuning.EnemySeparationStrength = 2f;
            host.Tuning.EnemySeparationMaxNeighbors = 1;
            AssertVector(host.Queries.ResolveEnemyCrowdSeparation(actor), Vector3.left);
            host.Tuning.EnemySeparationMaxNeighbors = 2;
            AssertVector(host.Queries.ResolveEnemyCrowdSeparation(actor), Vector3.zero);
        }

        [Test]
        public void CoincidentCrowdUsesOrderedInstanceIdsAndClampsMagnitude()
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            var actor = host.Add(Vector3.zero);
            var other = host.Add(Vector3.up * 100f);
            host.Tuning.EnemySeparationRadius = 2f;
            host.Tuning.EnemySeparationStrength = 0.7f;
            Vector3 expected = SurvivorsEnemySpatialQueries.ResolveDeterministicSeparationDirection(actor.GetInstanceID(), other.GetInstanceID());
            AssertVector(host.Queries.ResolveEnemyCrowdSeparation(actor), expected * 0.7f);
            float angle = 140f * Mathf.Deg2Rad; // 100 XOR (200 << 1) = 500; 500 modulo 360 = 140.
            AssertVector(SurvivorsEnemySpatialQueries.ResolveDeterministicSeparationDirection(100, 200),
                new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
            Assert.That(SurvivorsEnemySpatialQueries.ResolveDeterministicSeparationDirection(100, 200),
                Is.Not.EqualTo(SurvivorsEnemySpatialQueries.ResolveDeterministicSeparationDirection(200, 100)));
        }

        [Test]
        public void DisabledCrowdAndSingleActorReturnZeroAndReadReboundTuning()
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            var actor = host.Add(Vector3.zero);
            AssertVector(host.Queries.ResolveEnemyCrowdSeparation(actor), Vector3.zero);
            host.Add(Vector3.right);
            host.Tuning = new SurvivorsTemplateTuning { EnemySeparationStrength = 0f, EnemySeparationRadius = 2f };
            AssertVector(host.Queries.ResolveEnemyCrowdSeparation(actor), Vector3.zero);
            host.Tuning.EnemySeparationStrength = 2f;
            host.Tuning.EnemySeparationRadius = 0f;
            AssertVector(host.Queries.ResolveEnemyCrowdSeparation(actor), Vector3.zero);
            AssertVector(host.Queries.ResolveEnemyCrowdSeparation(null), Vector3.zero);
        }

        [Test]
        public void NormalLeashResetsAtHorizontalSoftBoundaryAndClampsNegativeTime()
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            var actor = host.Add(new Vector3(5f, 100f, 0f));
            actor.AddLeashTime(4f);
            host.Navigation.TryUpdateEnemyLeash(actor, 3f);
            Assert.That(actor.LeashTimerSeconds, Is.Zero);
            actor.transform.position = Vector3.right * 6f;
            host.Navigation.TryUpdateEnemyLeash(actor, -3f);
            Assert.That(actor.LeashTimerSeconds, Is.Zero);
            host.Navigation.TryUpdateEnemyLeash(actor, 1f);
            Assert.That(actor.LeashTimerSeconds, Is.EqualTo(1f));
            Assert.That(host.Events, Is.Empty);
        }

        [Test]
        public void NormalRecycleRequiresHardRadiusAndDelayThenRecordsSafetyBeforeResetAndCount()
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            var actor = host.Add(Vector3.right * 6f);
            host.Navigation.TryUpdateEnemyLeash(actor, 2f);
            Assert.That(host.Events, Is.Empty);
            actor.transform.position = Vector3.right * 10f;
            host.OnSafety = () =>
            {
                Assert.That(actor.transform.position, Is.EqualTo(host.Destination));
                Assert.That(actor.LeashTimerSeconds, Is.EqualTo(2f));
                Assert.That(host.Navigation.NormalEnemyRecycleCount, Is.Zero);
            };
            host.Navigation.TryUpdateEnemyLeash(actor, 0f);
            Assert.That(host.Events, Is.EqualTo(new[] { "padding:normal-recycle", "position", "safety:normal-recycle" }));
            Assert.That(host.LastSeed, Is.EqualTo(17L));
            Assert.That(host.LastMinimum, Is.EqualTo(12f));
            Assert.That(host.LastMaximum, Is.EqualTo(16f));
            Assert.That(host.LastPadding, Is.EqualTo(2.5f));
            Assert.That(host.LastDepth, Is.EqualTo(3f));
            Assert.That(host.LastSafetyPosition, Is.EqualTo(host.Destination));
            Assert.That(actor.LeashTimerSeconds, Is.Zero);
            Assert.That(host.Navigation.NormalEnemyRecycleCount, Is.EqualTo(1));
            host.OnSafety = null;
            actor.transform.position = Vector3.right * 10f;
            host.Navigation.TryUpdateEnemyLeash(actor, 2f);
            Assert.That(host.LastSeed, Is.EqualTo(18L));
        }

        [Test]
        public void NormalRecyclePreservesLowHealthEnemyAndUsesLiveThresholdConfiguration()
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            var actor = host.Add(Vector3.right * 10f);
            SurvivorsEnemyNavigationTestHost.SetCurrentHealth(actor, 15f);
            host.Navigation.TryUpdateEnemyLeash(actor, 2f);
            Assert.That(host.Events, Is.Empty);
            Assert.That(actor.LeashTimerSeconds, Is.EqualTo(2f));
            SurvivorsEnemyNavigationTestHost.SetCurrentHealth(actor, 16f);
            host.Tuning.EnemyRecycleMinimumRespawnDistance = 1f;
            host.Tuning.EnemyRecycleMaximumRespawnDistance = 0f;
            host.Navigation.TryUpdateEnemyLeash(actor, 0f);
            Assert.That(host.LastMinimum, Is.EqualTo(3.8f).Within(0.0001f));
            Assert.That(host.LastMaximum, Is.EqualTo(2f), "Preserve independent authored maximum calculation; safe-pose policy owns normalization.");
            Assert.That(host.Navigation.NormalEnemyRecycleCount, Is.EqualTo(1));
        }

        [TestCase(false, true, true, SurvivorsEnemyRole.Swarm)]
        [TestCase(true, false, true, SurvivorsEnemyRole.Swarm)]
        [TestCase(true, true, false, SurvivorsEnemyRole.Boss)]
        public void AuthoredLifecycleExclusionsLeaveExistingActorTimerUntouched(bool recycle, bool leash, bool reposition, SurvivorsEnemyRole role)
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            var actor = host.Add(Vector3.right * 100f, role, recycle, leash, reposition);
            actor.AddLeashTime(1f);
            host.Navigation.TryUpdateEnemyLeash(actor, 10f);
            Assert.That(actor.LeashTimerSeconds, Is.EqualTo(1f));
            Assert.That(host.Events, Is.Empty);
        }

        [Test]
        public void DeadAndMissingActorsNeverAccumulateLeashTime()
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            var actor = host.Add(Vector3.right * 100f);
            actor.AddLeashTime(1f);
            SurvivorsEnemyNavigationTestHost.SetCurrentHealth(actor, 0f);
            host.Navigation.TryUpdateEnemyLeash(actor, 10f);
            host.Navigation.TryUpdateEnemyLeash(null, 10f);
            Assert.That(actor.LeashTimerSeconds, Is.EqualTo(1f));
            Assert.That(host.Events, Is.Empty);
        }

        [Test]
        public void MajorReentryIgnoresWoundedProtectionAndPublishesMarkerAfterTimerAndCount()
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            host.PlayerPosition = Vector3.forward * 3f;
            var actor = host.Add(host.PlayerPosition + new Vector3(20f, 100f, 0f), SurvivorsEnemyRole.Boss, canRecycle: false);
            actor.AddLeashTime(9f);
            host.Navigation.TryUpdateEnemyLeash(actor, 1f);
            Assert.That(actor.LeashTimerSeconds, Is.Zero);
            actor.transform.position = host.PlayerPosition + Vector3.right * 21f;
            SurvivorsEnemyNavigationTestHost.SetCurrentHealth(actor, 1f);
            host.Navigation.TryUpdateEnemyLeash(actor, 2f);
            Assert.That(host.Events, Is.Empty);
            host.OnSafety = () => Assert.That(host.Navigation.MajorThreatRepositionCount, Is.Zero);
            host.OnReentry = () =>
            {
                Assert.That(actor.LeashTimerSeconds, Is.Zero);
                Assert.That(host.Navigation.MajorThreatRepositionCount, Is.EqualTo(1));
            };
            host.Navigation.TryUpdateEnemyLeash(actor, 1f);
            Assert.That(host.Events, Is.EqualTo(new[] { "padding:major-threat-reentry", "position", "safety:major-threat-reentry", "reentry" }));
            Assert.That(host.LastCenter, Is.EqualTo(host.PlayerPosition));
            Assert.That(host.LastMinimum, Is.EqualTo(15f));
            Assert.That(host.LastMaximum, Is.EqualTo(19.5f));
            Assert.That(host.LastSeed, Is.EqualTo(43L));
            Assert.That(host.LastReentryName, Is.EqualTo("Test Boss"));
            Assert.That(host.LastReentryRole, Is.EqualTo(SurvivorsEnemyRole.Boss));
            Assert.That(host.LastReentryDelta, Is.EqualTo(host.Destination - host.PlayerPosition));
            actor.AddLeashTime(4f);
            host.Navigation.ResetDiagnostics();
            Assert.That(host.Navigation.MajorThreatRepositionCount, Is.Zero);
            Assert.That(host.Navigation.NormalEnemyRecycleCount, Is.Zero);
            Assert.That(actor.LeashTimerSeconds, Is.EqualTo(4f), "Diagnostics reset must not own actor timers.");
        }

        [TestCase(SurvivorsEnemyRole.Elite)]
        [TestCase(SurvivorsEnemyRole.DreadElite)]
        [TestCase(SurvivorsEnemyRole.Miniboss)]
        [TestCase(SurvivorsEnemyRole.Boss)]
        public void CatchUpUsesAllMajorRolesAndStrictRadiusWithLiveSpeedFloor(SurvivorsEnemyRole role)
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            var actor = host.Add(Vector3.zero, role, canReposition: false);
            Assert.That(host.Navigation.ResolveEnemyCatchUpMoveSpeedMultiplier(actor, 15f), Is.EqualTo(1f));
            Assert.That(host.Navigation.ResolveEnemyCatchUpMoveSpeedMultiplier(actor, 16f), Is.EqualTo(2.5f));
            host.Tuning.MajorThreatCatchUpSpeedMultiplier = 0.5f;
            Assert.That(host.Navigation.ResolveEnemyCatchUpMoveSpeedMultiplier(actor, 16f), Is.EqualTo(1f));
            host.Tuning.MajorThreatCatchUpSpeedMultiplier = 3f;
            host.Tuning.MajorThreatCatchUpRadius = 0f;
            Assert.That(host.Navigation.ResolveEnemyCatchUpMoveSpeedMultiplier(actor, 16f), Is.EqualTo(1f));
        }

        [Test]
        public void NormalAndDeadMajorActorsDoNotCatchUp()
        {
            using var host = new SurvivorsEnemyNavigationTestHost();
            var normal = host.Add(Vector3.zero);
            var major = host.Add(Vector3.zero, SurvivorsEnemyRole.Boss);
            SurvivorsEnemyNavigationTestHost.SetCurrentHealth(major, 0f);
            Assert.That(host.Navigation.ResolveEnemyCatchUpMoveSpeedMultiplier(normal, 100f), Is.EqualTo(1f));
            Assert.That(host.Navigation.ResolveEnemyCatchUpMoveSpeedMultiplier(major, 100f), Is.EqualTo(1f));
            Assert.That(host.Navigation.ResolveEnemyCatchUpMoveSpeedMultiplier(null, 100f), Is.EqualTo(1f));
        }

        private static void AssertVector(Vector3 actual, Vector3 expected)
            => Assert.That(Vector3.Distance(actual, expected), Is.LessThan(0.00001f));
    }
}
