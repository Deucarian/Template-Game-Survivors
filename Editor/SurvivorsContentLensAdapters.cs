using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Deucarian.Attacks.Editor;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.RunUpgrades.Editor;
using Deucarian.WeaponSystems.Editor;
using UnityEditor;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    [InitializeOnLoad]
    public static class SurvivorsContentLensAdapters
    {
        public const string AttackAdapterId = "com.deucarian.template.game.survivors.attack-projection";
        public const string EnemyAdapterId = "com.deucarian.template.game.survivors.enemy-projection";
        public const string EncounterAdapterId = "com.deucarian.template.game.survivors.encounter-projection";
        public const string WeaponAdapterId = "com.deucarian.template.game.survivors.weapon-projection";
        public const string UpgradeAdapterId = "com.deucarian.template.game.survivors.upgrade-projection";

        static SurvivorsContentLensAdapters()
        {
            EnsureRegistered();
        }

        public static void EnsureRegistered()
        {
            GameContentRecordProjectionRegistry<AttackContentRecordProjection>.Register(new SurvivorsAttackAdapter());
            GameContentRecordProjectionRegistry<EnemyContentRecordProjection>.Register(new SurvivorsEnemyAdapter());
            GameContentRecordProjectionRegistry<EncounterContentRecordProjection>.Register(new SurvivorsEncounterAdapter());
            GameContentRecordProjectionRegistry<WeaponContentRecordProjection>.Register(new SurvivorsWeaponAdapter());
            GameContentRecordProjectionRegistry<UpgradeContentRecordProjection>.Register(new SurvivorsUpgradeAdapter());
        }

        private static bool IsSurvivors(
            GameContentRecordDescriptor record,
            GameContentRecordCapability capability)
        {
            return record != null &&
                   string.Equals(
                       record.CanonicalKey.OwningPackageId,
                       SurvivorsContentPackProvider.OwningPackageId,
                       StringComparison.OrdinalIgnoreCase) &&
                   record.HasCapability(capability);
        }

        private static string Read(GameContentRecordDescriptor record, string label)
        {
            return record?.PlayerFacingMetadata.FirstOrDefault(value =>
                string.Equals(value.Label, label, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
        }

        private static string ReadAll(GameContentRecordDescriptor record, string label)
        {
            return record == null
                ? string.Empty
                : string.Join(", ", record.PlayerFacingMetadata
                    .Where(value => string.Equals(value.Label, label, StringComparison.OrdinalIgnoreCase))
                    .Select(value => value.Value)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private static float ReadFloat(GameContentRecordDescriptor record, string label)
        {
            string value = Read(record, label).Trim();
            if (value.EndsWith("s", StringComparison.OrdinalIgnoreCase)) value = value.Substring(0, value.Length - 1);
            int space = value.IndexOf(' ');
            if (space > 0) value = value.Substring(0, space);
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ? parsed : 0f;
        }

        private static double ReadDouble(GameContentRecordDescriptor record, string label)
        {
            return double.TryParse(Read(record, label), NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed)
                ? parsed
                : 0d;
        }

        private static int ReadInt(GameContentRecordDescriptor record, string label)
        {
            return int.TryParse(Read(record, label), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
                ? parsed
                : 0;
        }

        private static bool ReadBool(GameContentRecordDescriptor record, string label)
        {
            string value = Read(record, label);
            return string.Equals(value, "Yes", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "True", StringComparison.OrdinalIgnoreCase);
        }

        private static string TargetId(
            GameContentRecordDescriptor record,
            Func<GameContentRecordReferenceDescriptor, bool> predicate)
        {
            GameContentRecordReferenceDescriptor reference = record.OutboundReferences.FirstOrDefault(predicate);
            return reference?.TargetRecordKey?.SourceRecordId ?? reference?.TargetRecordId ?? string.Empty;
        }

        private static IReadOnlyList<GameContentRecordKey> InboundKeys(GameContentRecordDescriptor record)
        {
            return record.InboundReferences.Select(value => value.TargetRecordKey)
                .Where(value => value != null)
                .Distinct()
                .ToArray();
        }

        private sealed class SurvivorsAttackAdapter : IGameContentRecordProjectionAdapter<AttackContentRecordProjection>
        {
            public string AdapterId => AttackAdapterId;
            public int SortOrder => 100;

            public bool TryProject(GameContentRecordDescriptor record, out AttackContentRecordProjection projection)
            {
                if (!IsSurvivors(record, GameContentRecordCapabilities.Attack))
                {
                    projection = null;
                    return false;
                }

                IReadOnlyList<GameContentRecordKey> related = InboundKeys(
                    record,
                    SurvivorsContentPackIndex.UpgradesSourceId);
                projection = new AttackContentRecordProjection(
                    record,
                    ReadFloat(record, "Damage"),
                    ReadFloat(record, "Cooldown"),
                    ReadFloat(record, "Range"),
                    "Game-defined automatic targeting",
                    Read(record, "Fire Mode"),
                    TargetId(record, value => value.RelationshipLabel.IndexOf("projectile", StringComparison.OrdinalIgnoreCase) >= 0),
                    Math.Max(1, ReadInt(record, "Fan Count")),
                    ReadFloat(record, "Projectile Radius"),
                    0f,
                    BuildWeaponMechanics(record),
                    "Tint " + Read(record, "Tint"),
                    related,
                    related.FirstOrDefault(value => value.SourceRecordId.IndexOf("evolution", StringComparison.OrdinalIgnoreCase) >= 0));
                return true;
            }
        }

        private sealed class SurvivorsWeaponAdapter : IGameContentRecordProjectionAdapter<WeaponContentRecordProjection>
        {
            public string AdapterId => WeaponAdapterId;
            public int SortOrder => 100;

            public bool TryProject(GameContentRecordDescriptor record, out WeaponContentRecordProjection projection)
            {
                if (!IsSurvivors(record, GameContentRecordCapabilities.Weapon))
                {
                    projection = null;
                    return false;
                }

                IReadOnlyList<GameContentRecordKey> related = InboundKeys(
                    record,
                    SurvivorsContentPackIndex.UpgradesSourceId);
                projection = new WeaponContentRecordProjection(
                    record,
                    false,
                    Read(record, "Fire Mode"),
                    ReadFloat(record, "Damage"),
                    ReadFloat(record, "Cooldown"),
                    ReadFloat(record, "Range"),
                    "Game-defined automatic targeting",
                    TargetId(record, value => value.RelationshipLabel.IndexOf("projectile", StringComparison.OrdinalIgnoreCase) >= 0),
                    ReadFloat(record, "Projectile Radius"),
                    related.Count == 0 ? "No linked rank records" : related.Count.ToString(CultureInfo.InvariantCulture) + " linked upgrade records",
                    JoinRelated(related, "mutation"),
                    JoinRelated(related, "evolution"),
                    "Tint " + Read(record, "Tint"));
                return true;
            }
        }

        private sealed class SurvivorsEnemyAdapter : IGameContentRecordProjectionAdapter<EnemyContentRecordProjection>
        {
            public string AdapterId => EnemyAdapterId;
            public int SortOrder => 100;

            public bool TryProject(GameContentRecordDescriptor record, out EnemyContentRecordProjection projection)
            {
                if (!IsSurvivors(record, GameContentRecordCapabilities.Enemy))
                {
                    projection = null;
                    return false;
                }

                string lifeBar = ReadBool(record, "Boss Life Bar")
                    ? "Prominent boss bar"
                    : ReadBool(record, "Overhead Life Bar") ? "Overhead life bar" : "No dedicated life bar";
                string marker = ReadBool(record, "Offscreen Marker")
                    ? "Offscreen marker: " + Read(record, "Marker Style")
                    : "No offscreen marker";
                string lifecycle = "Recycle " + YesNo(ReadBool(record, "Can Recycle")) +
                                   ", leash " + YesNo(ReadBool(record, "Can Leash")) +
                                   ", reposition " + YesNo(ReadBool(record, "Can Reposition"));
                string extension = "Soft leash " + Read(record, "Soft Leash") +
                                   ", hard recycle " + Read(record, "Hard Recycle") +
                                   ", catch-up x" + Read(record, "Catch-up Speed") +
                                   ", reposition timeout " + Read(record, "Reposition Timeout") +
                                   ". " + Read(record, "Role Behavior");
                projection = new EnemyContentRecordProjection(
                    record,
                    Read(record, "Role"),
                    ReadFloat(record, "Health"),
                    ReadFloat(record, "Move Speed"),
                    ReadFloat(record, "Radius"),
                    ReadFloat(record, "Contact Damage"),
                    ReadFloat(record, "Contact Interval"),
                    ReadInt(record, "XP Drop"),
                    lifecycle,
                    record.HasCapability(GameContentRecordCapabilities.MajorThreat),
                    lifeBar,
                    marker,
                    "Tint " + Read(record, "Tint"),
                    extension);
                return true;
            }
        }

        private sealed class SurvivorsEncounterAdapter : IGameContentRecordProjectionAdapter<EncounterContentRecordProjection>
        {
            public string AdapterId => EncounterAdapterId;
            public int SortOrder => 100;

            public bool TryProject(GameContentRecordDescriptor record, out EncounterContentRecordProjection projection)
            {
                if (!IsSurvivors(record, GameContentRecordCapabilities.Encounter))
                {
                    projection = null;
                    return false;
                }

                GameContentRecordKey[] enemies = record.OutboundReferences
                    .Where(value => value.RelationshipLabel.IndexOf("enemy", StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(value => value.TargetRecordKey)
                    .Where(value => value != null)
                    .ToArray();
                GameContentRecordKey[] rewards = record.OutboundReferences
                    .Where(value => value.RelationshipLabel.IndexOf("reward", StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(value => value.TargetRecordKey)
                    .Where(value => value != null)
                    .ToArray();
                string escalation = "Spawn " + Read(record, "Spawn Interval") +
                                    " to minimum " + Read(record, "Minimum Spawn Interval") +
                                    "; max alive " + Read(record, "Maximum Alive") +
                                    "; escalation every " + Read(record, "Escalation Interval") +
                                    "; horde " + Read(record, "Horde First") + " / " + Read(record, "Horde Interval") + ".";
                projection = new EncounterContentRecordProjection(
                    record,
                    "RunProfile / TimedMilestones",
                    ReadFloat(record, "Target Duration"),
                    ReadFloat(record, "Victory Time"),
                    ReadFloat(record, "First Elite"),
                    ReadFloat(record, "First Dread Elite"),
                    ReadFloat(record, "Miniboss"),
                    ReadFloat(record, "Boss"),
                    ReadBool(record, "Endless"),
                    escalation,
                    enemies,
                    rewards);
                return true;
            }
        }

        private sealed class SurvivorsUpgradeAdapter : IGameContentRecordProjectionAdapter<UpgradeContentRecordProjection>
        {
            public string AdapterId => UpgradeAdapterId;
            public int SortOrder => 100;

            public bool TryProject(GameContentRecordDescriptor record, out UpgradeContentRecordProjection projection)
            {
                if (!IsSurvivors(record, GameContentRecordCapabilities.Upgrade))
                {
                    projection = null;
                    return false;
                }

                string prerequisites = string.Join(", ", record.OutboundReferences
                    .Where(value => value.RelationshipLabel.IndexOf("require", StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(value => value.RelationshipLabel + " " + (value.TargetRecordKey?.SourceRecordId ?? value.TargetRecordId)));
                string references = string.Join(", ", record.OutboundReferences
                    .Select(value => value.RelationshipLabel + " " + (value.TargetRecordKey?.SourceRecordId ?? value.TargetRecordId)));
                double amount = ReadDouble(record, "Amount");
                projection = new UpgradeContentRecordProjection(
                    record,
                    record.Description,
                    Read(record, "Category"),
                    Read(record, "Rarity"),
                    ReadDouble(record, "Weight"),
                    ReadInt(record, "Max Rank"),
                    Read(record, "Effect"),
                    amount,
                    Read(record, "Target"),
                    prerequisites,
                    ReadAll(record, "Class Gate"),
                    references,
                    amount.ToString("0.###", CultureInfo.InvariantCulture) + " per authored rank effect");
                return true;
            }
        }

        private static IReadOnlyList<GameContentRecordKey> InboundKeys(
            GameContentRecordDescriptor record,
            string sourceId)
        {
            return InboundKeys(record).Where(value => string.Equals(
                    value.SourceId,
                    sourceId,
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        private static string BuildWeaponMechanics(GameContentRecordDescriptor record)
        {
            return "Pierce " + Read(record, "Pierce Count") +
                   ", chain " + Read(record, "Chain Count") +
                   ", fork " + Read(record, "Fork Count") +
                   ", return " + Read(record, "Return Count") +
                   ", fan " + Read(record, "Fan Count") +
                   ", spread " + Read(record, "Spread") + ".";
        }

        private static string JoinRelated(IEnumerable<GameContentRecordKey> keys, string token)
        {
            string[] matches = keys.Where(value =>
                    value.SourceRecordId.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(value => value.SourceRecordId)
                .ToArray();
            return matches.Length == 0 ? "None" : string.Join(", ", matches);
        }

        private static string YesNo(bool value)
        {
            return value ? "enabled" : "disabled";
        }
    }
}
