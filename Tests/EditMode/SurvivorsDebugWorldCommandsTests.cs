using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsDebugWorldCommandsTests
    {
        [Test]
        public void BurstRetainsFailureAccountingAndReadsPlayerPositionForEveryAttempt()
        {
            using var h = new Host(); h.MoveAfterSpawn = true;
            Assert.AreEqual(3, h.Commands.SpawnBurst(SurvivorsEnemyRole.Swarm, 4, 2));
            Assert.AreEqual(4, h.Positions.Count);
            Assert.AreEqual(new Vector3(2, 0, 0), h.Positions[0]);
            Assert.That(Vector3.Distance(new Vector3(1, 0, 2), h.Positions[1]), Is.LessThan(0.0001f));
            Assert.AreEqual(1, h.EnsureCount);
        }

        [Test]
        public void BurstClampsOnlyItsOwnAttemptCountAndMinimumRadius()
        {
            using var h = new Host(); h.Commands.SpawnBurst(SurvivorsEnemyRole.Swarm, -5, -2);
            Assert.AreEqual(new Vector3(.5f, 0, 0), h.Positions[0]);
            h.Positions.Clear(); h.Commands.SpawnBurst(SurvivorsEnemyRole.Swarm, 900, 1);
            Assert.AreEqual(256, h.Positions.Count);
        }

        [Test]
        public void FillReadsLiveCountAndKeepsNestedEnsureAndBurstCap()
        {
            using var h = new Host(); h.ActiveEnemyCount = 20;
            h.Commands.FillArena(SurvivorsEnemyRole.Elite, 512, 2);
            Assert.AreEqual(256, h.Positions.Count);
            Assert.AreEqual(2, h.EnsureCount);
            h.Positions.Clear(); h.Commands.FillArena(SurvivorsEnemyRole.Elite, 10, 2);
            Assert.IsEmpty(h.Positions);
            Assert.AreEqual(3, h.EnsureCount);
        }

        [Test]
        public void EncounterClearCapturesIdsBeforeDamageMutatesMembership()
        {
            using var h = new Host(); h.Ids.AddRange(new long[] { 7, 8, 9 });
            Assert.AreEqual(2, h.Commands.ClearEncounter(SurvivorsDebugEncounter.Shrine));
            CollectionAssert.AreEqual(new long[] { 7, 8, 9 }, h.DamagedIds);
            Assert.AreEqual("test.arena-shrine-clear", h.LastSource);
            Assert.AreEqual(10000, h.LastDamage);
            Assert.IsEmpty(h.Ids);
        }

        [Test]
        public void EmptyEncounterDoesNotReadMembers()
        {
            using var h = new Host();
            Assert.AreEqual(0, h.Commands.ClearEncounter(SurvivorsDebugEncounter.Horde));
            Assert.AreEqual(0, h.MemberReads);
        }

        [Test]
        public void MajorSpawnPreservesRoleFallbackAndUnflattenedForward()
        {
            using var h = new Host(); h.PlayerForward = new Vector3(0, 1, 1);
            h.Commands.SpawnMajor(SurvivorsEnemyRole.Swarm, 100);
            Assert.AreEqual(SurvivorsEnemyRole.Elite, h.LastRole);
            Assert.That(Vector3.Distance(h.PlayerForward.normalized * 40, h.Positions[0]), Is.LessThan(.0001f));
            h.PlayerForward = Vector3.zero; h.Commands.SpawnMajor(SurvivorsEnemyRole.Boss, -3);
            Assert.AreEqual(new Vector3(0, 0, 2), h.Positions[1]);
        }

        [Test]
        public void HealthOverrideOnlyAppliesWhenPositiveAndSpawned()
        {
            using var h = new Host();
            var enemy = h.Commands.SpawnWithHealth(Vector3.zero, SurvivorsEnemyRole.Swarm, 23);
            Assert.AreEqual(23, enemy.CurrentHealth);
            h.Commands.SpawnWithHealth(Vector3.zero, SurvivorsEnemyRole.Swarm, -1);
            h.Commands.SpawnWithHealth(Vector3.zero, SurvivorsEnemyRole.Swarm, -1);
            Assert.AreEqual(23, enemy.CurrentHealth);
        }

        [Test]
        public void StressEditsTuningBeforeEnsuringRunAndSprintRechecksStartAfterPacing()
        {
            using var h = new Host(); h.Commands.ApplyStress(300);
            Assert.AreEqual(300, h.CapAtEnsure);
            Assert.AreEqual(.18f, h.Tuning.EnemySpawnIntervalSeconds);
            Assert.AreEqual(160, h.Positions.Count);
            h.Started = false; h.Commands.SpawnSprintBoss(3);
            Assert.IsFalse(h.RestartRequested);
            Assert.AreEqual(1, h.ExplicitStarts);
            Assert.AreEqual(SurvivorsEnemyRole.Boss, h.LastRole);
        }

        private sealed class Host : ISurvivorsDebugWorldPort, IDisposable
        {
            private readonly GameObject _object = new GameObject("Debug world target");
            private readonly SurvivorsEnemyActor _actor;
            public readonly SurvivorsDebugWorldCommands Commands;
            public readonly List<Vector3> Positions = new List<Vector3>();
            public readonly List<long> Ids = new List<long>(), DamagedIds = new List<long>();
            public int EnsureCount, MemberReads, ExplicitStarts, CapAtEnsure;
            public bool MoveAfterSpawn, RestartRequested;
            public float LastDamage;
            public string LastSource;
            public SurvivorsEnemyRole LastRole;
            public Host() { _actor = _object.AddComponent<SurvivorsEnemyActor>(); Commands = new SurvivorsDebugWorldCommands(this); }
            public void Dispose() => UnityEngine.Object.DestroyImmediate(_object);
            public bool Started { get; set; }
            public SurvivorsPacingProfile PacingProfile { get; private set; }
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning();
            public Vector3 PlayerPosition { get; private set; }
            public Vector3 PlayerForward { get; set; } = Vector3.forward;
            public int ActiveEnemyCount { get; set; }
            public void EnsureRunStarted() { EnsureCount++; CapAtEnsure = Tuning.EnemyMaximumAlive; Started = true; }
            public void ApplyPacing(SurvivorsPacingProfile profile, bool restart) { PacingProfile = profile; RestartRequested = restart; }
            public void StartRun() { ExplicitStarts++; Started = true; }
            public SurvivorsEnemyActor SpawnEnemy(Vector3 position, SurvivorsEnemyRole role)
            { Positions.Add(position); LastRole = role; if (MoveAfterSpawn) PlayerPosition += Vector3.right; return Positions.Count == 2 ? null : _actor; }
            public int ActiveMembers(SurvivorsDebugEncounter encounter) => Ids.Count;
            public IEnumerable<long> Members(SurvivorsDebugEncounter encounter) { MemberReads++; return Ids; }
            public bool DamageMember(long id, float damage, string source)
            { DamagedIds.Add(id); Ids.Clear(); LastSource = source; LastDamage = damage; return id != 8; }
        }
    }
}
