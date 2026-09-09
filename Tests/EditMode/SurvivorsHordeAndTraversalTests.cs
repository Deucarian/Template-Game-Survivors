using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsHordeAndTraversalTests
    {
        [Test]
        public void HordeWarnsOnceAndConsumesBlockedSpawnWindowWithoutCatchup()
        {
            var port = new HordePort();
            var encounter = new SurvivorsHordeRushEncounter(port);
            encounter.Reset();
            port.RunTime = 8f;
            encounter.TickHordeRushEvents();
            encounter.TickHordeRushEvents();
            Assert.AreEqual(1, encounter.HordeRushWarningCount);
            Assert.AreEqual(2f, encounter.WarningRemaining);
            port.ActiveEnemyCount = port.MaximumAlive;
            port.RunTime = 10f;
            encounter.TickHordeRushEvents();
            Assert.AreEqual(30f, encounter.NextTime);
            Assert.AreEqual(0, encounter.HordeRushSpawnCount);
            Assert.IsFalse(encounter.WarningActive);
            port.ActiveEnemyCount = 0;
            port.RunTime = 30f;
            encounter.TickHordeRushEvents();
            Assert.AreEqual(5, encounter.ActiveCount, "The blocked rush still advances authored count scaling.");
            Assert.AreEqual(1, encounter.HordeRushSpawnCount);
        }

        [Test]
        public void HordeContinuesAfterIndividualSpawnFailureAndTracksOnlySuccessfulMembers()
        {
            var port = new HordePort { FailAttempt = 2 };
            var encounter = new SurvivorsHordeRushEncounter(port);
            encounter.Reset();
            Assert.AreEqual(2, encounter.Trigger());
            CollectionAssert.AreEqual(new long[] { 947, 949, 950 }, port.Seeds);
            Assert.AreEqual(2, encounter.ActiveCount);
            Assert.IsFalse(encounter.RemoveEnemy(1));
            Assert.IsFalse(encounter.RemoveEnemy(1));
            Assert.IsTrue(encounter.RemoveEnemy(2));
            Assert.IsFalse(encounter.RemoveEnemy(2));
            encounter.Reset();
            Assert.AreEqual(0, encounter.HordeRushEnemySpawnCount);
            Assert.AreEqual(0, encounter.ActiveCount);
        }

        [Test]
        public void ClearRewardRequiresSuccessfulDropBeforePulseAndSurgeAndKeepsSpecialCadence()
        {
            var port = new HordePort { FailPickups = true };
            var encounter = new SurvivorsHordeRushEncounter(port);
            encounter.Reset();
            encounter.SpawnHordeRushClearReward(Vector3.zero);
            Assert.AreEqual(0, encounter.HordeRushClearRewardCount);
            Assert.AreEqual(0, port.Pulses);
            port.FailPickups = false;
            port.Drops.Clear();
            encounter.SpawnHordeRushClearReward(Vector3.zero);
            CollectionAssert.AreEqual(new[] { SurvivorsPickupKind.Experience, SurvivorsPickupKind.Experience, SurvivorsPickupKind.Magnet }, port.Drops);
            Assert.AreEqual(2, encounter.HordeRushClearExperienceGemDropCount);
            Assert.AreEqual(1, encounter.HordeRushClearSpecialDropCount);
            Assert.AreEqual(4f, encounter.ClearSurgeRemaining);
            encounter.TickHordeRushClearSurge(1f);
            encounter.SpawnHordeRushClearReward(Vector3.zero);
            Assert.AreEqual(4f, encounter.ClearSurgeRemaining);
            Assert.AreEqual(3, encounter.HordeRushClearSpecialDropCount);
            Assert.AreEqual(6, encounter.HordeRushClearPulseHitCount);
            Assert.That(encounter.LastHordeRushClearFeedbackLabel, Does.Contain("+10 XP"));
            encounter.TickHordeRushClearSurge(5f);
            Assert.AreEqual(0f, encounter.ClearSurgeRemaining);
        }

        [Test]
        public void HordeRoleThresholdsAndAliveAllowanceRemainAuthoredPolicy()
        {
            var port = new HordePort { MaximumAlive = 2, ActiveEnemyCount = 1, RunTime = 150f };
            port.Tuning.HordeRushExtraAliveAllowance = 2;
            var encounter = new SurvivorsHordeRushEncounter(port);
            encounter.Reset();
            Assert.AreEqual(3, encounter.Trigger());
            Assert.AreEqual(SurvivorsEnemyRole.Splitter, port.Roles[0]);
            Assert.AreEqual(4, port.ActiveEnemyCount);
            encounter.ClearMembers();
            Assert.AreEqual(0, encounter.ActiveCount);
            Assert.AreEqual(1, encounter.HordeRushSpawnCount, "Clearing world membership preserves run diagnostics.");
        }

        [Test]
        public void TraversalPreservesShrineBeforeCachesAndCapsEachMovementBatch()
        {
            var port = new TravelPort();
            var traversal = new SurvivorsTraversalDirector(port);
            traversal.RecordTravel(Vector3.right * 100f, true);
            CollectionAssert.AreEqual(new[] { "shrine", "cache0", "cache1", "cache2" }, port.Events);
            Assert.AreEqual(Vector3.right * 100f, port.ShrineDirection);
            Assert.AreEqual(Vector3.right, port.CacheDirection);
            port.Events.Clear();
            traversal.RecordTravel(Vector3.right, true);
            CollectionAssert.AreEqual(new[] { "shrine", "cache0", "cache1", "cache2" }, port.Events);
        }

        [Test]
        public void ActiveShrineSuppressesItsTravelAccrualWhileCachesContinue()
        {
            var port = new TravelPort { ActiveShrineEnemyCount = 1 };
            var traversal = new SurvivorsTraversalDirector(port);
            traversal.RecordTravel(Vector3.right * 100f, true);
            Assert.IsFalse(port.Events.Contains("shrine"));
            port.Events.Clear();
            port.ActiveShrineEnemyCount = 0;
            traversal.RecordTravel(Vector3.right, true);
            Assert.IsFalse(port.Events.Contains("shrine"), "Blocked shrine travel must not be saved for a later trial.");
        }

        [Test]
        public void PausedOrZeroTravelDoesNotConsumeBudgetsAndRestartClearsRemainder()
        {
            var port = new TravelPort();
            var traversal = new SurvivorsTraversalDirector(port);
            traversal.RecordTravel(Vector3.right * 100f, false);
            traversal.RecordTravel(Vector3.zero, true);
            Assert.IsEmpty(port.Events);
            traversal.RecordTravel(Vector3.right * 9f, true);
            Assert.IsEmpty(port.Events);
            traversal.Reset();
            traversal.RecordTravel(Vector3.right, true);
            Assert.IsEmpty(port.Events);
        }

        [Test]
        public void HordeBonusProjectionUsesItsExistingClearTimerAndLiveClamps()
        {
            var port = new HordePort(); var horde = new SurvivorsHordeRushEncounter(port);
            Assert.AreEqual(0, horde.HordeRushClearSurgePickupRangeBonus);
            horde.SpawnHordeRushClearReward(Vector3.zero);
            port.Tuning.HordeRushClearSurgePickupRangeBonus = 9;
            port.Tuning.HordeRushClearSurgeCooldownMultiplierBonus = 2;
            Assert.AreEqual(9, horde.HordeRushClearSurgePickupRangeBonus);
            Assert.AreEqual(0, horde.HordeRushClearSurgeCooldownMultiplierBonus);
            horde.TickHordeRushClearSurge(10);
            Assert.IsFalse(horde.IsHordeRushClearSurgeActive);
            Assert.AreEqual(0, horde.HordeRushClearSurgePickupRangeBonus);
        }

        private sealed class HordePort : ISurvivorsHordeRushPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning
            {
                HordeRushFirstTimeSeconds = 10f, HordeRushIntervalSeconds = 20f, HordeRushWarningLeadSeconds = 2f,
                HordeRushBaseEnemyCount = 3, HordeRushEnemyCountIncreasePerRush = 2, HordeRushMaxEnemyCount = 8,
                HordeRushExtraAliveAllowance = 0, HordeRushSpawnRadius = 3f, SpawnBandDepth = 2f,
                EnemyExperienceReward = 5, HordeRushClearExperienceGemCount = 2, HordeRushClearExperienceMultiplier = 1f,
                HordeRushClearMagnetEveryRush = 1, HordeRushClearBloodShardEveryRush = 2, BloodShardPickupAmount = 4,
                HordeRushClearPulseRadius = 3f, HordeRushClearPulseDamage = 2f, HordeRushClearSurgeDurationSeconds = 4f
            };
            public float RunTime { get; set; }
            public int Escalation => 0;
            public int MaximumAlive { get; set; } = 100;
            public int ActiveEnemyCount { get; set; }
            public long SpawnSequence { get; private set; }
            public int FailAttempt;
            public bool FailPickups;
            public int Pulses;
            public readonly List<long> Seeds = new List<long>();
            public readonly List<SurvivorsEnemyRole> Roles = new List<SurvivorsEnemyRole>();
            public readonly List<SurvivorsPickupKind> Drops = new List<SurvivorsPickupKind>();
            public long SpawnEnemy(SurvivorsEnemyRole role, long seed, float minimum, float maximum)
            {
                Seeds.Add(seed);
                Roles.Add(role);
                if (Seeds.Count == FailAttempt) return 0;
                ActiveEnemyCount++;
                return ++SpawnSequence;
            }
            public bool SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount)
            {
                Drops.Add(kind);
                return !FailPickups;
            }
            public int DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source) { Pulses++; return 3; }
            public void ShowWarning(string label, float radius, float remaining) { }
            public void ShowBurst(string label) { }
            public void ShowClear(Vector3 position, string label, int hitCount) { }
        }

        private sealed class TravelPort : ISurvivorsTraversalPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning { RoamingCacheTravelInterval = 10f, ArenaShrineTravelInterval = 20f };
            public int ActiveShrineEnemyCount { get; set; }
            public readonly List<string> Events = new List<string>();
            public Vector3 ShrineDirection;
            public Vector3 CacheDirection;
            public void SpawnShrine(Vector3 direction) { ShrineDirection = direction; Events.Add("shrine"); }
            public void SpawnCache(Vector3 direction, int sequenceOffset) { CacheDirection = direction; Events.Add("cache" + sequenceOffset); }
        }
    }
}
