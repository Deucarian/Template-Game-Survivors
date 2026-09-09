using System;
using System.Collections.Generic;
using System.Reflection;
using Deucarian.Combat;
using Deucarian.WorldSpawning;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    internal sealed class SurvivorsThreatAbilityTestHost : ISurvivorsMajorThreatAbilityPort,
        ISurvivorsEnemySupportSpawnPort, IDisposable
    {
        private readonly List<GameObject> _owned = new List<GameObject>();
        internal readonly List<(SurvivorsEnemyRole role, long seed, float minimum, float maximum, string source)> Requests =
            new List<(SurvivorsEnemyRole, long, float, float, string)>();
        internal readonly List<string> Events = new List<string>();
        internal readonly SurvivorsEnemySupportSpawning Support;
        internal readonly SurvivorsMajorThreatAbilities Abilities;
        internal Func<int, bool> FailSpawn;
        internal Action OnSpawn;
        internal Action<float> DamageEffect;
        internal string LastLabel;
        internal string LastDamageSource;
        internal float LastDamage;
        internal int LastBurst;
        internal float LastTelegraphRadius;
        internal float LastTelegraphDuration;
        public SurvivorsTemplateTuning Tuning { get; set; } = new SurvivorsTemplateTuning();
        public SurvivorsRunState State { get; set; } = SurvivorsRunState.Playing;
        public Vector3 PlayerPosition { get; set; }
        public float CurrentHealth { get; set; } = 100f;
        public float BarrierValue { get; set; } = 20f;
        public int MaximumAlive { get; set; } = 100;
        public int EnemyCount { get; set; }
        public long SpawnSequence { get; set; } = 10;

        internal SurvivorsThreatAbilityTestHost()
        {
            Support = new SurvivorsEnemySupportSpawning(this);
            Abilities = new SurvivorsMajorThreatAbilities(this, Support);
            Tuning.MajorThreatEnrageHealthThreshold = 0.5f;
            Tuning.MajorThreatEnrageEliteSupportCount = 3;
            Tuning.MajorThreatEnrageBossSupportCount = 3;
            Tuning.MajorThreatEnrageExtraAliveAllowance = 0;
            Tuning.MajorThreatEnrageSupportRadius = 2f;
            Tuning.SpawnBandDepth = 3f;
            Tuning.SplitterChildCount = 3;
            Tuning.SplitterChildSpawnRadius = 2f;
            Tuning.SummonerSupportCount = 4;
            Tuning.SummonerSupportExtraAliveAllowance = 0;
            Tuning.SummonerSupportRadius = 2f;
            Tuning.MajorThreatSlamRadius = 2f;
            Tuning.MajorThreatSlamDamage = 10f;
            Tuning.MajorThreatSlamTelegraphSeconds = 0.7f;
            Tuning.PlayerRadius = 0.5f;
        }

        internal SurvivorsEnemyActor Add(SurvivorsEnemyRole role, float health = 100f, float radius = 1f)
        {
            var gameObject = new GameObject("threat-ability-test");
            _owned.Add(gameObject);
            var actor = gameObject.AddComponent<SurvivorsEnemyActor>();
            actor.OnWorldSpawned(new WorldSpawnContext(new SpawnInstanceId(_owned.Count), default, new SpawnPose(Vector3.zero, Quaternion.identity)));
            actor.Initialize(null, new SurvivorsEnemyProfile(role, "test", "Test " + role, 100f, 0f, radius, 0f, 1f, 1, Color.white));
            // Supply an existing health checkpoint without constructing an unrelated full controller.
            typeof(SurvivorsEnemyActor).GetField("_health", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(actor, new HealthState(new CombatantId("enemy.ability.test"), 100f, health,
                    lifeState: health <= 0f ? LifeState.Dead : LifeState.Alive));
            EnemyCount++;
            return actor;
        }

        public bool IsMajorRewardRole(SurvivorsEnemyRole role) => role == SurvivorsEnemyRole.Elite ||
            role == SurvivorsEnemyRole.DreadElite || role == SurvivorsEnemyRole.Miniboss || role == SurvivorsEnemyRole.Boss;
        public string ResolveMajorThreatHealthFallbackLabel(SurvivorsEnemyRole role) => "Fallback " + role;
        public void ApplyDamageToPlayer(float amount, string source)
        {
            Events.Add("damage"); LastDamage = amount; LastDamageSource = source;
            if (DamageEffect != null) DamageEffect(amount); else CurrentHealth -= amount;
        }
        public void RecordStreakRewardFeedback(string label, Color color) { Events.Add("banner"); LastLabel = label; }
        public void RecordMajorThreatSlamTelegraphEffect(Vector3 position, SurvivorsEnemyRole role, float radius, float durationSeconds)
        {
            Events.Add("telegraph"); LastTelegraphRadius = radius; LastTelegraphDuration = durationSeconds;
        }
        public void PlayBossFeedback(Vector3 position, int burstCount) { Events.Add("boss-pulse"); LastBurst = burstCount; }
        public void PlaySupportSpawnFeedback(Vector3 position, int burstCount) { Events.Add("support-pulse"); LastBurst = burstCount; }
        public SurvivorsEnemyActor SpawnGameplayEnemyOffscreen(SurvivorsEnemyRole role, long seed, float minimumDistance, float maximumDistance, string spawnSource)
        {
            Requests.Add((role, seed, minimumDistance, maximumDistance, spawnSource));
            Events.Add("spawn"); SpawnSequence++; OnSpawn?.Invoke();
            return FailSpawn?.Invoke(Requests.Count) == true ? null : Add(role);
        }
        public void Dispose()
        {
            foreach (var gameObject in _owned) if (gameObject != null) UnityEngine.Object.DestroyImmediate(gameObject);
            _owned.Clear();
        }
    }
}
