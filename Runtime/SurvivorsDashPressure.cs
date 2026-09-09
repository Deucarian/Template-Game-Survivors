using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.Combat;

namespace Deucarian.TemplateGameSurvivors
{
    internal static class SurvivorsDashPressure
    {
        public static int Apply(SurvivorsTemplateTuning tuning, IReadOnlyList<SurvivorsEnemyActor> activeEnemies, Vector3 start, Vector3 end, Vector3 dashDirection, Action onDamageHit)
        {
            float pressureRadius = Mathf.Max(0f, tuning.DashKnockbackRadius);
            float knockbackDistance = Mathf.Max(0f, tuning.DashKnockbackDistance);
            float damage = Mathf.Max(0f, tuning.DashDamage);
            if ((pressureRadius <= 0f && damage <= 0f) || activeEnemies.Count == 0)
            {
                return 0;
            }

            int impacted = 0;
            var enemies = new List<SurvivorsEnemyActor>(activeEnemies);
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                Vector3 enemyPosition = enemy.transform.position;
                Vector3 closest = ClosestPointOnSegment(start, end, enemyPosition);
                float allowedDistance = pressureRadius + enemy.Radius;
                if ((enemyPosition - closest).sqrMagnitude > allowedDistance * allowedDistance)
                {
                    continue;
                }

                impacted++;
                if (damage > 0f && enemy.ApplyDamage(damage, "survivors.player.arc-step") != null)
                {
                    onDamageHit();
                }

                if (!enemy.IsAlive || knockbackDistance <= 0f)
                {
                    continue;
                }

                Vector3 away = enemyPosition - closest;
                away.y = 0f;
                if (away.sqrMagnitude <= 0.0001f)
                {
                    away = dashDirection;
                }

                enemy.transform.position += away.normalized * knockbackDistance;
                enemy.transform.forward = away.normalized;
            }

            return impacted;
        }

        public static Vector3 ClosestPointOnSegment(Vector3 start, Vector3 end, Vector3 point)
        {
            Vector3 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= 0.0001f)
            {
                return start;
            }

            float t = Vector3.Dot(point - start, segment) / lengthSquared;
            return start + segment * Mathf.Clamp01(t);
        }
    }
}
