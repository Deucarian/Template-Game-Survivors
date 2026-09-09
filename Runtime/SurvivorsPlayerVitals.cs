using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.Combat;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns player health, barrier absorption, safety and the one-use clutch response.</summary>
    internal sealed class SurvivorsPlayerVitals
    {
        private const float LowHealthWarningThreshold = 0.3f;
        private readonly ISurvivorsPlayerDamagePort _port;
        private HealthState _health;
        private float _playerInvulnerabilityTimer;
        private bool _lowHealthClutchPulseUsed;
        public SurvivorsPlayerVitals(ISurvivorsPlayerDamagePort port) => _port = port ?? throw new ArgumentNullException(nameof(port));
        public bool IsBound => _health != null;
        public float CurrentHealth => _health == null ? 0f : (float)_health.CurrentHealth;
        public float MaxHealth => _health == null ? 0f : (float)_health.MaximumHealth;
        public float SafetyRemaining => Mathf.Max(0f, _playerInvulnerabilityTimer);
        public float BarrierValue { get; private set; }
        public float DamageTaken { get; private set; }
        public int LowHealthClutchPulseCount { get; private set; }
        public int LowHealthClutchPulseHitCount { get; private set; }
        public string LastLowHealthClutchPulseFeedbackLabel { get; private set; } = string.Empty;
        public int HealthPickupCollectedCount { get; private set; }
        public float HealthRestoredByPickups { get; private set; }

        public void Initialize(float maximumHealth)
        {
            _health = new HealthState(new CombatantId("combatant.survivors.player"), maximumHealth, maximumHealth);
            _playerInvulnerabilityTimer = 0f;
            _lowHealthClutchPulseUsed = false;
            BarrierValue = 0f;
            DamageTaken = 0f;
            LowHealthClutchPulseCount = 0;
            LowHealthClutchPulseHitCount = 0;
            LastLowHealthClutchPulseFeedbackLabel = string.Empty;
            HealthPickupCollectedCount = 0;
            HealthRestoredByPickups = 0f;
        }
        public void SetBarrier(float value) => BarrierValue = value;
        public void ResetDamageTaken() => DamageTaken = 0f;
        public void ExtendSafety(float seconds) => _playerInvulnerabilityTimer = Mathf.Max(_playerInvulnerabilityTimer, seconds);
        public void TickSafety(float deltaTime) => _playerInvulnerabilityTimer = Mathf.Max(0f, _playerInvulnerabilityTimer - deltaTime);
        public void Heal(float amount) { if (_health != null) _health.Heal(amount); }
        public void IncreaseMaximumHealth(double amount)
        {
            if (_health != null) _health.ChangeMaximumHealth(_health.MaximumHealth + amount, MaximumChangePolicy.FillToMaximum);
        }

        public void ApplyDamageToPlayer(float amount, string source)
        {
            if (_health == null || _port.State == SurvivorsRunState.GameOver || _port.State == SurvivorsRunState.Victory)
            {
                return;
            }

            float incoming = Mathf.Max(0f, amount);
            if (_playerInvulnerabilityTimer > 0f && incoming > 0f)
            {
                _port.ShowBlockedDamage(true);
                return;
            }

            if (incoming > 0f)
            {
                _playerInvulnerabilityTimer = Mathf.Max(_playerInvulnerabilityTimer, _port.Tuning.PlayerContactInvulnerabilitySeconds);
            }

            if (BarrierValue > 0f && incoming > 0f)
            {
                float absorbed = Mathf.Min(BarrierValue, incoming);
                BarrierValue -= absorbed;
                incoming -= absorbed;
            }

            if (incoming <= 0f)
            {
                _port.ShowBlockedDamage(false);
                return;
            }

            float healthFractionBefore = MaxHealth <= 0f ? 1f : CurrentHealth / MaxHealth;
            DamageRequest request = new DamageRequest(
                _health.Id,
                new[] { new DamageComponent(BasicSurvivorsGame.ArcaneDamageType, incoming) },
                sourceId: new CombatantId(string.IsNullOrWhiteSpace(source) ? "combatant.survivors.enemy" : source),
                preResolvedCritical: false);
            DamageResolutionResult result = CombatDamageResolver.Resolve(_port.CombatCatalog, _health, null, request);
            if (result != null && result.Damage != null)
            {
                DamageTaken += Mathf.Max(0f, (float)result.Damage.HealthDamage);
            }

            _port.RecordDamage(result == null ? null : result.Damage, _port.PlayerPosition);
            if (!_health.IsAlive)
            {
                _port.Defeat();
            }
            else
            {
                if (!TryTriggerLowHealthClutchPulse(healthFractionBefore))
                {
                    _port.ShowHurt();
                }
            }
        }

        private bool TryTriggerLowHealthClutchPulse(float healthFractionBefore)
        {
            if (_lowHealthClutchPulseUsed || _health == null || !_health.IsAlive || MaxHealth <= 0f)
            {
                return false;
            }

            float healthFractionAfter = CurrentHealth / MaxHealth;
            if (healthFractionBefore <= LowHealthWarningThreshold || healthFractionAfter > LowHealthWarningThreshold)
            {
                return false;
            }

            _lowHealthClutchPulseUsed = true;

            float safetySeconds = Mathf.Max(0f, _port.Tuning.LowHealthClutchSafetySeconds);
            if (safetySeconds > 0f)
            {
                _playerInvulnerabilityTimer = Mathf.Max(_playerInvulnerabilityTimer, safetySeconds);
            }

            float radius = Mathf.Max(0f, _port.Tuning.LowHealthClutchPulseRadius);
            float damage = Mathf.Max(0f, _port.Tuning.LowHealthClutchPulseDamage);
            int hitCount = 0;
            if (radius > 0f && damage > 0f)
            {
                hitCount = _port.DamageNonMajorEnemies(_port.PlayerPosition, radius, damage, "survivors.low-health.clutch-pulse");
            }

            LowHealthClutchPulseCount++;
            LowHealthClutchPulseHitCount += hitCount;
            LastLowHealthClutchPulseFeedbackLabel = $"Clutch Pulse: {hitCount} enemies hit, safety {safetySeconds:0.#}s";
            _port.ShowClutch(LastLowHealthClutchPulseFeedbackLabel, hitCount);
            return true;
        }

        public void RestoreHealthFromPickup(int amount)
        {
            if (_health == null || amount <= 0)
            {
                return;
            }

            float before = CurrentHealth;
            _health.Heal(amount);
            float restored = Mathf.Max(0f, CurrentHealth - before);
            HealthPickupCollectedCount++;
            HealthRestoredByPickups += restored;
        }

        public void TickBarrier(float deltaTime)
        {
            float regen = Mathf.Max(0f, _port.Tuning.BaseBarrierRegenPerSecond + _port.BarrierRegenPerSecondBonus);
            if (regen > 0f)
            {
                RestoreBarrier(regen * deltaTime);
            }
        }

        public void RestoreBarrier(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            BarrierValue = Mathf.Min(_port.BarrierCapacity, BarrierValue + amount);
        }
    }
}
