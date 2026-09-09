using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Read-only endless reward scaling shared by the exploration encounters.</summary>
    internal readonly struct SurvivorsExplorationBonuses
    {
        public SurvivorsExplorationBonuses(bool clearedVictory, int surgeTier)
        {
            Tier = clearedVictory ? Mathf.Clamp(Mathf.Max(1, surgeTier), 1, 12) : 0;
        }
        public int Tier { get; }
        public float ExperienceMultiplier => Tier <= 0 ? 1f : Mathf.Clamp(1f + Tier * 0.12f, 1f, 2.5f);
        public float PulseMultiplier => Tier <= 0 ? 1f : Mathf.Clamp(1f + Tier * 0.08f, 1f, 2f);
        public int GemBonus => Tier <= 0 ? 0 : Mathf.Min(6, (Tier + 1) / 2);
        public int PressureBonus => Tier <= 0 ? 0 : Mathf.Min(8, 1 + Tier / 2);
        public int ShardBonus => Tier <= 1 ? 0 : Mathf.Min(4, 1 + Tier / 3);
        public string LabelSuffix => Tier <= 0 ? string.Empty : $" + Endless T{Tier}";
    }
}
