using System;

using System.Collections.Generic;

using Deucarian.Common;

using Deucarian.Combat;

using Deucarian.GameplayFoundation;

using Deucarian.Persistence;

using Deucarian.Persistence.Unity;

using Deucarian.Projectiles;

using Deucarian.RunUpgrades;

using Deucarian.WeaponSystems;

using Deucarian.WorldSpawning;

using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    // Upgrade application, build catalog and modifier bindings.
    public sealed partial class SurvivorsTemplateController
    {
        private SurvivorsEvolutionAnnouncements EvolutionAnnouncements => _evolutionAnnouncements ??
            (_evolutionAnnouncements = new SurvivorsEvolutionAnnouncements(RunBuild, DraftOffers.Catalogs,
                RecordEvolutionGoalFeedback, RecordEvolutionReadyFeedback));

        private SurvivorsBuildSurgeRewards BuildSurges => _buildSurges ?? (_buildSurges = new SurvivorsBuildSurgeRewards(RunBuild, this));

        SurvivorsTemplateTuning ISurvivorsBuildSurgePort.Tuning => CurrentTuning;

        Vector3 ISurvivorsBuildSurgePort.PlayerPosition => PlayerPosition;

        int ISurvivorsBuildSurgePort.WeaponCount => ActiveWeaponCount;

        int ISurvivorsBuildSurgePort.DamageNonMajor(Vector3 position, float radius, float damage, string source) => DamageNonMajorEnemies(position, radius, damage, source);

        int ISurvivorsBuildSurgePort.RecallGems() => StartMagnetRecall();

        Color ISurvivorsBuildSurgePort.RelicAccent(SurvivorsRelicDefinition relic) => SurvivorsDraftCardFactory.ResolveRelicAccentColor(relic);

        void ISurvivorsBuildSurgePort.ShowFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);

        void ISurvivorsBuildSurgePort.PlayPulse(int count, bool boss) => PlayFeedback(boss ? _bossPulse : _levelUpPulse, PlayerPosition, count, boss ? _bossClip : _levelUpClip);

        private SurvivorsRunBuildState RunBuild => _runBuild ?? (_runBuild = new SurvivorsRunBuildState(this));

        private string ResolveUpgradeDisplayName(RunUpgradeId id) => RunBuild.ResolveUpgradeDisplayName(id);

        SurvivorsTemplateTuning ISurvivorsRunBuildPort.Tuning => CurrentTuning;

        int ISurvivorsRunBuildPort.WeaponCount => ActiveWeaponCount;

        bool ISurvivorsRunBuildPort.HasWeapon(string id) => RunWeapons.ContainsWeapon(id);

        void ISurvivorsRunBuildPort.AddWeapon(string id) => TryAddWeaponToLoadout(id);

        void ISurvivorsRunBuildPort.PassiveAdded(RunUpgradeDefinition upgrade) => BuildSurges.TryTriggerPassiveLoadoutSurge(upgrade);

        void ISurvivorsRunBuildPort.RecordEvolutionTime() => Telemetry.Record(SurvivorsRunMetric.FirstEvolutionAcquired, RunTimeSeconds);

        void ISurvivorsRunBuildPort.EvolutionAdded(RunUpgradeDefinition upgrade)
        {
            BuildSurges.TriggerWeaponEvolutionSurge(upgrade);
            BuildSurges.TriggerEvolutionChainSurge(upgrade);
        }

        private SurvivorsUpgradeModifiers UpgradeModifiers => _upgradeModifiers ?? (_upgradeModifiers = new SurvivorsUpgradeModifiers(this));

        public int BossRelicSurgeCount => BuildSurges.BossRelicSurgeCount;

        public int BossRelicSurgeHitCount => BuildSurges.BossRelicSurgeHitCount;

        public string LastBossRelicSurgeFeedbackLabel => BuildSurges.LastBossRelicSurgeFeedbackLabel;

        public int WeaponLoadoutSurgeActivationCount => BuildSurges.WeaponLoadoutSurgeActivationCount;

        public int WeaponLoadoutSurgePulseHitCount => BuildSurges.WeaponLoadoutSurgePulseHitCount;

        public string LastWeaponLoadoutSurgeFeedbackLabel => BuildSurges.LastWeaponLoadoutSurgeFeedbackLabel;

        public int PassiveLoadoutSurgeActivationCount => BuildSurges.PassiveLoadoutSurgeActivationCount;

        public int PassiveLoadoutSurgePulseHitCount => BuildSurges.PassiveLoadoutSurgePulseHitCount;

        public string LastPassiveLoadoutSurgeFeedbackLabel => BuildSurges.LastPassiveLoadoutSurgeFeedbackLabel;

        public int WeaponEvolutionFeedbackCount => RunBuild.WeaponEvolutionFeedbackCount;

        public int WeaponEvolutionSurgeCount => BuildSurges.WeaponEvolutionSurgeCount;

        public int WeaponEvolutionSurgeHitCount => BuildSurges.WeaponEvolutionSurgeHitCount;

        public int EvolutionMagnetRecallCount => BuildSurges.EvolutionMagnetRecallCount;

        public int EvolutionMagnetRecallGemCount => BuildSurges.EvolutionMagnetRecallGemCount;

        public int EvolutionChainSurgeActivationCount => BuildSurges.EvolutionChainSurgeActivationCount;

        public int EvolutionChainSurgePulseHitCount => BuildSurges.EvolutionChainSurgePulseHitCount;

        public string LastWeaponEvolutionSurgeFeedbackLabel => BuildSurges.LastWeaponEvolutionSurgeFeedbackLabel;

        public string LastEvolutionMagnetRecallFeedbackLabel => BuildSurges.LastEvolutionMagnetRecallFeedbackLabel;

        public string LastEvolutionChainSurgeFeedbackLabel => BuildSurges.LastEvolutionChainSurgeFeedbackLabel;

        public bool IsWeaponLoadoutSurgeActive => BuildSurges.IsWeaponLoadoutSurgeActive;

        public float WeaponLoadoutSurgeRemainingSeconds => BuildSurges.WeaponLoadoutSurgeRemainingSeconds;

        public float WeaponLoadoutSurgeDamageBonus => BuildSurges.WeaponLoadoutSurgeDamageBonus;

        public float WeaponLoadoutSurgeMoveSpeedBonus => BuildSurges.WeaponLoadoutSurgeMoveSpeedBonus;

        public float WeaponLoadoutSurgeCooldownMultiplierBonus => BuildSurges.WeaponLoadoutSurgeCooldownMultiplierBonus;

        public float WeaponLoadoutSurgePickupRangeBonus => BuildSurges.WeaponLoadoutSurgePickupRangeBonus;

        public bool IsPassiveLoadoutSurgeActive => BuildSurges.IsPassiveLoadoutSurgeActive;

        public float PassiveLoadoutSurgeRemainingSeconds => BuildSurges.PassiveLoadoutSurgeRemainingSeconds;

        public float PassiveLoadoutSurgeDamageBonus => BuildSurges.PassiveLoadoutSurgeDamageBonus;

        public float PassiveLoadoutSurgeMoveSpeedBonus => BuildSurges.PassiveLoadoutSurgeMoveSpeedBonus;

        public float PassiveLoadoutSurgeCooldownMultiplierBonus => BuildSurges.PassiveLoadoutSurgeCooldownMultiplierBonus;

        public float PassiveLoadoutSurgePickupRangeBonus => BuildSurges.PassiveLoadoutSurgePickupRangeBonus;

        public float PassiveLoadoutSurgeExperienceGainMultiplierBonus => BuildSurges.PassiveLoadoutSurgeExperienceGainMultiplierBonus;

        public bool IsBossRelicSurgeActive => BuildSurges.IsBossRelicSurgeActive;

        public float BossRelicSurgeRemainingSeconds => BuildSurges.BossRelicSurgeRemainingSeconds;

        public float BossRelicSurgeDamageBonus => BuildSurges.BossRelicSurgeDamageBonus;

        public float BossRelicSurgeMoveSpeedBonus => BuildSurges.BossRelicSurgeMoveSpeedBonus;

        public float BossRelicSurgeCooldownMultiplierBonus => BuildSurges.BossRelicSurgeCooldownMultiplierBonus;

        public float BossRelicSurgePickupRangeBonus => BuildSurges.BossRelicSurgePickupRangeBonus;

        public bool IsEvolutionChainSurgeActive => BuildSurges.IsEvolutionChainSurgeActive;

        public float EvolutionChainSurgeRemainingSeconds => BuildSurges.EvolutionChainSurgeRemainingSeconds;

        public float EvolutionChainSurgeDamageBonus => BuildSurges.EvolutionChainSurgeDamageBonus;

        public float EvolutionChainSurgeMoveSpeedBonus => BuildSurges.EvolutionChainSurgeMoveSpeedBonus;

        public float EvolutionChainSurgeCooldownMultiplierBonus => BuildSurges.EvolutionChainSurgeCooldownMultiplierBonus;

        public float EvolutionChainSurgePickupRangeBonus => BuildSurges.EvolutionChainSurgePickupRangeBonus;

        public float MoveSpeedBonus => UpgradeModifiers.MoveSpeedBonus;

        public float DamageBonus => UpgradeModifiers.DamageBonus;

        public float PersistentDamageBonus => UpgradeModifiers.PersistentDamageBonus;

        public float PersistentMaxHealthBonus => UpgradeModifiers.PersistentMaxHealthBonus;

        public float PersistentPickupRangeBonus => UpgradeModifiers.PersistentPickupRangeBonus;

        public float PersistentExperienceGainMultiplierBonus => UpgradeModifiers.PersistentExperienceGainMultiplierBonus;

        public int PersistentDraftRerollBonus => UpgradeModifiers.PersistentDraftRerollBonus;

        public float RelicDamageBonus => UpgradeModifiers.RelicDamageBonus;

        public float RelicCooldownMultiplierBonus => UpgradeModifiers.RelicCooldownMultiplierBonus;

        public float RelicPickupRangeBonus => UpgradeModifiers.RelicPickupRangeBonus;

        public float WeaponCooldownMultiplierBonus => UpgradeModifiers.WeaponCooldownMultiplierBonus;

        public float PickupRangeBonus => UpgradeModifiers.PickupRangeBonus;

        public float BarrierCapacityBonus => UpgradeModifiers.BarrierCapacityBonus;

        public float BarrierRegenPerSecondBonus => UpgradeModifiers.BarrierRegenPerSecondBonus;

        public float BarrierOnDamageRatio => UpgradeModifiers.BarrierOnDamageRatio;

        public float PoisonDamageRatio => UpgradeModifiers.PoisonDamageRatio;

        public float BleedDamageRatio => UpgradeModifiers.BleedDamageRatio;

        public float ExecuteThresholdNormalized => UpgradeModifiers.ExecuteThresholdNormalized;

        public float CriticalChanceBonus => UpgradeModifiers.CriticalChanceBonus;

        public float CriticalDamageMultiplierBonus => UpgradeModifiers.CriticalDamageMultiplierBonus;

        public float DraftLuckBonus => UpgradeModifiers.DraftLuckBonus;

        public float DeathNovaDamageBonus => UpgradeModifiers.DeathNovaDamageBonus;

        public float DeathNovaRadiusBonus => UpgradeModifiers.DeathNovaRadiusBonus;

        public float LifestealRatio => UpgradeModifiers.LifestealRatio;

        public float ExperienceGainMultiplierBonus => UpgradeModifiers.ExperienceGainMultiplierBonus;

        public float AreaRadiusBonus => UpgradeModifiers.AreaRadiusBonus;

        public int ProjectileFanBonus => UpgradeModifiers.ProjectileFanBonus;

        public int OrbitBladeBonus => UpgradeModifiers.OrbitBladeBonus;

        public float OrbitRadiusBonus => UpgradeModifiers.OrbitRadiusBonus;

        public int MeleeTargetBonus => UpgradeModifiers.MeleeTargetBonus;

        public int BurstCountBonus => UpgradeModifiers.BurstCountBonus;

        public int BurstEchoBonus => UpgradeModifiers.BurstEchoBonus;

        public int TargetedBurstSigilBonus => UpgradeModifiers.TargetedBurstSigilBonus;

        public int ProjectilePierceBonus => UpgradeModifiers.ProjectilePierceBonus;

        public int ProjectileChainBonus => UpgradeModifiers.ProjectileChainBonus;

        public int ProjectileForkBonus => UpgradeModifiers.ProjectileForkBonus;

        public int ProjectileReturnBonus => UpgradeModifiers.ProjectileReturnBonus;

        public int HitscanPierceBonus => UpgradeModifiers.HitscanPierceBonus;

        public int PayloadCountBonus => UpgradeModifiers.PayloadCountBonus;

        public float PayloadExplosionRadiusBonus => UpgradeModifiers.PayloadExplosionRadiusBonus;

        public float PayloadTriggerRadiusBonus => UpgradeModifiers.PayloadTriggerRadiusBonus;

        public float PickupAttractionSpeedBonus => UpgradeModifiers.PickupAttractionSpeedBonus;

        public float PickupMagnetPulseIntervalReductionBonus => UpgradeModifiers.PickupMagnetPulseIntervalReductionBonus;

        public int ActivePassiveCount => RunBuild.PassiveIds.Count;

        public int EvolvedWeaponCount => RunBuild.EvolutionIds.Count;

        public int MaxWeaponSlots => RunBuild.MaxWeaponSlots;

        public int MaxPassiveSlots => RunBuild.MaxPassiveSlots;

        public float FirstEvolutionEligibilityTimeSeconds => Telemetry.FirstEvolutionEligibilityTimeSeconds;

        public float FirstEvolutionAcquiredTimeSeconds => Telemetry.FirstEvolutionAcquiredTimeSeconds;

        void ISurvivorsUpgradeEffectSink.IncreaseMaximumHealth(double amount)
        {
            if (PlayerVitals.IsBound)
            {
                PlayerVitals.IncreaseMaximumHealth(amount);
            }
        }

        void ISurvivorsUpgradeEffectSink.RestoreBarrier(float amount) => PlayerVitals.RestoreBarrier(amount);

        void ISurvivorsUpgradeEffectSink.ScheduleMagnetPulse() => PickupCollection.ScheduleMagnetPulse();

        public bool IsUpgradeAvailableInRunForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return RunBuild.Catalog != null && !string.IsNullOrWhiteSpace(upgradeId) && RunBuild.Catalog.TryGet(new RunUpgradeId(upgradeId), out _);
        }

        public bool IsUpgradeEligibleInCurrentBuildForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return RunBuild.TryGetRunUpgrade(upgradeId, out RunUpgradeDefinition upgrade) && RunBuild.IsUpgradeEligibleForCurrentBuild(upgrade);
        }

        public SurvivorsRunUpgradeCategory GetUpgradeCategoryForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return RunBuild.TryGetRunUpgrade(upgradeId, out RunUpgradeDefinition upgrade)
                ? RunBuild.ResolveCurrentUpgradeCategory(upgrade)
                : SurvivorsRunUpgradeCategory.WeaponUpgrade;
        }

        public string GetUpgradeDescriptionForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return RunBuild.TryGetUpgradeMetadata(upgradeId, out SurvivorsRunUpgradeMetadata metadata)
                ? metadata.Description
                : ResolveUpgradeDisplayName(new RunUpgradeId(upgradeId));
        }

        public int GetRunUpgradeRankForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return string.IsNullOrWhiteSpace(upgradeId) ? 0 : RunBuild.State.GetRank(new RunUpgradeId(upgradeId));
        }

        public bool HasEvolvedUpgradeForTest(string upgradeId)
        {
            EnsureRunStartedForTest();
            return RunBuild.HasEvolution(upgradeId);
        }

        internal bool IsEvolutionActive(string upgradeId)
        {
            return RunBuild.HasEvolution(upgradeId);
        }

        private void ApplySelectedClassBonuses()
        {
            UpgradeModifiers.ApplyClass(_selectedClass);
        }

        private void ApplyRelic(SurvivorsRelicDefinition relic)
        {
            UpgradeModifiers.ApplyRelic(relic);
        }

        private void ApplyUpgrade(RunUpgradeDefinition upgrade)
        {
            UpgradeModifiers.Apply(upgrade);
            RunBuild.RecordRunBuildSelection(upgrade);
            RecordNewlyEligibleEvolutionFeedback();
        }
    }
}
