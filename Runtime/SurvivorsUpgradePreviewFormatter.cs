using System;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Formats existing effect descriptions against current display values; it never applies effects.</summary>
    internal sealed class SurvivorsUpgradePreviewFormatter
    {
        private readonly SurvivorsRunBuildState _build;
        private readonly Func<string, string> _shortWeaponName;

        public SurvivorsUpgradePreviewFormatter(SurvivorsRunBuildState build, Func<string, string> shortWeaponName)
        {
            _build = build ?? throw new ArgumentNullException(nameof(build));
            _shortWeaponName = shortWeaponName ?? throw new ArgumentNullException(nameof(shortWeaponName));
        }

        public string ResolveUpgradeEffectPreview(RunUpgradeDefinition choice, in SurvivorsDraftPreviewValues values)
        {
            if (choice == null || choice.Effects == null || choice.Effects.Count == 0)
            {
                return "No effect preview.";
            }

            string label = string.Empty;
            if (_build.TryGetUpgradeMetadata(choice.Id.Value, out SurvivorsRunUpgradeMetadata metadata) && metadata.IsEvolution)
            {
                label = "Evolves into " + _build.ResolveUpgradeDisplayName(choice.Id);
            }

            int shown = Mathf.Min(2, choice.Effects.Count);
            for (int i = 0; i < shown; i++)
            {
                string preview = FormatUpgradeEffectPreview(choice, choice.Effects[i], values);
                if (string.IsNullOrWhiteSpace(preview))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(label))
                {
                    label += "; ";
                }

                label += preview;
            }

            if (choice.Effects.Count > shown)
            {
                label += "; +" + (choice.Effects.Count - shown).ToString() + " more";
            }

            return string.IsNullOrWhiteSpace(label) ? "Adds behavior." : label;
        }

        private string FormatUpgradeEffectPreview(RunUpgradeDefinition choice, RunUpgradeEffectDescriptor effect, in SurvivorsDraftPreviewValues values)
        {
            float amount = (float)effect.Amount;
            if (effect.EffectId.Equals(BasicSurvivorsGame.DamageBonusEffect)) return FormatComparison("Damage", values.ProjectileDamage, values.ProjectileDamage + amount);
            if (effect.EffectId.Equals(BasicSurvivorsGame.FireRateEffect)) return FormatComparison("Cooldown", values.WeaponCooldownSeconds, Mathf.Max(0.12f, values.WeaponCooldownSeconds * Mathf.Max(0.2f, 1f + amount)), "0.00s");
            if (effect.EffectId.Equals(BasicSurvivorsGame.MoveSpeedEffect)) return FormatComparison("Move Speed", values.PlayerMoveSpeed, values.PlayerMoveSpeed + amount);
            if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetRangeEffect)) return FormatComparison("Pickup Radius", values.CurrentPickupAttractRange, values.CurrentPickupAttractRange + amount);
            if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetSpeedEffect)) return FormatComparison("Magnet Pull Speed", values.CurrentPickupAttractionSpeed, values.CurrentPickupAttractionSpeed + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetPulseEffect)) return FormatPulseIntervalComparison(amount, values);
            if (effect.EffectId.Equals(BasicSurvivorsGame.MaxHealthEffect)) return FormatComparison("Max Health", values.MaxHealth, values.MaxHealth + amount);
            if (effect.EffectId.Equals(BasicSurvivorsGame.OrbitBladeEffect)) return FormatComparison("Orbit Blades", values.OrbitBladeBonus, values.OrbitBladeBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.OrbitRadiusEffect)) return FormatComparison("Orbit Radius", values.OrbitRadiusBonus, values.OrbitRadiusBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.MeleeTargetEffect)) return FormatComparison("Slash Targets", values.MeleeTargetBonus, values.MeleeTargetBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.BurstCountEffect)) return FormatComparison("Burst Pulses", values.BurstCountBonus, values.BurstCountBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.BurstEchoEffect)) return FormatComparison("Echo Pulses", values.BurstEchoBonus, values.BurstEchoBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.TargetedBurstEffect)) return FormatComparison("Targeted Sigils", values.TargetedBurstSigilBonus, values.TargetedBurstSigilBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileFanEffect)) return FormatComparison("Projectiles", values.ProjectileFanBonus + 1, values.ProjectileFanBonus + 1 + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectilePierceEffect)) return FormatComparison("Pierce", values.ProjectilePierceBonus, values.ProjectilePierceBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileChainEffect)) return FormatComparison("Chain Count", values.ProjectileChainBonus, values.ProjectileChainBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileForkEffect)) return FormatComparison("Forks", values.ProjectileForkBonus, values.ProjectileForkBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileReturnEffect)) return FormatComparison("Returns", values.ProjectileReturnBonus, values.ProjectileReturnBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.HitscanPierceEffect)) return FormatComparison("Beam Pierce", values.HitscanPierceBonus, values.HitscanPierceBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.PayloadCountEffect)) return FormatComparison("Payloads", values.PayloadCountBonus, values.PayloadCountBonus + Mathf.Max(1, Mathf.RoundToInt(amount)), "0");
            if (effect.EffectId.Equals(BasicSurvivorsGame.PayloadRadiusEffect)) return FormatComparison("Area Radius", values.PayloadExplosionRadiusBonus, values.PayloadExplosionRadiusBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.PayloadTriggerRadiusEffect)) return FormatComparison("Trigger Range", values.PayloadTriggerRadiusBonus, values.PayloadTriggerRadiusBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.PoisonEffect)) return FormatPercentComparison("Poison", values.PoisonDamageRatio, values.PoisonDamageRatio + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.BleedEffect)) return FormatPercentComparison("Bleed", values.BleedDamageRatio, values.BleedDamageRatio + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.ExecuteEffect)) return FormatPercentComparison("Execute Threshold", values.ExecuteThresholdNormalized, Mathf.Clamp01(values.ExecuteThresholdNormalized + amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.CriticalChanceEffect)) return FormatPercentComparison("Crit Chance", values.CriticalChanceNormalized, Mathf.Clamp01(values.CriticalChanceNormalized + Mathf.Max(0f, amount)));
            if (effect.EffectId.Equals(BasicSurvivorsGame.CriticalDamageEffect)) return FormatComparison("Crit Damage", values.CriticalDamageMultiplier, values.CriticalDamageMultiplier + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.DraftLuckEffect)) return FormatPercentComparison("Draft Luck", values.DraftLuckBonus, values.DraftLuckBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.DeathNovaDamageEffect)) return FormatComparison("Death Nova Damage", values.DeathNovaDamage, values.DeathNovaDamage + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.DeathNovaRadiusEffect)) return FormatComparison("Death Nova Radius", values.DeathNovaRadius, values.DeathNovaRadius + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.LifestealEffect)) return FormatPercentComparison("Lifesteal", values.LifestealRatio, values.LifestealRatio + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.BarrierCapacityEffect)) return FormatComparison("Barrier", values.BarrierCapacity, values.BarrierCapacity + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.BarrierRegenEffect)) return FormatComparison("Barrier Regen", values.BarrierRegenPerSecondBonus, values.BarrierRegenPerSecondBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.BarrierOnDamageEffect)) return FormatPercentComparison("Barrier On Damage", values.BarrierOnDamageRatio, values.BarrierOnDamageRatio + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.ExperienceGainEffect)) return FormatPercentComparison("XP Gain", values.ExperienceGainMultiplierBonus, values.ExperienceGainMultiplierBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.AreaRadiusEffect)) return FormatComparison("Area Radius", values.AreaRadiusBonus, values.AreaRadiusBonus + Mathf.Max(0f, amount));
            if (effect.EffectId.Equals(BasicSurvivorsGame.WeaponUnlockEffect))
            {
                string displayName = choice != null &&
                    _build.TryGetUpgradeMetadata(choice.Id.Value, out SurvivorsRunUpgradeMetadata metadata) &&
                    !string.IsNullOrWhiteSpace(metadata.DisplayName)
                        ? metadata.DisplayName
                        : _shortWeaponName(effect.TargetId.Value);
                return "Unlocks " + displayName;
            }
            if (choice != null && IsPickupMagnetUpgrade(choice)) return "Improves collector effects.";
            return "Adds behavior.";
        }

        private static string FormatPulseIntervalComparison(float reduction, in SurvivorsDraftPreviewValues values)
        {
            float before = values.CurrentPickupMagnetPulseIntervalSeconds;
            float baseInterval = Mathf.Max(1f, values.PickupMagnetPulseBaseIntervalSeconds);
            float minimum = Mathf.Max(1f, values.PickupMagnetPulseMinimumIntervalSeconds);
            float after = before > 0f
                ? Mathf.Max(minimum, before - Mathf.Max(0f, reduction))
                : Mathf.Max(minimum, baseInterval - Mathf.Max(0f, reduction));
            string beforeLabel = before > 0f ? before.ToString("0.#") + "s" : "inactive";
            return "Pulse Interval: " + beforeLabel + " -> " + after.ToString("0.#") + "s";
        }

        private static string FormatComparison(string label, float before, float after, string format = "0.#")
        {
            return label + ": " + before.ToString(format) + " -> " + after.ToString(format);
        }

        private static string FormatComparison(string label, int before, int after, string format = "0")
        {
            return label + ": " + before.ToString(format) + " -> " + after.ToString(format);
        }

        private static string FormatPercentComparison(string label, float before, float after)
        {
            return label + ": " + before.ToString("P0") + " -> " + after.ToString("P0");
        }

        public static bool IsPickupMagnetUpgrade(RunUpgradeDefinition choice)
        {
            if (choice == null || choice.Effects == null)
            {
                return false;
            }

            for (int i = 0; i < choice.Effects.Count; i++)
            {
                RunUpgradeEffectDescriptor effect = choice.Effects[i];
                if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetRangeEffect) ||
                    effect.EffectId.Equals(BasicSurvivorsGame.MagnetSpeedEffect) ||
                    effect.EffectId.Equals(BasicSurvivorsGame.MagnetPulseEffect))
                {
                    return true;
                }
            }

            return false;
        }

        public string ResolveRelicEffectPreview(SurvivorsRelicDefinition relic)
        {
            if (relic == null)
            {
                return "No effect preview.";
            }

            switch (relic.EffectKind)
            {
                case SurvivorsRelicEffectKind.DamageBonus:
                    return $"+{relic.Amount:0.#} damage while this relic is held";
                case SurvivorsRelicEffectKind.CooldownMultiplier:
                    return $"{Mathf.Abs(relic.Amount):P0} faster attacks for {_shortWeaponName(relic.TargetId)}";
                case SurvivorsRelicEffectKind.PickupRange:
                    return $"+{relic.Amount:0.#} pickup radius and relic surge momentum";
                default:
                    return FormatRelicEffectSummary(relic);
            }
        }

        public string FormatRelicEffectSummary(SurvivorsRelicDefinition relic)
        {
            if (relic == null)
            {
                return "Missing effect";
            }

            string target = _shortWeaponName(relic.TargetId);
            switch (relic.EffectKind)
            {
                case SurvivorsRelicEffectKind.DamageBonus:
                    return $"+{relic.Amount:0.#} damage to {target}";
                case SurvivorsRelicEffectKind.CooldownMultiplier:
                    return $"{Mathf.Abs(relic.Amount):P0} faster cooldown on {target}";
                case SurvivorsRelicEffectKind.PickupRange:
                    return $"+{relic.Amount:0.#} pickup range";
                default:
                    return target;
            }
        }
    }
}
