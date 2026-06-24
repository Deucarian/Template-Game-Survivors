using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    public enum SurvivorsWeaponArchetype
    {
        Projectile = 0,
        Orbit = 1,
        Melee = 2,
        Burst = 3,
        Hitscan = 4
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
            int orbitCount = 1,
            float orbitRadius = 0f,
            float orbitDegreesPerSecond = 0f,
            float orbitContactTickIntervalSeconds = 0f,
            int meleeHitCount = 1,
            float meleeArcDegrees = 115f,
            float meleeVisualDurationSeconds = 0.16f,
            int burstCount = 1,
            float burstRepeatIntervalSeconds = 0.18f,
            float burstVisualDurationSeconds = 0.22f)
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
    }

    public sealed class SurvivorsWeaponLoadoutRuntime
    {
        private readonly List<SurvivorsWeaponRuntimeBase> _weapons = new List<SurvivorsWeaponRuntimeBase>();

        public SurvivorsWeaponLoadoutRuntime(SurvivorsTemplateController controller, IReadOnlyList<SurvivorsWeaponArchetypeDefinition> definitions)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

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
            }
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

        public void Dispose()
        {
            for (int i = 0; i < _weapons.Count; i++)
            {
                _weapons[i].Dispose();
            }

            _weapons.Clear();
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

            return Controller.LaunchProjectile(Definition, direction.normalized);
        }
    }

    internal sealed class SurvivorsOrbitWeaponRuntime : SurvivorsWeaponRuntimeBase
    {
        private readonly List<SurvivorsOrbitBladeActor> _blades = new List<SurvivorsOrbitBladeActor>();
        private float _rotationDegrees;

        public SurvivorsOrbitWeaponRuntime(SurvivorsTemplateController controller, SurvivorsWeaponArchetypeDefinition definition)
            : base(controller, definition)
        {
        }

        public int ActiveBladeCount => _blades.Count;

        public override void Tick(float deltaTime)
        {
            int desiredCount = Mathf.Max(1, Definition.OrbitCount + Controller.OrbitBladeBonus);
            EnsureBladeCount(desiredCount);
            _rotationDegrees += Definition.OrbitDegreesPerSecond * deltaTime;

            float radius = Definition.OrbitRadius;
            float hitRadius = Mathf.Max(0.18f, Definition.Range);
            float damage = Controller.ResolveWeaponDamage(Definition);
            for (int i = 0; i < _blades.Count; i++)
            {
                float angle = (_rotationDegrees + (360f / _blades.Count) * i) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                SurvivorsOrbitBladeActor blade = _blades[i];
                blade.Configure(Controller, Definition, damage, hitRadius, Definition.OrbitContactTickIntervalSeconds);
                blade.SetPose(Controller.PlayerPosition + offset + Vector3.up * 0.4f);
                blade.TickContacts(Controller.RunTimeSeconds);
            }
        }

        public override void Dispose()
        {
            for (int i = 0; i < _blades.Count; i++)
            {
                if (_blades[i] != null)
                {
                    SurvivorsVisualUtility.DestroyUnityObject(_blades[i].gameObject);
                }
            }

            _blades.Clear();
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
                    SurvivorsVisualUtility.DestroyUnityObject(blade.gameObject);
                }
            }
        }
    }

    internal sealed class SurvivorsMeleeWeaponRuntime : SurvivorsWeaponRuntimeBase
    {
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
            float rangeSquared = Definition.Range * Definition.Range;
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
                return false;
            }

            _candidates.Sort(CompareByDistanceToPlayer);
            int maxTargets = Mathf.Max(1, Definition.MeleeHitCount + Controller.MeleeTargetBonus);
            float damage = Controller.ResolveWeaponDamage(Definition);
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
                return false;
            }

            GameObject slashObject = SurvivorsVisualUtility.CreatePrimitiveVisual(
                Definition.DisplayName + " Slash",
                PrimitiveType.Cube,
                Definition.Tint,
                Controller.RuntimeWorldRoot);
            slashObject.transform.position = Controller.PlayerPosition + Vector3.up * 0.35f;
            slashObject.transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
            SurvivorsTimedVisual visual = slashObject.AddComponent<SurvivorsTimedVisual>();
            visual.Initialize(new Vector3(Definition.Range * 2f, 0.12f, Mathf.Max(0.45f, Definition.Range * 0.85f)), Definition.MeleeVisualDurationSeconds, Definition.Tint);
            Controller.RecordMeleeSwing();
            return true;
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
            if (Controller.FindNearestEnemy(Controller.PlayerPosition, Definition.Range) == null)
            {
                return false;
            }

            _sequenceOrigin = Controller.PlayerPosition;
            _pendingBursts = Mathf.Max(1, Definition.BurstCount + Controller.BurstCountBonus);
            _timeUntilNextBurst = 0f;
            return true;
        }

        private void ProcessPendingBursts(float deltaTime)
        {
            _timeUntilNextBurst -= deltaTime;
            while (_pendingBursts > 0 && _timeUntilNextBurst <= 0f)
            {
                EmitBurst(_sequenceOrigin, Definition.Range);
                _pendingBursts--;
                if (_pendingBursts > 0)
                {
                    _timeUntilNextBurst += Definition.BurstRepeatIntervalSeconds;
                }
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

                if (enemy.ApplyDamage(damage, Definition.Id) != null)
                {
                    Controller.RecordBurstHit();
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
                SurvivorsVisualUtility.DestroyUnityObject(gameObject);
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
                DestroyUnityObject(collider);
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

        public static void DestroyUnityObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }
    }
}
