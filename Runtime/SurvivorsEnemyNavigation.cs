using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns leash/reentry policy and diagnostics; actor timers remain on their existing actors.</summary>
    internal sealed class SurvivorsEnemyNavigation
    {
        private readonly ISurvivorsEnemyNavigationPort _port;
        internal SurvivorsEnemyNavigation(ISurvivorsEnemyNavigationPort port)
            => _port = port ?? throw new ArgumentNullException(nameof(port));

        internal int NormalEnemyRecycleCount { get; private set; }
        internal int MajorThreatRepositionCount { get; private set; }

        internal void ResetDiagnostics()
        {
            NormalEnemyRecycleCount = 0;
            MajorThreatRepositionCount = 0;
        }

        internal void TryUpdateEnemyLeash(SurvivorsEnemyActor enemy, float deltaTime)
        {
            if (enemy == null || !enemy.IsAlive || !enemy.CanLeash)
            {
                return;
            }

            Vector3 playerPosition = _port.PlayerPosition;
            Vector3 offset = enemy.transform.position - playerPosition;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (_port.IsMajorRewardRole(enemy.Role))
            {
                TryUpdateMajorThreatLeash(enemy, playerPosition, distance, deltaTime);
            }
            else
            {
                TryUpdateNormalEnemyLeash(enemy, playerPosition, distance, deltaTime);
            }
        }

        private void TryUpdateNormalEnemyLeash(SurvivorsEnemyActor enemy, Vector3 playerPosition, float distance, float deltaTime)
        {
            if (!enemy.CanRecycle)
            {
                return;
            }

            float softRadius = Mathf.Max(0f, _port.Tuning.EnemySoftLeashRadius);
            float hardRadius = Mathf.Max(softRadius + 0.1f, _port.Tuning.EnemyHardRecycleRadius);
            if (softRadius <= 0f || distance <= softRadius)
            {
                enemy.ResetLeashTimer();
                return;
            }

            enemy.AddLeashTime(deltaTime);
            if (distance < hardRadius ||
                enemy.LeashTimerSeconds < Mathf.Max(0f, _port.Tuning.EnemyRecycleDelaySeconds) ||
                enemy.HealthFraction <= 0.15f)
            {
                return;
            }

            enemy.transform.position = _port.ResolveSafeOffscreenPosition(
                playerPosition,
                Mathf.Max(_port.Tuning.EnemyRecycleMinimumRespawnDistance, _port.Tuning.PlayerRadius + enemy.Radius + 1.8f),
                Mathf.Max(_port.Tuning.EnemyRecycleMaximumRespawnDistance, _port.Tuning.EnemyRecycleMinimumRespawnDistance + 1f),
                enemy.InstanceId.Value + NormalEnemyRecycleCount + 17,
                _port.ResolveOffscreenSpawnPadding(enemy.Role, "normal-recycle"),
                _port.Tuning.SpawnBandDepth);
            _port.RecordGameplaySpawnSafety(enemy.Role, enemy.transform.position, "normal-recycle");
            enemy.ResetLeashTimer();
            NormalEnemyRecycleCount++;
        }

        private void TryUpdateMajorThreatLeash(SurvivorsEnemyActor enemy, Vector3 playerPosition, float distance, float deltaTime)
        {
            if (!enemy.CanReposition)
            {
                return;
            }

            float repositionRadius = Mathf.Max(0f, _port.Tuning.MajorThreatRepositionRadius);
            if (repositionRadius <= 0f || distance <= repositionRadius)
            {
                enemy.ResetLeashTimer();
                return;
            }

            enemy.AddLeashTime(deltaTime);
            if (enemy.LeashTimerSeconds < Mathf.Max(0f, _port.Tuning.MajorThreatRepositionDelaySeconds))
            {
                return;
            }

            float minimum = Mathf.Max(_port.Tuning.MajorThreatCatchUpRadius, _port.Tuning.PlayerRadius + enemy.Radius + 3.2f);
            float maximum = Mathf.Max(minimum + 1.5f, minimum + 4.5f);
            enemy.transform.position = _port.ResolveSafeOffscreenPosition(
                playerPosition,
                minimum,
                maximum,
                enemy.InstanceId.Value + MajorThreatRepositionCount + 43,
                _port.ResolveOffscreenSpawnPadding(enemy.Role, "major-threat-reentry"),
                _port.Tuning.SpawnBandDepth);
            _port.RecordGameplaySpawnSafety(enemy.Role, enemy.transform.position, "major-threat-reentry");
            enemy.ResetLeashTimer();
            MajorThreatRepositionCount++;
            _port.RecordMajorThreatReentry(enemy.Role, enemy.DisplayName, enemy.transform.position - playerPosition);
        }

        internal float ResolveEnemyCatchUpMoveSpeedMultiplier(SurvivorsEnemyActor enemy, float distance)
        {
            if (enemy == null || !enemy.IsAlive || !_port.IsMajorRewardRole(enemy.Role))
            {
                return 1f;
            }

            float catchUpRadius = Mathf.Max(0f, _port.Tuning.MajorThreatCatchUpRadius);
            if (catchUpRadius <= 0f || distance <= catchUpRadius)
            {
                return 1f;
            }

            return Mathf.Max(1f, _port.Tuning.MajorThreatCatchUpSpeedMultiplier);
        }
    }
}
