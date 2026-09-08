using System;
using Deucarian.Combat;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Observes resolved damage; popup resources and enemy health remain borrowed.</summary>
    internal sealed class SurvivorsDamageFeedback
    {
        private const float EnemyHitFlashSeconds = 0.13f;
        private readonly ISurvivorsDamageFeedbackPort _port;
        public SurvivorsDamageFeedback(ISurvivorsDamageFeedbackPort port) { _port = port; }
        public int EnemyHitFlashFeedbackCount { get; private set; }
        public int CriticalHitFeedbackCount { get; private set; }
        public int PlayerDamageFeedbackCount { get; private set; }
        public void ResetPlayerFeedback() { PlayerDamageFeedbackCount = 0; }
        public void ResetEnemyFeedback() { EnemyHitFlashFeedbackCount = 0; CriticalHitFeedbackCount = 0; }

        public void RecordEnemyDamageFeedback(ISurvivorsFeedbackEnemy enemy, DamageResult damage)
        {
            if (enemy == null)
            {
                return;
            }

            float resolvedDamage = ResolveDamagePopupAmount(damage);
            if (resolvedDamage <= 0f)
            {
                return;
            }

            RecordDamagePopup(enemy.Position, resolvedDamage, playerDamage: false, critical: damage.Critical.IsCritical);
            enemy.TriggerHitFlash(damage.Critical.IsCritical, EnemyHitFlashSeconds);
            EnemyHitFlashFeedbackCount++;
            if (damage.Critical.IsCritical)
            {
                CriticalHitFeedbackCount++;
            }

            _port.PlayCombatHitAudio();
            _port.TryEnrage(enemy);
        }

        public void RecordPlayerDamageFeedback(DamageResult damage, Vector3 position)
        {
            float resolvedDamage = ResolveDamagePopupAmount(damage);
            if (resolvedDamage <= 0f)
            {
                return;
            }

            PlayerDamageFeedbackCount++;
            RecordDamagePopup(position, resolvedDamage, playerDamage: true, critical: damage.Critical.IsCritical);
        }

        public void RecordDamagePopup(Vector3 worldPosition, float amount, bool playerDamage, bool critical)
        {
            _port.RecordPopup(worldPosition, amount, playerDamage, critical);
        }

        public static float ResolveDamagePopupAmount(DamageResult damage)
        {
            if (damage == null || !damage.Succeeded)
            {
                return 0f;
            }

            double amount = damage.HealthDamage > 0d ? damage.HealthDamage : damage.FinalDamage;
            if (amount <= 0d || double.IsNaN(amount) || double.IsInfinity(amount))
            {
                return 0f;
            }

            return (float)Math.Min(float.MaxValue, amount);
        }
    }
}
