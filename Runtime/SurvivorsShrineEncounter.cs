using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal sealed class SurvivorsShrineEncounter
    {
        public bool IsArenaShrineSurgeActive => SurgeRemaining > 0f;
        public float ArenaShrineSurgeRemainingSeconds => Mathf.Max(0f, SurgeRemaining);
        public float ArenaShrineSurgeDamageBonus => IsArenaShrineSurgeActive ? Mathf.Max(0f, _port.Tuning.ArenaShrineSurgeDamageBonus) : 0f;
        public float ArenaShrineSurgeMoveSpeedBonus => IsArenaShrineSurgeActive ? Mathf.Max(0f, _port.Tuning.ArenaShrineSurgeMoveSpeedBonus) : 0f;
        public float ArenaShrineSurgeCooldownMultiplierBonus => IsArenaShrineSurgeActive ? Mathf.Min(0f, _port.Tuning.ArenaShrineSurgeCooldownMultiplierBonus) : 0f;
        public float ArenaShrineSurgePickupRangeBonus => IsArenaShrineSurgeActive ? Mathf.Max(0f, _port.Tuning.ArenaShrineSurgePickupRangeBonus) : 0f;
        private readonly ISurvivorsExplorationPort _port;
        public SurvivorsShrineEncounter(ISurvivorsExplorationPort port) => _port = port ?? throw new ArgumentNullException(nameof(port));
        private readonly HashSet<long> _members = new HashSet<long>();
        public IReadOnlyCollection<long> ActiveMembers => _members;
        public int ActiveCount => _members.Count;
        public bool RemoveEnemy(long id) => _members.Remove(id) && _members.Count == 0;
        public void ClearMembers() => _members.Clear();
        private float _arenaShrineSurgeTimer;
        public float SurgeRemaining => Mathf.Max(0f, _arenaShrineSurgeTimer);
        public int ArenaShrineTrialCount { get; private set; }
        public int ArenaShrineEnemySpawnCount { get; private set; }
        public int ArenaShrineClearRewardCount { get; private set; }
        public int ArenaShrineClearExperienceGemDropCount { get; private set; }
        public int ArenaShrineClearBloodShardDropCount { get; private set; }
        public int ArenaShrineSurgeActivationCount { get; private set; }
        public int ArenaShrineSurgePulseHitCount { get; private set; }
        public string LastArenaShrineFeedbackLabel { get; private set; } = string.Empty;
        public string LastArenaShrineClearFeedbackLabel { get; private set; } = string.Empty;

        public void Reset()
        {
            ClearMembers();
            ArenaShrineTrialCount = 0;
            ArenaShrineEnemySpawnCount = 0;
            ArenaShrineClearRewardCount = 0;
            ArenaShrineClearExperienceGemDropCount = 0;
            ArenaShrineClearBloodShardDropCount = 0;
            ArenaShrineSurgeActivationCount = 0;
            ArenaShrineSurgePulseHitCount = 0;
            LastArenaShrineFeedbackLabel = string.Empty;
            LastArenaShrineClearFeedbackLabel = string.Empty;
            _arenaShrineSurgeTimer = 0f;
        }

        public int SpawnArenaShrineTrial(Vector3 direction)
        {
            Vector3 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : _port.PlayerForward;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }

            Vector3 side = new Vector3(-forward.z, 0f, forward.x);
            int trialNumber = ArenaShrineTrialCount + 1;
            int baseCount = Mathf.Max(1, _port.Tuning.ArenaShrineBaseEnemyCount);
            int maxCount = Mathf.Max(baseCount, _port.Tuning.ArenaShrineMaxEnemyCount);
            int targetCount = Mathf.Clamp(
                baseCount + Mathf.Max(0, trialNumber - 1) * Mathf.Max(0, _port.Tuning.ArenaShrineEnemyCountIncreasePerTrial) + Mathf.Max(0, _port.Escalation) / 3 + _port.Endless.PressureBonus,
                1,
                maxCount);
            int available = Mathf.Max(0, _port.MaximumAlive + Mathf.Max(0, _port.Tuning.ArenaShrineExtraAliveAllowance) - _port.ActiveEnemyCount);
            targetCount = Mathf.Min(targetCount, available);
            if (targetCount <= 0)
            {
                return 0;
            }

            float radius = Mathf.Max(2f, _port.Tuning.ArenaShrineSpawnRadius);
            Vector3 center = _port.PlayerPosition + forward * Mathf.Max(3.5f, radius * 0.85f);
            int spawned = 0;
            for (int i = 0; i < targetCount; i++)
            {
                SurvivorsEnemyRole role = ResolveArenaShrineRole(i, trialNumber);
                long enemy = _port.SpawnEnemy(
                    role,
                    _port.SpawnSequence + i + trialNumber * 41 + 853,
                    radius,
                    radius + _port.Tuning.SpawnBandDepth,
                    "arena-shrine");
                if (enemy <= 0)
                {
                    continue;
                }

                _members.Add(enemy);
                spawned++;
            }

            if (spawned <= 0)
            {
                return 0;
            }

            ArenaShrineTrialCount++;
            ArenaShrineEnemySpawnCount += spawned;
            LastArenaShrineFeedbackLabel = $"Arena Trial {ArenaShrineTrialCount}: shrine ring x{spawned}{_port.Endless.LabelSuffix}";
            _port.Feedback.Record(LastArenaShrineFeedbackLabel, new Color(1f, 0.78f, 0.35f));
            _port.Feedback.Pulse(center, Mathf.Clamp(28 + spawned * 3, 36, 82), true);
            return spawned;
        }

        private SurvivorsEnemyRole ResolveArenaShrineRole(int index, int trialNumber)
        {
            int pressure = Mathf.Max(0, _port.Escalation) + Mathf.Max(0, trialNumber - 1);
            int seed = index + trialNumber * 3;
            if (pressure >= 5 && seed % 6 == 0)
            {
                return SurvivorsEnemyRole.Splitter;
            }

            if (pressure >= 3 && seed % 5 == 0)
            {
                return SurvivorsEnemyRole.Spitter;
            }

            if (seed % 4 == 0)
            {
                return SurvivorsEnemyRole.Bruiser;
            }

            return seed % 2 == 0 ? SurvivorsEnemyRole.Runner : SurvivorsEnemyRole.Swarm;
        }

        public void SpawnArenaShrineClearReward(Vector3 position)
        {
            int gemCount = Mathf.Max(1, _port.Tuning.ArenaShrineClearExperienceGemCount + _port.Endless.GemBonus);
            float xpMultiplier = Mathf.Max(0.1f, _port.Tuning.ArenaShrineClearExperienceMultiplier);
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(_port.Tuning.EnemyExperienceReward * xpMultiplier * (1f + _port.Escalation * 0.1f) * _port.Endless.ExperienceMultiplier));
            float radius = 0.95f + Mathf.Min(1.25f, gemCount * 0.09f);
            int spawnedExperience = 0;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.3f) / gemCount) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (_port.SpawnPickup(SurvivorsPickupKind.Experience, position + offset, xpPerGem))
                {
                    spawnedExperience += xpPerGem;
                    ArenaShrineClearExperienceGemDropCount++;
                }
            }

            int shardAmount = Mathf.Max(0, _port.Tuning.ArenaShrineClearBloodShardAmount + _port.Endless.ShardBonus);
            bool spawnedBloodShard = false;
            if (shardAmount > 0 && _port.SpawnPickup(SurvivorsPickupKind.BloodShard, position + new Vector3(-radius * 0.75f, 0f, radius * 0.45f), shardAmount))
            {
                spawnedBloodShard = true;
                ArenaShrineClearBloodShardDropCount++;
            }

            bool surgeActivated = ActivateArenaShrineSurge();
            int pulseHitCount = TriggerArenaShrineSurgePulse(position);
            ArenaShrineClearRewardCount++;
            string label = $"Arena Trial Cleared: +{spawnedExperience} XP";
            if (spawnedBloodShard)
            {
                label += $" +{shardAmount} {_port.CurrencyRewardLabel}";
            }

            label += $" + Shrine Surge ({pulseHitCount} hit)";
            if (surgeActivated)
            {
                label += $" {SurgeRemaining:0.#}s";
            }

            label += _port.Endless.LabelSuffix;
            LastArenaShrineClearFeedbackLabel = label;
            LastArenaShrineFeedbackLabel = label;
            _port.Feedback.Record(label, new Color(1f, 0.86f, 0.42f));
            _port.Feedback.Pulse(position, Mathf.Clamp(34 + pulseHitCount * 5, 42, 92));
        }

        private bool ActivateArenaShrineSurge()
        {
            float duration = Mathf.Max(0f, _port.Tuning.ArenaShrineSurgeDurationSeconds);
            if (duration <= 0f)
            {
                return false;
            }

            _arenaShrineSurgeTimer = Mathf.Max(_arenaShrineSurgeTimer, duration);
            ArenaShrineSurgeActivationCount++;
            return true;
        }

        private int TriggerArenaShrineSurgePulse(Vector3 position)
        {
            float radius = Mathf.Max(0f, _port.Tuning.ArenaShrineSurgePulseRadius);
            float damage = Mathf.Max(0f, _port.Tuning.ArenaShrineSurgePulseDamage * _port.Endless.PulseMultiplier);
            if (radius <= 0f || damage <= 0f)
            {
                return 0;
            }

            int hitCount = 0;
            hitCount = _port.DamageNonMajorEnemies(position, radius, damage, "survivors.arena-shrine.surge");

            ArenaShrineSurgePulseHitCount += hitCount;
            return hitCount;
        }

        public void TickArenaShrineSurge(float deltaTime)
        {
            if (_arenaShrineSurgeTimer <= 0f)
            {
                return;
            }

            _arenaShrineSurgeTimer = Mathf.Max(0f, _arenaShrineSurgeTimer - Mathf.Max(0f, deltaTime));
        }
    }
}
