using System;
using System.Collections.Generic;
using Deucarian.WorldSpawning;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    internal sealed class SurvivorsPickupRewardTestHost : ISurvivorsMajorRewardPickupCachePort,
        ISurvivorsPickupCollectionPort, IDisposable
    {
        private readonly List<GameObject> _owned = new List<GameObject>();
        internal readonly List<SurvivorsPickupActor> Pickups = new List<SurvivorsPickupActor>();
        internal readonly List<(SurvivorsPickupKind kind, Vector3 position, int amount)> Requests =
            new List<(SurvivorsPickupKind, Vector3, int)>();
        internal readonly List<string> Events = new List<string>();
        internal readonly SurvivorsMajorRewardPickupCache Cache;
        internal readonly SurvivorsPickupCollection Collection;
        internal Func<int, bool> FailSpawn;
        internal Func<int, bool> InactiveSpawn;
        internal Action OnCollectionFeedback;
        internal Action OnDespawn;
        internal Action OnReward;
        internal int ExperienceResult = 13;
        internal int LastBurst;
        internal string LastLabel;
        internal Color LastColor;
        internal Vector3 LastPosition;
        public SurvivorsTemplateTuning Tuning { get; set; } = new SurvivorsTemplateTuning();
        public int RunEscalationLevel { get; set; }
        public float PickupMagnetPulseIntervalReductionBonus { get; set; }
        public Vector3 PlayerPosition { get; set; }

        internal SurvivorsPickupRewardTestHost()
        {
            Cache = new SurvivorsMajorRewardPickupCache(this);
            Collection = new SurvivorsPickupCollection(Pickups, this);
        }

        internal SurvivorsPickupActor Add(SurvivorsPickupKind kind, int amount = 1, bool withInstanceId = true)
        {
            var gameObject = new GameObject("pickup-reward-test");
            _owned.Add(gameObject);
            var pickup = gameObject.AddComponent<SurvivorsPickupActor>();
            pickup.Initialize(null, kind, amount, 1f, 2f, 0.2f);
            if (withInstanceId)
                pickup.OnWorldSpawned(new WorldSpawnContext(new SpawnInstanceId(_owned.Count), default, new SpawnPose(Vector3.zero, Quaternion.identity)));
            Pickups.Add(pickup);
            return pickup;
        }

        public bool IsMajorRewardRole(SurvivorsEnemyRole role) => role == SurvivorsEnemyRole.Elite ||
            role == SurvivorsEnemyRole.DreadElite || role == SurvivorsEnemyRole.Miniboss || role == SurvivorsEnemyRole.Boss;

        public SurvivorsPickupActor SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount)
        {
            Requests.Add((kind, position, amount));
            if (FailSpawn?.Invoke(Requests.Count) == true) return null;
            var pickup = Add(kind, amount);
            pickup.transform.position = position;
            if (InactiveSpawn?.Invoke(Requests.Count) == true) pickup.ResetForWorldSpawn();
            return pickup;
        }

        public string ResolveMajorRewardDropLabel(SurvivorsEnemyRole role) => role.ToString();
        public Color ResolveMajorRewardDropColor(SurvivorsEnemyRole role) => Color.magenta;
        public void RecordStreakRewardFeedback(string label, Color color)
        {
            Events.Add("banner"); LastLabel = label; LastColor = color;
        }
        public void RecordFirstExperiencePickupTime() => Events.Add("first-xp");
        public int GainExperience(int amount) { Events.Add("gain:" + amount); OnReward?.Invoke(); return ExperienceResult; }
        public void RecordExperienceCombo(int gained) => Events.Add("combo:" + gained);
        public void RestoreHealthFromPickup(int amount) { Events.Add("health:" + amount); OnReward?.Invoke(); }
        public void AddBloodShards(int amount) { Events.Add("shards:" + amount); OnReward?.Invoke(); }
        public void PlayCollectionFeedback(Vector3 position, SurvivorsPickupKind kind, int burstCount)
        {
            Events.Add("collect:" + kind); LastBurst = burstCount; LastPosition = position; OnCollectionFeedback?.Invoke();
        }
        public void PlayAttractionFeedback(Vector3 position) { Events.Add("attract"); LastPosition = position; }
        public void PlayMagnetRecallFeedback(Vector3 position, int burstCount)
        {
            Events.Add("recall"); LastBurst = burstCount; LastPosition = position;
        }
        public void DespawnCompletedPickup(SpawnInstanceId instanceId) { Events.Add("despawn:" + instanceId.Value); OnDespawn?.Invoke(); }
        public void Dispose()
        {
            foreach (var gameObject in _owned) if (gameObject != null) UnityEngine.Object.DestroyImmediate(gameObject);
            _owned.Clear(); Pickups.Clear();
        }
    }
}
