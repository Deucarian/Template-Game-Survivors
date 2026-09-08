using System;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns selected-card pulses, reward jackpot distribution and successful-drop accounting.</summary>
    internal sealed class SurvivorsDraftSelectionRewards
    {
        private readonly SurvivorsRunBuildState _build;
        private readonly ISurvivorsPickupRewardPort _port;
        public SurvivorsDraftSelectionRewards(SurvivorsRunBuildState build, ISurvivorsPickupRewardPort port)
        { _build = build ?? throw new ArgumentNullException(nameof(build)); _port = port ?? throw new ArgumentNullException(nameof(port)); }
        public int LevelUpPulseCount { get; private set; }
        public int LevelUpPulseHitCount { get; private set; }
        public string LastLevelUpPulseFeedbackLabel { get; private set; } = string.Empty;
        public int RewardUpgradeSurgeCount { get; private set; }
        public int RewardUpgradeSurgeHitCount { get; private set; }
        public string LastRewardUpgradeSurgeFeedbackLabel { get; private set; } = string.Empty;
        public int RewardJackpotCount { get; private set; }
        public int RewardJackpotExperienceGemDropCount { get; private set; }
        public int RewardJackpotBloodShardDropCount { get; private set; }
        public int RewardJackpotBloodShardsDropped { get; private set; }
        public string LastRewardJackpotFeedbackLabel { get; private set; } = string.Empty;
        public void Reset()
        {
            LevelUpPulseCount = 0;
            LevelUpPulseHitCount = 0;
            LastLevelUpPulseFeedbackLabel = string.Empty;
            RewardUpgradeSurgeCount = 0;
            RewardUpgradeSurgeHitCount = 0;
            LastRewardUpgradeSurgeFeedbackLabel = string.Empty;
            RewardJackpotCount = 0;
            RewardJackpotExperienceGemDropCount = 0;
            RewardJackpotBloodShardDropCount = 0;
            RewardJackpotBloodShardsDropped = 0;
            LastRewardJackpotFeedbackLabel = string.Empty;
        }
        public void TriggerRewardUpgradeSurge(RunUpgradeDefinition upgrade, SurvivorsRewardSelectionKind selectionKind)
        {
            float radius = Mathf.Max(0f, _port.Tuning.RewardUpgradeSurgeRadius);
            float damage = Mathf.Max(0f, _port.Tuning.RewardUpgradeSurgeDamage);
            string name = upgrade == null ? ResolveRewardKindLabel(selectionKind) : _build.ResolveUpgradeDisplayName(upgrade.Id);
            int hitCount = radius > 0f && damage > 0f ? _port.DamageNonMajor(_port.PlayerPosition, radius, damage, "survivors.reward.surge") : 0;

            RewardUpgradeSurgeCount++;
            RewardUpgradeSurgeHitCount += hitCount;
            LastRewardUpgradeSurgeFeedbackLabel = $"{name} Reward Surge: {hitCount} enemies hit";
            Color accent = selectionKind == SurvivorsRewardSelectionKind.BossUpgrade
                ? new Color(1f, 0.52f, 0.24f)
                : new Color(0.36f, 0.82f, 1f);
            _port.ShowFeedback(LastRewardUpgradeSurgeFeedbackLabel, accent);
            _port.PlayPulse(_port.PlayerPosition, Mathf.Clamp(34 + hitCount * 4, 40, 78), selectionKind == SurvivorsRewardSelectionKind.BossUpgrade, false);
        }

        public void TriggerLevelUpPulse(RunUpgradeDefinition upgrade)
        {
            float radius = Mathf.Max(0f, _port.Tuning.LevelUpPulseRadius);
            float damage = Mathf.Max(0f, _port.Tuning.LevelUpPulseDamage);
            string name = upgrade == null ? "Level Up" : _build.ResolveUpgradeDisplayName(upgrade.Id);
            int hitCount = radius > 0f && damage > 0f ? _port.DamageNonMajor(_port.PlayerPosition, radius, damage, "survivors.level-up.pulse") : 0;

            LevelUpPulseCount++;
            LevelUpPulseHitCount += hitCount;
            LastLevelUpPulseFeedbackLabel = $"{name} Level Pulse: {hitCount} enemies hit";
            _port.ShowFeedback(LastLevelUpPulseFeedbackLabel, new Color(1f, 0.84f, 0.32f));
            _port.PlayPulse(_port.PlayerPosition, Mathf.Clamp(20 + hitCount * 4, 26, 64), false, false);
        }

        public void TriggerRewardJackpot(RunUpgradeDefinition upgrade, SurvivorsRewardSelectionKind selectionKind)
        {
            if (upgrade == null || upgrade.Rarity < RunUpgradeRarity.Rare || !(selectionKind == SurvivorsRewardSelectionKind.EliteUpgrade || selectionKind == SurvivorsRewardSelectionKind.BossUpgrade))
            {
                return;
            }

            int rarityTier = Mathf.Max(0, (int)upgrade.Rarity - (int)RunUpgradeRarity.Rare);
            int rewardTierBonus = selectionKind == SurvivorsRewardSelectionKind.BossUpgrade ? 1 : 0;
            int gemCount = Mathf.Max(
                0,
                _port.Tuning.RewardJackpotExperienceGemBaseCount +
                rarityTier * Mathf.Max(0, _port.Tuning.RewardJackpotExperienceGemPerRarityTier) +
                rewardTierBonus);
            int xpPerGem = Mathf.Max(
                1,
                Mathf.RoundToInt(_port.Tuning.EnemyExperienceReward * (2.5f + rarityTier * 0.7f + rewardTierBonus * 0.5f)));
            int shardAmount = Mathf.Max(
                0,
                _port.Tuning.RewardJackpotBloodShardBaseAmount +
                rarityTier +
                rewardTierBonus +
                (upgrade.Rarity >= RunUpgradeRarity.Legendary ? Mathf.Max(0, _port.Tuning.RewardJackpotLegendaryExtraBloodShardAmount) : 0));

            Vector3 origin = _port.PlayerPosition;
            float cacheRadius = 0.82f + rarityTier * 0.12f;
            int spawnedGems = 0;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.35f) / Mathf.Max(1, gemCount)) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * cacheRadius;
                if (_port.SpawnPickup(SurvivorsPickupKind.Experience, origin + offset, xpPerGem, true))
                {
                    spawnedGems++;
                    RewardJackpotExperienceGemDropCount++;
                }
            }

            int spawnedShardAmount = 0;
            if (shardAmount > 0)
            {
                if (_port.SpawnPickup(SurvivorsPickupKind.BloodShard, origin + new Vector3(0f, 0f, cacheRadius * 0.45f), shardAmount, true))
                {
                    spawnedShardAmount = shardAmount;
                    RewardJackpotBloodShardDropCount++;
                    RewardJackpotBloodShardsDropped += shardAmount;
                }
            }

            if (spawnedGems <= 0 && spawnedShardAmount <= 0)
            {
                return;
            }

            RewardJackpotCount++;
            string rewardLabel = ResolveRewardKindLabel(selectionKind);
            int totalExperience = spawnedGems * xpPerGem;
            LastRewardJackpotFeedbackLabel = $"{rewardLabel} Jackpot: {upgrade.Rarity} +{totalExperience} XP";
            if (spawnedShardAmount > 0)
            {
                LastRewardJackpotFeedbackLabel += $" +{spawnedShardAmount} {_port.CurrencyLabel}";
            }

            _port.ShowFeedback(LastRewardJackpotFeedbackLabel, SurvivorsDraftCardFactory.ResolveRarityAccentColor(upgrade.Rarity));
            _port.PlayPulse(origin, Mathf.Clamp(24 + spawnedGems * 4 + spawnedShardAmount * 3, 32, 86), selectionKind == SurvivorsRewardSelectionKind.BossUpgrade, true);
        }
        public static string ResolveRewardKindLabel(SurvivorsRewardSelectionKind selectionKind)
        {
            switch (selectionKind)
            {
                case SurvivorsRewardSelectionKind.LevelUp:
                    return "Level Up";
                case SurvivorsRewardSelectionKind.BossRelic:
                    return "Boss Relic";
                case SurvivorsRewardSelectionKind.EliteUpgrade:
                    return "Elite Reward";
                case SurvivorsRewardSelectionKind.BossUpgrade:
                    return "Boss Reward";
                default:
                    return "Reward";
            }
        }
    }
}
