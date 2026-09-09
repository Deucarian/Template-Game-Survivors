using System;
using System.Collections.Generic;
using Deucarian.WorldSpawning;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsActorMembershipTests
    {
        [Test]
        public void EnemyReleaseRemovesListBeforeMembersAndPoolAndReturnsClearResults()
        {
            using var h = new Host(); var enemy = h.Enemy(7);
            SurvivorsEncounterClears clears = h.Owner.ReleaseKilledEnemy(enemy);
            CollectionAssert.AreEqual(new[] { "horde:7", "cache:7", "shrine:7", "forget", "service", "despawn:7:Killed" }, h.Events);
            Assert.IsTrue(clears.Horde); Assert.IsFalse(clears.Cache); Assert.IsTrue(clears.Shrine);
            Assert.IsEmpty(h.Owner.Enemies);
        }

        [Test]
        public void MemberCallbacksCanChangeTheNextLiveIdRead()
        {
            using var h = new Host(); var enemy = h.Enemy(2);
            h.AfterHorde = () => enemy.OnWorldSpawned(Context(9));
            h.Owner.ReleaseEnemy(enemy, DespawnReason.Killed);
            CollectionAssert.AreEqual(new[] { "horde:2", "cache:9", "shrine:9", "forget", "service", "despawn:9:Killed" }, h.Events);
        }

        [Test]
        public void MissingServiceAndInvalidIdsStillRemoveMembershipWithoutPoolCommand()
        {
            using var h = new Host(); var enemy = h.Enemy(0); h.Service = false;
            h.Owner.ReleaseEnemy(enemy, DespawnReason.Killed);
            Assert.AreEqual("service", h.Events[h.Events.Count - 1]);
            Assert.IsEmpty(h.Owner.Enemies);
        }

        [Test]
        public void DestroyedActorGuardsDoNotTouchBorrowedMembershipOrService()
        {
            using var h = new Host(); var enemy = h.Enemy(1); UnityEngine.Object.DestroyImmediate(enemy.gameObject);
            h.Owner.ReleaseEnemy(enemy, DespawnReason.Killed);
            h.Owner.ReleaseProjectile(null, DespawnReason.Killed);
            Assert.IsEmpty(h.Events);
        }

        [Test]
        public void ProjectileReleaseRemovesBeforePoolAndLeavesOtherCollectionsIntact()
        {
            using var h = new Host(); h.Enemy(3); var projectile = h.Projectile(8);
            h.Owner.ReleaseProjectile(projectile, DespawnReason.Killed);
            CollectionAssert.AreEqual(new[] { "service", "despawn:8:Killed" }, h.Events);
            Assert.IsEmpty(h.Owner.Projectiles); Assert.AreEqual(1, h.Owner.Enemies.Count);
        }

        [Test]
        public void AreaPulseUsesSnapshotThenLiveEligibilityAndCountsDispatchedCommands()
        {
            using var h = new SurvivorsEnemyNavigationTestHost();
            var first = h.Add(Vector3.zero); var next = h.Add(Vector3.right);
            h.Add(Vector3.zero, SurvivorsEnemyRole.Boss); h.Add(new Vector3(0, 5, 0));
            var seen = new List<SurvivorsEnemyActor>();
            var pulse = new SurvivorsAreaDamage(h.Queries, (enemy, damage, source) => {
                Assert.AreEqual(0, damage); Assert.AreEqual("pulse", source); seen.Add(enemy);
                h.Enemies.Clear(); SurvivorsEnemyNavigationTestHost.SetCurrentHealth(next, 0);
            });
            Assert.AreEqual(1, pulse.DamageNonMajorEnemies(Vector3.zero, 2, 0, "pulse"));
            CollectionAssert.AreEqual(new[] { first }, seen);
        }

        [Test]
        public void AreaPulseRetainsThreeDimensionalRadiusAndSourceOrder()
        {
            using var h = new SurvivorsEnemyNavigationTestHost();
            var first = h.Add(Vector3.up); var second = h.Add(Vector3.right); h.Add(Vector3.up * 2);
            var seen = new List<SurvivorsEnemyActor>();
            var pulse = new SurvivorsAreaDamage(h.Queries, (enemy, damage, source) => seen.Add(enemy));
            Assert.AreEqual(2, pulse.DamageNonMajorEnemies(Vector3.zero, 1, -3, "unchanged"));
            CollectionAssert.AreEqual(new[] { first, second }, seen);
        }

        private static WorldSpawnContext Context(long id) => new WorldSpawnContext(id > 0 ? new SpawnInstanceId(id) : default, default, new SpawnPose(Vector3.zero, Quaternion.identity));
        private sealed class Host : ISurvivorsActorMembershipPort, IDisposable
        {
            public readonly List<string> Events = new List<string>();
            private readonly List<GameObject> _objects = new List<GameObject>();
            public readonly SurvivorsActorMembership Owner;
            public bool Service = true;
            public Action AfterHorde;
            public Host() => Owner = new SurvivorsActorMembership(this);
            public SurvivorsEnemyActor Enemy(long id) { var obj = new GameObject("Membership enemy"); _objects.Add(obj); var enemy = obj.AddComponent<SurvivorsEnemyActor>(); enemy.OnWorldSpawned(Context(id)); Owner.Enemies.Add(enemy); return enemy; }
            public SurvivorsProjectileActor Projectile(long id) { var obj = new GameObject("Membership projectile"); _objects.Add(obj); var actor = obj.AddComponent<SurvivorsProjectileActor>(); actor.OnWorldSpawned(Context(id)); Owner.Projectiles.Add(actor); return actor; }
            public void Dispose() { foreach (var obj in _objects) if (obj != null) UnityEngine.Object.DestroyImmediate(obj); }
            public bool RemoveHordeMember(long id) { Assert.IsEmpty(Owner.Enemies); Events.Add("horde:" + id); AfterHorde?.Invoke(); return true; }
            public bool RemoveCacheMember(long id) { Events.Add("cache:" + id); return false; }
            public bool RemoveShrineMember(long id) { Events.Add("shrine:" + id); return true; }
            public void ForgetMajorThreat(SurvivorsEnemyActor enemy) => Events.Add("forget");
            public bool HasSpawnService { get { Events.Add("service"); return Service; } }
            public void Despawn(SpawnInstanceId id, DespawnReason reason) { Assert.IsEmpty(Owner.Projectiles); Events.Add($"despawn:{id.Value}:{reason}"); }
        }
    }
}
