using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsProjectileLaunchValues
    {
        public SurvivorsProjectileLaunchValues(Vector3 direction, float speed, float damage, float radius,
            float lifetime, int chains, int pierces, int forks, int returns, HashSet<int> ignoredEnemyIds)
        {
            Direction = direction; Speed = speed; Damage = damage; Radius = radius; Lifetime = lifetime;
            Chains = chains; Pierces = pierces; Forks = forks; Returns = returns; IgnoredEnemyIds = ignoredEnemyIds;
        }
        public Vector3 Direction { get; }
        public float Speed { get; }
        public float Damage { get; }
        public float Radius { get; }
        public float Lifetime { get; }
        public int Chains { get; }
        public int Pierces { get; }
        public int Forks { get; }
        public int Returns { get; }
        public HashSet<int> IgnoredEnemyIds { get; }
    }
}
