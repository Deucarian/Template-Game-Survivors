using System;
using System.Collections.Generic;
using Deucarian.Combat;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal interface ISurvivorsDeathNovaTarget
    {
        bool IsAlive { get; }
        Vector3 Position { get; }
        float Radius { get; }
        DamageResult ApplyDamage(float amount, string source);
    }
    /// <summary>Owns death-nova target capture and ordered application; live targets may disappear during the pulse.</summary>
    internal sealed class SurvivorsDeathNova
    {
        private readonly IReadOnlyList<ISurvivorsDeathNovaTarget> _enemies;
        private readonly Func<float> _damage, _radius;
        private readonly Action<Vector3, int> _feedback;
        public SurvivorsDeathNova(IReadOnlyList<ISurvivorsDeathNovaTarget> enemies, Func<float> damage, Func<float> radius, Action<Vector3, int> feedback)
        { _enemies = enemies; _damage = damage; _radius = radius; _feedback = feedback; }
        public int DeathNovaTriggerCount { get; private set; }
        public int DeathNovaHitCount { get; private set; }
        public void Reset() { DeathNovaTriggerCount = 0; DeathNovaHitCount = 0; }
        public void TryTriggerDeathNova(Vector3 position, string source, bool applyAugments)
        {
            float damage = _damage();
            float radius = _radius();
            if (damage <= 0f || radius <= 0f || !applyAugments || !SurvivorsEnemyDamage.CanApplyDamageAugments(source))
            {
                return;
            }

            var targets = new List<ISurvivorsDeathNovaTarget>();
            for (int i = 0; i < _enemies.Count; i++)
            {
                ISurvivorsDeathNovaTarget target = _enemies[i];
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                Vector3 delta = target.Position - position;
                delta.y = 0f;
                float range = radius + Mathf.Max(0f, target.Radius);
                if (delta.sqrMagnitude <= range * range)
                {
                    targets.Add(target);
                }
            }

            if (targets.Count == 0)
            {
                return;
            }

            DeathNovaTriggerCount++;
            _feedback(position, Mathf.Clamp(16 + targets.Count * 7, 18, 72));
            for (int i = 0; i < targets.Count; i++)
            {
                ISurvivorsDeathNovaTarget target = targets[i];
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                DamageResult result = target.ApplyDamage(damage, "survivors.augment.death-nova");
                if (result != null && result.HealthDamage > 0d)
                {
                    DeathNovaHitCount++;
                }
            }
        }
    }
}
