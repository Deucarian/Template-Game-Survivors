using System;
using System.Collections.Generic;
using Deucarian.Common;
using Deucarian.WorldSpawning;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns one run's generated hierarchy, prefab materials and pooled spawn service.</summary>
    internal sealed class SurvivorsRuntimeWorld : IDisposable
    {
        private readonly List<Material> _materials = new List<Material>();
        private bool _rootReleasedBySpawning;
        internal Transform Root { get; private set; }
        internal Transform PrefabRoot { get; private set; }
        internal GameObject Player { get; private set; }
        internal Renderer PlayerRenderer { get; private set; }
        internal GameObject EnemyPrefab { get; private set; }
        internal GameObject ExperiencePrefab { get; private set; }
        internal GameObject MagnetPrefab { get; private set; }
        internal GameObject HealthPrefab { get; private set; }
        internal GameObject BloodShardPrefab { get; private set; }
        internal GameObject ProjectilePrefab { get; private set; }
        internal WorldSpawnService Spawning { get; private set; }

        internal SurvivorsRuntimeWorld(Transform parent, SurvivorsRuntimeWorldPalette palette)
        {
            try
            {
                Root = new GameObject("SurvivorsRuntimeWorld").transform;
                Root.SetParent(parent, false);
                PrefabRoot = new GameObject("SurvivorsTemplatePrefabSources").transform;
                PrefabRoot.SetParent(Root, false);
                PrefabRoot.gameObject.SetActive(false);
                Player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Player.name = "Survivors Player";
                Player.transform.SetParent(Root, false);
                Player.transform.localScale = new Vector3(0.85f, 1f, 0.85f);
                PlayerRenderer = Player.GetComponentInChildren<Renderer>();
                OwnColor(PlayerRenderer, palette.Player);

                EnemyPrefab = CreatePrimitivePrefab("Survivors Swarm Enemy Prefab", PrimitiveType.Capsule,
                    new Color(0.88f, 0.22f, 0.32f), typeof(SurvivorsEnemyActor), palette);
                ExperiencePrefab = CreatePrimitivePrefab("Survivors XP Gem Prefab", PrimitiveType.Sphere,
                    palette.Experience, typeof(SurvivorsPickupActor), palette);
                MagnetPrefab = CreatePrimitivePrefab("Survivors Magnet Prefab", PrimitiveType.Sphere,
                    palette.Magnet, typeof(SurvivorsPickupActor), palette);
                HealthPrefab = CreatePrimitivePrefab("Survivors Vital Shard Prefab", PrimitiveType.Sphere,
                    palette.Health, typeof(SurvivorsPickupActor), palette);
                BloodShardPrefab = CreatePrimitivePrefab("Survivors Blood Shard Prefab", PrimitiveType.Sphere,
                    palette.BloodShard, typeof(SurvivorsPickupActor), palette);
                ProjectilePrefab = CreatePrimitivePrefab("Survivors Arcane Bolt Prefab", PrimitiveType.Sphere,
                    palette.Projectile, typeof(SurvivorsProjectileActor), palette);
                BuildSceneLighting();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        internal void BuildSpawning(ISpawnPoseResolver poses, int enemyMaximumAlive)
        {
            if (Root == null || _rootReleasedBySpawning) throw new ObjectDisposedException(nameof(SurvivorsRuntimeWorld));
            if (Spawning != null) throw new InvalidOperationException("This run's spawn service is already built.");
            var spawnables = new[]
            {
                new SpawnableDefinition(BasicSurvivorsGame.SwarmEnemySpawnableId, new GameObjectPrefabProvider(EnemyPrefab), 24, Mathf.Max(enemyMaximumAlive + 96, 256), "survivors-enemy-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.MinibossEnemySpawnableId, new GameObjectPrefabProvider(EnemyPrefab), 1, 8, "survivors-miniboss-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.BossEnemySpawnableId, new GameObjectPrefabProvider(EnemyPrefab), 1, 4, "survivors-boss-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.ExperiencePickupSpawnableId, new GameObjectPrefabProvider(ExperiencePrefab), 16, 256, "survivors-xp-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.MagnetPickupSpawnableId, new GameObjectPrefabProvider(MagnetPrefab), 2, 16, "survivors-magnet-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.HealthPickupSpawnableId, new GameObjectPrefabProvider(HealthPrefab), 4, 32, "survivors-health-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.BloodShardPickupSpawnableId, new GameObjectPrefabProvider(BloodShardPrefab), 4, 48, "survivors-blood-shard-pool"),
                new SpawnableDefinition(BasicSurvivorsGame.ProjectileSpawnableId, new GameObjectPrefabProvider(ProjectilePrefab), 12, 96, "survivors-projectile-pool")
            };
            try
            {
                Spawning = new WorldSpawnService(new SpawnableCatalog(spawnables), poses, Root, rootName: "SurvivorsWorldSpawning");
                Spawning.Warmup();
            }
            catch
            {
                ReleaseSpawns();
                throw;
            }
        }

        private GameObject CreatePrimitivePrefab(string name, PrimitiveType primitive, Color color, Type actorType,
            SurvivorsRuntimeWorldPalette palette)
        {
            GameObject prefab = GameObject.CreatePrimitive(primitive);
            prefab.name = name;
            prefab.transform.SetParent(PrefabRoot, false);
            prefab.transform.localScale = Vector3.one;
            OwnColor(prefab.GetComponentInChildren<Renderer>(), color);
            prefab.AddComponent(actorType);
            if (actorType == typeof(SurvivorsProjectileActor))
            {
                var trail = prefab.AddComponent<TrailRenderer>();
                trail.time = 0.18f;
                trail.startWidth = 0.2f;
                trail.endWidth = 0.02f;
                var material = new Material(Shader.Find("Sprites/Default"));
                _materials.Add(material);
                trail.material = material;
                trail.startColor = palette.TrailStart;
                trail.endColor = palette.TrailEnd;
            }
            prefab.SetActive(false);
            return prefab;
        }

        private void OwnColor(Renderer renderer, Color color)
            => _materials.Add(SurvivorsPrimitivePresentation.ApplyColor(renderer, color));

        private void BuildSceneLighting()
        {
            GameObject lightObject = new GameObject("Survivors Directional Light");
            lightObject.transform.SetParent(Root, false);
            lightObject.transform.localRotation = Quaternion.Euler(52f, -34f, 0f);
            Light directionalLight = lightObject.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.color = new Color(1f, 0.96f, 0.86f);
            directionalLight.intensity = 1.15f;
            directionalLight.shadows = LightShadows.Soft;
        }

        internal void ApplyPalette(SurvivorsRuntimeWorldPalette palette)
        {
            if (Root == null) return;
            SurvivorsPrimitivePresentation.SetRendererColor(PlayerRenderer, palette.Player);
            SetPrefabColor(ExperiencePrefab, palette.Experience);
            SetPrefabColor(MagnetPrefab, palette.Magnet);
            SetPrefabColor(HealthPrefab, palette.Health);
            SetPrefabColor(BloodShardPrefab, palette.BloodShard);
            SetPrefabColor(ProjectilePrefab, palette.Projectile);
            TrailRenderer trail = ProjectilePrefab == null ? null : ProjectilePrefab.GetComponent<TrailRenderer>();
            if (trail != null)
            {
                trail.startColor = palette.TrailStart;
                trail.endColor = palette.TrailEnd;
            }
        }

        private static void SetPrefabColor(GameObject prefab, Color color)
            => SurvivorsPrimitivePresentation.SetRendererColor(prefab == null ? null : prefab.GetComponentInChildren<Renderer>(), color);

        internal void ReleaseSpawns()
        {
            if (Spawning == null) return;
            // WorldSpawnService owns destruction of its supplied root as well as its pools.
            Spawning.Dispose();
            Spawning = null;
            _rootReleasedBySpawning = true;
        }

        public void Dispose()
        {
            ReleaseSpawns();
            if (!_rootReleasedBySpawning && Root != null) UnityObjectUtility.DestroySafely(Root.gameObject);
            foreach (Material material in _materials) UnityObjectUtility.DestroySafely(material);
            _materials.Clear();
            Root = null;
            PrefabRoot = null;
            Player = null;
            PlayerRenderer = null;
            EnemyPrefab = null;
            ExperiencePrefab = null;
            MagnetPrefab = null;
            HealthPrefab = null;
            BloodShardPrefab = null;
            ProjectilePrefab = null;
        }
    }
}
