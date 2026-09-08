using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Formats a copied build observation, then appends live objective and rank queries.</summary>
    internal sealed class SurvivorsDebugBuildModel
    {
        private readonly SurvivorsDebugUpgradeFormatter _formatter;
        private readonly Func<string> _evolutionObjective;
        public SurvivorsDebugBuildModel(SurvivorsDebugUpgradeFormatter formatter, Func<string> evolutionObjective)
        { _formatter = formatter; _evolutionObjective = evolutionObjective; }

        public IReadOnlyList<string> Describe(in SurvivorsDebugBuildValues values)
        {
            var lines = new List<string>
            {
                $"Weapons {values.ActiveWeaponCount}/{values.MaxWeaponSlots}: {values.ActiveWeaponList}",
                $"Passives {values.ActivePassiveCount}/{values.MaxPassiveSlots}, Evolutions {values.EvolvedWeaponCount}",
                $"Relics {values.SelectedRelicCount}/{values.TotalRelicCount}: {values.SelectedRelicList}",
                $"Stats: damage +{values.DamageBonus:0.#} surge +{values.SurgeDamageBonus:0.#}, crit {values.CriticalChanceNormalized:P0} x{values.CriticalDamageMultiplier:0.0}, luck +{values.DraftLuckBonus:P0}, cooldown {values.WeaponCooldownSeconds:0.00}s, move {values.PlayerMoveSpeed:0.0}, pickup {values.CurrentPickupAttractRange:0.#}, pull {values.CurrentPickupAttractionSpeed:0.#}, pulse {SurvivorsRunText.FormatMetricTime(values.CurrentPickupMagnetPulseIntervalSeconds)}, XP +{values.TotalExperienceGainBonus:P0}",
                $"Projectiles: fan +{values.ProjectileFanBonus}, pierce +{values.ProjectilePierceBonus}, chain +{values.ProjectileChainBonus}, fork +{values.ProjectileForkBonus}, return +{values.ProjectileReturnBonus}",
                $"Area: global +{values.AreaRadiusBonus:0.#}, orbit +{values.OrbitRadiusBonus:0.#}, burst +{values.BurstCountBonus}, echoes +{values.BurstEchoBonus}, payload +{values.PayloadCountBonus}, death nova {values.DeathNovaDamage:0.#}/{values.DeathNovaRadius:0.#}",
                $"Status: poison {values.PoisonDamageRatio:P0}, bleed {values.BleedDamageRatio:P0}, execute {values.ExecuteThresholdNormalized:P0}, lifesteal {values.LifestealRatio:P0}"
            };

            string evolutionObjective = _evolutionObjective();
            if (!string.IsNullOrWhiteSpace(evolutionObjective))
            {
                lines.Add(evolutionObjective);
            }

            _formatter.AppendSelectedUpgradeRankLines(lines);
            return lines;
        }
    }
}
