using System;
using System.Collections.Generic;
using Deucarian.Common;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    public enum SurvivorsWeaponArchetype
    {
        Projectile = 0,
        Orbit = 1,
        Melee = 2,
        Burst = 3,
        Hitscan = 4,
        Grenade = 5,
        Trap = 6,
        Mine = 7
    }

    public sealed class SurvivorsWeaponArchetypeDefinition
    {
        public SurvivorsWeaponArchetypeDefinition(
            string id,
            string displayName,
            SurvivorsWeaponArchetype archetype,
            float cooldownSeconds,
            float damage,
            float range,
            Color tint,
            float projectileSpeed = 0f,
            float projectileRadius = 0f,
            float projectileLifetimeSeconds = 0f,
            int projectilePierceCount = 0,
            int projectileChainCount = 0,
            int projectileForkCount = 0,
            int projectileReturnCount = 0,
            int projectileFanCount = 1,
            float projectileSpreadDegrees = 0f,
            int orbitCount = 1,
            float orbitRadius = 0f,
            float orbitDegreesPerSecond = 0f,
            float orbitContactTickIntervalSeconds = 0f,
            int meleeHitCount = 1,
            float meleeArcDegrees = 115f,
            float meleeVisualDurationSeconds = 0.16f,
            int burstCount = 1,
            float burstRepeatIntervalSeconds = 0.18f,
            float burstVisualDurationSeconds = 0.22f,
            int hitscanCount = 1,
            float hitscanWidth = 0.18f,
            float hitscanVisualDurationSeconds = 0.08f,
            bool hitscanPierces = false,
            int payloadCount = 1,
            float payloadTravelSpeed = 9f,
            float payloadArmingSeconds = 0.7f,
            float payloadLifetimeSeconds = 4f,
            float payloadTriggerRadius = 1.35f,
            float payloadExplosionRadius = 2.4f,
            float payloadPlacementRadius = 1.8f,
            bool payloadAutoDetonateAtExpiry = false,
            bool payloadLeavesHazard = false,
            float payloadHazardDurationSeconds = 0f,
            float payloadHazardTickIntervalSeconds = 0.45f,
            float payloadHazardDamageRatio = 0f)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Weapon id is required.", nameof(id));
            }

            Id = id;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
            Archetype = archetype;
            CooldownSeconds = Mathf.Max(0.05f, cooldownSeconds);
            Damage = Mathf.Max(0f, damage);
            Range = Mathf.Max(0.1f, range);
            Tint = tint;
            ProjectileSpeed = Mathf.Max(0f, projectileSpeed);
            ProjectileRadius = Mathf.Max(0.01f, projectileRadius);
            ProjectileLifetimeSeconds = Mathf.Max(0.05f, projectileLifetimeSeconds);
            ProjectilePierceCount = Mathf.Max(0, projectilePierceCount);
            ProjectileChainCount = Mathf.Max(0, projectileChainCount);
            ProjectileForkCount = Mathf.Max(0, projectileForkCount);
            ProjectileReturnCount = Mathf.Max(0, projectileReturnCount);
            ProjectileFanCount = Mathf.Max(1, projectileFanCount);
            ProjectileSpreadDegrees = Mathf.Max(0f, projectileSpreadDegrees);
            OrbitCount = Mathf.Max(1, orbitCount);
            OrbitRadius = Mathf.Max(0.1f, orbitRadius);
            OrbitDegreesPerSecond = Mathf.Max(0f, orbitDegreesPerSecond);
            OrbitContactTickIntervalSeconds = Mathf.Max(0.05f, orbitContactTickIntervalSeconds);
            MeleeHitCount = Mathf.Max(1, meleeHitCount);
            MeleeArcDegrees = Mathf.Clamp(meleeArcDegrees, 1f, 360f);
            MeleeVisualDurationSeconds = Mathf.Max(0.05f, meleeVisualDurationSeconds);
            BurstCount = Mathf.Max(1, burstCount);
            BurstRepeatIntervalSeconds = Mathf.Max(0.05f, burstRepeatIntervalSeconds);
            BurstVisualDurationSeconds = Mathf.Max(0.05f, burstVisualDurationSeconds);
            HitscanCount = Mathf.Max(1, hitscanCount);
            HitscanWidth = Mathf.Max(0.03f, hitscanWidth);
            HitscanVisualDurationSeconds = Mathf.Max(0.03f, hitscanVisualDurationSeconds);
            HitscanPierces = hitscanPierces;
            PayloadCount = Mathf.Max(1, payloadCount);
            PayloadTravelSpeed = Mathf.Max(0.05f, payloadTravelSpeed);
            PayloadArmingSeconds = Mathf.Max(0.05f, payloadArmingSeconds);
            PayloadLifetimeSeconds = Mathf.Max(0.1f, payloadLifetimeSeconds);
            PayloadTriggerRadius = Mathf.Max(0.1f, payloadTriggerRadius);
            PayloadExplosionRadius = Mathf.Max(0.1f, payloadExplosionRadius);
            PayloadPlacementRadius = Mathf.Max(0.1f, payloadPlacementRadius);
            PayloadAutoDetonateAtExpiry = payloadAutoDetonateAtExpiry;
            PayloadLeavesHazard = payloadLeavesHazard;
            PayloadHazardDurationSeconds = Mathf.Max(0f, payloadHazardDurationSeconds);
            PayloadHazardTickIntervalSeconds = Mathf.Max(0.05f, payloadHazardTickIntervalSeconds);
            PayloadHazardDamageRatio = Mathf.Max(0f, payloadHazardDamageRatio);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public SurvivorsWeaponArchetype Archetype { get; }
        public float CooldownSeconds { get; }
        public float Damage { get; }
        public float Range { get; }
        public Color Tint { get; }
        public float ProjectileSpeed { get; }
        public float ProjectileRadius { get; }
        public float ProjectileLifetimeSeconds { get; }
        public int ProjectilePierceCount { get; }
        public int ProjectileChainCount { get; }
        public int ProjectileForkCount { get; }
        public int ProjectileReturnCount { get; }
        public int ProjectileFanCount { get; }
        public float ProjectileSpreadDegrees { get; }
        public int OrbitCount { get; }
        public float OrbitRadius { get; }
        public float OrbitDegreesPerSecond { get; }
        public float OrbitContactTickIntervalSeconds { get; }
        public int MeleeHitCount { get; }
        public float MeleeArcDegrees { get; }
        public float MeleeVisualDurationSeconds { get; }
        public int BurstCount { get; }
        public float BurstRepeatIntervalSeconds { get; }
        public float BurstVisualDurationSeconds { get; }
        public int HitscanCount { get; }
        public float HitscanWidth { get; }
        public float HitscanVisualDurationSeconds { get; }
        public bool HitscanPierces { get; }
        public int PayloadCount { get; }
        public float PayloadTravelSpeed { get; }
        public float PayloadArmingSeconds { get; }
        public float PayloadLifetimeSeconds { get; }
        public float PayloadTriggerRadius { get; }
        public float PayloadExplosionRadius { get; }
        public float PayloadPlacementRadius { get; }
        public bool PayloadAutoDetonateAtExpiry { get; }
        public bool PayloadLeavesHazard { get; }
        public float PayloadHazardDurationSeconds { get; }
        public float PayloadHazardTickIntervalSeconds { get; }
        public float PayloadHazardDamageRatio { get; }
        public bool IsPayload => Archetype == SurvivorsWeaponArchetype.Grenade ||
            Archetype == SurvivorsWeaponArchetype.Trap ||
            Archetype == SurvivorsWeaponArchetype.Mine;
    }

    public sealed class SurvivorsWeaponLoadoutRuntime
    {
        private readonly SurvivorsTemplateController _controller;
        private readonly List<SurvivorsWeaponRuntimeBase> _weapons = new List<SurvivorsWeaponRuntimeBase>();
        private readonly List<string> _weaponIds = new List<string>();

        public SurvivorsWeaponLoadoutRuntime(SurvivorsTemplateController controller, IReadOnlyList<SurvivorsWeaponArchetypeDefinition> definitions)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            _controller = controller;

            if (definitions == null)
            {
                return;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                SurvivorsWeaponArchetypeDefinition definition = definitions[i];
                if (definition == null)
                {
                    continue;
                }

                _weapons.Add(CreateRuntime(controller, definition));
                _weaponIds.Add(definition.Id);
            }
        }

        public IReadOnlyList<string> WeaponIds => _weaponIds;
        public int WeaponCount => _weapons.Count;

        public bool TryAddWeapon(SurvivorsWeaponArchetypeDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id) || ContainsWeapon(definition.Id))
            {
                return false;
            }

            _weapons.Add(CreateRuntime(_controller, definition));
            _weaponIds.Add(definition.Id);
            return true;
        }

        public int ActiveOrbitBladeCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _weapons.Count; i++)
                {
                    if (_weapons[i] is SurvivorsOrbitWeaponRuntime orbit)
                    {
                        count += orbit.ActiveBladeCount;
                    }
                }

                return count;
            }
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < _weapons.Count; i++)
            {
                _weapons[i].Tick(deltaTime);
            }
        }

        public bool FireForTest(SurvivorsWeaponArchetype archetype)
        {
            bool fired = false;
            for (int i = 0; i < _weapons.Count; i++)
            {
                SurvivorsWeaponRuntimeBase weapon = _weapons[i];
                if (weapon.Definition.Archetype == archetype)
                {
                    fired |= weapon.FireForTest();
                }
            }

            return fired;
        }

        public bool ContainsWeapon(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            for (int i = 0; i < _weaponIds.Count; i++)
            {
                if (string.Equals(_weaponIds[i], id, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            for (int i = 0; i < _weapons.Count; i++)
            {
                _weapons[i].Dispose();
            }

            _weapons.Clear();
            _weaponIds.Clear();
        }

        private static SurvivorsWeaponRuntimeBase CreateRuntime(SurvivorsTemplateController controller, SurvivorsWeaponArchetypeDefinition definition)
        {
            switch (definition.Archetype)
            {
                case SurvivorsWeaponArchetype.Projectile:
                    return new SurvivorsProjectileWeaponRuntime(controller, definition);
                case SurvivorsWeaponArchetype.Orbit:
                    return new SurvivorsOrbitWeaponRuntime(controller, definition);
                case SurvivorsWeaponArchetype.Melee:
                    return new SurvivorsMeleeWeaponRuntime(controller, definition);
                case SurvivorsWeaponArchetype.Burst:
                    return new SurvivorsBurstWeaponRuntime(controller, definition);
                case SurvivorsWeaponArchetype.Hitscan:
                    return new SurvivorsHitscanWeaponRuntime(controller, definition);
                case SurvivorsWeaponArchetype.Grenade:
                    return new SurvivorsGrenadeWeaponRuntime(controller, definition);
                case SurvivorsWeaponArchetype.Trap:
                case SurvivorsWeaponArchetype.Mine:
                    return new SurvivorsPlacedPayloadWeaponRuntime(controller, definition);
                default:
                    return new SurvivorsDisabledWeaponRuntime(controller, definition);
            }
        }
    }

    internal abstract class SurvivorsWeaponRuntimeBase
    {
        protected SurvivorsWeaponRuntimeBase(SurvivorsTemplateController controller, SurvivorsWeaponArchetypeDefinition definition)
        {
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public SurvivorsWeaponArchetypeDefinition Definition { get; }

        protected SurvivorsTemplateController Controller { get; }

        public abstract void Tick(float deltaTime);

        public virtual bool FireForTest()
        {
            return false;
        }

        public virtual void Dispose()
        {
        }
    }

    internal sealed class SurvivorsDisabledWeaponRuntime : SurvivorsWeaponRuntimeBase
    {
        public SurvivorsDisabledWeaponRuntime(SurvivorsTemplateController controller, SurvivorsWeaponArchetypeDefinition definition)
            : base(controller, definition)
        {
        }

        public override void Tick(float deltaTime)
        {
        }
    }

    internal sealed class SurvivorsProjectileWeaponRuntime : SurvivorsWeaponRuntimeBase
    {
        private const int ArcaneStormRingProjectileCount = 8;
        private const int BlizzardCrownRingProjectileCount = 10;
        private float _cooldownRemaining = 0.08f;

        public SurvivorsProjectileWeaponRuntime(SurvivorsTemplateController controller, SurvivorsWeaponArchetypeDefinition definition)
            : base(controller, definition)
        {
        }

        public override void Tick(float deltaTime)
        {
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

            Vector3 origin = Controller.PlayerPosition + Vector3.up * 0.4f;
            Vector3 direction = target.transform.position - origin;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = Vector3.forward;
            }

            int projectileCount = Mathf.Max(1, Definition.ProjectileFanCount + Controller.ProjectileFanBonus);
            float spreadDegrees = projectileCount <= 1
                ? 0f
                : Mathf.Max(Definition.ProjectileSpreadDegrees, 12f * (projectileCount - 1));
            bool fired = false;
            for (int index = 0; index < projectileCount; index++)
            {
                float normalizedIndex = projectileCount <= 1 ? 0.5f : index / (float)(projectileCount - 1);
                float angle = Mathf.Lerp(-spreadDegrees * 0.5f, spreadDegrees * 0.5f, normalizedIndex);
                Vector3 resolvedDirection = Quaternion.Euler(0f, angle, 0f) * direction.normalized;
                fired |= Controller.LaunchProjectile(Definition, resolvedDirection);
            }

            if (Definition.Id == BasicSurvivorsGame.ArcaneWandWeaponContentId &&
                Controller.IsEvolutionActive(BasicSurvivorsGame.ArcaneStormEvolutionUpgradeId))
            {
                fired |= FireProjectileRing(direction.normalized, ArcaneStormRingProjectileCount);
            }

            if (Definition.Id == BasicSurvivorsGame.FrostFanWeaponContentId &&
                Controller.IsEvolutionActive(BasicSurvivorsGame.BlizzardCrownEvolutionUpgradeId))
            {
                fired |= FireProjectileRing(direction.normalized, BlizzardCrownRingProjectileCount);
            }

            return fired;
        }

        private bool FireProjectileRing(Vector3 forward, int projectileCount)
        {
            Vector3 resolvedForward = forward.sqrMagnitude <= 0.001f ? Vector3.forward : forward.normalized;
            bool fired = false;
            int count = Mathf.Max(1, projectileCount);
            for (int index = 0; index < count; index++)
            {
                float angle = (360f / count) * index;
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * resolvedForward;
                fired |= Controller.LaunchProjectile(Definition, direction);
            }

            return fired;
        }
    }

    internal sealed class SurvivorsHitscanWeaponRuntime : SurvivorsWeaponRuntimeBase
    {
        private const float TempestPrismSideBeamDegrees = 22f;
        private const float TempestPrismSideBeamRangeMultiplier = 0.9f;
        private const float TempestPrismSideBeamWidthMultiplier = 0.85f;
        private readonly List<SurvivorsEnemyActor> _targets = new List<SurvivorsEnemyActor>();
        private readonly List<SurvivorsEnemyActor> _beamHits = new List<SurvivorsEnemyActor>();
        private float _cooldownRemaining = 0.2f;

        public SurvivorsHitscanWeaponRuntime(SurvivorsTemplateController controller, SurvivorsWeaponArchetypeDefinition definition)
            : base(controller, definition)
        {
        }

        public override void Tick(float deltaTime)
        {
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
            Vector3 origin = Controller.PlayerPosition + Vector3.up * 0.45f;
            CollectNearestTargets(origin, Definition.Range, Mathf.Max(1, Definition.HitscanCount + Controller.ProjectileChainBonus + Controller.ProjectileForkBonus));
            if (_targets.Count == 0)
            {
                return false;
            }

            bool dealtDamage = false;
            bool tempestPrismActive = IsTempestPrismActive();
            int maxBeamHits = ResolveBeamMaxHits();
            for (int i = 0; i < _targets.Count; i++)
            {
                SurvivorsEnemyActor target = _targets[i];
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                Vector3 targetPoint = target.transform.position + Vector3.up * 0.45f;
                Vector3 beamDirection = ResolveBeamDirection(origin, targetPoint);
                if (UsesPiercingBeam())
                {
                    dealtDamage |= ApplyPiercingBeam(origin, origin + beamDirection * Definition.Range, Definition.HitscanWidth, maxBeamHits);
                }
                else
                {
                    dealtDamage |= ApplyDamage(target);
                    CreateBeamVisual(origin, targetPoint);
                }

                if (tempestPrismActive)
                {
                    dealtDamage |= FireTempestPrismSideBeams(origin, beamDirection, maxBeamHits);
                }
            }

            _targets.Clear();
            if (dealtDamage)
            {
                Controller.RecordHitscanFire();
            }

            return dealtDamage;
        }

        private void CollectNearestTargets(Vector3 origin, float range, int count)
        {
            _targets.Clear();
            IReadOnlyList<SurvivorsEnemyActor> enemies = Controller.ActiveEnemies;
            float rangeSquared = range * range;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                Vector3 offset = enemy.transform.position - origin;
                offset.y = 0f;
                if (offset.sqrMagnitude <= rangeSquared)
                {
                    _targets.Add(enemy);
                }
            }

            _targets.Sort(CompareDistanceToPlayer);
            while (_targets.Count > count)
            {
                _targets.RemoveAt(_targets.Count - 1);
            }
        }

        private bool UsesPiercingBeam()
        {
            return Definition.HitscanPierces || Controller.HitscanPierceBonus > 0 || Controller.ProjectilePierceBonus > 0;
        }

        private bool IsTempestPrismActive()
        {
            return Definition.Id == BasicSurvivorsGame.StarBeamWeaponContentId &&
                Controller.IsEvolutionActive(BasicSurvivorsGame.TempestPrismEvolutionUpgradeId);
        }

        private int ResolveBeamMaxHits()
        {
            return Mathf.Max(1, 1 + Controller.HitscanPierceBonus + Controller.ProjectilePierceBonus);
        }

        private static Vector3 ResolveBeamDirection(Vector3 origin, Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - origin;
            direction.y = 0f;
            return direction.sqrMagnitude <= 0.001f ? Vector3.forward : direction.normalized;
        }

        private bool FireTempestPrismSideBeams(Vector3 origin, Vector3 forward, int maxHits)
        {
            bool dealtDamage = false;
            float range = Mathf.Max(0.5f, Definition.Range * TempestPrismSideBeamRangeMultiplier);
            float width = Mathf.Max(0.05f, Definition.HitscanWidth * TempestPrismSideBeamWidthMultiplier);
            dealtDamage |= FireTempestPrismSideBeam(origin, forward, -TempestPrismSideBeamDegrees, range, width, maxHits);
            dealtDamage |= FireTempestPrismSideBeam(origin, forward, TempestPrismSideBeamDegrees, range, width, maxHits);
            return dealtDamage;
        }

        private bool FireTempestPrismSideBeam(Vector3 origin, Vector3 forward, float degrees, float range, float width, int maxHits)
        {
            Vector3 direction = Quaternion.Euler(0f, degrees, 0f) * forward;
            return ApplyPiercingBeam(origin, origin + direction.normalized * range, width, maxHits);
        }

        private bool ApplyPiercingBeam(Vector3 origin, Vector3 endpoint, float beamWidth, int maxHits)
        {
            CollectPierceHits(origin, endpoint, beamWidth, maxHits);
            bool dealtDamage = false;
            for (int hitIndex = 0; hitIndex < _beamHits.Count; hitIndex++)
            {
                dealtDamage |= ApplyDamage(_beamHits[hitIndex]);
            }

            CreateBeamVisual(origin, endpoint);
            return dealtDamage;
        }

        private void CollectPierceHits(Vector3 origin, Vector3 targetPosition, float beamWidth, int maxHits)
        {
            _beamHits.Clear();
            Vector3 flattenedOrigin = origin;
            flattenedOrigin.y = 0f;
            Vector3 flattenedTarget = targetPosition;
            flattenedTarget.y = 0f;
            Vector3 path = flattenedTarget - flattenedOrigin;
            float length = path.magnitude;
            if (length <= 0.001f)
            {
                return;
            }

            Vector3 direction = path / length;
            IReadOnlyList<SurvivorsEnemyActor> enemies = Controller.ActiveEnemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                Vector3 enemyPosition = enemy.transform.position;
                enemyPosition.y = 0f;
                Vector3 offset = enemyPosition - flattenedOrigin;
                float projection = Vector3.Dot(offset, direction);
                if (projection < -enemy.Radius || projection > length + enemy.Radius)
                {
                    continue;
                }

                Vector3 closest = flattenedOrigin + direction * Mathf.Clamp(projection, 0f, length);
                float hitRadius = beamWidth + enemy.Radius;
                if ((enemyPosition - closest).sqrMagnitude <= hitRadius * hitRadius)
                {
                    _beamHits.Add(enemy);
                }
            }

            _beamHits.Sort((left, right) =>
            {
                float leftDistance = (left.transform.position - flattenedOrigin).sqrMagnitude;
                float rightDistance = (right.transform.position - flattenedOrigin).sqrMagnitude;
                return leftDistance.CompareTo(rightDistance);
            });

            while (_beamHits.Count > maxHits)
            {
                _beamHits.RemoveAt(_beamHits.Count - 1);
            }
        }

        private bool ApplyDamage(SurvivorsEnemyActor enemy)
        {
            if (enemy == null || !enemy.IsAlive)
            {
                return false;
            }

            if (enemy.ApplyDamage(Controller.ResolveWeaponDamage(Definition), Definition.Id) == null)
            {
                return false;
            }

            Controller.RecordHitscanHit();
            return true;
        }

        private void CreateBeamVisual(Vector3 origin, Vector3 endpoint)
        {
            Vector3 beam = endpoint - origin;
            if (beam.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float length = Mathf.Max(0.05f, beam.magnitude);
            GameObject beamObject = SurvivorsVisualUtility.CreatePrimitiveVisual(
                Definition.DisplayName + " Beam",
                PrimitiveType.Cube,
                Definition.Tint,
                Controller.RuntimeWorldRoot);
            beamObject.transform.position = origin + beam * 0.5f;
            beamObject.transform.rotation = Quaternion.LookRotation(beam.normalized, Vector3.up);
            SurvivorsTimedVisual visual = beamObject.AddComponent<SurvivorsTimedVisual>();
            visual.Initialize(new Vector3(Definition.HitscanWidth, Definition.HitscanWidth, length), Definition.HitscanVisualDurationSeconds, Definition.Tint);
        }

        private int CompareDistanceToPlayer(SurvivorsEnemyActor left, SurvivorsEnemyActor right)
        {
            float leftDistance = (left.transform.position - Controller.PlayerPosition).sqrMagnitude;
            float rightDistance = (right.transform.position - Controller.PlayerPosition).sqrMagnitude;
            return leftDistance.CompareTo(rightDistance);
        }
    }

    internal sealed class SurvivorsOrbitWeaponRuntime : SurvivorsWeaponRuntimeBase
    {
        private const int CrimsonAegisMinimumCounterBladeCount = 2;
        private const float CrimsonAegisCounterBladeRatio = 0.5f;
        private const float CrimsonAegisCounterRingRadiusMultiplier = 0.68f;
        private const float CrimsonAegisCounterRingSpeedMultiplier = 1.35f;
        private const float CrimsonAegisCounterBladeDamageMultiplier = 0.7f;
        private readonly List<SurvivorsOrbitBladeActor> _blades = new List<SurvivorsOrbitBladeActor>();
        private float _rotationDegrees;

        public SurvivorsOrbitWeaponRuntime(SurvivorsTemplateController controller, SurvivorsWeaponArchetypeDefinition definition)
            : base(controller, definition)
        {
        }

        public int ActiveBladeCount => _blades.Count;

        public override void Tick(float deltaTime)
        {
            int primaryCount = Mathf.Max(1, Definition.OrbitCount + Controller.OrbitBladeBonus);
            int counterCount = ResolveCrimsonAegisCounterBladeCount(primaryCount);
            int desiredCount = primaryCount + counterCount;
            EnsureBladeCount(desiredCount);
            _rotationDegrees += Definition.OrbitDegreesPerSecond * deltaTime;

            float radius = Definition.OrbitRadius + Controller.OrbitRadiusBonus + Controller.AreaRadiusBonus;
            float hitRadius = Mathf.Max(0.18f, Definition.Range);
            float damage = Controller.ResolveWeaponDamage(Definition);
            UpdateBladeRing(0, primaryCount, radius, hitRadius, damage, Definition.OrbitContactTickIntervalSeconds, _rotationDegrees);
            if (counterCount > 0)
            {
                float counterRadius = Mathf.Max(0.8f, radius * CrimsonAegisCounterRingRadiusMultiplier);
                float counterDamage = damage * CrimsonAegisCounterBladeDamageMultiplier;
                float counterRotation = (_rotationDegrees * -CrimsonAegisCounterRingSpeedMultiplier) + (180f / counterCount);
                UpdateBladeRing(primaryCount, counterCount, counterRadius, hitRadius, counterDamage, Definition.OrbitContactTickIntervalSeconds, counterRotation);
            }
        }

        public override void Dispose()
        {
            for (int i = 0; i < _blades.Count; i++)
            {
                if (_blades[i] != null)
                {
                    SurvivorsVisualUtility.ReleaseTemplateObject(_blades[i].gameObject);
                }
            }

            _blades.Clear();
        }

        private int ResolveCrimsonAegisCounterBladeCount(int primaryCount)
        {
            if (!IsCrimsonAegisActive())
            {
                return 0;
            }

            return Mathf.Max(CrimsonAegisMinimumCounterBladeCount, Mathf.CeilToInt(primaryCount * CrimsonAegisCounterBladeRatio));
        }

        private bool IsCrimsonAegisActive()
        {
            if (Definition.Id != BasicSurvivorsGame.OrbitWardWeaponContentId &&
                Definition.Id != BasicSurvivorsGame.ThornHaloWeaponContentId)
            {
                return false;
            }

            return Controller.IsEvolutionActive(BasicSurvivorsGame.CrimsonAegisEvolutionUpgradeId);
        }

        private void UpdateBladeRing(int bladeOffset, int count, float radius, float hitRadius, float damage, float tickIntervalSeconds, float rotationDegrees)
        {
            if (count <= 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                int bladeIndex = bladeOffset + i;
                if (bladeIndex < 0 || bladeIndex >= _blades.Count)
                {
                    continue;
                }

                float angle = (rotationDegrees + (360f / count) * i) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                SurvivorsOrbitBladeActor blade = _blades[bladeIndex];
                blade.Configure(Controller, Definition, damage, hitRadius, tickIntervalSeconds);
                blade.SetPose(Controller.PlayerPosition + offset + Vector3.up * 0.4f);
                blade.TickContacts(Controller.RunTimeSeconds);
            }
        }

        private void EnsureBladeCount(int desiredCount)
        {
            while (_blades.Count < desiredCount)
            {
                GameObject bladeObject = SurvivorsVisualUtility.CreatePrimitiveVisual(
                    Definition.DisplayName + " Blade",
                    PrimitiveType.Sphere,
                    Definition.Tint,
                    Controller.RuntimeWorldRoot);
                SurvivorsOrbitBladeActor blade = bladeObject.AddComponent<SurvivorsOrbitBladeActor>();
                _blades.Add(blade);
            }

            while (_blades.Count > desiredCount)
            {
                SurvivorsOrbitBladeActor blade = _blades[_blades.Count - 1];
                _blades.RemoveAt(_blades.Count - 1);
                if (blade != null)
                {
                    SurvivorsVisualUtility.ReleaseTemplateObject(blade.gameObject);
                }
            }
        }
    }

    internal sealed class SurvivorsMeleeWeaponRuntime : SurvivorsWeaponRuntimeBase
    {
        private const float EclipseWaltzBackSweepDamageMultiplier = 0.85f;
        private readonly List<SurvivorsEnemyActor> _candidates = new List<SurvivorsEnemyActor>();
        private float _cooldownRemaining;

        public SurvivorsMeleeWeaponRuntime(SurvivorsTemplateController controller, SurvivorsWeaponArchetypeDefinition definition)
            : base(controller, definition)
        {
        }

        public override void Tick(float deltaTime)
        {
            _cooldownRemaining -= deltaTime;
            if (_cooldownRemaining > 0f)
            {
                return;
            }

            if (TrySwing())
            {
                _cooldownRemaining = Controller.ResolveWeaponCooldownSeconds(Definition);
            }
        }

        public override bool FireForTest()
        {
            _cooldownRemaining = 0f;
            return TrySwing();
        }

        private bool TrySwing()
        {
            SurvivorsEnemyActor anchor = Controller.FindNearestEnemy(Controller.PlayerPosition, Definition.Range);
            if (anchor == null)
            {
                return false;
            }

            Vector3 facing = anchor.transform.position - Controller.PlayerPosition;
            facing.y = 0f;
            if (facing.sqrMagnitude <= 0.001f)
            {
                facing = Controller.PlayerForward;
            }

            facing.Normalize();
            float halfArc = Mathf.Clamp(Definition.MeleeArcDegrees * 0.5f, 1f, 180f);
            float range = Definition.Range + Controller.AreaRadiusBonus;
            float rangeSquared = range * range;
            int maxTargets = Mathf.Max(1, Definition.MeleeHitCount + Controller.MeleeTargetBonus);
            float damage = Controller.ResolveWeaponDamage(Definition);
            int hitCount = ApplySweep(facing, halfArc, rangeSquared, range, maxTargets, damage);
            if (IsEclipseWaltzActive())
            {
                hitCount += ApplySweep(-facing, halfArc, rangeSquared, range, maxTargets, damage * EclipseWaltzBackSweepDamageMultiplier);
            }

            if (hitCount <= 0)
            {
                return false;
            }

            Controller.RecordMeleeSwing();
            return true;
        }

        private bool IsEclipseWaltzActive()
        {
            return Definition.Id == BasicSurvivorsGame.MoonSlashWeaponContentId &&
                Controller.IsEvolutionActive(BasicSurvivorsGame.EclipseWaltzEvolutionUpgradeId);
        }

        private int ApplySweep(Vector3 facing, float halfArc, float rangeSquared, float range, int maxTargets, float damage)
        {
            _candidates.Clear();
            IReadOnlyList<SurvivorsEnemyActor> enemies = Controller.ActiveEnemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                Vector3 offset = enemy.transform.position - Controller.PlayerPosition;
                offset.y = 0f;
                if (offset.sqrMagnitude > rangeSquared)
                {
                    continue;
                }

                if (Vector3.Angle(facing, offset.normalized) > halfArc)
                {
                    continue;
                }

                _candidates.Add(enemy);
            }

            if (_candidates.Count == 0)
            {
                return 0;
            }

            _candidates.Sort(CompareByDistanceToPlayer);
            int hitCount = 0;
            for (int i = 0; i < _candidates.Count && hitCount < maxTargets; i++)
            {
                SurvivorsEnemyActor enemy = _candidates[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                if (enemy.ApplyDamage(damage, Definition.Id) != null)
                {
                    Controller.RecordMeleeHit();
                    hitCount++;
                }
            }

            _candidates.Clear();
            if (hitCount <= 0)
            {
                return 0;
            }

            CreateSlashVisual(facing, range);
            return hitCount;
        }

        private void CreateSlashVisual(Vector3 facing, float range)
        {
            GameObject slashObject = SurvivorsVisualUtility.CreatePrimitiveVisual(
                Definition.DisplayName + " Slash",
                PrimitiveType.Cube,
                Definition.Tint,
                Controller.RuntimeWorldRoot);
            slashObject.transform.position = Controller.PlayerPosition + Vector3.up * 0.35f;
            slashObject.transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
            SurvivorsTimedVisual visual = slashObject.AddComponent<SurvivorsTimedVisual>();
            visual.Initialize(new Vector3(range * 2f, 0.12f, Mathf.Max(0.45f, range * 0.85f)), Definition.MeleeVisualDurationSeconds, Definition.Tint);
        }

        private int CompareByDistanceToPlayer(SurvivorsEnemyActor left, SurvivorsEnemyActor right)
        {
            float leftDistance = (left.transform.position - Controller.PlayerPosition).sqrMagnitude;
            float rightDistance = (right.transform.position - Controller.PlayerPosition).sqrMagnitude;
            return leftDistance.CompareTo(rightDistance);
        }
    }

    internal sealed class SurvivorsBurstWeaponRuntime : SurvivorsWeaponRuntimeBase
    {
        private const int InfernoHeartSatelliteBurstCount = 6;
        private const float InfernoHeartSatelliteDistanceRatio = 0.86f;
        private const float InfernoHeartSatelliteRadiusRatio = 0.48f;
        private readonly List<SurvivorsEnemyActor> _targets = new List<SurvivorsEnemyActor>();
        private float _cooldownRemaining;
        private float _timeUntilNextBurst;
        private int _pendingBursts;
        private Vector3 _sequenceOrigin;

        public SurvivorsBurstWeaponRuntime(SurvivorsTemplateController controller, SurvivorsWeaponArchetypeDefinition definition)
            : base(controller, definition)
        {
        }

        public override void Tick(float deltaTime)
        {
            if (_pendingBursts > 0)
            {
                ProcessPendingBursts(deltaTime);
            }

            _cooldownRemaining -= deltaTime;
            if (_pendingBursts > 0 || _cooldownRemaining > 0f)
            {
                return;
            }

            if (TryStartBurstSequence())
            {
                _cooldownRemaining = Controller.ResolveWeaponCooldownSeconds(Definition);
                ProcessPendingBursts(0f);
            }
        }

        public override bool FireForTest()
        {
            _cooldownRemaining = 0f;
            if (!TryStartBurstSequence())
            {
                return false;
            }

            ProcessPendingBursts(0f);
            return true;
        }

        private bool TryStartBurstSequence()
        {
            SurvivorsEnemyActor anchor = Controller.FindNearestEnemy(Controller.PlayerPosition, Definition.Range);
            if (anchor == null)
            {
                return false;
            }

            _sequenceOrigin = Controller.TargetedBurstSigilBonus > 0 ? anchor.transform.position : Controller.PlayerPosition;
            _pendingBursts = Mathf.Max(1, Definition.BurstCount + Controller.BurstCountBonus + Controller.BurstEchoBonus);
            _timeUntilNextBurst = 0f;
            return true;
        }

        private void ProcessPendingBursts(float deltaTime)
        {
            _timeUntilNextBurst -= deltaTime;
            while (_pendingBursts > 0 && _timeUntilNextBurst <= 0f)
            {
                float radius = Definition.Range + Controller.AreaRadiusBonus;
                EmitBurst(_sequenceOrigin, radius);
                if (Definition.Id == BasicSurvivorsGame.StarNovaWeaponContentId &&
                    Controller.IsEvolutionActive(BasicSurvivorsGame.InfernoHeartEvolutionUpgradeId))
                {
                    EmitInfernoHeartSatellites(_sequenceOrigin, radius);
                }

                _pendingBursts--;
                if (_pendingBursts > 0)
                {
                    _timeUntilNextBurst += Definition.BurstRepeatIntervalSeconds;
                }
            }
        }

        private void EmitInfernoHeartSatellites(Vector3 origin, float baseRadius)
        {
            float satelliteDistance = Mathf.Max(0.25f, baseRadius * InfernoHeartSatelliteDistanceRatio);
            float satelliteRadius = Mathf.Max(0.35f, baseRadius * InfernoHeartSatelliteRadiusRatio);
            for (int index = 0; index < InfernoHeartSatelliteBurstCount; index++)
            {
                float angle = (360f / InfernoHeartSatelliteBurstCount) * index;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * (Vector3.right * satelliteDistance);
                EmitBurst(origin + offset, satelliteRadius);
            }
        }

        private void EmitBurst(Vector3 origin, float radius)
        {
            Controller.CollectEnemiesWithinRadius(origin, radius, _targets);
            float damage = Controller.ResolveWeaponDamage(Definition);
            for (int i = 0; i < _targets.Count; i++)
            {
                SurvivorsEnemyActor enemy = _targets[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                var damageResult = enemy.ApplyDamage(damage, Definition.Id);
                if (damageResult != null)
                {
                    Controller.RecordBurstHit();
                    Controller.ApplyWeaponStatusEffectsToEnemy(enemy, Definition, damageResult);
                }
            }

            _targets.Clear();
            GameObject pulseObject = SurvivorsVisualUtility.CreatePrimitiveVisual(
                Definition.DisplayName + " Pulse",
                PrimitiveType.Cylinder,
                Definition.Tint,
                Controller.RuntimeWorldRoot);
            pulseObject.transform.position = origin + Vector3.up * 0.05f;
            SurvivorsTimedVisual visual = pulseObject.AddComponent<SurvivorsTimedVisual>();
            visual.Initialize(new Vector3(radius * 2f, 0.08f, radius * 2f), Definition.BurstVisualDurationSeconds, Definition.Tint);
            Controller.RecordBurstPulse();
        }
    }

    public sealed class SurvivorsOrbitBladeActor : MonoBehaviour
    {
        private readonly Dictionary<int, float> _nextHitTimeByEnemy = new Dictionary<int, float>();
        private SurvivorsTemplateController _controller;
        private SurvivorsWeaponArchetypeDefinition _definition;
        private float _damage;
        private float _hitRadius;
        private float _tickInterval;

        public void Configure(SurvivorsTemplateController controller, SurvivorsWeaponArchetypeDefinition definition, float damage, float hitRadius, float tickInterval)
        {
            _controller = controller;
            _definition = definition;
            _damage = Mathf.Max(0f, damage);
            _hitRadius = Mathf.Max(0.05f, hitRadius);
            _tickInterval = Mathf.Max(0.05f, tickInterval);
            transform.localScale = Vector3.one * Mathf.Max(0.25f, _hitRadius * 2f);
        }

        public void SetPose(Vector3 position)
        {
            transform.position = position;
        }

        public void TickContacts(float timeSeconds)
        {
            if (_controller == null || _definition == null)
            {
                return;
            }

            IReadOnlyList<SurvivorsEnemyActor> enemies = _controller.ActiveEnemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                SurvivorsEnemyActor enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                int key = enemy.GetInstanceID();
                if (_nextHitTimeByEnemy.TryGetValue(key, out float nextAllowed) && timeSeconds < nextAllowed)
                {
                    continue;
                }

                float combinedRadius = _hitRadius + enemy.Radius;
                if ((enemy.transform.position - transform.position).sqrMagnitude > combinedRadius * combinedRadius)
                {
                    continue;
                }

                if (enemy.ApplyDamage(_damage, _definition.Id) != null)
                {
                    _nextHitTimeByEnemy[key] = timeSeconds + _tickInterval;
                    _controller.RecordOrbitHit();
                }
            }
        }
    }

    public sealed class SurvivorsTimedVisual : MonoBehaviour
    {
        private Renderer _renderer;
        private Material _material;
        private float _lifetime;
        private float _elapsed;
        private Color _tint;
        private Vector3 _targetScale;

        public void Initialize(Vector3 targetScale, float lifetimeSeconds, Color tint)
        {
            _renderer = GetComponentInChildren<Renderer>();
            _targetScale = targetScale;
            _lifetime = Mathf.Max(0.03f, lifetimeSeconds);
            _elapsed = 0f;
            _tint = tint;
            transform.localScale = _targetScale * 0.35f;
            if (_renderer != null)
            {
                _material = _renderer.sharedMaterial;
                _material.color = _tint;
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _lifetime);
            transform.localScale = Vector3.Lerp(_targetScale * 0.35f, _targetScale, progress);
            Color faded = _tint;
            faded.a = Mathf.Lerp(_tint.a, 0f, progress);
            if (_material != null)
            {
                _material.color = faded;
            }

            if (_elapsed >= _lifetime)
            {
                SurvivorsVisualUtility.ReleaseTemplateObject(gameObject);
            }
        }
    }

    internal static class SurvivorsVisualUtility
    {
        public static GameObject CreatePrimitiveVisual(string name, PrimitiveType primitive, Color color, Transform parent)
        {
            GameObject visual = GameObject.CreatePrimitive(primitive);
            visual.name = name;
            if (parent != null)
            {
                visual.transform.SetParent(parent, false);
            }

            Collider collider = visual.GetComponent<Collider>();
            if (collider != null)
            {
                ReleaseTemplateObject(collider);
            }

            ApplyColor(visual.GetComponentInChildren<Renderer>(), color);
            return visual;
        }

        public static void ApplyColor(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            renderer.sharedMaterial = new Material(shader);
            renderer.sharedMaterial.color = color;
        }

        public static void ReleaseTemplateObject(UnityEngine.Object target)
        {
            UnityObjectUtility.DestroySafely(target);
        }
    }
}
