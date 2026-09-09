using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsStreakRewardPort
    {
        SurvivorsTemplateTuning Tuning { get; }
        string CurrencyLabel { get; }
        bool SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount);
        bool TryDropHealth(Vector3 position);
        void ShowFeedback(string label, Color color);
    }
    /// <summary>Owns kill streak cadence, successful reward accounting and the independent Tempo Surge timer.</summary>
    internal sealed class SurvivorsKillStreakRewards
    {
        private readonly ISurvivorsStreakRewardPort _port;
        public SurvivorsKillStreakRewards(ISurvivorsStreakRewardPort port) => _port = port ?? throw new ArgumentNullException(nameof(port));
        private const float KillStreakWindowSeconds = 3.8f;
        private const int KillStreakExperienceInterval = 8;
        private const int KillStreakHealthInterval = 16;
        private const int KillStreakMagnetInterval = 24;
        private const int KillStreakBloodShardInterval = 32;
        private const int KillStreakSurgeInterval = 16;
        private const int KillStreakSurgeMaxTier = 5;
        private const float StreakSurgeDurationSeconds = 6f;
        private const float StreakSurgeDamageBonusPerTier = 1.1f;
        private const float StreakSurgeMoveSpeedBonusPerTier = 0.16f;
        private const float StreakSurgeCooldownReductionPerTier = 0.025f;
        private const float StreakSurgePickupRangeBonusPerTier = 0.18f;
        private int _killStreakCount;
        private float _killStreakTimer;
        private float _streakSurgeTimer;
        public int BestKillStreak { get; private set; }
        public int StreakBonusDropCount { get; private set; }
        public int StreakHealthDropCount { get; private set; }
        public int StreakMagnetDropCount { get; private set; }
        public int StreakBloodShardDropCount { get; private set; }
        public int StreakSurgeTier { get; private set; }
        public int StreakSurgeActivationCount { get; private set; }
        public int CurrentKillStreak => _killStreakTimer > 0f ? _killStreakCount : 0;
        public bool IsStreakSurgeActive => _streakSurgeTimer > 0f && StreakSurgeTier > 0;
        public float StreakSurgeRemainingSeconds => Mathf.Max(0f, _streakSurgeTimer);
        public float StreakSurgeDamageBonus => IsStreakSurgeActive ? StreakSurgeTier * StreakSurgeDamageBonusPerTier : 0f;
        public float StreakSurgeMoveSpeedBonus => IsStreakSurgeActive ? StreakSurgeTier * StreakSurgeMoveSpeedBonusPerTier : 0f;
        public float StreakSurgeCooldownMultiplierBonus => IsStreakSurgeActive ? -StreakSurgeTier * StreakSurgeCooldownReductionPerTier : 0f;
        public float StreakSurgePickupRangeBonus => IsStreakSurgeActive ? StreakSurgeTier * StreakSurgePickupRangeBonusPerTier : 0f;
        public void Reset()
        {
            _killStreakCount = 0; _killStreakTimer = 0f; _streakSurgeTimer = 0f;
            BestKillStreak = StreakBonusDropCount = StreakHealthDropCount = StreakMagnetDropCount = StreakBloodShardDropCount = StreakSurgeTier = StreakSurgeActivationCount = 0;
        }
        public void RegisterKillStreak(Vector3 position)
        {
            _killStreakCount = _killStreakTimer > 0f ? _killStreakCount + 1 : 1;
            _killStreakTimer = KillStreakWindowSeconds;
            BestKillStreak = Mathf.Max(BestKillStreak, _killStreakCount);

            if (_killStreakCount % KillStreakExperienceInterval == 0)
            {
                int amount = Mathf.Max(2, Mathf.CeilToInt(_port.Tuning.EnemyExperienceReward * (2f + _killStreakCount * 0.15f)));
                Vector3 offset = new Vector3(Mathf.Sin(_killStreakCount) * 0.55f, 0f, Mathf.Cos(_killStreakCount) * 0.55f);
                if (_port.SpawnPickup(SurvivorsPickupKind.Experience, position + offset, amount))
                {
                    StreakBonusDropCount++;
                    _port.ShowFeedback($"{_killStreakCount} Streak: Bonus XP +{amount}", new Color(0.28f, 0.86f, 1f));
                }
            }

            if (_killStreakCount % KillStreakHealthInterval == 0)
            {
                Vector3 offset = new Vector3(Mathf.Cos(_killStreakCount * 0.7f) * 0.65f, 0f, Mathf.Sin(_killStreakCount * 0.7f) * 0.65f);
                if (_port.TryDropHealth(position + offset))
                {
                    StreakHealthDropCount++;
                    _port.ShowFeedback($"{_killStreakCount} Streak: Vital Shard", new Color(0.42f, 1f, 0.56f));
                }
            }

            if (_killStreakCount % KillStreakMagnetInterval == 0)
            {
                Vector3 offset = new Vector3(Mathf.Cos(_killStreakCount) * 0.75f, 0f, Mathf.Sin(_killStreakCount) * 0.75f);
                if (_port.SpawnPickup(SurvivorsPickupKind.Magnet, position + offset, 1))
                {
                    StreakMagnetDropCount++;
                    _port.ShowFeedback($"{_killStreakCount} Streak: Magnet Recall", new Color(0.55f, 0.78f, 1f));
                }
            }

            if (_killStreakCount % KillStreakBloodShardInterval == 0)
            {
                int amount = Mathf.Max(1, _port.Tuning.BloodShardPickupAmount + (_killStreakCount / KillStreakBloodShardInterval) - 1);
                Vector3 offset = new Vector3(Mathf.Sin(_killStreakCount * 0.31f) * 0.82f, 0f, Mathf.Cos(_killStreakCount * 0.31f) * 0.82f);
                if (_port.SpawnPickup(SurvivorsPickupKind.BloodShard, position + offset, amount))
                {
                    StreakBloodShardDropCount++;
                    _port.ShowFeedback($"{_killStreakCount} Streak: {_port.CurrencyLabel} +{amount}", new Color(1f, 0.34f, 0.42f));
                }
            }

            TryActivateStreakSurge();
        }

        private void TryActivateStreakSurge()
        {
            if (_killStreakCount <= 0 || _killStreakCount % KillStreakSurgeInterval != 0)
            {
                return;
            }

            int tier = Mathf.Clamp(_killStreakCount / KillStreakSurgeInterval, 1, KillStreakSurgeMaxTier);
            StreakSurgeTier = tier;
            StreakSurgeActivationCount++;
            _streakSurgeTimer = StreakSurgeDurationSeconds;
            _port.ShowFeedback($"{_killStreakCount} Streak: Tempo Surge T{tier}", new Color(1f, 0.74f, 0.24f));
        }

        public void TickKillStreak(float deltaTime)
        {
            if (_killStreakTimer <= 0f || _killStreakCount <= 0)
            {
                return;
            }

            _killStreakTimer = Mathf.Max(0f, _killStreakTimer - Mathf.Max(0f, deltaTime));
            if (_killStreakTimer <= 0f)
            {
                _killStreakCount = 0;
            }
        }

        public void TickStreakSurge(float deltaTime)
        {
            if (_streakSurgeTimer <= 0f || StreakSurgeTier <= 0)
            {
                return;
            }

            _streakSurgeTimer = Mathf.Max(0f, _streakSurgeTimer - Mathf.Max(0f, deltaTime));
            if (_streakSurgeTimer <= 0f)
            {
                StreakSurgeTier = 0;
            }
        }
    }
}
