using System;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns post-victory threat reward escalation and its independent timed bonuses.</summary>
    internal sealed class SurvivorsEndlessSurgeRewards
    {
        private readonly SurvivorsRunSession _runSession;
        private readonly ISurvivorsPickupRewardPort _port;
        private float _endlessSurgeTimer;
        public SurvivorsEndlessSurgeRewards(SurvivorsRunSession runSession, ISurvivorsPickupRewardPort port)
        { _runSession = runSession ?? throw new ArgumentNullException(nameof(runSession)); _port = port ?? throw new ArgumentNullException(nameof(port)); }
        public int EndlessSurgeActivationCount { get; private set; }
        public int EndlessSurgeTier { get; private set; }
        public int EndlessSurgeExperienceGemDropCount { get; private set; }
        public int EndlessSurgeBloodShardDropCount { get; private set; }
        public int EndlessSurgePulseHitCount { get; private set; }
        public string LastEndlessSurgeFeedbackLabel { get; private set; } = string.Empty;
        public bool IsEndlessSurgeActive => _endlessSurgeTimer > 0f && EndlessSurgeTier > 0;
        public float EndlessSurgeRemainingSeconds => Mathf.Max(0f, _endlessSurgeTimer);
        public float EndlessSurgeDamageBonus => IsEndlessSurgeActive ? Mathf.Max(0f, _port.Tuning.EndlessSurgeDamageBonus) * ResolveEndlessSurgeIntensityMultiplier() : 0f;
        public float EndlessSurgeMoveSpeedBonus => IsEndlessSurgeActive ? Mathf.Max(0f, _port.Tuning.EndlessSurgeMoveSpeedBonus) * ResolveEndlessSurgeIntensityMultiplier() : 0f;
        public float EndlessSurgeCooldownMultiplierBonus => IsEndlessSurgeActive ? Mathf.Min(0f, _port.Tuning.EndlessSurgeCooldownMultiplierBonus) * ResolveEndlessSurgeIntensityMultiplier() : 0f;
        public float EndlessSurgePickupRangeBonus => IsEndlessSurgeActive ? Mathf.Max(0f, _port.Tuning.EndlessSurgePickupRangeBonus) * ResolveEndlessSurgeIntensityMultiplier() : 0f;
        public void Reset()
        {
            EndlessSurgeActivationCount = 0;
            EndlessSurgeTier = 0;
            EndlessSurgeExperienceGemDropCount = 0;
            EndlessSurgeBloodShardDropCount = 0;
            EndlessSurgePulseHitCount = 0;
            LastEndlessSurgeFeedbackLabel = string.Empty;
            _endlessSurgeTimer = 0f;
        }
        public void TryActivateEndlessSurge(SurvivorsEnemyRole role, Vector3 position, int baseExperienceReward)
        {
            if (!_runSession.HasClearedVictory || !(role == SurvivorsEnemyRole.Boss || role == SurvivorsEnemyRole.Miniboss || role == SurvivorsEnemyRole.Elite || role == SurvivorsEnemyRole.DreadElite))
            {
                return;
            }

            float duration = Mathf.Max(0.1f, _port.Tuning.EndlessSurgeDurationSeconds);
            _endlessSurgeTimer = Mathf.Max(_endlessSurgeTimer, duration);
            EndlessSurgeActivationCount++;
            EndlessSurgeTier = Mathf.Max(1, EndlessSurgeTier + 1);

            int threatTier = ResolveEndlessSurgeThreatTier(role);
            int gemCount = Mathf.Max(0, _port.Tuning.EndlessSurgeExperienceGemCount + threatTier - 1 + Mathf.Min(6, EndlessSurgeTier / 2));
            int xpPerGem = Mathf.Max(
                1,
                Mathf.RoundToInt(Mathf.Max(1, baseExperienceReward) * Mathf.Max(0.1f, _port.Tuning.EndlessSurgeExperienceMultiplier) * (0.35f + threatTier * 0.25f)));
            int spawnedExperience = 0;
            float rewardRadius = 1.15f + Mathf.Min(1.3f, gemCount * 0.08f);
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.21f) / Mathf.Max(1, gemCount)) * Mathf.PI * 2f;
                float laneOffset = 0.14f * (i % 3);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (rewardRadius + laneOffset);
                if (_port.SpawnPickup(SurvivorsPickupKind.Experience, position + offset, xpPerGem, false))
                {
                    spawnedExperience += xpPerGem;
                    EndlessSurgeExperienceGemDropCount++;
                }
            }

            int shardAmount = Mathf.Max(1, _port.Tuning.EndlessSurgeBloodShardAmount + threatTier - 1 + EndlessSurgeTier / 3);
            if (_port.SpawnPickup(SurvivorsPickupKind.BloodShard, position + new Vector3(-rewardRadius * 0.55f, 0f, rewardRadius * 0.38f), shardAmount, false))
            {
                EndlessSurgeBloodShardDropCount++;
            }

            int pulseHitCount = TriggerEndlessSurgePulse(position);
            string label = $"Endless Surge T{EndlessSurgeTier}: {ResolveEndlessSurgeThreatLabel(role)} cleared, +{spawnedExperience} XP, +{shardAmount} {_port.CurrencyLabel}, {pulseHitCount} hit";
            LastEndlessSurgeFeedbackLabel = label;
            _port.ShowFeedback(label, new Color(0.52f, 0.84f, 1f));
            _port.PlayPulse(position, Mathf.Clamp(32 + pulseHitCount * 5 + threatTier * 6, 42, 96), false, true);
        }

        private int TriggerEndlessSurgePulse(Vector3 position)
        {
            float radius = Mathf.Max(0f, _port.Tuning.EndlessSurgePulseRadius);
            float damage = Mathf.Max(0f, _port.Tuning.EndlessSurgePulseDamage * ResolveEndlessSurgeIntensityMultiplier());
            if (radius <= 0f || damage <= 0f)
            {
                return 0;
            }

            int hitCount = _port.DamageNonMajor(position, radius, damage, "survivors.endless.surge");

            EndlessSurgePulseHitCount += hitCount;
            return hitCount;
        }

        private float ResolveEndlessSurgeIntensityMultiplier()
        {
            return Mathf.Clamp(1f + Mathf.Max(0, EndlessSurgeTier - 1) * 0.16f, 1f, 2.25f);
        }

        private static int ResolveEndlessSurgeThreatTier(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return 3;
                case SurvivorsEnemyRole.Miniboss:
                case SurvivorsEnemyRole.DreadElite:
                    return 2;
                default:
                    return 1;
            }
        }

        private static string ResolveEndlessSurgeThreatLabel(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return "Endless Boss";
                case SurvivorsEnemyRole.Miniboss:
                    return "Endless Miniboss";
                case SurvivorsEnemyRole.DreadElite:
                    return "Endless Dread";
                default:
                    return "Endless Elite";
            }
        }

        public void TickEndlessSurge(float deltaTime)
        {
            if (_endlessSurgeTimer <= 0f)
            {
                return;
            }

            _endlessSurgeTimer = Mathf.Max(0f, _endlessSurgeTimer - Mathf.Max(0f, deltaTime));
        }
    }
}
