using System;
using System.Collections.Generic;
using System.Linq;
using Deucarian.WorldSpawning;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsRuntimeWorldTests
    {
        [Test]
        public void HierarchyRetainsPlayerPrefabColliderTrailAndLightingConfiguration()
        {
            var parent = new GameObject("runtime-world-test-parent");
            parent.transform.position = new Vector3(2f, 3f, 4f);
            var world = new SurvivorsRuntimeWorld(parent.transform, Palette());
            try
            {
                Assert.That(world.Root.name, Is.EqualTo("SurvivorsRuntimeWorld"));
                Assert.That(world.Root.parent, Is.EqualTo(parent.transform));
                Assert.That(world.PrefabRoot.name, Is.EqualTo("SurvivorsTemplatePrefabSources"));
                Assert.That(world.PrefabRoot.gameObject.activeSelf, Is.False);
                Assert.That(world.Player.name, Is.EqualTo("Survivors Player"));
                Assert.That(world.Player.transform.position, Is.EqualTo(parent.transform.position));
                Assert.That(world.Player.transform.localScale, Is.EqualTo(new Vector3(0.85f, 1f, 0.85f)));
                Assert.That(world.Player.GetComponent<Collider>().enabled, Is.True);
                Assert.That(world.PrefabRoot.childCount, Is.EqualTo(6));
                foreach (Transform prefab in world.PrefabRoot)
                {
                    Assert.That(prefab.gameObject.activeSelf, Is.False);
                    Assert.That(prefab.localScale, Is.EqualTo(Vector3.one));
                    Assert.That(prefab.GetComponent<Collider>().enabled, Is.True);
                }
                Assert.That(world.EnemyPrefab.GetComponent<SurvivorsEnemyActor>(), Is.Not.Null);
                Assert.That(world.ExperiencePrefab.GetComponent<SurvivorsPickupActor>(), Is.Not.Null);
                Assert.That(world.ProjectilePrefab.GetComponent<SurvivorsProjectileActor>(), Is.Not.Null);
                TrailRenderer trail = world.ProjectilePrefab.GetComponent<TrailRenderer>();
                Assert.That(trail.time, Is.EqualTo(0.18f));
                Assert.That(trail.startWidth, Is.EqualTo(0.2f));
                Assert.That(trail.endWidth, Is.EqualTo(0.02f));
                // TrailRenderer exposes the assigned tint after its Color32 channel quantization.
                float expectedTrailAlpha = ((Color)(Color32)new Color(1f, 1f, 1f, 0.95f)).a;
                Assert.That(trail.startColor.a, Is.EqualTo(expectedTrailAlpha));
                Assert.That(trail.endColor.a, Is.Zero);
                Light light = world.Root.GetComponentInChildren<Light>();
                Assert.That(light.name, Is.EqualTo("Survivors Directional Light"));
                Assert.That(light.type, Is.EqualTo(LightType.Directional));
                Assert.That(light.intensity, Is.EqualTo(1.15f));
                Assert.That(light.shadows, Is.EqualTo(LightShadows.Soft));
                Assert.That(Quaternion.Angle(light.transform.localRotation, Quaternion.Euler(52f, -34f, 0f)), Is.LessThan(0.001f));
            }
            finally { world.Dispose(); Object.DestroyImmediate(parent); }
        }

        [Test]
        public void RecolorPreservesMaterialsAndEnemyBaseTintThenDisposalReleasesOnlyOwnedResources()
        {
            var parent = new GameObject("runtime-world-test-parent");
            var borrowed = new GameObject("borrowed-child");
            borrowed.transform.SetParent(parent.transform, false);
            var world = new SurvivorsRuntimeWorld(parent.transform, Palette());
            Material[] materials = Materials(world.Root);
            GameObject generatedRoot = world.Root.gameObject;
            Material playerMaterial = world.PlayerRenderer.sharedMaterial;
            Material enemyMaterial = world.EnemyPrefab.GetComponent<Renderer>().sharedMaterial;
            try
            {
                var replacement = new SurvivorsRuntimeWorldPalette(Color.red, Color.green, Color.blue, Color.yellow,
                    Color.cyan, Color.magenta, Color.white, Color.clear);
                world.ApplyPalette(replacement);
                Assert.That(world.PlayerRenderer.sharedMaterial, Is.SameAs(playerMaterial));
                Assert.That(playerMaterial.color, Is.EqualTo(Color.red));
                Assert.That(world.ExperiencePrefab.GetComponent<Renderer>().sharedMaterial.color, Is.EqualTo(Color.green));
                Assert.That(world.ProjectilePrefab.GetComponent<Renderer>().sharedMaterial.color, Is.EqualTo(Color.magenta));
                Assert.That(enemyMaterial.color, Is.EqualTo(new Color(0.88f, 0.22f, 0.32f)));
                Assert.That(Materials(world.Root), Is.EquivalentTo(materials));
                Assert.That(materials.Length, Is.EqualTo(8));
                world.Dispose();
                world.Dispose();
                Assert.That(generatedRoot == null, Is.True);
                Assert.That(materials.All(material => material == null), Is.True);
                Assert.That(parent != null && borrowed != null, Is.True);
            }
            finally { world.Dispose(); Object.DestroyImmediate(parent); }
        }

        [Test]
        public void SpawnCatalogRetainsAllWarmupCapacitiesAndRoutesRequestsThroughBorrowedPoseResolver()
        {
            var parent = new GameObject("runtime-world-test-parent");
            var poses = new Poses();
            var world = new SurvivorsRuntimeWorld(parent.transform, Palette());
            try
            {
                world.BuildSpawning(poses, 300);
                WorldSpawnSnapshot snapshot = world.Spawning.CreateSnapshot();
                var expected = new Dictionary<WorldSpawnableId, (int initial, int maximum)>
                {
                    [BasicSurvivorsGame.SwarmEnemySpawnableId] = (24, 396),
                    [BasicSurvivorsGame.MinibossEnemySpawnableId] = (1, 8),
                    [BasicSurvivorsGame.BossEnemySpawnableId] = (1, 4),
                    [BasicSurvivorsGame.ExperiencePickupSpawnableId] = (16, 256),
                    [BasicSurvivorsGame.MagnetPickupSpawnableId] = (2, 16),
                    [BasicSurvivorsGame.HealthPickupSpawnableId] = (4, 32),
                    [BasicSurvivorsGame.BloodShardPickupSpawnableId] = (4, 48),
                    [BasicSurvivorsGame.ProjectileSpawnableId] = (12, 96)
                };
                Assert.That(snapshot.Pools.Count, Is.EqualTo(8));
                foreach (SpawnPoolSnapshot pool in snapshot.Pools)
                {
                    Assert.That(pool.PooledCount, Is.EqualTo(expected[pool.SpawnableId].initial));
                    Assert.That(pool.MaximumCapacity, Is.EqualTo(expected[pool.SpawnableId].maximum));
                }
                long sequence = 0;
                foreach (WorldSpawnableId id in expected.Keys)
                {
                    SpawnResult result = world.Spawning.Spawn(Request(id, ++sequence));
                    Assert.That(result.Succeeded, Is.True, result.Message);
                    Assert.That(result.Instance.transform.position, Is.EqualTo(new Vector3(sequence, 0f, 3f)));
                }
                Assert.That(poses.Calls, Is.EqualTo(8));
                Assert.That(world.Spawning.ActiveCount, Is.EqualTo(8));
            }
            finally { world.Dispose(); Object.DestroyImmediate(parent); }
        }

        [Test]
        public void SpawnReleaseKeepsExistingServiceRootDestructionPhaseAndFinalOwnerReleasesMaterials()
        {
            var parent = new GameObject("runtime-world-test-parent");
            var world = new SurvivorsRuntimeWorld(parent.transform, Palette());
            Material[] materials = Materials(world.Root);
            GameObject generatedRoot = world.Root.gameObject;
            try
            {
                world.BuildSpawning(new Poses(), 1);
                Assert.That(world.Spawning.CreateSnapshot().Pools.Single(pool => pool.SpawnableId.Equals(BasicSurvivorsGame.SwarmEnemySpawnableId)).MaximumCapacity, Is.EqualTo(256));
                WorldSpawnService service = world.Spawning;
                world.ReleaseSpawns();
                Assert.That(world.Spawning, Is.Null);
                Assert.That(generatedRoot == null, Is.True, "WorldSpawnService.Dispose already owns destruction of its supplied root.");
                Assert.That(service.Spawn(Request(BasicSurvivorsGame.SwarmEnemySpawnableId, 1)).Succeeded, Is.False);
                Assert.That(materials.All(material => material != null), Is.True, "The world owner releases generated source materials after spawn teardown.");
                Assert.Throws<ObjectDisposedException>(() => world.BuildSpawning(new Poses(), 1));
                world.Dispose();
                Assert.That(materials.All(material => material == null), Is.True);
                Assert.That(parent != null, Is.True);
            }
            finally { world.Dispose(); Object.DestroyImmediate(parent); }
        }

        [Test]
        public void InvalidSpawnDependencyCanRetryWithoutDuplicatingPools()
        {
            var parent = new GameObject("runtime-world-test-parent");
            var world = new SurvivorsRuntimeWorld(parent.transform, Palette());
            try
            {
                Assert.Throws<ArgumentNullException>(() => world.BuildSpawning(null, 1));
                Assert.That(world.Spawning, Is.Null);
                Assert.That(world.Root != null, Is.True);
                world.BuildSpawning(new Poses(), 1);
                Assert.That(world.Spawning.CreateSnapshot().Pools.Count, Is.EqualTo(8));
                Assert.Throws<InvalidOperationException>(() => world.BuildSpawning(new Poses(), 1));
                Assert.That(world.Spawning.CreateSnapshot().Pools.Count, Is.EqualTo(8));
            }
            finally { world.Dispose(); Object.DestroyImmediate(parent); }
        }

        [Test]
        public void ParentLossStillAllowsPooledWorldAndMaterialCleanup()
        {
            var parent = new GameObject("runtime-world-test-parent");
            var world = new SurvivorsRuntimeWorld(parent.transform, Palette());
            Material[] materials = Materials(world.Root);
            try
            {
                world.BuildSpawning(new Poses(), 1);
                world.Spawning.Spawn(Request(BasicSurvivorsGame.ExperiencePickupSpawnableId, 1));
                Object.DestroyImmediate(parent);
                Assert.DoesNotThrow(world.Dispose);
                Assert.That(materials.All(material => material == null), Is.True);
                Assert.That(world.Spawning, Is.Null);
            }
            finally { world.Dispose(); if (parent != null) Object.DestroyImmediate(parent); }
        }

        private static SurvivorsRuntimeWorldPalette Palette() => SurvivorsRuntimeWorldPalette.Capture(new SurvivorsUiTheme());
        private static Material[] Materials(Transform root) => root.GetComponentsInChildren<Renderer>(true)
            .SelectMany(renderer => renderer.sharedMaterials).Where(material => material != null).Distinct().ToArray();
        private static WorldSpawnRequest Request(WorldSpawnableId id, long sequence)
            => new WorldSpawnRequest(id, new WorldSpawnChannelId("test.channel"), sequence, new WorldSpawnRequestContext("test"));
        private sealed class Poses : ISpawnPoseResolver
        {
            internal int Calls;
            public SpawnPoseResult TryResolvePose(WorldSpawnRequest request)
            {
                Calls++;
                return SpawnPoseResult.Success(new SpawnPose(new Vector3(request.Sequence, 0f, 3f), Quaternion.identity));
            }
        }
    }
}
