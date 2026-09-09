using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsRuntimeWorldPalette
    {
        internal readonly Color Player;
        internal readonly Color Experience;
        internal readonly Color Magnet;
        internal readonly Color Health;
        internal readonly Color BloodShard;
        internal readonly Color Projectile;
        internal readonly Color TrailStart;
        internal readonly Color TrailEnd;

        internal SurvivorsRuntimeWorldPalette(Color player, Color experience, Color magnet, Color health,
            Color bloodShard, Color projectile, Color trailStart, Color trailEnd)
        {
            Player = player;
            Experience = experience;
            Magnet = magnet;
            Health = health;
            BloodShard = bloodShard;
            Projectile = projectile;
            TrailStart = trailStart;
            TrailEnd = trailEnd;
        }

        internal static SurvivorsRuntimeWorldPalette Capture(SurvivorsUiTheme theme)
        {
            if (theme == null) throw new ArgumentNullException(nameof(theme));
            return new SurvivorsRuntimeWorldPalette(
                theme.GetPlayerColor(new Color(0.78f, 0.91f, 1f)),
                theme.GetExperiencePickupColor(new Color(0.22f, 0.83f, 1f)),
                theme.GetArenaAccentColor(new Color(1f, 0.86f, 0.18f)),
                theme.GetHealthPickupColor(new Color(1f, 0.24f, 0.42f)),
                theme.GetCurrencyPickupColor(new Color(0.96f, 0.08f, 0.18f)),
                theme.GetFeedbackAccentColor(new Color(0.78f, 0.42f, 1f)),
                SurvivorsPrimitivePresentation.WithAlpha(theme.GetFeedbackAccentColor(new Color(0.86f, 0.48f, 1f)), 0.95f),
                SurvivorsPrimitivePresentation.WithAlpha(theme.GetArenaAccentColor(new Color(0.18f, 0.86f, 1f)), 0f));
        }
    }
}
