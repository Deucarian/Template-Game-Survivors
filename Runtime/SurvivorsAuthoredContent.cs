using System;
using System.Collections.Generic;
using Deucarian.Progression;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    public enum SurvivorsAuthoredContentBindingPolicy
    {
        AllowFallbacks = 0,
        StrictSample = 1
    }

    public sealed class SurvivorsAuthoredContentDefinition
    {
        private readonly SurvivorsEnemyProfile[] _enemyProfiles;
        private readonly Dictionary<SurvivorsEnemyRole, SurvivorsEnemyProfile> _enemyProfilesByRole;
        private readonly Dictionary<string, RunFlowProfileRecordJson> _runFlowProfiles;
        private readonly SurvivorsGameplayTuningRecordJson _sharedGameplayTuning;
        private readonly SurvivorsMetaProgressionDefinition _metaProgressionDefinition;
        private readonly SurvivorsWeaponArchetypeDefinition[] _weaponDefinitions;
        private readonly RunUpgradeCatalog _runUpgradeCatalog;
        private readonly SurvivorsRunUpgradeMetadata[] _runUpgradeMetadata;
        private readonly SurvivorsClassUpgradeGateDefinition[] _classUpgradeGates;
        private readonly SurvivorsRelicDefinition[] _relicDefinitions;
        private readonly SurvivorsClassLibraryDefinition _classLibrary;
        private readonly SurvivorsProgressionTrackDefinition[] _progressionTracks;

        private SurvivorsAuthoredContentDefinition(
            SurvivorsEnemyProfile[] enemyProfiles,
            Dictionary<SurvivorsEnemyRole, SurvivorsEnemyProfile> enemyProfilesByRole,
            Dictionary<string, RunFlowProfileRecordJson> runFlowProfiles,
            SurvivorsGameplayTuningRecordJson sharedGameplayTuning,
            SurvivorsMetaProgressionDefinition metaProgressionDefinition,
            SurvivorsWeaponArchetypeDefinition[] weaponDefinitions,
            RunUpgradeCatalog runUpgradeCatalog,
            SurvivorsRunUpgradeMetadata[] runUpgradeMetadata,
            SurvivorsClassUpgradeGateDefinition[] classUpgradeGates,
            SurvivorsRelicDefinition[] relicDefinitions,
            SurvivorsClassLibraryDefinition classLibrary,
            SurvivorsProgressionTrackDefinition[] progressionTracks,
            SurvivorsAuthoredContentBindingPolicy bindingPolicy,
            string sourceSummary)
        {
            _enemyProfiles = enemyProfiles ?? Array.Empty<SurvivorsEnemyProfile>();
            _enemyProfilesByRole = enemyProfilesByRole ?? new Dictionary<SurvivorsEnemyRole, SurvivorsEnemyProfile>();
            _runFlowProfiles = runFlowProfiles ?? new Dictionary<string, RunFlowProfileRecordJson>(StringComparer.Ordinal);
            _sharedGameplayTuning = sharedGameplayTuning;
            _metaProgressionDefinition = metaProgressionDefinition;
            _weaponDefinitions = weaponDefinitions ?? Array.Empty<SurvivorsWeaponArchetypeDefinition>();
            _runUpgradeCatalog = runUpgradeCatalog;
            _runUpgradeMetadata = runUpgradeMetadata ?? Array.Empty<SurvivorsRunUpgradeMetadata>();
            _classUpgradeGates = classUpgradeGates ?? Array.Empty<SurvivorsClassUpgradeGateDefinition>();
            _relicDefinitions = relicDefinitions ?? Array.Empty<SurvivorsRelicDefinition>();
            _classLibrary = classLibrary;
            _progressionTracks = progressionTracks ?? Array.Empty<SurvivorsProgressionTrackDefinition>();
            BindingPolicy = bindingPolicy;
            SourceSummary = sourceSummary ?? string.Empty;
        }

        public string SourceSummary { get; }
        public SurvivorsAuthoredContentBindingPolicy BindingPolicy { get; }
        public bool IsStrictSample => BindingPolicy == SurvivorsAuthoredContentBindingPolicy.StrictSample;
        public bool UsesBuiltInFallbacks => BindingPolicy == SurvivorsAuthoredContentBindingPolicy.AllowFallbacks;
        public bool HasEnemyProfiles => _enemyProfiles.Length > 0;
        public bool HasRunFlowProfiles => _runFlowProfiles.Count > 0;
        public bool HasMetaProgression => _metaProgressionDefinition != null;
        public bool HasWeaponDefinitions => _weaponDefinitions.Length > 0;
        public bool HasRunUpgradeCatalog => _runUpgradeCatalog != null;
        public bool HasRunUpgradeMetadata => _runUpgradeMetadata.Length > 0;
        public bool HasClassUpgradeGates => _classUpgradeGates.Length > 0;
        public bool HasRelicDefinitions => _relicDefinitions.Length > 0;
        public bool HasClassLibrary => _classLibrary != null;
        public bool HasProgressionTracks => _progressionTracks.Length > 0;
        public IReadOnlyList<SurvivorsEnemyProfile> EnemyProfiles => _enemyProfiles;
        public SurvivorsMetaProgressionDefinition MetaProgressionDefinition => _metaProgressionDefinition;
        public IReadOnlyList<SurvivorsWeaponArchetypeDefinition> WeaponDefinitions => _weaponDefinitions;
        public RunUpgradeCatalog RunUpgradeCatalog => _runUpgradeCatalog;
        public IReadOnlyList<SurvivorsRunUpgradeMetadata> RunUpgradeMetadata => _runUpgradeMetadata;
        public IReadOnlyList<SurvivorsClassUpgradeGateDefinition> ClassUpgradeGates => _classUpgradeGates;
        public IReadOnlyList<SurvivorsRelicDefinition> RelicDefinitions => _relicDefinitions;
        public SurvivorsClassLibraryDefinition ClassLibrary => _classLibrary;
        public IReadOnlyList<SurvivorsProgressionTrackDefinition> ProgressionTracks => _progressionTracks;
        public static IReadOnlyList<string> AuthoredGameplayTuningFieldNames => SurvivorsGameplayTuningRecordJson.FieldNames;

        public static bool TryCreate(
            string enemyJson,
            string runFlowJson,
            string rewardJson,
            out SurvivorsAuthoredContentDefinition definition,
            out string error)
        {
            return TryCreateCore(
                weaponJson: null,
                upgradeJson: null,
                relicJson: null,
                classJson: null,
                progressionJson: null,
                enemyJson: enemyJson,
                runFlowJson: runFlowJson,
                rewardJson: rewardJson,
                requireFullContent: false,
                SurvivorsAuthoredContentBindingPolicy.AllowFallbacks,
                out definition,
                out error);
        }

        public static bool TryCreate(
            string weaponJson,
            string upgradeJson,
            string relicJson,
            string classJson,
            string progressionJson,
            string enemyJson,
            string runFlowJson,
            string rewardJson,
            out SurvivorsAuthoredContentDefinition definition,
            out string error)
        {
            return TryCreate(
                weaponJson,
                upgradeJson,
                relicJson,
                classJson,
                progressionJson,
                enemyJson,
                runFlowJson,
                rewardJson,
                SurvivorsAuthoredContentBindingPolicy.StrictSample,
                out definition,
                out error);
        }

        public static bool TryCreate(
            string weaponJson,
            string upgradeJson,
            string relicJson,
            string classJson,
            string progressionJson,
            string enemyJson,
            string runFlowJson,
            string rewardJson,
            SurvivorsAuthoredContentBindingPolicy bindingPolicy,
            out SurvivorsAuthoredContentDefinition definition,
            out string error)
        {
            return TryCreateCore(
                weaponJson,
                upgradeJson,
                relicJson,
                classJson,
                progressionJson,
                enemyJson,
                runFlowJson,
                rewardJson,
                requireFullContent: true,
                bindingPolicy,
                out definition,
                out error);
        }

        private static bool TryCreateCore(
            string weaponJson,
            string upgradeJson,
            string relicJson,
            string classJson,
            string progressionJson,
            string enemyJson,
            string runFlowJson,
            string rewardJson,
            bool requireFullContent,
            SurvivorsAuthoredContentBindingPolicy bindingPolicy,
            out SurvivorsAuthoredContentDefinition definition,
            out string error)
        {
            definition = null;
            var errors = new List<string>();
            bool strictSample = bindingPolicy == SurvivorsAuthoredContentBindingPolicy.StrictSample;
            if (requireFullContent && strictSample)
            {
                SurvivorsContentValidationResult validation = SurvivorsContentValidator.ValidateStrictAuthoredBindingJson(
                    weaponJson,
                    upgradeJson,
                    enemyJson,
                    rewardJson,
                    relicJson,
                    classJson,
                    progressionJson,
                    runFlowJson);
                if (!validation.Succeeded)
                {
                    error = string.Join("; ", validation.Errors);
                    return false;
                }
            }

            WeaponLibraryJson weapons = ParseJson<WeaponLibraryJson>(weaponJson, "weapon library", errors, requireFullContent);
            UpgradeLibraryJson upgrades = ParseJson<UpgradeLibraryJson>(upgradeJson, "upgrade library", errors, requireFullContent);
            RelicLibraryJson relics = ParseJson<RelicLibraryJson>(relicJson, "relic library", errors, requireFullContent);
            ClassLibraryJson classes = ParseJson<ClassLibraryJson>(classJson, "class library", errors, requireFullContent);
            ProgressionLibraryJson progression = ParseJson<ProgressionLibraryJson>(progressionJson, "progression library", errors, requireFullContent);
            EnemyLibraryJson enemies = ParseJson<EnemyLibraryJson>(enemyJson, "enemy library", errors, required: true);
            RunFlowLibraryJson runFlow = ParseJson<RunFlowLibraryJson>(runFlowJson, "run flow library", errors, required: true);
            RewardLibraryJson rewards = ParseJson<RewardLibraryJson>(rewardJson, "reward library", errors, required: true);

            SurvivorsWeaponArchetypeDefinition[] weaponDefinitions = CreateWeaponDefinitions(weapons, errors, !strictSample);
            RunUpgradeCatalog runUpgradeCatalog = CreateRunUpgradeCatalog(upgrades, errors, strictSample);
            SurvivorsRelicDefinition[] relicDefinitions = CreateRelicDefinitions(relics, errors);
            SurvivorsClassLibraryDefinition classLibrary = CreateClassLibraryDefinition(classes, errors);
            SurvivorsProgressionTrackDefinition[] progressionTracks = CreateProgressionTracks(progression, errors);
            SurvivorsRunUpgradeMetadata[] runUpgradeMetadata = CreateRunUpgradeMetadata(upgrades, errors);
            SurvivorsClassUpgradeGateDefinition[] classUpgradeGates = CreateClassUpgradeGates(upgrades, progressionTracks, !strictSample);
            SurvivorsEnemyProfile[] enemyProfiles = CreateEnemyProfiles(enemies, errors, !strictSample);
            Dictionary<SurvivorsEnemyRole, SurvivorsEnemyProfile> enemyProfilesByRole = IndexEnemyProfiles(enemyProfiles, errors);
            Dictionary<string, RunFlowProfileRecordJson> runFlowProfiles = IndexRunFlowProfiles(runFlow, errors);
            SurvivorsMetaProgressionDefinition metaProgression = CreateMetaProgressionDefinition(rewards, errors, !strictSample);

            if (requireFullContent)
            {
                RequireWeaponDefinitions(weaponDefinitions, errors);
                RequireRunUpgradeCatalog(runUpgradeCatalog, errors);
                RequireRunUpgradeMetadata(runUpgradeMetadata, errors);
                RequireRelicDefinitions(relicDefinitions, errors);
                RequireClassLibrary(classLibrary, errors);
                RequireProgressionTracks(progressionTracks, errors);
            }

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

            string summary = BuildSourceSummary(
                weaponDefinitions,
                runUpgradeCatalog,
                runUpgradeMetadata,
                relicDefinitions,
                classLibrary,
                progressionTracks,
                enemyProfiles,
                runFlowProfiles,
                metaProgression);
            definition = new SurvivorsAuthoredContentDefinition(
                enemyProfiles,
                enemyProfilesByRole,
                runFlowProfiles,
                runFlow == null ? null : runFlow.sharedGameplayTuning,
                metaProgression,
                weaponDefinitions,
                runUpgradeCatalog,
                runUpgradeMetadata,
                classUpgradeGates,
                relicDefinitions,
                classLibrary,
                progressionTracks,
                bindingPolicy,
                summary);
            error = string.Empty;
            return true;
        }

        public SurvivorsTemplateTuning CreateTuning(SurvivorsPacingProfile profile)
        {
            SurvivorsTemplateTuning tuning = BasicSurvivorsGame.CreateTuning(profile);
            if (!TryGetRunFlowProfile(profile, out RunFlowProfileRecordJson record))
            {
                if (IsStrictSample)
                {
                    throw new InvalidOperationException($"Strict authored content is missing run-flow profile {profile}.");
                }

                return tuning;
            }

            ApplyGameplayTuning(tuning, _sharedGameplayTuning, applyOnlyAuthoredOverrides: !IsStrictSample);
            ApplyRunFlowProfile(tuning, record);
            ApplyGameplayTuning(tuning, record.gameplayTuningOverrides, applyOnlyAuthoredOverrides: true);
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
                if (IsStrictSample)
                {
                    throw new InvalidOperationException($"Strict authored content is missing enemy role {role}.");
                }

                return BasicSurvivorsGame.CreateEnemyProfile(role, tuning);
            }

            SurvivorsTemplateTuning resolved = tuning ?? CreateTuning(SurvivorsPacingProfile.HumanPlaytest);
            SurvivorsTemplateTuning baseline = CreateTuning(SurvivorsPacingProfile.HumanPlaytest);
            float baselineHealth = ResolveEnemyHealthBasis(role, baseline);
            float profileHealth = ResolveEnemyHealthBasis(role, resolved);
            float baselineMoveSpeed = ResolveEnemyMoveSpeedBasis(role, baseline);
            float profileMoveSpeed = ResolveEnemyMoveSpeedBasis(role, resolved);
            float baselineRadius = ResolveEnemyRadiusBasis(role, baseline);
            float profileRadius = ResolveEnemyRadiusBasis(role, resolved);
            float baselineContactDamage = ResolveEnemyContactDamageBasis(role, baseline);
            float profileContactDamage = ResolveEnemyContactDamageBasis(role, resolved);
            float baselineContactInterval = ResolveEnemyContactIntervalBasis(role, baseline);
            float profileContactInterval = ResolveEnemyContactIntervalBasis(role, resolved);
            int baselineExperience = ResolveEnemyExperienceBasis(role, baseline);
            int profileExperience = ResolveEnemyExperienceBasis(role, resolved);
            return new SurvivorsEnemyProfile(
                role,
                authored.Id,
                authored.DisplayName,
                ScaleAuthoredFloat(authored.MaxHealth, baselineHealth, profileHealth),
                ScaleAuthoredFloat(authored.MoveSpeed, baselineMoveSpeed, profileMoveSpeed),
                ScaleAuthoredFloat(authored.Radius, baselineRadius, profileRadius),
                ScaleAuthoredFloat(authored.ContactDamage, baselineContactDamage, profileContactDamage),
                ScaleAuthoredFloat(authored.ContactIntervalSeconds, baselineContactInterval, profileContactInterval),
                ScaleAuthoredInt(authored.ExperienceReward, baselineExperience, profileExperience),
                authored.Tint,
                authored.RangedAttackRange,
                ScaleAuthoredFloat(authored.RangedAttackDamage, baselineContactDamage, profileContactDamage),
                authored.RangedAttackIntervalSeconds,
                authored.PreferredRange,
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

        private static float ResolveEnemyHealthBasis(SurvivorsEnemyRole role, SurvivorsTemplateTuning tuning)
        {
            if (role == SurvivorsEnemyRole.Miniboss)
            {
                return tuning.MinibossMaxHealth;
            }

            return role == SurvivorsEnemyRole.Boss ? tuning.BossMaxHealth : tuning.EnemyMaxHealth;
        }

        private static float ResolveEnemyMoveSpeedBasis(SurvivorsEnemyRole role, SurvivorsTemplateTuning tuning)
        {
            if (role == SurvivorsEnemyRole.Miniboss)
            {
                return tuning.MinibossMoveSpeed;
            }

            return role == SurvivorsEnemyRole.Boss ? tuning.BossMoveSpeed : tuning.EnemyMoveSpeed;
        }

        private static float ResolveEnemyRadiusBasis(SurvivorsEnemyRole role, SurvivorsTemplateTuning tuning)
        {
            if (role == SurvivorsEnemyRole.Miniboss)
            {
                return tuning.MinibossRadius;
            }

            return role == SurvivorsEnemyRole.Boss ? tuning.BossRadius : tuning.EnemyRadius;
        }

        private static float ResolveEnemyContactDamageBasis(SurvivorsEnemyRole role, SurvivorsTemplateTuning tuning)
        {
            if (role == SurvivorsEnemyRole.Miniboss)
            {
                return tuning.MinibossContactDamage;
            }

            return role == SurvivorsEnemyRole.Boss ? tuning.BossContactDamage : tuning.EnemyContactDamage;
        }

        private static float ResolveEnemyContactIntervalBasis(SurvivorsEnemyRole role, SurvivorsTemplateTuning tuning)
        {
            if (role == SurvivorsEnemyRole.Miniboss)
            {
                return tuning.MinibossContactIntervalSeconds;
            }

            return role == SurvivorsEnemyRole.Boss ? tuning.BossContactIntervalSeconds : tuning.EnemyContactIntervalSeconds;
        }

        private static int ResolveEnemyExperienceBasis(SurvivorsEnemyRole role, SurvivorsTemplateTuning tuning)
        {
            if (role == SurvivorsEnemyRole.Miniboss)
            {
                return tuning.MinibossExperienceReward;
            }

            return role == SurvivorsEnemyRole.Boss ? tuning.BossExperienceReward : tuning.EnemyExperienceReward;
        }

        private static float ScaleAuthoredFloat(float authoredValue, float baselineValue, float profileValue)
        {
            if (baselineValue <= 0f)
            {
                return authoredValue;
            }

            return authoredValue * (profileValue / baselineValue);
        }

        private static int ScaleAuthoredInt(int authoredValue, int baselineValue, int profileValue)
        {
            if (baselineValue <= 0)
            {
                return authoredValue;
            }

            return Mathf.Max(0, Mathf.RoundToInt(authoredValue * (profileValue / (float)baselineValue)));
        }

        private IReadOnlyList<SurvivorsEnemyProfile> CreateEnemyProfileDefinitions(SurvivorsTemplateTuning tuning)
        {
            if (_enemyProfiles.Length == 0)
            {
                if (IsStrictSample)
                {
                    throw new InvalidOperationException("Strict authored content has no enemy profiles.");
                }

                return BasicSurvivorsGame.CreateEnemyProfileDefinitions(tuning);
            }

            SurvivorsEnemyProfile[] profiles = new SurvivorsEnemyProfile[_enemyProfiles.Length];
            for (int i = 0; i < _enemyProfiles.Length; i++)
            {
                profiles[i] = ResolveEnemyProfile(_enemyProfiles[i].Role, tuning);
            }

            return profiles;
        }

        private static SurvivorsWeaponArchetypeDefinition[] CreateWeaponDefinitions(
            WeaponLibraryJson library,
            List<string> errors,
            bool allowFallbacks)
        {
            if (library == null || library.weapons == null)
            {
                return Array.Empty<SurvivorsWeaponArchetypeDefinition>();
            }

            Dictionary<string, ProjectileRecordJson> projectiles = IndexProjectiles(library.projectiles, errors);
            Dictionary<string, SurvivorsWeaponArchetypeDefinition> fallbackById = allowFallbacks
                ? IndexWeapons(BasicSurvivorsGame.CreateWeaponArchetypeDefinitions())
                : new Dictionary<string, SurvivorsWeaponArchetypeDefinition>(StringComparer.Ordinal);
            var definitions = new List<SurvivorsWeaponArchetypeDefinition>(library.weapons.Length);
            for (int i = 0; i < library.weapons.Length; i++)
            {
                WeaponRecordJson record = library.weapons[i];
                if (record == null)
                {
                    errors.Add($"Authored weapon record at index {i} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(record.id))
                {
                    errors.Add($"Authored weapon record at index {i} is missing an id.");
                    continue;
                }

                if (!Enum.TryParse(record.fireMode, true, out SurvivorsWeaponArchetype archetype))
                {
                    errors.Add($"Authored weapon {record.id} uses unknown fire mode {record.fireMode}.");
                    continue;
                }

                fallbackById.TryGetValue(record.id, out SurvivorsWeaponArchetypeDefinition fallback);
                projectiles.TryGetValue(record.projectileId ?? string.Empty, out ProjectileRecordJson projectile);
                float authoredRange = record.range > 0f ? record.range : record.radius;
                float authoredProjectileSpeed = record.projectileSpeed > 0f
                    ? record.projectileSpeed
                    : projectile == null ? 0f : projectile.speed;
                float authoredProjectileLifetime = record.projectileLifetimeSeconds > 0f
                    ? record.projectileLifetimeSeconds
                    : projectile == null ? 0f : projectile.lifetimeSeconds;
                try
                {
                    definitions.Add(new SurvivorsWeaponArchetypeDefinition(
                        record.id,
                        record.displayName,
                        archetype,
                        allowFallbacks ? ResolvePositive(record.cooldownSeconds, fallback == null ? 1f : fallback.CooldownSeconds) : record.cooldownSeconds,
                        allowFallbacks ? ResolvePositive(record.damage, fallback == null ? 1f : fallback.Damage) : record.damage,
                        allowFallbacks ? ResolvePositive(authoredRange, fallback == null ? 1f : fallback.Range) : authoredRange,
                        ResolveColor(record.tint, allowFallbacks && fallback != null ? fallback.Tint : Color.white),
                        projectileSpeed: allowFallbacks ? ResolvePositive(authoredProjectileSpeed, fallback == null ? 0f : fallback.ProjectileSpeed) : authoredProjectileSpeed,
                        projectileRadius: allowFallbacks ? ResolvePositive(record.projectileRadius, fallback == null ? 0.08f : fallback.ProjectileRadius) : record.projectileRadius,
                        projectileLifetimeSeconds: allowFallbacks ? ResolvePositive(authoredProjectileLifetime, fallback == null ? 1f : fallback.ProjectileLifetimeSeconds) : authoredProjectileLifetime,
                        projectilePierceCount: allowFallbacks ? ResolveNonNegative(record.pierceCount, fallback == null ? 0 : fallback.ProjectilePierceCount) : record.pierceCount,
                        projectileChainCount: allowFallbacks ? ResolveNonNegative(record.chainCount, fallback == null ? 0 : fallback.ProjectileChainCount) : record.chainCount,
                        projectileForkCount: allowFallbacks ? ResolveNonNegative(record.forkCount, fallback == null ? 0 : fallback.ProjectileForkCount) : record.forkCount,
                        projectileReturnCount: allowFallbacks ? ResolveNonNegative(record.returnCount, fallback == null ? 0 : fallback.ProjectileReturnCount) : record.returnCount,
                        projectileFanCount: allowFallbacks ? ResolvePositive(record.fanCount, fallback == null ? 1 : fallback.ProjectileFanCount) : record.fanCount,
                        projectileSpreadDegrees: allowFallbacks ? ResolveNonNegative(record.spreadDegrees, fallback == null ? 0f : fallback.ProjectileSpreadDegrees) : record.spreadDegrees,
                        orbitCount: allowFallbacks ? ResolvePositive(record.orbitCount, fallback == null ? 1 : fallback.OrbitCount) : record.orbitCount,
                        orbitRadius: allowFallbacks ? ResolvePositive(record.orbitRadius, fallback == null ? 1f : fallback.OrbitRadius) : record.orbitRadius,
                        orbitDegreesPerSecond: allowFallbacks ? ResolvePositive(record.orbitDegreesPerSecond, fallback == null ? 1f : fallback.OrbitDegreesPerSecond) : record.orbitDegreesPerSecond,
                        orbitContactTickIntervalSeconds: allowFallbacks ? ResolvePositive(record.contactTickIntervalSeconds, fallback == null ? 0.3f : fallback.OrbitContactTickIntervalSeconds) : record.contactTickIntervalSeconds,
                        meleeHitCount: allowFallbacks ? ResolvePositive(record.hitCount, fallback == null ? 1 : fallback.MeleeHitCount) : record.hitCount,
                        meleeArcDegrees: allowFallbacks ? ResolvePositive(record.arcDegrees, fallback == null ? 115f : fallback.MeleeArcDegrees) : record.arcDegrees,
                        meleeVisualDurationSeconds: allowFallbacks ? ResolvePositive(record.visualDurationSeconds, fallback == null ? 0.16f : fallback.MeleeVisualDurationSeconds) : record.visualDurationSeconds,
                        burstCount: allowFallbacks ? ResolvePositive(record.burstCount, fallback == null ? 1 : fallback.BurstCount) : record.burstCount,
                        burstRepeatIntervalSeconds: allowFallbacks ? ResolvePositive(record.repeatIntervalSeconds, fallback == null ? 0.18f : fallback.BurstRepeatIntervalSeconds) : record.repeatIntervalSeconds,
                        burstVisualDurationSeconds: allowFallbacks ? ResolvePositive(record.visualDurationSeconds, fallback == null ? 0.22f : fallback.BurstVisualDurationSeconds) : record.visualDurationSeconds,
                        hitscanCount: allowFallbacks ? ResolvePositive(record.hitscanCount, fallback == null ? 1 : fallback.HitscanCount) : record.hitscanCount,
                        hitscanWidth: allowFallbacks ? ResolvePositive(record.beamWidth, fallback == null ? 0.18f : fallback.HitscanWidth) : record.beamWidth,
                        hitscanVisualDurationSeconds: allowFallbacks ? ResolvePositive(record.visualDurationSeconds, fallback == null ? 0.08f : fallback.HitscanVisualDurationSeconds) : record.visualDurationSeconds,
                        hitscanPierces: record.pierces,
                        payloadCount: allowFallbacks ? ResolvePositive(record.payloadCount, fallback == null ? 1 : fallback.PayloadCount) : record.payloadCount,
                        payloadTravelSpeed: allowFallbacks ? ResolvePositive(record.payloadTravelSpeed, fallback == null ? 9f : fallback.PayloadTravelSpeed) : record.payloadTravelSpeed,
                        payloadArmingSeconds: allowFallbacks ? ResolvePositive(record.payloadArmingSeconds, fallback == null ? 0.7f : fallback.PayloadArmingSeconds) : record.payloadArmingSeconds,
                        payloadLifetimeSeconds: allowFallbacks ? ResolvePositive(record.payloadLifetimeSeconds, fallback == null ? 4f : fallback.PayloadLifetimeSeconds) : record.payloadLifetimeSeconds,
                        payloadTriggerRadius: allowFallbacks ? ResolvePositive(record.payloadTriggerRadius, fallback == null ? 1.35f : fallback.PayloadTriggerRadius) : record.payloadTriggerRadius,
                        payloadExplosionRadius: allowFallbacks ? ResolvePositive(record.payloadExplosionRadius, fallback == null ? 2.4f : fallback.PayloadExplosionRadius) : record.payloadExplosionRadius,
                        payloadPlacementRadius: allowFallbacks ? ResolvePositive(record.payloadPlacementRadius, fallback == null ? 1.8f : fallback.PayloadPlacementRadius) : record.payloadPlacementRadius,
                        payloadAutoDetonateAtExpiry: allowFallbacks ? record.payloadAutoDetonateAtExpiry || (fallback != null && fallback.PayloadAutoDetonateAtExpiry) : record.payloadAutoDetonateAtExpiry,
                        payloadLeavesHazard: record.payloadLeavesHazard,
                        payloadHazardDurationSeconds: allowFallbacks ? ResolveNonNegative(record.payloadHazardDurationSeconds, fallback == null ? 0f : fallback.PayloadHazardDurationSeconds) : record.payloadHazardDurationSeconds,
                        payloadHazardTickIntervalSeconds: allowFallbacks ? ResolvePositive(record.payloadHazardTickIntervalSeconds, fallback == null ? 0.45f : fallback.PayloadHazardTickIntervalSeconds) : record.payloadHazardTickIntervalSeconds,
                        payloadHazardDamageRatio: allowFallbacks ? ResolveNonNegative(record.payloadHazardDamageRatio, fallback == null ? 0f : fallback.PayloadHazardDamageRatio) : record.payloadHazardDamageRatio));
                }
                catch (Exception ex)
                {
                    errors.Add($"Authored weapon {record.id} could not bind: {ex.Message}");
                }
            }

            return definitions.ToArray();
        }

        private static RunUpgradeCatalog CreateRunUpgradeCatalog(UpgradeLibraryJson library, List<string> errors, bool requireExplicitValues)
        {
            if (library == null || library.upgrades == null)
            {
                return null;
            }

            Dictionary<string, RunUpgradeDefinition> fallbackById = IndexRunUpgrades(BasicSurvivorsGame.CreateRunUpgradeCatalog());
            var definitions = new List<RunUpgradeDefinition>(library.upgrades.Length);
            for (int i = 0; i < library.upgrades.Length; i++)
            {
                UpgradeRecordJson record = library.upgrades[i];
                if (record == null || string.IsNullOrWhiteSpace(record.id))
                {
                    errors.Add($"Authored upgrade record at index {i} is missing an id.");
                    continue;
                }

                if (!Enum.TryParse(record.rarity, true, out RunUpgradeRarity rarity))
                {
                    errors.Add($"Authored upgrade {record.id} uses unknown rarity {record.rarity}.");
                    continue;
                }

                fallbackById.TryGetValue(record.id, out RunUpgradeDefinition fallback);
                if (requireExplicitValues)
                {
                    ValidateExplicitUpgradeValues(record, errors);
                }

                IReadOnlyList<RunUpgradeEffectDescriptor> effects = CreateUpgradeEffects(record, errors);
                if (effects.Count == 0)
                {
                    continue;
                }

                try
                {
                    definitions.Add(new RunUpgradeDefinition(
                        new RunUpgradeId(record.id),
                        rarity,
                        requireExplicitValues ? record.weight : ResolvePositive(record.weight, fallback == null ? 1 : fallback.Weight),
                        requireExplicitValues ? record.maxRank : ResolvePositive(record.maxRank, fallback == null ? 1 : fallback.MaxRank),
                        effects));
                }
                catch (Exception ex)
                {
                    errors.Add($"Authored upgrade {record.id} could not bind: {ex.Message}");
                }
            }

            if (definitions.Count == 0)
            {
                return null;
            }

            try
            {
                return new RunUpgradeCatalog(definitions);
            }
            catch (Exception ex)
            {
                errors.Add($"Authored upgrade catalog could not bind: {ex.Message}");
                return null;
            }
        }

        private static SurvivorsRunUpgradeMetadata[] CreateRunUpgradeMetadata(UpgradeLibraryJson library, List<string> errors)
        {
            if (library == null || library.upgrades == null)
            {
                return Array.Empty<SurvivorsRunUpgradeMetadata>();
            }

            var metadata = new List<SurvivorsRunUpgradeMetadata>(library.upgrades.Length);
            for (int i = 0; i < library.upgrades.Length; i++)
            {
                UpgradeRecordJson record = library.upgrades[i];
                if (record == null || string.IsNullOrWhiteSpace(record.id))
                {
                    continue;
                }

                if (!Enum.TryParse(record.category, true, out SurvivorsRunUpgradeCategory category))
                {
                    errors.Add($"Authored upgrade {record.id} uses unknown category {record.category}.");
                    continue;
                }

                string affectedContentId = !string.IsNullOrWhiteSpace(record.affectedContentId)
                    ? record.affectedContentId
                    : ResolveAffectedContentId(record.target);
                string requiredOwnedWeaponId = !string.IsNullOrWhiteSpace(record.requiredOwnedWeaponId)
                    ? record.requiredOwnedWeaponId
                    : ResolveRequiredOwnedWeaponId(category, affectedContentId);
                string requiredUpgradeId = record.requiredUpgradeId ?? string.Empty;
                int requiredUpgradeRank = record.requiredUpgradeRank;
                string requiredPassiveUpgradeId = record.requiredPassiveUpgradeId ?? string.Empty;

                metadata.Add(new SurvivorsRunUpgradeMetadata(
                    record.id,
                    record.displayName,
                    category,
                    ResolveSlotKind(category),
                    affectedContentId,
                    record.description,
                    requiredOwnedWeaponId,
                    requiredUpgradeId,
                    requiredUpgradeRank,
                    requiredPassiveUpgradeId));
            }

            return metadata.ToArray();
        }

        private static SurvivorsClassUpgradeGateDefinition[] CreateClassUpgradeGates(
            UpgradeLibraryJson library,
            IReadOnlyList<SurvivorsProgressionTrackDefinition> progressionTracks,
            bool includeLegacyUpgradeGates)
        {
            var allowedClassIdsByUpgradeId = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            if (progressionTracks != null)
            {
                for (int trackIndex = 0; trackIndex < progressionTracks.Count; trackIndex++)
                {
                    SurvivorsProgressionTrackDefinition track = progressionTracks[trackIndex];
                    if (track == null || !track.IsClassSpecific)
                    {
                        continue;
                    }

                    for (int nodeIndex = 0; nodeIndex < track.Nodes.Count; nodeIndex++)
                    {
                        SurvivorsProgressionNodeDefinition node = track.Nodes[nodeIndex];
                        if (node != null)
                        {
                            AddClassUpgradeGate(allowedClassIdsByUpgradeId, node.UpgradeId, track.ClassId);
                        }
                    }
                }
            }

            if (includeLegacyUpgradeGates && library != null && library.upgrades != null)
            {
                for (int i = 0; i < library.upgrades.Length; i++)
                {
                    UpgradeRecordJson record = library.upgrades[i];
                    if (record == null || string.IsNullOrWhiteSpace(record.id) || record.allowedClasses == null || record.allowedClasses.Length == 0)
                    {
                        continue;
                    }

                    for (int classIndex = 0; classIndex < record.allowedClasses.Length; classIndex++)
                    {
                        AddClassUpgradeGate(allowedClassIdsByUpgradeId, record.id, record.allowedClasses[classIndex]);
                    }
                }
            }

            var gates = new List<SurvivorsClassUpgradeGateDefinition>(allowedClassIdsByUpgradeId.Count);
            foreach (KeyValuePair<string, List<string>> pair in allowedClassIdsByUpgradeId)
            {
                if (!string.IsNullOrWhiteSpace(pair.Key) && pair.Value.Count > 0)
                {
                    gates.Add(new SurvivorsClassUpgradeGateDefinition(pair.Key, pair.Value));
                }
            }

            return gates.ToArray();
        }

        private static void AddClassUpgradeGate(Dictionary<string, List<string>> allowedClassIdsByUpgradeId, string upgradeId, string classId)
        {
            if (allowedClassIdsByUpgradeId == null || string.IsNullOrWhiteSpace(upgradeId) || string.IsNullOrWhiteSpace(classId))
            {
                return;
            }

            if (!allowedClassIdsByUpgradeId.TryGetValue(upgradeId, out List<string> classIds))
            {
                classIds = new List<string>();
                allowedClassIdsByUpgradeId.Add(upgradeId, classIds);
            }

            for (int i = 0; i < classIds.Count; i++)
            {
                if (string.Equals(classIds[i], classId, StringComparison.Ordinal))
                {
                    return;
                }
            }

            classIds.Add(classId);
        }

        private static SurvivorsRelicDefinition[] CreateRelicDefinitions(RelicLibraryJson library, List<string> errors)
        {
            if (library == null || library.relics == null)
            {
                return Array.Empty<SurvivorsRelicDefinition>();
            }

            var definitions = new List<SurvivorsRelicDefinition>(library.relics.Length);
            for (int i = 0; i < library.relics.Length; i++)
            {
                RelicRecordJson record = library.relics[i];
                if (record == null || string.IsNullOrWhiteSpace(record.id))
                {
                    errors.Add($"Authored relic record at index {i} is missing an id.");
                    continue;
                }

                if (!Enum.TryParse(record.effectKind, true, out SurvivorsRelicEffectKind effectKind))
                {
                    errors.Add($"Authored relic {record.id} uses unknown effect kind {record.effectKind}.");
                    continue;
                }

                definitions.Add(new SurvivorsRelicDefinition(
                    record.id,
                    record.displayName,
                    record.target,
                    record.effect,
                    effectKind,
                    record.amount,
                    record.weight));
            }

            return definitions.ToArray();
        }

        private static SurvivorsClassLibraryDefinition CreateClassLibraryDefinition(ClassLibraryJson library, List<string> errors)
        {
            if (library == null || library.classes == null)
            {
                return null;
            }

            var classes = new List<SurvivorsClassDefinition>(library.classes.Length);
            for (int i = 0; i < library.classes.Length; i++)
            {
                ClassRecordJson record = library.classes[i];
                if (record == null || string.IsNullOrWhiteSpace(record.id))
                {
                    errors.Add($"Authored class record at index {i} is missing an id.");
                    continue;
                }

                classes.Add(new SurvivorsClassDefinition(
                    record.id,
                    record.displayName,
                    record.startingWeaponId,
                    record.unlockedByDefault,
                    record.unlockRewardId,
                    CreateClassStatModifiers(record.statModifiers, record.id, errors),
                    record.startingWeaponIds));
            }

            return new SurvivorsClassLibraryDefinition(classes, library.defaultClassId);
        }

        private static SurvivorsProgressionTrackDefinition[] CreateProgressionTracks(ProgressionLibraryJson library, List<string> errors)
        {
            if (library == null || library.tracks == null)
            {
                return Array.Empty<SurvivorsProgressionTrackDefinition>();
            }

            var tracks = new List<SurvivorsProgressionTrackDefinition>(library.tracks.Length);
            for (int i = 0; i < library.tracks.Length; i++)
            {
                ProgressionTrackRecordJson record = library.tracks[i];
                if (record == null || string.IsNullOrWhiteSpace(record.id))
                {
                    errors.Add($"Authored progression track at index {i} is missing an id.");
                    continue;
                }

                if (!Enum.TryParse(record.kind, true, out SurvivorsProgressionTrackKind kind))
                {
                    errors.Add($"Authored progression track {record.id} uses unknown kind {record.kind}.");
                    continue;
                }

                tracks.Add(new SurvivorsProgressionTrackDefinition(
                    record.id,
                    record.displayName,
                    kind,
                    record.classId,
                    record.targetWeaponId,
                    CreateProgressionNodes(record, errors)));
            }

            return tracks.ToArray();
        }

        private static SurvivorsProgressionNodeDefinition[] CreateProgressionNodes(ProgressionTrackRecordJson track, List<string> errors)
        {
            if (track.nodes == null)
            {
                return Array.Empty<SurvivorsProgressionNodeDefinition>();
            }

            var nodes = new List<SurvivorsProgressionNodeDefinition>(track.nodes.Length);
            for (int i = 0; i < track.nodes.Length; i++)
            {
                ProgressionNodeRecordJson record = track.nodes[i];
                if (record == null || string.IsNullOrWhiteSpace(record.id))
                {
                    errors.Add($"Authored progression track {track.id} node at index {i} is missing an id.");
                    continue;
                }

                if (!Enum.TryParse(record.kind, true, out SurvivorsProgressionNodeKind kind))
                {
                    errors.Add($"Authored progression node {record.id} uses unknown kind {record.kind}.");
                    continue;
                }

                nodes.Add(new SurvivorsProgressionNodeDefinition(
                    record.id,
                    record.displayName,
                    record.upgradeId,
                    kind,
                    record.tier,
                    record.pointCost,
                    record.maxRank));
            }

            return nodes.ToArray();
        }

        private static Dictionary<string, ProjectileRecordJson> IndexProjectiles(ProjectileRecordJson[] projectiles, List<string> errors)
        {
            var index = new Dictionary<string, ProjectileRecordJson>(StringComparer.Ordinal);
            if (projectiles == null)
            {
                return index;
            }

            for (int i = 0; i < projectiles.Length; i++)
            {
                ProjectileRecordJson projectile = projectiles[i];
                if (projectile == null || string.IsNullOrWhiteSpace(projectile.id))
                {
                    errors.Add($"Authored projectile record at index {i} is missing an id.");
                    continue;
                }

                if (index.ContainsKey(projectile.id))
                {
                    errors.Add($"Authored projectile content has duplicate id {projectile.id}.");
                    continue;
                }

                index.Add(projectile.id, projectile);
            }

            return index;
        }

        private static Dictionary<string, SurvivorsWeaponArchetypeDefinition> IndexWeapons(IReadOnlyList<SurvivorsWeaponArchetypeDefinition> weapons)
        {
            var index = new Dictionary<string, SurvivorsWeaponArchetypeDefinition>(StringComparer.Ordinal);
            if (weapons == null)
            {
                return index;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                SurvivorsWeaponArchetypeDefinition weapon = weapons[i];
                if (weapon != null && !string.IsNullOrWhiteSpace(weapon.Id) && !index.ContainsKey(weapon.Id))
                {
                    index.Add(weapon.Id, weapon);
                }
            }

            return index;
        }

        private static Dictionary<string, RunUpgradeDefinition> IndexRunUpgrades(RunUpgradeCatalog catalog)
        {
            var index = new Dictionary<string, RunUpgradeDefinition>(StringComparer.Ordinal);
            if (catalog == null)
            {
                return index;
            }

            for (int i = 0; i < catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = catalog.Definitions[i];
                if (definition != null && !definition.Id.IsEmpty && !index.ContainsKey(definition.Id.Value))
                {
                    index.Add(definition.Id.Value, definition);
                }
            }

            return index;
        }

        private static Dictionary<string, SurvivorsRunUpgradeMetadata> IndexRunUpgradeMetadata(IReadOnlyList<SurvivorsRunUpgradeMetadata> metadata)
        {
            var index = new Dictionary<string, SurvivorsRunUpgradeMetadata>(StringComparer.Ordinal);
            if (metadata == null)
            {
                return index;
            }

            for (int i = 0; i < metadata.Count; i++)
            {
                SurvivorsRunUpgradeMetadata entry = metadata[i];
                if (entry != null && !string.IsNullOrWhiteSpace(entry.UpgradeId) && !index.ContainsKey(entry.UpgradeId))
                {
                    index.Add(entry.UpgradeId, entry);
                }
            }

            return index;
        }

        private static void ValidateExplicitUpgradeValues(UpgradeRecordJson record, List<string> errors)
        {
            if (record == null || errors == null)
            {
                return;
            }

            if (record.weight <= 0d)
            {
                errors.Add($"Authored upgrade {record.id} must define weight above zero.");
            }

            if (record.maxRank <= 0)
            {
                errors.Add($"Authored upgrade {record.id} must define max rank above zero.");
            }

            if (record.effects != null && record.effects.Length > 0)
            {
                for (int i = 0; i < record.effects.Length; i++)
                {
                    UpgradeEffectRecordJson effect = record.effects[i];
                    if (effect == null || Math.Abs(effect.amount) <= double.Epsilon)
                    {
                        errors.Add($"Authored upgrade {record.id} effect at index {i} must define a non-zero amount.");
                    }
                }

                return;
            }

            if (Math.Abs(record.amount) <= double.Epsilon)
            {
                errors.Add($"Authored upgrade {record.id} must define a non-zero amount.");
            }
        }

        private static IReadOnlyList<RunUpgradeEffectDescriptor> CreateUpgradeEffects(UpgradeRecordJson record, List<string> errors)
        {
            if (record.effects != null && record.effects.Length > 0)
            {
                var authoredEffects = new List<RunUpgradeEffectDescriptor>(record.effects.Length);
                for (int i = 0; i < record.effects.Length; i++)
                {
                    UpgradeEffectRecordJson effect = record.effects[i];
                    if (effect == null || string.IsNullOrWhiteSpace(effect.effect) || string.IsNullOrWhiteSpace(effect.target))
                    {
                        errors.Add($"Authored upgrade {record.id} effect at index {i} is missing an effect or target.");
                        continue;
                    }

                    authoredEffects.Add(new RunUpgradeEffectDescriptor(
                        new RunUpgradeEffectId(effect.effect),
                        new RunUpgradeTargetId(effect.target),
                        effect.amount));
                }

                return authoredEffects;
            }

            if (!string.IsNullOrWhiteSpace(record.effect) && !string.IsNullOrWhiteSpace(record.target))
            {
                return new[]
                {
                    new RunUpgradeEffectDescriptor(
                        new RunUpgradeEffectId(record.effect),
                        new RunUpgradeTargetId(record.target),
                        record.amount)
                };
            }

            errors.Add($"Authored upgrade {record.id} is missing an effect or target.");
            return Array.Empty<RunUpgradeEffectDescriptor>();
        }

        private static SurvivorsClassStatModifierDefinition[] CreateClassStatModifiers(StatModifierRecordJson[] records, string classId, List<string> errors)
        {
            if (records == null)
            {
                return Array.Empty<SurvivorsClassStatModifierDefinition>();
            }

            var modifiers = new List<SurvivorsClassStatModifierDefinition>(records.Length);
            for (int i = 0; i < records.Length; i++)
            {
                StatModifierRecordJson record = records[i];
                if (record == null)
                {
                    errors.Add($"Authored class {classId} stat modifier at index {i} is null.");
                    continue;
                }

                if (!Enum.TryParse(record.stat, true, out SurvivorsClassStatKind kind))
                {
                    errors.Add($"Authored class {classId} stat modifier uses unknown stat {record.stat}.");
                    continue;
                }

                modifiers.Add(new SurvivorsClassStatModifierDefinition(kind, record.amount));
            }

            return modifiers.ToArray();
        }

        private static SurvivorsRunBuildSlotKind ResolveSlotKind(SurvivorsRunUpgradeCategory category)
        {
            switch (category)
            {
                case SurvivorsRunUpgradeCategory.Weapon:
                    return SurvivorsRunBuildSlotKind.Weapon;
                case SurvivorsRunUpgradeCategory.Passive:
                case SurvivorsRunUpgradeCategory.PassiveUpgrade:
                    return SurvivorsRunBuildSlotKind.Passive;
                default:
                    return SurvivorsRunBuildSlotKind.None;
            }
        }

        private static string ResolveAffectedContentId(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return string.Empty;
            }

            const string weaponPrefix = "survivors.weapon.";
            if (target.StartsWith(weaponPrefix, StringComparison.Ordinal))
            {
                string suffix = target.Substring(weaponPrefix.Length);
                if (!string.Equals(suffix, "payloads", StringComparison.Ordinal))
                {
                    return "weapon.survivors." + suffix;
                }
            }

            return target;
        }

        private static string ResolveRequiredOwnedWeaponId(SurvivorsRunUpgradeCategory category, string affectedContentId)
        {
            if (category == SurvivorsRunUpgradeCategory.WeaponUpgrade ||
                category == SurvivorsRunUpgradeCategory.Mutation ||
                category == SurvivorsRunUpgradeCategory.Evolution)
            {
                return affectedContentId;
            }

            return string.Empty;
        }

        private static Color ResolveColor(string value, Color fallback)
        {
            return !string.IsNullOrWhiteSpace(value) && ColorUtility.TryParseHtmlString(value, out Color parsed)
                ? parsed
                : fallback;
        }

        private static float ResolvePositive(float value, float fallback)
        {
            return value > 0f ? value : fallback;
        }

        private static int ResolvePositive(int value, int fallback)
        {
            return value > 0 ? value : fallback;
        }

        private static double ResolvePositive(double value, double fallback)
        {
            return value > 0d ? value : fallback;
        }

        private static float ResolveNonNegative(float value, float fallback)
        {
            return value >= 0f ? value : fallback;
        }

        private static int ResolveNonNegative(int value, int fallback)
        {
            return value >= 0 ? value : fallback;
        }

        private static string BuildSourceSummary(
            SurvivorsWeaponArchetypeDefinition[] weaponDefinitions,
            RunUpgradeCatalog runUpgradeCatalog,
            SurvivorsRunUpgradeMetadata[] runUpgradeMetadata,
            SurvivorsRelicDefinition[] relicDefinitions,
            SurvivorsClassLibraryDefinition classLibrary,
            SurvivorsProgressionTrackDefinition[] progressionTracks,
            SurvivorsEnemyProfile[] enemyProfiles,
            Dictionary<string, RunFlowProfileRecordJson> runFlowProfiles,
            SurvivorsMetaProgressionDefinition metaProgression)
        {
            int upgradeCount = runUpgradeCatalog == null ? 0 : runUpgradeCatalog.Definitions.Count;
            int classCount = classLibrary == null ? 0 : classLibrary.Classes.Count;
            int rewardCount = metaProgression == null ? 0 : metaProgression.Rewards.Count;
            return $"{weaponDefinitions.Length} weapons, {upgradeCount} upgrades, {runUpgradeMetadata.Length} draft cards, {relicDefinitions.Length} relics, {classCount} classes, {progressionTracks.Length} progression tracks, {enemyProfiles.Length} enemies, {runFlowProfiles.Count} run-flow profiles, {rewardCount} rewards";
        }

        private static void RequireWeaponDefinitions(SurvivorsWeaponArchetypeDefinition[] definitions, List<string> errors)
        {
            if (definitions == null || definitions.Length == 0)
            {
                errors.Add("Authored weapon content is missing weapon definitions.");
            }
        }

        private static void RequireRunUpgradeCatalog(RunUpgradeCatalog catalog, List<string> errors)
        {
            if (catalog == null || catalog.Definitions.Count == 0)
            {
                errors.Add("Authored upgrade content is missing upgrade definitions.");
            }
        }

        private static void RequireRunUpgradeMetadata(SurvivorsRunUpgradeMetadata[] metadata, List<string> errors)
        {
            if (metadata == null || metadata.Length == 0)
            {
                errors.Add("Authored upgrade content is missing draft card metadata.");
            }
        }

        private static void RequireRelicDefinitions(SurvivorsRelicDefinition[] definitions, List<string> errors)
        {
            if (definitions == null || definitions.Length == 0)
            {
                errors.Add("Authored relic content is missing relic definitions.");
            }
        }

        private static void RequireClassLibrary(SurvivorsClassLibraryDefinition classLibrary, List<string> errors)
        {
            if (classLibrary == null || classLibrary.Classes.Count == 0)
            {
                errors.Add("Authored class content is missing class definitions.");
            }
        }

        private static void RequireProgressionTracks(SurvivorsProgressionTrackDefinition[] tracks, List<string> errors)
        {
            if (tracks == null || tracks.Length == 0)
            {
                errors.Add("Authored progression content is missing progression tracks.");
            }
        }

        private static T ParseJson<T>(string json, string label, List<string> errors, bool required) where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                if (required)
                {
                    errors.Add($"Authored {label} JSON is required.");
                }

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

        private static SurvivorsEnemyProfile[] CreateEnemyProfiles(
            EnemyLibraryJson library,
            List<string> errors,
            bool allowFallbacks)
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

                SurvivorsEnemyProfile fallback = allowFallbacks
                    ? BasicSurvivorsGame.CreateEnemyProfile(role)
                    : default;
                profiles.Add(new SurvivorsEnemyProfile(
                    role,
                    allowFallbacks && string.IsNullOrWhiteSpace(record.id) ? fallback.Id : record.id,
                    allowFallbacks && string.IsNullOrWhiteSpace(record.displayName) ? fallback.DisplayName : record.displayName,
                    allowFallbacks && record.health <= 0f ? fallback.MaxHealth : record.health,
                    allowFallbacks && record.moveSpeed <= 0f ? fallback.MoveSpeed : record.moveSpeed,
                    allowFallbacks && record.radius <= 0f ? fallback.Radius : record.radius,
                    allowFallbacks && record.contactDamage < 0f ? fallback.ContactDamage : record.contactDamage,
                    allowFallbacks && record.contactIntervalSeconds <= 0f ? fallback.ContactIntervalSeconds : record.contactIntervalSeconds,
                    allowFallbacks && record.experienceDrop <= 0 ? fallback.ExperienceReward : record.experienceDrop,
                    ResolveColor(record.tint, allowFallbacks ? fallback.Tint : Color.white),
                    allowFallbacks && record.rangedAttackRange <= 0f ? fallback.RangedAttackRange : record.rangedAttackRange,
                    allowFallbacks && record.rangedAttackDamage <= 0f ? fallback.RangedAttackDamage : record.rangedAttackDamage,
                    allowFallbacks && record.rangedAttackIntervalSeconds <= 0f ? fallback.RangedAttackIntervalSeconds : record.rangedAttackIntervalSeconds,
                    allowFallbacks && record.preferredRange <= 0f ? fallback.PreferredRange : record.preferredRange,
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

        private static SurvivorsMetaProgressionDefinition CreateMetaProgressionDefinition(
            RewardLibraryJson library,
            List<string> errors,
            bool allowFallbacks)
        {
            if (library == null)
            {
                return null;
            }

            CurrencyId currencyId = allowFallbacks ? BasicSurvivorsGame.BloodShardsCurrencyId : default;
            string currencyDisplayName = "Blood Shards";
            if (library.currencies != null)
            {
                for (int i = 0; i < library.currencies.Length; i++)
                {
                    CurrencyRecordJson currency = library.currencies[i];
                    if (currency != null && !string.IsNullOrWhiteSpace(currency.id))
                    {
                        currencyId = new CurrencyId(currency.id);
                        currencyDisplayName = string.IsNullOrWhiteSpace(currency.displayName) ? currencyDisplayName : currency.displayName;
                        break;
                    }
                }
            }

            TrackId trackId = allowFallbacks ? BasicSurvivorsGame.LegacyExperienceTrackId : default;
            string trackDisplayName = "Legacy XP";
            if (library.tracks != null)
            {
                for (int i = 0; i < library.tracks.Length; i++)
                {
                    TrackRecordJson track = library.tracks[i];
                    if (track != null && !string.IsNullOrWhiteSpace(track.id))
                    {
                        trackId = new TrackId(track.id);
                        trackDisplayName = string.IsNullOrWhiteSpace(track.displayName) ? trackDisplayName : track.displayName;
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
                        allowFallbacks && string.IsNullOrWhiteSpace(reward.currencyId) ? currencyId : new CurrencyId(reward.currencyId),
                        reward.currencyAmount,
                        allowFallbacks && string.IsNullOrWhiteSpace(reward.trackId) ? trackId : new TrackId(reward.trackId),
                        reward.trackAmount));
                }
            }

            return new SurvivorsMetaProgressionDefinition(currencyId, trackId, persistentUpgrades, rewards, currencyDisplayName, trackDisplayName);
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

        private static void ApplyGameplayTuning(
            SurvivorsTemplateTuning tuning,
            SurvivorsGameplayTuningRecordJson authored,
            bool applyOnlyAuthoredOverrides)
        {
            if (tuning == null || authored == null)
            {
                return;
            }

            ApplyAuthoredValue(ref tuning.PlayerMoveSpeed, authored.playerMoveSpeed, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.PlayerRadius, authored.playerRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.PlayerMaxHealth, authored.playerMaxHealth, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.PlayerContactInvulnerabilitySeconds, authored.playerContactInvulnerabilitySeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.DashDistance, authored.dashDistance, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.DashCooldownSeconds, authored.dashCooldownSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.DashInvulnerabilitySeconds, authored.dashInvulnerabilitySeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.DashKnockbackRadius, authored.dashKnockbackRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.DashKnockbackDistance, authored.dashKnockbackDistance, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.DashDamage, authored.dashDamage, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EnemySpawnRadius, authored.enemySpawnRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EnemySpawnPackIncreaseEveryEscalations, authored.enemySpawnPackIncreaseEveryEscalations, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EnemySeparationRadius, authored.enemySeparationRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EnemySeparationStrength, authored.enemySeparationStrength, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EnemySeparationMaxNeighbors, authored.enemySeparationMaxNeighbors, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EnemyRangedAttackWindupSeconds, authored.enemyRangedAttackWindupSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.MajorThreatWarningLeadSeconds, authored.majorThreatWarningLeadSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.HordeRushClearSurgeDurationSeconds, authored.hordeRushClearSurgeDurationSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.HordeRushClearSurgeDamageBonus, authored.hordeRushClearSurgeDamageBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.HordeRushClearSurgeMoveSpeedBonus, authored.hordeRushClearSurgeMoveSpeedBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.HordeRushClearSurgeCooldownMultiplierBonus, authored.hordeRushClearSurgeCooldownMultiplierBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.HordeRushClearSurgePickupRangeBonus, authored.hordeRushClearSurgePickupRangeBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.MajorThreatEnrageHealthThreshold, authored.majorThreatEnrageHealthThreshold, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.MajorThreatEnrageEliteSupportCount, authored.majorThreatEnrageEliteSupportCount, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.MajorThreatEnrageMinibossSupportCount, authored.majorThreatEnrageMinibossSupportCount, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.MajorThreatEnrageBossSupportCount, authored.majorThreatEnrageBossSupportCount, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.MajorThreatEnrageExtraAliveAllowance, authored.majorThreatEnrageExtraAliveAllowance, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.MajorThreatEnrageSupportRadius, authored.majorThreatEnrageSupportRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.MajorThreatSlamIntervalSeconds, authored.majorThreatSlamIntervalSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.MajorThreatSlamTelegraphSeconds, authored.majorThreatSlamTelegraphSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.MajorThreatSlamRadius, authored.majorThreatSlamRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.MajorThreatSlamDamage, authored.majorThreatSlamDamage, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.OrbitKnockbackDistance, authored.orbitKnockbackDistance, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.CrimsonAegisOrbitKnockbackDistance, authored.crimsonAegisOrbitKnockbackDistance, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.MagnetRecallSpeedMultiplier, authored.magnetRecallSpeedMultiplier, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.GemRushDurationSeconds, authored.gemRushDurationSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.GemRushDamageBonus, authored.gemRushDamageBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.GemRushMoveSpeedBonus, authored.gemRushMoveSpeedBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.GemRushCooldownMultiplierBonus, authored.gemRushCooldownMultiplierBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.GemRushPickupRangeBonus, authored.gemRushPickupRangeBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.MajorRewardCacheAttractionSpeedMultiplier, authored.majorRewardCacheAttractionSpeedMultiplier, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneDiscoveryRadius, authored.waystoneDiscoveryRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneExperienceGemCount, authored.waystoneExperienceGemCount, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneBloodShardInterval, authored.waystoneBloodShardInterval, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneAmbushInterval, authored.waystoneAmbushInterval, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneAmbushBaseEnemyCount, authored.waystoneAmbushBaseEnemyCount, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneAmbushExtraAliveAllowance, authored.waystoneAmbushExtraAliveAllowance, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneAmbushRadius, authored.waystoneAmbushRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneFocusDurationSeconds, authored.waystoneFocusDurationSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneFocusDamageBonus, authored.waystoneFocusDamageBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneFocusMoveSpeedBonus, authored.waystoneFocusMoveSpeedBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneFocusCooldownMultiplierBonus, authored.waystoneFocusCooldownMultiplierBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneFocusPickupRangeBonus, authored.waystoneFocusPickupRangeBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneChainInterval, authored.waystoneChainInterval, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneChainBonusGemCount, authored.waystoneChainBonusGemCount, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneChainDurationSeconds, authored.waystoneChainDurationSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneChainDamageBonus, authored.waystoneChainDamageBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneChainMoveSpeedBonus, authored.waystoneChainMoveSpeedBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneChainCooldownMultiplierBonus, authored.waystoneChainCooldownMultiplierBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneChainPickupRangeBonus, authored.waystoneChainPickupRangeBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneChainPulseDamage, authored.waystoneChainPulseDamage, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WaystoneChainPulseRadius, authored.waystoneChainPulseRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.HealthPickupHealAmount, authored.healthPickupHealAmount, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.LowHealthClutchPulseDamage, authored.lowHealthClutchPulseDamage, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.LowHealthClutchPulseRadius, authored.lowHealthClutchPulseRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.LowHealthClutchSafetySeconds, authored.lowHealthClutchSafetySeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.BloodShardPickupAmount, authored.bloodShardPickupAmount, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WeaponLoadoutSurgeDurationSeconds, authored.weaponLoadoutSurgeDurationSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WeaponLoadoutSurgeDamageBonus, authored.weaponLoadoutSurgeDamageBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WeaponLoadoutSurgeMoveSpeedBonus, authored.weaponLoadoutSurgeMoveSpeedBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WeaponLoadoutSurgeCooldownMultiplierBonus, authored.weaponLoadoutSurgeCooldownMultiplierBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WeaponLoadoutSurgePickupRangeBonus, authored.weaponLoadoutSurgePickupRangeBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WeaponLoadoutSurgePulseDamage, authored.weaponLoadoutSurgePulseDamage, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.WeaponLoadoutSurgePulseRadius, authored.weaponLoadoutSurgePulseRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.PassiveLoadoutSurgeDurationSeconds, authored.passiveLoadoutSurgeDurationSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.PassiveLoadoutSurgeDamageBonus, authored.passiveLoadoutSurgeDamageBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.PassiveLoadoutSurgeMoveSpeedBonus, authored.passiveLoadoutSurgeMoveSpeedBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.PassiveLoadoutSurgeCooldownMultiplierBonus, authored.passiveLoadoutSurgeCooldownMultiplierBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.PassiveLoadoutSurgePickupRangeBonus, authored.passiveLoadoutSurgePickupRangeBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.PassiveLoadoutSurgeExperienceGainMultiplierBonus, authored.passiveLoadoutSurgeExperienceGainMultiplierBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.PassiveLoadoutSurgePulseDamage, authored.passiveLoadoutSurgePulseDamage, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.PassiveLoadoutSurgePulseRadius, authored.passiveLoadoutSurgePulseRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.StatusPoisonDurationSeconds, authored.statusPoisonDurationSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.StatusBleedDurationSeconds, authored.statusBleedDurationSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.StatusBurnDurationSeconds, authored.statusBurnDurationSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EvolutionSurgeDamage, authored.evolutionSurgeDamage, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EvolutionSurgeRadius, authored.evolutionSurgeRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EvolutionChainSurgeMinimumEvolutions, authored.evolutionChainSurgeMinimumEvolutions, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EvolutionChainSurgeDurationSeconds, authored.evolutionChainSurgeDurationSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EvolutionChainSurgeDamageBonus, authored.evolutionChainSurgeDamageBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EvolutionChainSurgeMoveSpeedBonus, authored.evolutionChainSurgeMoveSpeedBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EvolutionChainSurgeCooldownMultiplierBonus, authored.evolutionChainSurgeCooldownMultiplierBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EvolutionChainSurgePickupRangeBonus, authored.evolutionChainSurgePickupRangeBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EvolutionChainSurgePulseDamage, authored.evolutionChainSurgePulseDamage, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.EvolutionChainSurgePulseRadius, authored.evolutionChainSurgePulseRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.LevelUpPulseDamage, authored.levelUpPulseDamage, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.LevelUpPulseRadius, authored.levelUpPulseRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.RewardUpgradeSurgeDamage, authored.rewardUpgradeSurgeDamage, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.RewardUpgradeSurgeRadius, authored.rewardUpgradeSurgeRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.RewardJackpotExperienceGemBaseCount, authored.rewardJackpotExperienceGemBaseCount, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.RewardJackpotExperienceGemPerRarityTier, authored.rewardJackpotExperienceGemPerRarityTier, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.RewardJackpotBloodShardBaseAmount, authored.rewardJackpotBloodShardBaseAmount, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.RewardJackpotLegendaryExtraBloodShardAmount, authored.rewardJackpotLegendaryExtraBloodShardAmount, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.BossRelicSurgeDamage, authored.bossRelicSurgeDamage, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.BossRelicSurgeRadius, authored.bossRelicSurgeRadius, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.BossRelicSurgeDurationSeconds, authored.bossRelicSurgeDurationSeconds, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.BossRelicSurgeDamageBonus, authored.bossRelicSurgeDamageBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.BossRelicSurgeMoveSpeedBonus, authored.bossRelicSurgeMoveSpeedBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.BossRelicSurgeCooldownMultiplierBonus, authored.bossRelicSurgeCooldownMultiplierBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.BossRelicSurgePickupRangeBonus, authored.bossRelicSurgePickupRangeBonus, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.StartingBarrierCapacity, authored.startingBarrierCapacity, applyOnlyAuthoredOverrides);
            ApplyAuthoredValue(ref tuning.BaseBarrierRegenPerSecond, authored.baseBarrierRegenPerSecond, applyOnlyAuthoredOverrides);
        }

        private static void ApplyAuthoredValue(ref float target, float authored, bool applyOnlyAuthoredOverrides)
        {
            if (!applyOnlyAuthoredOverrides || !Mathf.Approximately(authored, 0f))
            {
                target = authored;
            }
        }

        private static void ApplyAuthoredValue(ref int target, int authored, bool applyOnlyAuthoredOverrides)
        {
            if (!applyOnlyAuthoredOverrides || authored != 0)
            {
                target = authored;
            }
        }

        private static void ApplyRunFlowProfile(SurvivorsTemplateTuning tuning, RunFlowProfileRecordJson profile)
        {
            if (!string.IsNullOrWhiteSpace(profile.displayName))
            {
                tuning.RunModeDisplayName = profile.displayName;
            }

            if (!string.IsNullOrWhiteSpace(profile.description))
            {
                tuning.RunModeDescription = profile.description;
            }

            tuning.TargetDurationSeconds = profile.targetDurationSeconds;
            tuning.RunModeDurationLabel = FormatDurationLabel(profile.targetDurationSeconds);
            tuning.EndlessContinuationEnabled = profile.endlessContinuationEnabled;
            tuning.RunRewardMultiplier = profile.runRewardMultiplier;
            tuning.EvolutionRequiredRankReduction = profile.evolutionRequiredRankReduction;
            tuning.EnemySpawnIntervalSeconds = profile.enemySpawnIntervalSeconds;
            tuning.EnemyMaximumAlive = profile.enemyMaximumAlive;
            tuning.EnemySpawnPackBaseCount = profile.enemySpawnPackBaseCount;
            tuning.EnemySpawnPackMaxCount = profile.enemySpawnPackMaxCount;
            tuning.EnemyRangedAttackDodgeExperienceReward = profile.enemyRangedAttackDodgeExperienceReward;
            tuning.EnemyMaxHealth = profile.enemyMaxHealth;
            tuning.EnemyMoveSpeed = profile.enemyMoveSpeed;
            tuning.EnemyRadius = profile.enemyRadius;
            tuning.EnemyContactDamage = profile.enemyContactDamage;
            tuning.EnemyContactIntervalSeconds = profile.enemyContactIntervalSeconds;
            tuning.EnemyExperienceReward = profile.enemyExperienceReward;
            tuning.SummonerSupportInitialDelaySeconds = profile.summonerSupportInitialDelaySeconds;
            tuning.SummonerSupportIntervalSeconds = profile.summonerSupportIntervalSeconds;
            tuning.SummonerSupportCount = profile.summonerSupportCount;
            tuning.SummonerSupportExtraAliveAllowance = profile.summonerSupportExtraAliveAllowance;
            tuning.SummonerSupportRadius = profile.summonerSupportRadius;
            tuning.SplitterChildCount = profile.splitterChildCount;
            tuning.SplitterChildSpawnRadius = profile.splitterChildSpawnRadius;
            tuning.EnemySoftLeashRadius = profile.enemySoftLeashRadius;
            tuning.EnemyHardRecycleRadius = profile.enemyHardRecycleRadius;
            tuning.EnemyRecycleDelaySeconds = profile.enemyRecycleDelaySeconds;
            tuning.EnemyRecycleMinimumRespawnDistance = profile.enemyRecycleMinimumRespawnDistance;
            tuning.EnemyRecycleMaximumRespawnDistance = profile.enemyRecycleMaximumRespawnDistance;
            tuning.MajorThreatCatchUpRadius = profile.majorThreatCatchUpRadius;
            tuning.MajorThreatCatchUpSpeedMultiplier = profile.majorThreatCatchUpSpeedMultiplier;
            tuning.MajorThreatRepositionRadius = profile.majorThreatRepositionRadius;
            tuning.MajorThreatRepositionDelaySeconds = profile.majorThreatRepositionDelaySeconds;
            tuning.OffscreenSpawnPadding = profile.offscreenSpawnPadding;
            tuning.SpawnBandDepth = profile.spawnBandDepth;
            tuning.MajorThreatOffscreenSpawnPadding = profile.majorThreatOffscreenSpawnPadding;
            tuning.RecycledEnemyOffscreenSpawnPadding = profile.recycledEnemyOffscreenSpawnPadding;
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
            tuning.MinibossMaxHealth = profile.minibossMaxHealth;
            tuning.MinibossMoveSpeed = profile.minibossMoveSpeed;
            tuning.MinibossRadius = profile.minibossRadius;
            tuning.MinibossContactDamage = profile.minibossContactDamage;
            tuning.MinibossContactIntervalSeconds = profile.minibossContactIntervalSeconds;
            tuning.MinibossExperienceReward = profile.minibossExperienceReward;
            tuning.BossSpawnTimeSeconds = profile.bossSpawnTimeSeconds;
            tuning.BossMaxHealth = profile.bossMaxHealth;
            tuning.BossMoveSpeed = profile.bossMoveSpeed;
            tuning.BossRadius = profile.bossRadius;
            tuning.BossContactDamage = profile.bossContactDamage;
            tuning.BossContactIntervalSeconds = profile.bossContactIntervalSeconds;
            tuning.BossExperienceReward = profile.bossExperienceReward;
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
            tuning.PickupAttractRange = profile.pickupAttractRange;
            tuning.PickupAttractionSpeed = profile.pickupAttractionSpeed;
            tuning.PickupCollectRadius = profile.pickupCollectRadius;
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

        private static string FormatDurationLabel(float seconds)
        {
            if (seconds <= 0f)
            {
                return string.Empty;
            }

            float minutes = seconds / 60f;
            if (Mathf.Approximately(minutes, Mathf.Round(minutes)))
            {
                return $"{Mathf.RoundToInt(minutes)} min";
            }

            return $"{minutes:0.#} min";
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
        private sealed class WeaponLibraryJson
        {
            public WeaponRecordJson[] weapons;
            public ProjectileRecordJson[] projectiles;
        }

        [Serializable]
        private sealed class WeaponRecordJson
        {
            public string id;
            public string displayName;
            public string fireMode;
            public string projectileId;
            public string tint;
            public float cooldownSeconds;
            public float damage;
            public float range;
            public float radius;
            public float projectileSpeed;
            public float projectileRadius;
            public float projectileLifetimeSeconds;
            public int pierceCount;
            public int chainCount;
            public int forkCount;
            public int returnCount;
            public int fanCount;
            public float spreadDegrees;
            public int orbitCount;
            public float orbitRadius;
            public float orbitDegreesPerSecond;
            public float contactTickIntervalSeconds;
            public int hitCount;
            public float arcDegrees;
            public float visualDurationSeconds;
            public int burstCount;
            public float repeatIntervalSeconds;
            public int hitscanCount;
            public float beamWidth;
            public bool pierces;
            public int payloadCount;
            public float payloadTravelSpeed;
            public float payloadArmingSeconds;
            public float payloadLifetimeSeconds;
            public float payloadTriggerRadius;
            public float payloadExplosionRadius;
            public float payloadPlacementRadius;
            public bool payloadAutoDetonateAtExpiry;
            public bool payloadLeavesHazard;
            public float payloadHazardDurationSeconds;
            public float payloadHazardTickIntervalSeconds;
            public float payloadHazardDamageRatio;
        }

        [Serializable]
        private sealed class ProjectileRecordJson
        {
            public string id;
            public float speed;
            public float lifetimeSeconds;
        }

        [Serializable]
        private sealed class UpgradeLibraryJson
        {
            public UpgradeRecordJson[] upgrades;
        }

        [Serializable]
        private sealed class UpgradeRecordJson
        {
            public string id;
            public string displayName;
            public string category;
            public string rarity;
            public int weight;
            public int maxRank;
            public string effect;
            public string target;
            public double amount;
            public UpgradeEffectRecordJson[] effects;
            public string description;
            public string affectedContentId;
            public string requiredOwnedWeaponId;
            public string requiredUpgradeId;
            public int requiredUpgradeRank;
            public string requiredPassiveUpgradeId;
            public string[] allowedClasses;
        }

        [Serializable]
        private sealed class UpgradeEffectRecordJson
        {
            public string effect;
            public string target;
            public double amount;
        }

        [Serializable]
        private sealed class RelicLibraryJson
        {
            public RelicRecordJson[] relics;
        }

        [Serializable]
        private sealed class RelicRecordJson
        {
            public string id;
            public string displayName;
            public string target;
            public string effect;
            public string effectKind;
            public float amount;
            public int weight;
        }

        [Serializable]
        private sealed class ClassLibraryJson
        {
            public string defaultClassId;
            public ClassRecordJson[] classes;
        }

        [Serializable]
        private sealed class ClassRecordJson
        {
            public string id;
            public string displayName;
            public string startingWeaponId;
            public string[] startingWeaponIds;
            public bool unlockedByDefault;
            public string unlockRewardId;
            public StatModifierRecordJson[] statModifiers;
        }

        [Serializable]
        private sealed class StatModifierRecordJson
        {
            public string stat;
            public float amount;
        }

        [Serializable]
        private sealed class ProgressionLibraryJson
        {
            public ProgressionTrackRecordJson[] tracks;
        }

        [Serializable]
        private sealed class ProgressionTrackRecordJson
        {
            public string id;
            public string displayName;
            public string kind;
            public string classId;
            public string targetWeaponId;
            public ProgressionNodeRecordJson[] nodes;
        }

        [Serializable]
        private sealed class ProgressionNodeRecordJson
        {
            public string id;
            public string displayName;
            public string upgradeId;
            public string kind;
            public int tier;
            public int pointCost;
            public int maxRank;
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
            public string tint;
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
            public string displayName;
        }

        [Serializable]
        private sealed class TrackRecordJson
        {
            public string id;
            public string displayName;
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
            public SurvivorsGameplayTuningRecordJson sharedGameplayTuning;
            public RunFlowProfileRecordJson[] profiles;
        }

        [Serializable]
        private sealed class RunFlowProfileRecordJson
        {
            public string id;
            public string displayName;
            public string description;
            public float targetDurationSeconds;
            public float runRewardMultiplier;
            public int evolutionRequiredRankReduction;
            public bool endlessContinuationEnabled;
            public float enemySpawnIntervalSeconds;
            public int enemyMaximumAlive;
            public int enemySpawnPackBaseCount;
            public int enemySpawnPackMaxCount;
            public int enemyRangedAttackDodgeExperienceReward;
            public float enemyMaxHealth;
            public float enemyMoveSpeed;
            public float enemyRadius;
            public float enemyContactDamage;
            public float enemyContactIntervalSeconds;
            public int enemyExperienceReward;
            public float summonerSupportInitialDelaySeconds;
            public float summonerSupportIntervalSeconds;
            public int summonerSupportCount;
            public int summonerSupportExtraAliveAllowance;
            public float summonerSupportRadius;
            public int splitterChildCount;
            public float splitterChildSpawnRadius;
            public float enemySoftLeashRadius;
            public float enemyHardRecycleRadius;
            public float enemyRecycleDelaySeconds;
            public float enemyRecycleMinimumRespawnDistance;
            public float enemyRecycleMaximumRespawnDistance;
            public float majorThreatCatchUpRadius;
            public float majorThreatCatchUpSpeedMultiplier;
            public float majorThreatRepositionRadius;
            public float majorThreatRepositionDelaySeconds;
            public float offscreenSpawnPadding;
            public float spawnBandDepth;
            public float majorThreatOffscreenSpawnPadding;
            public float recycledEnemyOffscreenSpawnPadding;
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
            public float minibossMaxHealth;
            public float minibossMoveSpeed;
            public float minibossRadius;
            public float minibossContactDamage;
            public float minibossContactIntervalSeconds;
            public int minibossExperienceReward;
            public float bossSpawnTimeSeconds;
            public float bossMaxHealth;
            public float bossMoveSpeed;
            public float bossRadius;
            public float bossContactDamage;
            public float bossContactIntervalSeconds;
            public int bossExperienceReward;
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
            public float pickupAttractRange;
            public float pickupAttractionSpeed;
            public float pickupCollectRadius;
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
            public SurvivorsGameplayTuningRecordJson gameplayTuningOverrides;
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

    [Serializable]
    internal sealed class SurvivorsGameplayTuningRecordJson
    {
        private static readonly string[] AuthoredFieldNames = CreateFieldNames();

        public static IReadOnlyList<string> FieldNames => AuthoredFieldNames;

        public float playerMoveSpeed;
        public float playerRadius;
        public float playerMaxHealth;
        public float playerContactInvulnerabilitySeconds;
        public float dashDistance;
        public float dashCooldownSeconds;
        public float dashInvulnerabilitySeconds;
        public float dashKnockbackRadius;
        public float dashKnockbackDistance;
        public float dashDamage;
        public float enemySpawnRadius;
        public int enemySpawnPackIncreaseEveryEscalations;
        public float enemySeparationRadius;
        public float enemySeparationStrength;
        public int enemySeparationMaxNeighbors;
        public float enemyRangedAttackWindupSeconds;
        public float majorThreatWarningLeadSeconds;
        public float hordeRushClearSurgeDurationSeconds;
        public float hordeRushClearSurgeDamageBonus;
        public float hordeRushClearSurgeMoveSpeedBonus;
        public float hordeRushClearSurgeCooldownMultiplierBonus;
        public float hordeRushClearSurgePickupRangeBonus;
        public float majorThreatEnrageHealthThreshold;
        public int majorThreatEnrageEliteSupportCount;
        public int majorThreatEnrageMinibossSupportCount;
        public int majorThreatEnrageBossSupportCount;
        public int majorThreatEnrageExtraAliveAllowance;
        public float majorThreatEnrageSupportRadius;
        public float majorThreatSlamIntervalSeconds;
        public float majorThreatSlamTelegraphSeconds;
        public float majorThreatSlamRadius;
        public float majorThreatSlamDamage;
        public float orbitKnockbackDistance;
        public float crimsonAegisOrbitKnockbackDistance;
        public float magnetRecallSpeedMultiplier;
        public float gemRushDurationSeconds;
        public float gemRushDamageBonus;
        public float gemRushMoveSpeedBonus;
        public float gemRushCooldownMultiplierBonus;
        public float gemRushPickupRangeBonus;
        public float majorRewardCacheAttractionSpeedMultiplier;
        public float waystoneDiscoveryRadius;
        public int waystoneExperienceGemCount;
        public int waystoneBloodShardInterval;
        public int waystoneAmbushInterval;
        public int waystoneAmbushBaseEnemyCount;
        public int waystoneAmbushExtraAliveAllowance;
        public float waystoneAmbushRadius;
        public float waystoneFocusDurationSeconds;
        public float waystoneFocusDamageBonus;
        public float waystoneFocusMoveSpeedBonus;
        public float waystoneFocusCooldownMultiplierBonus;
        public float waystoneFocusPickupRangeBonus;
        public int waystoneChainInterval;
        public int waystoneChainBonusGemCount;
        public float waystoneChainDurationSeconds;
        public float waystoneChainDamageBonus;
        public float waystoneChainMoveSpeedBonus;
        public float waystoneChainCooldownMultiplierBonus;
        public float waystoneChainPickupRangeBonus;
        public float waystoneChainPulseDamage;
        public float waystoneChainPulseRadius;
        public int healthPickupHealAmount;
        public float lowHealthClutchPulseDamage;
        public float lowHealthClutchPulseRadius;
        public float lowHealthClutchSafetySeconds;
        public int bloodShardPickupAmount;
        public float weaponLoadoutSurgeDurationSeconds;
        public float weaponLoadoutSurgeDamageBonus;
        public float weaponLoadoutSurgeMoveSpeedBonus;
        public float weaponLoadoutSurgeCooldownMultiplierBonus;
        public float weaponLoadoutSurgePickupRangeBonus;
        public float weaponLoadoutSurgePulseDamage;
        public float weaponLoadoutSurgePulseRadius;
        public float passiveLoadoutSurgeDurationSeconds;
        public float passiveLoadoutSurgeDamageBonus;
        public float passiveLoadoutSurgeMoveSpeedBonus;
        public float passiveLoadoutSurgeCooldownMultiplierBonus;
        public float passiveLoadoutSurgePickupRangeBonus;
        public float passiveLoadoutSurgeExperienceGainMultiplierBonus;
        public float passiveLoadoutSurgePulseDamage;
        public float passiveLoadoutSurgePulseRadius;
        public float statusPoisonDurationSeconds;
        public float statusBleedDurationSeconds;
        public float statusBurnDurationSeconds;
        public float evolutionSurgeDamage;
        public float evolutionSurgeRadius;
        public int evolutionChainSurgeMinimumEvolutions;
        public float evolutionChainSurgeDurationSeconds;
        public float evolutionChainSurgeDamageBonus;
        public float evolutionChainSurgeMoveSpeedBonus;
        public float evolutionChainSurgeCooldownMultiplierBonus;
        public float evolutionChainSurgePickupRangeBonus;
        public float evolutionChainSurgePulseDamage;
        public float evolutionChainSurgePulseRadius;
        public float levelUpPulseDamage;
        public float levelUpPulseRadius;
        public float rewardUpgradeSurgeDamage;
        public float rewardUpgradeSurgeRadius;
        public int rewardJackpotExperienceGemBaseCount;
        public int rewardJackpotExperienceGemPerRarityTier;
        public int rewardJackpotBloodShardBaseAmount;
        public int rewardJackpotLegendaryExtraBloodShardAmount;
        public float bossRelicSurgeDamage;
        public float bossRelicSurgeRadius;
        public float bossRelicSurgeDurationSeconds;
        public float bossRelicSurgeDamageBonus;
        public float bossRelicSurgeMoveSpeedBonus;
        public float bossRelicSurgeCooldownMultiplierBonus;
        public float bossRelicSurgePickupRangeBonus;
        public float startingBarrierCapacity;
        public float baseBarrierRegenPerSecond;

        private static string[] CreateFieldNames()
        {
            System.Reflection.FieldInfo[] fields = typeof(SurvivorsGameplayTuningRecordJson).GetFields(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var names = new string[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                names[i] = fields[i].Name;
            }

            return names;
        }
    }
}
