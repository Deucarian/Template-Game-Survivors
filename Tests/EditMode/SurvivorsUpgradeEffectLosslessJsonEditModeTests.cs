using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.TemplateGameSurvivors.Editor;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsUpgradeEffectLosslessJsonEditModeTests
    {
        private sealed class ParsedEffects
        {
            public string SourceText;
            public byte[] SourceBytes;
            public SurvivorsLosslessJsonDocument Document;
            public SurvivorsJsonNode Record;
            public SurvivorsJsonStructuredCollectionToken Token;
        }

        private static readonly object[] UnsafeRows =
        {
            new object[]
            {
                "{\"upgrades\":[{\"id\":\"upgrade.test\"}]}",
                "effects' is missing"
            },
            new object[]
            {
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[],\"effects\":[]}]}",
                "appears more than once"
            },
            new object[]
            {
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":{}}]}",
                "must be an array"
            },
            new object[]
            {
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[1]}]}",
                "must be an object"
            },
            new object[]
            {
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[{\"effect\":\"survivors.damage.flat\",\"effect\":\"survivors.area.radius\",\"target\":\"survivors.player\",\"amount\":1}]}]}",
                "duplicate direct property 'effect'"
            },
            new object[]
            {
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[{\"effect\":1,\"target\":\"survivors.player\",\"amount\":1}]}]}",
                "expected enum scalar type"
            },
            new object[]
            {
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[{\"effect\":\"survivors.damage.flat\",\"target\":2,\"amount\":1}]}]}",
                "expected enum scalar type"
            },
            new object[]
            {
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[{\"effect\":\"survivors.damage.flat\",\"target\":\"survivors.player\",\"amount\":\"one\"}]}]}",
                "expected number scalar type"
            },
            new object[]
            {
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[{\"effect\":\"survivors.damage.flat\",\"target\":\"survivors.player\",\"amount\":1,\"future\":[]}]}]}",
                "unsupported nested property 'future'"
            },
            new object[]
            {
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"nested\":{\"effects\":[]}}]}",
                "effects' is missing"
            },
            new object[]
            {
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[{\"effect\":\"survivors.unknown\",\"target\":\"survivors.player\",\"amount\":1}]}]}",
                "Effect Kind: The enum token"
            },
            new object[]
            {
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[{\"effect\":\"survivors.damage.flat\",\"target\":\"survivors.unknown\",\"amount\":1}]}]}",
                "Target: The enum token"
            },
            new object[]
            {
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[{\"effect\":\"survivors.damage.flat\",\"target\":\"survivors.player\",\"amount\":0}]}]}",
                "finite non-zero amount"
            }
        };

        [Test]
        public void Locator_ReadsOnlyDirectMappedFieldsAndRetainsUnknownScalarMetadata()
        {
            const string source =
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[" +
                "{\"effect\":\"survivors.damage.flat\",\"target\":\"survivors.weapon.arcane-wand\",\"amount\":1.25,\"future\":\"café\"}," +
                "{\"effect\":\"survivors.area.radius\",\"target\":\"survivors.area\",\"amount\":0.5}" +
                "]}],\"effects\":[\"unrelated-root-data\"]}";
            ParsedEffects parsed = Parse(source);

            Assert.That(parsed.Token.FieldId, Is.EqualTo("effects"));
            Assert.That(parsed.Token.Rows.Count, Is.EqualTo(2));
            Assert.That(parsed.Token.Rows.Select(row => row.Row.OriginalIndex), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(parsed.Token.Rows.Select(row => row.Row.RowKey).Distinct().Count(), Is.EqualTo(2));
            Assert.That(parsed.Token.Rows[0].FieldTokens.Keys,
                Is.EquivalentTo(new[] { "effect", "target", "amount" }));
            Assert.That(parsed.Token.Rows[0].Row.FieldValues.Select(value => value.FieldId),
                Is.EqualTo(new[] { "effect", "target", "amount" }));
            Assert.That(parsed.Token.Rows[0].PreservationOnlyProperties, Is.EqualTo(new[] { "future" }));
            Assert.That(parsed.Token.Rows[0].PreservationOnlyPropertyTokens.Single().Value.Kind,
                Is.EqualTo(SurvivorsJsonValueKind.String));
            Assert.That(parsed.Token.Rows[0].PreservationOnlyPropertyTokens.Single().Value.Start,
                Is.GreaterThan(parsed.Token.Rows[0].Node.Start));
            Assert.That(parsed.Token.Rows[0].Row.NativeKeyDisplayMetadata, Is.Empty);
            Assert.That(parsed.Token.Rows[0].Row.IsAdded, Is.False);
            Assert.That(parsed.Token.Node.Start, Is.GreaterThan(parsed.Record.Start));
            Assert.That(parsed.Token.Node.End, Is.LessThan(parsed.Record.End));
        }

        [TestCaseSource(nameof(UnsafeRows))]
        public void Locator_RejectsUnsafeOrUnsupportedDirectStructures(string source, string expectedReason)
        {
            Assert.That(TryParse(source, false, out _, out string error), Is.False);
            Assert.That(error, Does.Contain(expectedReason));
        }

        [Test]
        public void Locator_RejectsDuplicateParentIdsMissingParentsAndMalformedJson()
        {
            const string duplicate =
                "{\"upgrades\":[" +
                "{\"id\":\"upgrade.test\",\"effects\":[]}," +
                "{\"id\":\"upgrade.test\",\"effects\":[]}" +
                "]}";
            Assert.That(TryParse(duplicate, false, out _, out string duplicateError), Is.False);
            Assert.That(duplicateError, Does.Contain("duplicate record id 'upgrade.test'"));

            const string missing = "{\"upgrades\":[{\"id\":\"upgrade.other\",\"effects\":[]}]}";
            Assert.That(TryParse(missing, false, out _, out string missingError), Is.False);
            Assert.That(missingError, Does.Contain("was not found"));

            byte[] malformed = new UTF8Encoding(false).GetBytes("{\"upgrades\":[");
            Assert.That(SurvivorsLosslessJsonDocument.TryParse(
                malformed,
                out _,
                out string malformedError), Is.False);
            Assert.That(malformedError, Is.Not.Empty);
        }

        [Test]
        public void Patcher_MovesRawRowsPatchesOneTokenAndPreservesEveryByteOutsideArray()
        {
            const string firstRaw =
                "{\r\n" +
                "          \"effect\": \"survivors.damage.flat\",\r\n" +
                "          \"target\": \"survivors.weapon.arcane-wand\",\r\n" +
                "          \"amount\": 1.2300,\r\n" +
                "          \"future\": \"café \\\"kept\\\"\"\r\n" +
                "        }";
            const string secondRaw =
                "{\r\n" +
                "          \"effect\": \"survivors.area.radius\",\r\n" +
                "          \"target\": \"survivors.area\",\r\n" +
                "          \"amount\": 0.500000\r\n" +
                "        }";
            const string source =
                "{\n" +
                "  \"header\": \"Δ\",\n" +
                "  \"upgrades\": [\r\n" +
                "    {\r\n" +
                "      \"id\": \"upgrade.test\",\r\n" +
                "      \"effects\": [\r\n" +
                "        " + firstRaw + ",\r\n" +
                "        " + secondRaw + "\r\n" +
                "      ],\r\n" +
                "      \"unrelatedNumber\": 1.0000000000000002\r\n" +
                "    }\r\n" +
                "  ],\n" +
                "  \"tail\": \"unchanged\"\n" +
                "}\n";
            ParsedEffects parsed = Parse(source, true);
            GameContentOrderedStructuredCollectionValue changed = parsed.Token.Value.OrderedStructuredCollectionValue;
            GameContentStructuredRowValue first = changed.Rows[0];
            GameContentStructuredRowValue second = changed.Rows[1];
            changed = Apply(changed, GameContentStructuredCollectionOperation.ReplaceRowField(
                first.RowKey,
                "amount",
                GameContentFieldValue.FromNumber(2.5d)));
            changed = Apply(changed, GameContentStructuredCollectionOperation.MoveRow(first.RowKey, 1));

            Assert.That(Patch(parsed, changed, out string proposed, out byte[] proposedBytes, out string patchError),
                Is.True,
                patchError);
            Assert.That(proposed.Substring(0, parsed.Token.Node.Start),
                Is.EqualTo(source.Substring(0, parsed.Token.Node.Start)));
            Assert.That(proposed.EndsWith(source.Substring(parsed.Token.Node.End), StringComparison.Ordinal), Is.True);
            Assert.That(proposed, Does.Contain(secondRaw));
            Assert.That(proposed, Does.Contain(firstRaw.Replace("1.2300", "2.5")));
            Assert.That(proposed, Does.Contain("\"future\": \"café \\\"kept\\\"\""));
            Assert.That(proposed, Does.Contain("\"unrelatedNumber\": 1.0000000000000002"));
            Assert.That(proposed.IndexOf(secondRaw, StringComparison.Ordinal),
                Is.LessThan(proposed.IndexOf(firstRaw.Replace("1.2300", "2.5"), StringComparison.Ordinal)));
            Assert.That(proposedBytes.Take(3), Is.EqualTo(new byte[] { 0xEF, 0xBB, 0xBF }));
            Assert.That(Encoding.UTF8.GetString(proposedBytes, 3, proposedBytes.Length - 3), Is.EqualTo(proposed));

            ParsedEffects reparsed = ParseBytes(proposedBytes);
            Assert.That(reparsed.Token.Value.OrderedStructuredCollectionValue.Rows.Select(RowEffect),
                Is.EqualTo(new[] { "survivors.area.radius", "survivors.damage.flat" }));
            Assert.That(reparsed.Token.Value.OrderedStructuredCollectionValue.Rows.Select(RowAmount),
                Is.EqualTo(new[] { 0.5d, 2.5d }));
            Assert.That(reparsed.Token.Rows[1].PreservationOnlyProperties, Is.EqualTo(new[] { "future" }));
            Assert.That(second.RowKey, Is.Not.EqualTo(first.RowKey));
        }

        [TestCase("")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        public void AddedRows_UseDeterministicFieldOrderAndSourceLocalCompactOrMultilineStyle(string newline)
        {
            string source;
            if (string.IsNullOrEmpty(newline))
            {
                source =
                    "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[" +
                    "{\"effect\":\"survivors.damage.flat\",\"target\":\"survivors.player\",\"amount\":1}" +
                    "]}]}";
            }
            else
            {
                source = "{" + newline +
                         "  \"upgrades\": [" + newline +
                         "    {" + newline +
                         "      \"id\": \"upgrade.test\"," + newline +
                         "      \"effects\": [" + newline +
                         "        {" + newline +
                         "          \"effect\": \"survivors.damage.flat\"," + newline +
                         "          \"target\": \"survivors.player\"," + newline +
                         "          \"amount\": 1" + newline +
                         "        }" + newline +
                         "      ]" + newline +
                         "    }" + newline +
                         "  ]" + newline +
                         "}";
            }

            ParsedEffects parsed = Parse(source);
            GameContentOrderedStructuredCollectionValue added = Apply(
                parsed.Token.Value.OrderedStructuredCollectionValue,
                GameContentStructuredCollectionOperation.AddRow(new[]
                {
                    new GameContentStructuredRowFieldValue("amount", GameContentFieldValue.FromNumber(0.75d)),
                    new GameContentStructuredRowFieldValue("target", GameContentFieldValue.FromEnum("survivors.area")),
                    new GameContentStructuredRowFieldValue("effect", GameContentFieldValue.FromEnum("survivors.area.radius"))
                }));
            Assert.That(Patch(parsed, added, out string proposed, out _, out string error), Is.True, error);
            ParsedEffects reparsed = Parse(proposed);
            Assert.That(reparsed.Token.Rows.Count, Is.EqualTo(2));
            SurvivorsJsonStructuredRowToken newRow = reparsed.Token.Rows[1];
            string raw = proposed.Substring(newRow.Node.Start, newRow.Node.Length);
            Assert.That(raw.IndexOf("\"effect\"", StringComparison.Ordinal),
                Is.LessThan(raw.IndexOf("\"target\"", StringComparison.Ordinal)));
            Assert.That(raw.IndexOf("\"target\"", StringComparison.Ordinal),
                Is.LessThan(raw.IndexOf("\"amount\"", StringComparison.Ordinal)));
            Assert.That(raw, Does.Not.Contain("\"id\""));
            Assert.That(RowEffect(newRow.Row), Is.EqualTo("survivors.area.radius"));
            Assert.That(RowTarget(newRow.Row), Is.EqualTo("survivors.area"));
            Assert.That(RowAmount(newRow.Row), Is.EqualTo(0.75d));
            if (string.IsNullOrEmpty(newline))
            {
                Assert.That(raw, Does.Not.Contain("\r").And.Not.Contain("\n"));
                Assert.That(proposed, Does.Not.Contain("\r").And.Not.Contain("\n"));
            }
            else
            {
                Assert.That(raw, Does.Contain(newline + "          \"effect\""));
                Assert.That(raw.Replace(newline, string.Empty), Does.Not.Contain("\r").And.Not.Contain("\n"));
            }
        }

        [TestCase("\n")]
        [TestCase("\r\n")]
        public void EmptyAuthoredArray_CanReceiveCompleteExplicitRowWithoutHiddenDefaults(string newline)
        {
            string source = "{" + newline +
                            "  \"upgrades\": [" + newline +
                            "    {" + newline +
                            "      \"id\": \"upgrade.test\"," + newline +
                            "      \"effects\": [" + newline +
                            "      ]" + newline +
                            "    }" + newline +
                            "  ]" + newline +
                            "}";
            ParsedEffects parsed = Parse(source);
            Assert.That(parsed.Token.Value.OrderedStructuredCollectionValue.Count, Is.Zero);
            GameContentOrderedStructuredCollectionValue added = Apply(
                parsed.Token.Value.OrderedStructuredCollectionValue,
                GameContentStructuredCollectionOperation.AddRow(Fields(
                    "survivors.damage.flat",
                    "survivors.weapon.arcane-wand",
                    1.5d)));
            Assert.That(Patch(parsed, added, out string proposed, out _, out string error), Is.True, error);
            ParsedEffects reparsed = Parse(proposed);
            Assert.That(reparsed.Token.Rows.Count, Is.EqualTo(1));
            string raw = proposed.Substring(reparsed.Token.Rows[0].Node.Start, reparsed.Token.Rows[0].Node.Length);
            Assert.That(raw, Does.Contain("\"effect\""));
            Assert.That(raw, Does.Contain("\"target\""));
            Assert.That(raw, Does.Contain("\"amount\""));
            Assert.That(raw, Does.Not.Contain("\"id\""));
            Assert.That(raw, Does.Contain(newline));
        }

        [Test]
        public void Patcher_RejectsCraftedOriginalIdentityAndInvalidStructuredValues()
        {
            const string source =
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[" +
                "{\"effect\":\"survivors.damage.flat\",\"target\":\"survivors.player\",\"amount\":1}" +
                "]}]}";
            ParsedEffects parsed = Parse(source);
            GameContentStructuredRowValue original = parsed.Token.Value.OrderedStructuredCollectionValue.Rows[0];
            var craftedRow = new GameContentStructuredRowValue(
                GameContentStructuredRowKey.CreateSessionKey(),
                original.OriginalIndex,
                original.SchemaId,
                original.FieldValues,
                GameContentEditValidationState.Valid,
                original.DisplaySummary);
            var crafted = new GameContentOrderedStructuredCollectionValue(original.SchemaId, new[] { craftedRow });
            Assert.That(Patch(parsed, crafted, out _, out _, out string craftedError), Is.False);
            Assert.That(craftedError, Does.Contain("unknown or crafted session key"));

            var invalidFields = new[]
            {
                new GameContentStructuredRowFieldValue("effect", GameContentFieldValue.FromEnum("survivors.damage.flat")),
                new GameContentStructuredRowFieldValue("target", GameContentFieldValue.FromEnum("survivors.player")),
                new GameContentStructuredRowFieldValue("amount", GameContentFieldValue.FromNumber(0d))
            };
            var invalidRow = new GameContentStructuredRowValue(
                original.RowKey,
                original.OriginalIndex,
                original.SchemaId,
                invalidFields);
            var invalid = new GameContentOrderedStructuredCollectionValue(original.SchemaId, new[] { invalidRow });
            Assert.That(Patch(parsed, invalid, out _, out _, out string invalidError), Is.False);
            Assert.That(invalidError, Does.Contain("finite non-zero amount"));
        }

        [Test]
        public void RestoreOriginalOrder_AfterMoveProducesExactOriginalBytes()
        {
            const string source =
                "{\"upgrades\":[{\"id\":\"upgrade.test\",\"effects\":[" +
                "{\"effect\":\"survivors.damage.flat\",\"target\":\"survivors.player\",\"amount\":1}," +
                "{ \"effect\" : \"survivors.area.radius\" , \"target\" : \"survivors.area\" , \"amount\" : 0.5 }" +
                "]}]}";
            ParsedEffects parsed = Parse(source, true);
            GameContentOrderedStructuredCollectionValue current = parsed.Token.Value.OrderedStructuredCollectionValue;
            current = Apply(current, GameContentStructuredCollectionOperation.MoveRow(current.Rows[1].RowKey, 0));
            current = Apply(current, GameContentStructuredCollectionOperation.RestoreOriginalOrder());
            Assert.That(current.Equals(parsed.Token.Value.OrderedStructuredCollectionValue), Is.True);
            Assert.That(Patch(parsed, current, out string proposed, out byte[] proposedBytes, out string error), Is.True, error);
            Assert.That(proposed, Is.EqualTo(source));
            Assert.That(proposedBytes, Is.EqualTo(parsed.SourceBytes));
        }

        private static ParsedEffects Parse(string source, bool bom = false)
        {
            Assert.That(TryParse(source, bom, out ParsedEffects parsed, out string error), Is.True, error);
            return parsed;
        }

        private static ParsedEffects ParseBytes(byte[] bytes)
        {
            Assert.That(SurvivorsLosslessJsonDocument.TryParse(
                bytes,
                out SurvivorsLosslessJsonDocument document,
                out string parseError), Is.True, parseError);
            Assert.That(TryLocate(document, out SurvivorsJsonNode record, out string locateError), Is.True, locateError);
            Assert.That(SurvivorsJsonRecordNavigator.TryReadDirectStructuredCollection(
                document,
                record,
                SurvivorsUpgradeEffectEditing.Field,
                true,
                out SurvivorsJsonStructuredCollectionToken token,
                out string tokenError), Is.True, tokenError);
            return new ParsedEffects
            {
                SourceText = document.Text,
                SourceBytes = bytes,
                Document = document,
                Record = record,
                Token = token
            };
        }

        private static bool TryParse(
            string source,
            bool bom,
            out ParsedEffects parsed,
            out string error)
        {
            parsed = null;
            byte[] text = new UTF8Encoding(false).GetBytes(source ?? string.Empty);
            byte[] bytes = bom
                ? new byte[] { 0xEF, 0xBB, 0xBF }.Concat(text).ToArray()
                : text;
            if (!SurvivorsLosslessJsonDocument.TryParse(
                    bytes,
                    out SurvivorsLosslessJsonDocument document,
                    out error))
                return false;
            if (!TryLocate(document, out SurvivorsJsonNode record, out error)) return false;
            if (!SurvivorsJsonRecordNavigator.TryReadDirectStructuredCollection(
                    document,
                    record,
                    SurvivorsUpgradeEffectEditing.Field,
                    true,
                    out SurvivorsJsonStructuredCollectionToken token,
                    out error))
                return false;
            parsed = new ParsedEffects
            {
                SourceText = source,
                SourceBytes = bytes,
                Document = document,
                Record = record,
                Token = token
            };
            return true;
        }

        private static bool TryLocate(
            SurvivorsLosslessJsonDocument document,
            out SurvivorsJsonNode record,
            out string error)
        {
            return SurvivorsJsonRecordNavigator.TryLocateRecord(
                document,
                new SurvivorsJsonRecordLocator(
                    SurvivorsContentPackIndex.UpgradesSourceId,
                    "upgrades",
                    "upgrade.test",
                    "Upgrade"),
                out record,
                out error);
        }

        private static GameContentOrderedStructuredCollectionValue Apply(
            GameContentOrderedStructuredCollectionValue current,
            GameContentStructuredCollectionOperation operation)
        {
            Assert.That(GameContentStructuredCollectionMutation.TryApply(
                SurvivorsUpgradeEffectEditing.Field,
                current,
                operation,
                out GameContentOrderedStructuredCollectionValue proposed,
                out _,
                out string error), Is.True, error);
            Assert.That(SurvivorsUpgradeEffectEditing.TryValidate(proposed, out string domainError), Is.True, domainError);
            return proposed;
        }

        private static bool Patch(
            ParsedEffects parsed,
            GameContentOrderedStructuredCollectionValue proposed,
            out string proposedText,
            out byte[] proposedBytes,
            out string error)
        {
            var tokens = new Dictionary<string, SurvivorsJsonStructuredCollectionToken>(StringComparer.Ordinal)
            {
                [SurvivorsUpgradeEffectEditing.CollectionFieldId] = parsed.Token
            };
            var changes = new Dictionary<string, GameContentFieldValue>(StringComparer.Ordinal)
            {
                [SurvivorsUpgradeEffectEditing.CollectionFieldId] =
                    GameContentFieldValue.FromOrderedStructuredCollection(proposed)
            };
            return SurvivorsLosslessJsonPatcher.TryPatch(
                parsed.Document,
                null,
                null,
                tokens,
                changes,
                out proposedText,
                out proposedBytes,
                out error);
        }

        private static IReadOnlyList<GameContentStructuredRowFieldValue> Fields(
            string effect,
            string target,
            double amount)
        {
            return new[]
            {
                new GameContentStructuredRowFieldValue("effect", GameContentFieldValue.FromEnum(effect)),
                new GameContentStructuredRowFieldValue("target", GameContentFieldValue.FromEnum(target)),
                new GameContentStructuredRowFieldValue("amount", GameContentFieldValue.FromNumber(amount))
            };
        }

        private static string RowEffect(GameContentStructuredRowValue row)
        {
            return row.FieldValues.Single(value => value.FieldId == "effect").Value.StringValue;
        }

        private static string RowTarget(GameContentStructuredRowValue row)
        {
            return row.FieldValues.Single(value => value.FieldId == "target").Value.StringValue;
        }

        private static double RowAmount(GameContentStructuredRowValue row)
        {
            return row.FieldValues.Single(value => value.FieldId == "amount").Value.NumberValue;
        }
    }
}
