using System;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Distributes a major threat's deterministic cache and owns its drop diagnostics.</summary>
    internal sealed class SurvivorsMajorRewardPickupCache
    {
        private const float MajorRewardPickupCacheRadiusPadding = 0.55f;
        private readonly ISurvivorsMajorRewardPickupCachePort _port;
        internal int MajorRewardCacheDropCount { get; private set; }
        internal int MajorRewardCacheExperienceGemDropCount { get; private set; }
        internal int MajorRewardCacheSpecialDropCount { get; private set; }
        internal int MajorRewardCacheAttractedPickupCount { get; private set; }
        internal string LastMajorRewardCacheFeedbackLabel { get; private set; } = string.Empty;

        internal SurvivorsMajorRewardPickupCache(ISurvivorsMajorRewardPickupCachePort port)
            => _port = port ?? throw new ArgumentNullException(nameof(port));

        internal void ResetDiagnostics()
        {
            MajorRewardCacheDropCount = 0;
            MajorRewardCacheExperienceGemDropCount = 0;
            MajorRewardCacheSpecialDropCount = 0;
            MajorRewardCacheAttractedPickupCount = 0;
            LastMajorRewardCacheFeedbackLabel = string.Empty;
        }

        internal void SpawnMajorRewardPickupCache(Vector3 position, SurvivorsEnemyRole role, float radius)
        {
            if (!_port.IsMajorRewardRole(role))
            {
                return;
            }

            int gemCount = ResolveMajorRewardCacheExperienceGemCount(role);
            int xpPerGem = ResolveMajorRewardCacheExperiencePerGem(role);
            float cacheRadius = Mathf.Max(0.9f, radius + MajorRewardPickupCacheRadiusPadding);
            int spawnedExperience = 0;
            int attractedPickupCount = 0;
            for (int i = 0; i < gemCount; i++)
            {
                float angle = ((i + 0.18f) / gemCount) * Mathf.PI * 2f;
                float ringOffset = 0.18f * (i % 2);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (cacheRadius + ringOffset);
                SurvivorsPickupActor pickup = _port.SpawnPickup(SurvivorsPickupKind.Experience, position + offset, xpPerGem);
                if (pickup != null)
                {
                    spawnedExperience += xpPerGem;
                    MajorRewardCacheExperienceGemDropCount++;
                    if (StartMajorRewardCacheAttraction(pickup))
                    {
                        attractedPickupCount++;
                    }
                }
            }

            int specialDropCount = SpawnMajorRewardSpecialPickups(position, role, cacheRadius, out int attractedSpecialDropCount);
            attractedPickupCount += attractedSpecialDropCount;
            if (spawnedExperience <= 0 && specialDropCount <= 0)
            {
                return;
            }

            MajorRewardCacheDropCount++;
            MajorRewardCacheSpecialDropCount += specialDropCount;
            MajorRewardCacheAttractedPickupCount += attractedPickupCount;
            string rewardLabel = _port.ResolveMajorRewardDropLabel(role);
            string specialLabel = specialDropCount > 0 ? $" + {specialDropCount} special" : string.Empty;
            string pullLabel = attractedPickupCount > 0 ? $" pull x{attractedPickupCount}" : string.Empty;
            string label = $"{rewardLabel}: Cache +{spawnedExperience} XP{specialLabel}{pullLabel}";
            LastMajorRewardCacheFeedbackLabel = label;
            _port.RecordStreakRewardFeedback(label, _port.ResolveMajorRewardDropColor(role));
        }

        internal int ResolveMajorRewardCacheExperienceGemCount(SurvivorsEnemyRole role)
        {
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return 12;
                case SurvivorsEnemyRole.Miniboss:
                    return 8;
                case SurvivorsEnemyRole.DreadElite:
                    return 7;
                default:
                    return 5;
            }
        }

        internal int ResolveMajorRewardCacheExperiencePerGem(SurvivorsEnemyRole role)
        {
            float multiplier;
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    multiplier = 5.5f;
                    break;
                case SurvivorsEnemyRole.Miniboss:
                    multiplier = 3.25f;
                    break;
                case SurvivorsEnemyRole.DreadElite:
                    multiplier = 2.35f;
                    break;
                default:
                    multiplier = 1.85f;
                    break;
            }

            float escalationMultiplier = 1f + _port.RunEscalationLevel * 0.1f;
            return Mathf.Max(1, Mathf.RoundToInt(_port.Tuning.EnemyExperienceReward * multiplier * escalationMultiplier));
        }

        internal int SpawnMajorRewardSpecialPickups(Vector3 position, SurvivorsEnemyRole role, float cacheRadius, out int attractedPickupCount)
        {
            attractedPickupCount = 0;
            int specialDropCount = 0;
            Vector3 magnetPosition = position + new Vector3(cacheRadius * 0.62f, 0f, -cacheRadius * 0.36f);
            SurvivorsPickupActor magnet = _port.SpawnPickup(SurvivorsPickupKind.Magnet, magnetPosition, 1);
            if (magnet != null)
            {
                specialDropCount++;
                if (StartMajorRewardCacheAttraction(magnet))
                {
                    attractedPickupCount++;
                }
            }

            int shardAmount = ResolveMajorRewardCacheBloodShardAmount(role);
            if (shardAmount > 0)
            {
                Vector3 shardPosition = position + new Vector3(-cacheRadius * 0.58f, 0f, cacheRadius * 0.42f);
                SurvivorsPickupActor shard = _port.SpawnPickup(SurvivorsPickupKind.BloodShard, shardPosition, shardAmount);
                if (shard != null)
                {
                    specialDropCount++;
                    if (StartMajorRewardCacheAttraction(shard))
                    {
                        attractedPickupCount++;
                    }
                }
            }

            return specialDropCount;
        }

        internal bool StartMajorRewardCacheAttraction(SurvivorsPickupActor pickup)
        {
            return pickup != null && pickup.StartRewardCacheAttraction(_port.Tuning.MajorRewardCacheAttractionSpeedMultiplier);
        }

        internal int ResolveMajorRewardCacheBloodShardAmount(SurvivorsEnemyRole role)
        {
            int baseAmount = Mathf.Max(1, _port.Tuning.BloodShardPickupAmount);
            switch (role)
            {
                case SurvivorsEnemyRole.Boss:
                    return baseAmount + 4;
                case SurvivorsEnemyRole.Miniboss:
                    return baseAmount + 2;
                case SurvivorsEnemyRole.DreadElite:
                    return baseAmount + 1;
                default:
                    return 0;
            }
        }
    }
}
