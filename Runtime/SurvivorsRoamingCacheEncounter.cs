using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal sealed class SurvivorsRoamingCacheEncounter
    {
        public bool IsRoamingCacheSurgeActive => SurgeRemaining > 0f;
        public float RoamingCacheSurgeRemainingSeconds => Mathf.Max(0f, SurgeRemaining);
        public float RoamingCacheSurgeDamageBonus => IsRoamingCacheSurgeActive ? Mathf.Max(0f, _port.Tuning.RoamingCacheSurgeDamageBonus) : 0f;
        public float RoamingCacheSurgeMoveSpeedBonus => IsRoamingCacheSurgeActive ? Mathf.Max(0f, _port.Tuning.RoamingCacheSurgeMoveSpeedBonus) : 0f;
        public float RoamingCacheSurgeCooldownMultiplierBonus => IsRoamingCacheSurgeActive ? Mathf.Min(0f, _port.Tuning.RoamingCacheSurgeCooldownMultiplierBonus) : 0f;
        public float RoamingCacheSurgePickupRangeBonus => IsRoamingCacheSurgeActive ? Mathf.Max(0f, _port.Tuning.RoamingCacheSurgePickupRangeBonus) : 0f;
        private readonly ISurvivorsExplorationPort _port;
        public SurvivorsRoamingCacheEncounter(ISurvivorsExplorationPort port) => _port = port ?? throw new ArgumentNullException(nameof(port));
        private readonly HashSet<long> _members = new HashSet<long>();
        public IReadOnlyCollection<long> ActiveMembers => _members;
        public int ActiveCount => _members.Count;
        public bool RemoveEnemy(long id) => _members.Remove(id) && _members.Count == 0;
        public void ClearMembers() => _members.Clear();
        private float _roamingCacheSurgeTimer;
        public float SurgeRemaining => Mathf.Max(0f, _roamingCacheSurgeTimer);
        public int RoamingCacheDropCount { get; private set; }
        public int RoamingCacheExperienceGemDropCount { get; private set; }
        public int RoamingCacheMagnetDropCount { get; private set; }
        public int RoamingCacheBloodShardDropCount { get; private set; }
        public int RoamingCacheAmbushCount { get; private set; }
        public int RoamingCacheAmbushEnemySpawnCount { get; private set; }
        public int RoamingCacheAmbushClearRewardCount { get; private set; }
        public int RoamingCacheAmbushClearExperienceGemDropCount { get; private set; }
        public int RoamingCacheAmbushClearMagnetDropCount { get; private set; }
        public int RoamingCacheAmbushClearBloodShardDropCount { get; private set; }
        public int RoamingCacheSurgeActivationCount { get; private set; }
        public int RoamingCacheSurgeBonusExperienceGemDropCount { get; private set; }
        public int RoamingCacheSurgePulseHitCount { get; private set; }
        public string LastRoamingCacheAmbushClearFeedbackLabel { get; private set; } = string.Empty;
        public string LastRoamingCacheSurgeFeedbackLabel { get; private set; } = string.Empty;

        public void Reset()
        {
            ClearMembers();
            RoamingCacheDropCount = 0;
            RoamingCacheExperienceGemDropCount = 0;
            RoamingCacheMagnetDropCount = 0;
            RoamingCacheBloodShardDropCount = 0;
            RoamingCacheAmbushCount = 0;
            RoamingCacheAmbushEnemySpawnCount = 0;
            RoamingCacheAmbushClearRewardCount = 0;
            RoamingCacheAmbushClearExperienceGemDropCount = 0;
            RoamingCacheAmbushClearMagnetDropCount = 0;
            RoamingCacheAmbushClearBloodShardDropCount = 0;
            RoamingCacheSurgeActivationCount = 0;
            RoamingCacheSurgeBonusExperienceGemDropCount = 0;
            RoamingCacheSurgePulseHitCount = 0;
            LastRoamingCacheAmbushClearFeedbackLabel = string.Empty;
            LastRoamingCacheSurgeFeedbackLabel = string.Empty;
            _roamingCacheSurgeTimer = 0f;
        }

        public void SpawnRoamingArenaCache(Vector3 direction, int sequenceOffset)
        {
            Vector3 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : _port.PlayerForward;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }

            Vector3 side = new Vector3(-forward.z, 0f, forward.x);
            int nextCacheNumber = RoamingCacheDropCount + 1;
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(_port.Tuning.EnemyExperienceReward * (1f + _port.Escalation * 0.08f) * _port.Endless.ExperienceMultiplier));
            int gemCount = Mathf.Max(1, _port.Tuning.RoamingCacheExperienceGemCount + _port.Endless.GemBonus);
            int spawnedExperience = 0;
            Vector3 origin = _port.PlayerPosition + forward * (3.1f + sequenceOffset * 0.45f);
            for (int i = 0; i < gemCount; i++)
            {
                float lane = (i - (gemCount - 1) * 0.5f) * 0.72f;
                Vector3 position = origin + side * lane + forward * (i * 0.18f);
                if (_port.SpawnPickup(SurvivorsPickupKind.Experience, position, xpPerGem))
                {
                    spawnedExperience += xpPerGem;
                    RoamingCacheExperienceGemDropCount++;
                }
            }

            bool spawnedMagnet = false;
            if (SurvivorsExplorationRules.IsCadence(nextCacheNumber, _port.Tuning.RoamingCacheMagnetInterval))
            {
                Vector3 magnetPosition = origin + forward * 0.8f;
                if (_port.SpawnPickup(SurvivorsPickupKind.Magnet, magnetPosition, 1))
                {
                    spawnedMagnet = true;
                    RoamingCacheMagnetDropCount++;
                }
            }

            bool spawnedBloodShard = false;
            if (SurvivorsExplorationRules.IsCadence(nextCacheNumber, _port.Tuning.RoamingCacheBloodShardInterval))
            {
                Vector3 shardPosition = origin - forward * 0.35f;
                int shardAmount = _port.Tuning.BloodShardPickupAmount + _port.Endless.ShardBonus;
                if (_port.SpawnPickup(SurvivorsPickupKind.BloodShard, shardPosition, shardAmount))
                {
                    spawnedBloodShard = true;
                    RoamingCacheBloodShardDropCount++;
                }
            }

            int ambushSpawned = SpawnRoamingArenaCacheAmbush(origin, forward, side, nextCacheNumber);
            int surgeExperience = TryActivateRoamingCacheSurge(nextCacheNumber, origin, forward, side, xpPerGem, out int surgeHitCount);
            if (spawnedExperience <= 0 && !spawnedMagnet && !spawnedBloodShard && ambushSpawned <= 0 && surgeExperience <= 0 && surgeHitCount <= 0)
            {
                return;
            }

            RoamingCacheDropCount++;
            string label = $"Roaming Cache: +{spawnedExperience} XP";
            if (spawnedMagnet)
            {
                label += " + Magnet";
            }

            if (spawnedBloodShard)
            {
                label += " + Shard";
            }

            if (ambushSpawned > 0)
            {
                label += $" + Ambush x{ambushSpawned}";
            }

            if (surgeExperience > 0 || surgeHitCount > 0)
            {
                label += $" + Wayfinder Surge (+{surgeExperience} XP, {surgeHitCount} hit)";
            }

            label += _port.Endless.LabelSuffix;
            _port.Feedback.Record(label, new Color(0.35f, 0.9f, 1f));
        }

        private int TryActivateRoamingCacheSurge(int cacheNumber, Vector3 origin, Vector3 forward, Vector3 side, int xpPerGem, out int pulseHitCount)
        {
            pulseHitCount = 0;
            if (!SurvivorsExplorationRules.IsCadence(cacheNumber, _port.Tuning.RoamingCacheSurgeInterval))
            {
                return 0;
            }

            _roamingCacheSurgeTimer = Mathf.Max(0.1f, _port.Tuning.RoamingCacheSurgeDurationSeconds);
            RoamingCacheSurgeActivationCount++;

            int gemCount = Mathf.Max(0, _port.Tuning.RoamingCacheSurgeBonusGemCount + _port.Endless.GemBonus);
            int spawnedExperience = 0;
            int bonusXpPerGem = Mathf.Max(1, Mathf.RoundToInt(xpPerGem * (2f + _port.Endless.Tier * 0.12f)));
            Vector3 center = origin + forward * 1.15f;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.35f) / Mathf.Max(1, gemCount)) * Mathf.PI * 2f;
                Vector3 offset = side * (Mathf.Cos(angle) * 0.92f) + forward * (Mathf.Sin(angle) * 0.92f);
                if (_port.SpawnPickup(SurvivorsPickupKind.Experience, center + offset, bonusXpPerGem))
                {
                    spawnedExperience += bonusXpPerGem;
                    RoamingCacheSurgeBonusExperienceGemDropCount++;
                }
            }

            pulseHitCount = TriggerRoamingCacheSurgePulse(center);
            LastRoamingCacheSurgeFeedbackLabel = $"Wayfinder Surge: +{spawnedExperience} XP, {pulseHitCount} enemies hit{_port.Endless.LabelSuffix}";
            return spawnedExperience;
        }

        private int TriggerRoamingCacheSurgePulse(Vector3 center)
        {
            float radius = Mathf.Max(0f, _port.Tuning.RoamingCacheSurgePulseRadius);
            float damage = Mathf.Max(0f, _port.Tuning.RoamingCacheSurgePulseDamage * _port.Endless.PulseMultiplier);
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                hitCount = _port.DamageNonMajorEnemies(center, radius, damage, "survivors.roaming-cache.surge");
            }

            RoamingCacheSurgePulseHitCount += hitCount;
            _port.Feedback.Pulse(center, Mathf.Clamp(32 + hitCount * 4, 40, 80));
            return hitCount;
        }

        private int SpawnRoamingArenaCacheAmbush(Vector3 origin, Vector3 forward, Vector3 side, int cacheNumber)
        {
            if (!ShouldTriggerRoamingCacheAmbush(cacheNumber))
            {
                return 0;
            }

            int baseCount = Mathf.Max(0, _port.Tuning.RoamingCacheAmbushBaseEnemyCount);
            if (baseCount <= 0)
            {
                return 0;
            }

            int maxCount = Mathf.Max(baseCount, _port.Tuning.RoamingCacheAmbushMaxEnemyCount);
            int interval = Mathf.Max(1, _port.Tuning.RoamingCacheAmbushInterval);
            int start = Mathf.Max(1, _port.Tuning.RoamingCacheAmbushStartCache);
            int distancePressureBonus = Mathf.Max(0, cacheNumber - start) / (interval * 2);
            int escalationBonus = Mathf.Max(0, _port.Escalation) / 3;
            int targetCount = Mathf.Clamp(baseCount + distancePressureBonus + escalationBonus + _port.Endless.PressureBonus, 1, maxCount);
            int available = Mathf.Max(0, _port.MaximumAlive + Mathf.Max(0, _port.Tuning.RoamingCacheAmbushExtraAliveAllowance) - _port.ActiveEnemyCount);
            targetCount = Mathf.Min(targetCount, available);
            if (targetCount <= 0)
            {
                return 0;
            }

            int spawned = 0;
            float radius = Mathf.Max(1f, _port.Tuning.RoamingCacheAmbushRadius);
            Vector3 center = origin - forward * (radius * 0.35f);
            for (int i = 0; i < targetCount; i++)
            {
                SurvivorsEnemyRole role = SurvivorsExplorationRules.ResolveAmbushRole(i, cacheNumber, _port.Escalation);
                long enemy = _port.SpawnEnemy(
                    role,
                    _port.SpawnSequence + i + cacheNumber * 37 + 809,
                    radius,
                    radius + _port.Tuning.SpawnBandDepth,
                    "roaming-cache-ambush");
                if (enemy > 0)
                {
                    _members.Add(enemy);
                    spawned++;
                }
            }

            if (spawned > 0)
            {
                RoamingCacheAmbushCount++;
                RoamingCacheAmbushEnemySpawnCount += spawned;
            }

            return spawned;
        }

        private bool ShouldTriggerRoamingCacheAmbush(int cacheNumber)
        {
            int start = Mathf.Max(1, _port.Tuning.RoamingCacheAmbushStartCache);
            int interval = Mathf.Max(0, _port.Tuning.RoamingCacheAmbushInterval);
            return interval > 0 && cacheNumber >= start && ((cacheNumber - start) % interval) == 0;
        }

        public void SpawnRoamingCacheAmbushClearReward(Vector3 position)
        {
            int gemCount = Mathf.Max(1, _port.Tuning.RoamingCacheExperienceGemCount + 1 + _port.Endless.GemBonus);
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(_port.Tuning.EnemyExperienceReward * (1.35f + _port.Escalation * 0.08f) * _port.Endless.ExperienceMultiplier));
            float radius = 0.72f + Mathf.Min(0.9f, gemCount * 0.08f);
            int spawnedExperience = 0;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.2f) / gemCount) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (_port.SpawnPickup(SurvivorsPickupKind.Experience, position + offset, xpPerGem))
                {
                    spawnedExperience += xpPerGem;
                    RoamingCacheAmbushClearExperienceGemDropCount++;
                }
            }

            if (spawnedExperience <= 0)
            {
                return;
            }

            int clearNumber = RoamingCacheAmbushClearRewardCount + 1;
            bool spawnedMagnet = false;
            if (SurvivorsExplorationRules.IsCadence(clearNumber, _port.Tuning.RoamingCacheAmbushClearMagnetInterval))
            {
                Vector3 magnetPosition = position + new Vector3(radius * 0.85f, 0f, -radius * 0.3f);
                if (_port.SpawnPickup(SurvivorsPickupKind.Magnet, magnetPosition, 1))
                {
                    spawnedMagnet = true;
                    RoamingCacheAmbushClearMagnetDropCount++;
                }
            }

            bool spawnedBloodShard = false;
            if (SurvivorsExplorationRules.IsCadence(clearNumber, _port.Tuning.RoamingCacheAmbushClearBloodShardInterval))
            {
                Vector3 shardPosition = position + new Vector3(-radius * 0.7f, 0f, radius * 0.45f);
                int shardAmount = _port.Tuning.BloodShardPickupAmount + _port.Endless.ShardBonus;
                if (_port.SpawnPickup(SurvivorsPickupKind.BloodShard, shardPosition, shardAmount))
                {
                    spawnedBloodShard = true;
                    RoamingCacheAmbushClearBloodShardDropCount++;
                }
            }

            RoamingCacheAmbushClearRewardCount++;
            string label = $"Roaming Ambush Cleared: +{spawnedExperience} XP";
            if (spawnedMagnet)
            {
                label += " + Magnet";
            }

            if (spawnedBloodShard)
            {
                label += " + Shard";
            }

            label += _port.Endless.LabelSuffix;
            LastRoamingCacheAmbushClearFeedbackLabel = label;
            _port.Feedback.Record(label, new Color(0.5f, 1f, 0.68f));
            _port.Feedback.Pulse(position, 18);
        }

        public void TickRoamingCacheSurge(float deltaTime)
        {
            if (_roamingCacheSurgeTimer <= 0f)
            {
                return;
            }

            _roamingCacheSurgeTimer = Mathf.Max(0f, _roamingCacheSurgeTimer - Mathf.Max(0f, deltaTime));
        }
    }
}
