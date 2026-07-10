using System;
using System.Collections.Generic;
using Deucarian.Progression;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    public sealed class SurvivorsAuthoredContentDefinition
    {
        private readonly SurvivorsEnemyProfile[] _enemyProfiles;
        private readonly Dictionary<SurvivorsEnemyRole, SurvivorsEnemyProfile> _enemyProfilesByRole;
        private readonly Dictionary<string, RunFlowProfileRecordJson> _runFlowProfiles;
        private readonly SurvivorsMetaProgressionDefinition _metaProgressionDefinition;

        private SurvivorsAuthoredContentDefinition(
            SurvivorsEnemyProfile[] enemyProfiles,
            Dictionary<SurvivorsEnemyRole, SurvivorsEnemyProfile> enemyProfilesByRole,
            Dictionary<string, RunFlowProfileRecordJson> runFlowProfiles,
            SurvivorsMetaProgressionDefinition metaProgressionDefinition,
            string sourceSummary)
        {
            _enemyProfiles = enemyProfiles ?? Array.Empty<SurvivorsEnemyProfile>();
            _enemyProfilesByRole = enemyProfilesByRole ?? new Dictionary<SurvivorsEnemyRole, SurvivorsEnemyProfile>();
            _runFlowProfiles = runFlowProfiles ?? new Dictionary<string, RunFlowProfileRecordJson>(StringComparer.Ordinal);
            _metaProgressionDefinition = metaProgressionDefinition;
            SourceSummary = sourceSummary ?? string.Empty;
        }

        public string SourceSummary { get; }
        public bool HasEnemyProfiles => _enemyProfiles.Length > 0;
        public bool HasRunFlowProfiles => _runFlowProfiles.Count > 0;
        public bool HasMetaProgression => _metaProgressionDefinition != null;
        public IReadOnlyList<SurvivorsEnemyProfile> EnemyProfiles => _enemyProfiles;
        public SurvivorsMetaProgressionDefinition MetaProgressionDefinition => _metaProgressionDefinition;

        public static bool TryCreate(
            string enemyJson,
            string runFlowJson,
            string rewardJson,
            out SurvivorsAuthoredContentDefinition definition,
            out string error)
        {
            definition = null;
            var errors = new List<string>();
            EnemyLibraryJson enemies = ParseJson<EnemyLibraryJson>(enemyJson, "enemy library", errors);
            RunFlowLibraryJson runFlow = ParseJson<RunFlowLibraryJson>(runFlowJson, "run flow library", errors);
            RewardLibraryJson rewards = ParseJson<RewardLibraryJson>(rewardJson, "reward library", errors);

            SurvivorsEnemyProfile[] enemyProfiles = CreateEnemyProfiles(enemies, errors);
            Dictionary<SurvivorsEnemyRole, SurvivorsEnemyProfile> enemyProfilesByRole = IndexEnemyProfiles(enemyProfiles, errors);
            Dictionary<string, RunFlowProfileRecordJson> runFlowProfiles = IndexRunFlowProfiles(runFlow, errors);
            SurvivorsMetaProgressionDefinition metaProgression = CreateMetaProgressionDefinition(rewards, errors);

            RequireEnemyRole(enemyProfilesByRole, SurvivorsEnemyRole.Swarm, errors);
            RequireEnemyRole(enemyProfilesByRole, SurvivorsEnemyRole.Runner, errors);
            RequireEnemyRole(enemyProfilesByRole, SurvivorsEnemyRole.Bruiser, errors);
            RequireEnemyRole(enemyProfilesByRole, SurvivorsEnemyRole.Spitter, errors);
            RequireEnemyRole(enemyProfilesByRole, SurvivorsEnemyRole.Splitter, errors);
            RequireEnemyRole(enemyProfilesByRole, SurvivorsEnemyRole.Summoner, errors);
            RequireEnemyRole(enemyProfilesByRole, SurvivorsEnemyRole.Elite, errors);
            RequireEnemyRole(enemyProfilesByRole, SurvivorsEnemyRole.DreadElite, errors);
            RequireEnemyRole(enemyProfilesByRole, SurvivorsEnemyRole.Miniboss, errors);
            RequireEnemyRole(enemyProfilesByRole, SurvivorsEnemyRole.Boss, errors);
            RequireRunFlowProfile(runFlowProfiles, SurvivorsPacingProfile.HumanPlaytest, errors);
            RequireRunFlowProfile(runFlowProfiles, SurvivorsPacingProfile.SprintRun, errors);
            RequireReward(metaProgression, BasicSurvivorsGame.EliteRewardId, errors);
            RequireReward(metaProgression, BasicSurvivorsGame.MinibossRewardId, errors);
            RequireReward(metaProgression, BasicSurvivorsGame.BossRewardId, errors);
            RequireReward(metaProgression, BasicSurvivorsGame.EmberVanguardUnlockRewardId, errors);

            if (errors.Count > 0)
            {
                error = string.Join("; ", errors);
                return false;
            }

            string summary = $"{enemyProfiles.Length} enemies, {runFlowProfiles.Count} run-flow profiles, {metaProgression.Rewards.Count} rewards";
            definition = new SurvivorsAuthoredContentDefinition(enemyProfiles, enemyProfilesByRole, runFlowProfiles, metaProgression, summary);
            error = string.Empty;
            return true;
        }

        public SurvivorsTemplateTuning CreateTuning(SurvivorsPacingProfile profile)
        {
            SurvivorsTemplateTuning tuning = BasicSurvivorsGame.CreateTuning(profile);
            if (!TryGetRunFlowProfile(profile, out RunFlowProfileRecordJson record))
            {
                return tuning;
            }

            ApplyRunFlowProfile(tuning, record);
            return tuning;
        }

        public SurvivorsRunFlowDefinition CreateRunFlowDefinition(SurvivorsTemplateTuning tuning)
        {
            SurvivorsTemplateTuning resolved = tuning ?? CreateTuning(SurvivorsPacingProfile.HumanPlaytest);
            return new SurvivorsRunFlowDefinition(
                resolved.RunEscalationIntervalSeconds,
                resolved.MinimumEnemySpawnIntervalSeconds,
                resolved.EnemySpawnIntervalReductionPerEscalation,
                resolved.EnemyMaximumAliveIncreasePerEscalation,
                resolved.EnemyHealthMultiplierPerEscalation,
                resolved.EnemyMoveSpeedMultiplierPerEscalation,
                resolved.EnemyExperienceMultiplierPerEscalation,
                resolved.MinibossSpawnTimeSeconds,
                ResolveEnemyProfile(SurvivorsEnemyRole.Miniboss, resolved),
                resolved.BossSpawnTimeSeconds,
                ResolveEnemyProfile(SurvivorsEnemyRole.Boss, resolved),
                resolved.SurvivalVictoryTimeSeconds,
                CreateEnemyProfileDefinitions(resolved),
                firstEliteSpawnTimeSeconds: resolved.FirstEliteSpawnTimeSeconds,
                eliteSpawnIntervalSeconds: resolved.EliteSpawnIntervalSeconds,
                firstDreadEliteSpawnTimeSeconds: resolved.FirstDreadEliteSpawnTimeSeconds,
                dreadEliteSpawnIntervalSeconds: resolved.DreadEliteSpawnIntervalSeconds);
        }

        public bool TryGetEnemyProfile(SurvivorsEnemyRole role, out SurvivorsEnemyProfile profile)
        {
            return _enemyProfilesByRole.TryGetValue(role, out profile);
        }

        public bool TryGetRunFlowProfile(SurvivorsPacingProfile profile, out string profileId)
        {
            profileId = profile.ToString();
            return _runFlowProfiles.ContainsKey(profileId);
        }

        private bool TryGetRunFlowProfile(SurvivorsPacingProfile profile, out RunFlowProfileRecordJson record)
        {
            return _runFlowProfiles.TryGetValue(profile.ToString(), out record);
        }

        private SurvivorsEnemyProfile ResolveEnemyProfile(SurvivorsEnemyRole role, SurvivorsTemplateTuning tuning)
        {
            if (!_enemyProfilesByRole.TryGetValue(role, out SurvivorsEnemyProfile authored))
            {
                return BasicSurvivorsGame.CreateEnemyProfile(role, tuning);
            }

            SurvivorsEnemyProfile tuned = BasicSurvivorsGame.CreateEnemyProfile(role, tuning);
            return new SurvivorsEnemyProfile(
                role,
                authored.Id,
                authored.DisplayName,
                tuned.MaxHealth,
                tuned.MoveSpeed,
                tuned.Radius,
                tuned.ContactDamage,
                tuned.ContactIntervalSeconds,
                tuned.ExperienceReward,
                tuned.Tint,
                tuned.RangedAttackRange,
                tuned.RangedAttackDamage,
                tuned.RangedAttackIntervalSeconds,
                tuned.PreferredRange,
                authored.CanRecycle,
                authored.CanLeash,
                authored.CanReposition,
                authored.ShowOffscreenMarker,
                authored.ShowOverheadLifeBar,
                authored.ShowBossLifeBar,
                authored.MarkerStyle,
                authored.SoftLeashRadius,
                authored.HardRecycleRadius,
                authored.CatchUpSpeedMultiplier,
                authored.RepositionTimeoutSeconds,
                authored.SpawnTimeSeconds);
        }

        private IReadOnlyList<SurvivorsEnemyProfile> CreateEnemyProfileDefinitions(SurvivorsTemplateTuning tuning)
        {
            if (_enemyProfiles.Length == 0)
            {
                return BasicSurvivorsGame.CreateEnemyProfileDefinitions(tuning);
            }

            SurvivorsEnemyProfile[] profiles = new SurvivorsEnemyProfile[_enemyProfiles.Length];
            for (int i = 0; i < _enemyProfiles.Length; i++)
            {
                profiles[i] = ResolveEnemyProfile(_enemyProfiles[i].Role, tuning);
            }

            return profiles;
        }

        private static T ParseJson<T>(string json, string label, List<string> errors) where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                errors.Add($"Authored {label} JSON is required.");
                return null;
            }

            try
            {
                T parsed = JsonUtility.FromJson<T>(json);
                if (parsed == null)
                {
                    errors.Add($"Authored {label} JSON could not be parsed.");
                }

                return parsed;
            }
            catch (Exception ex)
            {
                errors.Add($"Authored {label} JSON could not be parsed: {ex.Message}");
                return null;
            }
        }

        private static SurvivorsEnemyProfile[] CreateEnemyProfiles(EnemyLibraryJson library, List<string> errors)
        {
            if (library == null || library.enemies == null)
            {
                return Array.Empty<SurvivorsEnemyProfile>();
            }

            var profiles = new List<SurvivorsEnemyProfile>(library.enemies.Length);
            for (int i = 0; i < library.enemies.Length; i++)
            {
                EnemyRecordJson record = library.enemies[i];
                if (record == null)
                {
                    errors.Add($"Authored enemy record at index {i} is null.");
                    continue;
                }

                if (!Enum.TryParse(record.role, true, out SurvivorsEnemyRole role))
                {
                    errors.Add($"Authored enemy {record.id} uses unknown role {record.role}.");
                    continue;
                }

                SurvivorsEnemyProfile fallback = BasicSurvivorsGame.CreateEnemyProfile(role);
                profiles.Add(new SurvivorsEnemyProfile(
                    role,
                    string.IsNullOrWhiteSpace(record.id) ? fallback.Id : record.id,
                    string.IsNullOrWhiteSpace(record.displayName) ? fallback.DisplayName : record.displayName,
                    record.health > 0f ? record.health : fallback.MaxHealth,
                    record.moveSpeed > 0f ? record.moveSpeed : fallback.MoveSpeed,
                    record.radius > 0f ? record.radius : fallback.Radius,
                    record.contactDamage >= 0f ? record.contactDamage : fallback.ContactDamage,
                    record.contactIntervalSeconds > 0f ? record.contactIntervalSeconds : fallback.ContactIntervalSeconds,
                    record.experienceDrop > 0 ? record.experienceDrop : fallback.ExperienceReward,
                    fallback.Tint,
                    record.rangedAttackRange > 0f ? record.rangedAttackRange : fallback.RangedAttackRange,
                    record.rangedAttackDamage > 0f ? record.rangedAttackDamage : fallback.RangedAttackDamage,
                    record.rangedAttackIntervalSeconds > 0f ? record.rangedAttackIntervalSeconds : fallback.RangedAttackIntervalSeconds,
                    record.preferredRange > 0f ? record.preferredRange : fallback.PreferredRange,
                    record.canRecycle,
                    record.canLeash,
                    record.canReposition,
                    record.showOffscreenMarker,
                    record.showOverheadLifeBar,
                    record.showBossLifeBar,
                    record.markerStyle,
                    record.softLeashRadius,
                    record.hardRecycleRadius,
                    record.catchUpSpeedMultiplier,
                    record.repositionTimeoutSeconds,
                    record.spawnTimeSeconds));
            }

            return profiles.ToArray();
        }

        private static Dictionary<SurvivorsEnemyRole, SurvivorsEnemyProfile> IndexEnemyProfiles(SurvivorsEnemyProfile[] profiles, List<string> errors)
        {
            var index = new Dictionary<SurvivorsEnemyRole, SurvivorsEnemyProfile>();
            for (int i = 0; i < profiles.Length; i++)
            {
                SurvivorsEnemyProfile profile = profiles[i];
                if (index.ContainsKey(profile.Role))
                {
                    errors.Add($"Authored enemy content has duplicate role {profile.Role}.");
                    continue;
                }

                index.Add(profile.Role, profile);
            }

            return index;
        }

        private static Dictionary<string, RunFlowProfileRecordJson> IndexRunFlowProfiles(RunFlowLibraryJson library, List<string> errors)
        {
            var index = new Dictionary<string, RunFlowProfileRecordJson>(StringComparer.Ordinal);
            if (library == null || library.profiles == null)
            {
                return index;
            }

            for (int i = 0; i < library.profiles.Length; i++)
            {
                RunFlowProfileRecordJson profile = library.profiles[i];
                if (profile == null)
                {
                    errors.Add($"Authored run-flow profile at index {i} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(profile.id))
                {
                    errors.Add($"Authored run-flow profile at index {i} is missing an id.");
                    continue;
                }

                if (index.ContainsKey(profile.id))
                {
                    errors.Add($"Authored run-flow content has duplicate profile {profile.id}.");
                    continue;
                }

                index.Add(profile.id, profile);
            }

            return index;
        }

        private static SurvivorsMetaProgressionDefinition CreateMetaProgressionDefinition(RewardLibraryJson library, List<string> errors)
        {
            if (library == null)
            {
                return null;
            }

            CurrencyId currencyId = BasicSurvivorsGame.BloodShardsCurrencyId;
            if (library.currencies != null)
            {
                for (int i = 0; i < library.currencies.Length; i++)
                {
                    CurrencyRecordJson currency = library.currencies[i];
                    if (currency != null && !string.IsNullOrWhiteSpace(currency.id))
                    {
                        currencyId = new CurrencyId(currency.id);
                        break;
                    }
                }
            }

            TrackId trackId = BasicSurvivorsGame.LegacyExperienceTrackId;
            if (library.tracks != null)
            {
                for (int i = 0; i < library.tracks.Length; i++)
                {
                    TrackRecordJson track = library.tracks[i];
                    if (track != null && !string.IsNullOrWhiteSpace(track.id))
                    {
                        trackId = new TrackId(track.id);
                        break;
                    }
                }
            }

            var persistentUpgrades = new List<SurvivorsPersistentUpgradeDefinition>();
            if (library.persistentUpgrades != null)
            {
                for (int i = 0; i < library.persistentUpgrades.Length; i++)
                {
                    PersistentUpgradeRecordJson upgrade = library.persistentUpgrades[i];
                    if (upgrade == null || string.IsNullOrWhiteSpace(upgrade.id))
                    {
                        errors.Add($"Authored persistent upgrade at index {i} is missing an id.");
                        continue;
                    }

                    persistentUpgrades.Add(new SurvivorsPersistentUpgradeDefinition(
                        new ResearchNodeId(upgrade.id),
                        upgrade.displayName,
                        upgrade.target,
                        upgrade.effect,
                        upgrade.maxRank,
                        upgrade.rankCosts,
                        upgrade.amountPerRank));
                }
            }

            var rewards = new List<SurvivorsRewardDefinition>();
            if (library.rewards != null)
            {
                for (int i = 0; i < library.rewards.Length; i++)
                {
                    RewardRecordJson reward = library.rewards[i];
                    if (reward == null || string.IsNullOrWhiteSpace(reward.id))
                    {
                        errors.Add($"Authored reward at index {i} is missing an id.");
                        continue;
                    }

                    rewards.Add(new SurvivorsRewardDefinition(
                        reward.id,
                        string.IsNullOrWhiteSpace(reward.currencyId) ? currencyId : new CurrencyId(reward.currencyId),
                        reward.currencyAmount,
                        string.IsNullOrWhiteSpace(reward.trackId) ? trackId : new TrackId(reward.trackId),
                        reward.trackAmount));
                }
            }

            return new SurvivorsMetaProgressionDefinition(currencyId, trackId, persistentUpgrades, rewards);
        }

        private static void RequireEnemyRole(Dictionary<SurvivorsEnemyRole, SurvivorsEnemyProfile> profiles, SurvivorsEnemyRole role, List<string> errors)
        {
            if (profiles == null || !profiles.ContainsKey(role))
            {
                errors.Add($"Authored enemy content is missing role {role}.");
            }
        }

        private static void RequireRunFlowProfile(Dictionary<string, RunFlowProfileRecordJson> profiles, SurvivorsPacingProfile profile, List<string> errors)
        {
            if (profiles == null || !profiles.ContainsKey(profile.ToString()))
            {
                errors.Add($"Authored run-flow content is missing profile {profile}.");
            }
        }

        private static void RequireReward(SurvivorsMetaProgressionDefinition definition, string rewardId, List<string> errors)
        {
            if (definition == null || !definition.TryGetReward(rewardId, out _))
            {
                errors.Add($"Authored reward content is missing reward {rewardId}.");
            }
        }

        private static void ApplyRunFlowProfile(SurvivorsTemplateTuning tuning, RunFlowProfileRecordJson profile)
        {
            tuning.TargetDurationSeconds = profile.targetDurationSeconds;
            tuning.EndlessContinuationEnabled = profile.endlessContinuationEnabled;
            tuning.RunRewardMultiplier = profile.runRewardMultiplier;
            tuning.EvolutionRequiredRankReduction = profile.evolutionRequiredRankReduction;
            tuning.EnemySpawnIntervalSeconds = profile.enemySpawnIntervalSeconds;
            tuning.EnemyMaximumAlive = profile.enemyMaximumAlive;
            tuning.EnemySpawnPackBaseCount = profile.enemySpawnPackBaseCount;
            tuning.EnemySpawnPackMaxCount = profile.enemySpawnPackMaxCount;
            tuning.EnemyRangedAttackDodgeExperienceReward = profile.enemyRangedAttackDodgeExperienceReward;
            tuning.EnemySoftLeashRadius = profile.enemySoftLeashRadius;
            tuning.EnemyHardRecycleRadius = profile.enemyHardRecycleRadius;
            tuning.EnemyRecycleDelaySeconds = profile.enemyRecycleDelaySeconds;
            tuning.EnemyRecycleMinimumRespawnDistance = profile.enemyRecycleMinimumRespawnDistance;
            tuning.EnemyRecycleMaximumRespawnDistance = profile.enemyRecycleMaximumRespawnDistance;
            tuning.MajorThreatCatchUpRadius = profile.majorThreatCatchUpRadius;
            tuning.MajorThreatCatchUpSpeedMultiplier = profile.majorThreatCatchUpSpeedMultiplier;
            tuning.MajorThreatRepositionRadius = profile.majorThreatRepositionRadius;
            tuning.MajorThreatRepositionDelaySeconds = profile.majorThreatRepositionDelaySeconds;
            tuning.OffscreenThreatMarkerDistance = profile.offscreenThreatMarkerDistance;
            tuning.RunEscalationIntervalSeconds = profile.escalationIntervalSeconds;
            tuning.MinimumEnemySpawnIntervalSeconds = profile.minimumEnemySpawnIntervalSeconds;
            tuning.EnemySpawnIntervalReductionPerEscalation = profile.enemySpawnIntervalReductionPerEscalation;
            tuning.EnemyMaximumAliveIncreasePerEscalation = profile.enemyMaximumAliveIncreasePerEscalation;
            tuning.EnemyHealthMultiplierPerEscalation = profile.enemyHealthMultiplierPerEscalation;
            tuning.EnemyMoveSpeedMultiplierPerEscalation = profile.enemyMoveSpeedMultiplierPerEscalation;
            tuning.EnemyExperienceMultiplierPerEscalation = profile.enemyExperienceMultiplierPerEscalation;
            tuning.FirstEliteSpawnTimeSeconds = profile.firstEliteSpawnTimeSeconds;
            tuning.EliteSpawnIntervalSeconds = profile.eliteSpawnIntervalSeconds;
            tuning.FirstDreadEliteSpawnTimeSeconds = profile.firstDreadEliteSpawnTimeSeconds;
            tuning.DreadEliteSpawnIntervalSeconds = profile.dreadEliteSpawnIntervalSeconds;
            tuning.MinibossSpawnTimeSeconds = profile.minibossSpawnTimeSeconds;
            tuning.BossSpawnTimeSeconds = profile.bossSpawnTimeSeconds;
            tuning.SurvivalVictoryTimeSeconds = profile.survivalVictoryTimeSeconds;
            tuning.HordeRushFirstTimeSeconds = profile.hordeRushFirstTimeSeconds;
            tuning.HordeRushIntervalSeconds = profile.hordeRushIntervalSeconds;
            tuning.HordeRushWarningLeadSeconds = profile.hordeRushWarningLeadSeconds;
            tuning.HordeRushBaseEnemyCount = profile.hordeRushBaseEnemyCount;
            tuning.HordeRushEnemyCountIncreasePerRush = profile.hordeRushEnemyCountIncreasePerRush;
            tuning.HordeRushMaxEnemyCount = profile.hordeRushMaxEnemyCount;
            tuning.HordeRushExtraAliveAllowance = profile.hordeRushExtraAliveAllowance;
            tuning.HordeRushSpawnRadius = profile.hordeRushSpawnRadius;
            tuning.HordeRushClearExperienceGemCount = profile.hordeRushClearExperienceGemCount;
            tuning.HordeRushClearExperienceMultiplier = profile.hordeRushClearExperienceMultiplier;
            tuning.HordeRushClearMagnetEveryRush = profile.hordeRushClearMagnetEveryRush;
            tuning.HordeRushClearBloodShardEveryRush = profile.hordeRushClearBloodShardEveryRush;
            tuning.HordeRushClearPulseDamage = profile.hordeRushClearPulseDamage;
            tuning.HordeRushClearPulseRadius = profile.hordeRushClearPulseRadius;
            tuning.PayloadHazardChainSnareThreshold = profile.payloadHazardChainSnareThreshold;
            tuning.PayloadHazardChainWindowSeconds = profile.payloadHazardChainWindowSeconds;
            tuning.PayloadHazardChainCooldownSeconds = profile.payloadHazardChainCooldownSeconds;
            tuning.PayloadHazardChainExperienceGemCount = profile.payloadHazardChainExperienceGemCount;
            tuning.PayloadHazardChainExperienceMultiplier = profile.payloadHazardChainExperienceMultiplier;
            tuning.PayloadHazardChainPulseDamage = profile.payloadHazardChainPulseDamage;
            tuning.PayloadHazardChainPulseRadius = profile.payloadHazardChainPulseRadius;
            tuning.RoamingCacheTravelInterval = profile.roamingCacheTravelInterval;
            tuning.RoamingCacheExperienceGemCount = profile.roamingCacheExperienceGemCount;
            tuning.RoamingCacheMagnetInterval = profile.roamingCacheMagnetInterval;
            tuning.RoamingCacheBloodShardInterval = profile.roamingCacheBloodShardInterval;
            tuning.RoamingCacheAmbushStartCache = profile.roamingCacheAmbushStartCache;
            tuning.RoamingCacheAmbushInterval = profile.roamingCacheAmbushInterval;
            tuning.RoamingCacheAmbushBaseEnemyCount = profile.roamingCacheAmbushBaseEnemyCount;
            tuning.RoamingCacheAmbushMaxEnemyCount = profile.roamingCacheAmbushMaxEnemyCount;
            tuning.RoamingCacheAmbushExtraAliveAllowance = profile.roamingCacheAmbushExtraAliveAllowance;
            tuning.RoamingCacheAmbushRadius = profile.roamingCacheAmbushRadius;
            tuning.RoamingCacheAmbushClearMagnetInterval = profile.roamingCacheAmbushClearMagnetInterval;
            tuning.RoamingCacheAmbushClearBloodShardInterval = profile.roamingCacheAmbushClearBloodShardInterval;
            tuning.RoamingCacheSurgeInterval = profile.roamingCacheSurgeInterval;
            tuning.RoamingCacheSurgeBonusGemCount = profile.roamingCacheSurgeBonusGemCount;
            tuning.RoamingCacheSurgeDurationSeconds = profile.roamingCacheSurgeDurationSeconds;
            tuning.RoamingCacheSurgeDamageBonus = profile.roamingCacheSurgeDamageBonus;
            tuning.RoamingCacheSurgeMoveSpeedBonus = profile.roamingCacheSurgeMoveSpeedBonus;
            tuning.RoamingCacheSurgeCooldownMultiplierBonus = profile.roamingCacheSurgeCooldownMultiplierBonus;
            tuning.RoamingCacheSurgePickupRangeBonus = profile.roamingCacheSurgePickupRangeBonus;
            tuning.RoamingCacheSurgePulseDamage = profile.roamingCacheSurgePulseDamage;
            tuning.RoamingCacheSurgePulseRadius = profile.roamingCacheSurgePulseRadius;
            tuning.ArenaShrineTravelInterval = profile.arenaShrineTravelInterval;
            tuning.ArenaShrineBaseEnemyCount = profile.arenaShrineBaseEnemyCount;
            tuning.ArenaShrineEnemyCountIncreasePerTrial = profile.arenaShrineEnemyCountIncreasePerTrial;
            tuning.ArenaShrineMaxEnemyCount = profile.arenaShrineMaxEnemyCount;
            tuning.ArenaShrineExtraAliveAllowance = profile.arenaShrineExtraAliveAllowance;
            tuning.ArenaShrineSpawnRadius = profile.arenaShrineSpawnRadius;
            tuning.ArenaShrineClearExperienceGemCount = profile.arenaShrineClearExperienceGemCount;
            tuning.ArenaShrineClearExperienceMultiplier = profile.arenaShrineClearExperienceMultiplier;
            tuning.ArenaShrineClearBloodShardAmount = profile.arenaShrineClearBloodShardAmount;
            tuning.ArenaShrineSurgeDurationSeconds = profile.arenaShrineSurgeDurationSeconds;
            tuning.ArenaShrineSurgeDamageBonus = profile.arenaShrineSurgeDamageBonus;
            tuning.ArenaShrineSurgeMoveSpeedBonus = profile.arenaShrineSurgeMoveSpeedBonus;
            tuning.ArenaShrineSurgeCooldownMultiplierBonus = profile.arenaShrineSurgeCooldownMultiplierBonus;
            tuning.ArenaShrineSurgePickupRangeBonus = profile.arenaShrineSurgePickupRangeBonus;
            tuning.ArenaShrineSurgePulseDamage = profile.arenaShrineSurgePulseDamage;
            tuning.ArenaShrineSurgePulseRadius = profile.arenaShrineSurgePulseRadius;
            tuning.DraftChoiceCount = profile.draftChoiceCount;
            tuning.DraftRerollCharges = profile.draftRerollCharges;
            tuning.DraftBanishCharges = profile.draftBanishCharges;
            tuning.DraftSkipBloodShards = profile.draftSkipBloodShards;
            tuning.RewardSelectionTimeoutSeconds = profile.rewardSelectionTimeoutSeconds;
            tuning.ExperienceRequiredBase = profile.experienceRequiredBase;
            tuning.ExperienceRequiredPerLevel = profile.experienceRequiredPerLevel;
            tuning.LevelUpDraftCooldownSeconds = profile.levelUpDraftCooldownSeconds;
            tuning.MaximumQueuedLevelUps = profile.maximumQueuedLevelUps;
            tuning.PickupMagnetPulseBaseIntervalSeconds = profile.pickupMagnetPulseBaseIntervalSeconds;
            tuning.PickupMagnetPulseMinimumIntervalSeconds = profile.pickupMagnetPulseMinimumIntervalSeconds;
            tuning.MaxWeaponSlots = profile.maxWeaponSlots;
            tuning.MaxPassiveSlots = profile.maxPassiveSlots;
            tuning.DraftMidRarityLevel = profile.draftMidRarityLevel;
            tuning.DraftLateRarityLevel = profile.draftLateRarityLevel;
            ApplyRarityTables(tuning, profile.rarityTables);
            tuning.EndlessEliteSpawnIntervalSeconds = profile.endlessEliteSpawnIntervalSeconds;
            tuning.EndlessMinibossSpawnIntervalSeconds = profile.endlessMinibossSpawnIntervalSeconds;
            tuning.EndlessBossSpawnIntervalSeconds = profile.endlessBossSpawnIntervalSeconds;
            tuning.EndlessSurgeExperienceGemCount = profile.endlessSurgeExperienceGemCount;
            tuning.EndlessSurgeExperienceMultiplier = profile.endlessSurgeExperienceMultiplier;
            tuning.EndlessSurgeBloodShardAmount = profile.endlessSurgeBloodShardAmount;
            tuning.EndlessSurgeDurationSeconds = profile.endlessSurgeDurationSeconds;
            tuning.EndlessSurgeDamageBonus = profile.endlessSurgeDamageBonus;
            tuning.EndlessSurgeMoveSpeedBonus = profile.endlessSurgeMoveSpeedBonus;
            tuning.EndlessSurgeCooldownMultiplierBonus = profile.endlessSurgeCooldownMultiplierBonus;
            tuning.EndlessSurgePickupRangeBonus = profile.endlessSurgePickupRangeBonus;
            tuning.EndlessSurgePulseDamage = profile.endlessSurgePulseDamage;
            tuning.EndlessSurgePulseRadius = profile.endlessSurgePulseRadius;
        }

        private static void ApplyRarityTables(SurvivorsTemplateTuning tuning, RarityTableRecordJson[] tables)
        {
            if (tables == null)
            {
                return;
            }

            for (int i = 0; i < tables.Length; i++)
            {
                RarityTableRecordJson table = tables[i];
                if (table == null)
                {
                    continue;
                }

                switch (table.id)
                {
                    case "NormalEarly":
                        tuning.NormalEarlyCommonWeight = table.common;
                        tuning.NormalEarlyUncommonWeight = table.uncommon;
                        tuning.NormalEarlyRareWeight = table.rare;
                        tuning.NormalEarlyEpicWeight = table.epic;
                        tuning.NormalEarlyLegendaryWeight = table.legendary;
                        break;
                    case "NormalMid":
                        tuning.NormalMidCommonWeight = table.common;
                        tuning.NormalMidUncommonWeight = table.uncommon;
                        tuning.NormalMidRareWeight = table.rare;
                        tuning.NormalMidEpicWeight = table.epic;
                        tuning.NormalMidLegendaryWeight = table.legendary;
                        break;
                    case "NormalLate":
                        tuning.NormalLateCommonWeight = table.common;
                        tuning.NormalLateUncommonWeight = table.uncommon;
                        tuning.NormalLateRareWeight = table.rare;
                        tuning.NormalLateEpicWeight = table.epic;
                        tuning.NormalLateLegendaryWeight = table.legendary;
                        break;
                    case "Elite":
                        tuning.EliteCommonWeight = table.common;
                        tuning.EliteUncommonWeight = table.uncommon;
                        tuning.EliteRareWeight = table.rare;
                        tuning.EliteEpicWeight = table.epic;
                        tuning.EliteLegendaryWeight = table.legendary;
                        break;
                    case "Boss":
                        tuning.BossCommonWeight = table.common;
                        tuning.BossUncommonWeight = table.uncommon;
                        tuning.BossRareWeight = table.rare;
                        tuning.BossEpicWeight = table.epic;
                        tuning.BossLegendaryWeight = table.legendary;
                        break;
                }
            }
        }

        [Serializable]
        private sealed class EnemyLibraryJson
        {
            public EnemyRecordJson[] enemies;
        }

        [Serializable]
        private sealed class EnemyRecordJson
        {
            public string id;
            public string displayName;
            public string role;
            public float health;
            public float moveSpeed;
            public float radius;
            public float contactDamage;
            public float contactIntervalSeconds;
            public int experienceDrop;
            public float spawnTimeSeconds;
            public float rangedAttackRange;
            public float rangedAttackDamage;
            public float rangedAttackIntervalSeconds;
            public float preferredRange;
            public bool canRecycle;
            public bool canLeash;
            public bool canReposition;
            public bool showOffscreenMarker;
            public bool showOverheadLifeBar;
            public bool showBossLifeBar;
            public string markerStyle;
            public float softLeashRadius;
            public float hardRecycleRadius;
            public float catchUpSpeedMultiplier;
            public float repositionTimeoutSeconds;
        }

        [Serializable]
        private sealed class RewardLibraryJson
        {
            public CurrencyRecordJson[] currencies;
            public TrackRecordJson[] tracks;
            public PersistentUpgradeRecordJson[] persistentUpgrades;
            public RewardRecordJson[] rewards;
        }

        [Serializable]
        private sealed class CurrencyRecordJson
        {
            public string id;
        }

        [Serializable]
        private sealed class TrackRecordJson
        {
            public string id;
        }

        [Serializable]
        private sealed class PersistentUpgradeRecordJson
        {
            public string id;
            public string displayName;
            public string target;
            public string effect;
            public int maxRank;
            public int[] rankCosts;
            public float amountPerRank;
        }

        [Serializable]
        private sealed class RewardRecordJson
        {
            public string id;
            public string currencyId;
            public int currencyAmount;
            public string trackId;
            public int trackAmount;
        }

        [Serializable]
        private sealed class RunFlowLibraryJson
        {
            public RunFlowProfileRecordJson[] profiles;
        }

        [Serializable]
        private sealed class RunFlowProfileRecordJson
        {
            public string id;
            public float targetDurationSeconds;
            public float runRewardMultiplier;
            public int evolutionRequiredRankReduction;
            public bool endlessContinuationEnabled;
            public float enemySpawnIntervalSeconds;
            public int enemyMaximumAlive;
            public int enemySpawnPackBaseCount;
            public int enemySpawnPackMaxCount;
            public int enemyRangedAttackDodgeExperienceReward;
            public float enemySoftLeashRadius;
            public float enemyHardRecycleRadius;
            public float enemyRecycleDelaySeconds;
            public float enemyRecycleMinimumRespawnDistance;
            public float enemyRecycleMaximumRespawnDistance;
            public float majorThreatCatchUpRadius;
            public float majorThreatCatchUpSpeedMultiplier;
            public float majorThreatRepositionRadius;
            public float majorThreatRepositionDelaySeconds;
            public float offscreenThreatMarkerDistance;
            public float escalationIntervalSeconds;
            public float minimumEnemySpawnIntervalSeconds;
            public float enemySpawnIntervalReductionPerEscalation;
            public int enemyMaximumAliveIncreasePerEscalation;
            public float enemyHealthMultiplierPerEscalation;
            public float enemyMoveSpeedMultiplierPerEscalation;
            public float enemyExperienceMultiplierPerEscalation;
            public float firstEliteSpawnTimeSeconds;
            public float eliteSpawnIntervalSeconds;
            public float firstDreadEliteSpawnTimeSeconds;
            public float dreadEliteSpawnIntervalSeconds;
            public float minibossSpawnTimeSeconds;
            public float bossSpawnTimeSeconds;
            public float survivalVictoryTimeSeconds;
            public float hordeRushFirstTimeSeconds;
            public float hordeRushIntervalSeconds;
            public float hordeRushWarningLeadSeconds;
            public int hordeRushBaseEnemyCount;
            public int hordeRushEnemyCountIncreasePerRush;
            public int hordeRushMaxEnemyCount;
            public int hordeRushExtraAliveAllowance;
            public float hordeRushSpawnRadius;
            public int hordeRushClearExperienceGemCount;
            public float hordeRushClearExperienceMultiplier;
            public int hordeRushClearMagnetEveryRush;
            public int hordeRushClearBloodShardEveryRush;
            public float hordeRushClearPulseDamage;
            public float hordeRushClearPulseRadius;
            public int payloadHazardChainSnareThreshold;
            public float payloadHazardChainWindowSeconds;
            public float payloadHazardChainCooldownSeconds;
            public int payloadHazardChainExperienceGemCount;
            public float payloadHazardChainExperienceMultiplier;
            public float payloadHazardChainPulseDamage;
            public float payloadHazardChainPulseRadius;
            public float roamingCacheTravelInterval;
            public int roamingCacheExperienceGemCount;
            public int roamingCacheMagnetInterval;
            public int roamingCacheBloodShardInterval;
            public int roamingCacheAmbushStartCache;
            public int roamingCacheAmbushInterval;
            public int roamingCacheAmbushBaseEnemyCount;
            public int roamingCacheAmbushMaxEnemyCount;
            public int roamingCacheAmbushExtraAliveAllowance;
            public float roamingCacheAmbushRadius;
            public int roamingCacheAmbushClearMagnetInterval;
            public int roamingCacheAmbushClearBloodShardInterval;
            public int roamingCacheSurgeInterval;
            public int roamingCacheSurgeBonusGemCount;
            public float roamingCacheSurgeDurationSeconds;
            public float roamingCacheSurgeDamageBonus;
            public float roamingCacheSurgeMoveSpeedBonus;
            public float roamingCacheSurgeCooldownMultiplierBonus;
            public float roamingCacheSurgePickupRangeBonus;
            public float roamingCacheSurgePulseDamage;
            public float roamingCacheSurgePulseRadius;
            public float arenaShrineTravelInterval;
            public int arenaShrineBaseEnemyCount;
            public int arenaShrineEnemyCountIncreasePerTrial;
            public int arenaShrineMaxEnemyCount;
            public int arenaShrineExtraAliveAllowance;
            public float arenaShrineSpawnRadius;
            public int arenaShrineClearExperienceGemCount;
            public float arenaShrineClearExperienceMultiplier;
            public int arenaShrineClearBloodShardAmount;
            public float arenaShrineSurgeDurationSeconds;
            public float arenaShrineSurgeDamageBonus;
            public float arenaShrineSurgeMoveSpeedBonus;
            public float arenaShrineSurgeCooldownMultiplierBonus;
            public float arenaShrineSurgePickupRangeBonus;
            public float arenaShrineSurgePulseDamage;
            public float arenaShrineSurgePulseRadius;
            public int draftChoiceCount;
            public int draftRerollCharges;
            public int draftBanishCharges;
            public int draftSkipBloodShards;
            public float rewardSelectionTimeoutSeconds;
            public int experienceRequiredBase;
            public int experienceRequiredPerLevel;
            public float levelUpDraftCooldownSeconds;
            public int maximumQueuedLevelUps;
            public float pickupMagnetPulseBaseIntervalSeconds;
            public float pickupMagnetPulseMinimumIntervalSeconds;
            public int maxWeaponSlots;
            public int maxPassiveSlots;
            public int draftMidRarityLevel;
            public int draftLateRarityLevel;
            public RarityTableRecordJson[] rarityTables;
            public float endlessEliteSpawnIntervalSeconds;
            public float endlessMinibossSpawnIntervalSeconds;
            public float endlessBossSpawnIntervalSeconds;
            public int endlessSurgeExperienceGemCount;
            public float endlessSurgeExperienceMultiplier;
            public int endlessSurgeBloodShardAmount;
            public float endlessSurgeDurationSeconds;
            public float endlessSurgeDamageBonus;
            public float endlessSurgeMoveSpeedBonus;
            public float endlessSurgeCooldownMultiplierBonus;
            public float endlessSurgePickupRangeBonus;
            public float endlessSurgePulseDamage;
            public float endlessSurgePulseRadius;
        }

        [Serializable]
        private sealed class RarityTableRecordJson
        {
            public string id;
            public int common;
            public int uncommon;
            public int rare;
            public int epic;
            public int legendary;
        }
    }
}
