using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Projects each requested build-menu tab from copied display observations.</summary>
    internal static class SurvivorsBuildMenuTextModel
    {
        public static IReadOnlyList<string> Stats(in SurvivorsBuildMenuStatsValues values)
        {
            return new List<string>
            {
                $"Damage +{values.DamageBonusTotal:0.#}   Surge +{values.SurgeDamageBonus:0.#}",
                $"Cooldown {values.WeaponCooldownSeconds:0.00}s   Move speed {values.PlayerMoveSpeed:0.0}",
                $"Health {values.CurrentHealth:0}/{values.MaxHealth:0}   Barrier {values.BarrierValue:0.#}/{values.BarrierCapacity:0.#}   Armor: contact safety {values.ContactInvulnerabilitySeconds:0.##}s",
                $"Pickup radius {values.CurrentPickupAttractRange:0.#}   Magnet range {values.CurrentPickupAttractRange:0.#}   Magnet speed {values.CurrentPickupAttractionSpeed:0.#}   Pulse {values.PickupPulseLabel}",
                $"XP gain +{values.ExperienceGainBonus:P0}   Draft luck +{values.DraftLuckBonus:P0}",
                $"Area/radius +{values.AreaRadiusBonus:0.#}   Orbit +{values.OrbitRadiusBonus:0.#}   Death nova {values.DeathNovaDamage:0.#}/{values.DeathNovaRadius:0.#}",
                $"Projectiles fan +{values.ProjectileFanBonus}   pierce +{values.ProjectilePierceBonus}   chain +{values.ProjectileChainBonus}   fork +{values.ProjectileForkBonus}   return +{values.ProjectileReturnBonus}",
                $"Payloads +{values.PayloadCountBonus}   payload radius +{values.PayloadExplosionRadiusBonus:0.#}   trigger +{values.PayloadTriggerRadiusBonus:0.#}",
                $"Status poison {values.PoisonDamageRatio:P0}   bleed {values.BleedDamageRatio:P0}   execute {values.ExecuteThresholdNormalized:P0}   lifesteal {values.LifestealRatio:P0}",
                $"Crit {values.CriticalChanceNormalized:P0} x{values.CriticalDamageMultiplier:0.0}"
            };
        }

        public static IReadOnlyList<string> RunInfo(in SurvivorsBuildMenuRunInfoValues values)
        {
            string remaining = values.IsEndlessRun
                ? "Endless"
                : SurvivorsRunText.FormatRunTime(Mathf.Max(0f, values.SurvivalVictoryTimeSeconds - values.RunTimeSeconds));
            return new List<string>
            {
                $"Mode: {values.CurrentRunModeDisplayName} ({values.PacingProfileLabel})",
                $"Elapsed {SurvivorsRunText.FormatRunTime(values.RunTimeSeconds)}   Remaining {remaining}",
                $"Milestone: {values.CurrentRunMilestoneHudLabel}",
                $"Phase {values.PhaseLabel} +{values.RunEscalationLevel}   Level {values.Level}   Kills {values.KilledCount}",
                $"Enemies {values.ActiveEnemyCount}/{values.CurrentEnemyMaximumAlive}   Elites {values.ActiveEliteCount}   Minibosses {values.ActiveMinibossCount}   Bosses {values.ActiveBossCount}",
                $"Run rewards: {values.CurrencyEarned} {values.CurrencyDisplayName}, {values.ProgressionEarned} {values.ProgressionDisplayName}",
                $"Meta bank: {values.MetaBloodShards} {values.CurrencyDisplayName}   {values.ProgressionDisplayName} {values.LifetimeLegacyExperience}",
                $"Rerolls {values.DraftRerollsRemaining}   Banishes {values.DraftBanishesRemaining}   Skip reward +{values.DraftSkipBloodShards} {values.CurrencyRewardLabel}",
                $"Waystones {values.WaystoneDiscoveryCount}   Roaming caches {values.RoamingCacheDropCount}   Arena trials {values.ArenaShrineTrialCount}"
            };
        }

        public static IReadOnlyList<string> Controls()
        {
            return new List<string>
            {
                "Move: WASD or arrow keys",
                "Arc Step: Space",
                "Draft choice: mouse or 1/2/3",
                "Draft tools: R reroll, S skip, Shift+1/2/3 banish",
                "Build menu: Esc, Tab, or B opens and closes",
                "Build menu tabs: 1 Current Build, 2 Stats, 3 Run Info, 4 Controls",
                "Debug overlay: F1",
                "Victory: C continues Standard into endless if available",
                "Result screen: Restart Same or Change Mode buttons"
            };
        }

        public static IReadOnlyList<string> CurrentBuild(IReadOnlyList<string> buildLines)
        {
            var lines = new List<string>(buildLines);
            if (lines.Count == 0)
            {
                lines.Add("No build data yet.");
            }

            return lines;
        }
    }
}
