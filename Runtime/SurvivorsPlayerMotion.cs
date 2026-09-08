using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.Combat;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns planar movement, dash cooldown and ordered travel/safety/pressure commands.</summary>
    internal sealed class SurvivorsPlayerMotion
    {
        private readonly ISurvivorsPlayerMotionPort _port;
        private float _dashCooldownTimer;
        public SurvivorsPlayerMotion(ISurvivorsPlayerMotionPort port) => _port = port ?? throw new ArgumentNullException(nameof(port));
        public float CooldownRemaining => Mathf.Max(0f, _dashCooldownTimer);
        public int DashUseCount { get; private set; }
        public int DashEnemyShoveCount { get; private set; }
        public int DashDamageHitCount { get; private set; }
        public string LastDashFeedbackLabel { get; private set; } = string.Empty;
        public void Reset()
        {
            _dashCooldownTimer = 0f;
            DashUseCount = 0;
            DashEnemyShoveCount = 0;
            DashDamageHitCount = 0;
            LastDashFeedbackLabel = string.Empty;
        }
        public void TickCooldown(float deltaTime) => _dashCooldownTimer = Mathf.Max(0f, _dashCooldownTimer - deltaTime);

        public void MovePlayer(Vector2 movementInput, float deltaTime)
        {
            if (!_port.HasPlayer || movementInput.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector2 normalized = movementInput.sqrMagnitude > 1f ? movementInput.normalized : movementInput;
            Vector3 delta = new Vector3(normalized.x, 0f, normalized.y) * (_port.MoveSpeed * deltaTime);
            _port.Position += delta;
            if (delta.sqrMagnitude > 0.0001f)
            {
                _port.Forward = delta.normalized;
                _port.RecordTravel(delta);
            }
        }

        public bool TryDash(Vector2 directionInput)
        {
            if (_port.State != SurvivorsRunState.Playing || !_port.HasPlayer || _dashCooldownTimer > 0f)
            {
                return false;
            }

            Vector3 direction = ResolveDashDirection(directionInput);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            float distance = Mathf.Max(0f, _port.Tuning.DashDistance);
            if (distance <= 0f)
            {
                return false;
            }

            Vector3 start = _port.Position;
            Vector3 delta = direction.normalized * distance;
            Vector3 end = start + delta;
            _port.Position = end;
            _port.Forward = direction.normalized;
            _port.RecordTravel(delta);

            _dashCooldownTimer = Mathf.Max(0.05f, _port.Tuning.DashCooldownSeconds);
            _port.ExtendSafety(_port.Tuning.DashInvulnerabilitySeconds);
            DashUseCount++;

            int shoved = _port.ApplyDashPressure(start, end, direction.normalized, () => DashDamageHitCount++);
            DashEnemyShoveCount += shoved;
            LastDashFeedbackLabel = shoved > 0 ? $"Arc Step: shoved {shoved}" : "Arc Step";
            _port.ShowDash(end, LastDashFeedbackLabel, shoved);
            return true;
        }

        private Vector3 ResolveDashDirection(Vector2 directionInput)
        {
            Vector2 planar = directionInput.sqrMagnitude > 1f ? directionInput.normalized : directionInput;
            Vector3 direction = planar.sqrMagnitude > 0.0001f
                ? new Vector3(planar.x, 0f, planar.y)
                : _port.Forward;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.forward;
            }

            return direction.normalized;
        }
    }
}
