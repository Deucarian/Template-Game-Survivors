using System;
using System.Collections.Generic;
using Deucarian.WorldSpawning;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    internal sealed class SurvivorsSpawnCommandTestHost : ISurvivorsEnemySpawnPort, ISurvivorsPickupSpawnPort, ISurvivorsProjectileLaunchPort, IDisposable
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        public readonly SurvivorsSpawnSequence Sequence = new SurvivorsSpawnSequence();
        public readonly SurvivorsEnemySpawner Enemies;
        public readonly SurvivorsPickupSpawner Pickups;
        public readonly SurvivorsProjectileLauncher Projectiles;
        public readonly List<string> Events = new List<string>();
        public readonly List<WorldSpawnRequest> Requests = new List<WorldSpawnRequest>();
        public readonly Dictionary<long, Vector3> Poses = new Dictionary<long, Vector3>();
        public bool HasSpawnService { get; set; } = true;
        public bool FailRequests;
        public bool ReturnMissingInstance;
        public SurvivorsTemplateTuning Tuning { get; } = new SurvivorsTemplateTuning { EnemySpawnRadius = 10f, SpawnBandDepth = 5f, PickupCollectRadius = 0.7f };
        public SurvivorsRunFlowRuntime RunFlow { get; set; }
        public Vector3 PlayerPosition => new Vector3(1f, 0f, 2f);
        public float CurrentPickupAttractRange { get; set; } = 5.5f;
        public float CurrentPickupAttractionSpeed { get; set; } = 7.5f;
        public int ProjectileChainBonus => 2;
        public int ProjectilePierceBonus => 3;
        public int ProjectileForkBonus => 4;
        public int ProjectileReturnBonus => 5;
        public float WeaponDamage = 19f;
        public Vector3 LastPlacementCenter;
        public float LastMinimum, LastMaximum, LastPadding, LastBandDepth;
        public long LastPlacementSeed;
        public int PlacementCalls;
        public SurvivorsEnemyProfile LastEnemyProfile;
        public SurvivorsPickupKind LastPickupKind;
        public int LastPickupAmount;
        public float LastPickupRange, LastPickupSpeed, LastPickupRadius;
        public SurvivorsProjectileLaunchValues LastLaunch;
        public Vector3 LastLaunchFeedbackPosition;

        public SurvivorsSpawnCommandTestHost()
        {
            Enemies = new SurvivorsEnemySpawner(this, Sequence);
            Pickups = new SurvivorsPickupSpawner(this, Sequence);
            Projectiles = new SurvivorsProjectileLauncher(this, Sequence);
        }
        public void RegisterExplicitPose(long sequence, Vector3 position) { Poses.Add(sequence, position); Events.Add("pose"); }
        public SpawnResult Spawn(WorldSpawnRequest request)
        {
            Events.Add("spawn");
            Requests.Add(request);
            if (FailRequests) return new SpawnResult(false, SpawnFailureReason.CapacityExhausted, default, null, request);
            if (ReturnMissingInstance) return new SpawnResult(true, SpawnFailureReason.None, new SpawnInstanceId(request.Sequence), null, request);
            var instance = new GameObject("spawn-command-result");
            instance.SetActive(false);
            _objects.Add(instance);
            instance.transform.position = Poses[request.Sequence];
            if (request.SpawnableId.Equals(BasicSurvivorsGame.ProjectileSpawnableId)) instance.AddComponent<SurvivorsProjectileActor>();
            else if (request.SpawnableId.Equals(BasicSurvivorsGame.SwarmEnemySpawnableId) ||
                request.SpawnableId.Equals(BasicSurvivorsGame.MinibossEnemySpawnableId) ||
                request.SpawnableId.Equals(BasicSurvivorsGame.BossEnemySpawnableId)) instance.AddComponent<SurvivorsEnemyActor>();
            else instance.AddComponent<SurvivorsPickupActor>();
            return new SpawnResult(true, SpawnFailureReason.None, new SpawnInstanceId(request.Sequence), instance, request);
        }
        public float ResolveGameplaySpawnMinimumDistance(SurvivorsEnemyRole role, float requested) => requested + 2f;
        public float ResolveGameplaySpawnMaximumDistance(SurvivorsEnemyRole role, float minimum, float maximum) => maximum + 3f;
        public float ResolveOffscreenSpawnPadding(SurvivorsEnemyRole role, string source) => 4f;
        public Vector3 ResolveSafeOffscreenPosition(Vector3 center, float minimum, float maximum, long seed, float padding, float bandDepth)
        {
            LastPlacementCenter = center; LastMinimum = minimum; LastMaximum = maximum;
            LastPlacementSeed = seed; LastPadding = padding; LastBandDepth = bandDepth; PlacementCalls++;
            Events.Add("placement");
            return new Vector3(20f, 0f, 30f);
        }
        public void InitializeEnemy(SurvivorsEnemyActor enemy, SurvivorsEnemyProfile profile) { LastEnemyProfile = profile; Events.Add("initialize.enemy"); }
        public void RegisterEnemy(SurvivorsEnemyActor enemy) => Events.Add("register.enemy:" + Enemies.SpawnedCount);
        public void RecordGameplaySpawnSafety(SurvivorsEnemyRole role, Vector3 position, string source) => Events.Add("safety:" + Enemies.SpawnedCount);
        public void RecordSpawnMetric(SurvivorsEnemyRole role) => Events.Add("metric:" + role + ":" + Enemies.MinibossSpawnCount + ":" + Enemies.BossSpawnCount);
        public void ShowEnemySpawnFeedback(bool major, Vector3 position, int burst) => Events.Add("feedback:" + burst + ":" + Enemies.SpawnedCount);
        public void InitializePickup(SurvivorsPickupActor pickup, SurvivorsPickupKind kind, int amount, float range, float speed, float radius)
        {
            LastPickupKind = kind; LastPickupAmount = amount; LastPickupRange = range; LastPickupSpeed = speed; LastPickupRadius = radius;
            Events.Add("initialize.pickup");
        }
        public void RegisterPickup(SurvivorsPickupActor pickup) => Events.Add("register.pickup");
        public float ResolveWeaponDamage(SurvivorsWeaponArchetypeDefinition definition) { Events.Add("damage"); return WeaponDamage; }
        public void InitializeProjectile(SurvivorsProjectileActor projectile, SurvivorsWeaponArchetypeDefinition definition, SurvivorsProjectileLaunchValues values)
        { LastLaunch = values; Events.Add("initialize.projectile"); }
        public void RegisterProjectile(SurvivorsProjectileActor projectile) => Events.Add("register.projectile:" + Projectiles.ProjectileLaunchCount);
        public void ShowProjectileLaunchFeedback(Vector3 origin) { LastLaunchFeedbackPosition = origin; Events.Add("launch:" + Projectiles.ProjectileLaunchCount); }
        public void Dispose() { foreach (GameObject instance in _objects) UnityEngine.Object.DestroyImmediate(instance); }
    }
}
