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
    public sealed class SurvivorsPickupActor : MonoBehaviour, IWorldSpawnedObject, IWorldSpawnResettable
    {
        private SurvivorsTemplateController _controller;
        private float _attractRange;
        private float _attractionSpeed;
        private float _collectRadius;
        private bool _globalRecall;
        private bool _rewardCacheAttraction;
        private bool _attractionFeedbackSent;
        private float _recallSpeedMultiplier;
        private float _currentSpeed;
        private Vector3 _baseScale;
        private float _pulseSeconds;

        public SpawnInstanceId InstanceId { get; private set; }
        public bool IsActive { get; private set; }
        public SurvivorsPickupKind Kind { get; private set; }
        public int Amount { get; private set; }
        public bool IsGlobalRecallActive => IsActive && _globalRecall;
        public bool IsRewardCacheAttractionActive => IsActive && _rewardCacheAttraction;
        public bool HasShownAttractionFeedback => _attractionFeedbackSent;

        public void Initialize(SurvivorsTemplateController controller, SurvivorsPickupKind kind, int amount, float attractRange, float attractionSpeed, float collectRadius)
        {
            _controller = controller;
            Kind = kind;
            Amount = Mathf.Max(1, amount);
            UpdateAttractionSettings(attractRange, attractionSpeed, collectRadius);
            _globalRecall = false;
            _rewardCacheAttraction = false;
            _attractionFeedbackSent = false;
            _recallSpeedMultiplier = 1f;
            _currentSpeed = 0f;
            _pulseSeconds = 0f;
            IsActive = true;
            _baseScale = ResolvePickupBaseScale(kind);
            transform.localScale = _baseScale;
        }

        public void UpdateAttractionSettings(float attractRange, float attractionSpeed, float collectRadius)
        {
            _attractRange = Mathf.Max(0.1f, attractRange);
            _attractionSpeed = Mathf.Max(0.1f, attractionSpeed);
            _collectRadius = Mathf.Max(0.1f, collectRadius);
        }

        public void StartGlobalRecall(float speedMultiplier)
        {
            if (Kind != SurvivorsPickupKind.Experience)
            {
                return;
            }

            _globalRecall = true;
            _recallSpeedMultiplier = Mathf.Max(1f, speedMultiplier);
            _currentSpeed = Mathf.Max(_currentSpeed, _attractionSpeed * 1.5f);
            BeginAttractionFeedback();
        }

        public bool StartRewardCacheAttraction(float speedMultiplier)
        {
            if (!IsActive)
            {
                return false;
            }

            _rewardCacheAttraction = true;
            _recallSpeedMultiplier = Mathf.Max(_recallSpeedMultiplier, speedMultiplier);
            _currentSpeed = Mathf.Max(_currentSpeed, _attractionSpeed * 1.25f);
            BeginAttractionFeedback();
            return true;
        }

        private static Vector3 ResolvePickupBaseScale(SurvivorsPickupKind kind)
        {
            switch (kind)
            {
                case SurvivorsPickupKind.Magnet:
                    return Vector3.one * 0.58f;
                case SurvivorsPickupKind.Health:
                    return Vector3.one * 0.44f;
                case SurvivorsPickupKind.BloodShard:
                    return Vector3.one * 0.4f;
                default:
                    return Vector3.one * 0.34f;
            }
        }

        public void Simulate(float deltaTime)
        {
            if (!IsActive || _controller == null)
            {
                return;
            }

            Vector3 playerPosition = _controller.PlayerPosition;
            Vector3 offset = playerPosition - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            bool forcedAttraction = _globalRecall || _rewardCacheAttraction;
            bool shouldAttract = forcedAttraction || distance <= _attractRange;
            if (shouldAttract && distance > 0.001f)
            {
                BeginAttractionFeedback();
                float targetSpeed = _attractionSpeed * (forcedAttraction ? _recallSpeedMultiplier * Mathf.Clamp(1f + distance * 0.18f, 1f, 10f) : 1f);
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, targetSpeed * 8f * deltaTime);
                float travel = Mathf.Min(distance, _currentSpeed * deltaTime);
                transform.position += offset.normalized * travel;
                distance = Vector3.Distance(transform.position, playerPosition);
            }
            else
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, _attractionSpeed * 5f * deltaTime);
            }

            if (forcedAttraction)
            {
                transform.Rotate(0f, 420f * deltaTime, 0f, Space.Self);
            }

            TickPickupPresentation(deltaTime, shouldAttract);

            if (distance <= _collectRadius)
            {
                IsActive = false;
                _controller.CollectPickup(this);
            }
        }

        private void BeginAttractionFeedback()
        {
            if (_attractionFeedbackSent || _controller == null)
            {
                return;
            }

            _attractionFeedbackSent = true;
            _controller.RecordPickupAttractionFeedback(Kind, transform.position);
        }

        private void TickPickupPresentation(float deltaTime, bool attracting)
        {
            if (_baseScale == Vector3.zero)
            {
                _baseScale = transform.localScale == Vector3.zero ? Vector3.one * 0.34f : transform.localScale;
            }

            _pulseSeconds += Mathf.Max(0f, deltaTime);
            float scale = 1f;
            if (_globalRecall || _rewardCacheAttraction)
            {
                scale = 1.28f + Mathf.Sin(_pulseSeconds * 18f) * 0.16f;
            }
            else if (attracting)
            {
                scale = 1.12f + Mathf.Sin(_pulseSeconds * 12f) * 0.08f;
            }

            transform.localScale = _baseScale * Mathf.Max(0.5f, scale);
        }

        public void OnWorldSpawned(WorldSpawnContext context)
        {
            InstanceId = context.InstanceId;
        }

        public void OnWorldDespawned(DespawnReason reason)
        {
            _controller = null;
            IsActive = false;
            _rewardCacheAttraction = false;
        }

        public void ResetForWorldSpawn()
        {
            _controller = null;
            IsActive = false;
            InstanceId = default;
            _globalRecall = false;
            _rewardCacheAttraction = false;
            _attractionFeedbackSent = false;
            _currentSpeed = 0f;
            _pulseSeconds = 0f;
        }
    }
}
