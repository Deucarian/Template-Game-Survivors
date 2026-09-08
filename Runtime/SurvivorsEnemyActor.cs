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
    public sealed class SurvivorsEnemyActor : MonoBehaviour, IWorldSpawnedObject, IWorldSpawnResettable
    {
        private SurvivorsTemplateController _controller;
        private SurvivorsEnemyPresentation _presentation;
        private SurvivorsEnemyPresentation Presentation => _presentation ??
            (_presentation = new SurvivorsEnemyPresentation(transform, () => IsMovementSlowed, () => IsBurning));
        private SurvivorsEnemyStatusEffects _statusEffects;
        private SurvivorsEnemyStatusEffects StatusEffects => _statusEffects ??
            (_statusEffects = new SurvivorsEnemyStatusEffects(() => IsAlive,
                (amount, source) => ApplyDamageInternal(amount, source, applyAugments: false),
                () => Presentation.Refresh()));
        private HealthState _health;
        private float _moveSpeed;
        private float _contactDamage;
        private float _contactInterval;
        private float _contactCooldown;
        private float _rangedAttackRange;
        private float _rangedAttackDamage;
        private float _rangedAttackInterval;
        private float _rangedAttackWindupSeconds;
        private float _preferredRange;
        private float _rangedAttackCooldown;
        private float _rangedAttackWindupTimer;
        private bool _rangedAttackTelegraphing;
        private float _summonSupportCooldown;
        private float _majorThreatSlamCooldown;
        private float _majorThreatSlamTelegraphTimer;
        private bool _majorThreatSlamTelegraphing;

        public SpawnInstanceId InstanceId { get; private set; }
        public bool IsAlive => _health != null && _health.IsAlive;
        public bool IsHitFlashActive => Presentation.IsHitFlashActive;
        public SurvivorsEnemyRole Role { get; private set; }
        public string ProfileId { get; private set; }
        public string DisplayName { get; private set; } = string.Empty;
        public float Radius { get; private set; }
        public int ExperienceReward { get; private set; }
        public float CurrentHealth => _health == null ? 0f : (float)_health.CurrentHealth;
        public float MaxHealth => _health == null ? 0f : (float)_health.MaximumHealth;
        public float HealthFraction => MaxHealth <= 0f ? 0f : CurrentHealth / MaxHealth;
        public float LeashTimerSeconds { get; private set; }
        public bool CanRecycle { get; private set; } = true;
        public bool CanLeash { get; private set; } = true;
        public bool CanReposition { get; private set; } = true;
        public bool ShowOffscreenMarker { get; private set; }
        public bool ShowOverheadLifeBar { get; private set; }
        public bool ShowBossLifeBar { get; private set; }
        public string MarkerStyle { get; private set; } = string.Empty;
        public bool IsMovementSlowed => StatusEffects.IsMovementSlowed;
        public float CurrentMoveSpeedMultiplier => StatusEffects.CurrentMoveSpeedMultiplier;
        public bool IsBurning => StatusEffects.IsBurning;
        public float CurrentBurnDamagePerSecond => StatusEffects.CurrentBurnDamagePerSecond;

        public void Initialize(SurvivorsTemplateController controller, SurvivorsEnemyProfile profile)
        {
            _controller = controller;
            Role = profile.Role;
            ProfileId = profile.Id;
            DisplayName = string.IsNullOrWhiteSpace(profile.DisplayName) ? profile.Role.ToString() : profile.DisplayName;
            _moveSpeed = Mathf.Max(0f, profile.MoveSpeed);
            Radius = Mathf.Max(0.05f, profile.Radius);
            _contactDamage = Mathf.Max(0f, profile.ContactDamage);
            _contactInterval = Mathf.Max(0.05f, profile.ContactIntervalSeconds);
            _contactCooldown = 0f;
            _rangedAttackRange = Mathf.Max(0f, profile.RangedAttackRange);
            _rangedAttackDamage = Mathf.Max(0f, profile.RangedAttackDamage);
            _rangedAttackInterval = Mathf.Max(0.05f, profile.RangedAttackIntervalSeconds);
            _rangedAttackWindupSeconds = controller == null ? 0f : Mathf.Max(0f, controller.CurrentTuning.EnemyRangedAttackWindupSeconds);
            _preferredRange = Mathf.Max(0f, profile.PreferredRange);
            _rangedAttackCooldown = Mathf.Min(0.75f, _rangedAttackInterval);
            _rangedAttackWindupTimer = 0f;
            _rangedAttackTelegraphing = false;
            _summonSupportCooldown = ResolveInitialSummonSupportCooldown(profile.Role, controller == null ? null : controller.CurrentTuning);
            _majorThreatSlamCooldown = ResolveInitialMajorThreatSlamCooldown(profile.Role, controller == null ? null : controller.CurrentTuning);
            _majorThreatSlamTelegraphTimer = 0f;
            _majorThreatSlamTelegraphing = false;
            CanRecycle = profile.CanRecycle;
            CanLeash = profile.CanLeash;
            CanReposition = profile.CanReposition;
            ShowOffscreenMarker = profile.ShowOffscreenMarker;
            ShowOverheadLifeBar = profile.ShowOverheadLifeBar;
            ShowBossLifeBar = profile.ShowBossLifeBar;
            MarkerStyle = string.IsNullOrWhiteSpace(profile.MarkerStyle) ? profile.Role.ToString() : profile.MarkerStyle;
            StatusEffects.Reset();
            ExperienceReward = Mathf.Max(1, profile.ExperienceReward);
            string id = InstanceId.Value > 0 ? "combatant.survivors.enemy." + InstanceId.Value : "combatant.survivors.enemy.pending";
            float maxHealth = Mathf.Max(1f, profile.MaxHealth);
            _health = new HealthState(new CombatantId(id), maxHealth, maxHealth);

            LeashTimerSeconds = 0f;
            Presentation.Initialize(Radius, profile.Tint);
        }

        public void AddLeashTime(float deltaTime)
        {
            LeashTimerSeconds += Mathf.Max(0f, deltaTime);
        }

        public void ResetLeashTimer()
        {
            LeashTimerSeconds = 0f;
        }

        public void Simulate(float deltaTime)
        {
            if (!IsAlive || _controller == null || !_controller.IsPlaying)
            {
                return;
            }

            Presentation.Tick(deltaTime);
            StatusEffects.TickMovementSlow(deltaTime);
            StatusEffects.TickDamageOverTime(deltaTime);
            if (!IsAlive)
            {
                return;
            }

            Vector3 direction = _controller.PlayerPosition - transform.position;
            direction.y = 0f;
            float distance = direction.magnitude;
            Vector3 moveDirection = Vector3.zero;
            Vector3 facingDirection = Vector3.forward;
            if (distance > 0.001f)
            {
                Vector3 normalized = direction / distance;
                moveDirection = ResolveMoveDirection(normalized, distance);
                facingDirection = normalized;
            }

            Vector3 separation = _controller.ResolveEnemyCrowdSeparation(this);
            if (separation.sqrMagnitude > 0.001f)
            {
                moveDirection += separation;
            }

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Vector3 resolvedMove = moveDirection.normalized;
                float catchUpMultiplier = _controller.ResolveEnemyCatchUpMoveSpeedMultiplier(this, distance);
                transform.position += resolvedMove * (_moveSpeed * CurrentMoveSpeedMultiplier * catchUpMultiplier * deltaTime);
                transform.forward = resolvedMove;
            }
            else if (distance > 0.001f)
            {
                transform.forward = facingDirection;
            }

            TickMajorThreatSlam(deltaTime, distance);
            TickRangedAttack(deltaTime, distance);
            TickSummonSupport(deltaTime);
            _contactCooldown -= deltaTime;
            if (_contactCooldown <= 0f && distance <= Radius + _controller.CurrentTuning.PlayerRadius)
            {
                _controller.ApplyDamageToPlayer(_contactDamage, "combatant.survivors.enemy." + InstanceId.Value);
                _contactCooldown = _contactInterval;
            }
        }

        public DamageResult ApplyDamage(float amount, string source)
        {
            return ApplyDamageInternal(amount, source, applyAugments: true);
        }

        public void ApplyDamageOverTime(float totalDamage, float durationSeconds, string statusId, string source)
        {
            StatusEffects.ApplyDamageOverTime(totalDamage, durationSeconds, statusId, source);
        }

        public bool ApplyMovementSlow(float multiplier, float durationSeconds)
        {
            return StatusEffects.ApplyMovementSlow(multiplier, durationSeconds);
        }

        public void ExecuteFromAugment(string source)
        {
            if (!IsAlive)
            {
                return;
            }

            ApplyDamageInternal(Mathf.Max(1f, CurrentHealth), (string.IsNullOrWhiteSpace(source) ? "survivors" : source) + ".augment.execute", applyAugments: false);
        }

        private DamageResult ApplyDamageInternal(float amount, string source, bool applyAugments)
        {
            if (_controller == null || _health == null || !IsAlive)
            {
                return null;
            }

            DamageResolutionResult result = _controller.ResolveEnemyDamage(_health, amount, source, applyAugments);
            if (result != null)
            {
                _controller.RecordEnemyDamageFeedback(this, result.Damage);
            }

            if (applyAugments && result != null && result.Damage != null && IsAlive)
            {
                _controller.ApplyDamageAugmentsToEnemy(this, result.Damage, source);
            }

            if (_health != null && !_health.IsAlive)
            {
                _controller.HandleEnemyKilled(this, source, applyAugments);
            }

            return result.Damage;
        }

        public void TriggerHitFlash(bool critical, float durationSeconds)
        {
            Presentation.TriggerHitFlash(critical, durationSeconds);
        }

        private Vector3 ResolveMoveDirection(Vector3 normalizedToPlayer, float distance)
        {
            if (_preferredRange <= 0f)
            {
                return normalizedToPlayer;
            }

            if (distance > _preferredRange * 1.12f)
            {
                return normalizedToPlayer;
            }

            if (distance < _preferredRange * 0.68f)
            {
                return -normalizedToPlayer;
            }

            return Vector3.zero;
        }

        private void TickMajorThreatSlam(float deltaTime, float distanceToPlayer)
        {
            if (_controller == null || !SurvivorsTemplateController.IsMajorThreatSlamRole(Role))
            {
                return;
            }

            SurvivorsTemplateTuning tuning = _controller.CurrentTuning;
            float interval = Mathf.Max(0f, tuning.MajorThreatSlamIntervalSeconds);
            float radius = Mathf.Max(0.5f, tuning.MajorThreatSlamRadius + Radius * 0.35f);
            if (interval <= 0f || radius <= 0f)
            {
                return;
            }

            if (_majorThreatSlamTelegraphing)
            {
                _majorThreatSlamTelegraphTimer = Mathf.Max(0f, _majorThreatSlamTelegraphTimer - Mathf.Max(0f, deltaTime));
                if (_majorThreatSlamTelegraphTimer <= 0f)
                {
                    _majorThreatSlamTelegraphing = false;
                    _majorThreatSlamCooldown = interval;
                    _controller.ResolveMajorThreatSlam(this);
                }

                return;
            }

            _majorThreatSlamCooldown = Mathf.Max(0f, _majorThreatSlamCooldown - Mathf.Max(0f, deltaTime));
            if (_majorThreatSlamCooldown > 0f || distanceToPlayer > radius * 1.35f)
            {
                return;
            }

            _majorThreatSlamTelegraphing = true;
            _majorThreatSlamTelegraphTimer = Mathf.Max(0.05f, tuning.MajorThreatSlamTelegraphSeconds);
            _controller.RecordMajorThreatSlamTelegraph(this);
        }

        private static float ResolveInitialMajorThreatSlamCooldown(SurvivorsEnemyRole role, SurvivorsTemplateTuning tuning)
        {
            if (tuning == null || !SurvivorsTemplateController.IsMajorThreatSlamRole(role))
            {
                return 0f;
            }

            return Mathf.Max(0f, tuning.MajorThreatSlamIntervalSeconds);
        }

        private static float ResolveInitialSummonSupportCooldown(SurvivorsEnemyRole role, SurvivorsTemplateTuning tuning)
        {
            if (tuning == null || role != SurvivorsEnemyRole.Summoner)
            {
                return 0f;
            }

            return Mathf.Max(0f, tuning.SummonerSupportInitialDelaySeconds);
        }

        private void TickRangedAttack(float deltaTime, float distance)
        {
            if (_controller == null || _rangedAttackRange <= 0f || _rangedAttackDamage <= 0f)
            {
                _rangedAttackTelegraphing = false;
                _rangedAttackWindupTimer = 0f;
                return;
            }

            float dt = Mathf.Max(0f, deltaTime);
            if (_rangedAttackTelegraphing)
            {
                _rangedAttackWindupTimer = Mathf.Max(0f, _rangedAttackWindupTimer - dt);
                if (_rangedAttackWindupTimer > 0f)
                {
                    return;
                }

                _rangedAttackTelegraphing = false;
                _rangedAttackCooldown = _rangedAttackInterval;
                if (ResolveDistanceToPlayer() <= _rangedAttackRange)
                {
                    _controller.ApplyDamageToPlayer(_rangedAttackDamage, "combatant.survivors.enemy.ranged." + InstanceId.Value);
                }
                else
                {
                    _controller.RecordEnemyRangedAttackDodgeFeedback(this);
                }

                return;
            }

            _rangedAttackCooldown = Mathf.Max(0f, _rangedAttackCooldown - dt);
            if (_rangedAttackCooldown > 0f || distance > _rangedAttackRange)
            {
                return;
            }

            _controller.RecordEnemyRangedAttackFeedback(transform.position, _controller.PlayerPosition, Role);
            if (_rangedAttackWindupSeconds <= 0f)
            {
                _controller.ApplyDamageToPlayer(_rangedAttackDamage, "combatant.survivors.enemy.ranged." + InstanceId.Value);
                _rangedAttackCooldown = _rangedAttackInterval;
                return;
            }

            _rangedAttackTelegraphing = true;
            _rangedAttackWindupTimer = _rangedAttackWindupSeconds;
        }

        private void TickSummonSupport(float deltaTime)
        {
            if (_controller == null || Role != SurvivorsEnemyRole.Summoner)
            {
                return;
            }

            SurvivorsTemplateTuning tuning = _controller.CurrentTuning;
            float interval = Mathf.Max(0f, tuning.SummonerSupportIntervalSeconds);
            if (interval <= 0f || tuning.SummonerSupportCount <= 0)
            {
                return;
            }

            _summonSupportCooldown = Mathf.Max(0f, _summonSupportCooldown - Mathf.Max(0f, deltaTime));
            if (_summonSupportCooldown > 0f)
            {
                return;
            }

            int spawned = _controller.SpawnSummonerSupport(this);
            _summonSupportCooldown = spawned > 0 ? interval : Mathf.Min(interval, 0.75f);
        }

        private float ResolveDistanceToPlayer()
        {
            if (_controller == null)
            {
                return float.MaxValue;
            }

            Vector3 delta = _controller.PlayerPosition - transform.position;
            delta.y = 0f;
            return delta.magnitude;
        }

        public void OverrideHealthForTest(float health)
        {
            string id = InstanceId.Value > 0 ? "combatant.survivors.enemy." + InstanceId.Value : "combatant.survivors.enemy.test";
            float resolved = Mathf.Max(1f, health);
            _health = new HealthState(new CombatantId(id), resolved, resolved);
        }

        public void OnWorldSpawned(WorldSpawnContext context)
        {
            InstanceId = context.InstanceId;
        }

        public void OnWorldDespawned(DespawnReason reason)
        {
            _controller = null;
            _health = null;
            ProfileId = null;
            DisplayName = string.Empty;
            StatusEffects.Reset();
            _rangedAttackWindupTimer = 0f;
            _rangedAttackTelegraphing = false;
            _summonSupportCooldown = 0f;

            LeashTimerSeconds = 0f;
            Presentation.ResetForPool();
        }

        public void ResetForWorldSpawn()
        {
            _controller = null;
            _health = null;
            _contactCooldown = 0f;
            _rangedAttackRange = 0f;
            _rangedAttackDamage = 0f;
            _rangedAttackInterval = 0f;
            _rangedAttackWindupSeconds = 0f;
            _preferredRange = 0f;
            _rangedAttackCooldown = 0f;
            _rangedAttackWindupTimer = 0f;
            _rangedAttackTelegraphing = false;
            _summonSupportCooldown = 0f;
            StatusEffects.Reset();

            LeashTimerSeconds = 0f;
            Presentation.ResetForPool();
            Role = SurvivorsEnemyRole.Swarm;
            ProfileId = null;
            DisplayName = string.Empty;
            InstanceId = default;
        }

        private void OnDestroy()
        {
            _presentation?.Dispose();
        }
    }
}
