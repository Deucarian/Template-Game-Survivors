using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Copied authored mode text; no configuration or start command is retained by the card.</summary>
    internal readonly struct SurvivorsRunModeCardView
    {
        public readonly string Title, Duration, Description, Milestones, Profile;
        public SurvivorsRunModeCardView(SurvivorsPacingProfile profile, SurvivorsTemplateTuning preview, string milestones)
        {
            Title = ResolveTitle(profile, preview);
            Duration = preview.RunModeDurationLabel;
            Description = preview.RunModeDescription;
            Milestones = milestones;
            Profile = "Profile " + BasicSurvivorsGame.GetPacingProfileDisplayName(profile);
        }
        public static string ResolveTitle(SurvivorsPacingProfile profile, SurvivorsTemplateTuning preview)
        {
            if (profile == SurvivorsPacingProfile.HumanPlaytest)
            {
                return "Standard / Human Playtest";
            }

            return preview == null || string.IsNullOrWhiteSpace(preview.RunModeDisplayName)
                ? BasicSurvivorsGame.GetPacingProfileDisplayName(profile)
                : preview.RunModeDisplayName;
        }
    }
}
