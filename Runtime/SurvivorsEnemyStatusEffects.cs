using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Template status stacking/timing policy. Damage resolution and tint changes use explicit ports.</summary>
    internal sealed class SurvivorsEnemyStatusEffects
    {
        private readonly Func<bool> _isAlive;
        private readonly Action<float, string> _applyDamage;
        private readonly Action _presentationChanged;
        private float _poisonDamagePerSecond;
        private float _poisonRemainingSeconds;
        private float _bleedDamagePerSecond;
        private float _bleedRemainingSeconds;
        private float _burnDamagePerSecond;
        private float _burnRemainingSeconds;
        private float _moveSpeedMultiplier = 1f;
        private float _slowRemainingSeconds;

        public SurvivorsEnemyStatusEffects(Func<bool> isAlive, Action<float, string> applyDamage, Action presentationChanged)
        {
            _isAlive = isAlive ?? throw new ArgumentNullException(nameof(isAlive));
            _applyDamage = applyDamage ?? throw new ArgumentNullException(nameof(applyDamage));
            _presentationChanged = presentationChanged ?? throw new ArgumentNullException(nameof(presentationChanged));
        }

        public bool IsMovementSlowed => _slowRemainingSeconds > 0f && _moveSpeedMultiplier < 1f;
        public float CurrentMoveSpeedMultiplier => IsMovementSlowed ? _moveSpeedMultiplier : 1f;
        public bool IsBurning => _burnRemainingSeconds > 0f && _burnDamagePerSecond > 0f;
        public float CurrentBurnDamagePerSecond => IsBurning ? _burnDamagePerSecond : 0f;

        public void Reset()
        {
            _poisonDamagePerSecond = 0f;
            _poisonRemainingSeconds = 0f;
            _bleedDamagePerSecond = 0f;
            _bleedRemainingSeconds = 0f;
            _burnDamagePerSecond = 0f;
            _burnRemainingSeconds = 0f;
            _moveSpeedMultiplier = 1f;
            _slowRemainingSeconds = 0f;
        }

        public void ApplyDamageOverTime(float totalDamage, float durationSeconds, string statusId, string source)
        {
            float duration = Mathf.Max(0.1f, durationSeconds);
            float perSecond = Mathf.Max(0f, totalDamage) / duration;
            if (perSecond <= 0f)
            {
                return;
            }

            if (string.Equals(statusId, "status.survivors.burn", StringComparison.Ordinal))
            {
                _burnDamagePerSecond += perSecond;
                _burnRemainingSeconds = Mathf.Max(_burnRemainingSeconds, duration);
                _presentationChanged();
            }
            else if (string.Equals(statusId, "status.survivors.bleed", StringComparison.Ordinal))
            {
                _bleedDamagePerSecond += perSecond;
                _bleedRemainingSeconds = Mathf.Max(_bleedRemainingSeconds, duration);
            }
            else
            {
                _poisonDamagePerSecond += perSecond;
                _poisonRemainingSeconds = Mathf.Max(_poisonRemainingSeconds, duration);
            }
        }

        public bool ApplyMovementSlow(float multiplier, float durationSeconds)
        {
            if (!_isAlive())
            {
                return false;
            }

            float resolvedMultiplier = Mathf.Clamp(multiplier, 0.15f, 1f);
            float duration = Mathf.Max(0f, durationSeconds);
            if (resolvedMultiplier >= 1f || duration <= 0f)
            {
                return false;
            }

            _moveSpeedMultiplier = Mathf.Min(_moveSpeedMultiplier, resolvedMultiplier);
            _slowRemainingSeconds = Mathf.Max(_slowRemainingSeconds, duration);
            _presentationChanged();
            return true;
        }

        public void TickMovementSlow(float deltaTime)
        {
            if (_slowRemainingSeconds <= 0f)
            {
                return;
            }

            _slowRemainingSeconds = Mathf.Max(0f, _slowRemainingSeconds - Mathf.Max(0f, deltaTime));
            if (_slowRemainingSeconds > 0f)
            {
                return;
            }

            _moveSpeedMultiplier = 1f;
            _presentationChanged();
        }

        public void TickDamageOverTime(float deltaTime)
        {
            if (_poisonRemainingSeconds > 0f && _poisonDamagePerSecond > 0f)
            {
                float tick = _poisonDamagePerSecond * deltaTime;
                _poisonRemainingSeconds = Mathf.Max(0f, _poisonRemainingSeconds - deltaTime);
                _applyDamage(tick, "survivors.status.poison");
                if (_poisonRemainingSeconds <= 0f)
                {
                    _poisonDamagePerSecond = 0f;
                }
            }

            if (!_isAlive())
            {
                return;
            }

            if (_bleedRemainingSeconds > 0f && _bleedDamagePerSecond > 0f)
            {
                float tick = _bleedDamagePerSecond * deltaTime;
                _bleedRemainingSeconds = Mathf.Max(0f, _bleedRemainingSeconds - deltaTime);
                _applyDamage(tick, "survivors.status.bleed");
                if (_bleedRemainingSeconds <= 0f)
                {
                    _bleedDamagePerSecond = 0f;
                }
            }

            if (!_isAlive())
            {
                return;
            }

            if (_burnRemainingSeconds > 0f && _burnDamagePerSecond > 0f)
            {
                float tick = _burnDamagePerSecond * deltaTime;
                _burnRemainingSeconds = Mathf.Max(0f, _burnRemainingSeconds - deltaTime);
                _applyDamage(tick, "survivors.status.burn");
                if (_burnRemainingSeconds <= 0f)
                {
                    _burnDamagePerSecond = 0f;
                    _presentationChanged();
                }
            }
        }
    }
}
