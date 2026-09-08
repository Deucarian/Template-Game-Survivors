using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Applies a pulse to a spatial snapshot, checking live actor eligibility again for every command.</summary>
    internal sealed class SurvivorsAreaDamage
    {
        private readonly SurvivorsEnemySpatialQueries _queries;
        private readonly Action<SurvivorsEnemyActor, float, string> _apply;
        public SurvivorsAreaDamage(SurvivorsEnemySpatialQueries queries, Action<SurvivorsEnemyActor, float, string> apply)
        { _queries = queries; _apply = apply; }
        public int DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source)
        {
            var targets = new List<SurvivorsEnemyActor>();
            _queries.CollectEnemiesWithinRadius(position, radius, targets);
            int count = 0;
            foreach (SurvivorsEnemyActor enemy in targets)
            {
                if (enemy == null || !enemy.IsAlive || SurvivorsEnemyRosterQueries.IsMajorRewardRole(enemy.Role)) continue;
                _apply(enemy, damage, source);
                count++;
            }
            return count;
        }
    }
}
