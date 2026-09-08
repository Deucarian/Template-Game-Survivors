using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns major threat enrage membership and slam/enrage transactions.</summary>
    internal sealed class SurvivorsMajorThreatAbilities
    {
        private readonly ISurvivorsMajorThreatAbilityPort _port;
        private readonly SurvivorsEnemySupportSpawning _support;
        private readonly HashSet<SurvivorsEnemyActor> _enragedMajorThreats = new HashSet<SurvivorsEnemyActor>();
        internal int MajorThreatEnrageCount { get; private set; }
        internal int MajorThreatEnrageSupportSpawnCount { get; private set; }
        internal string LastMajorThreatEnrageFeedbackLabel { get; private set; } = string.Empty;
        internal int MajorThreatSlamWarningCount { get; private set; }
        internal int MajorThreatSlamCastCount { get; private set; }
        internal int MajorThreatSlamHitCount { get; private set; }
        internal string LastMajorThreatSlamFeedbackLabel { get; private set; } = string.Empty;

        internal SurvivorsMajorThreatAbilities(ISurvivorsMajorThreatAbilityPort port, SurvivorsEnemySupportSpawning support)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            _support = support ?? throw new ArgumentNullException(nameof(support));
        }

        internal void ForgetEnemy(SurvivorsEnemyActor enemy) => _enragedMajorThreats.Remove(enemy);
        internal void ClearMembers() => _enragedMajorThreats.Clear();
        internal void ResetDiagnostics()
        {
            MajorThreatEnrageCount = 0;
            MajorThreatEnrageSupportSpawnCount = 0;
            LastMajorThreatEnrageFeedbackLabel = string.Empty;
            MajorThreatSlamWarningCount = 0;
            MajorThreatSlamCastCount = 0;
            MajorThreatSlamHitCount = 0;
            LastMajorThreatSlamFeedbackLabel = string.Empty;
        }

        internal static bool IsMajorThreatSlamRole(SurvivorsEnemyRole role)
        {
            return role == SurvivorsEnemyRole.DreadElite || role == SurvivorsEnemyRole.Miniboss || role == SurvivorsEnemyRole.Boss;
        }

        internal void RecordMajorThreatSlamTelegraph(SurvivorsEnemyActor enemy)
        {
            if (enemy == null)
            {
                return;
            }

            MajorThreatSlamWarningCount++;
            string name = string.IsNullOrWhiteSpace(enemy.DisplayName)
                ? _port.ResolveMajorThreatHealthFallbackLabel(enemy.Role)
                : enemy.DisplayName;
            LastMajorThreatSlamFeedbackLabel = $"{name} winding slam";
            _port.RecordStreakRewardFeedback(LastMajorThreatSlamFeedbackLabel, ResolveMajorThreatEnrageFeedbackColor(enemy.Role));
            float radius = Mathf.Max(0.5f, _port.Tuning.MajorThreatSlamRadius + enemy.Radius * 0.35f);
            _port.RecordMajorThreatSlamTelegraphEffect(enemy.transform.position, enemy.Role, radius, _port.Tuning.MajorThreatSlamTelegraphSeconds);
            _port.PlayBossFeedback(enemy.transform.position, enemy.Role == SurvivorsEnemyRole.Boss ? 42 : 30);
        }

        internal void ResolveMajorThreatSlam(SurvivorsEnemyActor enemy)
        {
            if (enemy == null || !enemy.IsAlive || _port.State != SurvivorsRunState.Playing)
            {
                return;
            }

            MajorThreatSlamCastCount++;
            float radius = Mathf.Max(0.5f, _port.Tuning.MajorThreatSlamRadius + enemy.Radius * 0.35f);
            float damage = Mathf.Max(0f, _port.Tuning.MajorThreatSlamDamage);
            float distance = Vector3.Distance(enemy.transform.position, _port.PlayerPosition);
            bool hit = damage > 0f && distance <= radius + _port.Tuning.PlayerRadius;
            float healthBefore = _port.CurrentHealth;
            float barrierBefore = _port.BarrierValue;
            if (hit)
            {
                _port.ApplyDamageToPlayer(damage, "combatant.survivors.enemy.slam." + enemy.InstanceId.Value);
            }

            bool damagedPlayer = _port.CurrentHealth < healthBefore || _port.BarrierValue < barrierBefore;
            if (damagedPlayer)
            {
                MajorThreatSlamHitCount++;
            }

            string name = string.IsNullOrWhiteSpace(enemy.DisplayName)
                ? _port.ResolveMajorThreatHealthFallbackLabel(enemy.Role)
                : enemy.DisplayName;
            LastMajorThreatSlamFeedbackLabel = damagedPlayer ? $"{name} slam hit" : $"{name} slam missed";
            _port.PlayBossFeedback(enemy.transform.position, enemy.Role == SurvivorsEnemyRole.Boss ? 58 : 40);
        }

        internal void TryTriggerMajorThreatEnrage(SurvivorsEnemyActor enemy)
        {
            if (enemy == null ||
                !enemy.IsAlive ||
                _port.State != SurvivorsRunState.Playing ||
                !_port.IsMajorRewardRole(enemy.Role) ||
                _enragedMajorThreats.Contains(enemy))
            {
                return;
            }

            float threshold = Mathf.Clamp01(_port.Tuning.MajorThreatEnrageHealthThreshold);
            if (threshold <= 0f || enemy.HealthFraction > threshold)
            {
                return;
            }

            _enragedMajorThreats.Add(enemy);
            int spawned = _support.SpawnMajorThreatEnrageSupport(enemy, MajorThreatEnrageCount);
            MajorThreatEnrageCount++;
            MajorThreatEnrageSupportSpawnCount += spawned;

            string name = string.IsNullOrWhiteSpace(enemy.DisplayName)
                ? _port.ResolveMajorThreatHealthFallbackLabel(enemy.Role)
                : enemy.DisplayName;
            LastMajorThreatEnrageFeedbackLabel = $"{name} enraged: +{spawned} support";
            _port.RecordStreakRewardFeedback(LastMajorThreatEnrageFeedbackLabel, ResolveMajorThreatEnrageFeedbackColor(enemy.Role));
            _port.PlayBossFeedback(enemy.transform.position, enemy.Role == SurvivorsEnemyRole.Boss ? 64 : 42);
        }

        internal static Color ResolveMajorThreatEnrageFeedbackColor(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return new Color(1f, 0.2f, 0.3f);
                case SurvivorsEnemyRole.Miniboss:
                    return new Color(0.95f, 0.36f, 1f);
                case SurvivorsEnemyRole.DreadElite:
                    return new Color(0.35f, 0.85f, 1f);
                default:
                    return new Color(1f, 0.68f, 0.2f);
            }
        }
    }
}
