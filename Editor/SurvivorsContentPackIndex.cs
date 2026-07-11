using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.GameplayFoundation;
using UnityEditor;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    public sealed class SurvivorsContentPackIndex
    {
        public const string WeaponsSourceId = "weapons";
        public const string UpgradesSourceId = "upgrades";
        public const string EnemiesSourceId = "enemies";
        public const string RewardsSourceId = "rewards";
        public const string RelicsSourceId = "relics";
        public const string ClassesSourceId = "classes";
        public const string ProgressionSourceId = "progression";
        public const string PickupsSourceId = "pickups";
        public const string RunFlowSourceId = "run-flow";
        public const string PrimaryThemeSourceId = "theme.primary";
        public const string AlternateThemeSourceId = "theme.alternate";

        private static readonly string[] RequiredSourceIdsInternal =
        {
            WeaponsSourceId,
            UpgradesSourceId,
            EnemiesSourceId,
            RewardsSourceId,
            RelicsSourceId,
            ClassesSourceId,
            ProgressionSourceId,
            PickupsSourceId,
            RunFlowSourceId,
            PrimaryThemeSourceId,
            AlternateThemeSourceId
        };

        private static readonly CategoryDefinition[] CategoryDefinitions =
        {
            new CategoryDefinition("weapons", "Weapons", "Authored weapon archetypes and presentation metadata."),
            new CategoryDefinition("projectiles", "Projectiles", "Authored projectile movement and lifetime records."),
            new CategoryDefinition("upgrades", "Upgrades", "All draftable authored upgrade records."),
            new CategoryDefinition("passives", "Passives", "Passive build options."),
            new CategoryDefinition("pickup-magnet", "Pickup / Magnet", "Pickup behaviors and pickup or magnet build options."),
            new CategoryDefinition("mutations", "Mutations", "Weapon and build mutations."),
            new CategoryDefinition("evolutions", "Evolutions", "Evolution recipes and rewards."),
            new CategoryDefinition("relics", "Relics", "Boss and run relic definitions."),
            new CategoryDefinition("classes", "Classes", "Playable classes and starting loadouts."),
            new CategoryDefinition("enemies", "Enemies", "All authored enemy roles."),
            new CategoryDefinition("elites", "Elites", "Elite and dread-elite enemy records."),
            new CategoryDefinition("minibosses", "Minibosses", "Miniboss enemy records."),
            new CategoryDefinition("bosses", "Bosses", "Boss and final-boss enemy records."),
            new CategoryDefinition("run-profiles", "Run Profiles", "Authored Standard, Sprint, debug, and showcase profiles."),
            new CategoryDefinition("waves-milestones", "Waves / Milestones", "Threat, horde, boss, and victory timing views of run profiles."),
            new CategoryDefinition("rewards", "Rewards", "Currencies, tracks, and reward grants."),
            new CategoryDefinition("meta-upgrades", "Meta Upgrades", "Persistent upgrade records."),
            new CategoryDefinition("progression", "Progression", "Progression tracks and nodes."),
            new CategoryDefinition("themes", "Themes", "Authored UI and world presentation themes."),
            new CategoryDefinition("audio-events", "Audio Events", "Authored theme audio palette events."),
            new CategoryDefinition("tutorial", "Tutorial", "Authored tutorial copy and steps.")
        };

        private SurvivorsContentPackIndex(
            GameContentPackManifest manifest,
            IReadOnlyList<GameContentRecordDescriptor> records,
            IReadOnlyList<GameContentCategoryDescriptor> categories,
            GameContentAuthoringValidationResult validation)
        {
            Manifest = manifest;
            Records = records ?? Array.Empty<GameContentRecordDescriptor>();
            Categories = categories ?? Array.Empty<GameContentCategoryDescriptor>();
            Validation = validation ?? GameContentAuthoringValidationResult.Valid;
        }

        public GameContentPackManifest Manifest { get; }
        public IReadOnlyList<GameContentRecordDescriptor> Records { get; }
        public IReadOnlyList<GameContentCategoryDescriptor> Categories { get; }
        public GameContentAuthoringValidationResult Validation { get; }
        public static IReadOnlyList<string> RequiredSourceIds => RequiredSourceIdsInternal;

        public static SurvivorsContentPackIndex Build(GameContentPackManifest manifest)
        {
            var issues = new List<GameContentAuthoringValidationIssue>();
            if (manifest == null)
            {
                issues.Add(GameContentAuthoringValidationIssue.Error("Manifest", "A Survivors content-pack manifest is required."));
                return new SurvivorsContentPackIndex(
                    null,
                    Array.Empty<GameContentRecordDescriptor>(),
                    BuildCategories(Array.Empty<GameContentRecordDescriptor>()),
                    new GameContentAuthoringValidationResult(issues));
            }

            GameContentAuthoringValidationResult selectedValidation = ValidateSelectedSources(manifest);
            AppendIssues(issues, selectedValidation.Issues);
            var builders = new List<RecordBuilder>();
            var rawIds = new Dictionary<string, List<RecordBuilder>>(StringComparer.OrdinalIgnoreCase);
            int order = 0;

            WeaponLibraryJson weapons = Parse<WeaponLibraryJson>(manifest, WeaponsSourceId, issues);
            AddWeapons(manifest, weapons, builders, rawIds, ref order);

            UpgradeLibraryJson upgrades = Parse<UpgradeLibraryJson>(manifest, UpgradesSourceId, issues);
            AddUpgrades(manifest, upgrades, builders, rawIds, ref order);

            EnemyLibraryJson enemies = Parse<EnemyLibraryJson>(manifest, EnemiesSourceId, issues);
            AddEnemies(manifest, enemies, builders, rawIds, ref order);

            PickupLibraryJson pickups = Parse<PickupLibraryJson>(manifest, PickupsSourceId, issues);
            AddPickups(manifest, pickups, builders, rawIds, ref order);

            RelicLibraryJson relics = Parse<RelicLibraryJson>(manifest, RelicsSourceId, issues);
            AddRelics(manifest, relics, builders, rawIds, ref order);

            ClassLibraryJson classes = Parse<ClassLibraryJson>(manifest, ClassesSourceId, issues);
            AddClasses(manifest, classes, builders, rawIds, ref order);

            RewardLibraryJson rewards = Parse<RewardLibraryJson>(manifest, RewardsSourceId, issues);
            AddRewards(manifest, rewards, builders, rawIds, ref order);

            ProgressionLibraryJson progression = Parse<ProgressionLibraryJson>(manifest, ProgressionSourceId, issues);
            AddProgression(manifest, progression, builders, rawIds, ref order);

            RunFlowLibraryJson runFlow = Parse<RunFlowLibraryJson>(manifest, RunFlowSourceId, issues);
            AddRunFlow(manifest, runFlow, builders, rawIds, ref order);

            AddTheme(manifest, PrimaryThemeSourceId, Parse<UiThemeRecordJson>(manifest, PrimaryThemeSourceId, issues), builders, rawIds, ref order);
            AddTheme(manifest, AlternateThemeSourceId, Parse<UiThemeRecordJson>(manifest, AlternateThemeSourceId, issues), builders, rawIds, ref order);

            ResolveReferences(manifest.OwningPackageId, manifest.PackId, builders, rawIds, issues);
            ApplyValidationToRecords(builders, selectedValidation.Issues);
            IReadOnlyList<GameContentRecordDescriptor> records = BuildDescriptors(manifest, builders);
            IReadOnlyList<GameContentCategoryDescriptor> categories = BuildCategories(records);
            return new SurvivorsContentPackIndex(
                manifest,
                records,
                categories,
                new GameContentAuthoringValidationResult(DistinctIssues(issues)));
        }

        public static GameContentAuthoringValidationResult ValidateSelectedSources(GameContentPackManifest manifest)
        {
            var issues = new List<GameContentAuthoringValidationIssue>();
            if (manifest == null)
            {
                issues.Add(GameContentAuthoringValidationIssue.Error("Manifest", "A Survivors content-pack manifest is required."));
                return new GameContentAuthoringValidationResult(issues);
            }

            var sources = new Dictionary<string, TextAsset>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < RequiredSourceIdsInternal.Length; i++)
            {
                string sourceId = RequiredSourceIdsInternal[i];
                if (!manifest.TryGetSource(sourceId, out GameContentPackSourceReference source) || source.TextAsset == null)
                {
                    issues.Add(GameContentAuthoringValidationIssue.Error(
                        manifest.name + "." + sourceId,
                        "Required selected manifest source '" + sourceId + "' is missing."));
                    continue;
                }

                sources[sourceId] = source.TextAsset;
            }

            if (issues.Count > 0) return new GameContentAuthoringValidationResult(issues);

            ContentValidationReport report = SurvivorsEditorContentValidation.ValidateJsonContent(
                sources[WeaponsSourceId].text,
                sources[UpgradesSourceId].text,
                sources[EnemiesSourceId].text,
                sources[RewardsSourceId].text,
                sources[RelicsSourceId].text,
                sources[ClassesSourceId].text,
                sources[ProgressionSourceId].text,
                sources[PickupsSourceId].text,
                sources[RunFlowSourceId].text,
                sources[PrimaryThemeSourceId].text,
                sources[AlternateThemeSourceId].text);
            return GameContentAuthoringValidationReports.ToAuthoringResult(report);
        }

        public bool TryGetRunProfile(string profileId, out float targetDurationSeconds, out float victoryTimeSeconds)
        {
            GameContentRecordDescriptor record = Records.FirstOrDefault(candidate =>
                candidate.IsInCategory("run-profiles") &&
                string.Equals(candidate.SourceRecordId, profileId, StringComparison.OrdinalIgnoreCase));
            targetDurationSeconds = ReadMetadataFloat(record, "Target Duration");
            victoryTimeSeconds = ReadMetadataFloat(record, "Victory Time");
            return record != null;
        }

        private static void AddWeapons(
            GameContentPackManifest manifest,
            WeaponLibraryJson library,
            ICollection<RecordBuilder> records,
            IDictionary<string, List<RecordBuilder>> rawIds,
            ref int order)
        {
            if (library == null) return;
            WeaponRecordJson[] weapons = library.weapons ?? Array.Empty<WeaponRecordJson>();
            for (int i = 0; i < weapons.Length; i++)
            {
                WeaponRecordJson value = weapons[i];
                if (value == null) continue;
                RecordBuilder record = AddRecord(
                    manifest,
                    WeaponsSourceId,
                    value.id,
                    "weapons",
                    null,
                    value.displayName,
                    "Authored " + Display(value.fireMode, "weapon") + " weapon.",
                    "weapons[" + i + "]",
                    records,
                    rawIds,
                    ref order);
                record.Metadata.Add(Metadata("Fire Mode", value.fireMode));
                record.Metadata.Add(Metadata("Damage", value.damage));
                record.Metadata.Add(Metadata("Cooldown", value.cooldownSeconds, "0.###s"));
                record.Metadata.Add(Metadata("Range", value.range));
                record.Metadata.Add(Metadata("Tint", value.tint));
                record.Metadata.Add(Metadata("Projectile Radius", value.projectileRadius));
                record.Metadata.Add(Metadata("Pierce Count", value.pierceCount));
                record.Metadata.Add(Metadata("Chain Count", value.chainCount));
                record.Metadata.Add(Metadata("Fork Count", value.forkCount));
                record.Metadata.Add(Metadata("Return Count", value.returnCount));
                record.Metadata.Add(Metadata("Fan Count", value.fanCount));
                record.Metadata.Add(Metadata("Spread", value.spreadDegrees, "0.### degrees"));
                AddPending(record, value.projectileId, "projectile", false);
            }

            ProjectileRecordJson[] projectiles = library.projectiles ?? Array.Empty<ProjectileRecordJson>();
            for (int i = 0; i < projectiles.Length; i++)
            {
                ProjectileRecordJson value = projectiles[i];
                if (value == null) continue;
                RecordBuilder record = AddRecord(
                    manifest,
                    WeaponsSourceId,
                    value.id,
                    "projectiles",
                    null,
                    Display(value.id, "Projectile"),
                    "Authored projectile movement record.",
                    "projectiles[" + i + "]",
                    records,
                    rawIds,
                    ref order);
                record.Metadata.Add(Metadata("Speed", value.speed));
                record.Metadata.Add(Metadata("Lifetime", value.lifetimeSeconds, "0.###s"));
            }
        }

        private static void AddUpgrades(
            GameContentPackManifest manifest,
            UpgradeLibraryJson library,
            ICollection<RecordBuilder> records,
            IDictionary<string, List<RecordBuilder>> rawIds,
            ref int order)
        {
            if (library == null) return;
            UpgradeRecordJson[] upgrades = library.upgrades ?? Array.Empty<UpgradeRecordJson>();
            for (int i = 0; i < upgrades.Length; i++)
            {
                UpgradeRecordJson value = upgrades[i];
                if (value == null) continue;
                string primary = UpgradeCategory(value.category);
                var categories = new List<string> { "upgrades" };
                if (IsPickupUpgrade(value)) categories.Add("pickup-magnet");
                RecordBuilder record = AddRecord(
                    manifest,
                    UpgradesSourceId,
                    value.id,
                    primary,
                    categories,
                    value.displayName,
                    value.description,
                    "upgrades[" + i + "]",
                    records,
                    rawIds,
                    ref order);
                record.Metadata.Add(Metadata("Category", value.category));
                record.Metadata.Add(Metadata("Rarity", value.rarity));
                record.Metadata.Add(Metadata("Weight", value.weight));
                record.Metadata.Add(Metadata("Max Rank", value.maxRank));
                record.Metadata.Add(Metadata("Effect", value.effect));
                record.Metadata.Add(Metadata("Amount", value.amount));
                record.Metadata.Add(Metadata("Target", value.target));
                AddPending(record, value.requiredUpgradeId, "requires upgrade rank " + value.requiredUpgradeRank, true);
                AddPending(record, value.requiredPassiveUpgradeId, "requires passive", true);
                AddPending(record, value.requiredOwnedWeaponId, "requires owned weapon", true);
                AddPending(record, value.affectedContentId, "affects content", false, onlyWhenResolvable: true);
                AddPending(record, NormalizeContentTarget(value.target), "targets", false, onlyWhenResolvable: true);
            }
        }

        private static void AddEnemies(
            GameContentPackManifest manifest,
            EnemyLibraryJson library,
            ICollection<RecordBuilder> records,
            IDictionary<string, List<RecordBuilder>> rawIds,
            ref int order)
        {
            if (library == null) return;
            EnemyRecordJson[] enemies = library.enemies ?? Array.Empty<EnemyRecordJson>();
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyRecordJson value = enemies[i];
                if (value == null) continue;
                var categories = new List<string> { "enemies" };
                if (string.Equals(value.role, "Elite", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value.role, "DreadElite", StringComparison.OrdinalIgnoreCase)) categories.Add("elites");
                if (string.Equals(value.role, "Miniboss", StringComparison.OrdinalIgnoreCase)) categories.Add("minibosses");
                if (string.Equals(value.role, "Boss", StringComparison.OrdinalIgnoreCase)) categories.Add("bosses");
                RecordBuilder record = AddRecord(
                    manifest,
                    EnemiesSourceId,
                    value.id,
                    "enemies",
                    categories,
                    value.displayName,
                    "Authored " + Display(value.role, "enemy") + " threat.",
                    "enemies[" + i + "]",
                    records,
                    rawIds,
                    ref order);
                record.Metadata.Add(Metadata("Role", value.role));
                record.Metadata.Add(Metadata("Role Behavior", DescribeEnemyRoleBehavior(value.role)));
                record.Metadata.Add(Metadata("Health", value.health));
                record.Metadata.Add(Metadata("Move Speed", value.moveSpeed));
                record.Metadata.Add(Metadata("Radius", value.radius));
                record.Metadata.Add(Metadata("Contact Damage", value.contactDamage));
                record.Metadata.Add(Metadata("Contact Interval", value.contactIntervalSeconds, "0.###s"));
                record.Metadata.Add(Metadata("XP Drop", value.experienceDrop));
                record.Metadata.Add(Metadata("Tint", value.tint));
                record.Metadata.Add(Metadata("Can Recycle", value.canRecycle));
                record.Metadata.Add(Metadata("Can Leash", value.canLeash));
                record.Metadata.Add(Metadata("Can Reposition", value.canReposition));
                record.Metadata.Add(Metadata("Offscreen Marker", value.showOffscreenMarker));
                record.Metadata.Add(Metadata("Overhead Life Bar", value.showOverheadLifeBar));
                record.Metadata.Add(Metadata("Boss Life Bar", value.showBossLifeBar));
                record.Metadata.Add(Metadata("Marker Style", value.markerStyle));
                record.Metadata.Add(Metadata("Soft Leash", value.softLeashRadius));
                record.Metadata.Add(Metadata("Hard Recycle", value.hardRecycleRadius));
                record.Metadata.Add(Metadata("Catch-up Speed", value.catchUpSpeedMultiplier));
                record.Metadata.Add(Metadata("Reposition Timeout", value.repositionTimeoutSeconds, "0.###s"));
            }
        }

        private static void AddPickups(
            GameContentPackManifest manifest,
            PickupLibraryJson library,
            ICollection<RecordBuilder> records,
            IDictionary<string, List<RecordBuilder>> rawIds,
            ref int order)
        {
            if (library == null) return;
            PickupRecordJson[] pickups = library.pickups ?? Array.Empty<PickupRecordJson>();
            for (int i = 0; i < pickups.Length; i++)
            {
                PickupRecordJson value = pickups[i];
                if (value == null) continue;
                RecordBuilder record = AddRecord(
                    manifest,
                    PickupsSourceId,
                    value.id,
                    "pickup-magnet",
                    null,
                    value.displayName,
                    "Authored pickup behavior.",
                    "pickups[" + i + "]",
                    records,
                    rawIds,
                    ref order);
                record.Metadata.Add(Metadata("Behavior", value.behavior));
            }
        }

        private static void AddRelics(
            GameContentPackManifest manifest,
            RelicLibraryJson library,
            ICollection<RecordBuilder> records,
            IDictionary<string, List<RecordBuilder>> rawIds,
            ref int order)
        {
            if (library == null) return;
            RelicRecordJson[] relics = library.relics ?? Array.Empty<RelicRecordJson>();
            for (int i = 0; i < relics.Length; i++)
            {
                RelicRecordJson value = relics[i];
                if (value == null) continue;
                RecordBuilder record = AddRecord(
                    manifest,
                    RelicsSourceId,
                    value.id,
                    "relics",
                    null,
                    value.displayName,
                    "Authored relic reward.",
                    "relics[" + i + "]",
                    records,
                    rawIds,
                    ref order);
                record.Metadata.Add(Metadata("Effect", value.effect));
                record.Metadata.Add(Metadata("Effect Kind", value.effectKind));
                record.Metadata.Add(Metadata("Amount", value.amount));
                AddPending(record, NormalizeContentTarget(value.target), "targets", false, onlyWhenResolvable: true);
            }
        }

        private static void AddClasses(
            GameContentPackManifest manifest,
            ClassLibraryJson library,
            ICollection<RecordBuilder> records,
            IDictionary<string, List<RecordBuilder>> rawIds,
            ref int order)
        {
            if (library == null) return;
            ClassRecordJson[] classes = library.classes ?? Array.Empty<ClassRecordJson>();
            for (int i = 0; i < classes.Length; i++)
            {
                ClassRecordJson value = classes[i];
                if (value == null) continue;
                RecordBuilder record = AddRecord(
                    manifest,
                    ClassesSourceId,
                    value.id,
                    "classes",
                    null,
                    value.displayName,
                    value.unlockedByDefault ? "Default unlocked class." : "Unlockable class.",
                    "classes[" + i + "]",
                    records,
                    rawIds,
                    ref order);
                record.Metadata.Add(Metadata("Unlocked By Default", value.unlockedByDefault));
                string[] startingWeapons = value.startingWeaponIds == null || value.startingWeaponIds.Length == 0
                    ? new[] { value.startingWeaponId }
                    : value.startingWeaponIds;
                foreach (string weaponId in startingWeapons) AddPending(record, weaponId, "starts with", true);
                AddPending(record, value.unlockRewardId, "unlocked by reward", true);
            }
        }

        private static void AddRewards(
            GameContentPackManifest manifest,
            RewardLibraryJson library,
            ICollection<RecordBuilder> records,
            IDictionary<string, List<RecordBuilder>> rawIds,
            ref int order)
        {
            if (library == null) return;
            CurrencyRecordJson[] currencies = library.currencies ?? Array.Empty<CurrencyRecordJson>();
            for (int i = 0; i < currencies.Length; i++)
            {
                CurrencyRecordJson value = currencies[i];
                if (value == null) continue;
                AddRecord(manifest, RewardsSourceId, value.id, "rewards", null, value.displayName, "Persistent reward currency.", "currencies[" + i + "]", records, rawIds, ref order);
            }

            TrackRecordJson[] tracks = library.tracks ?? Array.Empty<TrackRecordJson>();
            for (int i = 0; i < tracks.Length; i++)
            {
                TrackRecordJson value = tracks[i];
                if (value == null) continue;
                AddRecord(manifest, RewardsSourceId, value.id, "rewards", null, value.displayName, "Persistent reward track.", "tracks[" + i + "]", records, rawIds, ref order);
            }

            PersistentUpgradeRecordJson[] meta = library.persistentUpgrades ?? Array.Empty<PersistentUpgradeRecordJson>();
            for (int i = 0; i < meta.Length; i++)
            {
                PersistentUpgradeRecordJson value = meta[i];
                if (value == null) continue;
                RecordBuilder record = AddRecord(manifest, RewardsSourceId, value.id, "meta-upgrades", null, value.displayName, "Persistent meta upgrade.", "persistentUpgrades[" + i + "]", records, rawIds, ref order);
                record.Metadata.Add(Metadata("Effect", value.effect));
                record.Metadata.Add(Metadata("Max Rank", value.maxRank));
                record.Metadata.Add(Metadata("Amount Per Rank", value.amountPerRank));
                AddPending(record, NormalizeContentTarget(value.target), "targets", false, onlyWhenResolvable: true);
            }

            RewardRecordJson[] rewards = library.rewards ?? Array.Empty<RewardRecordJson>();
            for (int i = 0; i < rewards.Length; i++)
            {
                RewardRecordJson value = rewards[i];
                if (value == null) continue;
                RecordBuilder record = AddRecord(manifest, RewardsSourceId, value.id, "rewards", null, Display(value.id, "Reward"), "Authored persistent reward grant.", "rewards[" + i + "]", records, rawIds, ref order);
                record.Metadata.Add(Metadata("Currency Amount", value.currencyAmount));
                record.Metadata.Add(Metadata("Track Amount", value.trackAmount));
                AddPending(record, value.currencyId, "grants currency", true);
                AddPending(record, value.trackId, "advances track", true);
            }
        }

        private static void AddProgression(
            GameContentPackManifest manifest,
            ProgressionLibraryJson library,
            ICollection<RecordBuilder> records,
            IDictionary<string, List<RecordBuilder>> rawIds,
            ref int order)
        {
            if (library == null) return;
            ProgressionTrackRecordJson[] tracks = library.tracks ?? Array.Empty<ProgressionTrackRecordJson>();
            for (int i = 0; i < tracks.Length; i++)
            {
                ProgressionTrackRecordJson value = tracks[i];
                if (value == null) continue;
                RecordBuilder track = AddRecord(manifest, ProgressionSourceId, value.id, "progression", null, value.displayName, "Authored " + Display(value.kind, "progression") + " track.", "tracks[" + i + "]", records, rawIds, ref order);
                track.Metadata.Add(Metadata("Kind", value.kind));
                track.Metadata.Add(Metadata("Node Count", value.nodes == null ? 0 : value.nodes.Length));
                AddPending(track, value.classId, "belongs to class", true);
                AddPending(track, value.targetWeaponId, "belongs to weapon", true);

                ProgressionNodeRecordJson[] nodes = value.nodes ?? Array.Empty<ProgressionNodeRecordJson>();
                for (int nodeIndex = 0; nodeIndex < nodes.Length; nodeIndex++)
                {
                    ProgressionNodeRecordJson node = nodes[nodeIndex];
                    if (node == null) continue;
                    RecordBuilder nodeRecord = AddRecord(manifest, ProgressionSourceId, node.id, "progression", null, node.displayName, "Authored progression node.", "tracks[" + i + "].nodes[" + nodeIndex + "]", records, rawIds, ref order);
                    nodeRecord.Metadata.Add(Metadata("Kind", node.kind));
                    nodeRecord.Metadata.Add(Metadata("Tier", node.tier));
                    nodeRecord.Metadata.Add(Metadata("Point Cost", node.pointCost));
                    nodeRecord.Metadata.Add(Metadata("Max Rank", node.maxRank));
                    AddPending(nodeRecord, value.id, "belongs to track", true);
                    AddPending(nodeRecord, node.upgradeId, "unlocks upgrade", true);
                    AddUpgradeClassGate(rawIds, node.upgradeId, value.classId);
                }
            }
        }

        private static void AddUpgradeClassGate(
            IDictionary<string, List<RecordBuilder>> rawIds,
            string upgradeId,
            string classId)
        {
            if (string.IsNullOrWhiteSpace(upgradeId) || string.IsNullOrWhiteSpace(classId) ||
                rawIds == null || !rawIds.TryGetValue(upgradeId.Trim(), out List<RecordBuilder> candidates))
                return;
            foreach (RecordBuilder candidate in candidates)
            {
                if (!string.Equals(candidate.SourceId, UpgradesSourceId, StringComparison.OrdinalIgnoreCase)) continue;
                if (candidate.Metadata.Any(value =>
                        string.Equals(value.Label, "Class Gate", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(value.Value, classId, StringComparison.OrdinalIgnoreCase))) continue;
                candidate.Metadata.Add(Metadata("Class Gate", classId));
            }
        }

        private static void AddRunFlow(
            GameContentPackManifest manifest,
            RunFlowLibraryJson library,
            ICollection<RecordBuilder> records,
            IDictionary<string, List<RecordBuilder>> rawIds,
            ref int order)
        {
            if (library == null) return;
            RunFlowProfileRecordJson[] profiles = library.profiles ?? Array.Empty<RunFlowProfileRecordJson>();
            for (int i = 0; i < profiles.Length; i++)
            {
                RunFlowProfileRecordJson value = profiles[i];
                if (value == null) continue;
                RecordBuilder record = AddRecord(
                    manifest,
                    RunFlowSourceId,
                    value.id,
                    "run-profiles",
                    new[] { "waves-milestones" },
                    value.displayName,
                    value.description,
                    "profiles[" + i + "]",
                    records,
                    rawIds,
                    ref order);
                record.Metadata.Add(Metadata("Target Duration", value.targetDurationSeconds, "0.###"));
                record.Metadata.Add(Metadata("Victory Time", value.survivalVictoryTimeSeconds, "0.###"));
                record.Metadata.Add(Metadata("First Elite", value.firstEliteSpawnTimeSeconds, "0.###s"));
                record.Metadata.Add(Metadata("First Dread Elite", value.firstDreadEliteSpawnTimeSeconds, "0.###s"));
                record.Metadata.Add(Metadata("Miniboss", value.minibossSpawnTimeSeconds, "0.###s"));
                record.Metadata.Add(Metadata("Boss", value.bossSpawnTimeSeconds, "0.###s"));
                record.Metadata.Add(Metadata("Endless", value.endlessContinuationEnabled));
                record.Metadata.Add(Metadata("Reward Multiplier", value.runRewardMultiplier));
                record.Metadata.Add(Metadata("Spawn Interval", value.enemySpawnIntervalSeconds, "0.###s"));
                record.Metadata.Add(Metadata("Maximum Alive", value.enemyMaximumAlive));
                record.Metadata.Add(Metadata("Escalation Interval", value.escalationIntervalSeconds, "0.###s"));
                record.Metadata.Add(Metadata("Minimum Spawn Interval", value.minimumEnemySpawnIntervalSeconds, "0.###s"));
                record.Metadata.Add(Metadata("Spawn Interval Reduction", value.enemySpawnIntervalReductionPerEscalation, "0.###s"));
                record.Metadata.Add(Metadata("Horde First", value.hordeRushFirstTimeSeconds, "0.###s"));
                record.Metadata.Add(Metadata("Horde Interval", value.hordeRushIntervalSeconds, "0.###s"));
                foreach (RecordBuilder enemy in records.Where(candidate => candidate.SourceId == EnemiesSourceId))
                    AddPending(record, enemy.SourceRecordId, "uses authored enemy role", false);
                foreach (RecordBuilder reward in records.Where(candidate =>
                             candidate.SourceId == RewardsSourceId &&
                             string.Equals(candidate.PrimaryCategory, "rewards", StringComparison.OrdinalIgnoreCase)))
                    AddPending(record, reward.SourceRecordId, "uses authored pack reward", false);
            }
        }

        private static void AddTheme(
            GameContentPackManifest manifest,
            string sourceId,
            UiThemeRecordJson theme,
            ICollection<RecordBuilder> records,
            IDictionary<string, List<RecordBuilder>> rawIds,
            ref int order)
        {
            if (theme == null) return;
            string themeId = sourceId;
            RecordBuilder themeRecord = AddRecord(manifest, sourceId, themeId, "themes", null, theme.themeName, "Authored UI, world presentation, audio, and tutorial theme.", "$", records, rawIds, ref order);
            themeRecord.Metadata.Add(Metadata("HUD Accent", theme.hudAccentColor));
            themeRecord.Metadata.Add(Metadata("Standard Label", theme.standardModeDisplayName));
            themeRecord.Metadata.Add(Metadata("Sprint Label", theme.sprintModeDisplayName));
            themeRecord.Metadata.Add(Metadata("World Presentation", theme.worldPresentation != null && theme.worldPresentation.authored));

            AudioEventRecordJson[] audioEvents = theme.audioEvents ?? Array.Empty<AudioEventRecordJson>();
            for (int i = 0; i < audioEvents.Length; i++)
            {
                AudioEventRecordJson value = audioEvents[i];
                if (value == null) continue;
                RecordBuilder audio = AddRecord(manifest, sourceId, value.id, "audio-events", null, Display(value.id, "Audio Event"), "Authored audio palette event.", "audioEvents[" + i + "]", records, rawIds, ref order, registerRawId: false);
                audio.Metadata.Add(Metadata("Category", value.category));
                audio.Metadata.Add(Metadata("Volume", value.volume));
                audio.Metadata.Add(Metadata("Throttle", value.throttleSeconds, "0.###s"));
                AddPendingScoped(audio, themeRecord.PackScopedId, "defined by theme");
            }

            TutorialStepRecordJson[] tutorial = theme.tutorialSteps ?? Array.Empty<TutorialStepRecordJson>();
            for (int i = 0; i < tutorial.Length; i++)
            {
                TutorialStepRecordJson value = tutorial[i];
                if (value == null) continue;
                RecordBuilder step = AddRecord(manifest, sourceId, value.id, "tutorial", null, value.title, value.lines == null ? "Authored tutorial step." : string.Join(" ", value.lines), "tutorialSteps[" + i + "]", records, rawIds, ref order, registerRawId: false);
                step.Metadata.Add(Metadata("Line Count", value.lines == null ? 0 : value.lines.Length));
                AddPendingScoped(step, themeRecord.PackScopedId, "defined by theme");
            }
        }

        private static void ResolveReferences(
            string owningPackageId,
            string packId,
            IReadOnlyList<RecordBuilder> records,
            IReadOnlyDictionary<string, List<RecordBuilder>> rawIds,
            ICollection<GameContentAuthoringValidationIssue> packIssues)
        {
            var scoped = records
                .GroupBy(record => record.PackScopedId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
            foreach (IGrouping<string, RecordBuilder> duplicate in records
                         .GroupBy(record => record.PackScopedId, StringComparer.OrdinalIgnoreCase)
                         .Where(group => group.Count() > 1))
            {
                string message = "Duplicate pack-scoped record ID '" + duplicate.Key + "'.";
                packIssues.Add(GameContentAuthoringValidationIssue.Error("Records", message));
                foreach (RecordBuilder record in duplicate)
                    record.Issues.Add(GameContentAuthoringValidationIssue.Error(record.SourceLocator, message));
            }
            foreach (RecordBuilder source in records)
            {
                foreach (PendingReference pending in source.PendingReferences)
                {
                    RecordBuilder target = null;
                    if (pending.Scoped)
                    {
                        scoped.TryGetValue(pending.TargetId, out target);
                    }
                    else
                    {
                        string targetId = NormalizeContentTarget(pending.TargetId);
                        if (rawIds.TryGetValue(targetId, out List<RecordBuilder> candidates) && candidates.Count == 1)
                            target = candidates[0];
                    }

                    if (pending.OnlyWhenResolvable && target == null) continue;
                    string resolvedId = target == null
                        ? BuildScopedId(packId, "missing", pending.TargetId)
                        : target.PackScopedId;
                    source.Outbound.Add(new GameContentRecordReferenceDescriptor(
                        resolvedId,
                        target == null ? string.Empty : target.PrimaryCategory,
                        packId,
                        pending.Relationship,
                        pending.Required,
                        target != null,
                        owningPackageId,
                        target == null
                            ? null
                            : new GameContentRecordKey(
                                owningPackageId,
                                packId,
                                target.SourceRecordId,
                                target.SourceId,
                                target.SourceLocator)));

                    if (target != null)
                    {
                        target.Inbound.Add(new GameContentRecordReferenceDescriptor(
                            source.PackScopedId,
                            source.PrimaryCategory,
                            packId,
                            pending.Relationship + " from " + source.DisplayName,
                            pending.Required,
                            true,
                            owningPackageId,
                            new GameContentRecordKey(
                                owningPackageId,
                                packId,
                                source.SourceRecordId,
                                source.SourceId,
                                source.SourceLocator)));
                        continue;
                    }

                    string message = "Record '" + source.SourceRecordId + "' has a broken " + pending.Relationship + " reference to '" + pending.TargetId + "'.";
                    source.Issues.Add(GameContentAuthoringValidationIssue.Error(source.SourceLocator, message));
                    packIssues.Add(GameContentAuthoringValidationIssue.Error(source.SourceLocator, message));
                }
            }
        }

        private static IReadOnlyList<GameContentRecordDescriptor> BuildDescriptors(
            GameContentPackManifest manifest,
            IEnumerable<RecordBuilder> builders)
        {
            return builders
                .OrderBy(builder => builder.Order)
                .ThenBy(builder => builder.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Select(builder => new GameContentRecordDescriptor(
                    builder.PackScopedId,
                    builder.SourceRecordId,
                    builder.PrimaryCategory,
                    builder.Categories,
                    builder.DisplayName,
                    builder.Description,
                    builder.Description,
                    builder.Metadata,
                    builder.SourceAsset,
                    builder.SourcePath,
                    builder.SourceLocator,
                    builder.Outbound,
                    builder.Inbound,
                    new GameContentAuthoringValidationResult(builder.Issues),
                    builder.Order,
                    null,
                    builder.PrimaryCategory,
                    new GameContentRecordKey(
                        manifest.OwningPackageId,
                        manifest.PackId,
                        builder.SourceRecordId,
                        builder.SourceId,
                        builder.SourceLocator),
                    BuildCapabilities(builder)))
                .ToArray();
        }

        private static IReadOnlyList<GameContentRecordCapability> BuildCapabilities(RecordBuilder builder)
        {
            var values = new List<GameContentRecordCapability>();
            if (builder == null) return values;
            if (string.Equals(builder.SourceId, WeaponsSourceId, StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(builder.PrimaryCategory, "weapons", StringComparison.OrdinalIgnoreCase))
                {
                    AddCapability(values, GameContentRecordCapabilities.Weapon);
                    AddCapability(values, GameContentRecordCapabilities.Attack);
                }
                else if (string.Equals(builder.PrimaryCategory, "projectiles", StringComparison.OrdinalIgnoreCase))
                {
                    AddCapability(values, GameContentRecordCapabilities.Projectile);
                }
            }
            else if (string.Equals(builder.SourceId, EnemiesSourceId, StringComparison.OrdinalIgnoreCase))
            {
                AddCapability(values, GameContentRecordCapabilities.Enemy);
            }
            else if (string.Equals(builder.SourceId, UpgradesSourceId, StringComparison.OrdinalIgnoreCase))
            {
                AddCapability(values, GameContentRecordCapabilities.Upgrade);
                string category = GetMetadata(builder, "Category");
                if (Contains(category, "weapon")) AddCapability(values, GameContentRecordCapabilities.WeaponUpgrade);
            }
            else if (string.Equals(builder.SourceId, RunFlowSourceId, StringComparison.OrdinalIgnoreCase))
            {
                AddCapability(values, GameContentRecordCapabilities.Encounter);
                AddCapability(values, GameContentRecordCapabilities.RunProfile);
                AddCapability(values, GameContentRecordCapabilities.TimedMilestone);
                AddCapability(values, GameContentRecordCapabilities.HordeEvent);
                AddCapability(values, GameContentRecordCapabilities.EliteEvent);
                AddCapability(values, GameContentRecordCapabilities.BossEvent);
            }
            else if (string.Equals(builder.SourceId, PickupsSourceId, StringComparison.OrdinalIgnoreCase))
            {
                AddCapability(values, GameContentRecordCapabilities.PickupMagnet);
            }
            else if (string.Equals(builder.SourceId, RewardsSourceId, StringComparison.OrdinalIgnoreCase))
            {
                AddCapability(values, string.Equals(builder.PrimaryCategory, "meta-upgrades", StringComparison.OrdinalIgnoreCase)
                    ? GameContentRecordCapabilities.Upgrade
                    : GameContentRecordCapabilities.Reward);
                if (string.Equals(builder.PrimaryCategory, "meta-upgrades", StringComparison.OrdinalIgnoreCase))
                    AddCapability(values, GameContentRecordCapabilities.MetaUpgrade);
            }
            else if (string.Equals(builder.PrimaryCategory, "themes", StringComparison.OrdinalIgnoreCase))
            {
                AddCapability(values, GameContentRecordCapabilities.Theme);
            }

            if (builder.Categories.Any(category => string.Equals(category, "passives", StringComparison.OrdinalIgnoreCase)))
                AddCapability(values, GameContentRecordCapabilities.Passive);
            if (builder.Categories.Any(category => string.Equals(category, "pickup-magnet", StringComparison.OrdinalIgnoreCase)) &&
                string.Equals(builder.SourceId, UpgradesSourceId, StringComparison.OrdinalIgnoreCase))
                AddCapability(values, GameContentRecordCapabilities.PickupMagnet);
            if (builder.Categories.Any(category => string.Equals(category, "mutations", StringComparison.OrdinalIgnoreCase)))
                AddCapability(values, GameContentRecordCapabilities.Mutation);
            if (builder.Categories.Any(category => string.Equals(category, "evolutions", StringComparison.OrdinalIgnoreCase)))
                AddCapability(values, GameContentRecordCapabilities.Evolution);
            if (builder.Categories.Any(category => string.Equals(category, "elites", StringComparison.OrdinalIgnoreCase)))
            {
                AddCapability(values, GameContentRecordCapabilities.Elite);
                AddCapability(values, GameContentRecordCapabilities.MajorThreat);
            }
            if (builder.Categories.Any(category => string.Equals(category, "minibosses", StringComparison.OrdinalIgnoreCase)))
            {
                AddCapability(values, GameContentRecordCapabilities.Miniboss);
                AddCapability(values, GameContentRecordCapabilities.MajorThreat);
            }
            if (builder.Categories.Any(category => string.Equals(category, "bosses", StringComparison.OrdinalIgnoreCase)))
            {
                AddCapability(values, GameContentRecordCapabilities.Boss);
                AddCapability(values, GameContentRecordCapabilities.MajorThreat);
            }

            return values;
        }

        private static void AddCapability(
            ICollection<GameContentRecordCapability> values,
            GameContentRecordCapability capability)
        {
            if (!values.Contains(capability)) values.Add(capability);
        }

        private static string GetMetadata(RecordBuilder builder, string label)
        {
            return builder.Metadata.FirstOrDefault(value =>
                string.Equals(value.Label, label, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
        }

        private static void ApplyValidationToRecords(
            IReadOnlyList<RecordBuilder> records,
            IEnumerable<GameContentAuthoringValidationIssue> validationIssues)
        {
            if (validationIssues == null) return;
            foreach (GameContentAuthoringValidationIssue issue in validationIssues)
            {
                if (issue == null) continue;
                RecordBuilder match = records
                    .Where(record => !string.IsNullOrWhiteSpace(record.SourceRecordId))
                    .OrderByDescending(record => record.SourceRecordId.Length)
                    .FirstOrDefault(record =>
                        Contains(issue.Path, record.SourceRecordId) || Contains(issue.Message, record.SourceRecordId));
                if (match != null) match.Issues.Add(issue);
            }
        }

        private static IReadOnlyList<GameContentCategoryDescriptor> BuildCategories(IReadOnlyList<GameContentRecordDescriptor> records)
        {
            return CategoryDefinitions
                .Select((definition, order) => new GameContentCategoryDescriptor(
                    definition.Id,
                    definition.DisplayName,
                    definition.Description,
                    definition.Id,
                    order,
                    records.Count(record => record.IsInCategory(definition.Id))))
                .ToArray();
        }

        private static RecordBuilder AddRecord(
            GameContentPackManifest manifest,
            string sourceId,
            string sourceRecordId,
            string primaryCategory,
            IEnumerable<string> categories,
            string displayName,
            string description,
            string locator,
            ICollection<RecordBuilder> records,
            IDictionary<string, List<RecordBuilder>> rawIds,
            ref int order,
            bool registerRawId = true)
        {
            manifest.TryGetSource(sourceId, out GameContentPackSourceReference source);
            var record = new RecordBuilder
            {
                PackScopedId = BuildScopedId(manifest.PackId, sourceId, sourceRecordId),
                SourceRecordId = Normalize(sourceRecordId),
                SourceId = sourceId,
                PrimaryCategory = primaryCategory,
                DisplayName = Display(displayName, sourceRecordId),
                Description = Normalize(description),
                SourceAsset = source == null ? null : source.TextAsset,
                SourcePath = source == null || source.TextAsset == null ? string.Empty : AssetDatabase.GetAssetPath(source.TextAsset),
                SourceLocator = sourceId + ":" + locator,
                Order = order++
            };
            record.Categories.Add(primaryCategory);
            if (categories != null)
            {
                foreach (string category in categories)
                    if (!string.IsNullOrWhiteSpace(category)) record.Categories.Add(category);
            }

            if (string.IsNullOrWhiteSpace(record.SourceRecordId))
                record.Issues.Add(GameContentAuthoringValidationIssue.Error(record.SourceLocator, "Record ID is required."));
            records.Add(record);
            if (registerRawId && !string.IsNullOrWhiteSpace(record.SourceRecordId))
            {
                if (!rawIds.TryGetValue(record.SourceRecordId, out List<RecordBuilder> matches))
                {
                    matches = new List<RecordBuilder>();
                    rawIds[record.SourceRecordId] = matches;
                }
                matches.Add(record);
            }
            return record;
        }

        private static T Parse<T>(GameContentPackManifest manifest, string sourceId, ICollection<GameContentAuthoringValidationIssue> issues)
            where T : class
        {
            if (!manifest.TryGetSource(sourceId, out GameContentPackSourceReference source) || source.TextAsset == null) return null;
            try
            {
                T value = JsonUtility.FromJson<T>(source.TextAsset.text);
                if (value == null)
                    issues.Add(GameContentAuthoringValidationIssue.Error(sourceId, "Selected source could not be parsed."));
                return value;
            }
            catch (Exception exception)
            {
                issues.Add(GameContentAuthoringValidationIssue.Error(
                    sourceId,
                    "Selected source could not be parsed: " + exception.GetBaseException().Message));
                return null;
            }
        }

        private static void AddPending(
            RecordBuilder record,
            string targetId,
            string relationship,
            bool required,
            bool onlyWhenResolvable = false)
        {
            if (record == null || string.IsNullOrWhiteSpace(targetId)) return;
            record.PendingReferences.Add(new PendingReference(targetId, relationship, required, onlyWhenResolvable, false));
        }

        private static void AddPendingScoped(RecordBuilder record, string targetId, string relationship)
        {
            if (record == null || string.IsNullOrWhiteSpace(targetId)) return;
            record.PendingReferences.Add(new PendingReference(targetId, relationship, true, false, true));
        }

        private static string UpgradeCategory(string category)
        {
            if (string.Equals(category, "Passive", StringComparison.OrdinalIgnoreCase)) return "passives";
            if (string.Equals(category, "Mutation", StringComparison.OrdinalIgnoreCase)) return "mutations";
            if (string.Equals(category, "Evolution", StringComparison.OrdinalIgnoreCase)) return "evolutions";
            return "upgrades";
        }

        private static bool IsPickupUpgrade(UpgradeRecordJson value)
        {
            if (value == null) return false;
            return Contains(value.effect, "pickup") || Contains(value.effect, "magnet") ||
                   Contains(value.target, "pickup") || Contains(value.target, "magnet") ||
                   (value.effects != null && value.effects.Any(effect => effect != null &&
                       (Contains(effect.effect, "pickup") || Contains(effect.effect, "magnet") ||
                        Contains(effect.target, "pickup") || Contains(effect.target, "magnet"))));
        }

        private static string NormalizeContentTarget(string value)
        {
            string normalized = Normalize(value);
            const string runtimeWeaponPrefix = "survivors.weapon.";
            if (normalized.StartsWith(runtimeWeaponPrefix, StringComparison.OrdinalIgnoreCase))
                return "weapon.survivors." + normalized.Substring(runtimeWeaponPrefix.Length);
            return normalized;
        }

        private static string BuildScopedId(string packId, string sourceId, string sourceRecordId)
        {
            return Normalize(packId) + "::" + Normalize(sourceId) + "::" + Normalize(sourceRecordId);
        }

        private static GameContentMetadataDescriptor Metadata(string label, string value)
        {
            return new GameContentMetadataDescriptor(label, value ?? string.Empty);
        }

        private static GameContentMetadataDescriptor Metadata(string label, bool value)
        {
            return Metadata(label, value ? "Yes" : "No");
        }

        private static GameContentMetadataDescriptor Metadata(string label, int value)
        {
            return Metadata(label, value.ToString(CultureInfo.InvariantCulture));
        }

        private static GameContentMetadataDescriptor Metadata(string label, float value, string format = "0.###")
        {
            return Metadata(label, value.ToString(format, CultureInfo.InvariantCulture));
        }

        private static GameContentMetadataDescriptor Metadata(string label, double value)
        {
            return Metadata(label, value.ToString("0.###", CultureInfo.InvariantCulture));
        }

        private static float ReadMetadataFloat(GameContentRecordDescriptor record, string label)
        {
            if (record == null) return 0f;
            string value = record.PlayerFacingMetadata.FirstOrDefault(item =>
                string.Equals(item.Label, label, StringComparison.OrdinalIgnoreCase))?.Value;
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ? parsed : 0f;
        }

        private static void AppendIssues(
            ICollection<GameContentAuthoringValidationIssue> target,
            IEnumerable<GameContentAuthoringValidationIssue> source)
        {
            if (source == null) return;
            foreach (GameContentAuthoringValidationIssue issue in source)
                if (issue != null) target.Add(issue);
        }

        private static IReadOnlyList<GameContentAuthoringValidationIssue> DistinctIssues(
            IEnumerable<GameContentAuthoringValidationIssue> issues)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            return issues.Where(issue => issue != null)
                .Where(issue => keys.Add(issue.Severity + "|" + issue.Path + "|" + issue.Message))
                .ToArray();
        }

        private static bool Contains(string value, string token)
        {
            return !string.IsNullOrWhiteSpace(value) && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string DescribeEnemyRoleBehavior(string role)
        {
            if (string.Equals(role, "Splitter", StringComparison.OrdinalIgnoreCase))
                return "Splits into child enemies on death.";
            if (string.Equals(role, "Summoner", StringComparison.OrdinalIgnoreCase))
                return "Calls support enemies while active.";
            return "No splitter or summoner role behavior.";
        }

        private static string Display(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? Normalize(fallback) : value.Trim();
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private sealed class RecordBuilder
        {
            public string PackScopedId;
            public string SourceRecordId;
            public string SourceId;
            public string PrimaryCategory;
            public string DisplayName;
            public string Description;
            public TextAsset SourceAsset;
            public string SourcePath;
            public string SourceLocator;
            public int Order;
            public readonly HashSet<string> Categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public readonly List<GameContentMetadataDescriptor> Metadata = new List<GameContentMetadataDescriptor>();
            public readonly List<PendingReference> PendingReferences = new List<PendingReference>();
            public readonly List<GameContentRecordReferenceDescriptor> Outbound = new List<GameContentRecordReferenceDescriptor>();
            public readonly List<GameContentRecordReferenceDescriptor> Inbound = new List<GameContentRecordReferenceDescriptor>();
            public readonly List<GameContentAuthoringValidationIssue> Issues = new List<GameContentAuthoringValidationIssue>();
        }

        private readonly struct PendingReference
        {
            public PendingReference(string targetId, string relationship, bool required, bool onlyWhenResolvable, bool scoped)
            {
                TargetId = targetId;
                Relationship = relationship;
                Required = required;
                OnlyWhenResolvable = onlyWhenResolvable;
                Scoped = scoped;
            }

            public string TargetId { get; }
            public string Relationship { get; }
            public bool Required { get; }
            public bool OnlyWhenResolvable { get; }
            public bool Scoped { get; }
        }

        private readonly struct CategoryDefinition
        {
            public CategoryDefinition(string id, string displayName, string description)
            {
                Id = id;
                DisplayName = displayName;
                Description = description;
            }

            public string Id { get; }
            public string DisplayName { get; }
            public string Description { get; }
        }

        [Serializable] private sealed class WeaponLibraryJson { public WeaponRecordJson[] weapons; public ProjectileRecordJson[] projectiles; }
        [Serializable] private sealed class WeaponRecordJson { public string id; public string displayName; public string fireMode; public string tint; public string projectileId; public float cooldownSeconds; public float damage; public float range; public float projectileRadius; public int pierceCount; public int chainCount; public int forkCount; public int returnCount; public int fanCount; public float spreadDegrees; }
        [Serializable] private sealed class ProjectileRecordJson { public string id; public float speed; public float lifetimeSeconds; }
        [Serializable] private sealed class UpgradeLibraryJson { public UpgradeRecordJson[] upgrades; }
        [Serializable] private sealed class UpgradeRecordJson { public string id; public string displayName; public string category; public string rarity; public double weight; public int maxRank; public string effect; public string target; public double amount; public UpgradeEffectRecordJson[] effects; public string description; public string requiredUpgradeId; public int requiredUpgradeRank; public string requiredPassiveUpgradeId; public string requiredOwnedWeaponId; public string affectedContentId; }
        [Serializable] private sealed class UpgradeEffectRecordJson { public string effect; public string target; public double amount; }
        [Serializable] private sealed class EnemyLibraryJson { public EnemyRecordJson[] enemies; }
        [Serializable] private sealed class EnemyRecordJson { public string id; public string displayName; public string role; public string tint; public float health; public float moveSpeed; public float radius; public float contactDamage; public float contactIntervalSeconds; public int experienceDrop; public bool canRecycle; public bool canLeash; public bool canReposition; public bool showOffscreenMarker; public bool showOverheadLifeBar; public bool showBossLifeBar; public string markerStyle; public float softLeashRadius; public float hardRecycleRadius; public float catchUpSpeedMultiplier; public float repositionTimeoutSeconds; }
        [Serializable] private sealed class PickupLibraryJson { public PickupRecordJson[] pickups; }
        [Serializable] private sealed class PickupRecordJson { public string id; public string displayName; public string behavior; }
        [Serializable] private sealed class RelicLibraryJson { public RelicRecordJson[] relics; }
        [Serializable] private sealed class RelicRecordJson { public string id; public string displayName; public string target; public string effect; public string effectKind; public float amount; }
        [Serializable] private sealed class ClassLibraryJson { public string defaultClassId; public ClassRecordJson[] classes; }
        [Serializable] private sealed class ClassRecordJson { public string id; public string displayName; public string startingWeaponId; public string[] startingWeaponIds; public bool unlockedByDefault; public string unlockRewardId; }
        [Serializable] private sealed class RewardLibraryJson { public CurrencyRecordJson[] currencies; public TrackRecordJson[] tracks; public PersistentUpgradeRecordJson[] persistentUpgrades; public RewardRecordJson[] rewards; }
        [Serializable] private sealed class CurrencyRecordJson { public string id; public string displayName; }
        [Serializable] private sealed class TrackRecordJson { public string id; public string displayName; }
        [Serializable] private sealed class PersistentUpgradeRecordJson { public string id; public string displayName; public string target; public string effect; public int maxRank; public float amountPerRank; }
        [Serializable] private sealed class RewardRecordJson { public string id; public string currencyId; public int currencyAmount; public string trackId; public int trackAmount; }
        [Serializable] private sealed class ProgressionLibraryJson { public ProgressionTrackRecordJson[] tracks; }
        [Serializable] private sealed class ProgressionTrackRecordJson { public string id; public string displayName; public string kind; public string classId; public string targetWeaponId; public ProgressionNodeRecordJson[] nodes; }
        [Serializable] private sealed class ProgressionNodeRecordJson { public string id; public string displayName; public string upgradeId; public string kind; public int tier; public int pointCost; public int maxRank; }
        [Serializable] private sealed class RunFlowLibraryJson { public RunFlowProfileRecordJson[] profiles; }
        [Serializable] private sealed class RunFlowProfileRecordJson { public string id; public string displayName; public string description; public float targetDurationSeconds; public float runRewardMultiplier; public bool endlessContinuationEnabled; public float enemySpawnIntervalSeconds; public int enemyMaximumAlive; public float escalationIntervalSeconds; public float minimumEnemySpawnIntervalSeconds; public float enemySpawnIntervalReductionPerEscalation; public float firstEliteSpawnTimeSeconds; public float firstDreadEliteSpawnTimeSeconds; public float minibossSpawnTimeSeconds; public float bossSpawnTimeSeconds; public float survivalVictoryTimeSeconds; public float hordeRushFirstTimeSeconds; public float hordeRushIntervalSeconds; }
        [Serializable] private sealed class UiThemeRecordJson { public string themeName; public string standardModeDisplayName; public string sprintModeDisplayName; public string hudAccentColor; public WorldPresentationRecordJson worldPresentation; public AudioEventRecordJson[] audioEvents; public TutorialStepRecordJson[] tutorialSteps; }
        [Serializable] private sealed class WorldPresentationRecordJson { public bool authored; }
        [Serializable] private sealed class AudioEventRecordJson { public string id; public string category; public float volume; public float throttleSeconds; }
        [Serializable] private sealed class TutorialStepRecordJson { public string id; public string title; public string[] lines; }
    }
}
