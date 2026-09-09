using System;
using System.Collections.Generic;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Current loadout and prepared collector/relic observations, used only during one projection.</summary>
    internal readonly struct SurvivorsBuildHudValues
    {
        public SurvivorsBuildHudValues(IReadOnlyList<string> weaponIds, int weaponCount, float pickupRange,
            float pickupSpeed, string pickupPulseLabel, string selectedRelicLabel)
        {
            ActiveWeaponIds = weaponIds ?? throw new ArgumentNullException(nameof(weaponIds));
            ActiveWeaponCount = weaponCount;
            CurrentPickupAttractRange = pickupRange;
            CurrentPickupAttractionSpeed = pickupSpeed;
            PickupPulseLabel = pickupPulseLabel;
            SelectedRelicLabel = selectedRelicLabel;
        }

        public IReadOnlyList<string> ActiveWeaponIds { get; }
        public int ActiveWeaponCount { get; }
        public float CurrentPickupAttractRange { get; }
        public float CurrentPickupAttractionSpeed { get; }
        public string PickupPulseLabel { get; }
        public string SelectedRelicLabel { get; }
    }
}
