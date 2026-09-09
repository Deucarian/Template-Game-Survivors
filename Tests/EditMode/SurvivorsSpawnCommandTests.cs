using System.Collections.Generic;
using System.Linq;
using Deucarian.WorldSpawning;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsSpawnCommandTests
    {
        [Test]
        public void FailedEnemyPickupAndProjectileAttemptsConsumeOneSharedSequenceWithoutSuccessEffects()
        {
            using (var host = new SurvivorsSpawnCommandTestHost { FailRequests = true })
            {
                Assert.IsNull(host.Enemies.SpawnEnemy(Vector3.zero, true, SurvivorsEnemyRole.Boss));
                Assert.IsNull(host.Pickups.SpawnPickup(SurvivorsPickupKind.Experience, Vector3.one, 5));
                Assert.IsFalse(host.Projectiles.LaunchProjectile(Weapon(), Vector3.forward));
                CollectionAssert.AreEqual(new long[] { 1, 2, 3 }, host.Requests.Select(x => x.Sequence).ToArray());
                CollectionAssert.AreEqual(new[] { "pose", "spawn", "pose", "spawn", "pose", "spawn" }, host.Events);
                Assert.AreEqual(0, host.Enemies.SpawnedCount);
                Assert.AreEqual(0, host.Enemies.BossSpawnCount);
                Assert.AreEqual(0, host.Projectiles.ProjectileLaunchCount);
                host.Enemies.ResetDiagnostics();
                host.Projectiles.ResetDiagnostics();
                Assert.AreEqual(3, host.Sequence.Current);
                host.Sequence.Reset();
                Assert.AreEqual(0, host.Sequence.Current);
            }
        }

        [Test]
        public void SuccessfulResultWithMissingInstanceHasTheSameFailureBoundary()
        {
            using (var host = new SurvivorsSpawnCommandTestHost { ReturnMissingInstance = true })
            {
                Assert.IsNull(host.Enemies.SpawnEnemy(Vector3.zero, true, SurvivorsEnemyRole.Swarm));
                Assert.IsNull(host.Pickups.SpawnPickup(SurvivorsPickupKind.Health, Vector3.zero, 2));
                Assert.IsFalse(host.Projectiles.LaunchProjectile(Weapon(), Vector3.forward));
                Assert.AreEqual(3, host.Sequence.Current);
                Assert.AreEqual(0, host.Enemies.SpawnedCount);
                Assert.AreEqual(0, host.Projectiles.ProjectileLaunchCount);
                Assert.AreEqual(6, host.Events.Count);
            }
        }

        [TestCase(SurvivorsEnemyRole.Swarm, "group.survivors.opening-swarm", 10, null)]
        [TestCase(SurvivorsEnemyRole.Elite, "group.survivors.elites", 30, "Elite")]
        [TestCase(SurvivorsEnemyRole.DreadElite, "group.survivors.dread-elites", 30, "Elite")]
        [TestCase(SurvivorsEnemyRole.Miniboss, "group.survivors.miniboss", 42, "Miniboss")]
        [TestCase(SurvivorsEnemyRole.Boss, "group.survivors.boss", 58, "Boss")]
        public void ExplicitEnemyRegistersBeforeCountersAndMetricsBeforeRoleFeedback(SurvivorsEnemyRole role, string group, int burst, string metric)
        {
            using (var host = new SurvivorsSpawnCommandTestHost())
            {
                var position = new Vector3(4f, 0f, 5f);
                Assert.IsNotNull(host.Enemies.SpawnEnemy(position, true, role));
                Assert.AreEqual(position, host.Poses[1]);
                var request = host.Requests.Single();
                Assert.AreEqual(BasicSurvivorsGame.ExplicitSpawnChannelId, request.ChannelId);
                Assert.AreEqual("SurvivorsTemplate", request.Context.SourceSystem);
                Assert.AreEqual("wave.survivors.opening-ring", request.Context.WaveId);
                Assert.AreEqual(group, request.Context.GroupId);
                Assert.AreEqual(0, host.PlacementCalls);
                var expected = new List<string> { "pose", "spawn", "initialize.enemy", "register.enemy:0" };
                if (metric != null) expected.Add("metric:" + metric + ":0:0");
                expected.Add("feedback:" + burst + ":1");
                CollectionAssert.AreEqual(expected, host.Events);
                Assert.AreEqual(role == SurvivorsEnemyRole.Boss ? 1 : 0, host.Enemies.BossSpawnCount);
                Assert.AreEqual(role == SurvivorsEnemyRole.Miniboss ? 1 : 0, host.Enemies.MinibossSpawnCount);
            }
        }

        [Test]
        public void OffscreenGameplaySpawnUsesCallerSeedAndResolvedBoundsOnceThenTracksSafety()
        {
            using (var host = new SurvivorsSpawnCommandTestHost())
            {
                host.Enemies.SpawnGameplayEnemyOffscreen(SurvivorsEnemyRole.Runner, 73, 10f, 20f, "caller");
                Assert.AreEqual(1, host.PlacementCalls);
                Assert.AreEqual(73, host.LastPlacementSeed);
                Assert.AreEqual(host.PlayerPosition, host.LastPlacementCenter);
                Assert.AreEqual(12f, host.LastMinimum);
                Assert.AreEqual(23f, host.LastMaximum);
                Assert.AreEqual(4f, host.LastPadding);
                Assert.AreEqual(5f, host.LastBandDepth);
                Assert.AreEqual(new Vector3(20f, 0f, 30f), host.Poses[1]);
                CollectionAssert.AreEqual(new[] { "placement", "pose", "spawn", "initialize.enemy", "register.enemy:0", "safety:1", "feedback:10:1" }, host.Events);
            }
        }

        [Test]
        public void ImplicitEnemyUsesRequestSequenceForPlacementAndExplicitBackendChannel()
        {
            using (var host = new SurvivorsSpawnCommandTestHost())
            {
                host.Pickups.SpawnPickup(SurvivorsPickupKind.Health, Vector3.zero, 1);
                host.Events.Clear();
                host.Enemies.SpawnEnemy(Vector3.one, false, SurvivorsEnemyRole.Swarm);
                Assert.AreEqual(2, host.LastPlacementSeed);
                Assert.AreEqual(18f, host.LastMaximum);
                Assert.AreEqual(BasicSurvivorsGame.ExplicitSpawnChannelId, host.Requests.Last().ChannelId);
                Assert.That(host.Events, Does.Contain("safety:1"));
            }
        }

        [Test]
        public void ProfilesUseCurrentRunFlowMajorDefinitionsAndRoleGroupMappingRemainsExact()
        {
            using (var host = new SurvivorsSpawnCommandTestHost())
            {
                Assert.AreEqual(SurvivorsEnemyRole.Boss, host.Enemies.ResolveEnemyProfile(SurvivorsEnemyRole.Boss).Role);
                host.RunFlow = new SurvivorsRunFlowRuntime(BasicSurvivorsGame.CreateRunFlowDefinition(host.Tuning));
                Assert.AreEqual(host.RunFlow.Definition.Boss, host.Enemies.ResolveEnemyProfile(SurvivorsEnemyRole.Boss));
                Assert.AreEqual(host.RunFlow.Definition.Miniboss, host.Enemies.ResolveEnemyProfile(SurvivorsEnemyRole.Miniboss));
                CollectionAssert.AreEqual(new[] { "group.survivors.runners", "group.survivors.bruisers", "group.survivors.spitters", "group.survivors.splitters", "group.survivors.summoners" },
                    new[] { SurvivorsEnemyRole.Runner, SurvivorsEnemyRole.Bruiser, SurvivorsEnemyRole.Spitter, SurvivorsEnemyRole.Splitter, SurvivorsEnemyRole.Summoner }.Select(SurvivorsEnemySpawner.ResolveEnemyGroupId).ToArray());
                Assert.AreEqual(BasicSurvivorsGame.BossEnemySpawnableId, SurvivorsEnemySpawner.ResolveEnemySpawnableId(SurvivorsEnemyRole.Boss));
                Assert.AreEqual(BasicSurvivorsGame.MinibossEnemySpawnableId, SurvivorsEnemySpawner.ResolveEnemySpawnableId(SurvivorsEnemyRole.Miniboss));
                Assert.AreEqual(BasicSurvivorsGame.SwarmEnemySpawnableId, SurvivorsEnemySpawner.ResolveEnemySpawnableId(SurvivorsEnemyRole.DreadElite));
            }
        }

        [TestCase(SurvivorsPickupKind.Experience)]
        [TestCase(SurvivorsPickupKind.Magnet)]
        [TestCase(SurvivorsPickupKind.Health)]
        [TestCase(SurvivorsPickupKind.BloodShard)]
        public void PickupRequestRetainsKindClampsAmountAndReadsLiveAttractionSettings(SurvivorsPickupKind kind)
        {
            using (var host = new SurvivorsSpawnCommandTestHost())
            {
                Assert.IsNotNull(host.Pickups.SpawnPickup(kind, Vector3.one, -3));
                Assert.AreEqual(kind, host.LastPickupKind);
                Assert.AreEqual(1, host.LastPickupAmount);
                Assert.AreEqual(5.5f, host.LastPickupRange);
                Assert.AreEqual(7.5f, host.LastPickupSpeed);
                Assert.AreEqual(0.7f, host.LastPickupRadius);
                Assert.AreEqual(kind.ToString(), host.Requests[0].Context.GroupId);
                CollectionAssert.AreEqual(new[] { "pose", "spawn", "initialize.pickup", "register.pickup" }, host.Events);
                host.CurrentPickupAttractRange = 13f;
                host.CurrentPickupAttractionSpeed = 17f;
                host.Pickups.SpawnPickup(kind, Vector3.zero, 8);
                Assert.AreEqual(8, host.LastPickupAmount);
                Assert.AreEqual(13f, host.LastPickupRange);
                Assert.AreEqual(17f, host.LastPickupSpeed);
            }
        }

        [Test]
        public void ProjectileLaunchUsesCurrentBonusesFallbackDirectionAndSuccessFeedbackOrder()
        {
            using (var host = new SurvivorsSpawnCommandTestHost())
            {
                var definition = Weapon();
                Assert.IsTrue(host.Projectiles.LaunchProjectile(definition, Vector3.zero));
                Assert.AreEqual(Vector3.forward, host.LastLaunch.Direction);
                Assert.AreEqual(host.PlayerPosition + Vector3.up * 0.4f + Vector3.forward * 0.55f, host.Poses[1]);
                Assert.AreEqual(host.PlayerPosition + Vector3.up * 0.4f, host.LastLaunchFeedbackPosition);
                Assert.AreEqual(definition.ProjectileChainCount + 2, host.LastLaunch.Chains);
                Assert.AreEqual(definition.ProjectilePierceCount + 3, host.LastLaunch.Pierces);
                Assert.AreEqual(definition.ProjectileForkCount + 4, host.LastLaunch.Forks);
                Assert.AreEqual(definition.ProjectileReturnCount + 5, host.LastLaunch.Returns);
                Assert.AreEqual(19f, host.LastLaunch.Damage);
                Assert.AreEqual(definition.ProjectileSpeed, host.LastLaunch.Speed);
                Assert.AreEqual(definition.ProjectileRadius, host.LastLaunch.Radius);
                Assert.AreEqual(definition.ProjectileLifetimeSeconds, host.LastLaunch.Lifetime);
                Assert.IsNull(host.LastLaunch.IgnoredEnemyIds);
                CollectionAssert.AreEqual(new[] { "pose", "spawn", "damage", "initialize.projectile", "register.projectile:0", "launch:1" }, host.Events);
            }
        }

        [Test]
        public void ChainedProjectileKeepsExplicitOptionsAndBorrowedIgnoredSet()
        {
            using (var host = new SurvivorsSpawnCommandTestHost())
            {
                var ignored = new HashSet<int> { 17, 25 };
                var origin = new Vector3(3f, 2f, 4f);
                Assert.IsTrue(host.Projectiles.LaunchProjectileFrom(Weapon(), origin, Vector3.right * 3f, 4, 3, 2, 1, ignored));
                Assert.AreEqual(Vector3.right, host.LastLaunch.Direction);
                Assert.AreEqual(origin + Vector3.right * 0.55f, host.Poses[1]);
                Assert.AreSame(ignored, host.LastLaunch.IgnoredEnemyIds);
                CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, new[] { host.LastLaunch.Chains, host.LastLaunch.Pierces, host.LastLaunch.Forks, host.LastLaunch.Returns });
            }
        }

        [Test]
        public void MissingProjectileDefinitionOrBackendDoesNotConsumeSequenceOrPose()
        {
            using (var host = new SurvivorsSpawnCommandTestHost())
            {
                Assert.IsFalse(host.Projectiles.LaunchProjectile(null, Vector3.forward));
                host.HasSpawnService = false;
                Assert.IsFalse(host.Projectiles.LaunchProjectile(Weapon(), Vector3.forward));
                Assert.IsFalse(host.Projectiles.LaunchProjectileFrom(null, Vector3.zero, Vector3.one, 0, 0, 0, 0, null));
                Assert.AreEqual(0, host.Sequence.Current);
                Assert.IsEmpty(host.Poses);
                Assert.IsEmpty(host.Events);
            }
        }

        private static SurvivorsWeaponArchetypeDefinition Weapon() =>
            BasicSurvivorsGame.CreateWeaponArchetypeDefinitions(new SurvivorsTemplateTuning()).First(x => x.Id == BasicSurvivorsGame.ArcaneWandWeaponContentId);
    }
}
