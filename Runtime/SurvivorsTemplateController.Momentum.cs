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
    // Ordered run momentum and surge reward bindings.
    public sealed partial class SurvivorsTemplateController
    {
        private SurvivorsEndlessSurgeRewards EndlessSurges => _endlessSurges ?? (_endlessSurges = new SurvivorsEndlessSurgeRewards(_runSession, this));

        private SurvivorsDraftSelectionRewards SelectionRewards => _selectionRewards ?? (_selectionRewards = new SurvivorsDraftSelectionRewards(RunBuild, this));

        private void TryActivateEndlessSurge(SurvivorsEnemyRole role, Vector3 position, int baseExperienceReward) => EndlessSurges.TryActivateEndlessSurge(role, position, baseExperienceReward);

        private void TickEndlessSurge(float deltaTime) => EndlessSurges.TickEndlessSurge(deltaTime);

        private void TriggerRewardUpgradeSurge(RunUpgradeDefinition upgrade, SurvivorsRewardSelectionKind kind) => SelectionRewards.TriggerRewardUpgradeSurge(upgrade, kind);

        private void TriggerLevelUpPulse(RunUpgradeDefinition upgrade) => SelectionRewards.TriggerLevelUpPulse(upgrade);

        private void TriggerRewardJackpot(RunUpgradeDefinition upgrade, SurvivorsRewardSelectionKind kind) => SelectionRewards.TriggerRewardJackpot(upgrade, kind);

        SurvivorsTemplateTuning ISurvivorsPickupRewardPort.Tuning => CurrentTuning;

        Vector3 ISurvivorsPickupRewardPort.PlayerPosition => PlayerPosition;

        string ISurvivorsPickupRewardPort.CurrencyLabel => CurrencyRewardLabel;

        int ISurvivorsPickupRewardPort.DamageNonMajor(Vector3 position, float radius, float damage, string source) => DamageNonMajorEnemies(position, radius, damage, source);

        bool ISurvivorsPickupRewardPort.SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount, bool attract)
        {
            SurvivorsPickupActor pickup = SpawnPickup(kind, position, amount);
            if (pickup == null) return false;
            if (attract) StartMajorRewardCacheAttraction(pickup);
            return true;
        }

        void ISurvivorsPickupRewardPort.ShowFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);

        void ISurvivorsPickupRewardPort.PlayPulse(Vector3 position, int count, bool boss, bool pickupAudio) => PlayFeedback(boss ? _bossPulse : _levelUpPulse, position, count, pickupAudio ? _pickupClip : boss ? _bossClip : _levelUpClip);

        private void RecordStreakRewardFeedback(string label, Color color) => StreakFeedback.RecordStreakRewardFeedback(label, color);

        private SurvivorsStreakFeedbackHistory StreakFeedback => _streakFeedback ?? (_streakFeedback = new SurvivorsStreakFeedbackHistory(_streakRewardBanner.Show));

        private SurvivorsKillStreakRewards KillStreakRewards => _killStreakRewards ?? (_killStreakRewards = new SurvivorsKillStreakRewards(this));

        private void RegisterKillStreak(Vector3 position) => KillStreakRewards.RegisterKillStreak(position);

        private void TickKillStreak(float deltaTime) => KillStreakRewards.TickKillStreak(deltaTime);

        private void TickStreakSurge(float deltaTime) => KillStreakRewards.TickStreakSurge(deltaTime);

        private void TickGemRush(float deltaTime) => ExperienceRhythm.TickGemRush(deltaTime);

        SurvivorsTemplateTuning ISurvivorsStreakRewardPort.Tuning => CurrentTuning;

        string ISurvivorsStreakRewardPort.CurrencyLabel => CurrencyDisplayName;

        bool ISurvivorsStreakRewardPort.SpawnPickup(SurvivorsPickupKind kind, Vector3 position, int amount) => SpawnPickup(kind, position, amount) != null;

        bool ISurvivorsStreakRewardPort.TryDropHealth(Vector3 position) => TryDropHealthPickup(position);

        void ISurvivorsStreakRewardPort.ShowFeedback(string label, Color color) => RecordStreakRewardFeedback(label, color);

        public int LevelUpPulseCount => SelectionRewards.LevelUpPulseCount;

        public int LevelUpPulseHitCount => SelectionRewards.LevelUpPulseHitCount;

        public string LastLevelUpPulseFeedbackLabel => SelectionRewards.LastLevelUpPulseFeedbackLabel;

        public int RewardUpgradeSurgeCount => SelectionRewards.RewardUpgradeSurgeCount;

        public int RewardUpgradeSurgeHitCount => SelectionRewards.RewardUpgradeSurgeHitCount;

        public string LastRewardUpgradeSurgeFeedbackLabel => SelectionRewards.LastRewardUpgradeSurgeFeedbackLabel;

        public int RewardJackpotCount => SelectionRewards.RewardJackpotCount;

        public int RewardJackpotExperienceGemDropCount => SelectionRewards.RewardJackpotExperienceGemDropCount;

        public int RewardJackpotBloodShardDropCount => SelectionRewards.RewardJackpotBloodShardDropCount;

        public int RewardJackpotBloodShardsDropped => SelectionRewards.RewardJackpotBloodShardsDropped;

        public string LastRewardJackpotFeedbackLabel => SelectionRewards.LastRewardJackpotFeedbackLabel;

        public int EndlessSurgeActivationCount => EndlessSurges.EndlessSurgeActivationCount;

        public int EndlessSurgeTier => EndlessSurges.EndlessSurgeTier;

        public int EndlessSurgeExperienceGemDropCount => EndlessSurges.EndlessSurgeExperienceGemDropCount;

        public int EndlessSurgeBloodShardDropCount => EndlessSurges.EndlessSurgeBloodShardDropCount;

        public int EndlessSurgePulseHitCount => EndlessSurges.EndlessSurgePulseHitCount;

        public string LastEndlessSurgeFeedbackLabel => EndlessSurges.LastEndlessSurgeFeedbackLabel;

        public int GemRushActivationCount => ExperienceRhythm.GemRushActivationCount;

        public string LastGemRushFeedbackLabel => ExperienceRhythm.LastGemRushFeedbackLabel;

        public int BestKillStreak => KillStreakRewards.BestKillStreak;

        public int StreakBonusDropCount => KillStreakRewards.StreakBonusDropCount;

        public int StreakHealthDropCount => KillStreakRewards.StreakHealthDropCount;

        public int StreakMagnetDropCount => KillStreakRewards.StreakMagnetDropCount;

        public int StreakBloodShardDropCount => KillStreakRewards.StreakBloodShardDropCount;

        public int StreakRewardFeedbackCount => StreakFeedback.StreakRewardFeedbackCount;

        public string LastStreakRewardFeedbackLabel => StreakFeedback.LastStreakRewardFeedbackLabel;

        public int StreakSurgeTier => KillStreakRewards.StreakSurgeTier;

        public int StreakSurgeActivationCount => KillStreakRewards.StreakSurgeActivationCount;

        public int CurrentKillStreak => KillStreakRewards.CurrentKillStreak;

        public bool IsStreakSurgeActive => KillStreakRewards.IsStreakSurgeActive;

        public float StreakSurgeRemainingSeconds => KillStreakRewards.StreakSurgeRemainingSeconds;

        public float StreakSurgeDamageBonus => KillStreakRewards.StreakSurgeDamageBonus;

        public float StreakSurgeMoveSpeedBonus => KillStreakRewards.StreakSurgeMoveSpeedBonus;

        public float StreakSurgeCooldownMultiplierBonus => KillStreakRewards.StreakSurgeCooldownMultiplierBonus;

        public float StreakSurgePickupRangeBonus => KillStreakRewards.StreakSurgePickupRangeBonus;

        public bool IsGemRushActive => ExperienceRhythm.IsGemRushActive;

        public float GemRushRemainingSeconds => ExperienceRhythm.GemRushRemainingSeconds;

        public float GemRushDamageBonus => ExperienceRhythm.GemRushDamageBonus;

        public float GemRushMoveSpeedBonus => ExperienceRhythm.GemRushMoveSpeedBonus;

        public float GemRushCooldownMultiplierBonus => ExperienceRhythm.GemRushCooldownMultiplierBonus;

        public float GemRushPickupRangeBonus => ExperienceRhythm.GemRushPickupRangeBonus;

        public bool IsEndlessSurgeActive => EndlessSurges.IsEndlessSurgeActive;

        public float EndlessSurgeRemainingSeconds => EndlessSurges.EndlessSurgeRemainingSeconds;

        public float EndlessSurgeDamageBonus => EndlessSurges.EndlessSurgeDamageBonus;

        public float EndlessSurgeMoveSpeedBonus => EndlessSurges.EndlessSurgeMoveSpeedBonus;

        public float EndlessSurgeCooldownMultiplierBonus => EndlessSurges.EndlessSurgeCooldownMultiplierBonus;

        public float EndlessSurgePickupRangeBonus => EndlessSurges.EndlessSurgePickupRangeBonus;

        public string ActiveStreakRewardFeedbackLabel => _streakRewardBanner.RemainingSeconds > 0f ? _streakRewardBanner.Label : string.Empty;

        public float StreakRewardFeedbackRemainingSeconds => Mathf.Max(0f, _streakRewardBanner.RemainingSeconds);

        private void DrawStreakRewardFeedback() => _streakRewardBanner.Draw(_rewardFeedbackStyle);

        private void TickStreakRewardFeedback(float deltaTime) => _streakRewardBanner.Tick(deltaTime);
    }
}
