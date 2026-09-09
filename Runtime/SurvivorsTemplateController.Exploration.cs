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
    // Roaming cache, shrine and waystone encounter bindings.
    public sealed partial class SurvivorsTemplateController
    {
        private string ResolveWaystoneCompassHudLabel()
        {
            if (CurrentTuning.WaystoneDiscoveryRadius <= 0f)
                return SurvivorsCompactHudLabels.Waystone(false, WaystoneDiscoveryCount, false, 0f, Vector3.zero, false);
            if (TryResolveClosestArenaLandmark(ignoreDiscovered: true, out _, out float distance, out Vector3 delta))
                return SurvivorsCompactHudLabels.Waystone(true, WaystoneDiscoveryCount, true, distance, delta, false);
            bool any = TryResolveClosestArenaLandmark(ignoreDiscovered: false, out _, out _, out _);
            return SurvivorsCompactHudLabels.Waystone(true, WaystoneDiscoveryCount, false, 0f, Vector3.zero, any);
        }

        private SurvivorsRoamingCacheEncounter RoamingCaches => _roamingCaches ?? (_roamingCaches = new SurvivorsRoamingCacheEncounter(this));

        private SurvivorsShrineEncounter ShrineTrials => _shrineTrials ?? (_shrineTrials = new SurvivorsShrineEncounter(this));

        private SurvivorsWaystoneExploration Waystones => _waystones ?? (_waystones = new SurvivorsWaystoneExploration(this));

        private SurvivorsExplorationBonuses ExplorationBonuses => new SurvivorsExplorationBonuses(_runSession.HasClearedVictory, EndlessSurgeTier);

        private SurvivorsExplorationFeedback ExplorationFeedback => _explorationFeedback ?? (_explorationFeedback = new SurvivorsExplorationFeedback(
            RecordStreakRewardFeedback,
            (position, particles, danger) => PlayFeedback(danger ? _bossPulse : _levelUpPulse, position, particles, danger ? _dangerClip : _pickupClip)));

        SurvivorsTemplateTuning ISurvivorsExplorationPort.Tuning => CurrentTuning;

        int ISurvivorsExplorationPort.Escalation => RunEscalationLevel;

        int ISurvivorsExplorationPort.MaximumAlive => ResolveEnemyMaximumAlive();

        long ISurvivorsExplorationPort.SpawnSequence => _spawnSequence;

        SurvivorsExplorationBonuses ISurvivorsExplorationPort.Endless => ExplorationBonuses;

        string ISurvivorsExplorationPort.CurrencyRewardLabel => CurrencyRewardLabel;

        SurvivorsExplorationFeedback ISurvivorsExplorationPort.Feedback => ExplorationFeedback;

        long ISurvivorsExplorationPort.SpawnEnemy(SurvivorsEnemyRole role, long seed, float minimum, float maximum, string source) =>
            SpawnGameplayEnemyOffscreen(role, seed, minimum, maximum, source)?.InstanceId.Value ?? 0;

        bool ISurvivorsExplorationPort.SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount) => SpawnPickup(kind, position, amount) != null;

        int ISurvivorsExplorationPort.DamageNonMajorEnemies(Vector3 position, float radius, float damage, string source) =>
            DamageNonMajorEnemies(position, radius, damage, source);

        public int RoamingCacheDropCount => RoamingCaches.RoamingCacheDropCount;

        public int RoamingCacheExperienceGemDropCount => RoamingCaches.RoamingCacheExperienceGemDropCount;

        public int RoamingCacheMagnetDropCount => RoamingCaches.RoamingCacheMagnetDropCount;

        public int RoamingCacheBloodShardDropCount => RoamingCaches.RoamingCacheBloodShardDropCount;

        public int RoamingCacheAmbushCount => RoamingCaches.RoamingCacheAmbushCount;

        public int RoamingCacheAmbushEnemySpawnCount => RoamingCaches.RoamingCacheAmbushEnemySpawnCount;

        public int RoamingCacheAmbushClearRewardCount => RoamingCaches.RoamingCacheAmbushClearRewardCount;

        public int RoamingCacheAmbushClearExperienceGemDropCount => RoamingCaches.RoamingCacheAmbushClearExperienceGemDropCount;

        public int RoamingCacheAmbushClearMagnetDropCount => RoamingCaches.RoamingCacheAmbushClearMagnetDropCount;

        public int RoamingCacheAmbushClearBloodShardDropCount => RoamingCaches.RoamingCacheAmbushClearBloodShardDropCount;

        public int RoamingCacheSurgeActivationCount => RoamingCaches.RoamingCacheSurgeActivationCount;

        public int RoamingCacheSurgeBonusExperienceGemDropCount => RoamingCaches.RoamingCacheSurgeBonusExperienceGemDropCount;

        public int RoamingCacheSurgePulseHitCount => RoamingCaches.RoamingCacheSurgePulseHitCount;

        public string LastRoamingCacheFeedbackLabel => ExplorationFeedback.LastLabel;

        public string LastRoamingCacheAmbushClearFeedbackLabel => RoamingCaches.LastRoamingCacheAmbushClearFeedbackLabel;

        public string LastRoamingCacheSurgeFeedbackLabel => RoamingCaches.LastRoamingCacheSurgeFeedbackLabel;

        public int ArenaShrineTrialCount => ShrineTrials.ArenaShrineTrialCount;

        public int ArenaShrineEnemySpawnCount => ShrineTrials.ArenaShrineEnemySpawnCount;

        public int ArenaShrineClearRewardCount => ShrineTrials.ArenaShrineClearRewardCount;

        public int ArenaShrineClearExperienceGemDropCount => ShrineTrials.ArenaShrineClearExperienceGemDropCount;

        public int ArenaShrineClearBloodShardDropCount => ShrineTrials.ArenaShrineClearBloodShardDropCount;

        public int ArenaShrineSurgeActivationCount => ShrineTrials.ArenaShrineSurgeActivationCount;

        public int ArenaShrineSurgePulseHitCount => ShrineTrials.ArenaShrineSurgePulseHitCount;

        public string LastArenaShrineFeedbackLabel => ShrineTrials.LastArenaShrineFeedbackLabel;

        public string LastArenaShrineClearFeedbackLabel => ShrineTrials.LastArenaShrineClearFeedbackLabel;

        public int WaystoneDiscoveryCount => Waystones.WaystoneDiscoveryCount;

        public int WaystoneExperienceGemDropCount => Waystones.WaystoneExperienceGemDropCount;

        public int WaystoneBloodShardDropCount => Waystones.WaystoneBloodShardDropCount;

        public int WaystoneAmbushCount => Waystones.WaystoneAmbushCount;

        public int WaystoneAmbushEnemySpawnCount => Waystones.WaystoneAmbushEnemySpawnCount;

        public int WaystoneFocusActivationCount => Waystones.WaystoneFocusActivationCount;

        public int WaystoneChainSurgeActivationCount => Waystones.WaystoneChainSurgeActivationCount;

        public int WaystoneChainSurgeBonusExperienceGemDropCount => Waystones.WaystoneChainSurgeBonusExperienceGemDropCount;

        public int WaystoneChainSurgePulseHitCount => Waystones.WaystoneChainSurgePulseHitCount;

        public string LastWaystoneDiscoveryFeedbackLabel => Waystones.LastWaystoneDiscoveryFeedbackLabel;

        public string LastWaystoneChainSurgeFeedbackLabel => Waystones.LastWaystoneChainSurgeFeedbackLabel;

        public bool IsRoamingCacheSurgeActive => RoamingCaches.IsRoamingCacheSurgeActive;

        public float RoamingCacheSurgeRemainingSeconds => RoamingCaches.RoamingCacheSurgeRemainingSeconds;

        public float RoamingCacheSurgeDamageBonus => RoamingCaches.RoamingCacheSurgeDamageBonus;

        public float RoamingCacheSurgeMoveSpeedBonus => RoamingCaches.RoamingCacheSurgeMoveSpeedBonus;

        public float RoamingCacheSurgeCooldownMultiplierBonus => RoamingCaches.RoamingCacheSurgeCooldownMultiplierBonus;

        public float RoamingCacheSurgePickupRangeBonus => RoamingCaches.RoamingCacheSurgePickupRangeBonus;

        public bool IsArenaShrineSurgeActive => ShrineTrials.IsArenaShrineSurgeActive;

        public float ArenaShrineSurgeRemainingSeconds => ShrineTrials.ArenaShrineSurgeRemainingSeconds;

        public float ArenaShrineSurgeDamageBonus => ShrineTrials.ArenaShrineSurgeDamageBonus;

        public float ArenaShrineSurgeMoveSpeedBonus => ShrineTrials.ArenaShrineSurgeMoveSpeedBonus;

        public float ArenaShrineSurgeCooldownMultiplierBonus => ShrineTrials.ArenaShrineSurgeCooldownMultiplierBonus;

        public float ArenaShrineSurgePickupRangeBonus => ShrineTrials.ArenaShrineSurgePickupRangeBonus;

        public bool IsWaystoneFocusActive => Waystones.IsWaystoneFocusActive;

        public float WaystoneFocusRemainingSeconds => Waystones.WaystoneFocusRemainingSeconds;

        public float WaystoneFocusDamageBonus => Waystones.WaystoneFocusDamageBonus;

        public float WaystoneFocusMoveSpeedBonus => Waystones.WaystoneFocusMoveSpeedBonus;

        public float WaystoneFocusCooldownMultiplierBonus => Waystones.WaystoneFocusCooldownMultiplierBonus;

        public float WaystoneFocusPickupRangeBonus => Waystones.WaystoneFocusPickupRangeBonus;

        public bool IsWaystoneChainSurgeActive => Waystones.IsWaystoneChainSurgeActive;

        public float WaystoneChainSurgeRemainingSeconds => Waystones.WaystoneChainSurgeRemainingSeconds;

        public float WaystoneChainSurgeDamageBonus => Waystones.WaystoneChainSurgeDamageBonus;

        public float WaystoneChainSurgeMoveSpeedBonus => Waystones.WaystoneChainSurgeMoveSpeedBonus;

        public float WaystoneChainSurgeCooldownMultiplierBonus => Waystones.WaystoneChainSurgeCooldownMultiplierBonus;

        public float WaystoneChainSurgePickupRangeBonus => Waystones.WaystoneChainSurgePickupRangeBonus;

        public int EndlessExplorationBonusTier => ExplorationBonuses.Tier;

        public int ActiveRoamingCacheAmbushEnemyCount => RoamingCaches.ActiveCount;

        public int ActiveArenaShrineEnemyCount => ShrineTrials.ActiveCount;

        public float CurrentWaystoneCompassDistanceForTest => TryResolveClosestArenaLandmark(ignoreDiscovered: true, out _, out float distance, out _) ? distance : 0f;

        public string CurrentWaystoneCompassHudLabel => ResolveWaystoneCompassHudLabel();
    }
}
