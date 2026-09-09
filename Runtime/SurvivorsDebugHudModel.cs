using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Formats the legacy debug rows without acquiring or changing gameplay state.</summary>
    internal static class SurvivorsDebugHudModel
    {
        internal static IReadOnlyList<string> BuildRows(in SurvivorsDebugHudValues values)
        {
            var lines = new List<string>
            {
                $"LV {values.Vitals.Level}   Time {SurvivorsRunText.FormatRunTime(values.RunTimeSeconds)}   Phase {values.RunPhaseLabel} +{values.RunEscalationLevel}",
                values.MilestoneLabel,
                $"Enemies {values.ActiveEnemyCount}/{values.EnemyMaximumAlive}   Kills {values.KilledCount}",
                $"Split {values.SplitterCount}   Call {values.SummonerCount}   Elite {values.EliteCount}   Mini {values.MinibossCount}   Boss {values.BossCount}",
                $"{values.CurrencyDisplayName} {values.MetaBloodShards}   Poison {values.PoisonDamageRatio:0.##}   Bleed {values.BleedDamageRatio:0.##}   Execute {values.ExecuteThresholdNormalized:P0}",
                "Weapons: " + values.WeaponLabel,
                $"Mode {values.ModeDisplayName}   Profile {BasicSurvivorsGame.GetPacingProfileDisplayName(values.PacingProfile)}",
                $"Spawn {values.EnemySpawnIntervalSeconds:0.00}s   Enemy Speed x{values.EnemySpeedMultiplier:0.##}",
                $"Streak {values.CurrentKillStreak}   Best {values.BestKillStreak}   Bonus Drops {values.StreakBonusDropCount}{values.SurgeLabel}",
                $"Reward Timeout {SurvivorsRunText.FormatRewardTimeout(values.RewardSelectionTimeoutSeconds)}   Reroll {values.DraftRerollsRemaining}   Banish {values.DraftBanishesRemaining}",
                values.BuildSlotLabel,
                values.WaystoneCompass
            };
            if (!string.IsNullOrWhiteSpace(values.EvolutionObjective)) lines.Add(values.EvolutionObjective);
            lines.Add(values.DashLabel);
            return lines;
        }

        internal static float RunRatio(in SurvivorsDebugHudValues values)
            => Mathf.Clamp01(values.RunTimeSeconds / Mathf.Max(1f, values.SurvivalVictoryTimeSeconds));
    }
}
