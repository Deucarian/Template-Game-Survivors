using System.Collections.Generic;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Orders the compact player HUD rows and optional evolution hint without reading gameplay state.</summary>
    internal static class SurvivorsPlayerHudTextModel
    {
        public static IReadOnlyList<string> BuildLines(in SurvivorsPlayerHudValues values)
        {
            var lines = new List<string>(6)
            {
                values.MilestoneLabel,
                values.BuildSlotLabel,
                "Weapons: " + values.ActiveWeaponLabel,
                $"Pickup {values.CurrentPickupAttractRange:0.#}   Pull {values.CurrentPickupAttractionSpeed:0.#}   Pulse {values.PickupPulseLabel}"
            };

            string evolution = values.EvolutionLabel;
            if (!string.IsNullOrWhiteSpace(evolution))
            {
                lines.Add(evolution);
            }

            lines.Add("Tab/B Build Menu");
            return lines;
        }
    }
}
