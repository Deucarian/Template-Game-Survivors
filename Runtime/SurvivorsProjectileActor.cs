using System;
using System.Collections.Generic;
using Deucarian.Common;
using Deucarian.Combat;
using Deucarian.GameplayFoundation;
using Deucarian.Persistence;
using Deucarian.Persistence.Unity;
using Deucarian.Projectiles;
using Deucarian.RunUpgrades;
using Deucarian.WeaponSystems;
using Deucarian.WorldSpawning;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    public sealed class SurvivorsProjectileActor : MonoBehaviour, IWorldSpawnedObject, IWorldSpawnResettable
    {
        private readonly HashSet<int> _hitEnemyIds = new HashSet<int>();
        private readonly List<SurvivorsEnemyActor> _forkTargets = new List<SurvivorsEnemyActor>();
        private SurvivorsTemplateController _controller;
        private SurvivorsWeaponArchetypeDefinition _definition;
        private Vector3 _direction;
        private float _speed;
        private float _damage;
        private float _radius;
        private float _lifetime;
        private int _remainingChains;
        private int _remainingPierces;
        private int _remainingForks;
        private int _remainingReturns;
        private bool _returningToPlayer;
        private Renderer _renderer;
        private TrailRenderer _trail;
        private Material _runtimeMaterial;

        public SpawnInstanceId InstanceId { get; private set; }
        public bool IsActive { get; private set; }

        public void Initialize(
            SurvivorsTemplateController controller,
            SurvivorsWeaponArchetypeDefinition definition,
            Vector3 direction,
            float speed,
            float damage,
            float radius,
            float lifetime,
            int remainingChains,
            int remainingPierces,
            int remainingForks,
            int remainingReturns,
            HashSet<int> ignoredEnemyIds = null)
        {
            _controller = controller;
            _definition = definition;
            _direction = direction.sqrMagnitude <= 0.001f ? Vector3.forward : direction.normalized;
            _speed = Mathf.Max(0f, speed);
            _damage = Mathf.Max(0f, damage);
            _radius = Mathf.Max(0.05f, radius);
            _lifetime = Mathf.Max(0.05f, lifetime);
            _remainingChains = Mathf.Max(0, remainingChains);
            _remainingPierces = Mathf.Max(0, remainingPierces);
            _remainingForks = Mathf.Max(0, remainingForks);
            _remainingReturns = Mathf.Max(0, remainingReturns);
            _returningToPlayer = false;
            ApplyPresentation(definition == null
                ? controller == null ? new Color(0.78f, 0.42f, 1f) : controller.ResolveProjectileFallbackColor()
                : definition.Tint);
            _hitEnemyIds.Clear();
            if (ignoredEnemyIds != null)
            {
                foreach (int ignoredId in ignoredEnemyIds)
                {
                    _hitEnemyIds.Add(ignoredId);
                }
            }

            IsActive = true;
            transform.localScale = Vector3.one * (_radius * 2f);
        }

        public void Simulate(float deltaTime)
        {
            if (!IsActive || _controller == null)
            {
                return;
            }

            _lifetime -= deltaTime;
            if (_lifetime <= 0f)
            {
                if (!TryStartReturnToPlayer())
                {
                    Release(DespawnReason.OutOfBounds);
                }

                return;
            }

            if (_returningToPlayer)
            {
                Vector3 toPlayer = _controller.PlayerPosition + Vector3.up * 0.4f - transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude <= 0.18f)
                {
                    Release(DespawnReason.Completed);
                    return;
                }

                if (toPlayer.sqrMagnitude > 0.001f)
                {
                    _direction = toPlayer.normalized;
                }
            }

            Vector3 previousPosition = transform.position;
            Vector3 currentPosition = previousPosition + _direction * (_speed * deltaTime);
            transform.position = currentPosition;
            if (TryFindSegmentHit(previousPosition, currentPosition, out SurvivorsEnemyActor hitEnemy, out float hitRange, out Vector3 hitPosition))
            {
                transform.position = hitPosition;
                int enemyId = hitEnemy.GetInstanceID();
                _hitEnemyIds.Add(enemyId);
                DamageResult damage = hitEnemy.ApplyDamage(_damage, _definition == null ? "combatant.survivors.player" : _definition.Id);
                _controller.ApplyWeaponStatusEffectsToEnemy(hitEnemy, _definition, damage);
                SpawnForkProjectiles(hitEnemy);
                if (_remainingPierces > 0)
                {
                    _remainingPierces--;
                    _controller.RecordProjectilePierceHit();
                    transform.position += _direction * Mathf.Max(0.08f, hitRange);
                    return;
                }

                if (_remainingChains > 0 && TryRetargetFrom(hitEnemy))
                {
                    _remainingChains--;
                    _controller.RecordProjectileChainHit();
                    return;
                }

                if (!TryStartReturnToPlayer())
                {
                    Release(DespawnReason.Completed);
                }

                return;
            }
        }

        private bool TryFindSegmentHit(Vector3 start, Vector3 end, out SurvivorsEnemyActor hitEnemy, out float hitRange, out Vector3 hitPosition)
        {
            hitEnemy = null;
            hitRange = 0f;
            hitPosition = end;
            if (_controller == null)
            {
                return false;
            }

            IReadOnlyList<SurvivorsEnemyActor> enemies = _controller.ActiveEnemies;
            Vector3 segment = end - start;
            segment.y = 0f;
            float segmentLengthSquared = segment.sqrMagnitude;
            float bestSegmentT = float.MaxValue;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                int enemyId = enemy.GetInstanceID();
                if (_hitEnemyIds.Contains(enemyId))
                {
                    continue;
                }

                float candidateHitRange = _radius + enemy.Radius;
                Vector3 enemyPosition = enemy.transform.position;
                Vector3 closestPoint = end;
                float segmentT = 1f;
                if (segmentLengthSquared > 0.0001f)
                {
                    Vector3 enemyOffset = enemyPosition - start;
                    enemyOffset.y = 0f;
                    segmentT = Mathf.Clamp01(Vector3.Dot(enemyOffset, segment) / segmentLengthSquared);
                    closestPoint = start + segment * segmentT;
                }

                Vector3 missOffset = enemyPosition - closestPoint;
                missOffset.y = 0f;
                if (missOffset.sqrMagnitude > candidateHitRange * candidateHitRange || segmentT >= bestSegmentT)
                {
                    continue;
                }

                bestSegmentT = segmentT;
                hitEnemy = enemy;
                hitRange = candidateHitRange;
                hitPosition = closestPoint;
            }

            return hitEnemy != null;
        }

        private void SpawnForkProjectiles(SurvivorsEnemyActor lastHitEnemy)
        {
            if (_controller == null || _definition == null || _remainingForks <= 0)
            {
                return;
            }

            _forkTargets.Clear();
            IReadOnlyList<SurvivorsEnemyActor> enemies = _controller.ActiveEnemies;
            Vector3 origin = transform.position;
            const float forkSearchRange = 5f;
            float forkSearchRangeSquared = forkSearchRange * forkSearchRange;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive || _hitEnemyIds.Contains(enemy.GetInstanceID()))
                {
                    continue;
                }

                Vector3 offset = enemy.transform.position - origin;
                offset.y = 0f;
                if (offset.sqrMagnitude <= forkSearchRangeSquared)
                {
                    _forkTargets.Add(enemy);
                }
            }

            _forkTargets.Sort((left, right) =>
            {
                float leftDistance = (left.transform.position - origin).sqrMagnitude;
                float rightDistance = (right.transform.position - origin).sqrMagnitude;
                return leftDistance.CompareTo(rightDistance);
            });

            int forkCount = Mathf.Min(_remainingForks, Mathf.Max(1, _forkTargets.Count));
            _remainingForks = Mathf.Max(0, _remainingForks - 1);
            for (int i = 0; i < forkCount; i++)
            {
                Vector3 forkDirection;
                if (i < _forkTargets.Count && _forkTargets[i] != null)
                {
                    forkDirection = _forkTargets[i].transform.position - origin;
                    forkDirection.y = 0f;
                }
                else
                {
                    float angle = (-18f + i * 36f) * Mathf.Deg2Rad;
                    forkDirection = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f) * _direction;
                }

                if (forkDirection.sqrMagnitude <= 0.001f)
                {
                    forkDirection = _direction;
                }

                if (_controller.LaunchProjectileFrom(
                    _definition,
                    origin,
                    forkDirection.normalized,
                    _remainingChains,
                    _remainingPierces,
                    _remainingForks,
                    _remainingReturns,
                    _hitEnemyIds))
                {
                    _controller.RecordProjectileForkSpawn();
                }
            }

            _forkTargets.Clear();
        }

        private bool TryRetargetFrom(SurvivorsEnemyActor lastHitEnemy)
        {
            if (_controller == null)
            {
                return false;
            }

            SurvivorsEnemyActor best = null;
            float bestDistance = 5f * 5f;
            IReadOnlyList<SurvivorsEnemyActor> enemies = _controller.ActiveEnemies;
            Vector3 origin = lastHitEnemy == null ? transform.position : lastHitEnemy.transform.position;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive || _hitEnemyIds.Contains(enemy.GetInstanceID()))
                {
                    continue;
                }

                Vector3 offset = enemy.transform.position - origin;
                offset.y = 0f;
                float distance = offset.sqrMagnitude;
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }

            if (best == null)
            {
                return false;
            }

            Vector3 direction = best.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return false;
            }

            _direction = direction.normalized;
            _lifetime = Mathf.Max(_lifetime, 0.7f);
            return true;
        }

        private bool TryStartReturnToPlayer()
        {
            if (_returningToPlayer || _controller == null || _remainingReturns <= 0)
            {
                return false;
            }

            Vector3 direction = _controller.PlayerPosition + Vector3.up * 0.4f - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return false;
            }

            _remainingReturns--;
            _returningToPlayer = true;
            _direction = direction.normalized;
            _speed *= 1.2f;
            _lifetime = Mathf.Max(_lifetime, 1.25f);
            _controller.RecordProjectileReturnStart();
            return true;
        }

        private void Release(DespawnReason reason)
        {
            IsActive = false;
            if (_controller != null)
            {
                _controller.ReleaseProjectile(this, reason);
            }
        }

        public void OnWorldSpawned(WorldSpawnContext context)
        {
            InstanceId = context.InstanceId;
        }

        public void OnWorldDespawned(DespawnReason reason)
        {
            _controller = null;
            _definition = null;
            _hitEnemyIds.Clear();
            _forkTargets.Clear();
            IsActive = false;
        }

        public void ResetForWorldSpawn()
        {
            _controller = null;
            _definition = null;
            _hitEnemyIds.Clear();
            _forkTargets.Clear();
            _remainingChains = 0;
            _remainingPierces = 0;
            _remainingForks = 0;
            _remainingReturns = 0;
            _returningToPlayer = false;
            IsActive = false;
            InstanceId = default;
        }

        private void OnDestroy()
        {
            UnityObjectUtility.DestroySafely(_runtimeMaterial);
            _runtimeMaterial = null;
        }

        private void ApplyPresentation(Color tint)
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<Renderer>();
            }

            if (_renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (_runtimeMaterial == null || _runtimeMaterial.shader != shader)
                {
                    UnityObjectUtility.DestroySafely(_runtimeMaterial);
                    _runtimeMaterial = new Material(shader);
                }

                _runtimeMaterial.color = tint;
                _renderer.sharedMaterial = _runtimeMaterial;
            }

            if (_trail == null)
            {
                _trail = GetComponent<TrailRenderer>();
            }

            if (_trail != null)
            {
                Color trailStart = tint;
                trailStart.a = 0.95f;
                _trail.startColor = trailStart;
                _trail.endColor = _controller == null
                    ? new Color(0.18f, 0.86f, 1f, 0f)
                    : _controller.ResolveProjectileTrailEndColor();
            }
        }
    }
}
