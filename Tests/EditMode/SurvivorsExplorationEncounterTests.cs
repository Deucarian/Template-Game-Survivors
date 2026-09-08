using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsExplorationEncounterTests
    {
        [Test]
        public void EndlessExplorationUsesClearedVictoryAndClampsTier()
        {
            var beforeVictory = new SurvivorsExplorationBonuses(false, 12);
            Assert.AreEqual(0, beforeVictory.Tier);
            Assert.AreEqual(1f, beforeVictory.ExperienceMultiplier);
            Assert.AreEqual(0, beforeVictory.PressureBonus);
            var first = new SurvivorsExplorationBonuses(true, 0);
            Assert.AreEqual(1, first.Tier);
            Assert.AreEqual(1, first.GemBonus);
            Assert.AreEqual(0, first.ShardBonus);
            var maximum = new SurvivorsExplorationBonuses(true, 999);
            Assert.AreEqual(12, maximum.Tier);
            Assert.AreEqual(6, maximum.GemBonus);
            Assert.AreEqual(7, maximum.PressureBonus);
            Assert.AreEqual(4, maximum.ShardBonus);
            Assert.AreEqual(" + Endless T12", maximum.LabelSuffix);
        }

        [Test]
        public void RoamingCacheKeepsAuthoredLaneGeometryAndEndlessDropScaling()
        {
            var port = new ExplorationPort { Endless = new SurvivorsExplorationBonuses(true, 3) };
            var cache = new SurvivorsRoamingCacheEncounter(port);
            cache.SpawnRoamingArenaCache(Vector3.forward * 10f, 2);
            Assert.AreEqual(4, cache.RoamingCacheExperienceGemDropCount);
            Assert.AreEqual(14, port.Amounts[0]);
            Assert.That(port.Positions[0].x, Is.EqualTo(1.08f).Within(0.001f));
            Assert.That(port.Positions[0].z, Is.EqualTo(4f).Within(0.001f));
            Assert.That(port.Positions[3].x, Is.EqualTo(-1.08f).Within(0.001f));
            Assert.That(port.Feedback.LastLabel, Does.Contain("+56 XP").And.Contain("Endless T3"));
        }

        [Test]
        public void FailedCacheDropsDoNotAdvanceCadenceEvenWhenEmptySurgeActivates()
        {
            var port = new ExplorationPort { FailPickups = true };
            port.Tuning.RoamingCacheSurgeInterval = 1;
            port.Tuning.RoamingCacheSurgeDurationSeconds = 0f;
            var cache = new SurvivorsRoamingCacheEncounter(port);
            cache.SpawnRoamingArenaCache(Vector3.zero, 0);
            Assert.AreEqual(0, cache.RoamingCacheDropCount);
            Assert.AreEqual(1, cache.RoamingCacheSurgeActivationCount);
            Assert.AreEqual(0.1f, cache.SurgeRemaining);
            Assert.AreEqual(string.Empty, port.Feedback.LastLabel);
            Assert.AreEqual(1, port.PresentationPulses.Count, "The authored surge cue still plays with no damage or gems.");
            cache.TickRoamingCacheSurge(-1f);
            Assert.AreEqual(0.1f, cache.SurgeRemaining);
            cache.TickRoamingCacheSurge(1f);
            Assert.AreEqual(0f, cache.SurgeRemaining);
        }

        [Test]
        public void CacheAmbushContinuesAfterFailureAndOnlyLastMemberCompletes()
        {
            var port = new ExplorationPort { FailSpawnAttempt = 2 };
            port.Tuning.RoamingCacheAmbushStartCache = 1;
            port.Tuning.RoamingCacheAmbushInterval = 1;
            port.Tuning.RoamingCacheAmbushBaseEnemyCount = 3;
            port.Tuning.RoamingCacheAmbushMaxEnemyCount = 3;
            var cache = new SurvivorsRoamingCacheEncounter(port);
            cache.SpawnRoamingArenaCache(Vector3.forward, 0);
            CollectionAssert.AreEqual(new long[] { 846, 848, 849 }, port.Seeds);
            Assert.AreEqual(2, cache.ActiveCount);
            Assert.AreEqual(1, cache.RoamingCacheAmbushCount);
            Assert.IsFalse(cache.RemoveEnemy(1));
            Assert.IsFalse(cache.RemoveEnemy(1));
            Assert.IsTrue(cache.RemoveEnemy(2));
            Assert.IsFalse(cache.RemoveEnemy(2));
        }

        [Test]
        public void CacheClearRequiresSuccessfulExperienceBeforeSpecialsOrClearCount()
        {
            var port = new ExplorationPort { FailPickups = true };
            port.Tuning.RoamingCacheAmbushClearMagnetInterval = 1;
            port.Tuning.RoamingCacheAmbushClearBloodShardInterval = 1;
            var cache = new SurvivorsRoamingCacheEncounter(port);
            cache.SpawnRoamingCacheAmbushClearReward(Vector3.zero);
            Assert.AreEqual(0, cache.RoamingCacheAmbushClearRewardCount);
            Assert.That(port.Kinds, Has.All.EqualTo(SurvivorsPickupKind.Experience));
            port.FailPickups = false;
            port.Kinds.Clear();
            cache.SpawnRoamingCacheAmbushClearReward(Vector3.zero);
            CollectionAssert.AreEqual(new[] { SurvivorsPickupKind.Experience, SurvivorsPickupKind.Experience,
                SurvivorsPickupKind.Experience, SurvivorsPickupKind.Magnet, SurvivorsPickupKind.BloodShard }, port.Kinds);
            Assert.AreEqual(1, cache.RoamingCacheAmbushClearRewardCount);
            Assert.AreEqual(1, cache.RoamingCacheAmbushClearMagnetDropCount);
            Assert.AreEqual(cache.LastRoamingCacheAmbushClearFeedbackLabel, port.Feedback.LastLabel);
            cache.Reset();
            Assert.AreEqual(0, cache.RoamingCacheAmbushClearRewardCount);
        }

        [Test]
        public void ShrineConsumesSuccessfulTrialOnlyAndRespectsLiveCapacity()
        {
            var port = new ExplorationPort { ActiveEnemyCount = 10, MaximumAlive = 10 };
            var shrine = new SurvivorsShrineEncounter(port);
            Assert.AreEqual(0, shrine.SpawnArenaShrineTrial(Vector3.forward));
            Assert.AreEqual(0, shrine.ArenaShrineTrialCount);
            port.ActiveEnemyCount = 0;
            port.FailSpawnAttempt = 2;
            Assert.AreEqual(2, shrine.SpawnArenaShrineTrial(Vector3.forward));
            CollectionAssert.AreEqual(new long[] { 894, 896, 897 }, port.Seeds);
            CollectionAssert.AreEqual(new[] { SurvivorsEnemyRole.Swarm, SurvivorsEnemyRole.Bruiser, SurvivorsEnemyRole.Swarm }, port.Roles);
            Assert.AreEqual(1, shrine.ArenaShrineTrialCount);
            Assert.IsFalse(shrine.RemoveEnemy(1));
            Assert.IsTrue(shrine.RemoveEnemy(2));
            shrine.ClearMembers();
            Assert.AreEqual(1, shrine.ArenaShrineTrialCount, "World release does not reset run diagnostics.");
        }

        [Test]
        public void ShrineClearStillRewardsSurgeAndPulseWhenEveryPickupFails()
        {
            var port = new ExplorationPort { FailPickups = true };
            var shrine = new SurvivorsShrineEncounter(port);
            shrine.SpawnArenaShrineClearReward(Vector3.zero);
            Assert.AreEqual(1, shrine.ArenaShrineClearRewardCount);
            Assert.AreEqual(0, shrine.ArenaShrineClearExperienceGemDropCount);
            Assert.AreEqual(3f, shrine.SurgeRemaining);
            Assert.AreEqual(2, shrine.ArenaShrineSurgePulseHitCount);
            Assert.AreEqual("survivors.arena-shrine.surge", port.DamageSources[0]);
            Assert.That(port.Feedback.LastLabel, Does.Contain("+0 XP").And.Contain("Shrine Surge"));
            shrine.TickArenaShrineSurge(1f);
            shrine.SpawnArenaShrineClearReward(Vector3.zero);
            Assert.AreEqual(3f, shrine.SurgeRemaining);
            shrine.Reset();
            Assert.AreEqual(0f, shrine.SurgeRemaining);
            Assert.AreEqual(0, shrine.ArenaShrineSurgePulseHitCount);
        }

        [Test]
        public void WaystoneDiscoveryUsesHorizontalRadiusAndConsumesEachKeyOnceDespiteFailedDrops()
        {
            var port = new ExplorationPort { FailPickups = true };
            var waystones = new SurvivorsWaystoneExploration(port);
            Assert.IsFalse(waystones.TryDiscover(-1, new Vector3(2.01f, 0f, 0f)));
            Assert.IsTrue(waystones.TryDiscover(-1, new Vector3(2f, 100f, 0f)));
            Assert.IsFalse(waystones.TryDiscover(-1, Vector3.zero));
            Assert.AreEqual(1, waystones.WaystoneDiscoveryCount);
            Assert.AreEqual(0, waystones.WaystoneExperienceGemDropCount);
            Assert.AreEqual(3f, waystones.FocusRemaining);
            Assert.IsTrue(waystones.IsDiscovered(-1));
            waystones.ClearDiscoveries();
            Assert.IsTrue(waystones.TryDiscover(-1, Vector3.zero));
            Assert.AreEqual(2, waystones.WaystoneDiscoveryCount);
            waystones.Reset();
            Assert.IsFalse(waystones.IsDiscovered(-1));
            Assert.AreEqual(0, waystones.WaystoneDiscoveryCount);
        }

        [Test]
        public void WaystoneChainUsesDiscoveryCadenceAndPresentsPulseBeforeDiscoveryFeedback()
        {
            var port = new ExplorationPort();
            port.Tuning.WaystoneChainInterval = 2;
            port.Tuning.WaystoneChainDurationSeconds = 0f;
            port.Tuning.WaystoneChainBonusGemCount = 1;
            port.Tuning.WaystoneChainPulseRadius = 4f;
            port.Tuning.WaystoneChainPulseDamage = 8f;
            var waystones = new SurvivorsWaystoneExploration(port);
            waystones.TryDiscover(1, Vector3.zero);
            Assert.AreEqual(0, waystones.WaystoneChainSurgeActivationCount);
            port.Events.Clear();
            waystones.TryDiscover(2, Vector3.zero);
            Assert.AreEqual(1, waystones.WaystoneChainSurgeActivationCount);
            Assert.AreEqual(1, waystones.WaystoneChainSurgeBonusExperienceGemDropCount);
            Assert.AreEqual(0.1f, waystones.ChainRemaining);
            CollectionAssert.AreEqual(new[] { "damage", "pulse", "message", "pulse" }, port.Events);
            Assert.AreEqual(waystones.LastWaystoneDiscoveryFeedbackLabel, port.Feedback.LastLabel);
            Assert.That(port.Feedback.LastLabel, Does.Contain("Waystone Chain"));
            waystones.TickWaystoneChainSurge(1f);
            Assert.AreEqual(0f, waystones.ChainRemaining);
        }

        private sealed class ExplorationPort : ISurvivorsExplorationPort
        {
            public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning
            {
                EnemyExperienceReward = 10, BloodShardPickupAmount = 2,
                RoamingCacheExperienceGemCount = 2, RoamingCacheMagnetInterval = 0, RoamingCacheBloodShardInterval = 0,
                RoamingCacheAmbushInterval = 0, RoamingCacheSurgeInterval = 0, RoamingCacheSurgeBonusGemCount = 0,
                RoamingCacheSurgePulseRadius = 0f, RoamingCacheSurgePulseDamage = 0f,
                RoamingCacheAmbushClearMagnetInterval = 0, RoamingCacheAmbushClearBloodShardInterval = 0,
                ArenaShrineBaseEnemyCount = 3, ArenaShrineMaxEnemyCount = 3, ArenaShrineExtraAliveAllowance = 0,
                ArenaShrineClearExperienceGemCount = 2, ArenaShrineClearBloodShardAmount = 1,
                ArenaShrineSurgeDurationSeconds = 3f, ArenaShrineSurgePulseRadius = 5f, ArenaShrineSurgePulseDamage = 10f,
                WaystoneDiscoveryRadius = 2f, WaystoneFocusDurationSeconds = 3f, WaystoneExperienceGemCount = 2,
                WaystoneBloodShardInterval = 0, WaystoneAmbushInterval = 0, WaystoneChainInterval = 0
            };
            public int Escalation => 0;
            public Vector3 PlayerPosition => Vector3.zero;
            public Vector3 PlayerForward => Vector3.forward;
            public int MaximumAlive { get; set; } = 100;
            public int ActiveEnemyCount { get; set; }
            public long SpawnSequence { get; private set; }
            public SurvivorsExplorationBonuses Endless { get; set; }
            public string CurrencyRewardLabel => "Shards";
            public SurvivorsExplorationFeedback Feedback { get; }
            public bool FailPickups;
            public int FailSpawnAttempt;
            public readonly List<SurvivorsPickupKind> Kinds = new List<SurvivorsPickupKind>();
            public readonly List<Vector3> Positions = new List<Vector3>();
            public readonly List<int> Amounts = new List<int>();
            public readonly List<long> Seeds = new List<long>();
            public readonly List<SurvivorsEnemyRole> Roles = new List<SurvivorsEnemyRole>();
            public readonly List<string> DamageSources = new List<string>();
            public readonly List<int> PresentationPulses = new List<int>();
            public readonly List<string> Events = new List<string>();
            public ExplorationPort()
            {
                Feedback = new SurvivorsExplorationFeedback((label, color) => Events.Add("message"),
                    (position, particles, danger) => { PresentationPulses.Add(particles); Events.Add("pulse"); });
            }
            public long SpawnEnemy(SurvivorsEnemyRole role, long seed, float minimum, float maximum, string source)
            {
                Seeds.Add(seed);
                Roles.Add(role);
                if (Seeds.Count == FailSpawnAttempt) return 0;
                ActiveEnemyCount++;
                return ++SpawnSequence;
            }
            public bool SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount)
            {
                Kinds.Add(kind);
                Positions.Add(position);
                Amounts.Add(amount);
                return !FailPickups;
            }
            public int DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source)
            {
                DamageSources.Add(source);
                Events.Add("damage");
                return 2;
            }
        }
    }
}
