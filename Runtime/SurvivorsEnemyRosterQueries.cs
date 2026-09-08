using System;
using System.Collections.Generic;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Role classification and live roster observations over existing enemy membership.</summary>
    internal sealed class SurvivorsEnemyRosterQueries
    {
        private readonly IReadOnlyList<SurvivorsEnemyActor> _enemies;
        internal SurvivorsEnemyRosterQueries(IReadOnlyList<SurvivorsEnemyActor> enemies)
            => _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));

        internal int CountEnemiesByRole(SurvivorsEnemyRole role)
        {
            int count = 0;
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy != null && enemy.IsAlive && enemy.Role == role)
                {
                    count++;
                }
            }

            return count;
        }

        internal int CountEliteEnemies()
        {
            int count = 0;
            for (int i = 0; i < _enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy != null && enemy.IsAlive && IsEliteRole(enemy.Role))
                {
                    count++;
                }
            }

            return count;
        }

        internal static bool IsEliteRole(SurvivorsEnemyRole role)
        {
            return role == SurvivorsEnemyRole.Elite || role == SurvivorsEnemyRole.DreadElite;
        }

        internal static bool IsMajorRewardRole(SurvivorsEnemyRole role)
        {
            return IsEliteRole(role) || role == SurvivorsEnemyRole.Miniboss || role == SurvivorsEnemyRole.Boss;
        }

        internal static SurvivorsEnemyRole ResolveDebugMajorEnemyRole(SurvivorsEnemyRole role)
        {
            if (IsEliteRole(role) || role == SurvivorsEnemyRole.Miniboss || role == SurvivorsEnemyRole.Boss)
            {
                return role;
            }

            return SurvivorsEnemyRole.Elite;
        }
    }
}
