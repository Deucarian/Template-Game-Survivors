using System;
using System.Collections.Generic;
using System.Reflection;
using Deucarian.Combat;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    internal sealed class SurvivorsEnemyNavigationTestHost : ISurvivorsEnemyNavigationPort, IDisposable
    {
        internal readonly List<SurvivorsEnemyActor> Enemies = new List<SurvivorsEnemyActor>();
        internal readonly List<string> Events = new List<string>();
        internal readonly SurvivorsEnemyNavigation Navigation;
        internal readonly SurvivorsEnemySpatialQueries Queries;
        private readonly List<GameObject> _owned = new List<GameObject>();
        internal Action OnSafety;
        internal Action OnReentry;
        internal Vector3 Destination = new Vector3(-7f, 0f, 8f);
        internal Vector3 LastCenter;
        internal Vector3 LastSafetyPosition;
        internal Vector3 LastReentryDelta;
        internal string LastReentryName;
        internal SurvivorsEnemyRole LastReentryRole;
        internal long LastSeed;
        internal float LastMinimum;
        internal float LastMaximum;
        internal float LastPadding;
        internal float LastDepth;
        public SurvivorsTemplateTuning Tuning { get; set; } = new SurvivorsTemplateTuning();
        public Vector3 PlayerPosition { get; set; }

        internal SurvivorsEnemyNavigationTestHost()
        {
            Navigation = new SurvivorsEnemyNavigation(this);
            Queries = new SurvivorsEnemySpatialQueries(Enemies, () => Tuning);
            Tuning.EnemySoftLeashRadius = 5f;
            Tuning.EnemyHardRecycleRadius = 10f;
            Tuning.EnemyRecycleDelaySeconds = 2f;
            Tuning.EnemyRecycleMinimumRespawnDistance = 12f;
            Tuning.EnemyRecycleMaximumRespawnDistance = 16f;
            Tuning.MajorThreatRepositionRadius = 20f;
            Tuning.MajorThreatRepositionDelaySeconds = 3f;
            Tuning.MajorThreatCatchUpRadius = 15f;
            Tuning.MajorThreatCatchUpSpeedMultiplier = 2.5f;
            Tuning.PlayerRadius = 1f;
            Tuning.SpawnBandDepth = 3f;
        }

        internal SurvivorsEnemyActor Add(Vector3 position, SurvivorsEnemyRole role = SurvivorsEnemyRole.Swarm,
            bool canRecycle = true, bool canLeash = true, bool canReposition = true, float radius = 1f)
        {
            var root = new GameObject("enemy-navigation-test");
            _owned.Add(root);
            var actor = root.AddComponent<SurvivorsEnemyActor>();
            actor.Initialize(null, new SurvivorsEnemyProfile(role, "test", "Test " + role,
                100f, 0f, radius, 0f, 1f, 1, Color.white,
                canRecycle: canRecycle, canLeash: canLeash, canReposition: canReposition));
            root.transform.position = position;
            Enemies.Add(actor);
            return actor;
        }

        internal static void SetCurrentHealth(SurvivorsEnemyActor actor, float health)
        {
            // Set the existing actor checkpoint without requiring a full controller/combat world.
            typeof(SurvivorsEnemyActor).GetField("_health", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(actor, new HealthState(new CombatantId("enemy.navigation.test"), 100f, health,
                    lifeState: health <= 0f ? LifeState.Dead : LifeState.Alive));
        }

        public bool IsMajorRewardRole(SurvivorsEnemyRole role)
            => role == SurvivorsEnemyRole.Elite || role == SurvivorsEnemyRole.DreadElite ||
               role == SurvivorsEnemyRole.Miniboss || role == SurvivorsEnemyRole.Boss;

        public float ResolveOffscreenSpawnPadding(SurvivorsEnemyRole role, string reason)
        {
            Events.Add("padding:" + reason);
            return 2.5f;
        }

        public Vector3 ResolveSafeOffscreenPosition(Vector3 center, float minimumDistance, float maximumDistance,
            long seed, float padding, float bandDepth)
        {
            Events.Add("position");
            LastCenter = center;
            LastMinimum = minimumDistance;
            LastMaximum = maximumDistance;
            LastSeed = seed;
            LastPadding = padding;
            LastDepth = bandDepth;
            return Destination;
        }

        public void RecordGameplaySpawnSafety(SurvivorsEnemyRole role, Vector3 position, string reason)
        {
            Events.Add("safety:" + reason);
            LastSafetyPosition = position;
            OnSafety?.Invoke();
        }

        public void RecordMajorThreatReentry(SurvivorsEnemyRole role, string displayName, Vector3 playerToEnemy)
        {
            Events.Add("reentry");
            LastReentryRole = role;
            LastReentryName = displayName;
            LastReentryDelta = playerToEnemy;
            OnReentry?.Invoke();
        }

        public void Dispose()
        {
            foreach (GameObject item in _owned) if (item != null) UnityEngine.Object.DestroyImmediate(item);
            _owned.Clear();
            Enemies.Clear();
        }
    }
}
