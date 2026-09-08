using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Projects live player/pickup combat stats without owning modifiers or surge clocks.</summary>
    internal sealed class SurvivorsPlayerStats
    {
        private const float BaseDeathNovaRadius = 1.65f;
        private const float LowHealthWarningThreshold = 0.3f;
        private readonly ISurvivorsPlayerStatReadPort _port;
        internal SurvivorsPlayerStats(ISurvivorsPlayerStatReadPort port)
            => _port = port ?? throw new ArgumentNullException(nameof(port));

        internal float PlayerMoveSpeed
        {
            get
            {
                float value = _port.Tuning.PlayerMoveSpeed + _port.Modifiers.MoveSpeedBonus;
                return _port.MovementSurges.AddTo(value);
            }
        }

        internal float CurrentPickupAttractRange
        {
            get
            {
                float value = _port.Tuning.PickupAttractRange + _port.Modifiers.PickupRangeBonus;
                return Mathf.Max(0f, _port.PickupSurges.AddTo(value));
            }
        }

        internal float CurrentPickupAttractionSpeed => Mathf.Max(0.1f, _port.Tuning.PickupAttractionSpeed + _port.Modifiers.PickupAttractionSpeedBonus);
        internal float CriticalChanceNormalized => Mathf.Clamp01(_port.Modifiers.CriticalChanceBonus);
        internal float CriticalDamageMultiplier => Mathf.Clamp(1.5f + _port.Modifiers.CriticalDamageMultiplierBonus, 1f, 100f);
        internal float DeathNovaDamage => Mathf.Max(0f, _port.Modifiers.DeathNovaDamageBonus);
        internal float DeathNovaRadius => DeathNovaDamage <= 0f ? 0f : Mathf.Max(0f, BaseDeathNovaRadius + _port.Modifiers.DeathNovaRadiusBonus + _port.Modifiers.AreaRadiusBonus * 0.5f);
        internal float BarrierCapacity => Mathf.Max(0f, _port.Tuning.StartingBarrierCapacity + _port.Modifiers.BarrierCapacityBonus);
        internal bool IsLowHealthWarningActive => (_port.State == SurvivorsRunState.Playing || _port.State == SurvivorsRunState.LevelUp) && _port.MaxHealth > 0f && _port.CurrentHealth / _port.MaxHealth <= LowHealthWarningThreshold;
    }
}
