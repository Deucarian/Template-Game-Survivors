using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsHordeRushPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        float RunTime { get; }
        int Escalation { get; }
        int MaximumAlive { get; }
        int ActiveEnemyCount { get; }
        long SpawnSequence { get; }
        long SpawnEnemy(SurvivorsEnemyRole role, long seed, float minimum, float maximum);
        bool SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount);
        int DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source);
        void ShowWarning(string label, float radius, float remaining);
        void ShowBurst(string label);
        void ShowClear(Vector3 position, string label, int hitCount);
    }

    /// <summary>Owns horde scheduling, membership, clear rewards and Breaker momentum.</summary>
    internal sealed class SurvivorsHordeRushEncounter
    {
        private readonly ISurvivorsHordeRushPort _port;
        private readonly HashSet<long> _members = new HashSet<long>();
        private SurvivorsTemplateTuning CurrentTuning => _port.Tuning;
        private float RunTimeSeconds => _port.RunTime;
        private int RunEscalationLevel => _port.Escalation;
        public SurvivorsHordeRushEncounter(ISurvivorsHordeRushPort port) => _port = port ?? throw new ArgumentNullException(nameof(port));
        public IReadOnlyCollection<long> ActiveMembers => _members;
        public int ActiveCount => _members.Count;
        public bool RemoveEnemy(long id) => _members.Remove(id) && _members.Count == 0;
        public void ClearMembers() => _members.Clear();
        public float NextTime => _nextHordeRushTimeSeconds;
        public bool WarningActive => !string.IsNullOrEmpty(_hordeRushWarningLabel) && RunTimeSeconds < _hordeRushWarningTargetTimeSeconds;
        public string WarningLabel => WarningActive ? _hordeRushWarningLabel : string.Empty;
        public float WarningRemaining => WarningActive ? Mathf.Max(0f, _hordeRushWarningTargetTimeSeconds - RunTimeSeconds) : 0f;
        public float ClearSurgeRemaining => Mathf.Max(0f, _hordeRushClearSurgeTimer);
        private bool _hordeRushWarningShown;
        private float _nextHordeRushTimeSeconds;
        private int _hordeRushSequence;
        private string _hordeRushWarningLabel = string.Empty;
        private float _hordeRushWarningTargetTimeSeconds;
        private float _hordeRushClearSurgeTimer;
        public int HordeRushSpawnCount { get; private set; }
        public int HordeRushEnemySpawnCount { get; private set; }
        public int HordeRushWarningCount { get; private set; }
        public int HordeRushClearRewardCount { get; private set; }
        public int HordeRushClearExperienceGemDropCount { get; private set; }
        public int HordeRushClearSpecialDropCount { get; private set; }
        public int HordeRushClearPulseCount { get; private set; }
        public int HordeRushClearPulseHitCount { get; private set; }
        public int HordeRushClearSurgeActivationCount { get; private set; }
        public string LastHordeRushFeedbackLabel { get; private set; } = string.Empty;
        public string LastHordeRushClearFeedbackLabel { get; private set; } = string.Empty;
        public string LastHordeRushClearPulseFeedbackLabel { get; private set; } = string.Empty;

        public void Reset()
        {
            ClearMembers();
            _hordeRushClearSurgeTimer = 0f;
            HordeRushSpawnCount = 0;
            HordeRushEnemySpawnCount = 0;
            HordeRushWarningCount = 0;
            HordeRushClearRewardCount = 0;
            HordeRushClearExperienceGemDropCount = 0;
            HordeRushClearSpecialDropCount = 0;
            HordeRushClearPulseCount = 0;
            HordeRushClearPulseHitCount = 0;
            HordeRushClearSurgeActivationCount = 0;
            LastHordeRushFeedbackLabel = string.Empty;
            LastHordeRushClearFeedbackLabel = string.Empty;
            LastHordeRushClearPulseFeedbackLabel = string.Empty;
            ResetHordeRushSchedule();
        }

        public int Trigger()
        {
            int spawned = SpawnHordeRushBurst();
            _hordeRushSequence++;
            ScheduleNextHordeRush(RunTimeSeconds, firstRush: false);
            if (spawned <= 0)
            {
                return 0;
            }

            HordeRushSpawnCount++;
            HordeRushEnemySpawnCount += spawned;
            LastHordeRushFeedbackLabel = $"Horde Rush {HordeRushSpawnCount}: {spawned} enemies";
            _port.ShowBurst(LastHordeRushFeedbackLabel);
            return spawned;
        }

        private void ResetHordeRushSchedule()
        {
            _hordeRushSequence = 0;
            _hordeRushWarningShown = false;
            _hordeRushWarningLabel = string.Empty;
            _hordeRushWarningTargetTimeSeconds = 0f;
            ScheduleNextHordeRush(0f, firstRush: true);
        }

        public void EnsureFutureHordeRushScheduled()
        {
            if (_nextHordeRushTimeSeconds <= RunTimeSeconds)
            {
                ScheduleNextHordeRush(RunTimeSeconds, firstRush: false);
            }
        }

        private void ScheduleNextHordeRush(float startTimeSeconds, bool firstRush)
        {
            _hordeRushWarningShown = false;
            _hordeRushWarningLabel = string.Empty;
            _hordeRushWarningTargetTimeSeconds = 0f;
            float interval = firstRush
                ? CurrentTuning.HordeRushFirstTimeSeconds
                : CurrentTuning.HordeRushIntervalSeconds;
            _nextHordeRushTimeSeconds = interval <= 0f
                ? 0f
                : Mathf.Max(0f, startTimeSeconds) + Mathf.Max(0.1f, interval);
        }

        public void TickHordeRushEvents()
        {
            if (_nextHordeRushTimeSeconds <= 0f)
            {
                return;
            }

            TryBeginHordeRushWarning();
            if (RunTimeSeconds < _nextHordeRushTimeSeconds)
            {
                return;
            }

            Trigger();
        }

        private void TryBeginHordeRushWarning()
        {
            float leadSeconds = Mathf.Max(0f, CurrentTuning.HordeRushWarningLeadSeconds);
            if (_hordeRushWarningShown || leadSeconds <= 0f || _nextHordeRushTimeSeconds <= 0f)
            {
                return;
            }

            float warningTime = Mathf.Max(0f, _nextHordeRushTimeSeconds - leadSeconds);
            if (RunTimeSeconds < warningTime || RunTimeSeconds >= _nextHordeRushTimeSeconds)
            {
                return;
            }

            _hordeRushWarningShown = true;
            _hordeRushWarningLabel = "HORDE RUSH INCOMING";
            _hordeRushWarningTargetTimeSeconds = _nextHordeRushTimeSeconds;
            HordeRushWarningCount++;
            LastHordeRushFeedbackLabel = _hordeRushWarningLabel;
            _port.ShowWarning(_hordeRushWarningLabel, Mathf.Max(3f, CurrentTuning.HordeRushSpawnRadius), _nextHordeRushTimeSeconds - RunTimeSeconds);
        }

        private int SpawnHordeRushBurst()
        {
            int available = Mathf.Max(0, _port.MaximumAlive + Mathf.Max(0, CurrentTuning.HordeRushExtraAliveAllowance) - _port.ActiveEnemyCount);
            if (available <= 0)
            {
                return 0;
            }

            int targetCount = Mathf.Min(ResolveHordeRushEnemyCount(), available);
            float radius = Mathf.Max(3f, CurrentTuning.HordeRushSpawnRadius);
            int spawned = 0;
            for (int i = 0; i < targetCount; i++)
            {
                float laneRadius = radius + ((i & 1) == 0 ? 0f : 1.35f);
                SurvivorsEnemyRole role = ResolveHordeRushRole(i);
                long enemy = _port.SpawnEnemy(
                    role,
                    _port.SpawnSequence + i + _hordeRushSequence * 47 + 947,
                    laneRadius,
                    laneRadius + CurrentTuning.SpawnBandDepth);
                if (enemy > 0)
                {
                    _members.Add(enemy);
                    spawned++;
                }
            }

            return spawned;
        }

        public void SpawnHordeRushClearReward(Vector3 position)
        {
            int gemCount = Mathf.Max(1, CurrentTuning.HordeRushClearExperienceGemCount);
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(
                CurrentTuning.EnemyExperienceReward *
                Mathf.Max(0.1f, CurrentTuning.HordeRushClearExperienceMultiplier) *
                (1f + RunEscalationLevel * 0.08f)));
            float radius = 0.85f + Mathf.Min(1.1f, gemCount * 0.08f);
            int spawnedExperience = 0;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.1f) / gemCount) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (_port.SpawnPickup(SurvivorsPickupKind.Experience, position + offset, xpPerGem))
                {
                    spawnedExperience += xpPerGem;
                    HordeRushClearExperienceGemDropCount++;
                }
            }

            int specialDropCount = 0;
            int nextClearNumber = HordeRushClearRewardCount + 1;
            if (ShouldDropHordeRushSpecial(nextClearNumber, CurrentTuning.HordeRushClearMagnetEveryRush))
            {
                Vector3 magnetPosition = position + new Vector3(radius * 0.7f, 0f, -radius * 0.35f);
                if (_port.SpawnPickup(SurvivorsPickupKind.Magnet, magnetPosition, 1))
                {
                    specialDropCount++;
                }
            }

            if (ShouldDropHordeRushSpecial(nextClearNumber, CurrentTuning.HordeRushClearBloodShardEveryRush))
            {
                int shardAmount = Mathf.Max(1, CurrentTuning.BloodShardPickupAmount);
                Vector3 shardPosition = position + new Vector3(-radius * 0.55f, 0f, radius * 0.48f);
                if (_port.SpawnPickup(SurvivorsPickupKind.BloodShard, shardPosition, shardAmount))
                {
                    specialDropCount++;
                }
            }

            if (spawnedExperience <= 0 && specialDropCount <= 0)
            {
                return;
            }

            HordeRushClearRewardCount++;
            HordeRushClearSpecialDropCount += specialDropCount;
            int pulseHitCount = TriggerHordeRushClearPulse(position);
            bool surgeActivated = ActivateHordeRushClearSurge();
            string specialLabel = specialDropCount > 0 ? $" + {specialDropCount} special" : string.Empty;
            string pulseLabel = pulseHitCount > 0 ? $" + Breaker Pulse ({pulseHitCount} hit)" : string.Empty;
            string surgeLabel = surgeActivated ? $" + Breaker Surge {ClearSurgeRemaining:0.#}s" : string.Empty;
            string label = $"Horde Rush Cleared: +{spawnedExperience} XP{specialLabel}{pulseLabel}{surgeLabel}";
            LastHordeRushClearFeedbackLabel = label;
            LastHordeRushFeedbackLabel = label;
            _port.ShowClear(position, label, pulseHitCount);
        }

        private bool ActivateHordeRushClearSurge()
        {
            float duration = Mathf.Max(0f, CurrentTuning.HordeRushClearSurgeDurationSeconds);
            if (duration <= 0f)
            {
                return false;
            }

            _hordeRushClearSurgeTimer = Mathf.Max(_hordeRushClearSurgeTimer, duration);
            HordeRushClearSurgeActivationCount++;
            return true;
        }

        private int TriggerHordeRushClearPulse(Vector3 position)
        {
            float radius = Mathf.Max(0f, CurrentTuning.HordeRushClearPulseRadius);
            float damage = Mathf.Max(0f, CurrentTuning.HordeRushClearPulseDamage);
            if (radius <= 0f || damage <= 0f)
            {
                return 0;
            }

            int hitCount = _port.DamageNonMajorEnemies(position, radius, damage, "survivors.horde-rush.clear-pulse");

            HordeRushClearPulseCount++;
            HordeRushClearPulseHitCount += hitCount;
            LastHordeRushClearPulseFeedbackLabel = $"Breaker Pulse: {hitCount} enemies hit";
            return hitCount;
        }

        private static bool ShouldDropHordeRushSpecial(int clearNumber, int cadence)
        {
            return cadence > 0 && clearNumber > 0 && clearNumber % cadence == 0;
        }

        private int ResolveHordeRushEnemyCount()
        {
            int baseCount = Mathf.Max(1, CurrentTuning.HordeRushBaseEnemyCount);
            int increase = Mathf.Max(0, CurrentTuning.HordeRushEnemyCountIncreasePerRush);
            int maxCount = Mathf.Max(baseCount, CurrentTuning.HordeRushMaxEnemyCount);
            int escalationBonus = Mathf.Max(0, RunEscalationLevel);
            return Mathf.Clamp(baseCount + _hordeRushSequence * increase + escalationBonus, 1, maxCount);
        }

        private SurvivorsEnemyRole ResolveHordeRushRole(int index)
        {
            if (RunTimeSeconds >= 150f && index % 11 == 0)
            {
                return SurvivorsEnemyRole.Splitter;
            }

            if (RunTimeSeconds >= 110f && index % 7 == 0)
            {
                return SurvivorsEnemyRole.Bruiser;
            }

            if (RunTimeSeconds >= 95f && index % 5 == 0)
            {
                return SurvivorsEnemyRole.Spitter;
            }

            if (index % 3 == 0)
            {
                return SurvivorsEnemyRole.Runner;
            }

            return SurvivorsEnemyRole.Swarm;
        }

        public void TickHordeRushClearSurge(float deltaTime)
        {
            if (_hordeRushClearSurgeTimer <= 0f)
            {
                return;
            }

            _hordeRushClearSurgeTimer = Mathf.Max(0f, _hordeRushClearSurgeTimer - Mathf.Max(0f, deltaTime));
        }

    }
}
