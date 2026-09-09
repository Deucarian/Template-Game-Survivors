using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal static class SurvivorsExplorationRules
    {
        public static SurvivorsEnemyRole ResolveAmbushRole(int index, int cacheNumber, int escalation)
        {
            int pressure = Mathf.Max(0, escalation);
            int seed = index + cacheNumber;
            if (pressure >= 6 && seed % 7 == 0)
            {
                return SurvivorsEnemyRole.Splitter;
            }

            if (pressure >= 3 && seed % 5 == 0)
            {
                return SurvivorsEnemyRole.Spitter;
            }

            return seed % 3 == 0 ? SurvivorsEnemyRole.Runner : SurvivorsEnemyRole.Swarm;
        }

        public static bool IsCadence(int cacheNumber, int cadence)
        {
            return cadence > 0 && cacheNumber > 0 && cacheNumber % cadence == 0;
        }
    }
}
