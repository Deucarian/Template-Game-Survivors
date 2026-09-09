using System;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Applies template-owned stat policy; health and pickup scheduling are explicit effects.</summary>
    internal sealed class SurvivorsUpgradeModifiers
    {
        private readonly ISurvivorsUpgradeEffectSink _effects;

        public SurvivorsUpgradeModifiers(ISurvivorsUpgradeEffectSink effects)
        {
            _effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }

        public float MoveSpeedBonus { get; private set; }
        public float DamageBonus { get; private set; }
        public float PersistentDamageBonus { get; private set; }
        public float PersistentMaxHealthBonus { get; private set; }
        public float PersistentPickupRangeBonus { get; private set; }
        public float PersistentExperienceGainMultiplierBonus { get; private set; }
        public int PersistentDraftRerollBonus { get; private set; }
        public float RelicDamageBonus { get; private set; }
        public float RelicCooldownMultiplierBonus { get; private set; }
        public float RelicPickupRangeBonus { get; private set; }
        public float WeaponCooldownMultiplierBonus { get; private set; }
        public float PickupRangeBonus { get; private set; }
        public float BarrierCapacityBonus { get; private set; }
        public float BarrierRegenPerSecondBonus { get; private set; }
        public float BarrierOnDamageRatio { get; private set; }
        public float PoisonDamageRatio { get; private set; }
        public float BleedDamageRatio { get; private set; }
        public float ExecuteThresholdNormalized { get; private set; }
        public float CriticalChanceBonus { get; private set; }
        public float CriticalDamageMultiplierBonus { get; private set; }
        public float DraftLuckBonus { get; private set; }
        public float DeathNovaDamageBonus { get; private set; }
        public float DeathNovaRadiusBonus { get; private set; }
        public float LifestealRatio { get; private set; }
        public float ExperienceGainMultiplierBonus { get; private set; }
        public float AreaRadiusBonus { get; private set; }
        public int ProjectileFanBonus { get; private set; }
        public int OrbitBladeBonus { get; private set; }
        public float OrbitRadiusBonus { get; private set; }
        public int MeleeTargetBonus { get; private set; }
        public int BurstCountBonus { get; private set; }
        public int BurstEchoBonus { get; private set; }
        public int TargetedBurstSigilBonus { get; private set; }
        public int ProjectilePierceBonus { get; private set; }
        public int ProjectileChainBonus { get; private set; }
        public int ProjectileForkBonus { get; private set; }
        public int ProjectileReturnBonus { get; private set; }
        public int HitscanPierceBonus { get; private set; }
        public int PayloadCountBonus { get; private set; }
        public float PayloadExplosionRadiusBonus { get; private set; }
        public float PayloadTriggerRadiusBonus { get; private set; }
        public float PickupAttractionSpeedBonus { get; private set; }
        public float PickupMagnetPulseIntervalReductionBonus { get; private set; }

        public void Reset()
        {
            MoveSpeedBonus = 0f;
            DamageBonus = 0f;
            PersistentDamageBonus = 0f;
            PersistentMaxHealthBonus = 0f;
            PersistentPickupRangeBonus = 0f;
            PersistentExperienceGainMultiplierBonus = 0f;
            PersistentDraftRerollBonus = 0;
            RelicDamageBonus = 0f;
            RelicCooldownMultiplierBonus = 0f;
            RelicPickupRangeBonus = 0f;
            WeaponCooldownMultiplierBonus = 0f;
            PickupRangeBonus = 0f;
            BarrierCapacityBonus = 0f;
            BarrierRegenPerSecondBonus = 0f;
            BarrierOnDamageRatio = 0f;
            PoisonDamageRatio = 0f;
            BleedDamageRatio = 0f;
            ExecuteThresholdNormalized = 0f;
            CriticalChanceBonus = 0f;
            CriticalDamageMultiplierBonus = 0f;
            DraftLuckBonus = 0f;
            DeathNovaDamageBonus = 0f;
            DeathNovaRadiusBonus = 0f;
            LifestealRatio = 0f;
            ExperienceGainMultiplierBonus = 0f;
            AreaRadiusBonus = 0f;
            ProjectileFanBonus = 0;
            OrbitBladeBonus = 0;
            OrbitRadiusBonus = 0f;
            MeleeTargetBonus = 0;
            BurstCountBonus = 0;
            BurstEchoBonus = 0;
            TargetedBurstSigilBonus = 0;
            ProjectilePierceBonus = 0;
            ProjectileChainBonus = 0;
            ProjectileForkBonus = 0;
            ProjectileReturnBonus = 0;
            HitscanPierceBonus = 0;
            PayloadCountBonus = 0;
            PayloadExplosionRadiusBonus = 0f;
            PayloadTriggerRadiusBonus = 0f;
            PickupAttractionSpeedBonus = 0f;
            PickupMagnetPulseIntervalReductionBonus = 0f;
        }

        public void ApplyPersistent(SurvivorsPersistentBonuses bonuses)
        {
            DamageBonus += bonuses.Damage - PersistentDamageBonus;
            PickupRangeBonus += bonuses.PickupRange - PersistentPickupRangeBonus;
            ExperienceGainMultiplierBonus += bonuses.ExperienceGain - PersistentExperienceGainMultiplierBonus;
            float healthDelta = bonuses.MaxHealth - PersistentMaxHealthBonus;
            PersistentDamageBonus = bonuses.Damage;
            PersistentMaxHealthBonus = bonuses.MaxHealth;
            PersistentPickupRangeBonus = bonuses.PickupRange;
            PersistentExperienceGainMultiplierBonus = bonuses.ExperienceGain;
            PersistentDraftRerollBonus = Mathf.Max(0, Mathf.RoundToInt(bonuses.DraftRerolls));
            if (Mathf.Abs(healthDelta) > 0.001f)
            {
                _effects.IncreaseMaximumHealth(healthDelta);
            }
        }

        public void ApplyClass(SurvivorsClassDefinition selectedClass)
        {
            if (selectedClass == null)
            {
                return;
            }

            for (int i = 0; i < selectedClass.StartingStatModifiers.Count; i++)
            {
                SurvivorsClassStatModifierDefinition modifier = selectedClass.StartingStatModifiers[i];
                if (modifier == null)
                {
                    continue;
                }

                if (modifier.StatKind == SurvivorsClassStatKind.MoveSpeed)
                {
                    MoveSpeedBonus += modifier.Amount;
                }
                else if (modifier.StatKind == SurvivorsClassStatKind.Damage)
                {
                    DamageBonus += modifier.Amount;
                }
                else if (modifier.StatKind == SurvivorsClassStatKind.MaxHealth)
                {
                    _effects.IncreaseMaximumHealth(modifier.Amount);
                }
            }
        }

        public void ApplyRelic(SurvivorsRelicDefinition relic)
        {
            if (relic == null)
            {
                return;
            }

            if (relic.EffectKind == SurvivorsRelicEffectKind.DamageBonus)
            {
                RelicDamageBonus += relic.Amount;
                DamageBonus += relic.Amount;
            }
            else if (relic.EffectKind == SurvivorsRelicEffectKind.CooldownMultiplier)
            {
                RelicCooldownMultiplierBonus += relic.Amount;
                WeaponCooldownMultiplierBonus = Mathf.Max(-0.75f, WeaponCooldownMultiplierBonus + relic.Amount);
            }
            else if (relic.EffectKind == SurvivorsRelicEffectKind.PickupRange)
            {
                RelicPickupRangeBonus += relic.Amount;
                PickupRangeBonus += relic.Amount;
            }
        }

        public void Apply(RunUpgradeDefinition upgrade)
        {
            for (int i = 0; i < upgrade.Effects.Count; i++)
            {
                RunUpgradeEffectDescriptor effect = upgrade.Effects[i];
                if (effect.EffectId.Equals(BasicSurvivorsGame.DamageBonusEffect))
                {
                    DamageBonus += (float)effect.Amount;
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.FireRateEffect))
                {
                    WeaponCooldownMultiplierBonus = Mathf.Max(-0.75f, WeaponCooldownMultiplierBonus + (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.MoveSpeedEffect))
                {
                    MoveSpeedBonus += (float)effect.Amount;
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetRangeEffect))
                {
                    PickupRangeBonus += (float)effect.Amount;
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetSpeedEffect))
                {
                    PickupAttractionSpeedBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.MagnetPulseEffect))
                {
                    PickupMagnetPulseIntervalReductionBonus += Mathf.Max(0f, (float)effect.Amount);
                    _effects.ScheduleMagnetPulse();
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.MaxHealthEffect))
                {
                    _effects.IncreaseMaximumHealth(effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.OrbitBladeEffect))
                {
                    OrbitBladeBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.OrbitRadiusEffect))
                {
                    OrbitRadiusBonus += Mathf.Max(0f, (float)effect.Amount);
                    OrbitBladeBonus += 1;
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.MeleeTargetEffect))
                {
                    MeleeTargetBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.BurstCountEffect))
                {
                    BurstCountBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.BurstEchoEffect))
                {
                    BurstEchoBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.TargetedBurstEffect))
                {
                    TargetedBurstSigilBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileFanEffect))
                {
                    ProjectileFanBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectilePierceEffect))
                {
                    ProjectilePierceBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileChainEffect))
                {
                    ProjectileChainBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileForkEffect))
                {
                    ProjectileForkBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ProjectileReturnEffect))
                {
                    ProjectileReturnBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.HitscanPierceEffect))
                {
                    HitscanPierceBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.PayloadCountEffect))
                {
                    PayloadCountBonus += Mathf.Max(1, Mathf.RoundToInt((float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.PayloadRadiusEffect))
                {
                    PayloadExplosionRadiusBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.PayloadTriggerRadiusEffect))
                {
                    PayloadTriggerRadiusBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.PoisonEffect))
                {
                    PoisonDamageRatio += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.BleedEffect))
                {
                    BleedDamageRatio += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ExecuteEffect))
                {
                    ExecuteThresholdNormalized = Mathf.Clamp01(ExecuteThresholdNormalized + (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.CriticalChanceEffect))
                {
                    CriticalChanceBonus = Mathf.Clamp01(CriticalChanceBonus + Mathf.Max(0f, (float)effect.Amount));
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.CriticalDamageEffect))
                {
                    CriticalDamageMultiplierBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.DraftLuckEffect))
                {
                    DraftLuckBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.DeathNovaDamageEffect))
                {
                    DeathNovaDamageBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.DeathNovaRadiusEffect))
                {
                    DeathNovaRadiusBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.LifestealEffect))
                {
                    LifestealRatio += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.BarrierCapacityEffect))
                {
                    BarrierCapacityBonus += Mathf.Max(0f, (float)effect.Amount);
                    _effects.RestoreBarrier((float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.BarrierRegenEffect))
                {
                    BarrierRegenPerSecondBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.BarrierOnDamageEffect))
                {
                    BarrierOnDamageRatio += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.ExperienceGainEffect))
                {
                    ExperienceGainMultiplierBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.AreaRadiusEffect))
                {
                    AreaRadiusBonus += Mathf.Max(0f, (float)effect.Amount);
                }
                else if (effect.EffectId.Equals(BasicSurvivorsGame.WeaponUnlockEffect))
                {
                    // Weapon unlocks are applied through run-build metadata so slot rules stay centralized.
                }
            }

        }
    }
}
