using System;
using System.Collections.Generic;
using System.Linq;
using Deucarian.GameContentAuthoring.Editor;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    internal static class SurvivorsUpgradeEffectEditing
    {
        public const string CollectionFieldId = "effects";
        public const string RowSchemaId = "survivors.upgrade.effect-row.v1";
        public const string EffectFieldId = "effect";
        public const string TargetFieldId = "target";
        public const string AmountFieldId = "amount";

        private static readonly string[] EffectIds =
        {
            "survivors.damage.flat",
            "survivors.weapon.cooldown_multiplier",
            "survivors.player.move_speed",
            "survivors.pickup.range",
            "survivors.pickup.pull_speed",
            "survivors.pickup.magnet_pulse",
            "survivors.player.max_health",
            "survivors.orbit.blade_count",
            "survivors.orbit.radius",
            "survivors.melee.target_count",
            "survivors.burst.count",
            "survivors.burst.echo_count",
            "survivors.burst.targeted_sigils",
            "survivors.projectile.fan_count",
            "survivors.projectile.pierce_count",
            "survivors.projectile.chain_count",
            "survivors.projectile.fork_count",
            "survivors.projectile.return_count",
            "survivors.hitscan.pierce",
            "survivors.payload.count",
            "survivors.payload.radius",
            "survivors.payload.trigger_radius",
            "survivors.status.poison_ratio",
            "survivors.status.bleed_ratio",
            "survivors.status.execute_threshold",
            "survivors.critical.chance",
            "survivors.critical.damage_multiplier",
            "survivors.critical.damage",
            "survivors.draft.luck",
            "survivors.on_kill.nova_damage",
            "survivors.on_kill.nova_radius",
            "survivors.sustain.lifesteal_ratio",
            "survivors.barrier.capacity",
            "survivors.barrier.regen",
            "survivors.barrier.on_damage_ratio",
            "survivors.experience.gain_multiplier",
            "survivors.area.radius",
            "survivors.weapon.unlock"
        };

        private static readonly string[] TargetIds =
        {
            "survivors.player",
            "survivors.weapon.arcane-wand",
            "survivors.weapon.frost-fan",
            "survivors.weapon.orbit-ward",
            "survivors.weapon.thorn-halo",
            "survivors.weapon.moon-slash",
            "survivors.weapon.star-nova",
            "survivors.weapon.star-beam",
            "survivors.weapon.gravity-grenade",
            "survivors.weapon.rune-trap",
            "survivors.weapon.aether-mine",
            "survivors.weapon.payloads",
            "survivors.pickups",
            "survivors.status",
            "survivors.critical",
            "survivors.draft",
            "survivors.on_kill",
            "survivors.barrier",
            "survivors.experience",
            "survivors.area"
        };

        private static readonly IReadOnlyList<GameContentEnumOption> EffectOptions = EffectIds
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .Select(value => new GameContentEnumOption(value, Humanize(value)))
            .ToArray();

        private static readonly IReadOnlyList<GameContentEnumOption> TargetOptions = TargetIds
            .OrderBy(value => value, StringComparer.Ordinal)
            .Select(value => new GameContentEnumOption(value, Humanize(value)))
            .ToArray();

        private static readonly GameContentFieldDescriptor SharedField = CreateField();

        public static GameContentFieldDescriptor Field => SharedField;
        public static IReadOnlyList<string> SupportedEffectIds => EffectOptions.Select(value => value.Token).ToArray();
        public static IReadOnlyList<string> SupportedTargetIds => TargetOptions.Select(value => value.Token).ToArray();

        public static bool TryValidate(
            GameContentOrderedStructuredCollectionValue collection,
            out string reason)
        {
            if (collection == null)
            {
                reason = "An ordered Upgrade effect collection is required.";
                return false;
            }
            if (!SharedField.StructuredCollection.Accepts(collection, out reason)) return false;

            for (int index = 0; index < collection.Rows.Count; index++)
            {
                GameContentStructuredRowValue row = collection.Rows[index];
                if (!row.TryGetFieldValue(EffectFieldId, out GameContentFieldValue effect) ||
                    effect.FieldType != GameContentFieldType.Enum ||
                    !EffectOptions.Any(option => string.Equals(option.Token, effect.StringValue, StringComparison.Ordinal)))
                {
                    reason = "Effect row " + (index + 1) + " must use one supported effect kind.";
                    return false;
                }
                if (!row.TryGetFieldValue(TargetFieldId, out GameContentFieldValue target) ||
                    target.FieldType != GameContentFieldType.Enum ||
                    !TargetOptions.Any(option => string.Equals(option.Token, target.StringValue, StringComparison.Ordinal)))
                {
                    reason = "Effect row " + (index + 1) + " must use one supported gameplay target token.";
                    return false;
                }
                if (!row.TryGetFieldValue(AmountFieldId, out GameContentFieldValue amount) ||
                    amount.FieldType != GameContentFieldType.Number ||
                    double.IsNaN(amount.NumberValue) ||
                    double.IsInfinity(amount.NumberValue) ||
                    Math.Abs(amount.NumberValue) <= double.Epsilon)
                {
                    reason = "Effect row " + (index + 1) + " requires a finite non-zero amount.";
                    return false;
                }
            }

            reason = string.Empty;
            return true;
        }

        private static GameContentFieldDescriptor CreateField()
        {
            var row = new GameContentStructuredRowDescriptor(
                RowSchemaId,
                "Effect",
                "An embedded value owned by the selected Upgrade. It has no canonical identity, save identity, or top-level CRUD lifecycle. Preservation-only scalar properties remain untouched; nested or ambiguous row structures are read-only.",
                new[]
                {
                    new GameContentFieldDescriptor(
                        EffectFieldId,
                        "survivors.upgrade.effect.kind",
                        "Effect Kind",
                        "Closed runtime effect token. Arbitrary effect IDs are not accepted.",
                        GameContentFieldType.Enum,
                        order: 10,
                        group: "Effect",
                        required: true,
                        enumOptions: EffectOptions),
                    new GameContentFieldDescriptor(
                        TargetFieldId,
                        "survivors.upgrade.effect.target",
                        "Target",
                        "Closed gameplay target token. This mixed protocol includes weapon and semantic targets, so it is not exposed as a free-form string or canonical record selector.",
                        GameContentFieldType.Enum,
                        order: 20,
                        group: "Effect",
                        required: true,
                        enumOptions: TargetOptions),
                    new GameContentFieldDescriptor(
                        AmountFieldId,
                        "survivors.upgrade.effect.amount",
                        "Amount",
                        "Finite non-zero authored amount applied by the parent Upgrade.",
                        GameContentFieldType.Number,
                        order: 30,
                        group: "Effect",
                        required: true)
                },
                new[] { EffectFieldId, TargetFieldId, AmountFieldId },
                supportsAdd: true,
                supportsRemove: true,
                supportsMove: true,
                supportsRowFieldReplacement: true,
                representsIndependentCanonicalRecord: false);
            var collection = new GameContentStructuredCollectionFieldDescriptor(
                CollectionFieldId,
                "survivors.upgrade.effects",
                "Effects",
                "Ordered embedded effects consumed through the selected Upgrade. Add and Remove never create or delete an Upgrade; stable Upgrade IDs remain immutable. Changes require content refresh/rebind or a run restart, and imported sample refresh may replace project-owned edits.",
                row,
                minimumCount: 0,
                maximumCount: null,
                orderingSemantics: "Effects are bound and applied in authored array order.",
                duplicatePolicy: GameContentStructuredRowDuplicatePolicy.Allow,
                permittedOperations: GameContentStructuredCollectionPermittedOperations.All,
                runtimeImpact: GameContentReferenceRuntimeImpact.Refresh |
                               GameContentReferenceRuntimeImpact.Rebind |
                               GameContentReferenceRuntimeImpact.Restart);
            return GameContentFieldDescriptor.FromStructuredCollection(collection, 20, "Behavior");
        }

        private static string Humanize(string token)
        {
            string value = token ?? string.Empty;
            const string prefix = "survivors.";
            if (value.StartsWith(prefix, StringComparison.Ordinal)) value = value.Substring(prefix.Length);
            return value.Replace('_', ' ').Replace(".", " / ");
        }
    }
}
