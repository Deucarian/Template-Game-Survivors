using System;
using Deucarian.Combat;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns dodge reward observations and success-only drop counts, without owning actors.</summary>
    internal sealed class SurvivorsRangedDodgeRewards
    {
        private readonly ISurvivorsRangedDodgePort _port;
        public SurvivorsRangedDodgeRewards(ISurvivorsRangedDodgePort port) { _port = port; }
        public int EnemyRangedAttackDodgeFeedbackCount { get; private set; }
        public int EnemyRangedAttackDodgeExperienceGemDropCount { get; private set; }
        public string LastEnemyRangedAttackDodgeFeedbackLabel { get; private set; } = string.Empty;
        public void Reset()
        {
            EnemyRangedAttackDodgeFeedbackCount = 0;
            EnemyRangedAttackDodgeExperienceGemDropCount = 0;
            LastEnemyRangedAttackDodgeFeedbackLabel = string.Empty;
        }

        public void RecordEnemyRangedAttackDodgeFeedback(ISurvivorsFeedbackEnemy enemy)
        {
            if (enemy == null)
            {
                return;
            }

            int experienceReward = Mathf.Max(0, _port.Tuning.EnemyRangedAttackDodgeExperienceReward);
            bool spawnedExperience = false;
            if (experienceReward > 0)
            {
                Vector3 rewardPosition = ResolveRangedDodgeRewardPosition(enemy);
                spawnedExperience = _port.TrySpawnExperience(rewardPosition, experienceReward);
                if (spawnedExperience)
                {
                    EnemyRangedAttackDodgeExperienceGemDropCount++;
                }
            }

            string name = string.IsNullOrWhiteSpace(enemy.DisplayName)
                ? ResolveRangedDodgeFallbackLabel(enemy.Role)
                : enemy.DisplayName;
            LastEnemyRangedAttackDodgeFeedbackLabel = spawnedExperience
                ? $"{name} shot dodged: +{experienceReward} XP"
                : $"{name} shot dodged";
            EnemyRangedAttackDodgeFeedbackCount++;
            _port.ShowStreakFeedback(LastEnemyRangedAttackDodgeFeedbackLabel, new Color(0.52f, 0.95f, 1f));
            _port.PlayDodgePulse(_port.PlayerPosition, spawnedExperience ? 14 : 8);
        }

        public Vector3 ResolveRangedDodgeRewardPosition(ISurvivorsFeedbackEnemy enemy)
        {
            Vector3 player = _port.PlayerPosition;
            Vector3 away = enemy == null ? Vector3.forward : player - enemy.Position;
            away.y = 0f;
            if (away.sqrMagnitude <= 0.001f)
            {
                away = Vector3.forward;
            }

            float offset = Mathf.Max(_port.Tuning.PickupCollectRadius + 0.35f, 0.75f);
            return player + away.normalized * offset;
        }

        public static string ResolveRangedDodgeFallbackLabel(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Spitter:
                    return "Spitter";
                case SurvivorsEnemyRole.Elite:
                case SurvivorsEnemyRole.DreadElite:
                    return "Elite";
                case SurvivorsEnemyRole.Miniboss:
                    return "Miniboss";
                case SurvivorsEnemyRole.Boss:
                    return "Boss";
                default:
                    return "Ranged shot";
            }
        }
    }
}
