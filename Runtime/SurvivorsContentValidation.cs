using System;
using System.Collections.Generic;
using Deucarian.GameplayFoundation;
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
        private static readonly string[] RequiredRunFlowRarityTableIds =
        {
            "NormalEarly",
            "NormalMid",
            "NormalLate",
            "Elite",
            "Boss"
        };

        public static SurvivorsContentValidationResult ValidateRuntimeContent(
            IReadOnlyList<SurvivorsWeaponArchetypeDefinition> weapons,
            RunUpgradeCatalog upgrades,
            IReadOnlyList<RunUpgradeTargetId> knownUpgradeTargets,
            SurvivorsRunFlowDefinition runFlow = null,
            SurvivorsMetaProgressionDefinition metaProgression = null,
            IReadOnlyList<SurvivorsRelicDefinition> relics = null,
            SurvivorsClassLibraryDefinition classes = null,
            IReadOnlyList<SurvivorsClassUpgradeGateDefinition> classUpgradeGates = null,
            IReadOnlyList<SurvivorsProgressionTrackDefinition> progressionTracks = null)
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

            if (classUpgradeGates != null)
            {
                ValidateClassUpgradeGates(classUpgradeGates, upgrades, classes, result);
            }

            if (progressionTracks != null)
            {
                ValidateProgressionTracks(progressionTracks, upgrades, classes, BuildKnownWeaponIds(weapons), result);
            }

            return result;
        }

        public static SurvivorsContentValidationResult ValidateRunFlowContent(SurvivorsRunFlowDefinition runFlow)
        {
            var result = new SurvivorsContentValidationResult();
            ValidateRunFlowDefinition(runFlow, result);
            return result;
        }

        public static SurvivorsContentValidationResult ValidateSampleJson(
            string weaponJson,
            string upgradeJson,
            string enemyJson = null,
            string rewardJson = null,
            string relicJson = null,
            string classJson = null,
            string progressionJson = null,
            string pickupJson = null,
            string runFlowJson = null)
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
            ProgressionLibraryJson progressionLibrary = string.IsNullOrWhiteSpace(progressionJson)
                ? null
                : ParseJson<ProgressionLibraryJson>(progressionJson, "progression library", result);
            PickupLibraryJson pickupLibrary = string.IsNullOrWhiteSpace(pickupJson)
                ? null
                : ParseJson<PickupLibraryJson>(pickupJson, "pickup library", result);
            RunFlowLibraryJson runFlowLibrary = string.IsNullOrWhiteSpace(runFlowJson)
                ? null
                : ParseJson<RunFlowLibraryJson>(runFlowJson, "run flow library", result);
            ValidateWeaponLibrary(weaponLibrary, result);
            ValidateUpgradeLibrary(upgradeLibrary, classLibrary, result);
            ValidateEnemyLibrary(enemyLibrary, result);
            ValidatePickupLibrary(pickupLibrary, result);
            ValidateRewardLibrary(rewardLibrary, result);
            ValidateRelicLibrary(relicLibrary, result);
            ValidateClassLibraryJson(classLibrary, result);
            ValidateProgressionLibrary(progressionLibrary, upgradeLibrary, classLibrary, result);
            ValidateRunFlowLibrary(runFlowLibrary, result);
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

            if (runFlow.FirstEliteSpawnTimeSeconds <= 0f)
            {
                result.AddError("First elite spawn time must be above zero.");
            }

            if (runFlow.EliteSpawnIntervalSeconds <= 0f)
            {
                result.AddError("Recurring elite spawn interval must be above zero.");
            }

            if (runFlow.FirstDreadEliteSpawnTimeSeconds <= runFlow.FirstEliteSpawnTimeSeconds)
            {
                result.AddError("First dread elite spawn time must be later than the first elite spawn time.");
            }

            if (runFlow.DreadEliteSpawnIntervalSeconds <= 0f)
            {
                result.AddError("Recurring dread elite spawn interval must be above zero.");
            }

            if (runFlow.MinibossSpawnTimeSeconds <= runFlow.FirstEliteSpawnTimeSeconds)
            {
                result.AddError("Miniboss spawn time must be later than the first elite spawn time.");
            }

            if (runFlow.MinibossSpawnTimeSeconds <= runFlow.FirstDreadEliteSpawnTimeSeconds)
            {
                result.AddError("Miniboss spawn time must be later than the first dread elite spawn time.");
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

                if (upgrade.AmountPerRank <= 0f)
                {
                    result.AddError($"Persistent upgrade {upgrade.Id.Value} amount per rank must be above zero.");
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
                    definition.StartingWeaponIds,
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

            if (!string.IsNullOrWhiteSpace(classLibrary.DefaultClassId))
            {
                if (!classIds.Contains(classLibrary.DefaultClassId))
                {
                    result.AddError($"Class library references unknown default class id: {classLibrary.DefaultClassId}");
                }
                else if (!classLibrary.TryGetClass(classLibrary.DefaultClassId, out SurvivorsClassDefinition defaultClass) || !defaultClass.IsUnlockedByDefault)
                {
                    result.AddError($"Class library default class must be unlocked by default: {classLibrary.DefaultClassId}");
                }
            }
        }

        private static void ValidateClassUpgradeGates(
            IReadOnlyList<SurvivorsClassUpgradeGateDefinition> gates,
            RunUpgradeCatalog upgrades,
            SurvivorsClassLibraryDefinition classes,
            SurvivorsContentValidationResult result)
        {
            if (gates == null)
            {
                return;
            }

            var knownUpgradeIds = new HashSet<string>(StringComparer.Ordinal);
            if (upgrades != null)
            {
                for (int i = 0; i < upgrades.Definitions.Count; i++)
                {
                    RunUpgradeDefinition definition = upgrades.Definitions[i];
                    if (definition != null)
                    {
                        knownUpgradeIds.Add(definition.Id.Value);
                    }
                }
            }

            var knownClassIds = BuildKnownClassIds(classes);
            var gatedUpgradeIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < gates.Count; i++)
            {
                SurvivorsClassUpgradeGateDefinition gate = gates[i];
                if (gate == null)
                {
                    result.AddError($"Class upgrade gate at index {i} is null.");
                    continue;
                }

                ValidateClassUpgradeGateRecord(gate.UpgradeId, gate.AllowedClassIds, knownUpgradeIds, knownClassIds, result);
                if (!string.IsNullOrWhiteSpace(gate.UpgradeId) && !gatedUpgradeIds.Add(gate.UpgradeId))
                {
                    result.AddError($"Duplicate class upgrade gate id: {gate.UpgradeId}");
                }
            }
        }

        private static void ValidateProgressionTracks(
            IReadOnlyList<SurvivorsProgressionTrackDefinition> tracks,
            RunUpgradeCatalog upgrades,
            SurvivorsClassLibraryDefinition classes,
            HashSet<string> knownWeaponIds,
            SurvivorsContentValidationResult result)
        {
            if (tracks == null || tracks.Count == 0)
            {
                result.AddError("At least one progression track definition is required.");
                return;
            }

            Dictionary<string, int> knownUpgradeMaxRanks = BuildKnownUpgradeMaxRanks(upgrades);
            HashSet<string> knownClassIds = BuildKnownClassIds(classes);
            var trackIds = new HashSet<string>(StringComparer.Ordinal);
            var nodeIds = new HashSet<string>(StringComparer.Ordinal);
            var passiveAtlasClassIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < tracks.Count; i++)
            {
                SurvivorsProgressionTrackDefinition track = tracks[i];
                if (track == null)
                {
                    result.AddError($"Progression track at index {i} is null.");
                    continue;
                }

                string resolvedTrackId = string.IsNullOrWhiteSpace(track.Id) ? "unknown progression track" : track.Id;
                if (string.IsNullOrWhiteSpace(track.Id))
                {
                    result.AddError("Progression track is missing an id.");
                }
                else if (!trackIds.Add(track.Id))
                {
                    result.AddError($"Duplicate progression track id: {track.Id}");
                }

                ValidateProgressionTrackOwnership(
                    resolvedTrackId,
                    track.Kind.ToString(),
                    track.ClassId,
                    track.TargetWeaponId,
                    knownClassIds,
                    knownWeaponIds,
                    passiveAtlasClassIds,
                    result);

                if (track.Nodes == null || track.Nodes.Count == 0)
                {
                    result.AddError($"Progression track {resolvedTrackId} requires at least one node.");
                    continue;
                }

                for (int nodeIndex = 0; nodeIndex < track.Nodes.Count; nodeIndex++)
                {
                    SurvivorsProgressionNodeDefinition node = track.Nodes[nodeIndex];
                    if (node == null)
                    {
                        result.AddError($"Progression track {resolvedTrackId} node at index {nodeIndex} is null.");
                        continue;
                    }

                    ValidateProgressionNodeRecord(
                        resolvedTrackId,
                        node.Id,
                        node.UpgradeId,
                        node.Kind.ToString(),
                        node.Tier,
                        node.PointCost,
                        node.MaxRank,
                        knownUpgradeMaxRanks,
                        nodeIds,
                        result);
                }
            }

            if (classes != null)
            {
                for (int i = 0; i < classes.Classes.Count; i++)
                {
                    SurvivorsClassDefinition playerClass = classes.Classes[i];
                    if (playerClass != null && !string.IsNullOrWhiteSpace(playerClass.Id) && !passiveAtlasClassIds.Contains(playerClass.Id))
                    {
                        result.AddError($"Class {playerClass.Id} requires a passive atlas progression track.");
                    }
                }
            }
        }

        private static void ValidateWeaponDefinitions(IReadOnlyList<SurvivorsWeaponArchetypeDefinition> weapons, SurvivorsContentValidationResult result)
        {
            var sharedReport = new ContentValidationReport();
            ContentValidation.RequireUniqueIds(weapons, "Survivors weapon", weapon => weapon.Id, sharedReport, requireAtLeastOne: true);
            AddSharedErrors(result, sharedReport);
            if (weapons == null || weapons.Count == 0)
            {
                return;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                SurvivorsWeaponArchetypeDefinition weapon = weapons[i];
                if (weapon == null)
                {
                    continue;
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

            var sharedReport = new ContentValidationReport();
            ContentReferenceSet knownTargets = ContentReferenceSet.From(knownUpgradeTargets, target => target.Value);
            ContentValidation.RequireUniqueIds(upgrades.Definitions, "run upgrade", upgrade => upgrade.Id.Value, sharedReport, requireAtLeastOne: true);
            AddSharedErrors(result, sharedReport);

            for (int i = 0; i < upgrades.Definitions.Count; i++)
            {
                RunUpgradeDefinition upgrade = upgrades.Definitions[i];
                if (upgrade == null)
                {
                    continue;
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

        private static void AddSharedErrors(SurvivorsContentValidationResult result, ContentValidationReport report)
        {
            if (result == null || report == null)
            {
                return;
            }

            string[] errors = report.GetMessages();
            for (int i = 0; i < errors.Length; i++)
            {
                result.AddError(errors[i]);
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

        private static void ValidateUpgradeLibrary(UpgradeLibraryJson library, ClassLibraryJson classLibrary, SurvivorsContentValidationResult result)
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

            HashSet<string> knownClassIds = BuildKnownClassIds(classLibrary);
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

                ValidateAllowedClassIds("Upgrade " + upgrade.id, upgrade.allowedClasses, knownClassIds, result);
            }

            for (int i = 0; i < library.upgrades.Length; i++)
            {
                UpgradeRecordJson upgrade = library.upgrades[i];
                if (upgrade != null && !string.IsNullOrWhiteSpace(upgrade.id))
                {
                    ValidateUpgradeRecordMetadata(upgrade, upgradeIds, result);
                }
            }

            ValidateSampleEvolutionUpgradeCoverage(upgradeIds, result);
        }

        private static void ValidateUpgradeRecordMetadata(
            UpgradeRecordJson upgrade,
            HashSet<string> knownUpgradeIds,
            SurvivorsContentValidationResult result)
        {
            RunUpgradeRarity parsedRarity = default;
            bool rarityValid = !string.IsNullOrWhiteSpace(upgrade.rarity) &&
                Enum.TryParse(upgrade.rarity, ignoreCase: true, out parsedRarity) &&
                Enum.IsDefined(typeof(RunUpgradeRarity), parsedRarity);
            if (!rarityValid)
            {
                result.AddError($"Upgrade {upgrade.id} references unknown rarity: {upgrade.rarity}");
            }

            bool isEvolution = false;
            if (!string.IsNullOrWhiteSpace(upgrade.category))
            {
                if (!Enum.TryParse(upgrade.category, ignoreCase: true, out SurvivorsRunUpgradeCategory parsedCategory) ||
                    !Enum.IsDefined(typeof(SurvivorsRunUpgradeCategory), parsedCategory))
                {
                    result.AddError($"Upgrade {upgrade.id} references unknown category: {upgrade.category}");
                }
                else
                {
                    isEvolution = parsedCategory == SurvivorsRunUpgradeCategory.Evolution;
                }
            }

            ValidateOptionalUpgradeReference(upgrade.id, "required upgrade", upgrade.requiredUpgradeId, knownUpgradeIds, result);
            ValidateOptionalUpgradeReference(upgrade.id, "required passive upgrade", upgrade.requiredPassiveUpgradeId, knownUpgradeIds, result);
            if (upgrade.requiredUpgradeRank < 0)
            {
                result.AddError($"Upgrade {upgrade.id} cannot require a negative upgrade rank.");
            }

            if (!isEvolution)
            {
                return;
            }

            if (parsedRarity != RunUpgradeRarity.Legendary)
            {
                result.AddError($"Evolution upgrade {upgrade.id} must use Legendary rarity.");
            }

            if (string.IsNullOrWhiteSpace(upgrade.requiredUpgradeId))
            {
                result.AddError($"Evolution upgrade {upgrade.id} requires a ranked upgrade prerequisite.");
            }

            if (upgrade.requiredUpgradeRank <= 0)
            {
                result.AddError($"Evolution upgrade {upgrade.id} requires prerequisite rank above zero.");
            }

            if (string.IsNullOrWhiteSpace(upgrade.requiredPassiveUpgradeId))
            {
                result.AddError($"Evolution upgrade {upgrade.id} requires a passive prerequisite.");
            }
        }

        private static void ValidateOptionalUpgradeReference(
            string upgradeId,
            string label,
            string referencedUpgradeId,
            HashSet<string> knownUpgradeIds,
            SurvivorsContentValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(referencedUpgradeId))
            {
                return;
            }

            if (knownUpgradeIds == null || !knownUpgradeIds.Contains(referencedUpgradeId))
            {
                result.AddError($"Upgrade {upgradeId} references unknown {label}: {referencedUpgradeId}");
            }
        }

        private static void ValidateSampleEvolutionUpgradeCoverage(HashSet<string> sampleUpgradeIds, SurvivorsContentValidationResult result)
        {
            IReadOnlyList<SurvivorsRunUpgradeMetadata> metadata = BasicSurvivorsGame.CreateRunUpgradeMetadata();
            for (int i = 0; i < metadata.Count; i++)
            {
                SurvivorsRunUpgradeMetadata entry = metadata[i];
                if (entry == null || !entry.IsEvolution)
                {
                    continue;
                }

                if (sampleUpgradeIds == null || !sampleUpgradeIds.Contains(entry.UpgradeId))
                {
                    result.AddError($"Sample upgrade library missing evolution upgrade: {entry.UpgradeId}");
                }
            }
        }

        private static void ValidateSampleEvolutionProgressionCoverage(HashSet<string> sampleEvolutionNodeUpgradeIds, SurvivorsContentValidationResult result)
        {
            IReadOnlyList<SurvivorsRunUpgradeMetadata> metadata = BasicSurvivorsGame.CreateRunUpgradeMetadata();
            for (int i = 0; i < metadata.Count; i++)
            {
                SurvivorsRunUpgradeMetadata entry = metadata[i];
                if (entry == null || !entry.IsEvolution)
                {
                    continue;
                }

                if (sampleEvolutionNodeUpgradeIds == null || !sampleEvolutionNodeUpgradeIds.Contains(entry.UpgradeId))
                {
                    result.AddError($"Sample progression library missing evolution node: {entry.UpgradeId}");
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

                    float amountPerRank = upgrade.amountPerRank != 0f ? upgrade.amountPerRank : upgrade.damageBonusPerRank;
                    if (amountPerRank <= 0f)
                    {
                        result.AddError($"Persistent upgrade {upgrade.id} amount per rank must be above zero.");
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
                    classRecord.startingWeaponIds,
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

            if (!string.IsNullOrWhiteSpace(library.defaultClassId))
            {
                if (!classIds.Contains(library.defaultClassId))
                {
                    result.AddError($"Sample class library references unknown default class id: {library.defaultClassId}");
                }
                else
                {
                    bool defaultUnlocked = false;
                    for (int i = 0; i < library.classes.Length; i++)
                    {
                        ClassRecordJson classRecord = library.classes[i];
                        if (classRecord != null &&
                            string.Equals(classRecord.id, library.defaultClassId, StringComparison.Ordinal) &&
                            classRecord.unlockedByDefault)
                        {
                            defaultUnlocked = true;
                            break;
                        }
                    }

                    if (!defaultUnlocked)
                    {
                        result.AddError($"Sample class library default class must be unlocked by default: {library.defaultClassId}");
                    }
                }
            }
        }

        private static void ValidateProgressionLibrary(
            ProgressionLibraryJson library,
            UpgradeLibraryJson upgradeLibrary,
            ClassLibraryJson classLibrary,
            SurvivorsContentValidationResult result)
        {
            if (library == null)
            {
                return;
            }

            if (library.tracks == null || library.tracks.Length == 0)
            {
                result.AddError("Sample progression library must contain at least one track.");
                return;
            }

            HashSet<string> knownUpgradeIds = BuildKnownUpgradeIds(upgradeLibrary);
            HashSet<string> knownClassIds = BuildKnownClassIds(classLibrary);
            HashSet<string> knownWeaponIds = BuildKnownWeaponIds(BasicSurvivorsGame.CreateWeaponArchetypeDefinitions());
            var trackIds = new HashSet<string>(StringComparer.Ordinal);
            var nodeIds = new HashSet<string>(StringComparer.Ordinal);
            var passiveAtlasClassIds = new HashSet<string>(StringComparer.Ordinal);
            var evolutionNodeUpgradeIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < library.tracks.Length; i++)
            {
                ProgressionTrackRecordJson track = library.tracks[i];
                if (track == null)
                {
                    result.AddError($"Progression track record at index {i} is null.");
                    continue;
                }

                string resolvedTrackId = string.IsNullOrWhiteSpace(track.id) ? "unknown progression track" : track.id;
                if (string.IsNullOrWhiteSpace(track.id))
                {
                    result.AddError("Progression track record is missing an id.");
                }
                else if (!trackIds.Add(track.id))
                {
                    result.AddError($"Duplicate progression track id: {track.id}");
                }

                ValidateProgressionTrackOwnership(
                    resolvedTrackId,
                    track.kind,
                    track.classId,
                    track.targetWeaponId,
                    knownClassIds,
                    knownWeaponIds,
                    passiveAtlasClassIds,
                    result);

                if (track.nodes == null || track.nodes.Length == 0)
                {
                    result.AddError($"Progression track {resolvedTrackId} requires at least one node.");
                    continue;
                }

                for (int nodeIndex = 0; nodeIndex < track.nodes.Length; nodeIndex++)
                {
                    ProgressionNodeRecordJson node = track.nodes[nodeIndex];
                    if (node == null)
                    {
                        result.AddError($"Progression track {resolvedTrackId} node at index {nodeIndex} is null.");
                        continue;
                    }

                    ValidateProgressionNodeRecord(
                        resolvedTrackId,
                        node.id,
                        node.upgradeId,
                        node.kind,
                        node.tier,
                        node.pointCost,
                        node.maxRank,
                        knownUpgradeIds,
                        nodeIds,
                        result);

                    if (!string.IsNullOrWhiteSpace(node.kind) &&
                        Enum.TryParse(node.kind, ignoreCase: true, out SurvivorsProgressionNodeKind parsedKind) &&
                        parsedKind == SurvivorsProgressionNodeKind.Evolution &&
                        !string.IsNullOrWhiteSpace(node.upgradeId))
                    {
                        evolutionNodeUpgradeIds.Add(node.upgradeId);
                    }
                }
            }

            if (classLibrary != null && classLibrary.classes != null)
            {
                for (int i = 0; i < classLibrary.classes.Length; i++)
                {
                    ClassRecordJson playerClass = classLibrary.classes[i];
                    if (playerClass != null && !string.IsNullOrWhiteSpace(playerClass.id) && !passiveAtlasClassIds.Contains(playerClass.id))
                    {
                        result.AddError($"Class {playerClass.id} requires a passive atlas progression track.");
                    }
                }
            }

            ValidateSampleEvolutionProgressionCoverage(evolutionNodeUpgradeIds, result);
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

                if (role == SurvivorsEnemyRole.Splitter)
                {
                    if (enemy.splitChildCount <= 0)
                    {
                        result.AddError($"Enemy {enemy.id} requires split child count above zero.");
                    }

                    if (enemy.splitChildRadius <= 0f)
                    {
                        result.AddError($"Enemy {enemy.id} requires split child radius above zero.");
                    }

                    if (string.IsNullOrWhiteSpace(enemy.behavior))
                    {
                        result.AddError($"Enemy {enemy.id} requires splitter behavior text.");
                    }
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

        private static void ValidatePickupLibrary(PickupLibraryJson library, SurvivorsContentValidationResult result)
        {
            if (library == null)
            {
                return;
            }

            if (library.pickups == null || library.pickups.Length == 0)
            {
                result.AddError("Sample pickup library must contain at least one pickup.");
                return;
            }

            var pickupIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < library.pickups.Length; i++)
            {
                PickupRecordJson pickup = library.pickups[i];
                if (pickup == null || string.IsNullOrWhiteSpace(pickup.id))
                {
                    result.AddError($"Pickup record at index {i} is missing an id.");
                    continue;
                }

                if (!pickupIds.Add(pickup.id))
                {
                    result.AddError($"Duplicate pickup id: {pickup.id}");
                }

                if (string.IsNullOrWhiteSpace(pickup.displayName))
                {
                    result.AddError($"Pickup {pickup.id} is missing a display name.");
                }

                if (pickup.attractRange < 0f)
                {
                    result.AddError($"Pickup {pickup.id} cannot use negative attract range.");
                }

                if (pickup.attractionSpeed < 0f)
                {
                    result.AddError($"Pickup {pickup.id} cannot use negative attraction speed.");
                }

                if (pickup.id == BasicSurvivorsGame.ExperiencePickupSpawnableId.Value)
                {
                    if (pickup.attractRange <= 0f)
                    {
                        result.AddError($"Pickup {pickup.id} requires attract range above zero.");
                    }

                    if (pickup.attractionSpeed <= 0f)
                    {
                        result.AddError($"Pickup {pickup.id} requires attraction speed above zero.");
                    }
                }
                else if (string.IsNullOrWhiteSpace(pickup.behavior))
                {
                    result.AddError($"Pickup {pickup.id} requires behavior text.");
                }
            }

            RequirePickupId(pickupIds, BasicSurvivorsGame.ExperiencePickupSpawnableId.Value, result);
            RequirePickupId(pickupIds, BasicSurvivorsGame.MagnetPickupSpawnableId.Value, result);
            RequirePickupId(pickupIds, BasicSurvivorsGame.HealthPickupSpawnableId.Value, result);
            RequirePickupId(pickupIds, BasicSurvivorsGame.BloodShardPickupSpawnableId.Value, result);
        }

        private static void ValidateRunFlowLibrary(RunFlowLibraryJson library, SurvivorsContentValidationResult result)
        {
            if (library == null)
            {
                return;
            }

            if (library.profiles == null || library.profiles.Length == 0)
            {
                result.AddError("Sample run flow library must contain at least one pacing profile.");
                return;
            }

            var profileIds = new HashSet<string>(StringComparer.Ordinal);
            bool containsHumanPlaytest = false;
            for (int i = 0; i < library.profiles.Length; i++)
            {
                RunFlowProfileRecordJson profile = library.profiles[i];
                if (profile == null)
                {
                    result.AddError($"Run flow profile at index {i} is null.");
                    continue;
                }

                string resolvedId = string.IsNullOrWhiteSpace(profile.id) ? "unknown profile" : profile.id;
                if (string.IsNullOrWhiteSpace(profile.id))
                {
                    result.AddError($"Run flow profile at index {i} is missing an id.");
                }
                else if (!profileIds.Add(profile.id))
                {
                    result.AddError($"Duplicate run flow profile id: {profile.id}");
                }

                if (string.Equals(profile.id, SurvivorsPacingProfile.HumanPlaytest.ToString(), StringComparison.Ordinal))
                {
                    containsHumanPlaytest = true;
                }

                ValidateRunFlowDefinition(CreateRunFlowDefinition(profile), result);
                ValidateRunFlowPacingRecord(profile, resolvedId, result);
            }

            if (!containsHumanPlaytest)
            {
                result.AddError("Sample run flow library must include a HumanPlaytest profile.");
            }
        }

        private static SurvivorsRunFlowDefinition CreateRunFlowDefinition(RunFlowProfileRecordJson profile)
        {
            SurvivorsTemplateTuning tuning = BasicSurvivorsGame.CreateDefaultTuning();
            return new SurvivorsRunFlowDefinition(
                profile.escalationIntervalSeconds,
                profile.minimumEnemySpawnIntervalSeconds,
                profile.enemySpawnIntervalReductionPerEscalation,
                profile.enemyMaximumAliveIncreasePerEscalation,
                profile.enemyHealthMultiplierPerEscalation,
                profile.enemyMoveSpeedMultiplierPerEscalation,
                profile.enemyExperienceMultiplierPerEscalation,
                profile.minibossSpawnTimeSeconds,
                BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Miniboss, tuning),
                profile.bossSpawnTimeSeconds,
                BasicSurvivorsGame.CreateEnemyProfile(SurvivorsEnemyRole.Boss, tuning),
                profile.survivalVictoryTimeSeconds,
                BasicSurvivorsGame.CreateEnemyProfileDefinitions(tuning),
                profile.firstEliteSpawnTimeSeconds,
                profile.eliteSpawnIntervalSeconds,
                profile.firstDreadEliteSpawnTimeSeconds,
                profile.dreadEliteSpawnIntervalSeconds);
        }

        private static void ValidateRunFlowPacingRecord(RunFlowProfileRecordJson profile, string profileId, SurvivorsContentValidationResult result)
        {
            ValidatePositive(profileId, "enemy spawn interval", profile.enemySpawnIntervalSeconds, result);
            ValidatePositive(profileId, "enemy maximum alive", profile.enemyMaximumAlive, result);
            ValidatePositive(profileId, "enemy spawn pack base count", profile.enemySpawnPackBaseCount, result);
            ValidatePositive(profileId, "enemy spawn pack max count", profile.enemySpawnPackMaxCount, result);
            ValidatePositive(profileId, "enemy ranged dodge XP reward", profile.enemyRangedAttackDodgeExperienceReward, result);
            if (profile.enemySpawnPackMaxCount < profile.enemySpawnPackBaseCount)
            {
                result.AddError($"Run flow profile {profileId} max spawn pack count must be at least the base spawn pack count.");
            }

            ValidatePositive(profileId, "horde rush first time", profile.hordeRushFirstTimeSeconds, result);
            ValidatePositive(profileId, "horde rush interval", profile.hordeRushIntervalSeconds, result);
            ValidatePositive(profileId, "horde rush warning lead", profile.hordeRushWarningLeadSeconds, result);
            ValidatePositive(profileId, "horde rush base enemy count", profile.hordeRushBaseEnemyCount, result);
            ValidatePositive(profileId, "horde rush enemy count increase", profile.hordeRushEnemyCountIncreasePerRush, result);
            ValidatePositive(profileId, "horde rush max enemy count", profile.hordeRushMaxEnemyCount, result);
            if (profile.hordeRushMaxEnemyCount < profile.hordeRushBaseEnemyCount)
            {
                result.AddError($"Run flow profile {profileId} horde rush max enemy count must be at least the base count.");
            }

            ValidatePositive(profileId, "horde rush extra alive allowance", profile.hordeRushExtraAliveAllowance, result);
            ValidatePositive(profileId, "horde rush spawn radius", profile.hordeRushSpawnRadius, result);
            ValidatePositive(profileId, "horde rush clear XP gem count", profile.hordeRushClearExperienceGemCount, result);
            ValidatePositive(profileId, "horde rush clear XP multiplier", profile.hordeRushClearExperienceMultiplier, result);
            ValidatePositive(profileId, "horde rush clear magnet cadence", profile.hordeRushClearMagnetEveryRush, result);
            ValidatePositive(profileId, "horde rush clear blood shard cadence", profile.hordeRushClearBloodShardEveryRush, result);
            if (profile.hordeRushClearBloodShardEveryRush <= profile.hordeRushClearMagnetEveryRush)
            {
                result.AddError($"Run flow profile {profileId} horde rush clear blood shard cadence must be longer than magnet cadence.");
            }

            ValidatePositive(profileId, "horde rush clear pulse damage", profile.hordeRushClearPulseDamage, result);
            ValidatePositive(profileId, "horde rush clear pulse radius", profile.hordeRushClearPulseRadius, result);
            ValidatePositive(profileId, "roaming cache travel interval", profile.roamingCacheTravelInterval, result);
            ValidatePositive(profileId, "roaming cache XP gem count", profile.roamingCacheExperienceGemCount, result);
            ValidatePositive(profileId, "roaming cache magnet interval", profile.roamingCacheMagnetInterval, result);
            ValidatePositive(profileId, "roaming cache blood shard interval", profile.roamingCacheBloodShardInterval, result);
            if (profile.roamingCacheBloodShardInterval <= profile.roamingCacheMagnetInterval)
            {
                result.AddError($"Run flow profile {profileId} roaming cache blood shard interval must be longer than magnet interval.");
            }

            ValidatePositive(profileId, "roaming cache ambush start cache", profile.roamingCacheAmbushStartCache, result);
            ValidatePositive(profileId, "roaming cache ambush interval", profile.roamingCacheAmbushInterval, result);
            ValidatePositive(profileId, "roaming cache ambush base enemy count", profile.roamingCacheAmbushBaseEnemyCount, result);
            ValidatePositive(profileId, "roaming cache ambush max enemy count", profile.roamingCacheAmbushMaxEnemyCount, result);
            if (profile.roamingCacheAmbushMaxEnemyCount < profile.roamingCacheAmbushBaseEnemyCount)
            {
                result.AddError($"Run flow profile {profileId} roaming cache ambush max enemy count must be at least the base count.");
            }

            ValidatePositive(profileId, "roaming cache ambush extra alive allowance", profile.roamingCacheAmbushExtraAliveAllowance, result);
            ValidatePositive(profileId, "roaming cache ambush radius", profile.roamingCacheAmbushRadius, result);
            ValidatePositive(profileId, "roaming cache ambush clear magnet interval", profile.roamingCacheAmbushClearMagnetInterval, result);
            ValidatePositive(profileId, "roaming cache ambush clear blood shard interval", profile.roamingCacheAmbushClearBloodShardInterval, result);
            ValidatePositive(profileId, "roaming cache surge interval", profile.roamingCacheSurgeInterval, result);
            ValidatePositive(profileId, "roaming cache surge bonus XP gem count", profile.roamingCacheSurgeBonusGemCount, result);
            ValidatePositive(profileId, "roaming cache surge duration", profile.roamingCacheSurgeDurationSeconds, result);
            ValidatePositive(profileId, "roaming cache surge damage bonus", profile.roamingCacheSurgeDamageBonus, result);
            ValidatePositive(profileId, "roaming cache surge move speed bonus", profile.roamingCacheSurgeMoveSpeedBonus, result);
            if (profile.roamingCacheSurgeCooldownMultiplierBonus >= 0f || profile.roamingCacheSurgeCooldownMultiplierBonus <= -0.75f)
            {
                result.AddError($"Run flow profile {profileId} roaming cache surge cooldown multiplier must be below zero and above -0.75.");
            }

            ValidatePositive(profileId, "roaming cache surge pickup range bonus", profile.roamingCacheSurgePickupRangeBonus, result);
            ValidatePositive(profileId, "roaming cache surge pulse damage", profile.roamingCacheSurgePulseDamage, result);
            ValidatePositive(profileId, "roaming cache surge pulse radius", profile.roamingCacheSurgePulseRadius, result);
            ValidatePositive(profileId, "draft choice count", profile.draftChoiceCount, result);
            ValidatePositive(profileId, "max weapon slots", profile.maxWeaponSlots, result);
            ValidatePositive(profileId, "max passive slots", profile.maxPassiveSlots, result);
            ValidatePositive(profileId, "draft mid rarity level", profile.draftMidRarityLevel, result);
            ValidatePositive(profileId, "draft late rarity level", profile.draftLateRarityLevel, result);
            if (profile.draftLateRarityLevel <= profile.draftMidRarityLevel)
            {
                result.AddError($"Run flow profile {profileId} late rarity level must be after mid rarity level.");
            }

            ValidateRunFlowRarityTables(profile.rarityTables, profileId, result);

            ValidatePositive(profileId, "endless elite interval", profile.endlessEliteSpawnIntervalSeconds, result);
            ValidatePositive(profileId, "endless miniboss interval", profile.endlessMinibossSpawnIntervalSeconds, result);
            ValidatePositive(profileId, "endless boss interval", profile.endlessBossSpawnIntervalSeconds, result);
            if (profile.endlessMinibossSpawnIntervalSeconds <= profile.endlessEliteSpawnIntervalSeconds)
            {
                result.AddError($"Run flow profile {profileId} endless miniboss interval must be longer than elite interval.");
            }

            if (profile.endlessBossSpawnIntervalSeconds <= profile.endlessMinibossSpawnIntervalSeconds)
            {
                result.AddError($"Run flow profile {profileId} endless boss interval must be longer than miniboss interval.");
            }
        }

        private static void ValidateRunFlowRarityTables(
            RarityTableRecordJson[] rarityTables,
            string profileId,
            SurvivorsContentValidationResult result)
        {
            if (rarityTables == null || rarityTables.Length == 0)
            {
                result.AddError($"Run flow profile {profileId} must define rarity tables for draft odds.");
                return;
            }

            var tableIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < rarityTables.Length; i++)
            {
                RarityTableRecordJson table = rarityTables[i];
                if (table == null)
                {
                    result.AddError($"Run flow profile {profileId} rarity table at index {i} is null.");
                    continue;
                }

                string tableId = string.IsNullOrWhiteSpace(table.id) ? $"index {i}" : table.id;
                if (string.IsNullOrWhiteSpace(table.id))
                {
                    result.AddError($"Run flow profile {profileId} rarity table at index {i} is missing an id.");
                }
                else if (!tableIds.Add(table.id))
                {
                    result.AddError($"Run flow profile {profileId} has duplicate rarity table id: {table.id}");
                }

                ValidateRarityTableWeight(profileId, tableId, "common", table.common, result);
                ValidateRarityTableWeight(profileId, tableId, "uncommon", table.uncommon, result);
                ValidateRarityTableWeight(profileId, tableId, "rare", table.rare, result);
                ValidateRarityTableWeight(profileId, tableId, "epic", table.epic, result);
                ValidateRarityTableWeight(profileId, tableId, "legendary", table.legendary, result);

                long totalWeight =
                    (long)table.common +
                    table.uncommon +
                    table.rare +
                    table.epic +
                    table.legendary;
                if (totalWeight <= 0)
                {
                    result.AddError($"Run flow profile {profileId} rarity table {tableId} requires at least one positive weight.");
                }

                ValidateRarityTableCurve(profileId, table, tableId, result);
            }

            for (int i = 0; i < RequiredRunFlowRarityTableIds.Length; i++)
            {
                string requiredId = RequiredRunFlowRarityTableIds[i];
                if (!tableIds.Contains(requiredId))
                {
                    result.AddError($"Run flow profile {profileId} is missing rarity table {requiredId}.");
                }
            }
        }

        private static void ValidateRarityTableCurve(
            string profileId,
            RarityTableRecordJson table,
            string tableId,
            SurvivorsContentValidationResult result)
        {
            switch (table.id)
            {
                case "NormalEarly":
                    RequirePositiveRarityWeight(profileId, tableId, "common", table.common, result);
                    RequirePositiveRarityWeight(profileId, tableId, "uncommon", table.uncommon, result);
                    RequirePositiveRarityWeight(profileId, tableId, "rare", table.rare, result);
                    RequireZeroRarityWeight(profileId, tableId, "legendary", table.legendary, result);
                    break;
                case "NormalMid":
                    RequirePositiveRarityWeight(profileId, tableId, "uncommon", table.uncommon, result);
                    RequirePositiveRarityWeight(profileId, tableId, "rare", table.rare, result);
                    RequirePositiveRarityWeight(profileId, tableId, "epic", table.epic, result);
                    RequireZeroRarityWeight(profileId, tableId, "legendary", table.legendary, result);
                    break;
                case "NormalLate":
                    RequirePositiveRarityWeight(profileId, tableId, "rare", table.rare, result);
                    RequirePositiveRarityWeight(profileId, tableId, "epic", table.epic, result);
                    RequirePositiveRarityWeight(profileId, tableId, "legendary", table.legendary, result);
                    break;
                case "Elite":
                    RequireZeroRarityWeight(profileId, tableId, "common", table.common, result);
                    RequirePositiveRarityWeight(profileId, tableId, "uncommon", table.uncommon, result);
                    RequirePositiveRarityWeight(profileId, tableId, "rare", table.rare, result);
                    RequirePositiveRarityWeight(profileId, tableId, "epic", table.epic, result);
                    RequirePositiveRarityWeight(profileId, tableId, "legendary", table.legendary, result);
                    break;
                case "Boss":
                    RequireZeroRarityWeight(profileId, tableId, "common", table.common, result);
                    RequireZeroRarityWeight(profileId, tableId, "uncommon", table.uncommon, result);
                    RequirePositiveRarityWeight(profileId, tableId, "rare", table.rare, result);
                    RequirePositiveRarityWeight(profileId, tableId, "epic", table.epic, result);
                    RequirePositiveRarityWeight(profileId, tableId, "legendary", table.legendary, result);
                    break;
            }
        }

        private static void ValidateRarityTableWeight(
            string profileId,
            string tableId,
            string rarity,
            int weight,
            SurvivorsContentValidationResult result)
        {
            if (weight < 0)
            {
                result.AddError($"Run flow profile {profileId} rarity table {tableId} cannot use negative {rarity} weight.");
            }
        }

        private static void RequirePositiveRarityWeight(
            string profileId,
            string tableId,
            string rarity,
            int weight,
            SurvivorsContentValidationResult result)
        {
            if (weight <= 0)
            {
                result.AddError($"Run flow profile {profileId} rarity table {tableId} requires {rarity} weight above zero.");
            }
        }

        private static void RequireZeroRarityWeight(
            string profileId,
            string tableId,
            string rarity,
            int weight,
            SurvivorsContentValidationResult result)
        {
            if (weight != 0)
            {
                result.AddError($"Run flow profile {profileId} rarity table {tableId} requires {rarity} weight to be zero.");
            }
        }

        private static void ValidatePositive(string profileId, string label, float value, SurvivorsContentValidationResult result)
        {
            if (value <= 0f)
            {
                result.AddError($"Run flow profile {profileId} requires {label} above zero.");
            }
        }

        private static void ValidatePositive(string profileId, string label, int value, SurvivorsContentValidationResult result)
        {
            if (value <= 0)
            {
                result.AddError($"Run flow profile {profileId} requires {label} above zero.");
            }
        }

        private static void RequirePickupId(HashSet<string> pickupIds, string requiredId, SurvivorsContentValidationResult result)
        {
            if (pickupIds == null || string.IsNullOrWhiteSpace(requiredId) || !pickupIds.Contains(requiredId))
            {
                result.AddError($"Sample pickup library is missing required pickup id: {requiredId}");
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
            IReadOnlyList<string> startingWeaponIds,
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

            ValidateClassStartingWeaponIds(resolvedId, startingWeaponIds, knownWeaponIds, result);

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
            string[] startingWeaponIds,
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

            ValidateClassStartingWeaponIds(resolvedId, startingWeaponIds, knownWeaponIds, result);

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

        private static void ValidateClassStartingWeaponIds(
            string classId,
            IReadOnlyList<string> startingWeaponIds,
            HashSet<string> knownWeaponIds,
            SurvivorsContentValidationResult result)
        {
            if (startingWeaponIds == null || startingWeaponIds.Count == 0)
            {
                result.AddError($"Class {classId} requires at least one starting weapon in its loadout.");
                return;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < startingWeaponIds.Count; i++)
            {
                string weaponId = startingWeaponIds[i];
                if (string.IsNullOrWhiteSpace(weaponId))
                {
                    result.AddError($"Class {classId} starting weapon loadout contains an empty id.");
                    continue;
                }

                if (!seen.Add(weaponId))
                {
                    result.AddError($"Class {classId} has duplicate starting weapon id: {weaponId}");
                }

                if (knownWeaponIds == null || !knownWeaponIds.Contains(weaponId))
                {
                    result.AddError($"Class {classId} references unknown loadout weapon: {weaponId}");
                }
            }
        }

        private static void ValidateClassUpgradeGateRecord(
            string upgradeId,
            IReadOnlyList<string> allowedClassIds,
            HashSet<string> knownUpgradeIds,
            HashSet<string> knownClassIds,
            SurvivorsContentValidationResult result)
        {
            string resolvedId = string.IsNullOrWhiteSpace(upgradeId) ? "unknown upgrade" : upgradeId;
            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                result.AddError("Class upgrade gate is missing an upgrade id.");
            }
            else if (knownUpgradeIds == null || !knownUpgradeIds.Contains(upgradeId))
            {
                result.AddError($"Class upgrade gate references unknown upgrade id: {upgradeId}");
            }

            ValidateAllowedClassIds("Class upgrade gate " + resolvedId, allowedClassIds, knownClassIds, result);
        }

        private static void ValidateAllowedClassIds(
            string label,
            IReadOnlyList<string> allowedClassIds,
            HashSet<string> knownClassIds,
            SurvivorsContentValidationResult result)
        {
            if (allowedClassIds == null || allowedClassIds.Count == 0)
            {
                return;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < allowedClassIds.Count; i++)
            {
                string classId = allowedClassIds[i];
                if (string.IsNullOrWhiteSpace(classId))
                {
                    result.AddError($"{label} allowed class list contains an empty id.");
                    continue;
                }

                if (!seen.Add(classId))
                {
                    result.AddError($"{label} allowed class list contains duplicate id: {classId}");
                }

                if (knownClassIds == null || !knownClassIds.Contains(classId))
                {
                    result.AddError($"{label} references unknown class id: {classId}");
                }
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

        private static Dictionary<string, int> BuildKnownUpgradeMaxRanks(RunUpgradeCatalog upgrades)
        {
            var ranks = new Dictionary<string, int>(StringComparer.Ordinal);
            if (upgrades == null)
            {
                return ranks;
            }

            for (int i = 0; i < upgrades.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = upgrades.Definitions[i];
                if (definition != null && !string.IsNullOrWhiteSpace(definition.Id.Value))
                {
                    ranks[definition.Id.Value] = definition.MaxRank;
                }
            }

            return ranks;
        }

        private static HashSet<string> BuildKnownUpgradeIds(UpgradeLibraryJson library)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (library == null || library.upgrades == null)
            {
                return ids;
            }

            for (int i = 0; i < library.upgrades.Length; i++)
            {
                UpgradeRecordJson upgrade = library.upgrades[i];
                if (upgrade != null && !string.IsNullOrWhiteSpace(upgrade.id))
                {
                    ids.Add(upgrade.id);
                }
            }

            return ids;
        }

        private static void ValidateProgressionTrackOwnership(
            string trackId,
            string kind,
            string classId,
            string targetWeaponId,
            HashSet<string> knownClassIds,
            HashSet<string> knownWeaponIds,
            HashSet<string> passiveAtlasClassIds,
            SurvivorsContentValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(kind) ||
                !Enum.TryParse(kind, ignoreCase: true, out SurvivorsProgressionTrackKind parsedKind) ||
                !Enum.IsDefined(typeof(SurvivorsProgressionTrackKind), parsedKind))
            {
                result.AddError($"Progression track {trackId} references unknown kind: {kind}");
                return;
            }

            if (parsedKind == SurvivorsProgressionTrackKind.PassiveAtlas)
            {
                if (string.IsNullOrWhiteSpace(classId))
                {
                    result.AddError($"Passive atlas {trackId} requires a class id.");
                }
                else if (knownClassIds == null || !knownClassIds.Contains(classId))
                {
                    result.AddError($"Passive atlas {trackId} references unknown class id: {classId}");
                }
                else if (passiveAtlasClassIds != null && !passiveAtlasClassIds.Add(classId))
                {
                    result.AddError($"Class {classId} has more than one passive atlas progression track.");
                }

                if (!string.IsNullOrWhiteSpace(targetWeaponId))
                {
                    result.AddError($"Passive atlas {trackId} should not target a weapon.");
                }

                return;
            }

            if (parsedKind == SurvivorsProgressionTrackKind.WeaponSkillTrack)
            {
                if (string.IsNullOrWhiteSpace(targetWeaponId) || knownWeaponIds == null || !knownWeaponIds.Contains(targetWeaponId))
                {
                    result.AddError($"Weapon skill track {trackId} references unknown target weapon: {targetWeaponId}");
                }

                if (!string.IsNullOrWhiteSpace(classId) && (knownClassIds == null || !knownClassIds.Contains(classId)))
                {
                    result.AddError($"Weapon skill track {trackId} references unknown class id: {classId}");
                }
            }
        }

        private static void ValidateProgressionNodeRecord(
            string trackId,
            string nodeId,
            string upgradeId,
            string kind,
            int tier,
            int pointCost,
            int maxRank,
            Dictionary<string, int> knownUpgradeMaxRanks,
            HashSet<string> nodeIds,
            SurvivorsContentValidationResult result)
        {
            ValidateProgressionNodeBasics(trackId, nodeId, upgradeId, kind, tier, pointCost, maxRank, nodeIds, result);
            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                return;
            }

            if (knownUpgradeMaxRanks == null || !knownUpgradeMaxRanks.TryGetValue(upgradeId, out int upgradeMaxRank))
            {
                result.AddError($"Progression node {nodeId} references unknown upgrade id: {upgradeId}");
            }
            else if (maxRank > upgradeMaxRank)
            {
                result.AddError($"Progression node {nodeId} max rank exceeds upgrade {upgradeId} max rank.");
            }
        }

        private static void ValidateProgressionNodeRecord(
            string trackId,
            string nodeId,
            string upgradeId,
            string kind,
            int tier,
            int pointCost,
            int maxRank,
            HashSet<string> knownUpgradeIds,
            HashSet<string> nodeIds,
            SurvivorsContentValidationResult result)
        {
            ValidateProgressionNodeBasics(trackId, nodeId, upgradeId, kind, tier, pointCost, maxRank, nodeIds, result);
            if (!string.IsNullOrWhiteSpace(upgradeId) && (knownUpgradeIds == null || !knownUpgradeIds.Contains(upgradeId)))
            {
                result.AddError($"Progression node {nodeId} references unknown upgrade id: {upgradeId}");
            }
        }

        private static void ValidateProgressionNodeBasics(
            string trackId,
            string nodeId,
            string upgradeId,
            string kind,
            int tier,
            int pointCost,
            int maxRank,
            HashSet<string> nodeIds,
            SurvivorsContentValidationResult result)
        {
            string resolvedNodeId = string.IsNullOrWhiteSpace(nodeId) ? "unknown progression node" : nodeId;
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                result.AddError($"Progression track {trackId} contains a node with no id.");
            }
            else if (nodeIds != null && !nodeIds.Add(nodeId))
            {
                result.AddError($"Duplicate progression node id: {nodeId}");
            }

            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                result.AddError($"Progression node {resolvedNodeId} is missing an upgrade id.");
            }

            if (string.IsNullOrWhiteSpace(kind) ||
                !Enum.TryParse(kind, ignoreCase: true, out SurvivorsProgressionNodeKind parsedKind) ||
                !Enum.IsDefined(typeof(SurvivorsProgressionNodeKind), parsedKind))
            {
                result.AddError($"Progression node {resolvedNodeId} references unknown kind: {kind}");
            }

            if (tier < 0)
            {
                result.AddError($"Progression node {resolvedNodeId} cannot use a negative tier.");
            }

            if (pointCost <= 0)
            {
                result.AddError($"Progression node {resolvedNodeId} requires point cost above zero.");
            }

            if (maxRank <= 0)
            {
                result.AddError($"Progression node {resolvedNodeId} requires max rank above zero.");
            }
        }

        private static HashSet<string> BuildKnownClassIds(SurvivorsClassLibraryDefinition classes)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (classes == null)
            {
                return ids;
            }

            for (int i = 0; i < classes.Classes.Count; i++)
            {
                SurvivorsClassDefinition definition = classes.Classes[i];
                if (definition != null && !string.IsNullOrWhiteSpace(definition.Id))
                {
                    ids.Add(definition.Id);
                }
            }

            return ids;
        }

        private static HashSet<string> BuildKnownClassIds(ClassLibraryJson classLibrary)
        {
            if (classLibrary == null)
            {
                return BuildKnownClassIds(BasicSurvivorsGame.CreateClassLibraryDefinition());
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (classLibrary.classes == null)
            {
                return ids;
            }

            for (int i = 0; i < classLibrary.classes.Length; i++)
            {
                ClassRecordJson classRecord = classLibrary.classes[i];
                if (classRecord != null && !string.IsNullOrWhiteSpace(classRecord.id))
                {
                    ids.Add(classRecord.id);
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
            public string displayName;
            public string category;
            public string rarity;
            public string effect;
            public string target;
            public string description;
            public string requiredUpgradeId;
            public int requiredUpgradeRank;
            public string requiredPassiveUpgradeId;
            public string[] allowedClasses;
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
            public int splitChildCount;
            public float splitChildRadius;
            public string behavior;
            public bool finalBoss;
        }

        [Serializable]
        private sealed class PickupLibraryJson
        {
            public PickupRecordJson[] pickups;
        }

        [Serializable]
        private sealed class PickupRecordJson
        {
            public string id;
            public string displayName;
            public float attractRange;
            public float attractionSpeed;
            public string behavior;
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
        private sealed class RunFlowLibraryJson
        {
            public RunFlowProfileRecordJson[] profiles;
        }

        [Serializable]
        private sealed class RunFlowProfileRecordJson
        {
            public string id;
            public string displayName;
            public float enemySpawnIntervalSeconds;
            public int enemyMaximumAlive;
            public int enemySpawnPackBaseCount;
            public int enemySpawnPackMaxCount;
            public int enemyRangedAttackDodgeExperienceReward;
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
            public int draftChoiceCount;
            public int maxWeaponSlots;
            public int maxPassiveSlots;
            public int draftMidRarityLevel;
            public int draftLateRarityLevel;
            public RarityTableRecordJson[] rarityTables;
            public float endlessEliteSpawnIntervalSeconds;
            public float endlessMinibossSpawnIntervalSeconds;
            public float endlessBossSpawnIntervalSeconds;
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
    }
}
