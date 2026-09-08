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
    // Pickup spawn, collection, attraction and reward cache bindings.
    public sealed partial class SurvivorsTemplateController
    {
        private void SpawnMajorRewardPickupCache(Vector3 position, SurvivorsEnemyRole role, float radius) => MajorRewardPickupCache.SpawnMajorRewardPickupCache(position, role, radius);

        private bool StartMajorRewardCacheAttraction(SurvivorsPickupActor pickup) => MajorRewardPickupCache.StartMajorRewardCacheAttraction(pickup);

        private int StartMagnetRecall() => PickupCollection.StartMagnetRecall();

        private float ResolvePickupMagnetPulseIntervalSeconds() => PickupCollection.ResolvePickupMagnetPulseIntervalSeconds();

        private void TickPickupMagnetPulse(float deltaTime) => PickupCollection.TickPickupMagnetPulse(deltaTime);

        internal void CollectPickup(SurvivorsPickupActor pickup) => PickupCollection.CollectPickup(pickup);

        internal void RecordPickupAttractionFeedback(SurvivorsPickupKind kind, Vector3 position) => PickupCollection.RecordPickupAttractionFeedback(kind, position);

        private SurvivorsMajorRewardPickupCache MajorRewardPickupCache => _majorRewardPickupCache ?? (_majorRewardPickupCache = new SurvivorsMajorRewardPickupCache(this));

        private SurvivorsPickupCollection PickupCollection => _pickupCollection ?? (_pickupCollection = new SurvivorsPickupCollection(_pickups, this));

        SurvivorsTemplateTuning ISurvivorsMajorRewardPickupCachePort.Tuning => CurrentTuning;

        int ISurvivorsMajorRewardPickupCachePort.RunEscalationLevel => RunEscalationLevel;

        bool ISurvivorsMajorRewardPickupCachePort.IsMajorRewardRole(SurvivorsEnemyRole role) => IsMajorRewardRole(role);

        SurvivorsPickupActor ISurvivorsMajorRewardPickupCachePort.SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount) => SpawnPickup(kind, position, amount);

        string ISurvivorsMajorRewardPickupCachePort.ResolveMajorRewardDropLabel(SurvivorsEnemyRole role) => ResolveMajorRewardDropLabel(role);

        Color ISurvivorsMajorRewardPickupCachePort.ResolveMajorRewardDropColor(SurvivorsEnemyRole role) => ResolveMajorRewardDropColor(role);

        void ISurvivorsMajorRewardPickupCachePort.RecordStreakRewardFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);

        SurvivorsTemplateTuning ISurvivorsPickupCollectionPort.Tuning => CurrentTuning;

        float ISurvivorsPickupCollectionPort.PickupMagnetPulseIntervalReductionBonus => PickupMagnetPulseIntervalReductionBonus;

        Vector3 ISurvivorsPickupCollectionPort.PlayerPosition => PlayerPosition;

        void ISurvivorsPickupCollectionPort.RecordFirstExperiencePickupTime() => Telemetry.Record(SurvivorsRunMetric.FirstExperiencePickup, RunTimeSeconds);

        int ISurvivorsPickupCollectionPort.GainExperience(int amount) => GainExperience(amount);

        void ISurvivorsPickupCollectionPort.RecordExperienceCombo(int gained) => RecordExperienceCombo(gained);

        void ISurvivorsPickupCollectionPort.RestoreHealthFromPickup(int amount) => PlayerVitals.RestoreHealthFromPickup(amount);

        void ISurvivorsPickupCollectionPort.AddBloodShards(int amount) => RunRewards.AddBloodShards(amount);

        void ISurvivorsPickupCollectionPort.PlayCollectionFeedback(Vector3 position, SurvivorsPickupKind kind, int burstCount)
        {
            string audioEventId = kind == SurvivorsPickupKind.Experience ? AudioEventXpPickup : AudioEventUiSelect;
            PlayFeedback(_pickupPulse, position, burstCount, _pickupClip, audioEventId, 0.12f);
        }

        void ISurvivorsPickupCollectionPort.PlayAttractionFeedback(Vector3 position) => PlayFeedback(_pickupPulse, position, 6, null);

        void ISurvivorsPickupCollectionPort.PlayMagnetRecallFeedback(Vector3 position, int burstCount) => PlayFeedback(_pickupPulse, position, burstCount, _pickupClip, AudioEventMagnetPulse, 0.4f);

        void ISurvivorsPickupCollectionPort.RecordStreakRewardFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);

        void ISurvivorsPickupCollectionPort.DespawnCompletedPickup(SpawnInstanceId id) => _spawnService?.Despawn(id, DespawnReason.Completed);

        private SurvivorsPickupActor SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount) => PickupSpawner.SpawnPickup(kind, position, amount);

        private static WorldSpawnableId ResolvePickupSpawnableId(SurvivorsPickupKind kind) => SurvivorsPickupSpawner.ResolvePickupSpawnableId(kind);

        private SurvivorsPickupSpawner PickupSpawner => _pickupSpawner ?? (_pickupSpawner = new SurvivorsPickupSpawner(this, SpawnSequence));

        SurvivorsTemplateTuning ISurvivorsPickupSpawnPort.Tuning => CurrentTuning;

        float ISurvivorsPickupSpawnPort.CurrentPickupAttractRange => CurrentPickupAttractRange;

        float ISurvivorsPickupSpawnPort.CurrentPickupAttractionSpeed => CurrentPickupAttractionSpeed;

        void ISurvivorsPickupSpawnPort.InitializePickup(SurvivorsPickupActor pickup, SurvivorsPickupKind kind, int amount, float range, float speed, float radius) => pickup.Initialize(this, kind, amount, range, speed, radius);

        void ISurvivorsPickupSpawnPort.RegisterPickup(SurvivorsPickupActor pickup) => _pickups.Add(pickup);

        private SurvivorsExperienceComboRewards ExperienceRhythm => _experienceRhythm ?? (_experienceRhythm = new SurvivorsExperienceComboRewards(() => CurrentTuning));

        private void RecordExperienceCombo(int gained) => ExperienceRhythm.RecordExperienceCombo(gained);

        public int Experience => _experienceProgression.Experience;

        public int EnemyRangedAttackDodgeExperienceGemDropCount => RangedDodgeRewards.EnemyRangedAttackDodgeExperienceGemDropCount;

        public int MajorRewardCacheDropCount => MajorRewardPickupCache.MajorRewardCacheDropCount;

        public int MajorRewardCacheExperienceGemDropCount => MajorRewardPickupCache.MajorRewardCacheExperienceGemDropCount;

        public int MajorRewardCacheSpecialDropCount => MajorRewardPickupCache.MajorRewardCacheSpecialDropCount;

        public int MajorRewardCacheAttractedPickupCount => MajorRewardPickupCache.MajorRewardCacheAttractedPickupCount;

        public int ExperiencePickupFeedbackCount => PickupCollection.ExperiencePickupFeedbackCount;

        public int BloodShardPickupCollectedCount => PickupCollection.BloodShardPickupCollectedCount;

        public int BloodShardsCollectedFromPickups => PickupCollection.BloodShardsCollectedFromPickups;

        public int PickupAttractionFeedbackCount => PickupCollection.PickupAttractionFeedbackCount;

        public int MagnetRecallFeedbackCount => PickupCollection.MagnetRecallFeedbackCount;

        public int ExperienceCollected => _experienceProgression.ExperienceCollected;

        public int MagnetRecallCount => PickupCollection.MagnetRecallCount;

        public int ActivePickupCount => _pickups.Count;

        public int ActiveMajorRewardCacheAttractedPickupCount => PickupCollection.ActiveMajorRewardCacheAttractedPickupCount;

        public float CurrentPickupMagnetPulseIntervalSeconds => ResolvePickupMagnetPulseIntervalSeconds();

        public int MagnetPulseActivationCount => PickupCollection.MagnetPulseActivationCount;

        public string LastMagnetPulseFeedbackLabel => PickupCollection.LastMagnetPulseFeedbackLabel;

        public int CurrentExperienceComboPickupCount => ExperienceRhythm.CurrentExperienceComboPickupCount;

        public int CurrentExperienceComboAmount => ExperienceRhythm.CurrentExperienceComboAmount;

        public int RequiredExperienceForNextLevel => _experienceProgression.RequiredExperience(CurrentTuning);

        public float FirstExperiencePickupTimeSeconds => Telemetry.FirstExperiencePickupTimeSeconds;

        public int ThrottledExperienceOverflow => _experienceProgression.ThrottledExperienceOverflow;

        public SurvivorsPickupActor SpawnExperienceForTest(Vector3 position, int amount)
        {
            EnsureRunStartedForTest();
            return SpawnPickup(SurvivorsPickupKind.Experience, position, amount);
        }

        public SurvivorsPickupActor SpawnMagnetForTest(Vector3 position)
        {
            EnsureRunStartedForTest();
            return SpawnPickup(SurvivorsPickupKind.Magnet, position, 1);
        }

        public SurvivorsPickupActor SpawnBloodShardForTest(Vector3 position, int amount)
        {
            EnsureRunStartedForTest();
            return SpawnPickup(SurvivorsPickupKind.BloodShard, position, amount);
        }

        public void DebugGrantExperience(int amount)
        {
            EnsureRunStartedForTest();
            GainExperience(Mathf.Max(1, amount));
        }

        public void TriggerMagnetRecall()
        {
            StartMagnetRecall();
        }

        private int GainExperience(int amount)
        {
            int gained = _experienceProgression.Gain(amount,
                ExperienceGainMultiplierBonus + PassiveLoadoutSurgeExperienceGainMultiplierBonus, CurrentTuning);
            TryOpenPendingLevelUpDraft();
            return gained;
        }
    }
}
