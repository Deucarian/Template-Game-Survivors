using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal abstract class SurvivorsPayloadWeaponRuntimeBase : SurvivorsWeaponRuntimeBase
    {
        private const int EvolvedSatelliteHazardCount = 6;
        private const float EvolvedHazardRadiusMultiplier = 1.18f;
        private const float EvolvedHazardDurationMultiplier = 1.65f;
        private const float EvolvedHazardTickIntervalMultiplier = 0.65f;
        private const float EvolvedHazardDamageMultiplier = 1.25f;
        private const float EvolvedSatelliteHazardRadiusMultiplier = 0.58f;
        private const float EvolvedSatelliteHazardDistanceMultiplier = 0.82f;
        private readonly List<SurvivorsPayloadActorBase> _payloads = new List<SurvivorsPayloadActorBase>();
        protected readonly List<SurvivorsEnemyActor> DamageTargets = new List<SurvivorsEnemyActor>();

        protected SurvivorsPayloadWeaponRuntimeBase(SurvivorsTemplateController controller, SurvivorsWeaponArchetypeDefinition definition)
            : base(controller, definition)
        {
        }

        public override void Dispose()
        {
            for (int i = 0; i < _payloads.Count; i++)
            {
                if (_payloads[i] != null)
                {
                    _payloads[i].DisposePayload();
                }
            }

            _payloads.Clear();
        }

        protected int ResolvePayloadCount()
        {
            return Mathf.Max(1, Definition.PayloadCount + Controller.PayloadCountBonus);
        }

        protected float ResolveExplosionRadius()
        {
            return Mathf.Max(0.1f, Definition.PayloadExplosionRadius + Controller.PayloadExplosionRadiusBonus + Controller.AreaRadiusBonus);
        }

        protected float ResolveTriggerRadius()
        {
            return Mathf.Max(0.1f, Definition.PayloadTriggerRadius + Controller.PayloadTriggerRadiusBonus + Controller.AreaRadiusBonus * 0.5f);
        }

        protected void RegisterPayload(SurvivorsPayloadActorBase payload)
        {
            if (payload != null)
            {
                _payloads.Add(payload);
            }
        }

        protected void TickPayloads(float deltaTime)
        {
            for (int i = _payloads.Count - 1; i >= 0; i--)
            {
                SurvivorsPayloadActorBase payload = _payloads[i];
                if (payload == null || !payload.IsActive)
                {
                    _payloads.RemoveAt(i);
                    continue;
                }

                payload.Simulate(deltaTime);
                if (!payload.IsActive)
                {
                    _payloads.RemoveAt(i);
                }
            }
        }

        protected void Detonate(Vector3 origin)
        {
            float radius = ResolveExplosionRadius();
            float damage = Controller.ResolveWeaponDamage(Definition);
            SurvivorsPayloadUtility.ApplyExplosion(Controller, Definition, origin, radius, damage, DamageTargets);
            bool payloadEvolutionActive = IsPayloadEvolutionActive();
            bool leavesHazard = Definition.PayloadLeavesHazard || payloadEvolutionActive;
            float baseHazardDuration = payloadEvolutionActive
                ? Mathf.Max(Definition.PayloadHazardDurationSeconds, 1.4f)
                : Definition.PayloadHazardDurationSeconds;
            float baseHazardTickInterval = payloadEvolutionActive
                ? Mathf.Max(0.05f, Definition.PayloadHazardTickIntervalSeconds)
                : Definition.PayloadHazardTickIntervalSeconds;
            float baseHazardDamageRatio = payloadEvolutionActive
                ? Mathf.Max(Definition.PayloadHazardDamageRatio, 0.16f)
                : Definition.PayloadHazardDamageRatio;
            if (leavesHazard && baseHazardDuration > 0f && baseHazardDamageRatio > 0f)
            {
                float hazardRadius = Mathf.Max(0.2f, radius * 0.85f);
                float hazardDuration = baseHazardDuration;
                float hazardTickInterval = baseHazardTickInterval;
                float hazardDamage = damage * baseHazardDamageRatio;
                if (payloadEvolutionActive)
                {
                    hazardRadius *= EvolvedHazardRadiusMultiplier;
                    hazardDuration *= EvolvedHazardDurationMultiplier;
                    hazardTickInterval *= EvolvedHazardTickIntervalMultiplier;
                    hazardDamage *= EvolvedHazardDamageMultiplier;
                }

                SpawnHazard(origin, hazardRadius, hazardDuration, hazardTickInterval, hazardDamage);
                if (payloadEvolutionActive)
                {
                    SpawnSatelliteHazards(origin, hazardRadius, hazardDuration, hazardTickInterval, hazardDamage);
                }
            }
        }

        private bool IsPayloadEvolutionActive()
        {
            if (Definition.Id == BasicSurvivorsGame.GravityGrenadeWeaponContentId)
            {
                return Controller.IsEvolutionActive(BasicSurvivorsGame.GravefieldEngineEvolutionUpgradeId);
            }

            if (Definition.Id == BasicSurvivorsGame.RuneTrapWeaponContentId ||
                Definition.Id == BasicSurvivorsGame.AetherMineWeaponContentId)
            {
                return Controller.IsEvolutionActive(BasicSurvivorsGame.AetherfieldMatrixEvolutionUpgradeId);
            }

            return false;
        }

        private void SpawnSatelliteHazards(Vector3 origin, float hazardRadius, float durationSeconds, float tickIntervalSeconds, float damagePerTick)
        {
            float distance = Mathf.Max(0.35f, hazardRadius * EvolvedSatelliteHazardDistanceMultiplier);
            float satelliteRadius = Mathf.Max(0.25f, hazardRadius * EvolvedSatelliteHazardRadiusMultiplier);
            float satelliteDamage = damagePerTick * EvolvedSatelliteHazardRadiusMultiplier;
            for (int index = 0; index < EvolvedSatelliteHazardCount; index++)
            {
                float angle = (360f / EvolvedSatelliteHazardCount) * index;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * distance);
                SpawnHazard(origin + offset, satelliteRadius, durationSeconds, tickIntervalSeconds, satelliteDamage);
            }
        }

        private void SpawnHazard(Vector3 origin, float radius, float durationSeconds, float tickIntervalSeconds, float damagePerTick)
        {
            GameObject hazardObject = new GameObject(Definition.DisplayName + " Hazard");
            hazardObject.transform.SetParent(Controller.RuntimeWorldRoot, false);
            hazardObject.transform.position = origin + Vector3.up * 0.02f;
            SurvivorsPayloadHazardActor hazard = hazardObject.AddComponent<SurvivorsPayloadHazardActor>();
            hazard.Initialize(
                Controller,
                Definition,
                radius,
                durationSeconds,
                tickIntervalSeconds,
                damagePerTick);
            RegisterPayload(hazard);
        }
    }

    internal sealed class SurvivorsGrenadeWeaponRuntime : SurvivorsPayloadWeaponRuntimeBase
    {
        private float _cooldownRemaining = 0.35f;

        public SurvivorsGrenadeWeaponRuntime(SurvivorsTemplateController controller, SurvivorsWeaponArchetypeDefinition definition)
            : base(controller, definition)
        {
        }

        public override void Tick(float deltaTime)
        {
            TickPayloads(deltaTime);
            _cooldownRemaining -= deltaTime;
            if (_cooldownRemaining > 0f)
            {
                return;
            }

            if (TryFire())
            {
                _cooldownRemaining = Controller.ResolveWeaponCooldownSeconds(Definition);
            }
        }

        public override bool FireForTest()
        {
            _cooldownRemaining = 0f;
            return TryFire();
        }

        private bool TryFire()
        {
            SurvivorsEnemyActor target = Controller.FindNearestEnemy(Controller.PlayerPosition, Definition.Range);
            if (target == null)
            {
                return false;
            }

            int payloadCount = ResolvePayloadCount();
            float spreadRadius = Mathf.Max(0.6f, ResolveExplosionRadius() * 0.8f);
            Vector3 origin = Controller.PlayerPosition + Vector3.up * 0.4f;
            for (int i = 0; i < payloadCount; i++)
            {
                float angle = payloadCount == 1 ? 0f : (360f / payloadCount) * i;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * spreadRadius * 0.45f);
                Vector3 targetPosition = target.transform.position + offset;
                targetPosition.y = 0f;
                SpawnThrownPayload(origin, targetPosition);
            }

            return true;
        }

        private void SpawnThrownPayload(Vector3 origin, Vector3 target)
        {
            GameObject payloadObject = new GameObject(Definition.DisplayName + " Payload");
            payloadObject.transform.SetParent(Controller.RuntimeWorldRoot, false);
            SurvivorsThrownPayloadActor payload = payloadObject.AddComponent<SurvivorsThrownPayloadActor>();
            float distance = Vector3.Distance(origin, target);
            float travelDuration = distance / Mathf.Max(1f, Definition.PayloadTravelSpeed);
            payload.Initialize(
                Definition,
                origin,
                target,
                travelDuration,
                Definition.PayloadArmingSeconds,
                Mathf.Clamp(distance * 0.18f, 0.5f, 2.4f),
                Detonate);
            RegisterPayload(payload);
            Controller.RecordPayloadThrow();
        }
    }

    internal sealed class SurvivorsPlacedPayloadWeaponRuntime : SurvivorsPayloadWeaponRuntimeBase
    {
        private float _cooldownRemaining = 0.45f;

        public SurvivorsPlacedPayloadWeaponRuntime(SurvivorsTemplateController controller, SurvivorsWeaponArchetypeDefinition definition)
            : base(controller, definition)
        {
        }

        public override void Tick(float deltaTime)
        {
            TickPayloads(deltaTime);
            _cooldownRemaining -= deltaTime;
            if (_cooldownRemaining > 0f)
            {
                return;
            }

            if (TryDeploy())
            {
                _cooldownRemaining = Controller.ResolveWeaponCooldownSeconds(Definition);
            }
        }

        public override bool FireForTest()
        {
            _cooldownRemaining = 0f;
            return TryDeploy();
        }

        private bool TryDeploy()
        {
            SurvivorsEnemyActor target = Controller.FindNearestEnemy(Controller.PlayerPosition, Definition.Range);
            Vector3 anchor = target == null
                ? Controller.PlayerPosition + Controller.PlayerForward * Mathf.Min(Definition.Range, 4f)
                : target.transform.position;
            int payloadCount = ResolvePayloadCount();
            for (int i = 0; i < payloadCount; i++)
            {
                float angle = payloadCount == 1 ? 0f : (360f / payloadCount) * i;
                float placementRadius = Definition.PayloadPlacementRadius;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * placementRadius);
                Vector3 spawnPosition = Definition.Archetype == SurvivorsWeaponArchetype.Mine
                    ? Controller.PlayerPosition + offset
                    : anchor + offset * 0.45f;
                spawnPosition.y = 0f;
                SpawnPlacedPayload(spawnPosition);
            }

            return true;
        }

        private void SpawnPlacedPayload(Vector3 position)
        {
            GameObject payloadObject = new GameObject(Definition.DisplayName + " Payload");
            payloadObject.transform.SetParent(Controller.RuntimeWorldRoot, false);
            payloadObject.transform.position = position + Vector3.up * 0.03f;
            SurvivorsPlacedPayloadActor payload = payloadObject.AddComponent<SurvivorsPlacedPayloadActor>();
            payload.Initialize(
                Controller,
                Definition,
                Definition.PayloadArmingSeconds,
                Definition.PayloadLifetimeSeconds,
                ResolveTriggerRadius(),
                Definition.PayloadAutoDetonateAtExpiry,
                Detonate);
            RegisterPayload(payload);
            Controller.RecordPayloadPlaced();
        }
    }

    public abstract class SurvivorsPayloadActorBase : MonoBehaviour
    {
        public bool IsActive { get; protected set; }

        public abstract void Simulate(float deltaTime);

        public void DisposePayload()
        {
            IsActive = false;
            SurvivorsVisualUtility.ReleaseTemplateObject(gameObject);
        }
    }

    public sealed class SurvivorsThrownPayloadActor : SurvivorsPayloadActorBase
    {
        private Action<Vector3> _onDetonate;
        private float _travelDuration;
        private float _fuseDuration;
        private float _elapsed;
        private float _arcHeight;
        private Vector3 _start;
        private Vector3 _target;
        private Transform _visual;
        private bool _landed;

        public void Initialize(
            SurvivorsWeaponArchetypeDefinition definition,
            Vector3 start,
            Vector3 target,
            float travelDuration,
            float fuseDuration,
            float arcHeight,
            Action<Vector3> onDetonate)
        {
            _start = start;
            _target = target;
            _travelDuration = Mathf.Max(0.05f, travelDuration);
            _fuseDuration = Mathf.Max(0.01f, fuseDuration);
            _arcHeight = Mathf.Max(0.1f, arcHeight);
            _onDetonate = onDetonate;
            _elapsed = 0f;
            _landed = false;
            IsActive = true;
            transform.position = start;

            GameObject visual = SurvivorsVisualUtility.CreatePrimitiveVisual("Payload", PrimitiveType.Sphere, definition.Tint, transform);
            visual.transform.localScale = Vector3.one * 0.35f;
            _visual = visual.transform;
        }

        public override void Simulate(float deltaTime)
        {
            if (!IsActive)
            {
                return;
            }

            _elapsed += deltaTime;
            if (!_landed)
            {
                float progress = Mathf.Clamp01(_elapsed / _travelDuration);
                Vector3 position = Vector3.Lerp(_start, _target, progress);
                position.y += Mathf.Sin(progress * Mathf.PI) * _arcHeight;
                transform.position = position;
                transform.Rotate(0f, 360f * deltaTime, 0f, Space.Self);
                if (progress >= 1f)
                {
                    _landed = true;
                    _elapsed = 0f;
                    transform.position = _target;
                    if (_visual != null)
                    {
                        _visual.localScale *= 1.12f;
                    }
                }

                return;
            }

            if (_visual != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 18f) * 0.08f;
                _visual.localScale = Vector3.one * 0.39f * pulse;
            }

            if (_elapsed >= _fuseDuration)
            {
                IsActive = false;
                _onDetonate?.Invoke(transform.position);
                SurvivorsVisualUtility.ReleaseTemplateObject(gameObject);
            }
        }
    }

    public sealed class SurvivorsPlacedPayloadActor : SurvivorsPayloadActorBase
    {
        private SurvivorsTemplateController _controller;
        private Action<Vector3> _onDetonate;
        private float _armingSeconds;
        private float _remainingLifetime;
        private float _triggerRadius;
        private bool _armed;
        private bool _autoDetonateAtExpiry;
        private Transform _visual;

        public void Initialize(
            SurvivorsTemplateController controller,
            SurvivorsWeaponArchetypeDefinition definition,
            float armingSeconds,
            float lifetimeSeconds,
            float triggerRadius,
            bool autoDetonateAtExpiry,
            Action<Vector3> onDetonate)
        {
            _controller = controller;
            _armingSeconds = Mathf.Max(0.05f, armingSeconds);
            _remainingLifetime = Mathf.Max(0.1f, lifetimeSeconds);
            _triggerRadius = Mathf.Max(0.1f, triggerRadius);
            _autoDetonateAtExpiry = autoDetonateAtExpiry;
            _onDetonate = onDetonate;
            _armed = false;
            IsActive = true;

            GameObject visual = SurvivorsVisualUtility.CreatePrimitiveVisual("Payload", PrimitiveType.Cylinder, definition.Tint, transform);
            visual.transform.localScale = new Vector3(_triggerRadius * 0.6f, 0.1f, _triggerRadius * 0.6f);
            _visual = visual.transform;
        }

        public override void Simulate(float deltaTime)
        {
            if (!IsActive)
            {
                return;
            }

            _remainingLifetime -= deltaTime;
            if (!_armed)
            {
                _armingSeconds -= deltaTime;
                if (_visual != null)
                {
                    _visual.localScale = Vector3.Lerp(_visual.localScale, new Vector3(_triggerRadius, 0.08f, _triggerRadius), 0.12f);
                }

                if (_armingSeconds <= 0f)
                {
                    _armed = true;
                }
            }
            else
            {
                if (_visual != null)
                {
                    float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.08f;
                    _visual.localScale = new Vector3(_triggerRadius * pulse, 0.08f, _triggerRadius * pulse);
                }

                if (HasEnemyInsideRadius())
                {
                    Detonate();
                    return;
                }
            }

            if (_remainingLifetime <= 0f)
            {
                if (_autoDetonateAtExpiry)
                {
                    Detonate();
                }
                else
                {
                    IsActive = false;
                    SurvivorsVisualUtility.ReleaseTemplateObject(gameObject);
                }
            }
        }

        private bool HasEnemyInsideRadius()
        {
            if (_controller == null)
            {
                return false;
            }

            float radiusSquared = _triggerRadius * _triggerRadius;
            IReadOnlyList<SurvivorsEnemyActor> enemies = _controller.ActiveEnemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                Vector3 offset = enemy.transform.position - transform.position;
                offset.y = 0f;
                if (offset.sqrMagnitude <= radiusSquared)
                {
                    return true;
                }
            }

            return false;
        }

        private void Detonate()
        {
            IsActive = false;
            _onDetonate?.Invoke(transform.position);
            SurvivorsVisualUtility.ReleaseTemplateObject(gameObject);
        }
    }

    public sealed class SurvivorsPayloadHazardActor : SurvivorsPayloadActorBase
    {
        private readonly List<SurvivorsEnemyActor> _targets = new List<SurvivorsEnemyActor>();
        private SurvivorsTemplateController _controller;
        private SurvivorsWeaponArchetypeDefinition _definition;
        private float _radius;
        private float _duration;
        private float _tickInterval;
        private float _timeUntilTick;
        private float _damagePerTick;

        public void Initialize(
            SurvivorsTemplateController controller,
            SurvivorsWeaponArchetypeDefinition definition,
            float radius,
            float duration,
            float tickInterval,
            float damagePerTick)
        {
            _controller = controller;
            _definition = definition;
            _radius = Mathf.Max(0.1f, radius);
            _duration = Mathf.Max(0.05f, duration);
            _tickInterval = Mathf.Max(0.05f, tickInterval);
            _timeUntilTick = 0f;
            _damagePerTick = Mathf.Max(0f, damagePerTick);
            IsActive = true;

            GameObject visual = SurvivorsVisualUtility.CreatePrimitiveVisual("Payload Hazard", PrimitiveType.Cylinder, definition.Tint, transform);
            visual.transform.localScale = new Vector3(_radius * 2f, 0.04f, _radius * 2f);
        }

        public override void Simulate(float deltaTime)
        {
            if (!IsActive || _controller == null || _definition == null)
            {
                return;
            }

            _duration -= deltaTime;
            _timeUntilTick -= deltaTime;
            while (_timeUntilTick <= 0f && _duration > 0f)
            {
                TickDamage();
                _timeUntilTick += _tickInterval;
            }

            if (_duration <= 0f)
            {
                IsActive = false;
                SurvivorsVisualUtility.ReleaseTemplateObject(gameObject);
            }
        }

        private void TickDamage()
        {
            if (_damagePerTick <= 0f)
            {
                return;
            }

            _controller.CollectEnemiesWithinRadius(transform.position, _radius, _targets);
            for (int i = 0; i < _targets.Count; i++)
            {
                SurvivorsEnemyActor enemy = _targets[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                if (enemy.ApplyDamage(_damagePerTick, _definition.Id + ".hazard") != null)
                {
                    _controller.RecordPayloadHazardTick();
                }
            }

            _targets.Clear();
        }
    }

    internal static class SurvivorsPayloadUtility
    {
        public static void ApplyExplosion(
            SurvivorsTemplateController controller,
            SurvivorsWeaponArchetypeDefinition definition,
            Vector3 origin,
            float radius,
            float damage,
            List<SurvivorsEnemyActor> targets)
        {
            if (controller == null || definition == null || targets == null)
            {
                return;
            }

            controller.CollectEnemiesWithinRadius(origin, radius, targets);
            for (int i = 0; i < targets.Count; i++)
            {
                SurvivorsEnemyActor enemy = targets[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                if (enemy.ApplyDamage(damage, definition.Id) != null)
                {
                    controller.RecordPayloadExplosionHit();
                }
            }

            targets.Clear();
            CreateExplosionVisual(controller, definition, origin, radius);
            controller.RecordPayloadDetonation();
        }

        private static void CreateExplosionVisual(
            SurvivorsTemplateController controller,
            SurvivorsWeaponArchetypeDefinition definition,
            Vector3 origin,
            float radius)
        {
            GameObject pulseObject = SurvivorsVisualUtility.CreatePrimitiveVisual(
                definition.DisplayName + " Explosion",
                PrimitiveType.Cylinder,
                definition.Tint,
                controller.RuntimeWorldRoot);
            pulseObject.transform.position = origin + Vector3.up * 0.05f;
            SurvivorsTimedVisual visual = pulseObject.AddComponent<SurvivorsTimedVisual>();
            visual.Initialize(new Vector3(radius * 2f, 0.08f, radius * 2f), 0.24f, definition.Tint);
        }
    }
}
