using System;
using System.Collections.Generic;
using Deucarian.Progression;
using Deucarian.RunUpgrades;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors
{
    public sealed class SurvivorsContentValidationResult
    {
        private readonly List<string> _errors = new List<string>();

        public bool Succeeded => _errors.Count == 0;
        public IReadOnlyList<string> Errors => _errors;

        internal void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                _errors.Add(error);
            }
        }
    }

    public static class SurvivorsContentValidator
    {
        public static SurvivorsContentValidationResult ValidateRuntimeContent(
            IReadOnlyList<SurvivorsWeaponArchetypeDefinition> weapons,
            RunUpgradeCatalog upgrades,
            IReadOnlyList<RunUpgradeTargetId> knownUpgradeTargets,
            SurvivorsRunFlowDefinition runFlow = null,
            SurvivorsMetaProgressionDefinition metaProgression = null,
            IReadOnlyList<SurvivorsRelicDefinition> relics = null,
            SurvivorsClassLibraryDefinition classes = null)
        {
            var result = new SurvivorsContentValidationResult();
            ValidateWeaponDefinitions(weapons, result);
            ValidateUpgradeCatalog(upgrades, knownUpgradeTargets, result);
            if (runFlow != null)
            {
                ValidateRunFlowDefinition(runFlow, result);
            }

            if (metaProgression != null)
            {
                ValidateMetaProgressionDefinition(metaProgression, knownUpgradeTargets, result);
            }

            if (relics != null)
            {
                ValidateRelicDefinitions(relics, knownUpgradeTargets, result);
            }

            if (classes != null)
            {
                ValidateClassLibrary(classes, BuildKnownWeaponIds(weapons), result);
            }

            return result;
        }

        public static SurvivorsContentValidationResult ValidateRunFlowContent(SurvivorsRunFlowDefinition runFlow)
        {
            var result = new SurvivorsContentValidationResult();
            ValidateRunFlowDefinition(runFlow, result);
            return result;
        }

        public static SurvivorsContentValidationResult ValidateSampleJson(string weaponJson, string upgradeJson, string enemyJson = null, string rewardJson = null, string relicJson = null, string classJson = null)
        {
            var result = new SurvivorsContentValidationResult();
            WeaponLibraryJson weaponLibrary = ParseJson<WeaponLibraryJson>(weaponJson, "weapon library", result);
            UpgradeLibraryJson upgradeLibrary = ParseJson<UpgradeLibraryJson>(upgradeJson, "upgrade library", result);
            EnemyLibraryJson enemyLibrary = string.IsNullOrWhiteSpace(enemyJson)
                ? null
                : ParseJson<EnemyLibraryJson>(enemyJson, "enemy library", result);
            RewardLibraryJson rewardLibrary = string.IsNullOrWhiteSpace(rewardJson)
                ? null
                : ParseJson<RewardLibraryJson>(rewardJson, "reward library", result);
            RelicLibraryJson relicLibrary = string.IsNullOrWhiteSpace(relicJson)
                ? null
                : ParseJson<RelicLibraryJson>(relicJson, "relic library", result);
            ClassLibraryJson classLibrary = string.IsNullOrWhiteSpace(classJson)
                ? null
                : ParseJson<ClassLibraryJson>(classJson, "class library", result);
            ValidateWeaponLibrary(weaponLibrary, result);
            ValidateUpgradeLibrary(upgradeLibrary, result);
            ValidateEnemyLibrary(enemyLibrary, result);
            ValidateRewardLibrary(rewardLibrary, result);
            ValidateRelicLibrary(relicLibrary, result);
            ValidateClassLibraryJson(classLibrary, result);
            return result;
        }

        private static void ValidateRunFlowDefinition(SurvivorsRunFlowDefinition runFlow, SurvivorsContentValidationResult result)
        {
            if (runFlow == null)
            {
                result.AddError("Run flow definition is required.");
                return;
            }

            if (runFlow.EscalationIntervalSeconds <= 0f)
            {
                result.AddError("Run flow escalation interval must be above zero.");
            }

            if (runFlow.MinimumEnemySpawnIntervalSeconds <= 0f)
            {
                result.AddError("Run flow minimum spawn interval must be above zero.");
            }

            if (runFlow.EnemySpawnIntervalReductionPerEscalation < 0f)
            {
                result.AddError("Run flow spawn interval reduction cannot be negative.");
            }

            if (runFlow.EnemyMaximumAliveIncreasePerEscalation < 0)
            {
                result.AddError("Run flow max-alive increase cannot be negative.");
            }

            if (runFlow.MinibossSpawnTimeSeconds <= 0f)
            {
                result.AddError("Miniboss spawn time must be above zero.");
            }

            if (runFlow.BossSpawnTimeSeconds <= runFlow.MinibossSpawnTimeSeconds)
            {
                result.AddError("Boss spawn time must be later than the miniboss spawn time.");
            }

            if (runFlow.SurvivalVictoryTimeSeconds <= runFlow.BossSpawnTimeSeconds)
            {
                result.AddError("Survival victory time must be later than the boss spawn time.");
            }

            ValidateEnemyProfile(runFlow.Miniboss, SurvivorsEnemyRole.Miniboss, "miniboss", result);
            ValidateEnemyProfile(runFlow.Boss, SurvivorsEnemyRole.Boss, "boss", result);
        }

        private static void ValidateEnemyProfile(SurvivorsEnemyProfile profile, SurvivorsEnemyRole expectedRole, string label, SurvivorsContentValidationResult result)
        {
            if (profile.Role != expectedRole)
            {
                result.AddError($"{label} profile must use role {expectedRole}.");
            }

            ValidateEnemyStats(profile.Id, profile.MaxHealth, profile.MoveSpeed, profile.Radius, profile.ContactDamage, profile.ContactIntervalSeconds, profile.ExperienceReward, result);
        }

        private static void ValidateMetaProgressionDefinition(
            SurvivorsMetaProgressionDefinition metaProgression,
            IReadOnlyList<RunUpgradeTargetId> knownUpgradeTargets,
            SurvivorsContentValidationResult result)
        {
            if (metaProgression == null)
            {
                result.AddError("Meta progression definition is required.");
                return;
            }

            if (metaProgression.BloodShardsCurrencyId.IsEmpty)
            {
                result.AddError("Meta progression requires a blood shards currency id.");
            }

            if (metaProgression.LegacyExperienceTrackId.IsEmpty)
            {
                result.AddError("Meta progression requires a legacy XP track id.");
            }

            var knownTargets = new HashSet<string>(StringComparer.Ordinal);
            if (knownUpgradeTargets != null)
            {
                for (int i = 0; i < knownUpgradeTargets.Count; i++)
                {
                    knownTargets.Add(knownUpgradeTargets[i].Value);
                }
            }

            var upgradeIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < metaProgression.PersistentUpgrades.Count; i++)
            {
                SurvivorsPersistentUpgradeDefinition upgrade = metaProgression.PersistentUpgrades[i];
                if (upgrade == null)
                {
                    result.AddError($"Persistent upgrade definition at index {i} is null.");
                    continue;
                }

                if (upgrade.Id.IsEmpty || string.IsNullOrWhiteSpace(upgrade.Id.Value))
                {
                    result.AddError($"Persistent upgrade definition at index {i} is missing an id.");
                }
                else if (!upgradeIds.Add(upgrade.Id.Value))
                {
                    result.AddError($"Duplicate persistent upgrade id: {upgrade.Id.Value}");
                }

                if (string.IsNullOrWhiteSpace(upgrade.EffectId))
                {
                    result.AddError($"Persistent upgrade {upgrade.Id.Value} is missing an effect id.");
                }

                if (string.IsNullOrWhiteSpace(upgrade.TargetId) || !knownTargets.Contains(upgrade.TargetId))
                {
                    result.AddError($"Persistent upgrade {upgrade.Id.Value} targets unknown content id: {upgrade.TargetId}");
                }

                if (upgrade.MaxRank < 1)
                {
                    result.AddError($"Persistent upgrade {upgrade.Id.Value} requires max rank at least one.");
                }

                if (upgrade.RankCosts.Count != upgrade.MaxRank)
                {
                    result.AddError($"Persistent upgrade {upgrade.Id.Value} rank cost count must match max rank.");
                }

                for (int costIndex = 0; costIndex < upgrade.RankCosts.Count; costIndex++)
                {
                    if (upgrade.RankCosts[costIndex] <= 0)
                    {
                        result.AddError($"Persistent upgrade {upgrade.Id.Value} rank cost must be above zero.");
                    }
                }
            }

            var rewardIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < metaProgression.Rewards.Count; i++)
            {
                SurvivorsRewardDefinition reward = metaProgression.Rewards[i];
                if (reward == null)
                {
                    result.AddError($"Reward definition at index {i} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(reward.Id))
                {
                    result.AddError($"Reward definition at index {i} is missing an id.");
                }
                else if (!rewardIds.Add(reward.Id))
                {
                    result.AddError($"Duplicate reward id: {reward.Id}");
                }

                if (!reward.CurrencyId.Equals(metaProgression.BloodShardsCurrencyId))
                {
                    result.AddError($"Reward {reward.Id} references unknown currency id: {reward.CurrencyId.Value}");
                }

                if (!reward.TrackId.Equals(metaProgression.LegacyExperienceTrackId))
                {
                    result.AddError($"Reward {reward.Id} references unknown progression track id: {reward.TrackId.Value}");
                }

                if (reward.CurrencyAmount <= 0 && reward.TrackAmount <= 0)
                {
                    result.AddError($"Reward {reward.Id} must grant currency or legacy XP.");
                }
            }
        }

        private static void ValidateRelicDefinitions(
            IReadOnlyList<SurvivorsRelicDefinition> relics,
            IReadOnlyList<RunUpgradeTargetId> knownUpgradeTargets,
            SurvivorsContentValidationResult result)
        {
            if (relics == null || relics.Count == 0)
            {
                result.AddError("At least one relic definition is required.");
                return;
            }

            var knownTargets = new HashSet<string>(StringComparer.Ordinal);
            if (knownUpgradeTargets != null)
            {
                for (int i = 0; i < knownUpgradeTargets.Count; i++)
                {
                    knownTargets.Add(knownUpgradeTargets[i].Value);
                }
            }

            var relicIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < relics.Count; i++)
            {
                SurvivorsRelicDefinition relic = relics[i];
                if (relic == null)
                {
                    result.AddError($"Relic definition at index {i} is null.");
                    continue;
                }

                ValidateRelicRecord(relic.Id, relic.TargetId, relic.EffectId, relic.EffectKind.ToString(), relic.Amount, relic.Weight, knownTargets, result);
                if (!string.IsNullOrWhiteSpace(relic.Id) && !relicIds.Add(relic.Id))
                {
                    result.AddError($"Duplicate relic id: {relic.Id}");
                }
            }
        }

        private static void ValidateClassLibrary(
            SurvivorsClassLibraryDefinition classLibrary,
            HashSet<string> knownWeaponIds,
            SurvivorsContentValidationResult result)
        {
            if (classLibrary == null || classLibrary.Classes.Count == 0)
            {
                result.AddError("At least one class definition is required.");
                return;
            }

            var classIds = new HashSet<string>(StringComparer.Ordinal);
            int defaultUnlockedCount = 0;
            for (int i = 0; i < classLibrary.Classes.Count; i++)
            {
                SurvivorsClassDefinition definition = classLibrary.Classes[i];
                if (definition == null)
                {
                    result.AddError($"Class definition at index {i} is null.");
                    continue;
                }

                ValidateClassRecord(
                    definition.Id,
                    definition.StartingWeaponId,
                    definition.IsUnlockedByDefault,
                    definition.UnlockRewardId,
                    definition.StartingStatModifiers,
                    knownWeaponIds,
                    result);
                if (!string.IsNullOrWhiteSpace(definition.Id) && !classIds.Add(definition.Id))
                {
                    result.AddError($"Duplicate class id: {definition.Id}");
                }

                if (definition.IsUnlockedByDefault)
                {
                    defaultUnlockedCount++;
                }
            }

            if (defaultUnlockedCount == 0)
            {
                result.AddError("Class library requires at least one default unlocked class.");
            }
        }

        private static void ValidateWeaponDefinitions(IReadOnlyList<SurvivorsWeaponArchetypeDefinition> weapons, SurvivorsContentValidationResult result)
        {
            if (weapons == null || weapons.Count == 0)
            {
                result.AddError("At least one Survivors weapon definition is required.");
                return;
            }

            var weaponIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < weapons.Count; i++)
            {
                SurvivorsWeaponArchetypeDefinition weapon = weapons[i];
                if (weapon == null)
                {
                    result.AddError($"Weapon definition at index {i} is null.");
                    continue;
                }

                if (!weaponIds.Add(weapon.Id))
                {
                    result.AddError($"Duplicate weapon id: {weapon.Id}");
                }

                if (!Enum.IsDefined(typeof(SurvivorsWeaponArchetype), weapon.Archetype))
                {
                    result.AddError($"Weapon {weapon.Id} uses unsupported archetype {weapon.Archetype}.");
                }

                if (weapon.Archetype == SurvivorsWeaponArchetype.Projectile)
                {
                    if (weapon.ProjectileSpeed <= 0f)
                    {
                        result.AddError($"Projectile weapon {weapon.Id} requires projectile speed above zero.");
                    }

                    if (weapon.ProjectileLifetimeSeconds <= 0f)
                    {
                        result.AddError($"Projectile weapon {weapon.Id} requires projectile lifetime above zero.");
                    }
                }

                if (weapon.Archetype == SurvivorsWeaponArchetype.Hitscan)
                {
                    if (weapon.HitscanCount <= 0)
                    {
                        result.AddError($"Hitscan weapon {weapon.Id} requires hitscan count above zero.");
                    }

                    if (weapon.HitscanWidth <= 0f)
                    {
                        result.AddError($"Hitscan weapon {weapon.Id} requires beam width above zero.");
                    }
                }

                if (weapon.IsPayload)
                {
                    ValidatePayloadRuntimeWeapon(weapon.Id, weapon.PayloadCount, weapon.PayloadTravelSpeed, weapon.PayloadArmingSeconds, weapon.PayloadLifetimeSeconds, weapon.PayloadTriggerRadius, weapon.PayloadExplosionRadius, weapon.PayloadLeavesHazard, weapon.PayloadHazardDurationSeconds, weapon.PayloadHazardTickIntervalSeconds, weapon.PayloadHazardDamageRatio, result);
                }
            }
        }

        private static void ValidateUpgradeCatalog(
            RunUpgradeCatalog upgrades,
            IReadOnlyList<RunUpgradeTargetId> knownUpgradeTargets,
            SurvivorsContentValidationResult result)
        {
            if (upgrades == null || upgrades.Definitions.Count == 0)
            {
                result.AddError("At least one run upgrade definition is required.");
                return;
            }

            var knownTargets = new HashSet<string>(StringComparer.Ordinal);
            if (knownUpgradeTargets != null)
            {
                for (int i = 0; i < knownUpgradeTargets.Count; i++)
                {
                    knownTargets.Add(knownUpgradeTargets[i].Value);
                }
            }

            var upgradeIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < upgrades.Definitions.Count; i++)
            {
                RunUpgradeDefinition upgrade = upgrades.Definitions[i];
                if (upgrade == null)
                {
                    result.AddError($"Upgrade definition at index {i} is null.");
                    continue;
                }

                if (!upgradeIds.Add(upgrade.Id.Value))
                {
                    result.AddError($"Duplicate upgrade id: {upgrade.Id.Value}");
                }

                for (int effectIndex = 0; effectIndex < upgrade.Effects.Count; effectIndex++)
                {
                    RunUpgradeEffectDescriptor effect = upgrade.Effects[effectIndex];
                    if (!knownTargets.Contains(effect.TargetId.Value))
                    {
                        result.AddError($"Upgrade {upgrade.Id.Value} targets unknown content id: {effect.TargetId.Value}");
                    }
                }
            }
        }

        private static void ValidateWeaponLibrary(WeaponLibraryJson library, SurvivorsContentValidationResult result)
        {
            if (library == null)
            {
                return;
            }

            var projectileIds = new HashSet<string>(StringComparer.Ordinal);
            if (library.projectiles != null)
            {
                for (int i = 0; i < library.projectiles.Length; i++)
                {
                    ProjectileRecordJson projectile = library.projectiles[i];
                    if (projectile == null || string.IsNullOrWhiteSpace(projectile.id))
                    {
                        result.AddError($"Projectile record at index {i} is missing an id.");
                        continue;
                    }

                    if (!projectileIds.Add(projectile.id))
                    {
                        result.AddError($"Duplicate projectile id: {projectile.id}");
                    }
                }
            }

            var weaponIds = new HashSet<string>(StringComparer.Ordinal);
            if (library.weapons == null || library.weapons.Length == 0)
            {
                result.AddError("Sample weapon library must contain at least one weapon.");
                return;
            }

            for (int i = 0; i < library.weapons.Length; i++)
            {
                WeaponRecordJson weapon = library.weapons[i];
                if (weapon == null || string.IsNullOrWhiteSpace(weapon.id))
                {
                    result.AddError($"Weapon record at index {i} is missing an id.");
                    continue;
                }

                if (!weaponIds.Add(weapon.id))
                {
                    result.AddError($"Duplicate weapon id: {weapon.id}");
                }

                if (!Enum.TryParse(weapon.fireMode, ignoreCase: true, out SurvivorsWeaponArchetype archetype) ||
                    !Enum.IsDefined(typeof(SurvivorsWeaponArchetype), archetype))
                {
                    result.AddError($"Weapon {weapon.id} references unknown archetype: {weapon.fireMode}");
                    continue;
                }

                if (archetype == SurvivorsWeaponArchetype.Projectile && !projectileIds.Contains(weapon.projectileId))
                {
                    result.AddError($"Weapon {weapon.id} references missing projectile id: {weapon.projectileId}");
                }

                if (IsPayloadArchetype(archetype))
                {
                    ValidatePayloadRuntimeWeapon(
                        weapon.id,
                        weapon.payloadCount,
                        weapon.payloadTravelSpeed,
                        weapon.payloadArmingSeconds,
                        weapon.payloadLifetimeSeconds,
                        weapon.payloadTriggerRadius,
                        weapon.payloadExplosionRadius,
                        weapon.payloadLeavesHazard,
                        weapon.payloadHazardDurationSeconds,
                        weapon.payloadHazardTickIntervalSeconds,
                        weapon.payloadHazardDamageRatio,
                        result);
                }
            }
        }

        private static bool IsPayloadArchetype(SurvivorsWeaponArchetype archetype)
        {
            return archetype == SurvivorsWeaponArchetype.Grenade ||
                archetype == SurvivorsWeaponArchetype.Trap ||
                archetype == SurvivorsWeaponArchetype.Mine;
        }

        private static void ValidatePayloadRuntimeWeapon(
            string weaponId,
            int payloadCount,
            float payloadTravelSpeed,
            float payloadArmingSeconds,
            float payloadLifetimeSeconds,
            float payloadTriggerRadius,
            float payloadExplosionRadius,
            bool payloadLeavesHazard,
            float payloadHazardDurationSeconds,
            float payloadHazardTickIntervalSeconds,
            float payloadHazardDamageRatio,
            SurvivorsContentValidationResult result)
        {
            if (payloadCount < 1)
            {
                result.AddError($"Payload weapon {weaponId} requires payload count at least one.");
            }

            if (payloadTravelSpeed <= 0f)
            {
                result.AddError($"Payload weapon {weaponId} requires payload travel speed above zero.");
            }

            if (payloadArmingSeconds <= 0f)
            {
                result.AddError($"Payload weapon {weaponId} requires payload arming time above zero.");
            }

            if (payloadLifetimeSeconds <= 0f)
            {
                result.AddError($"Payload weapon {weaponId} requires payload lifetime above zero.");
            }

            if (payloadTriggerRadius <= 0f)
            {
                result.AddError($"Payload weapon {weaponId} requires payload trigger radius above zero.");
            }

            if (payloadExplosionRadius <= 0f)
            {
                result.AddError($"Payload weapon {weaponId} requires payload explosion radius above zero.");
            }

            if (payloadLeavesHazard)
            {
                if (payloadHazardDurationSeconds <= 0f)
                {
                    result.AddError($"Payload weapon {weaponId} requires hazard duration above zero.");
                }

                if (payloadHazardTickIntervalSeconds <= 0f)
                {
                    result.AddError($"Payload weapon {weaponId} requires hazard tick interval above zero.");
                }

                if (payloadHazardDamageRatio < 0f)
                {
                    result.AddError($"Payload weapon {weaponId} cannot use negative hazard damage ratio.");
                }
            }
        }

        private static void ValidateUpgradeLibrary(UpgradeLibraryJson library, SurvivorsContentValidationResult result)
        {
            if (library == null)
            {
                return;
            }

            if (library.upgrades == null || library.upgrades.Length == 0)
            {
                result.AddError("Sample upgrade library must contain at least one upgrade.");
                return;
            }

            var knownTargets = new HashSet<string>(StringComparer.Ordinal);
            IReadOnlyList<RunUpgradeTargetId> targetIds = BasicSurvivorsGame.CreateKnownUpgradeTargets();
            for (int i = 0; i < targetIds.Count; i++)
            {
                knownTargets.Add(targetIds[i].Value);
            }

            var upgradeIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < library.upgrades.Length; i++)
            {
                UpgradeRecordJson upgrade = library.upgrades[i];
                if (upgrade == null || string.IsNullOrWhiteSpace(upgrade.id))
                {
                    result.AddError($"Upgrade record at index {i} is missing an id.");
                    continue;
                }

                if (!upgradeIds.Add(upgrade.id))
                {
                    result.AddError($"Duplicate upgrade id: {upgrade.id}");
                }

                if (string.IsNullOrWhiteSpace(upgrade.effect))
                {
                    result.AddError($"Upgrade {upgrade.id} is missing an effect id.");
                }

                if (!knownTargets.Contains(upgrade.target))
                {
                    result.AddError($"Upgrade {upgrade.id} references unknown target: {upgrade.target}");
                }
            }
        }

        private static void ValidateRewardLibrary(RewardLibraryJson library, SurvivorsContentValidationResult result)
        {
            if (library == null)
            {
                return;
            }

            var currencyIds = new HashSet<string>(StringComparer.Ordinal);
            if (library.currencies == null || library.currencies.Length == 0)
            {
                result.AddError("Sample reward library must contain at least one currency.");
            }
            else
            {
                for (int i = 0; i < library.currencies.Length; i++)
                {
                    CurrencyRecordJson currency = library.currencies[i];
                    if (currency == null || string.IsNullOrWhiteSpace(currency.id))
                    {
                        result.AddError($"Currency record at index {i} is missing an id.");
                        continue;
                    }

                    if (!IsValidCurrencyId(currency.id))
                    {
                        result.AddError($"Currency {currency.id} is not a valid currency id.");
                    }

                    if (!currencyIds.Add(currency.id))
                    {
                        result.AddError($"Duplicate currency id: {currency.id}");
                    }
                }
            }

            var trackIds = new HashSet<string>(StringComparer.Ordinal);
            if (library.tracks == null || library.tracks.Length == 0)
            {
                result.AddError("Sample reward library must contain at least one legacy XP track.");
            }
            else
            {
                for (int i = 0; i < library.tracks.Length; i++)
                {
                    TrackRecordJson track = library.tracks[i];
                    if (track == null || string.IsNullOrWhiteSpace(track.id))
                    {
                        result.AddError($"Progression track record at index {i} is missing an id.");
                        continue;
                    }

                    if (!IsValidTrackId(track.id))
                    {
                        result.AddError($"Progression track {track.id} is not a valid track id.");
                    }

                    if (!trackIds.Add(track.id))
                    {
                        result.AddError($"Duplicate progression track id: {track.id}");
                    }
                }
            }

            var knownTargets = new HashSet<string>(StringComparer.Ordinal);
            IReadOnlyList<RunUpgradeTargetId> targetIds = BasicSurvivorsGame.CreateKnownUpgradeTargets();
            for (int i = 0; i < targetIds.Count; i++)
            {
                knownTargets.Add(targetIds[i].Value);
            }

            var persistentUpgradeIds = new HashSet<string>(StringComparer.Ordinal);
            if (library.persistentUpgrades == null || library.persistentUpgrades.Length == 0)
            {
                result.AddError("Sample reward library must contain at least one persistent upgrade.");
            }
            else
            {
                for (int i = 0; i < library.persistentUpgrades.Length; i++)
                {
                    PersistentUpgradeRecordJson upgrade = library.persistentUpgrades[i];
                    if (upgrade == null || string.IsNullOrWhiteSpace(upgrade.id))
                    {
                        result.AddError($"Persistent upgrade record at index {i} is missing an id.");
                        continue;
                    }

                    if (!IsValidResearchNodeId(upgrade.id))
                    {
                        result.AddError($"Persistent upgrade {upgrade.id} is not a valid upgrade id.");
                    }

                    if (!persistentUpgradeIds.Add(upgrade.id))
                    {
                        result.AddError($"Duplicate persistent upgrade id: {upgrade.id}");
                    }

                    if (string.IsNullOrWhiteSpace(upgrade.effect))
                    {
                        result.AddError($"Persistent upgrade {upgrade.id} is missing an effect id.");
                    }

                    if (!knownTargets.Contains(upgrade.target))
                    {
                        result.AddError($"Persistent upgrade {upgrade.id} references unknown target: {upgrade.target}");
                    }

                    if (upgrade.maxRank < 1)
                    {
                        result.AddError($"Persistent upgrade {upgrade.id} requires max rank at least one.");
                    }

                    if (upgrade.rankCosts == null || upgrade.rankCosts.Length != upgrade.maxRank)
                    {
                        result.AddError($"Persistent upgrade {upgrade.id} rank cost count must match max rank.");
                    }
                    else
                    {
                        for (int costIndex = 0; costIndex < upgrade.rankCosts.Length; costIndex++)
                        {
                            if (upgrade.rankCosts[costIndex] <= 0)
                            {
                                result.AddError($"Persistent upgrade {upgrade.id} rank cost must be above zero.");
                            }
                        }
                    }
                }
            }

            var rewardIds = new HashSet<string>(StringComparer.Ordinal);
            if (library.rewards == null || library.rewards.Length == 0)
            {
                result.AddError("Sample reward library must contain at least one reward.");
                return;
            }

            for (int i = 0; i < library.rewards.Length; i++)
            {
                RewardRecordJson reward = library.rewards[i];
                if (reward == null || string.IsNullOrWhiteSpace(reward.id))
                {
                    result.AddError($"Reward record at index {i} is missing an id.");
                    continue;
                }

                if (!rewardIds.Add(reward.id))
                {
                    result.AddError($"Duplicate reward id: {reward.id}");
                }

                if (!currencyIds.Contains(reward.currencyId))
                {
                    result.AddError($"Reward {reward.id} references unknown currency: {reward.currencyId}");
                }

                if (!trackIds.Contains(reward.trackId))
                {
                    result.AddError($"Reward {reward.id} references unknown progression track: {reward.trackId}");
                }

                if (reward.currencyAmount < 0)
                {
                    result.AddError($"Reward {reward.id} cannot grant negative currency.");
                }

                if (reward.trackAmount < 0)
                {
                    result.AddError($"Reward {reward.id} cannot grant negative legacy XP.");
                }

                if (reward.currencyAmount == 0 && reward.trackAmount == 0)
                {
                    result.AddError($"Reward {reward.id} must grant currency or legacy XP.");
                }
            }
        }

        private static void ValidateRelicLibrary(RelicLibraryJson library, SurvivorsContentValidationResult result)
        {
            if (library == null)
            {
                return;
            }

            if (library.relics == null || library.relics.Length == 0)
            {
                result.AddError("Sample relic library must contain at least one relic.");
                return;
            }

            var knownTargets = new HashSet<string>(StringComparer.Ordinal);
            IReadOnlyList<RunUpgradeTargetId> targetIds = BasicSurvivorsGame.CreateKnownUpgradeTargets();
            for (int i = 0; i < targetIds.Count; i++)
            {
                knownTargets.Add(targetIds[i].Value);
            }

            var relicIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < library.relics.Length; i++)
            {
                RelicRecordJson relic = library.relics[i];
                if (relic == null)
                {
                    result.AddError($"Relic record at index {i} is null.");
                    continue;
                }

                ValidateRelicRecord(relic.id, relic.target, relic.effect, relic.effectKind, relic.amount, relic.weight, knownTargets, result);
                if (!string.IsNullOrWhiteSpace(relic.id) && !relicIds.Add(relic.id))
                {
                    result.AddError($"Duplicate relic id: {relic.id}");
                }
            }
        }

        private static void ValidateClassLibraryJson(ClassLibraryJson library, SurvivorsContentValidationResult result)
        {
            if (library == null)
            {
                return;
            }

            if (library.classes == null || library.classes.Length == 0)
            {
                result.AddError("Sample class library must contain at least one class.");
                return;
            }

            HashSet<string> knownWeaponIds = BuildKnownWeaponIds(BasicSurvivorsGame.CreateWeaponArchetypeDefinitions());
            var classIds = new HashSet<string>(StringComparer.Ordinal);
            int defaultUnlockedCount = 0;
            for (int i = 0; i < library.classes.Length; i++)
            {
                ClassRecordJson classRecord = library.classes[i];
                if (classRecord == null)
                {
                    result.AddError($"Class record at index {i} is null.");
                    continue;
                }

                ValidateClassRecord(
                    classRecord.id,
                    classRecord.startingWeaponId,
                    classRecord.unlockedByDefault,
                    classRecord.unlockRewardId,
                    classRecord.statModifiers,
                    knownWeaponIds,
                    result);
                if (!string.IsNullOrWhiteSpace(classRecord.id) && !classIds.Add(classRecord.id))
                {
                    result.AddError($"Duplicate class id: {classRecord.id}");
                }

                if (classRecord.unlockedByDefault)
                {
                    defaultUnlockedCount++;
                }
            }

            if (defaultUnlockedCount == 0)
            {
                result.AddError("Sample class library requires at least one default unlocked class.");
            }
        }

        private static void ValidateEnemyLibrary(EnemyLibraryJson library, SurvivorsContentValidationResult result)
        {
            if (library == null)
            {
                return;
            }

            if (library.enemies == null || library.enemies.Length == 0)
            {
                result.AddError("Sample enemy library must contain at least one enemy.");
                return;
            }

            var enemyIds = new HashSet<string>(StringComparer.Ordinal);
            int bossCount = 0;
            int minibossCount = 0;
            for (int i = 0; i < library.enemies.Length; i++)
            {
                EnemyRecordJson enemy = library.enemies[i];
                if (enemy == null || string.IsNullOrWhiteSpace(enemy.id))
                {
                    result.AddError($"Enemy record at index {i} is missing an id.");
                    continue;
                }

                if (!enemyIds.Add(enemy.id))
                {
                    result.AddError($"Duplicate enemy id: {enemy.id}");
                }

                if (!Enum.TryParse(enemy.role, ignoreCase: true, out SurvivorsEnemyRole role) ||
                    !Enum.IsDefined(typeof(SurvivorsEnemyRole), role))
                {
                    result.AddError($"Enemy {enemy.id} references unknown role: {enemy.role}");
                    continue;
                }

                if (role == SurvivorsEnemyRole.Miniboss)
                {
                    minibossCount++;
                }
                else if (role == SurvivorsEnemyRole.Boss)
                {
                    bossCount++;
                }

                ValidateEnemyStats(enemy.id, enemy.health, enemy.moveSpeed, enemy.radius, enemy.contactDamage, enemy.contactIntervalSeconds, enemy.experienceDrop, result);
                if (role != SurvivorsEnemyRole.Swarm && enemy.spawnTimeSeconds <= 0f)
                {
                    result.AddError($"Enemy {enemy.id} requires spawn time above zero.");
                }
            }

            if (minibossCount == 0)
            {
                result.AddError("Sample enemy library must contain a miniboss definition.");
            }

            if (bossCount == 0)
            {
                result.AddError("Sample enemy library must contain a boss definition.");
            }
        }

        private static void ValidateEnemyStats(
            string enemyId,
            float health,
            float moveSpeed,
            float radius,
            float contactDamage,
            float contactIntervalSeconds,
            int experienceDrop,
            SurvivorsContentValidationResult result)
        {
            string id = string.IsNullOrWhiteSpace(enemyId) ? "unknown enemy" : enemyId;
            if (health <= 0f)
            {
                result.AddError($"Enemy {id} requires health above zero.");
            }

            if (moveSpeed <= 0f)
            {
                result.AddError($"Enemy {id} requires move speed above zero.");
            }

            if (radius <= 0f)
            {
                result.AddError($"Enemy {id} requires radius above zero.");
            }

            if (contactDamage < 0f)
            {
                result.AddError($"Enemy {id} cannot use negative contact damage.");
            }

            if (contactIntervalSeconds <= 0f)
            {
                result.AddError($"Enemy {id} requires contact interval above zero.");
            }

            if (experienceDrop <= 0)
            {
                result.AddError($"Enemy {id} requires experience drop above zero.");
            }
        }

        private static void ValidateRelicRecord(
            string id,
            string targetId,
            string effectId,
            string effectKind,
            float amount,
            int weight,
            HashSet<string> knownTargets,
            SurvivorsContentValidationResult result)
        {
            string resolvedId = string.IsNullOrWhiteSpace(id) ? "unknown relic" : id;
            if (string.IsNullOrWhiteSpace(id))
            {
                result.AddError("Relic record is missing an id.");
            }

            if (string.IsNullOrWhiteSpace(effectId))
            {
                result.AddError($"Relic {resolvedId} is missing an effect id.");
            }

            if (string.IsNullOrWhiteSpace(targetId) || knownTargets == null || !knownTargets.Contains(targetId))
            {
                result.AddError($"Relic {resolvedId} references unknown target: {targetId}");
            }

            if (weight <= 0)
            {
                result.AddError($"Relic {resolvedId} requires weight above zero.");
            }

            if (!Enum.TryParse(effectKind, ignoreCase: true, out SurvivorsRelicEffectKind parsedKind) ||
                !Enum.IsDefined(typeof(SurvivorsRelicEffectKind), parsedKind))
            {
                result.AddError($"Relic {resolvedId} references unknown effect kind: {effectKind}");
                return;
            }

            if (parsedKind == SurvivorsRelicEffectKind.CooldownMultiplier)
            {
                if (amount >= 0f || amount <= -0.75f)
                {
                    result.AddError($"Relic {resolvedId} cooldown multiplier amount must be below zero and above -0.75.");
                }
            }
            else if (amount <= 0f)
            {
                result.AddError($"Relic {resolvedId} amount must be above zero.");
            }
        }

        private static void ValidateClassRecord(
            string id,
            string startingWeaponId,
            bool unlockedByDefault,
            string unlockRewardId,
            IReadOnlyList<SurvivorsClassStatModifierDefinition> statModifiers,
            HashSet<string> knownWeaponIds,
            SurvivorsContentValidationResult result)
        {
            string resolvedId = string.IsNullOrWhiteSpace(id) ? "unknown class" : id;
            if (string.IsNullOrWhiteSpace(id))
            {
                result.AddError("Class record is missing an id.");
            }

            if (string.IsNullOrWhiteSpace(startingWeaponId) || knownWeaponIds == null || !knownWeaponIds.Contains(startingWeaponId))
            {
                result.AddError($"Class {resolvedId} references unknown starting weapon: {startingWeaponId}");
            }

            if (!unlockedByDefault && string.IsNullOrWhiteSpace(unlockRewardId))
            {
                result.AddError($"Class {resolvedId} requires an unlock reward id.");
            }

            if (statModifiers == null)
            {
                return;
            }

            for (int i = 0; i < statModifiers.Count; i++)
            {
                SurvivorsClassStatModifierDefinition modifier = statModifiers[i];
                if (modifier == null)
                {
                    result.AddError($"Class {resolvedId} stat modifier at index {i} is null.");
                    continue;
                }

                ValidateClassStatRecord(resolvedId, modifier.StatKind.ToString(), modifier.Amount, result);
            }
        }

        private static void ValidateClassRecord(
            string id,
            string startingWeaponId,
            bool unlockedByDefault,
            string unlockRewardId,
            StatModifierRecordJson[] statModifiers,
            HashSet<string> knownWeaponIds,
            SurvivorsContentValidationResult result)
        {
            string resolvedId = string.IsNullOrWhiteSpace(id) ? "unknown class" : id;
            if (string.IsNullOrWhiteSpace(id))
            {
                result.AddError("Class record is missing an id.");
            }

            if (string.IsNullOrWhiteSpace(startingWeaponId) || knownWeaponIds == null || !knownWeaponIds.Contains(startingWeaponId))
            {
                result.AddError($"Class {resolvedId} references unknown starting weapon: {startingWeaponId}");
            }

            if (!unlockedByDefault && string.IsNullOrWhiteSpace(unlockRewardId))
            {
                result.AddError($"Class {resolvedId} requires an unlock reward id.");
            }

            if (statModifiers == null)
            {
                return;
            }

            for (int i = 0; i < statModifiers.Length; i++)
            {
                StatModifierRecordJson modifier = statModifiers[i];
                if (modifier == null)
                {
                    result.AddError($"Class {resolvedId} stat modifier at index {i} is null.");
                    continue;
                }

                ValidateClassStatRecord(resolvedId, modifier.stat, modifier.amount, result);
            }
        }

        private static void ValidateClassStatRecord(string classId, string stat, float amount, SurvivorsContentValidationResult result)
        {
            if (!Enum.TryParse(stat, ignoreCase: true, out SurvivorsClassStatKind parsedStat) ||
                !Enum.IsDefined(typeof(SurvivorsClassStatKind), parsedStat))
            {
                result.AddError($"Class {classId} references unknown stat: {stat}");
            }

            if (Mathf.Approximately(amount, 0f))
            {
                result.AddError($"Class {classId} stat modifier amount cannot be zero.");
            }
        }

        private static HashSet<string> BuildKnownWeaponIds(IReadOnlyList<SurvivorsWeaponArchetypeDefinition> weapons)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (weapons == null)
            {
                return ids;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                SurvivorsWeaponArchetypeDefinition weapon = weapons[i];
                if (weapon != null && !string.IsNullOrWhiteSpace(weapon.Id))
                {
                    ids.Add(weapon.Id);
                }
            }

            return ids;
        }

        private static bool IsValidCurrencyId(string value)
        {
            try
            {
                return !new CurrencyId(value).IsEmpty;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsValidTrackId(string value)
        {
            try
            {
                return !new TrackId(value).IsEmpty;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsValidResearchNodeId(string value)
        {
            try
            {
                return !new ResearchNodeId(value).IsEmpty;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static T ParseJson<T>(string json, string label, SurvivorsContentValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                result.AddError($"Sample {label} JSON is empty.");
                return default;
            }

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (ArgumentException exception)
            {
                result.AddError($"Sample {label} JSON could not be parsed: {exception.Message}");
                return default;
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
            public string fireMode;
            public string projectileId;
            public int payloadCount;
            public float payloadTravelSpeed;
            public float payloadArmingSeconds;
            public float payloadLifetimeSeconds;
            public float payloadTriggerRadius;
            public float payloadExplosionRadius;
            public bool payloadLeavesHazard;
            public float payloadHazardDurationSeconds;
            public float payloadHazardTickIntervalSeconds;
            public float payloadHazardDamageRatio;
        }

        [Serializable]
        private sealed class ProjectileRecordJson
        {
            public string id;
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
            public string rarity;
            public string effect;
            public string target;
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
            public bool finalBoss;
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
            public float damageBonusPerRank;
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
            public ClassRecordJson[] classes;
        }

        [Serializable]
        private sealed class ClassRecordJson
        {
            public string id;
            public string displayName;
            public string startingWeaponId;
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
    }
}
