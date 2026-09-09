using System;
using Deucarian.Combat;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns damage augmentation decisions and weapon-status diagnostics, with explicit effect ports.</summary>
    internal sealed class SurvivorsDamageAugments
    {
        private readonly ISurvivorsDamageAugmentPort _port;
        private const float FrostFanEnemyMoveSpeedMultiplier = 0.68f;
        private const float FrostFanEnemySlowDurationSeconds = 1.65f;
        private const float BlizzardCrownEnemyMoveSpeedMultiplier = 0.52f;
        private const float BlizzardCrownEnemySlowDurationSeconds = 2.35f;
        private const float CinderBurstBurnDamageRatio = 0.26f;
        private const float InfernoHeartBurnDamageRatio = 0.42f;
        private const float InfernoHeartBurnDurationMultiplier = 1.45f;

        public SurvivorsDamageAugments(ISurvivorsDamageAugmentPort port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
        }

        public int FrostFanSlowApplicationCount { get; private set; }
        public string LastFrostFanSlowFeedbackLabel { get; private set; } = string.Empty;
        public int CinderBurnApplicationCount { get; private set; }
        public string LastCinderBurnFeedbackLabel { get; private set; } = string.Empty;

        public void Reset()
        {
            FrostFanSlowApplicationCount = 0;
            LastFrostFanSlowFeedbackLabel = string.Empty;
            CinderBurnApplicationCount = 0;
            LastCinderBurnFeedbackLabel = string.Empty;
        }

        public void ApplyDamageAugmentsToEnemy(ISurvivorsDamageAugmentTarget enemy, DamageResult damage, string source)
        {
            if (enemy == null || damage == null || !enemy.IsAlive || !SurvivorsEnemyDamage.CanApplyDamageAugments(source))
            {
                return;
            }

            float dealt = Mathf.Max(0f, (float)damage.HealthDamage);
            if (dealt <= 0f)
            {
                return;
            }

            if (_port.Values.Lifesteal > 0f && _port.IsPlayerBound)
            {
                _port.HealPlayer(dealt * _port.Values.Lifesteal);
            }

            if (_port.Values.Barrier > 0f)
            {
                _port.RestoreBarrier(dealt * _port.Values.Barrier);
            }

            if (_port.Values.Poison > 0f)
            {
                enemy.ApplyDamageOverTime(
                    dealt * _port.Values.Poison,
                    _port.Tuning.StatusPoisonDurationSeconds,
                    "status.survivors.poison",
                    source);
            }

            if (_port.Values.Bleed > 0f)
            {
                enemy.ApplyDamageOverTime(
                    dealt * _port.Values.Bleed,
                    _port.Tuning.StatusBleedDurationSeconds,
                    "status.survivors.bleed",
                    source);
            }

            if (_port.Values.Execute > 0f && enemy.HealthFraction <= _port.Values.Execute)
            {
                enemy.ExecuteFromAugment(source);
            }
        }

        public void ApplyWeaponStatusEffectsToEnemy(ISurvivorsDamageAugmentTarget enemy, SurvivorsWeaponArchetypeDefinition definition, DamageResult damage)
        {
            if (enemy == null || definition == null || !enemy.IsAlive)
            {
                return;
            }

            if (string.Equals(definition.Id, BasicSurvivorsGame.FrostFanWeaponContentId, StringComparison.Ordinal))
            {
                bool evolved = _port.IsEvolutionActive(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId);
                float multiplier = evolved ? BlizzardCrownEnemyMoveSpeedMultiplier : FrostFanEnemyMoveSpeedMultiplier;
                float duration = evolved ? BlizzardCrownEnemySlowDurationSeconds : FrostFanEnemySlowDurationSeconds;
                if (!enemy.ApplyMovementSlow(multiplier, duration))
                {
                    return;
                }

                FrostFanSlowApplicationCount++;
                string sourceName = evolved
                    ? _port.ResolveUpgradeName(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId)
                    : _port.ResolveWeaponName(definition.Id);
                LastFrostFanSlowFeedbackLabel = $"{sourceName} chilled {enemy.DisplayName}";
            }

            if (!string.Equals(definition.Id, BasicSurvivorsGame.StarNovaWeaponContentId, StringComparison.Ordinal) || damage == null)
            {
                return;
            }

            float dealt = Mathf.Max(0f, (float)damage.HealthDamage);
            if (dealt <= 0f)
            {
                return;
            }

            bool inferno = _port.IsEvolutionActive(BasicSurvivorsGame.InfernoHeartEvolutionUpgradeId);
            float ratio = inferno ? InfernoHeartBurnDamageRatio : CinderBurstBurnDamageRatio;
            float durationMultiplier = inferno ? InfernoHeartBurnDurationMultiplier : 1f;
            enemy.ApplyDamageOverTime(
                dealt * ratio,
                _port.Tuning.StatusBurnDurationSeconds * durationMultiplier,
                "status.survivors.burn",
                definition.Id);
            CinderBurnApplicationCount++;
            LastCinderBurnFeedbackLabel = inferno
                ? $"Inferno Heart burned {enemy.DisplayName}"
                : $"Cinder Burst burned {enemy.DisplayName}";
        }

    }
}
