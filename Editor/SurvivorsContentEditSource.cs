using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Deucarian.GameContentAuthoring.Editor;
using UnityEditor;
using UnityEngine;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    internal enum SurvivorsEditableRecordKind
    {
        Weapon,
        Projectile,
        Enemy
    }

    internal sealed class SurvivorsContentEditDefinition
    {
        private sealed class FieldSpec
        {
            public FieldSpec(GameContentFieldDescriptor descriptor, bool propertyRequired)
            {
                Descriptor = descriptor;
                PropertyRequired = propertyRequired;
            }

            public GameContentFieldDescriptor Descriptor { get; }
            public bool PropertyRequired { get; }
        }

        private SurvivorsContentEditDefinition(
            SurvivorsJsonRecordLocator locator,
            SurvivorsEditableRecordKind recordKind,
            IReadOnlyList<GameContentFieldDescriptor> fields,
            IReadOnlyDictionary<string, SurvivorsJsonScalarToken> tokens,
            IReadOnlyDictionary<string, GameContentFieldValue> values)
        {
            Locator = locator;
            RecordKind = recordKind;
            Fields = fields;
            Tokens = tokens;
            Values = values;
        }

        public SurvivorsJsonRecordLocator Locator { get; }
        public SurvivorsEditableRecordKind RecordKind { get; }
        public IReadOnlyList<GameContentFieldDescriptor> Fields { get; }
        public IReadOnlyDictionary<string, SurvivorsJsonScalarToken> Tokens { get; }
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
            var values = new Dictionary<string, GameContentFieldValue>(StringComparer.Ordinal);
            foreach (FieldSpec spec in BuildFieldSpecs(kind))
            {
                GameContentFieldDescriptor field = spec.Descriptor;
                if (!SurvivorsJsonRecordNavigator.TryReadDirectScalar(
                        document,
                        recordNode,
                        field.FieldId,
                        field.FieldType,
                        spec.PropertyRequired,
                        out SurvivorsJsonScalarToken token,
                        out error))
                    return false;
                if (token == null) continue;
                fields.Add(field);
                tokens.Add(field.FieldId, token);
                values.Add(field.FieldId, token.Value);
            }

            if (fields.All(field => field.IsReadOnly))
            {
                error = "The selected record has no supported direct scalar fields.";
                return false;
            }

            definition = new SurvivorsContentEditDefinition(locator, kind, fields, tokens, values);
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
            else
            {
                error = "Only existing weapon, projectile, and enemy records are editable in this milestone.";
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
                default:
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
            }
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
        public const string BackendSchemaVersion = "survivors-lossless-json-v1";

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
                error = "Only the imported Basic Survivors and Neon Arcana packs support scalar JSON editing.";
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
                !string.Equals(sourceId, SurvivorsContentPackIndex.EnemiesSourceId, StringComparison.OrdinalIgnoreCase))
            {
                error = "Only the weapons and enemies JSON sources are editable in this milestone.";
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
    }

    internal static class SurvivorsAtomicFile
    {
        private static bool? support;
        private static string supportReason = string.Empty;

        public static bool TryConfirmSupport(out string reason)
        {
            if (support.HasValue)
            {
                reason = supportReason;
                return support.Value;
            }

            string directory = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                "Library",
                "Deucarian",
                "GameContentAuthoring",
                "AtomicProbe"));
            string id = Guid.NewGuid().ToString("N");
            string destination = Path.Combine(directory, id + ".destination");
            string replacement = Path.Combine(directory, id + ".replacement");
            try
            {
                Directory.CreateDirectory(directory);
                WriteNew(destination, new byte[] { 1, 2, 3 });
                WriteNew(replacement, new byte[] { 4, 5, 6 });
                File.Replace(replacement, destination, null);
                support = File.ReadAllBytes(destination).SequenceEqual(new byte[] { 4, 5, 6 });
                supportReason = support.Value
                    ? string.Empty
                    : "The active filesystem did not preserve the expected atomic replacement result.";
            }
            catch (Exception exception)
            {
                support = false;
                supportReason = "Safe atomic file replacement is unavailable on the active editor platform/filesystem: " +
                                exception.GetBaseException().Message;
            }
            finally
            {
                DeleteIfPresent(destination);
                DeleteIfPresent(replacement);
            }

            reason = supportReason;
            return support.Value;
        }

        public static bool TryReplace(
            string destination,
            byte[] exactBytes,
            SurvivorsEditTransactionHooks hooks,
            out string error)
        {
            error = string.Empty;
            string directory = Path.GetDirectoryName(destination);
            if (string.IsNullOrWhiteSpace(directory) || !File.Exists(destination))
            {
                error = "Atomic replacement requires an existing destination file.";
                return false;
            }
            string temporary = Path.Combine(
                directory,
                "." + Path.GetFileName(destination) + "." + Guid.NewGuid().ToString("N") + ".deucarian-tmp");
            try
            {
                WriteNew(temporary, exactBytes);
                hooks?.BeforeAtomicReplace?.Invoke();
                File.Replace(temporary, destination, null);
                hooks?.AfterAtomicReplace?.Invoke();
                return true;
            }
            catch (Exception exception)
            {
                error = "Atomic replacement failed: " + exception.GetBaseException().Message;
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

        internal static void ResetProbeForTests()
        {
            support = null;
            supportReason = string.Empty;
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

            string sourceKey = SurvivorsContentEditHash.Sha256(source.SourcePath.FullPath);
            string sourceDirectory = Path.Combine(RootPath, sourceKey);
            string transactionId = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffffffZ", CultureInfo.InvariantCulture) +
                                   "-" + Guid.NewGuid().ToString("N");
            string backupPath = Path.Combine(sourceDirectory, transactionId + ".backup");
            string metadataPath = Path.Combine(sourceDirectory, transactionId + ".json");
            var metadata = new SurvivorsRecoveryMetadata
            {
                backendId = SurvivorsContentPackProvider.StableProviderId,
                sourceAssetPath = source.SourcePath.AssetPath,
                sourceGuid = source.SourcePath.AssetGuid,
                sourceLockKey = source.SourceTarget.LockKey,
                originalHash = source.ExactHash,
                proposedHash = proposedHash ?? string.Empty,
                timestampUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
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
            SurvivorsAtomicFile.TryReplace(handle.MetadataPath, bytes, null, out _);
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
