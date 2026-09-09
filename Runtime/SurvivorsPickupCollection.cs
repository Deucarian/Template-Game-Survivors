using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns pickup collection transactions and the magnet recall/pulse cadence.</summary>
    internal sealed class SurvivorsPickupCollection
    {
        private readonly IList<SurvivorsPickupActor> _pickups;
        private readonly ISurvivorsPickupCollectionPort _port;
        private float _pickupMagnetPulseTimer;
        private string _lastMagnetPulseFeedbackLabel = string.Empty;
        internal int ExperiencePickupFeedbackCount { get; private set; }
        internal int BloodShardPickupCollectedCount { get; private set; }
        internal int BloodShardsCollectedFromPickups { get; private set; }
        internal int PickupAttractionFeedbackCount { get; private set; }
        internal int MagnetRecallFeedbackCount { get; private set; }
        internal int MagnetRecallCount { get; private set; }
        internal int MagnetPulseActivationCount { get; private set; }
        internal string LastMagnetPulseFeedbackLabel => _lastMagnetPulseFeedbackLabel;
        internal float PulseSecondsRemaining => _pickupMagnetPulseTimer;
        internal int ActiveMajorRewardCacheAttractedPickupCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _pickups.Count; i++)
                {
                    SurvivorsPickupActor pickup = _pickups[i];
                    if (pickup != null && pickup.IsRewardCacheAttractionActive) count++;
                }
                return count;
            }
        }

        internal SurvivorsPickupCollection(IList<SurvivorsPickupActor> pickups, ISurvivorsPickupCollectionPort port)
        {
            _pickups = pickups ?? throw new ArgumentNullException(nameof(pickups));
            _port = port ?? throw new ArgumentNullException(nameof(port));
        }

        internal void ResetDiagnostics()
        {
            ExperiencePickupFeedbackCount = 0;
            BloodShardPickupCollectedCount = 0;
            BloodShardsCollectedFromPickups = 0;
            PickupAttractionFeedbackCount = 0;
            MagnetRecallFeedbackCount = 0;
            MagnetRecallCount = 0;
            MagnetPulseActivationCount = 0;
            _lastMagnetPulseFeedbackLabel = string.Empty;
        }

        internal void ResetPulseSchedule() => _pickupMagnetPulseTimer = ResolvePickupMagnetPulseIntervalSeconds();

        internal void ScheduleMagnetPulse()
        {
            if (_pickupMagnetPulseTimer <= 0f) ResetPulseSchedule();
        }

        internal float ResolvePickupMagnetPulseIntervalSeconds()
        {
            if (_port.PickupMagnetPulseIntervalReductionBonus <= 0f)
            {
                return -1f;
            }

            float baseInterval = Mathf.Max(1f, _port.Tuning.PickupMagnetPulseBaseIntervalSeconds);
            float minimum = Mathf.Max(1f, _port.Tuning.PickupMagnetPulseMinimumIntervalSeconds);
            return Mathf.Max(minimum, baseInterval - _port.PickupMagnetPulseIntervalReductionBonus);
        }

        internal void TickPickupMagnetPulse(float deltaTime)
        {
            float interval = ResolvePickupMagnetPulseIntervalSeconds();
            if (interval <= 0f || _pickups.Count == 0)
            {
                _pickupMagnetPulseTimer = interval;
                return;
            }

            if (_pickupMagnetPulseTimer <= 0f)
            {
                _pickupMagnetPulseTimer = interval;
            }

            _pickupMagnetPulseTimer -= Mathf.Max(0f, deltaTime);
            if (_pickupMagnetPulseTimer > 0f)
            {
                return;
            }

            _pickupMagnetPulseTimer = interval;
            int recalled = StartMagnetRecall();
            if (recalled <= 0)
            {
                return;
            }

            MagnetPulseActivationCount++;
            _lastMagnetPulseFeedbackLabel = $"Vacuum Pulse: {recalled} XP gems pulled";
            _port.RecordStreakRewardFeedback(_lastMagnetPulseFeedbackLabel, new Color(0.42f, 0.88f, 1f));
        }

        internal int StartMagnetRecall()
        {
            MagnetRecallCount++;
            int recalled = 0;
            for (int i = 0; i < _pickups.Count; i++)
            {
                SurvivorsPickupActor pickup = _pickups[i];
                if (pickup != null && pickup.Kind == SurvivorsPickupKind.Experience)
                {
                    pickup.StartGlobalRecall(_port.Tuning.MagnetRecallSpeedMultiplier);
                    recalled++;
                }
            }

            if (recalled > 0)
            {
                MagnetRecallFeedbackCount++;
                _port.PlayMagnetRecallFeedback(_port.PlayerPosition, Mathf.Clamp(12 + recalled * 3, 18, 72));
            }

            return recalled;
        }

        internal void CollectPickup(SurvivorsPickupActor pickup)
        {
            if (pickup == null)
            {
                return;
            }

            if (pickup.Kind == SurvivorsPickupKind.Magnet)
            {
                StartMagnetRecall();
            }
            else if (pickup.Kind == SurvivorsPickupKind.Health)
            {
                _port.RestoreHealthFromPickup(Mathf.Max(1, pickup.Amount));
            }
            else if (pickup.Kind == SurvivorsPickupKind.BloodShard)
            {
                CollectBloodShardPickup(Mathf.Max(1, pickup.Amount));
            }
            else
            {
                _port.RecordFirstExperiencePickupTime();
                int gained = _port.GainExperience(Mathf.Max(1, pickup.Amount));
                _port.RecordExperienceCombo(gained);
                ExperiencePickupFeedbackCount++;
            }

            _port.PlayCollectionFeedback(pickup.transform.position, pickup.Kind, ResolvePickupFeedbackBurstCount(pickup.Kind));
            _pickups.Remove(pickup);
            if (pickup.InstanceId.Value > 0)
            {
                _port.DespawnCompletedPickup(pickup.InstanceId);
            }
        }

        internal void CollectBloodShardPickup(int amount)
        {
            int gained = Mathf.Max(1, amount);
            _port.AddBloodShards(gained);
            BloodShardPickupCollectedCount++;
            BloodShardsCollectedFromPickups += gained;
        }

        internal static int ResolvePickupFeedbackBurstCount(SurvivorsPickupKind kind)
        {
            switch (kind)
            {
                case SurvivorsPickupKind.Magnet:
                    return 28;
                case SurvivorsPickupKind.Health:
                    return 22;
                case SurvivorsPickupKind.BloodShard:
                    return 18;
                default:
                    return 10;
            }
        }

        internal void RecordPickupAttractionFeedback(SurvivorsPickupKind kind, Vector3 position)
        {
            if (kind == SurvivorsPickupKind.Magnet)
            {
                return;
            }

            PickupAttractionFeedbackCount++;
            _port.PlayAttractionFeedback(position);
        }
    }
}
