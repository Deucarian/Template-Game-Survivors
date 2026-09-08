using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal sealed class SurvivorsWaystoneExploration
    {
        private readonly ISurvivorsExplorationPort _port;
        public SurvivorsWaystoneExploration(ISurvivorsExplorationPort port) => _port = port ?? throw new ArgumentNullException(nameof(port));
        private readonly HashSet<long> _discovered = new HashSet<long>();
        public bool IsDiscovered(long key) => _discovered.Contains(key);
        public void ClearDiscoveries() => _discovered.Clear();
        public bool TryDiscover(long key, Vector3 position)
        {
            float radius = _port.Tuning.WaystoneDiscoveryRadius;
            if (radius <= 0f || _discovered.Contains(key)) return false;
            Vector3 delta = position - _port.PlayerPosition;
            delta.y = 0f;
            if (delta.sqrMagnitude > radius * radius) return false;
            _discovered.Add(key);
            SpawnWaystoneDiscoveryReward(position);
            return true;
        }
        private float _waystoneFocusTimer;
        private float _waystoneChainSurgeTimer;
        public float FocusRemaining => Mathf.Max(0f, _waystoneFocusTimer);
        public float ChainRemaining => Mathf.Max(0f, _waystoneChainSurgeTimer);
        public int WaystoneDiscoveryCount { get; private set; }
        public int WaystoneExperienceGemDropCount { get; private set; }
        public int WaystoneBloodShardDropCount { get; private set; }
        public int WaystoneAmbushCount { get; private set; }
        public int WaystoneAmbushEnemySpawnCount { get; private set; }
        public int WaystoneFocusActivationCount { get; private set; }
        public int WaystoneChainSurgeActivationCount { get; private set; }
        public int WaystoneChainSurgeBonusExperienceGemDropCount { get; private set; }
        public int WaystoneChainSurgePulseHitCount { get; private set; }
        public string LastWaystoneDiscoveryFeedbackLabel { get; private set; } = string.Empty;
        public string LastWaystoneChainSurgeFeedbackLabel { get; private set; } = string.Empty;

        public void Reset()
        {
            ClearDiscoveries();
            WaystoneDiscoveryCount = 0;
            WaystoneExperienceGemDropCount = 0;
            WaystoneBloodShardDropCount = 0;
            WaystoneAmbushCount = 0;
            WaystoneAmbushEnemySpawnCount = 0;
            WaystoneFocusActivationCount = 0;
            WaystoneChainSurgeActivationCount = 0;
            WaystoneChainSurgeBonusExperienceGemDropCount = 0;
            WaystoneChainSurgePulseHitCount = 0;
            LastWaystoneDiscoveryFeedbackLabel = string.Empty;
            LastWaystoneChainSurgeFeedbackLabel = string.Empty;
            _waystoneFocusTimer = 0f;
            _waystoneChainSurgeTimer = 0f;
        }

        private void SpawnWaystoneDiscoveryReward(Vector3 position)
        {
            int discoveryNumber = WaystoneDiscoveryCount + 1;
            bool focusActivated = ActivateWaystoneFocus();
            int gemCount = Mathf.Max(1, _port.Tuning.WaystoneExperienceGemCount + _port.Endless.GemBonus);
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(_port.Tuning.EnemyExperienceReward * (1.45f + _port.Escalation * 0.08f) * _port.Endless.ExperienceMultiplier));
            int spawnedExperience = 0;
            float rewardRadius = 0.7f + Mathf.Min(0.8f, gemCount * 0.08f);
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.15f) / gemCount) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * rewardRadius;
                if (_port.SpawnPickup(SurvivorsPickupKind.Experience, position + offset, xpPerGem))
                {
                    spawnedExperience += xpPerGem;
                    WaystoneExperienceGemDropCount++;
                }
            }

            bool spawnedBloodShard = false;
            if (SurvivorsExplorationRules.IsCadence(discoveryNumber, _port.Tuning.WaystoneBloodShardInterval))
            {
                int shardAmount = _port.Tuning.BloodShardPickupAmount + _port.Endless.ShardBonus;
                if (_port.SpawnPickup(SurvivorsPickupKind.BloodShard, position + new Vector3(-rewardRadius, 0f, rewardRadius * 0.45f), shardAmount))
                {
                    spawnedBloodShard = true;
                    WaystoneBloodShardDropCount++;
                }
            }

            int ambushSpawned = SpawnWaystoneDiscoveryAmbush(position, discoveryNumber);
            int chainExperience = TryActivateWaystoneChainSurge(discoveryNumber, position, xpPerGem, out int chainHitCount);
            WaystoneDiscoveryCount++;
            string label = $"Waystone Discovered: +{spawnedExperience} XP";
            if (spawnedBloodShard)
            {
                label += " + Shard";
            }

            if (focusActivated)
            {
                label += $" + Focus {FocusRemaining:0.#}s";
            }

            if (ambushSpawned > 0)
            {
                label += $" + Ambush x{ambushSpawned}";
            }

            if (chainExperience > 0 || chainHitCount > 0)
            {
                label += $" + Waystone Chain (+{chainExperience} XP, {chainHitCount} hit)";
            }

            label += _port.Endless.LabelSuffix;
            LastWaystoneDiscoveryFeedbackLabel = label;
            _port.Feedback.Record(label, new Color(0.58f, 0.95f, 1f));
            _port.Feedback.Pulse(position, ambushSpawned > 0 ? 24 : 16);
        }

        private bool ActivateWaystoneFocus()
        {
            float duration = Mathf.Max(0f, _port.Tuning.WaystoneFocusDurationSeconds);
            if (duration <= 0f)
            {
                return false;
            }

            _waystoneFocusTimer = Mathf.Max(_waystoneFocusTimer, duration);
            WaystoneFocusActivationCount++;
            return true;
        }

        private int TryActivateWaystoneChainSurge(int discoveryNumber, Vector3 position, int xpPerGem, out int pulseHitCount)
        {
            pulseHitCount = 0;
            if (!SurvivorsExplorationRules.IsCadence(discoveryNumber, _port.Tuning.WaystoneChainInterval))
            {
                return 0;
            }

            _waystoneChainSurgeTimer = Mathf.Max(0.1f, _port.Tuning.WaystoneChainDurationSeconds);
            WaystoneChainSurgeActivationCount++;

            int gemCount = Mathf.Max(0, _port.Tuning.WaystoneChainBonusGemCount + _port.Endless.GemBonus);
            int spawnedExperience = 0;
            int bonusXpPerGem = Mathf.Max(1, Mathf.RoundToInt(xpPerGem * (2.35f + _port.Endless.Tier * 0.12f)));
            float rewardRadius = 0.85f + Mathf.Min(0.95f, gemCount * 0.09f);
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.4f) / Mathf.Max(1, gemCount)) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * rewardRadius;
                if (_port.SpawnPickup(SurvivorsPickupKind.Experience, position + offset, bonusXpPerGem))
                {
                    spawnedExperience += bonusXpPerGem;
                    WaystoneChainSurgeBonusExperienceGemDropCount++;
                }
            }

            pulseHitCount = TriggerWaystoneChainSurgePulse(position);
            LastWaystoneChainSurgeFeedbackLabel = $"Waystone Chain: {discoveryNumber} discoveries, +{spawnedExperience} XP, {pulseHitCount} enemies hit{_port.Endless.LabelSuffix}";
            return spawnedExperience;
        }

        private int TriggerWaystoneChainSurgePulse(Vector3 center)
        {
            float radius = Mathf.Max(0f, _port.Tuning.WaystoneChainPulseRadius);
            float damage = Mathf.Max(0f, _port.Tuning.WaystoneChainPulseDamage * _port.Endless.PulseMultiplier);
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                hitCount = _port.DamageNonMajorEnemies(center, radius, damage, "survivors.waystone.chain-surge");
            }

            WaystoneChainSurgePulseHitCount += hitCount;
            _port.Feedback.Pulse(center, Mathf.Clamp(34 + hitCount * 5, 42, 86));
            return hitCount;
        }

        private int SpawnWaystoneDiscoveryAmbush(Vector3 position, int discoveryNumber)
        {
            if (!SurvivorsExplorationRules.IsCadence(discoveryNumber, _port.Tuning.WaystoneAmbushInterval))
            {
                return 0;
            }

            int baseCount = Mathf.Max(0, _port.Tuning.WaystoneAmbushBaseEnemyCount);
            if (baseCount <= 0)
            {
                return 0;
            }

            int available = Mathf.Max(0, _port.MaximumAlive + Mathf.Max(0, _port.Tuning.WaystoneAmbushExtraAliveAllowance) - _port.ActiveEnemyCount);
            int targetCount = Mathf.Min(baseCount + Mathf.Max(0, _port.Escalation) / 4 + _port.Endless.PressureBonus, available);
            if (targetCount <= 0)
            {
                return 0;
            }

            int spawned = 0;
            float radius = Mathf.Max(1f, _port.Tuning.WaystoneAmbushRadius);
            for (int i = 0; i < targetCount; i++)
            {
                SurvivorsEnemyRole role = SurvivorsExplorationRules.ResolveAmbushRole(i, discoveryNumber + 2, _port.Escalation);
                if (_port.SpawnEnemy(
                    role,
                    _port.SpawnSequence + i + discoveryNumber * 43 + 907,
                    radius,
                    radius + _port.Tuning.SpawnBandDepth,
                    "waystone-ambush") > 0)
                {
                    spawned++;
                }
            }

            if (spawned > 0)
            {
                WaystoneAmbushCount++;
                WaystoneAmbushEnemySpawnCount += spawned;
            }

            return spawned;
        }

        public void TickWaystoneFocus(float deltaTime)
        {
            if (_waystoneFocusTimer <= 0f)
            {
                return;
            }

            _waystoneFocusTimer = Mathf.Max(0f, _waystoneFocusTimer - Mathf.Max(0f, deltaTime));
        }

        public void TickWaystoneChainSurge(float deltaTime)
        {
            if (_waystoneChainSurgeTimer <= 0f)
            {
                return;
            }

            _waystoneChainSurgeTimer = Mathf.Max(0f, _waystoneChainSurgeTimer - Mathf.Max(0f, deltaTime));
        }
    }
}
