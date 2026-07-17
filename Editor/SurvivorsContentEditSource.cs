using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Deucarian.GameContentAuthoring.Editor;
using UnityEditor;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    internal enum SurvivorsEditableRecordKind
    {
        Weapon,
        Projectile,
        Enemy,
        Upgrade,
        Evolution,
        TutorialStep
    }

    internal sealed class SurvivorsContentEditDefinition
    {
        private sealed class FieldSpec
        {
            public FieldSpec(
                GameContentFieldDescriptor descriptor,
                bool propertyRequired,
                GameContentFieldType? storageFieldType = null)
            {
                Descriptor = descriptor;
                PropertyRequired = propertyRequired;
                StorageFieldType = storageFieldType ?? descriptor.FieldType;
            }

            public GameContentFieldDescriptor Descriptor { get; }
            public bool PropertyRequired { get; }
            public GameContentFieldType StorageFieldType { get; }
        }

        private SurvivorsContentEditDefinition(
            SurvivorsJsonRecordLocator locator,
            SurvivorsEditableRecordKind recordKind,
            IReadOnlyList<GameContentFieldDescriptor> fields,
            IReadOnlyDictionary<string, SurvivorsJsonScalarToken> tokens,
            IReadOnlyDictionary<string, SurvivorsJsonCollectionToken> collectionTokens,
            IReadOnlyDictionary<string, SurvivorsJsonStructuredCollectionToken> structuredCollectionTokens,
            IReadOnlyDictionary<string, GameContentFieldValue> values)
        {
            Locator = locator;
            RecordKind = recordKind;
            Fields = fields;
            Tokens = tokens;
            CollectionTokens = collectionTokens;
            StructuredCollectionTokens = structuredCollectionTokens;
            Values = values;
        }

        public SurvivorsJsonRecordLocator Locator { get; }
        public SurvivorsEditableRecordKind RecordKind { get; }
        public IReadOnlyList<GameContentFieldDescriptor> Fields { get; }
        public IReadOnlyDictionary<string, SurvivorsJsonScalarToken> Tokens { get; }
        public IReadOnlyDictionary<string, SurvivorsJsonCollectionToken> CollectionTokens { get; }
        public IReadOnlyDictionary<string, SurvivorsJsonStructuredCollectionToken> StructuredCollectionTokens { get; }
        public IReadOnlyDictionary<string, GameContentFieldValue> Values { get; }
        public int EditableFieldCount => Fields.Count(field => !field.IsReadOnly);

        public static bool TryCreate(
            GameContentRecordDescriptor record,
            SurvivorsLosslessJsonDocument document,
            out SurvivorsContentEditDefinition definition,
            out string error)
        {
            definition = null;
            if (!TryBuildLocator(record, out SurvivorsJsonRecordLocator locator, out SurvivorsEditableRecordKind kind, out error))
                return false;
            if (!SurvivorsJsonRecordNavigator.TryLocateRecord(document, locator, out SurvivorsJsonNode recordNode, out error))
                return false;

            var fields = new List<GameContentFieldDescriptor>();
            var tokens = new Dictionary<string, SurvivorsJsonScalarToken>(StringComparer.Ordinal);
            var collectionTokens = new Dictionary<string, SurvivorsJsonCollectionToken>(StringComparer.Ordinal);
            var structuredCollectionTokens =
                new Dictionary<string, SurvivorsJsonStructuredCollectionToken>(StringComparer.Ordinal);
            var values = new Dictionary<string, GameContentFieldValue>(StringComparer.Ordinal);
            foreach (FieldSpec spec in BuildFieldSpecs(kind))
            {
                GameContentFieldDescriptor field = spec.Descriptor;
                if (field.FieldType == GameContentFieldType.OrderedStructuredCollection)
                {
                    if (!SurvivorsJsonRecordNavigator.TryReadDirectStructuredCollection(
                            document,
                            recordNode,
                            field,
                            spec.PropertyRequired,
                            out SurvivorsJsonStructuredCollectionToken structuredToken,
                            out error))
                        return false;
                    if (structuredToken == null) continue;
                    fields.Add(field);
                    structuredCollectionTokens.Add(field.FieldId, structuredToken);
                    values.Add(field.FieldId, structuredToken.Value);
                    continue;
                }
                if (field.FieldType == GameContentFieldType.OrderedScalarCollection)
                {
                    if (!SurvivorsJsonRecordNavigator.TryReadDirectStringCollection(
                            recordNode,
                            field.FieldId,
                            spec.PropertyRequired,
                            out SurvivorsJsonCollectionToken collectionToken,
                            out error))
                        return false;
                    if (collectionToken == null) continue;
                    fields.Add(field);
                    collectionTokens.Add(field.FieldId, collectionToken);
                    values.Add(field.FieldId, collectionToken.Value);
                    continue;
                }

                if (!SurvivorsJsonRecordNavigator.TryReadDirectScalar(
                        document,
                        recordNode,
                        field.FieldId,
                        spec.StorageFieldType,
                        spec.PropertyRequired,
                        out SurvivorsJsonScalarToken token,
                        out error))
                    return false;
                if (token == null) continue;
                fields.Add(field);
                tokens.Add(field.FieldId, token);
                values.Add(
                    field.FieldId,
                    field.FieldType == GameContentFieldType.RecordReference
                        ? ResolveEvolutionPassiveReference(record, document, token.Value.StringValue)
                        : token.Value);
            }

            if (fields.All(field => field.IsReadOnly))
            {
                error = "The selected record has no supported direct editable fields.";
                return false;
            }

            definition = new SurvivorsContentEditDefinition(
                locator,
                kind,
                fields,
                tokens,
                collectionTokens,
                structuredCollectionTokens,
                values);
            return true;
        }

        private static bool TryBuildLocator(
            GameContentRecordDescriptor record,
            out SurvivorsJsonRecordLocator locator,
            out SurvivorsEditableRecordKind kind,
            out string error)
        {
            locator = null;
            kind = default;
            error = string.Empty;
            if (record?.CanonicalKey == null || !record.CanonicalKey.IsValid)
            {
                error = "A canonical Survivors record is required.";
                return false;
            }

            string sourceId = record.CanonicalKey.SourceId;
            string collection;
            if (string.Equals(sourceId, SurvivorsContentPackIndex.WeaponsSourceId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(record.CategoryId, "weapons", StringComparison.OrdinalIgnoreCase))
            {
                collection = "weapons";
                kind = SurvivorsEditableRecordKind.Weapon;
            }
            else if (string.Equals(sourceId, SurvivorsContentPackIndex.WeaponsSourceId, StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(record.CategoryId, "projectiles", StringComparison.OrdinalIgnoreCase))
            {
                collection = "projectiles";
                kind = SurvivorsEditableRecordKind.Projectile;
            }
            else if (string.Equals(sourceId, SurvivorsContentPackIndex.EnemiesSourceId, StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(record.CategoryId, "enemies", StringComparison.OrdinalIgnoreCase))
            {
                collection = "enemies";
                kind = SurvivorsEditableRecordKind.Enemy;
            }
            else if (string.Equals(sourceId, SurvivorsContentPackIndex.UpgradesSourceId, StringComparison.OrdinalIgnoreCase) &&
                      string.Equals(record.CategoryId, "evolutions", StringComparison.OrdinalIgnoreCase))
            {
                collection = "upgrades";
                kind = SurvivorsEditableRecordKind.Evolution;
            }
            else if (string.Equals(sourceId, SurvivorsContentPackIndex.UpgradesSourceId, StringComparison.OrdinalIgnoreCase) &&
                     record.HasCapability(GameContentRecordCapabilities.Upgrade))
            {
                collection = "upgrades";
                kind = SurvivorsEditableRecordKind.Upgrade;
            }
            else if ((string.Equals(sourceId, SurvivorsContentPackIndex.PrimaryThemeSourceId, StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(sourceId, SurvivorsContentPackIndex.AlternateThemeSourceId, StringComparison.OrdinalIgnoreCase)) &&
                     string.Equals(record.CategoryId, "tutorial", StringComparison.OrdinalIgnoreCase))
            {
                collection = "tutorialSteps";
                kind = SurvivorsEditableRecordKind.TutorialStep;
            }
            else
            {
                error = "Only existing weapon, projectile, enemy, Upgrade Effects, evolution prerequisite, and tutorial Lines records are editable.";
                return false;
            }

            locator = new SurvivorsJsonRecordLocator(
                sourceId,
                collection,
                record.CanonicalKey.SourceRecordId,
                kind.ToString());
            return true;
        }

        private static IReadOnlyList<FieldSpec> BuildFieldSpecs(SurvivorsEditableRecordKind kind)
        {
            switch (kind)
            {
                case SurvivorsEditableRecordKind.Weapon:
                    return new[]
                    {
                        ReadOnly("id", "survivors.identity.id", "Stable ID", "Stable record identity used by authored references.", 0, "Identity", true),
                        ReadOnly("fireMode", "survivors.weapon.fire-mode", "Fire Mode", "Weapon archetype is structural and remains read-only.", 5, "Identity", true),
                        ReadOnly("projectileId", "survivors.weapon.projectile-id", "Projectile ID", "Projectile references remain read-only.", 6, "Targeting", false),
                        Text("displayName", "survivors.presentation.display-name", "Display Name", "Player-facing weapon name.", 10, "Presentation", true),
                        PositiveNumber("cooldownSeconds", "survivors.weapon.cooldown", "Cooldown", "Seconds between attacks.", 20, "Combat", true),
                        PositiveNumber("damage", "survivors.weapon.damage", "Damage", "Base authored weapon damage.", 30, "Combat", true),
                        PositiveNumber("range", "survivors.weapon.range", "Range", "Direct authored attack range where this archetype owns one.", 40, "Targeting", false),
                        PositiveNumber("radius", "survivors.weapon.radius", "Area Radius", "Direct authored area radius where this archetype owns one.", 45, "Combat", false),
                        PositiveNumber("projectileRadius", "survivors.weapon.projectile-radius", "Projectile Radius", "Direct projectile collision radius where this weapon owns one.", 50, "Targeting", false)
                    };
                case SurvivorsEditableRecordKind.Projectile:
                    return new[]
                    {
                        ReadOnly("id", "survivors.identity.id", "Stable ID", "Stable projectile identity used by authored references.", 0, "Identity", true),
                        PositiveNumber("speed", "survivors.projectile.speed", "Speed", "Authored projectile travel speed.", 10, "Motion", true),
                        PositiveNumber("lifetimeSeconds", "survivors.projectile.lifetime", "Lifetime", "Authored projectile lifetime in seconds.", 20, "Motion", true)
                    };
                case SurvivorsEditableRecordKind.Enemy:
                    return new[]
                    {
                        ReadOnly("id", "survivors.identity.id", "Stable ID", "Stable enemy identity used by authored waves and rewards.", 0, "Identity", true),
                        ReadOnly("role", "survivors.enemy.role", "Role", "Enemy role controls authored lifecycle behavior and remains read-only.", 5, "Identity", true),
                        Text("displayName", "survivors.presentation.display-name", "Display Name", "Player-facing enemy name.", 10, "Presentation", true),
                        PositiveNumber("health", "survivors.enemy.health", "Health", "Authored maximum health.", 20, "Combat", true),
                        PositiveNumber("contactDamage", "survivors.enemy.contact-damage", "Contact Damage", "Damage dealt on contact.", 30, "Combat", true),
                        PositiveNumber("contactIntervalSeconds", "survivors.enemy.contact-interval", "Contact Interval", "Seconds between contact hits.", 40, "Combat", true),
                        PositiveNumber("moveSpeed", "survivors.enemy.move-speed", "Move Speed", "Authored movement speed.", 50, "Movement", true),
                        PositiveNumber("radius", "survivors.enemy.radius", "Radius", "Authored enemy collision radius.", 60, "Movement", true),
                        PositiveInteger("experienceDrop", "survivors.enemy.experience-drop", "Experience Drop", "XP awarded when this enemy dies.", 70, "Reward", true)
                    };
                case SurvivorsEditableRecordKind.Upgrade:
                    return new[]
                    {
                        ReadOnly("id", "survivors.identity.id", "Stable ID", "Stable Upgrade identity used by authored references.", 0, "Identity", true),
                        new FieldSpec(SurvivorsUpgradeEffectEditing.Field, true)
                    };
                case SurvivorsEditableRecordKind.Evolution:
                    return new[]
                    {
                        ReadOnly("id", "survivors.identity.id", "Stable ID", "Stable evolution identity used by authored progression and runtime metadata.", 0, "Identity", true),
                        RecordReference(
                            "requiredPassiveUpgradeId",
                            "survivors.evolution.required-passive",
                            "Required Passive",
                            "Same-pack Passive upgrade required before this evolution can be drafted.",
                            10,
                            "Prerequisites",
                            true),
                        new FieldSpec(SurvivorsUpgradeEffectEditing.Field, true)
                    };
                case SurvivorsEditableRecordKind.TutorialStep:
                    return new[]
                    {
                        ReadOnly("id", "survivors.identity.id", "Stable ID", "Stable tutorial-step identity used by authored themes and runtime lookup.", 0, "Identity", true),
                        TutorialLines()
                    };
                default:
                    return Array.Empty<FieldSpec>();
            }
        }

        private static GameContentFieldValue ResolveEvolutionPassiveReference(
            GameContentRecordDescriptor sourceRecord,
            SurvivorsLosslessJsonDocument document,
            string targetId)
        {
            string originalReference = string.IsNullOrWhiteSpace(targetId) ? "<empty JSON reference>" : targetId;
            GameContentRecordKey targetKey = string.IsNullOrWhiteSpace(targetId)
                ? null
                : new GameContentRecordKey(
                    sourceRecord.CanonicalKey.OwningPackageId,
                    sourceRecord.CanonicalKey.PackId,
                    targetId,
                    SurvivorsContentPackIndex.UpgradesSourceId);
            var locator = new SurvivorsJsonRecordLocator(
                SurvivorsContentPackIndex.UpgradesSourceId,
                "upgrades",
                targetId,
                "Passive");
            if (!SurvivorsJsonRecordNavigator.TryLocateRecord(document, locator, out SurvivorsJsonNode target, out string locateError))
            {
                return GameContentFieldValue.FromRecordReference(
                    GameContentRecordReferenceValue.Broken(originalReference, locateError, targetKey));
            }

            if (!SurvivorsJsonRecordNavigator.TryReadDirectScalar(
                    document,
                    target,
                    "category",
                    GameContentFieldType.String,
                    true,
                    out SurvivorsJsonScalarToken category,
                    out string categoryError))
            {
                return GameContentFieldValue.FromRecordReference(
                    GameContentRecordReferenceValue.Broken(originalReference, categoryError, targetKey));
            }
            if (!string.Equals(category.Value.StringValue, "Passive", StringComparison.OrdinalIgnoreCase))
            {
                return GameContentFieldValue.FromRecordReference(
                    GameContentRecordReferenceValue.Broken(
                        originalReference,
                        "The authored target exists but is not a Passive upgrade.",
                        targetKey));
            }

            string displayName = targetId;
            if (SurvivorsJsonRecordNavigator.TryReadDirectScalar(
                    document,
                    target,
                    "displayName",
                    GameContentFieldType.String,
                    false,
                    out SurvivorsJsonScalarToken displayNameToken,
                    out _) &&
                displayNameToken != null)
                displayName = displayNameToken.Value.StringValue;
            return GameContentFieldValue.FromRecordReference(
                GameContentRecordReferenceValue.Resolved(
                    targetKey,
                    displayName,
                    SurvivorsContentPackIndex.UpgradesSourceId));
        }

        private static FieldSpec ReadOnly(
            string fieldId,
            string semanticId,
            string displayName,
            string description,
            int order,
            string group,
            bool propertyRequired)
        {
            return new FieldSpec(
                new GameContentFieldDescriptor(
                    fieldId,
                    semanticId,
                    displayName,
                    description,
                    GameContentFieldType.String,
                    true,
                    "Stable identities, archetypes, and references are read-only in this milestone.",
                    order,
                    group,
                    propertyRequired),
                propertyRequired);
        }

        private static FieldSpec Text(
            string fieldId,
            string semanticId,
            string displayName,
            string description,
            int order,
            string group,
            bool propertyRequired)
        {
            return new FieldSpec(
                new GameContentFieldDescriptor(
                    fieldId,
                    semanticId,
                    displayName,
                    description,
                    GameContentFieldType.String,
                    order: order,
                    group: group,
                    required: true,
                    minimumLength: 1,
                    maximumLength: 128),
                propertyRequired);
        }

        private static FieldSpec RecordReference(
            string fieldId,
            string semanticId,
            string displayName,
            string description,
            int order,
            string group,
            bool propertyRequired)
        {
            return new FieldSpec(
                new GameContentFieldDescriptor(
                    fieldId,
                    semanticId,
                    displayName,
                    description,
                    GameContentFieldType.RecordReference,
                    order: order,
                    group: group,
                    required: true,
                    recordReference: new GameContentRecordReferenceFieldDescriptor(
                        "Passive Upgrade",
                        new[]
                        {
                            GameContentRecordCapabilities.Upgrade,
                            GameContentRecordCapabilities.Passive
                        },
                        GameContentReferencePackPolicy.SameSelectedPack,
                        GameContentReferenceRuntimeImpact.Refresh | GameContentReferenceRuntimeImpact.Rebind,
                        allowClear: false)),
                propertyRequired,
                GameContentFieldType.String);
        }

        private static FieldSpec TutorialLines()
        {
            var item = new GameContentFieldDescriptor(
                "lines.item",
                "survivors.tutorial.lines.item",
                "Line",
                "One player-facing tutorial line. Blank or whitespace-only lines are invalid.",
                GameContentFieldType.String,
                required: true,
                minimumLength: 1);
            return new FieldSpec(
                new GameContentFieldDescriptor(
                    "lines",
                    "survivors.tutorial.lines",
                    "Lines",
                    "Player-facing tutorial copy in displayed order. Adding or removing a line does not create or delete the tutorial step; IDs and other structure remain read-only. Imported-sample refresh may replace project-owned edits.",
                    GameContentFieldType.OrderedScalarCollection,
                    order: 10,
                    group: "Tutorial",
                    required: true,
                    collection: new GameContentCollectionFieldDescriptor(
                        item,
                        1,
                        SurvivorsContentValidator.MaximumTutorialLineCount,
                        true,
                        "The tutorial panel displays lines in this exact order.",
                        GameContentReferenceRuntimeImpact.Refresh |
                        GameContentReferenceRuntimeImpact.Rebind |
                        GameContentReferenceRuntimeImpact.Restart)),
                true);
        }

        private static FieldSpec PositiveNumber(
            string fieldId,
            string semanticId,
            string displayName,
            string description,
            int order,
            string group,
            bool propertyRequired)
        {
            return new FieldSpec(
                new GameContentFieldDescriptor(
                    fieldId,
                    semanticId,
                    displayName,
                    description,
                    GameContentFieldType.Number,
                    order: order,
                    group: group,
                    minimumNumber: double.Epsilon,
                    maximumNumber: float.MaxValue),
                propertyRequired);
        }

        private static FieldSpec PositiveInteger(
            string fieldId,
            string semanticId,
            string displayName,
            string description,
            int order,
            string group,
            bool propertyRequired)
        {
            return new FieldSpec(
                new GameContentFieldDescriptor(
                    fieldId,
                    semanticId,
                    displayName,
                    description,
                    GameContentFieldType.Integer,
                    order: order,
                    group: group,
                    minimumNumber: 1d,
                    maximumNumber: int.MaxValue),
                propertyRequired);
        }
    }

    internal sealed class SurvivorsProjectOwnedSourcePath
    {
        public SurvivorsProjectOwnedSourcePath(
            string assetPath,
            string fullPath,
            string importedSampleRootAssetPath,
            string assetGuid)
        {
            AssetPath = assetPath;
            FullPath = fullPath;
            ImportedSampleRootAssetPath = importedSampleRootAssetPath;
            AssetGuid = assetGuid;
        }

        public string AssetPath { get; }
        public string FullPath { get; }
        public string ImportedSampleRootAssetPath { get; }
        public string AssetGuid { get; }
    }

    internal static class SurvivorsProjectOwnedSourcePolicy
    {
        public static bool TryResolve(
            GameContentPackManifest manifest,
            GameContentPackSourceReference source,
            out SurvivorsProjectOwnedSourcePath resolved,
            out string error)
        {
            resolved = null;
            error = string.Empty;
            if (manifest == null || source?.TextAsset == null)
            {
                error = "The selected manifest source is missing.";
                return false;
            }

            string manifestPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(manifest));
            string sourcePath = NormalizeAssetPath(AssetDatabase.GetAssetPath(source.TextAsset));
            if (!IsSafeAssetPath(manifestPath) || !IsSafeAssetPath(sourcePath))
            {
                error = "The manifest and JSON source must resolve to safe project-relative asset paths.";
                return false;
            }
            if (!manifestPath.StartsWith("Assets/Samples/", StringComparison.OrdinalIgnoreCase))
            {
                error = "Only project-owned imported samples under Assets/Samples can be edited. Package and package-cache sources remain read-only.";
                return false;
            }

            int contentPacksMarker = manifestPath.LastIndexOf("/ContentPacks/", StringComparison.OrdinalIgnoreCase);
            if (contentPacksMarker <= "Assets/Samples".Length)
            {
                error = "The imported manifest is not inside the expected sample ContentPacks folder.";
                return false;
            }

            string importedRoot = manifestPath.Substring(0, contentPacksMarker);
            string declaredContentRoot = importedRoot + "/Content/";
            if (!sourcePath.StartsWith(declaredContentRoot, StringComparison.OrdinalIgnoreCase))
            {
                error = "The selected JSON source is outside the imported sample's declared Content root.";
                return false;
            }

            string projectRoot;
            string assetsRoot;
            string fullPath;
            string fullImportedRoot;
            try
            {
                projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                assetsRoot = Path.GetFullPath(Application.dataPath);
                fullPath = Path.GetFullPath(Path.Combine(projectRoot, sourcePath.Replace('/', Path.DirectorySeparatorChar)));
                fullImportedRoot = Path.GetFullPath(Path.Combine(projectRoot, importedRoot.Replace('/', Path.DirectorySeparatorChar)));
            }
            catch (Exception exception)
            {
                error = "The imported JSON source path could not be canonicalized: " + exception.GetBaseException().Message;
                return false;
            }

            if (!IsBelow(fullPath, assetsRoot) || !IsBelow(fullPath, fullImportedRoot))
            {
                error = "The canonical JSON source path escapes the project-owned imported sample root.";
                return false;
            }
            if (!File.Exists(fullPath))
            {
                error = "The imported JSON source file is missing: " + sourcePath + ".";
                return false;
            }
            if (ContainsReparsePoint(projectRoot, fullPath, out string reparsePath))
            {
                error = "Editing is disabled because the source path crosses a symbolic link, junction, or reparse point: " + reparsePath + ".";
                return false;
            }

            try
            {
                if ((File.GetAttributes(fullPath) & FileAttributes.ReadOnly) != 0)
                {
                    error = "The imported JSON source is read-only: " + sourcePath + ".";
                    return false;
                }
                using (new FileStream(fullPath, FileMode.Open, FileAccess.Write, FileShare.Read))
                {
                }
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                error = "The imported JSON source is not writable: " + exception.GetBaseException().Message;
                return false;
            }

            string guid = AssetDatabase.AssetPathToGUID(sourcePath);
            if (string.IsNullOrWhiteSpace(guid))
            {
                error = "The imported JSON source has no stable AssetDatabase GUID.";
                return false;
            }

            resolved = new SurvivorsProjectOwnedSourcePath(sourcePath, fullPath, importedRoot, guid);
            return true;
        }

        public static bool IsImportedManifest(GameContentPackManifest manifest, out string reason)
        {
            reason = string.Empty;
            if (manifest == null)
            {
                reason = "The content-pack manifest is missing.";
                return false;
            }
            string path = NormalizeAssetPath(AssetDatabase.GetAssetPath(manifest));
            if (!IsSafeAssetPath(path) || !path.StartsWith("Assets/Samples/", StringComparison.OrdinalIgnoreCase))
            {
                reason = "Only a project-owned imported sample under Assets/Samples can enable JSON editing.";
                return false;
            }
            if (path.LastIndexOf("/ContentPacks/", StringComparison.OrdinalIgnoreCase) < 0)
            {
                reason = "The imported manifest must live under its sample ContentPacks folder.";
                return false;
            }
            return true;
        }

        private static bool IsSafeAssetPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path)) return false;
            string[] segments = path.Split('/');
            return segments.All(segment => !string.IsNullOrWhiteSpace(segment) && segment != "." && segment != "..");
        }

        private static string NormalizeAssetPath(string path)
        {
            return string.IsNullOrWhiteSpace(path) ? string.Empty : path.Trim().Replace('\\', '/');
        }

        private static bool IsBelow(string candidate, string root)
        {
            string normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string normalizedCandidate = candidate.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            StringComparison comparison = Path.DirectorySeparatorChar == '\\'
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            return normalizedCandidate.StartsWith(normalizedRoot, comparison);
        }

        private static bool ContainsReparsePoint(string projectRoot, string fullPath, out string path)
        {
            path = string.Empty;
            string root = projectRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string relative = fullPath.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string current = root;
            IEnumerable<string> segments = relative.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string segment in new[] { string.Empty }.Concat(segments))
            {
                if (!string.IsNullOrEmpty(segment)) current = Path.Combine(current, segment);
                if (!Directory.Exists(current) && !File.Exists(current)) continue;
                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) == 0) continue;
                path = current;
                return true;
            }
            return false;
        }
    }

    internal static class SurvivorsContentEditHash
    {
        public static string Sha256(byte[] bytes)
        {
            using (SHA256 algorithm = SHA256.Create())
            {
                return ToHex(algorithm.ComputeHash(bytes ?? Array.Empty<byte>()));
            }
        }

        public static string Sha256(string text)
        {
            return Sha256(new UTF8Encoding(false, true).GetBytes(text ?? string.Empty));
        }

        private static string ToHex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            for (int index = 0; index < bytes.Length; index++)
                builder.Append(bytes[index].ToString("x2", CultureInfo.InvariantCulture));
            return builder.ToString();
        }
    }

    internal static class SurvivorsContentSourceRevision
    {
        public const string BackendSchemaVersion = "survivors-lossless-json-v2";

        public static GameContentSourceRevision Create(
            GameContentPackManifest manifest,
            GameContentRecordKey recordKey,
            SurvivorsProjectOwnedSourcePath sourcePath,
            byte[] exactBytes)
        {
            string sourceListFingerprint = BuildSourceListFingerprint(manifest);
            string token = BackendSchemaVersion +
                           "|bytes=" + SurvivorsContentEditHash.Sha256(exactBytes) +
                           "|guid=" + (sourcePath?.AssetGuid ?? string.Empty) +
                           "|path=" + (sourcePath?.AssetPath ?? string.Empty) +
                           "|pack=" + (manifest?.StableKey ?? string.Empty) +
                           "|sources=" + sourceListFingerprint +
                           "|record=" + (recordKey?.StableKey ?? string.Empty);
            return new GameContentSourceRevision(token);
        }

        private static string BuildSourceListFingerprint(GameContentPackManifest manifest)
        {
            if (manifest == null) return SurvivorsContentEditHash.Sha256(string.Empty);
            var builder = new StringBuilder();
            builder.Append(manifest.StableKey).Append('|').Append(manifest.SchemaVersion).AppendLine();
            for (int index = 0; index < manifest.ContentSources.Count; index++)
            {
                GameContentPackSourceReference source = manifest.ContentSources[index];
                string path = source?.TextAsset == null
                    ? string.Empty
                    : AssetDatabase.GetAssetPath(source.TextAsset).Replace('\\', '/');
                string guid = string.IsNullOrWhiteSpace(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path);
                builder.Append(index).Append('|')
                    .Append(source?.SourceId ?? string.Empty).Append('|')
                    .Append(source?.SourceKind ?? string.Empty).Append('|')
                    .Append(guid).Append('|')
                    .Append(path).Append('|')
                    .Append(source?.DisplayLabel ?? string.Empty).Append('|')
                    .Append(source?.CategoryHint ?? string.Empty).Append('|')
                    .Append(source != null && source.Required ? "required" : "optional")
                    .AppendLine();
            }
            return SurvivorsContentEditHash.Sha256(builder.ToString());
        }
    }

    internal sealed class SurvivorsEditableSource
    {
        private SurvivorsEditableSource(
            GameContentPackManifest manifest,
            GameContentRecordDescriptor record,
            GameContentPackSourceReference sourceReference,
            SurvivorsProjectOwnedSourcePath sourcePath,
            byte[] exactBytes,
            SurvivorsLosslessJsonDocument document,
            SurvivorsContentEditDefinition definition,
            GameContentSourceRevision revision,
            GameContentSourceTarget sourceTarget)
        {
            Manifest = manifest;
            Record = record;
            SourceReference = sourceReference;
            SourcePath = sourcePath;
            ExactBytes = exactBytes;
            Document = document;
            Definition = definition;
            Revision = revision;
            SourceTarget = sourceTarget;
        }

        public GameContentPackManifest Manifest { get; }
        public GameContentRecordDescriptor Record { get; }
        public GameContentPackSourceReference SourceReference { get; }
        public SurvivorsProjectOwnedSourcePath SourcePath { get; }
        public byte[] ExactBytes { get; }
        public SurvivorsLosslessJsonDocument Document { get; }
        public SurvivorsContentEditDefinition Definition { get; }
        public GameContentSourceRevision Revision { get; }
        public GameContentSourceTarget SourceTarget { get; }
        public string SourceId => Record.CanonicalKey.SourceId;
        public string ExactHash => SurvivorsContentEditHash.Sha256(ExactBytes);

        public GameContentSourceRevision CreateRevision(byte[] exactBytes)
        {
            return SurvivorsContentSourceRevision.Create(Manifest, Record.CanonicalKey, SourcePath, exactBytes);
        }

        public static bool TryCreate(
            GameContentPackManifest manifest,
            GameContentRecordDescriptor record,
            bool requireAtomicReplacement,
            out SurvivorsEditableSource source,
            out string error)
        {
            source = null;
            error = string.Empty;
            if (!IsSupportedPack(manifest))
            {
                error = "Only the imported Basic Survivors and Neon Arcana packs support approved JSON field editing.";
                return false;
            }
            if (record?.CanonicalKey == null ||
                !string.Equals(record.CanonicalKey.OwningPackageId, manifest.OwningPackageId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(record.CanonicalKey.PackId, manifest.PackId, StringComparison.OrdinalIgnoreCase))
            {
                error = "The selected record is not owned by the selected Survivors pack.";
                return false;
            }

            string sourceId = record.CanonicalKey.SourceId;
            if (!string.Equals(sourceId, SurvivorsContentPackIndex.WeaponsSourceId, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sourceId, SurvivorsContentPackIndex.EnemiesSourceId, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sourceId, SurvivorsContentPackIndex.UpgradesSourceId, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sourceId, SurvivorsContentPackIndex.PrimaryThemeSourceId, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sourceId, SurvivorsContentPackIndex.AlternateThemeSourceId, StringComparison.OrdinalIgnoreCase))
            {
                error = "Only the weapons, enemies, approved evolution prerequisite, and authored tutorial theme JSON sources are editable in this milestone.";
                return false;
            }
            if (!manifest.TryGetSource(sourceId, out GameContentPackSourceReference sourceReference) || sourceReference?.TextAsset == null)
            {
                error = "The selected record's authored JSON source is missing.";
                return false;
            }
            if (!string.Equals(sourceReference.SourceKind, "json", StringComparison.OrdinalIgnoreCase))
            {
                error = "The selected record is not backed by an authored JSON source.";
                return false;
            }
            if (!SurvivorsProjectOwnedSourcePolicy.TryResolve(manifest, sourceReference, out SurvivorsProjectOwnedSourcePath sourcePath, out error))
                return false;
            if (requireAtomicReplacement && !SurvivorsAtomicFile.TryConfirmSupport(out error)) return false;

            byte[] exactBytes;
            try
            {
                exactBytes = File.ReadAllBytes(sourcePath.FullPath);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                error = "The exact JSON source bytes could not be read: " + exception.GetBaseException().Message;
                return false;
            }
            if (!SurvivorsLosslessJsonDocument.TryParse(exactBytes, out SurvivorsLosslessJsonDocument document, out error))
                return false;
            if (!SurvivorsContentEditDefinition.TryCreate(record, document, out SurvivorsContentEditDefinition definition, out error))
                return false;

            GameContentSourceRevision revision = SurvivorsContentSourceRevision.Create(
                manifest,
                record.CanonicalKey,
                sourcePath,
                exactBytes);
            string lockPath = Path.DirectorySeparatorChar == '\\'
                ? sourcePath.FullPath.ToLowerInvariant()
                : sourcePath.FullPath;
            var target = new GameContentSourceTarget(
                "survivors-json::" + SurvivorsContentEditHash.Sha256(lockPath),
                manifest.DisplayName + " / " + sourceReference.DisplayLabel,
                sourcePath.AssetPath,
                sourcePath.AssetGuid + "::" + sourceId);
            source = new SurvivorsEditableSource(
                manifest,
                record,
                sourceReference,
                sourcePath,
                exactBytes,
                document,
                definition,
                revision,
                target);
            return true;
        }

        public static bool CanAdvertiseEditing(
            GameContentPackManifest manifest,
            GameContentPackSourceKind sourceKind,
            out string reason)
        {
            if (!IsSupportedPack(manifest))
            {
                reason = "Only Basic Survivors and Neon Arcana opt into existing-record editing.";
                return false;
            }
            if (sourceKind != GameContentPackSourceKind.ImportedSample)
            {
                reason = "Package source remains read-only. Import the sample to create a project-owned copy under Assets/Samples.";
                return false;
            }
            if (!SurvivorsProjectOwnedSourcePolicy.IsImportedManifest(manifest, out reason)) return false;
            return SurvivorsAtomicFile.TryConfirmSupport(out reason);
        }

        private static bool IsSupportedPack(GameContentPackManifest manifest)
        {
            if (manifest == null ||
                !string.Equals(manifest.OwningPackageId, SurvivorsContentPackProvider.OwningPackageId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(manifest.ProviderId, SurvivorsContentPackProvider.StableProviderId, StringComparison.OrdinalIgnoreCase))
                return false;
            return string.Equals(manifest.PackId, "basic-survivors", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(manifest.PackId, "neon-arcana", StringComparison.OrdinalIgnoreCase);
        }
    }

    internal sealed class SurvivorsEditTransactionHooks
    {
        public Action BeforeAtomicReplace;
        public Action AfterAtomicReplace;
        public Action BeforeImport;
        public Action AfterImport;
        public SurvivorsAtomicFileOperations AtomicFileOperations;
    }

    internal enum SurvivorsAtomicReplaceFailureDisposition
    {
        RetryScheduled,
        RetryExhausted,
        NonRetryable
    }

    internal enum SurvivorsAtomicReplaceFinalDisposition
    {
        Succeeded,
        SucceededAfterRetry,
        NonRetryableFailure,
        RetryExhausted,
        RetryPreconditionFailed,
        Aborted
    }

    internal sealed class SurvivorsAtomicReplaceFailure
    {
        public SurvivorsAtomicReplaceFailure(
            string operationStage,
            string destinationPath,
            int attempt,
            int maximumAttempts,
            Exception exception,
            int? win32ErrorCode,
            bool retryable,
            SurvivorsAtomicReplaceFailureDisposition disposition)
        {
            OperationStage = operationStage ?? string.Empty;
            DestinationPath = destinationPath ?? string.Empty;
            Attempt = attempt;
            MaximumAttempts = maximumAttempts;
            ExceptionType = exception?.GetType().FullName ?? "Unavailable";
            ExceptionMessage = exception?.GetBaseException().Message ?? string.Empty;
            HResult = exception?.HResult ?? 0;
            Win32ErrorCode = win32ErrorCode;
            Retryable = retryable;
            Disposition = disposition;
        }

        public string OperationStage { get; }
        public string DestinationPath { get; }
        public int Attempt { get; }
        public int MaximumAttempts { get; }
        public string ExceptionType { get; }
        public string ExceptionMessage { get; }
        public int HResult { get; }
        public int? Win32ErrorCode { get; }
        public bool Retryable { get; }
        public SurvivorsAtomicReplaceFailureDisposition Disposition { get; }

        public string ToDiagnosticText()
        {
            string win32 = Win32ErrorCode.HasValue
                ? Win32ErrorCode.Value.ToString(CultureInfo.InvariantCulture)
                : "unavailable";
            return "[stage=" + OperationStage +
                   "; destination='" + DestinationPath +
                   "'; attempt=" + Attempt.ToString(CultureInfo.InvariantCulture) +
                   "/" + MaximumAttempts.ToString(CultureInfo.InvariantCulture) +
                   "; exception=" + ExceptionType +
                   "; HResult=0x" + unchecked((uint)HResult).ToString("X8", CultureInfo.InvariantCulture) +
                   "; Win32=" + win32 +
                   "; disposition=" + Disposition +
                   "] " + ExceptionMessage;
        }
    }

    internal sealed class SurvivorsAtomicReplaceResult
    {
        private SurvivorsAtomicReplaceResult(
            bool succeeded,
            int attemptCount,
            SurvivorsAtomicReplaceFinalDisposition finalDisposition,
            string finalReason,
            IEnumerable<SurvivorsAtomicReplaceFailure> failures)
        {
            Succeeded = succeeded;
            AttemptCount = attemptCount;
            FinalDisposition = finalDisposition;
            FinalReason = finalReason ?? string.Empty;
            Failures = (failures ?? Array.Empty<SurvivorsAtomicReplaceFailure>()).ToArray();
            Message = BuildMessage();
        }

        public bool Succeeded { get; }
        public int AttemptCount { get; }
        public SurvivorsAtomicReplaceFinalDisposition FinalDisposition { get; }
        public string FinalReason { get; }
        public IReadOnlyList<SurvivorsAtomicReplaceFailure> Failures { get; }
        public string Message { get; }
        public bool HadReplacementFailures => Failures.Count > 0;
        public bool IsTransientAvailabilityFailure =>
            !Succeeded && Failures.Count > 0 && Failures.All(failure => failure.Retryable);

        public static SurvivorsAtomicReplaceResult Success(
            int attemptCount,
            IEnumerable<SurvivorsAtomicReplaceFailure> failures)
        {
            SurvivorsAtomicReplaceFailure[] captured =
                (failures ?? Array.Empty<SurvivorsAtomicReplaceFailure>()).ToArray();
            return new SurvivorsAtomicReplaceResult(
                true,
                attemptCount,
                captured.Length == 0
                    ? SurvivorsAtomicReplaceFinalDisposition.Succeeded
                    : SurvivorsAtomicReplaceFinalDisposition.SucceededAfterRetry,
                string.Empty,
                captured);
        }

        public static SurvivorsAtomicReplaceResult Failure(
            int attemptCount,
            SurvivorsAtomicReplaceFinalDisposition finalDisposition,
            string finalReason,
            IEnumerable<SurvivorsAtomicReplaceFailure> failures = null)
        {
            return new SurvivorsAtomicReplaceResult(
                false,
                attemptCount,
                finalDisposition,
                finalReason,
                failures);
        }

        private string BuildMessage()
        {
            string headline = Succeeded
                ? "Atomic replacement succeeded after " + AttemptCount.ToString(CultureInfo.InvariantCulture) +
                  " of " + SurvivorsAtomicFile.MaximumAttempts.ToString(CultureInfo.InvariantCulture) + " attempts."
                : "Atomic replacement stopped with disposition " + FinalDisposition + ": " + FinalReason;
            if (Failures.Count == 0) return headline;
            return headline + " Replacement evidence: " + string.Join(
                " | ",
                Failures.Select(failure => failure.ToDiagnosticText()));
        }
    }

    internal sealed class SurvivorsAtomicRetryPreconditionResult
    {
        private SurvivorsAtomicRetryPreconditionResult(bool succeeded, string message)
        {
            Succeeded = succeeded;
            Message = message ?? string.Empty;
        }

        public bool Succeeded { get; }
        public string Message { get; }

        public static SurvivorsAtomicRetryPreconditionResult Current()
        {
            return new SurvivorsAtomicRetryPreconditionResult(true, string.Empty);
        }

        public static SurvivorsAtomicRetryPreconditionResult Failure(string message)
        {
            return new SurvivorsAtomicRetryPreconditionResult(false, message);
        }
    }

    internal sealed class SurvivorsAtomicFileOperations
    {
        public Action<string, string> ReplacementOperation;
        public Action<int> DelayMilliseconds;
        public Func<DateTime> UtcNow;
        public string ProbeDirectory;
        public string TemporaryDirectory;

        public void Replace(string replacement, string destination)
        {
            if (ReplacementOperation != null)
            {
                ReplacementOperation(replacement, destination);
                return;
            }
            File.Replace(replacement, destination, null);
        }

        public void Delay(int milliseconds)
        {
            if (DelayMilliseconds != null)
            {
                DelayMilliseconds(milliseconds);
                return;
            }
            Thread.Sleep(milliseconds);
        }

        public DateTime GetUtcNow()
        {
            return UtcNow != null ? UtcNow() : DateTime.UtcNow;
        }
    }

    internal sealed class SurvivorsAtomicReplaceRequest
    {
        public SurvivorsAtomicReplaceRequest(
            string destinationPath,
            string diagnosticDestinationPath,
            byte[] exactBytes,
            string expectedDestinationHash,
            string operationName,
            SurvivorsEditTransactionHooks hooks = null,
            Func<SurvivorsAtomicRetryPreconditionResult> retryPrecondition = null,
            Func<bool> isAborted = null,
            SurvivorsAtomicFileOperations operations = null)
        {
            DestinationPath = destinationPath ?? string.Empty;
            DiagnosticDestinationPath = diagnosticDestinationPath ?? string.Empty;
            ExactBytes = (byte[])(exactBytes ?? Array.Empty<byte>()).Clone();
            ExpectedDestinationHash = expectedDestinationHash ?? string.Empty;
            OperationName = string.IsNullOrWhiteSpace(operationName) ? "AtomicReplace" : operationName;
            Hooks = hooks;
            RetryPrecondition = retryPrecondition;
            IsAborted = isAborted;
            Operations = operations ?? hooks?.AtomicFileOperations ?? new SurvivorsAtomicFileOperations();
        }

        public string DestinationPath { get; }
        public string DiagnosticDestinationPath { get; }
        public byte[] ExactBytes { get; }
        public string ExpectedDestinationHash { get; }
        public string OperationName { get; }
        public SurvivorsEditTransactionHooks Hooks { get; }
        public Func<SurvivorsAtomicRetryPreconditionResult> RetryPrecondition { get; }
        public Func<bool> IsAborted { get; }
        public SurvivorsAtomicFileOperations Operations { get; }
    }

    internal static class SurvivorsAtomicFile
    {
        public const int MaximumAttempts = 4;
        public const int ProbeTransientCooldownMilliseconds = 1000;
        private static readonly int[] RetryDelayMilliseconds = { 25, 75, 200 };
        private static bool? support;
        private static string supportReason = string.Empty;
        private static string transientSupportReason = string.Empty;
        private static DateTime transientSupportRetryUtc = DateTime.MinValue;

        internal static string DefaultTemporaryDirectory => Path.GetFullPath(Path.Combine(
            Application.dataPath,
            "..",
            "Library",
            "Deucarian",
            "GameContentAuthoring",
            "AtomicTransactions"));

        public static bool TryConfirmSupport(out string reason)
        {
            return TryConfirmSupport(null, out reason);
        }

        internal static bool TryConfirmSupport(
            SurvivorsAtomicFileOperations operations,
            out string reason)
        {
            if (support.HasValue)
            {
                reason = supportReason;
                return support.Value;
            }

            var activeOperations = operations ?? new SurvivorsAtomicFileOperations();
            DateTime now = activeOperations.GetUtcNow();
            if (!string.IsNullOrWhiteSpace(transientSupportReason) && now < transientSupportRetryUtc)
            {
                reason = transientSupportReason + " A fresh support probe is available after the one-second transient cooldown.";
                return false;
            }
            transientSupportReason = string.Empty;
            transientSupportRetryUtc = DateTime.MinValue;

            string directory = string.IsNullOrWhiteSpace(activeOperations.ProbeDirectory)
                ? Path.GetFullPath(Path.Combine(
                    Application.dataPath,
                    "..",
                    "Library",
                    "Deucarian",
                    "GameContentAuthoring",
                    "AtomicProbe"))
                : Path.GetFullPath(activeOperations.ProbeDirectory);
            string id = Guid.NewGuid().ToString("N");
            string destination = Path.Combine(directory, id + ".destination");
            try
            {
                Directory.CreateDirectory(directory);
                byte[] originalBytes = { 1, 2, 3 };
                byte[] replacementBytes = { 4, 5, 6 };
                WriteNew(destination, originalBytes);
                var request = new SurvivorsAtomicReplaceRequest(
                    destination,
                    ToDiagnosticPath(destination),
                    replacementBytes,
                    SurvivorsContentEditHash.Sha256(originalBytes),
                    "SupportProbe",
                    operations: activeOperations);
                if (!TryReplace(request, out SurvivorsAtomicReplaceResult replacementResult))
                {
                    string failure = "Safe atomic file replacement is unavailable on the active editor platform/filesystem: " +
                                     replacementResult.Message;
                    if (replacementResult.IsTransientAvailabilityFailure)
                    {
                        support = null;
                        transientSupportReason = failure;
                        transientSupportRetryUtc = now.AddMilliseconds(ProbeTransientCooldownMilliseconds);
                        reason = transientSupportReason;
                        return false;
                    }
                    support = false;
                    supportReason = failure;
                    reason = supportReason;
                    return false;
                }

                support = File.ReadAllBytes(destination).SequenceEqual(replacementBytes);
                supportReason = support.Value
                    ? string.Empty
                    : "The active filesystem did not preserve the expected atomic replacement result.";
            }
            catch (Exception exception)
            {
                support = false;
                supportReason = "Safe atomic file replacement is unavailable on the active editor platform/filesystem: " +
                                FormatExceptionEvidence(exception);
            }
            finally
            {
                DeleteIfPresent(destination);
            }

            reason = supportReason;
            return support.Value;
        }

        public static bool TryReplace(
            SurvivorsAtomicReplaceRequest request,
            out SurvivorsAtomicReplaceResult result)
        {
            if (request == null)
            {
                result = SurvivorsAtomicReplaceResult.Failure(
                    0,
                    SurvivorsAtomicReplaceFinalDisposition.NonRetryableFailure,
                    "An atomic replacement request is required.");
                return false;
            }

            string destination = request.DestinationPath;
            string diagnosticDestination = string.IsNullOrWhiteSpace(request.DiagnosticDestinationPath)
                ? ToDiagnosticPath(destination)
                : request.DiagnosticDestinationPath;
            string directory = Path.GetDirectoryName(destination);
            if (string.IsNullOrWhiteSpace(directory) || !File.Exists(destination))
            {
                result = SurvivorsAtomicReplaceResult.Failure(
                    0,
                    SurvivorsAtomicReplaceFinalDisposition.NonRetryableFailure,
                    "Atomic replacement requires an existing destination file at '" + diagnosticDestination + "'.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(request.ExpectedDestinationHash))
            {
                result = SurvivorsAtomicReplaceResult.Failure(
                    0,
                    SurvivorsAtomicReplaceFinalDisposition.NonRetryableFailure,
                    "Atomic replacement requires an expected destination hash.");
                return false;
            }
            if (!TryReadHash(destination, out string initialHash, out string initialReadError) ||
                !string.Equals(initialHash, request.ExpectedDestinationHash, StringComparison.Ordinal))
            {
                string detail = string.IsNullOrWhiteSpace(initialReadError)
                    ? "The destination hash no longer matches the expected transaction bytes."
                    : "The destination hash could not be verified: " + initialReadError;
                result = SurvivorsAtomicReplaceResult.Failure(
                    0,
                    SurvivorsAtomicReplaceFinalDisposition.RetryPreconditionFailed,
                    detail);
                return false;
            }

            string temporaryDirectory = string.IsNullOrWhiteSpace(request.Operations.TemporaryDirectory)
                ? DefaultTemporaryDirectory
                : Path.GetFullPath(request.Operations.TemporaryDirectory);
            string temporary = BuildTemporaryPath(temporaryDirectory, Guid.NewGuid());
            string proposedHash = SurvivorsContentEditHash.Sha256(request.ExactBytes);
            var failures = new List<SurvivorsAtomicReplaceFailure>();
            int attempt = 1;
            try
            {
                try
                {
                    Directory.CreateDirectory(temporaryDirectory);
                    WriteNew(temporary, request.ExactBytes);
                }
                catch (Exception exception)
                {
                    failures.Add(CaptureFailure(
                        request.OperationName + ".PrepareTemporary",
                        diagnosticDestination,
                        attempt,
                        exception,
                        false,
                        SurvivorsAtomicReplaceFailureDisposition.NonRetryable));
                    result = SurvivorsAtomicReplaceResult.Failure(
                        0,
                        SurvivorsAtomicReplaceFinalDisposition.NonRetryableFailure,
                        "The durable replacement file could not be prepared.",
                        failures);
                    return false;
                }

                while (attempt <= MaximumAttempts)
                {
                    try
                    {
                        request.Hooks?.BeforeAtomicReplace?.Invoke();
                    }
                    catch (Exception exception)
                    {
                        failures.Add(CaptureFailure(
                            request.OperationName + ".BeforeReplaceHook",
                            diagnosticDestination,
                            attempt,
                            exception,
                            false,
                            SurvivorsAtomicReplaceFailureDisposition.NonRetryable));
                        result = SurvivorsAtomicReplaceResult.Failure(
                            attempt - 1,
                            SurvivorsAtomicReplaceFinalDisposition.NonRetryableFailure,
                            "The pre-replacement operation failed before File.Replace was called.",
                            failures);
                        return false;
                    }

                    try
                    {
                        request.Operations.Replace(temporary, destination);
                    }
                    catch (Exception exception)
                    {
                        bool retryable = IsRetryableReplacementException(exception, out int? win32ErrorCode);
                        bool exhausted = retryable && attempt >= MaximumAttempts;
                        SurvivorsAtomicReplaceFailureDisposition failureDisposition = retryable
                            ? exhausted
                                ? SurvivorsAtomicReplaceFailureDisposition.RetryExhausted
                                : SurvivorsAtomicReplaceFailureDisposition.RetryScheduled
                            : SurvivorsAtomicReplaceFailureDisposition.NonRetryable;
                        failures.Add(new SurvivorsAtomicReplaceFailure(
                            request.OperationName + ".Replace",
                            diagnosticDestination,
                            attempt,
                            MaximumAttempts,
                            exception,
                            win32ErrorCode,
                            retryable,
                            failureDisposition));

                        if (!retryable)
                        {
                            result = SurvivorsAtomicReplaceResult.Failure(
                                attempt,
                                SurvivorsAtomicReplaceFinalDisposition.NonRetryableFailure,
                                "File.Replace reported a nonretryable exception or error code.",
                                failures);
                            return false;
                        }
                        if (exhausted)
                        {
                            result = SurvivorsAtomicReplaceResult.Failure(
                                attempt,
                                SurvivorsAtomicReplaceFinalDisposition.RetryExhausted,
                                "The bounded retry budget was exhausted after known transient replacement failures.",
                                failures);
                            return false;
                        }

                        if (IsAborted(request, out string abortReason))
                        {
                            result = SurvivorsAtomicReplaceResult.Failure(
                                attempt,
                                SurvivorsAtomicReplaceFinalDisposition.Aborted,
                                abortReason,
                                failures);
                            return false;
                        }

                        int delay = RetryDelayMilliseconds[attempt - 1];
                        try
                        {
                            request.Operations.Delay(delay);
                        }
                        catch (Exception delayException)
                        {
                            failures.Add(CaptureFailure(
                                request.OperationName + ".RetryDelay",
                                diagnosticDestination,
                                attempt,
                                delayException,
                                false,
                                SurvivorsAtomicReplaceFailureDisposition.NonRetryable));
                            result = SurvivorsAtomicReplaceResult.Failure(
                                attempt,
                                SurvivorsAtomicReplaceFinalDisposition.NonRetryableFailure,
                                "The bounded retry delay failed.",
                                failures);
                            return false;
                        }

                        if (!TryValidateRetryPreconditions(
                                request,
                                temporary,
                                proposedHash,
                                out string preconditionError))
                        {
                            result = SurvivorsAtomicReplaceResult.Failure(
                                attempt,
                                IsAborted(request, out _)
                                    ? SurvivorsAtomicReplaceFinalDisposition.Aborted
                                    : SurvivorsAtomicReplaceFinalDisposition.RetryPreconditionFailed,
                                preconditionError,
                                failures);
                            return false;
                        }

                        attempt++;
                        continue;
                    }

                    try
                    {
                        request.Hooks?.AfterAtomicReplace?.Invoke();
                    }
                    catch (Exception exception)
                    {
                        failures.Add(CaptureFailure(
                            request.OperationName + ".AfterReplaceHook",
                            diagnosticDestination,
                            attempt,
                            exception,
                            false,
                            SurvivorsAtomicReplaceFailureDisposition.NonRetryable));
                        result = SurvivorsAtomicReplaceResult.Failure(
                            attempt,
                            SurvivorsAtomicReplaceFinalDisposition.NonRetryableFailure,
                            "The post-replacement operation failed after File.Replace completed.",
                            failures);
                        return false;
                    }

                    result = SurvivorsAtomicReplaceResult.Success(attempt, failures);
                    return true;
                }

                result = SurvivorsAtomicReplaceResult.Failure(
                    MaximumAttempts,
                    SurvivorsAtomicReplaceFinalDisposition.RetryExhausted,
                    "The bounded retry loop ended without a replacement result.",
                    failures);
                return false;
            }
            finally
            {
                DeleteIfPresent(temporary);
            }
        }

        public static void WriteNew(string path, byte[] bytes)
        {
            using (var stream = new FileStream(
                       path,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       4096,
                       FileOptions.WriteThrough))
            {
                byte[] safe = bytes ?? Array.Empty<byte>();
                stream.Write(safe, 0, safe.Length);
                stream.Flush(true);
            }
        }

        internal static string BuildTemporaryPath(string temporaryDirectory, Guid transactionId)
        {
            return Path.Combine(temporaryDirectory, "." + transactionId.ToString("N") + ".deucarian-tmp");
        }

        internal static void ResetProbeForTests()
        {
            support = null;
            supportReason = string.Empty;
            transientSupportReason = string.Empty;
            transientSupportRetryUtc = DateTime.MinValue;
        }

        private static bool TryValidateRetryPreconditions(
            SurvivorsAtomicReplaceRequest request,
            string temporary,
            string proposedHash,
            out string error)
        {
            if (IsAborted(request, out error)) return false;
            if (!File.Exists(request.DestinationPath))
            {
                error = "Retry refused because the destination file no longer exists.";
                return false;
            }
            if (!File.Exists(temporary))
            {
                error = "Retry refused because the immutable prepared replacement file no longer exists.";
                return false;
            }
            if (!TryReadHash(request.DestinationPath, out string destinationHash, out string destinationError))
            {
                error = "Retry refused because the destination hash could not be verified: " + destinationError;
                return false;
            }
            if (!string.Equals(destinationHash, request.ExpectedDestinationHash, StringComparison.Ordinal))
            {
                error = "Retry refused because the destination hash changed after the previous replacement attempt.";
                return false;
            }
            if (!TryReadHash(temporary, out string temporaryHash, out string temporaryError))
            {
                error = "Retry refused because the immutable prepared replacement hash could not be verified: " + temporaryError;
                return false;
            }
            if (!string.Equals(temporaryHash, proposedHash, StringComparison.Ordinal))
            {
                error = "Retry refused because the immutable prepared replacement bytes changed.";
                return false;
            }

            if (request.RetryPrecondition != null)
            {
                SurvivorsAtomicRetryPreconditionResult precondition;
                try
                {
                    precondition = request.RetryPrecondition();
                }
                catch (Exception exception)
                {
                    error = "Retry refused because the session precondition threw: " + FormatExceptionEvidence(exception);
                    return false;
                }
                if (precondition == null || !precondition.Succeeded)
                {
                    error = "Retry refused because the session source/revision precondition failed: " +
                            (precondition?.Message ?? "No precondition result was returned.");
                    return false;
                }
            }

            return !IsAborted(request, out error);
        }

        private static bool IsAborted(SurvivorsAtomicReplaceRequest request, out string reason)
        {
            reason = string.Empty;
            if (request?.IsAborted == null) return false;
            try
            {
                if (!request.IsAborted()) return false;
                reason = "Retry aborted because the edit session is disposed or no longer accepting the transaction.";
                return true;
            }
            catch (Exception exception)
            {
                reason = "Retry aborted because the transaction abort check failed: " + FormatExceptionEvidence(exception);
                return true;
            }
        }

        private static SurvivorsAtomicReplaceFailure CaptureFailure(
            string stage,
            string destination,
            int attempt,
            Exception exception,
            bool retryable,
            SurvivorsAtomicReplaceFailureDisposition disposition)
        {
            return new SurvivorsAtomicReplaceFailure(
                stage,
                destination,
                attempt,
                MaximumAttempts,
                exception,
                TryGetWin32ErrorCode(exception),
                retryable,
                disposition);
        }

        private static bool IsRetryableReplacementException(Exception exception, out int? win32ErrorCode)
        {
            win32ErrorCode = TryGetWin32ErrorCode(exception);
            if (!(exception is IOException) || exception is UnauthorizedAccessException || !win32ErrorCode.HasValue)
                return false;
            return win32ErrorCode.Value == 32 ||
                   win32ErrorCode.Value == 33 ||
                   win32ErrorCode.Value == 1175;
        }

        internal static int? TryGetWin32ErrorCode(Exception exception)
        {
            if (exception == null) return null;
            int hresult = exception.HResult;
            int facility = (hresult >> 16) & 0x1fff;
            if (facility == 7) return hresult & 0xffff;
            if (hresult > 0 && hresult <= 0xffff) return hresult;
            return null;
        }

        private static bool TryReadHash(string path, out string hash, out string error)
        {
            hash = string.Empty;
            error = string.Empty;
            try
            {
                hash = SurvivorsContentEditHash.Sha256(File.ReadAllBytes(path));
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                error = FormatExceptionEvidence(exception);
                return false;
            }
        }

        private static string FormatExceptionEvidence(Exception exception)
        {
            if (exception == null) return "No exception evidence was captured.";
            int? win32 = TryGetWin32ErrorCode(exception);
            return exception.GetType().FullName +
                   " (HResult=0x" + unchecked((uint)exception.HResult).ToString("X8", CultureInfo.InvariantCulture) +
                   ", Win32=" + (win32.HasValue
                       ? win32.Value.ToString(CultureInfo.InvariantCulture)
                       : "unavailable") +
                   "): " + exception.GetBaseException().Message;
        }

        private static string ToDiagnosticPath(string path)
        {
            try
            {
                string fullPath = Path.GetFullPath(path ?? string.Empty);
                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                StringComparison comparison = Path.DirectorySeparatorChar == '\\'
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;
                string rootWithSeparator = projectRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                                           Path.DirectorySeparatorChar;
                if (fullPath.StartsWith(rootWithSeparator, comparison))
                    return fullPath.Substring(rootWithSeparator.Length).Replace('\\', '/');
                return Path.GetFileName(fullPath);
            }
            catch
            {
                return Path.GetFileName(path ?? string.Empty);
            }
        }

        private static void DeleteIfPresent(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path)) File.Delete(path);
            }
            catch
            {
            }
        }
    }

    [Serializable]
    internal sealed class SurvivorsRecoveryMetadata
    {
        public string backendId;
        public string sourceAssetPath;
        public string sourceGuid;
        public string sourceLockKey;
        public string originalHash;
        public string proposedHash;
        public string timestampUtc;
        public string phase;
        public string packKey;
        public string recordKey;
        public string backupFile;
        public string actionableMessage;
    }

    internal sealed class SurvivorsRecoveryHandle
    {
        public SurvivorsRecoveryHandle(
            SurvivorsRecoveryMetadata metadata,
            string metadataPath,
            string backupPath,
            string sourceDirectory)
        {
            Metadata = metadata;
            MetadataPath = metadataPath;
            BackupPath = backupPath;
            SourceDirectory = sourceDirectory;
        }

        public SurvivorsRecoveryMetadata Metadata { get; }
        public string MetadataPath { get; }
        public string BackupPath { get; }
        public string SourceDirectory { get; }
    }

    internal static class SurvivorsRecoveryStore
    {
        public const int SuccessfulBackupsPerSource = 5;
        internal const int PortablePathLimit = 240;
        internal const int SourceDirectoryHashLength = 32;
        internal const int TransactionNonceLength = 16;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);

        public static string RootPath => Path.GetFullPath(Path.Combine(
            Application.dataPath,
            "..",
            "Library",
            "Deucarian",
            "GameContentAuthoring",
            "Recovery",
            "Survivors"));

        public static bool TryPrepare(
            SurvivorsEditableSource source,
            string proposedHash,
            out SurvivorsRecoveryHandle handle,
            out string error)
        {
            handle = null;
            error = string.Empty;
            if (source == null)
            {
                error = "A resolved editable source is required for recovery preparation.";
                return false;
            }

            DateTime timestampUtc = DateTime.UtcNow;
            BuildPaths(
                RootPath,
                source.SourcePath.FullPath,
                timestampUtc,
                Guid.NewGuid(),
                out string sourceDirectory,
                out string backupPath,
                out string metadataPath);
            var metadata = new SurvivorsRecoveryMetadata
            {
                backendId = SurvivorsContentPackProvider.StableProviderId,
                sourceAssetPath = source.SourcePath.AssetPath,
                sourceGuid = source.SourcePath.AssetGuid,
                sourceLockKey = source.SourceTarget.LockKey,
                originalHash = source.ExactHash,
                proposedHash = proposedHash ?? string.Empty,
                timestampUtc = timestampUtc.ToString("O", CultureInfo.InvariantCulture),
                phase = "Prepared",
                packKey = source.Manifest.StableKey,
                recordKey = source.Record.CanonicalKey.StableKey,
                backupFile = Path.GetFileName(backupPath),
                actionableMessage = "Exact original bytes retained before atomic replacement."
            };

            try
            {
                Directory.CreateDirectory(sourceDirectory);
                SurvivorsAtomicFile.WriteNew(backupPath, source.ExactBytes);
                SurvivorsAtomicFile.WriteNew(metadataPath, Utf8.GetBytes(JsonUtility.ToJson(metadata, true)));
                handle = new SurvivorsRecoveryHandle(metadata, metadataPath, backupPath, sourceDirectory);
                return true;
            }
            catch (Exception exception)
            {
                DeleteIfPresent(metadataPath);
                DeleteIfPresent(backupPath);
                error = "Recovery bytes and metadata could not be persisted before commit: " +
                        exception.GetBaseException().Message;
                return false;
            }
        }

        internal static string GetSourceDirectory(string sourceFullPath)
        {
            return GetSourceDirectory(RootPath, sourceFullPath);
        }

        internal static void BuildPaths(
            string rootPath,
            string sourceFullPath,
            DateTime timestampUtc,
            Guid transactionNonce,
            out string sourceDirectory,
            out string backupPath,
            out string metadataPath)
        {
            sourceDirectory = GetSourceDirectory(rootPath, sourceFullPath);
            DateTime utc = timestampUtc.Kind == DateTimeKind.Utc
                ? timestampUtc
                : timestampUtc.ToUniversalTime();
            string nonce = transactionNonce.ToString("N").Substring(0, TransactionNonceLength);
            string transactionId = utc.ToString("yyyyMMddTHHmmssfffZ", CultureInfo.InvariantCulture) + "-" + nonce;
            backupPath = Path.Combine(sourceDirectory, transactionId + ".backup");
            metadataPath = Path.Combine(sourceDirectory, transactionId + ".json");
        }

        private static string GetSourceDirectory(string rootPath, string sourceFullPath)
        {
            string sourceHash = SurvivorsContentEditHash.Sha256(sourceFullPath ?? string.Empty);
            return Path.Combine(rootPath, sourceHash.Substring(0, SourceDirectoryHashLength));
        }

        public static void Update(
            SurvivorsRecoveryHandle handle,
            string phase,
            string actionableMessage)
        {
            if (handle?.Metadata == null) return;
            handle.Metadata.phase = phase ?? string.Empty;
            handle.Metadata.actionableMessage = actionableMessage ?? string.Empty;
            byte[] bytes = Utf8.GetBytes(JsonUtility.ToJson(handle.Metadata, true));
            if (!File.Exists(handle.MetadataPath)) return;
            byte[] currentBytes;
            try
            {
                currentBytes = File.ReadAllBytes(handle.MetadataPath);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                return;
            }
            var request = new SurvivorsAtomicReplaceRequest(
                handle.MetadataPath,
                Path.GetFileName(handle.MetadataPath),
                bytes,
                SurvivorsContentEditHash.Sha256(currentBytes),
                "RecoveryMetadata");
            SurvivorsAtomicFile.TryReplace(request, out _);
        }

        public static void DeletePrepared(SurvivorsRecoveryHandle handle)
        {
            if (handle == null) return;
            DeleteIfPresent(handle.MetadataPath);
            DeleteIfPresent(handle.BackupPath);
        }

        public static void PruneResolved(SurvivorsRecoveryHandle handle)
        {
            if (handle == null || !Directory.Exists(handle.SourceDirectory)) return;
            try
            {
                SurvivorsRecoveryMetadata[] resolved = Directory.GetFiles(handle.SourceDirectory, "*.json", SearchOption.TopDirectoryOnly)
                    .Select(path => new { path, metadata = Read(path) })
                    .Where(value => value.metadata != null && IsResolved(value.metadata.phase))
                    .OrderByDescending(value => value.metadata.timestampUtc, StringComparer.Ordinal)
                    .Select(value => value.metadata)
                    .ToArray();
                for (int index = SuccessfulBackupsPerSource; index < resolved.Length; index++)
                {
                    SurvivorsRecoveryMetadata metadata = resolved[index];
                    string metadataPath = Directory.GetFiles(handle.SourceDirectory, "*.json", SearchOption.TopDirectoryOnly)
                        .FirstOrDefault(path =>
                        {
                            SurvivorsRecoveryMetadata candidate = Read(path);
                            return candidate != null &&
                                   string.Equals(candidate.timestampUtc, metadata.timestampUtc, StringComparison.Ordinal) &&
                                   string.Equals(candidate.backupFile, metadata.backupFile, StringComparison.Ordinal);
                        });
                    DeleteIfPresent(metadataPath);
                    DeleteIfPresent(Path.Combine(handle.SourceDirectory, metadata.backupFile ?? string.Empty));
                }
            }
            catch
            {
            }
        }

        public static GameContentRecoveryRecord ToRecoveryRecord(
            SurvivorsRecoveryHandle handle,
            GameContentSourceRevision oldRevision,
            GameContentSourceRevision newRevision,
            string phase,
            string message)
        {
            return new GameContentRecoveryRecord(
                SurvivorsContentPackProvider.StableProviderId,
                handle?.Metadata?.sourceLockKey ?? string.Empty,
                handle?.Metadata?.sourceAssetPath ?? string.Empty,
                oldRevision,
                newRevision,
                DateTime.UtcNow,
                phase,
                message + " Recovery metadata: " + (handle?.MetadataPath ?? RootPath));
        }

        private static SurvivorsRecoveryMetadata Read(string path)
        {
            try
            {
                return JsonUtility.FromJson<SurvivorsRecoveryMetadata>(File.ReadAllText(path, Utf8));
            }
            catch
            {
                return null;
            }
        }

        private static bool IsResolved(string phase)
        {
            return string.Equals(phase, "Verified", StringComparison.Ordinal) ||
                   string.Equals(phase, "RolledBack", StringComparison.Ordinal) ||
                   string.Equals(phase, "RolledBackAfterCommitFailure", StringComparison.Ordinal);
        }

        private static void DeleteIfPresent(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path)) File.Delete(path);
            }
            catch
            {
            }
        }
    }

    internal static class SurvivorsImportedSampleEditConsent
    {
        private const string SessionKey = "Deucarian.Survivors.ImportedSampleEditConsent.v1";

        public static bool EnsureGranted()
        {
            if (SessionState.GetBool(SessionKey, false)) return true;
            bool accepted = EditorUtility.DisplayDialog(
                "Edit Imported Survivors Content",
                "This JSON is a project-owned imported sample copy. Updating or reimporting the package sample may replace or conflict with local edits. Package Installer owns sample refresh and synchronization.",
                "Edit Imported Copy",
                "Cancel");
            if (accepted) SessionState.SetBool(SessionKey, true);
            return accepted;
        }

        internal static void GrantForTests()
        {
            SessionState.SetBool(SessionKey, true);
        }

        internal static void ClearForTests()
        {
            SessionState.EraseBool(SessionKey);
        }
    }
}
