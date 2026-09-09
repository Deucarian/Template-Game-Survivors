using System;
using System.Collections.Generic;
using Deucarian.Combat;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns trap-snare windows, cooldown and triggered radial rewards.</summary>
    internal sealed class SurvivorsPayloadHazardRewards
    {
        private readonly ISurvivorsPickupRewardPort _port;
        public SurvivorsPayloadHazardRewards(ISurvivorsPickupRewardPort port) => _port = port ?? throw new ArgumentNullException(nameof(port));
        private int _payloadHazardChainSnareCount;
        private float _payloadHazardChainWindowTimer;
        private float _payloadHazardChainCooldownTimer;
        public int PayloadHazardTickCount { get; private set; }
        public int PayloadHazardSnareCount { get; private set; }
        public string LastPayloadHazardSnareFeedbackLabel { get; private set; } = string.Empty;
        public int PayloadHazardChainActivationCount { get; private set; }
        public int PayloadHazardChainPulseHitCount { get; private set; }
        public int PayloadHazardChainExperienceGemDropCount { get; private set; }
        public string LastPayloadHazardChainFeedbackLabel { get; private set; } = string.Empty;
        public void Reset()
        {
            PayloadHazardTickCount = 0;
            PayloadHazardSnareCount = 0;
            LastPayloadHazardSnareFeedbackLabel = string.Empty;
            PayloadHazardChainActivationCount = 0;
            PayloadHazardChainPulseHitCount = 0;
            PayloadHazardChainExperienceGemDropCount = 0;
            LastPayloadHazardChainFeedbackLabel = string.Empty;
            _payloadHazardChainSnareCount = 0;
            _payloadHazardChainWindowTimer = 0;
            _payloadHazardChainCooldownTimer = 0;
        }
        public void RecordPayloadHazardTick()
        {
            PayloadHazardTickCount++;
        }

        public void RecordPayloadHazardSnare(string enemyDisplayName, SurvivorsWeaponArchetypeDefinition definition, Vector3 origin)
        {
            if (definition == null)
            {
                return;
            }

            PayloadHazardSnareCount++;
            LastPayloadHazardSnareFeedbackLabel = $"{definition.DisplayName} hazard snared {enemyDisplayName}";
            RegisterPayloadHazardChainSnare(origin, definition);
        }

        private void RegisterPayloadHazardChainSnare(Vector3 origin, SurvivorsWeaponArchetypeDefinition definition)
        {
            if (_payloadHazardChainCooldownTimer > 0f)
            {
                return;
            }

            if (_payloadHazardChainWindowTimer <= 0f)
            {
                _payloadHazardChainSnareCount = 0;
            }

            _payloadHazardChainSnareCount++;
            _payloadHazardChainWindowTimer = Mathf.Max(0.05f, _port.Tuning.PayloadHazardChainWindowSeconds);
            int threshold = Mathf.Max(1, _port.Tuning.PayloadHazardChainSnareThreshold);
            if (_payloadHazardChainSnareCount < threshold)
            {
                return;
            }

            TriggerPayloadHazardChain(origin, definition);
        }

        private void TriggerPayloadHazardChain(Vector3 origin, SurvivorsWeaponArchetypeDefinition definition)
        {
            int caughtSnares = Mathf.Max(1, _payloadHazardChainSnareCount);
            _payloadHazardChainSnareCount = 0;
            _payloadHazardChainWindowTimer = 0f;
            _payloadHazardChainCooldownTimer = Mathf.Max(0f, _port.Tuning.PayloadHazardChainCooldownSeconds);

            float radius = Mathf.Max(0f, _port.Tuning.PayloadHazardChainPulseRadius);
            float damage = Mathf.Max(0f, _port.Tuning.PayloadHazardChainPulseDamage);
            int hitCount = radius > 0f && damage > 0f ? _port.DamageNonMajor(origin, radius, damage, "survivors.payload-hazard.chain") : 0;

            int gemCount = Mathf.Max(0, _port.Tuning.PayloadHazardChainExperienceGemCount);
            int xpPerGem = Mathf.Max(1, Mathf.RoundToInt(_port.Tuning.EnemyExperienceReward * Mathf.Max(0.1f, _port.Tuning.PayloadHazardChainExperienceMultiplier)));
            int spawnedExperience = 0;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.23f) / Mathf.Max(1, gemCount)) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.78f;
                if (_port.SpawnPickup(SurvivorsPickupKind.Experience, origin + offset, xpPerGem, false))
                {
                    spawnedExperience += xpPerGem;
                    PayloadHazardChainExperienceGemDropCount++;
                }
            }

            PayloadHazardChainActivationCount++;
            PayloadHazardChainPulseHitCount += hitCount;
            string name = definition == null || string.IsNullOrWhiteSpace(definition.DisplayName)
                ? "Payload"
                : definition.DisplayName;
            LastPayloadHazardChainFeedbackLabel = $"Trap Chain: {name} caught {caughtSnares} snares, +{spawnedExperience} XP, {hitCount} enemies hit";
            _port.ShowFeedback(LastPayloadHazardChainFeedbackLabel, new Color(0.62f, 0.9f, 1f));
            _port.PlayPulse(origin, Mathf.Clamp(28 + hitCount * 4, 34, 78), false, true);
        }

        public void TickPayloadHazardChain(float deltaTime)
        {
            float dt = Mathf.Max(0f, deltaTime);
            if (_payloadHazardChainCooldownTimer > 0f)
            {
                _payloadHazardChainCooldownTimer = Mathf.Max(0f, _payloadHazardChainCooldownTimer - dt);
            }

            if (_payloadHazardChainWindowTimer <= 0f)
            {
                return;
            }

            _payloadHazardChainWindowTimer = Mathf.Max(0f, _payloadHazardChainWindowTimer - dt);
            if (_payloadHazardChainWindowTimer <= 0f)
            {
                _payloadHazardChainSnareCount = 0;
            }
        }
    }
}
