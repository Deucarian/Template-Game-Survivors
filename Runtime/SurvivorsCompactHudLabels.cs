using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Compact weapon/relic/exploration labels; all collections and authored names remain borrowed.</summary>
    internal sealed class SurvivorsCompactHudLabels
    {
        private readonly SurvivorsBuildContentLabels _labels;
        internal SurvivorsCompactHudLabels(SurvivorsBuildContentLabels labels)
            => _labels = labels ?? throw new ArgumentNullException(nameof(labels));

        internal string Weapons(IReadOnlyList<string> activeWeapons)
        {
            if (activeWeapons.Count == 0) return "none";
            const int maxShown = 4;
            string label = string.Empty;
            int shown = Mathf.Min(maxShown, activeWeapons.Count);
            for (int i = 0; i < shown; i++)
            {
                if (i > 0) label += ", ";
                label += _labels.ShortWeaponName(activeWeapons[i]);
            }
            if (activeWeapons.Count > shown) label += " +" + (activeWeapons.Count - shown).ToString();
            return label;
        }

        internal static string Relics(IReadOnlyList<SurvivorsRelicDefinition> selected)
        {
            if (selected.Count == 0) return "none";
            const int maxShown = 3;
            string label = string.Empty;
            int shown = Mathf.Min(maxShown, selected.Count);
            for (int i = 0; i < shown; i++)
            {
                if (i > 0) label += ", ";
                SurvivorsRelicDefinition relic = selected[i];
                label += relic == null || string.IsNullOrWhiteSpace(relic.DisplayName) ? "Unknown Relic" : relic.DisplayName;
            }
            if (selected.Count > shown) label += " +" + (selected.Count - shown).ToString();
            return label;
        }

        internal static string Waystone(bool enabled, int discovered, bool hasUndiscovered, float distance,
            Vector3 delta, bool hasAnyLandmark)
        {
            if (!enabled) return $"Explore Waystones off   Found {discovered}";
            if (hasUndiscovered) return $"Explore Waystone {SurvivorsThreatHudModel.CompassDirection(delta)} {distance:0}m   Found {discovered}";
            if (hasAnyLandmark) return $"Explore Waystone new grid   Found {discovered}";
            return $"Explore Waystone --   Found {discovered}";
        }
    }
}
