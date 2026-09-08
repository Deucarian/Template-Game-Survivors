using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsPickupAttractionValues
    {
        public readonly float Range, Speed, CollectRadius;
        public SurvivorsPickupAttractionValues(float range, float speed, float collectRadius) { Range = range; Speed = speed; CollectRadius = collectRadius; }
    }
    /// <summary>Ticks the existing live actor collections in reverse order and removes inactive members.</summary>
    internal sealed class SurvivorsActorSimulation
    {
        private readonly IList<SurvivorsEnemyActor> _enemies;
        private readonly IList<SurvivorsProjectileActor> _projectiles;
        private readonly IList<SurvivorsPickupActor> _pickups;
        private readonly Action<SurvivorsEnemyActor, float> _leash;
        private readonly Func<SurvivorsPickupAttractionValues> _pickupValues;
        public SurvivorsActorSimulation(IList<SurvivorsEnemyActor> enemies, IList<SurvivorsProjectileActor> projectiles,
            IList<SurvivorsPickupActor> pickups, Action<SurvivorsEnemyActor, float> leash, Func<SurvivorsPickupAttractionValues> pickupValues)
        { _enemies = enemies; _projectiles = projectiles; _pickups = pickups; _leash = leash; _pickupValues = pickupValues; }
        public void TickEnemies(float deltaTime)
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                SurvivorsEnemyActor enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    _enemies.RemoveAt(i);
                    continue;
                }

                _leash(enemy, deltaTime);
                enemy.Simulate(deltaTime);
            }
        }

        public void TickProjectiles(float deltaTime)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                SurvivorsProjectileActor projectile = _projectiles[i];
                if (projectile == null || !projectile.IsActive)
                {
                    _projectiles.RemoveAt(i);
                    continue;
                }

                projectile.Simulate(deltaTime);
            }
        }

        public void TickPickups(float deltaTime)
        {
            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                SurvivorsPickupActor pickup = _pickups[i];
                if (pickup == null || !pickup.IsActive)
                {
                    _pickups.RemoveAt(i);
                    continue;
                }

                SurvivorsPickupAttractionValues values = _pickupValues();
                pickup.UpdateAttractionSettings(values.Range, values.Speed, values.CollectRadius);
                pickup.Simulate(deltaTime);
            }
        }
    }
}
