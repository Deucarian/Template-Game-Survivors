using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Queries the live enemy list without caching membership; owns spatial selection and crowd geometry.</summary>
    internal sealed class SurvivorsEnemySpatialQueries
    {
        private readonly IReadOnlyList<SurvivorsEnemyActor> _enemies;
        private readonly Func<SurvivorsTemplateTuning> _tuning;
        private SurvivorsTemplateTuning CurrentTuning => _tuning();

        internal SurvivorsEnemySpatialQueries(IReadOnlyList<SurvivorsEnemyActor> enemies, Func<SurvivorsTemplateTuning> tuning)
        {
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
        }

        internal SurvivorsEnemyActor FindNearestEnemy(Vector3 origin, float range)
        {
            SurvivorsEnemyActor best = null;
            float bestDistance = range * range;
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                float distance = (enemy.transform.position - origin).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }

            return best;
        }

        internal void CollectEnemiesWithinRadius(Vector3 origin, float radius, List<SurvivorsEnemyActor> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            float radiusSquared = radius * radius;
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                if ((enemy.transform.position - origin).sqrMagnitude <= radiusSquared)
                {
                    results.Add(enemy);
                }
            }
        }

        internal Vector3 ResolveEnemyCrowdSeparation(SurvivorsEnemyActor actor)
        {
            if (actor == null || _enemies.Count <= 1)
            {
                return Vector3.zero;
            }

            float strength = Mathf.Max(0f, CurrentTuning.EnemySeparationStrength);
            float baseRadius = Mathf.Max(0f, CurrentTuning.EnemySeparationRadius);
            if (strength <= 0f || baseRadius <= 0f)
            {
                return Vector3.zero;
            }

            Vector3 origin = actor.transform.position;
            Vector3 push = Vector3.zero;
            int neighborCount = 0;
            int maxNeighbors = Mathf.Max(1, CurrentTuning.EnemySeparationMaxNeighbors);
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor other = _enemies[i];
                if (other == null || other == actor || !other.IsAlive)
                {
                    continue;
                }

                float radius = Mathf.Max(baseRadius, actor.Radius + other.Radius);
                Vector3 away = origin - other.transform.position;
                away.y = 0f;
                float sqrDistance = away.sqrMagnitude;
                if (sqrDistance > radius * radius)
                {
                    continue;
                }

                float distance = 0f;
                if (sqrDistance <= 0.0001f)
                {
                    away = ResolveDeterministicSeparationDirection(actor.GetInstanceID(), other.GetInstanceID());
                }
                else
                {
                    distance = Mathf.Sqrt(sqrDistance);
                    away /= distance;
                }

                float weight = radius <= 0f ? 0f : 1f - Mathf.Clamp01(distance / radius);
                push += away * weight;
                neighborCount++;
                if (neighborCount >= maxNeighbors)
                {
                    break;
                }
            }

            if (push.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            return push.normalized * Mathf.Min(strength, push.magnitude * strength);
        }

        internal static Vector3 ResolveDeterministicSeparationDirection(int firstInstanceId, int secondInstanceId)
        {
            int hash = firstInstanceId ^ (secondInstanceId << 1);
            float angle = ((hash & 0x7fffffff) % 360) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        }
    }
}
