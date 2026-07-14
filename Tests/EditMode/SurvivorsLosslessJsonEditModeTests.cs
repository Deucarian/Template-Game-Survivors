using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.TemplateGameSurvivors.Editor;
using NUnit.Framework;

namespace Deucarian.TemplateGameSurvivors.Tests
{
    public sealed class SurvivorsLosslessJsonEditModeTests
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);

        [Test]
        public void Parser_TracksStructureScalarsSeparatorsAndWhitespace()
        {
            const string json = " {\r\n\t\"items\" : [\"line\\nvalue\", -12.5e+2, true, false, null] } ";
            SurvivorsLosslessJsonDocument document = Parse(json);

            Assert.That(document.Root.Kind, Is.EqualTo(SurvivorsJsonValueKind.Object));
            Assert.That(document.Tokens.Select(token => token.Kind), Does.Contain(SurvivorsJsonTokenKind.ObjectStart));
            Assert.That(document.Tokens.Select(token => token.Kind), Does.Contain(SurvivorsJsonTokenKind.ArrayStart));
            Assert.That(document.Tokens.Select(token => token.Kind), Does.Contain(SurvivorsJsonTokenKind.PropertyName));
            Assert.That(document.Tokens.Select(token => token.Kind), Does.Contain(SurvivorsJsonTokenKind.String));
            Assert.That(document.Tokens.Select(token => token.Kind), Does.Contain(SurvivorsJsonTokenKind.Number));
            Assert.That(document.Tokens.Select(token => token.Kind), Does.Contain(SurvivorsJsonTokenKind.True));
            Assert.That(document.Tokens.Select(token => token.Kind), Does.Contain(SurvivorsJsonTokenKind.False));
            Assert.That(document.Tokens.Select(token => token.Kind), Does.Contain(SurvivorsJsonTokenKind.Null));
            Assert.That(document.Tokens.Select(token => token.Kind), Does.Contain(SurvivorsJsonTokenKind.Colon));
            Assert.That(document.Tokens.Select(token => token.Kind), Does.Contain(SurvivorsJsonTokenKind.Comma));
            Assert.That(document.Tokens.Select(token => token.Kind), Does.Contain(SurvivorsJsonTokenKind.Whitespace));
        }

        [Test]
        public void Parser_PreservesUtf8BomDuringPatch()
        {
            const string json = "{\"weapons\":[{\"id\":\"weapon.one\",\"damage\":7}]}";
            SurvivorsLosslessJsonDocument document = Parse(json, true);
            SurvivorsJsonScalarToken token = Locate(document, "weapons", "weapon.one", "damage", GameContentFieldType.Number);

            byte[] proposed = Patch(document, token, GameContentFieldValue.FromNumber(9.25), out string text);

            Assert.That(document.HasUtf8Bom, Is.True);
            Assert.That(proposed.Take(3), Is.EqualTo(new byte[] { 0xEF, 0xBB, 0xBF }));
            Assert.That(text, Is.EqualTo("{\"weapons\":[{\"id\":\"weapon.one\",\"damage\":9.25}]}"));
        }

        [TestCase("\n")]
        [TestCase("\r\n")]
        public void Patcher_PreservesUniformLineEndings(string newline)
        {
            string json = "{" + newline + "  \"weapons\": [" + newline +
                          "    { \"id\": \"weapon.one\", \"displayName\": \"Old\" }" + newline +
                          "  ]" + newline + "}" + newline;
            SurvivorsLosslessJsonDocument document = Parse(json);
            SurvivorsJsonScalarToken token = Locate(document, "weapons", "weapon.one", "displayName", GameContentFieldType.String);

            Patch(document, token, GameContentFieldValue.FromString("New"), out string proposed);

            Assert.That(proposed, Is.EqualTo(json.Replace("\"Old\"", "\"New\"")));
        }

        [Test]
        public void Patcher_PreservesMixedLineEndingsAndUnknownFieldsExactly()
        {
            const string json = "{\r\n  \"unknown\": 1.2300,\n  \"weapons\": [\r\n" +
                                "    {\"id\":\"weapon.one\", \"nested\": {\"damage\":999}, \"damage\":7.000, \"future\":true},\n" +
                                "    {\"id\":\"weapon.two\",\"damage\":8e0}\r\n  ]\n}\r\n";
            SurvivorsLosslessJsonDocument document = Parse(json);
            SurvivorsJsonScalarToken token = Locate(document, "weapons", "weapon.one", "damage", GameContentFieldType.Number);

            Patch(document, token, GameContentFieldValue.FromNumber(9.25), out string proposed);

            Assert.That(proposed, Is.EqualTo(json.Replace("\"damage\":7.000", "\"damage\":9.25")));
            Assert.That(proposed, Does.Contain("1.2300"));
            Assert.That(proposed, Does.Contain("8e0"));
            Assert.That(proposed, Does.Contain("\"future\":true"));
            Assert.That(proposed, Does.Contain("\"nested\": {\"damage\":999}"));
        }

        [TestCase("")]
        [TestCase("{")]
        [TestCase("{\"a\":1,}")]
        [TestCase("[01]")]
        [TestCase("[1.]")]
        [TestCase("[1e]")]
        [TestCase("[\"bad\\x\"]")]
        [TestCase("[true false]")]
        public void Parser_RejectsMalformedJson(string json)
        {
            Assert.That(
                SurvivorsLosslessJsonDocument.TryParse(Utf8.GetBytes(json), out _, out string error),
                Is.False);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void Parser_RejectsMalformedUtf8()
        {
            byte[] bytes = { (byte)'{', (byte)'"', 0xC3, 0x28, (byte)'"', (byte)':', (byte)'1', (byte)'}' };
            Assert.That(SurvivorsLosslessJsonDocument.TryParse(bytes, out _, out string error), Is.False);
            Assert.That(error, Does.Contain("UTF-8"));
        }

        [Test]
        public void Locator_DecodesEscapedIdsAndPropertyNames()
        {
            SurvivorsLosslessJsonDocument document = Parse(
                "{\"weapons\":[{\"\\u0069d\":\"weapon.\\u006fne\",\"damage\":7}]}" );
            var locator = new SurvivorsJsonRecordLocator("weapons", "weapons", "weapon.one", "Weapon");

            Assert.That(SurvivorsJsonRecordNavigator.TryLocateRecord(document, locator, out SurvivorsJsonNode record, out string error), Is.True, error);
            Assert.That(SurvivorsJsonRecordNavigator.TryReadDirectScalar(
                document,
                record,
                "damage",
                GameContentFieldType.Number,
                true,
                out SurvivorsJsonScalarToken token,
                out error), Is.True, error);
            Assert.That(token.Value.NumberValue, Is.EqualTo(7d));
        }

        [Test]
        public void Locator_RejectsDuplicateDirectPropertyAmbiguity()
        {
            SurvivorsLosslessJsonDocument document = Parse(
                "{\"weapons\":[{\"id\":\"weapon.one\",\"damage\":7,\"\\u0064amage\":9}]}" );
            var locator = new SurvivorsJsonRecordLocator("weapons", "weapons", "weapon.one", "Weapon");
            Assert.That(SurvivorsJsonRecordNavigator.TryLocateRecord(document, locator, out SurvivorsJsonNode record, out string error), Is.True, error);

            Assert.That(SurvivorsJsonRecordNavigator.TryReadDirectScalar(
                document,
                record,
                "damage",
                GameContentFieldType.Number,
                true,
                out _,
                out error), Is.False);
            Assert.That(error, Does.Contain("more than once"));
        }

        [Test]
        public void Locator_RejectsDuplicateRecordIds()
        {
            SurvivorsLosslessJsonDocument document = Parse(
                "{\"weapons\":[{\"id\":\"weapon.one\",\"damage\":7},{\"id\":\"weapon.one\",\"damage\":8}]}" );
            var locator = new SurvivorsJsonRecordLocator("weapons", "weapons", "weapon.one", "Weapon");

            Assert.That(SurvivorsJsonRecordNavigator.TryLocateRecord(document, locator, out _, out string error), Is.False);
            Assert.That(error, Does.Contain("duplicate record id"));
        }

        [Test]
        public void Locator_RejectsMissingCollectionRecordAndDirectProperty()
        {
            SurvivorsLosslessJsonDocument missingCollection = Parse("{\"other\":[]}");
            var locator = new SurvivorsJsonRecordLocator("weapons", "weapons", "weapon.one", "Weapon");
            Assert.That(SurvivorsJsonRecordNavigator.TryLocateRecord(missingCollection, locator, out _, out string collectionError), Is.False);
            Assert.That(collectionError, Does.Contain("missing"));

            SurvivorsLosslessJsonDocument missingRecord = Parse("{\"weapons\":[{\"id\":\"weapon.two\"}]}");
            Assert.That(SurvivorsJsonRecordNavigator.TryLocateRecord(missingRecord, locator, out _, out string recordError), Is.False);
            Assert.That(recordError, Does.Contain("not found"));

            SurvivorsLosslessJsonDocument nestedOnly = Parse(
                "{\"weapons\":[{\"id\":\"weapon.one\",\"nested\":{\"damage\":7}}]}" );
            Assert.That(SurvivorsJsonRecordNavigator.TryLocateRecord(nestedOnly, locator, out SurvivorsJsonNode record, out string locateError), Is.True, locateError);
            Assert.That(SurvivorsJsonRecordNavigator.TryReadDirectScalar(
                nestedOnly,
                record,
                "damage",
                GameContentFieldType.Number,
                true,
                out _,
                out string propertyError), Is.False);
            Assert.That(propertyError, Does.Contain("direct property"));
        }

        [Test]
        public void Locator_RejectsWrongScalarType()
        {
            SurvivorsLosslessJsonDocument document = Parse(
                "{\"weapons\":[{\"id\":\"weapon.one\",\"damage\":\"7\"}]}" );
            var locator = new SurvivorsJsonRecordLocator("weapons", "weapons", "weapon.one", "Weapon");
            Assert.That(SurvivorsJsonRecordNavigator.TryLocateRecord(document, locator, out SurvivorsJsonNode record, out string error), Is.True, error);

            Assert.That(SurvivorsJsonRecordNavigator.TryReadDirectScalar(
                document,
                record,
                "damage",
                GameContentFieldType.Number,
                true,
                out _,
                out error), Is.False);
            Assert.That(error, Does.Contain("expected number"));
        }

        [Test]
        public void Patcher_EncodesStringIntegerNumberAndBooleanScalars()
        {
            const string json = "{\"weapons\":[{\"id\":\"weapon.one\",\"displayName\":\"Old\",\"count\":2,\"damage\":7.000,\"enabled\":false}]}";
            SurvivorsLosslessJsonDocument document = Parse(json);
            var tokens = new Dictionary<string, SurvivorsJsonScalarToken>(StringComparer.Ordinal)
            {
                ["displayName"] = Locate(document, "weapons", "weapon.one", "displayName", GameContentFieldType.String),
                ["count"] = Locate(document, "weapons", "weapon.one", "count", GameContentFieldType.Integer),
                ["damage"] = Locate(document, "weapons", "weapon.one", "damage", GameContentFieldType.Number),
                ["enabled"] = Locate(document, "weapons", "weapon.one", "enabled", GameContentFieldType.Boolean)
            };
            var changes = new Dictionary<string, GameContentFieldValue>(StringComparer.Ordinal)
            {
                ["displayName"] = GameContentFieldValue.FromString("Quote \" slash \\ line\n"),
                ["count"] = GameContentFieldValue.FromInteger(4),
                ["damage"] = GameContentFieldValue.FromNumber(9.25),
                ["enabled"] = GameContentFieldValue.FromBoolean(true)
            };

            Assert.That(SurvivorsLosslessJsonPatcher.TryPatch(
                document,
                tokens,
                changes,
                out string proposed,
                out _,
                out string error), Is.True, error);
            Assert.That(proposed, Is.EqualTo(
                "{\"weapons\":[{\"id\":\"weapon.one\",\"displayName\":\"Quote \\\" slash \\\\ line\\n\",\"count\":4,\"damage\":9.25,\"enabled\":true}]}"));
        }

        [Test]
        public void Patcher_RejectsNonFiniteNumbersAndTypeChanges()
        {
            SurvivorsLosslessJsonDocument document = Parse(
                "{\"weapons\":[{\"id\":\"weapon.one\",\"damage\":7}]}" );
            SurvivorsJsonScalarToken token = Locate(document, "weapons", "weapon.one", "damage", GameContentFieldType.Number);

            Assert.That(TryPatch(document, token, GameContentFieldValue.FromNumber(double.NaN), out string nanError), Is.False);
            Assert.That(nanError, Does.Contain("finite"));
            Assert.That(TryPatch(document, token, GameContentFieldValue.FromString("seven"), out string typeError), Is.False);
            Assert.That(typeError, Does.Contain("does not match"));
        }

        [Test]
        public void CollectionLocator_UsesStableStepIdAndPreservesDuplicateItemIdentity()
        {
            SurvivorsLosslessJsonDocument document = Parse(
                "{\"tutorialSteps\":[{\"id\":\"movement\",\"nested\":{\"lines\":[\"wrong\"]},\"lines\":[\"same\",\"same\"]}]}" );
            SurvivorsJsonCollectionToken token = LocateCollection(document, "tutorialSteps", "movement", "lines");
            GameContentOrderedCollectionValue value = token.Value.OrderedCollectionValue;

            Assert.That(value.ItemType, Is.EqualTo(GameContentFieldType.String));
            Assert.That(value.Items.Select(item => item.Value.StringValue), Is.EqualTo(new[] { "same", "same" }));
            Assert.That(value.Items.Select(item => item.OriginalIndex), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(value.Items[0].ItemKey, Is.Not.EqualTo(value.Items[1].ItemKey));
        }

        [TestCase("{\"tutorialSteps\":[{\"id\":\"movement\"}]}", "missing")]
        [TestCase("{\"tutorialSteps\":[{\"id\":\"movement\",\"lines\":{},\"future\":true}]}", "must be an array")]
        [TestCase("{\"tutorialSteps\":[{\"id\":\"movement\",\"lines\":[\"ok\",7]}]}", "must be a string")]
        [TestCase("{\"tutorialSteps\":[{\"id\":\"movement\",\"lines\":[\"one\"],\"\\u006cines\":[\"two\"]}]}", "more than once")]
        [TestCase("{\"tutorialSteps\":[{\"id\":\"movement\",\"nested\":{\"lines\":[\"wrong\"]}}]}", "direct property")]
        public void CollectionLocator_RejectsUnsafeDirectArrayShapes(string json, string expectedError)
        {
            SurvivorsLosslessJsonDocument document = Parse(json);
            var locator = new SurvivorsJsonRecordLocator("theme.primary", "tutorialSteps", "movement", "TutorialStep");
            Assert.That(SurvivorsJsonRecordNavigator.TryLocateRecord(
                document,
                locator,
                out SurvivorsJsonNode record,
                out string locateError), Is.True, locateError);

            Assert.That(SurvivorsJsonRecordNavigator.TryReadDirectStringCollection(
                record,
                "lines",
                true,
                out _,
                out string error), Is.False);
            Assert.That(error, Does.Contain(expectedError));
        }

        [Test]
        public void CollectionLocator_RejectsDuplicateStableStepIds()
        {
            SurvivorsLosslessJsonDocument document = Parse(
                "{\"tutorialSteps\":[{\"id\":\"movement\",\"lines\":[\"one\"]},{\"id\":\"movement\",\"lines\":[\"two\"]}]}" );
            var locator = new SurvivorsJsonRecordLocator("theme.primary", "tutorialSteps", "movement", "TutorialStep");

            Assert.That(SurvivorsJsonRecordNavigator.TryLocateRecord(document, locator, out _, out string error), Is.False);
            Assert.That(error, Does.Contain("duplicate record id"));
        }

        [Test]
        public void CollectionPatcher_PreservesCompactStyleBomAndEscapesStrings()
        {
            const string json = "{\"future\":1.2300,\"tutorialSteps\":[{\"id\":\"movement\",\"lines\":[\"old\",\"two\"],\"unknown\":true}]}";
            SurvivorsLosslessJsonDocument document = Parse(json, true);
            SurvivorsJsonCollectionToken token = LocateCollection(document, "tutorialSteps", "movement", "lines");
            GameContentFieldValue value = CollectionValue("quote \" slash \\", "line\ncontrol\t", "unicode \u03a9");

            byte[] bytes = PatchCollection(document, token, value, out string proposed);

            Assert.That(bytes.Take(3), Is.EqualTo(new byte[] { 0xEF, 0xBB, 0xBF }));
            Assert.That(proposed.Substring(0, token.Node.Start), Is.EqualTo(json.Substring(0, token.Node.Start)));
            SurvivorsLosslessJsonDocument reparsed = Parse(proposed);
            SurvivorsJsonCollectionToken proposedToken = LocateCollection(reparsed, "tutorialSteps", "movement", "lines");
            Assert.That(proposed.Substring(proposedToken.Node.End), Is.EqualTo(json.Substring(token.Node.End)));
            Assert.That(proposedToken.Value.OrderedCollectionValue.Items.Select(item => item.Value.StringValue),
                Is.EqualTo(new[] { "quote \" slash \\", "line\ncontrol\t", "unicode \u03a9" }));
            string arrayText = proposed.Substring(proposedToken.Node.Start, proposedToken.Node.Length);
            Assert.That(arrayText, Does.Not.Contain("\n"));
            Assert.That(arrayText, Does.Contain("\\\"").And.Contain("\\\\").And.Contain("\\n").And.Contain("\\t"));
        }

        [TestCase("\n")]
        [TestCase("\r\n")]
        public void CollectionPatcher_PreservesMultilineNewlinesIndentationAndOutsideBytes(string newline)
        {
            string originalArray = "[" + newline +
                                   "        \"First\"," + newline +
                                   "        \"Second\"," + newline +
                                   "        \"Third\"" + newline +
                                   "      ]";
            string json = "{" + newline +
                          "  \"tutorialSteps\": [" + newline +
                          "    {" + newline +
                          "      \"id\": \"movement\"," + newline +
                          "      \"lines\": " + originalArray + "," + newline +
                          "      \"unknown\": 1.2300" + newline +
                          "    }" + newline +
                          "  ]" + newline +
                          "}" + newline;
            SurvivorsLosslessJsonDocument document = Parse(json);
            SurvivorsJsonCollectionToken token = LocateCollection(document, "tutorialSteps", "movement", "lines");
            string replacementArray = "[" + newline +
                                      "        \"Third\"," + newline +
                                      "        \"Changed\"," + newline +
                                      "        \"First\"" + newline +
                                      "      ]";

            PatchCollection(document, token, CollectionValue("Third", "Changed", "First"), out string proposed);

            Assert.That(proposed, Is.EqualTo(json.Replace(originalArray, replacementArray)));
            Assert.That(proposed, Does.Contain("\"unknown\": 1.2300"));
        }

        [Test]
        public void CollectionPatcher_PreservesMixedLineEndingsOutsideArraySpan()
        {
            const string originalArray = "[\r\n          \"First\",\r\n          \"Second\"\r\n        ]";
            const string json = "{\r\n  \"future\": 8e0,\n  \"tutorialSteps\": [\r\n" +
                                "    {\"id\":\"movement\", \"lines\": " + originalArray + ", \"tail\":false}\n  ]\r\n}\n";
            SurvivorsLosslessJsonDocument document = Parse(json);
            SurvivorsJsonCollectionToken token = LocateCollection(document, "tutorialSteps", "movement", "lines");
            const string replacementArray = "[\r\n          \"Second\",\r\n          \"First\"\r\n        ]";

            PatchCollection(document, token, CollectionValue("Second", "First"), out string proposed);

            Assert.That(proposed, Is.EqualTo(json.Replace(originalArray, replacementArray)));
        }

        [Test]
        public void CollectionPatcher_DerivesFormattingForOriginallyEmptyArrays()
        {
            const string compactJson = "{\"tutorialSteps\":[{\"id\":\"movement\",\"lines\":[]}]}";
            SurvivorsLosslessJsonDocument compact = Parse(compactJson);
            SurvivorsJsonCollectionToken compactToken = LocateCollection(compact, "tutorialSteps", "movement", "lines");
            PatchCollection(compact, compactToken, CollectionValue("One", "Two"), out string compactProposed);
            Assert.That(compactProposed, Is.EqualTo(compactJson.Replace("[]", "[\"One\", \"Two\"]")));

            const string multilineJson = "{\r\n  \"tutorialSteps\": [{\r\n    \"id\": \"movement\",\r\n    \"lines\": [\r\n    ]\r\n  }]\r\n}";
            SurvivorsLosslessJsonDocument multiline = Parse(multilineJson);
            SurvivorsJsonCollectionToken multilineToken = LocateCollection(multiline, "tutorialSteps", "movement", "lines");
            PatchCollection(multiline, multilineToken, CollectionValue("One", "Two"), out string multilineProposed);
            Assert.That(multilineProposed, Does.Contain("\"lines\": [\r\n      \"One\",\r\n      \"Two\"\r\n    ]"));
        }

        private static SurvivorsLosslessJsonDocument Parse(string json, bool bom = false)
        {
            byte[] payload = Utf8.GetBytes(json);
            byte[] bytes = payload;
            if (bom)
            {
                bytes = new byte[payload.Length + 3];
                bytes[0] = 0xEF;
                bytes[1] = 0xBB;
                bytes[2] = 0xBF;
                Buffer.BlockCopy(payload, 0, bytes, 3, payload.Length);
            }
            Assert.That(SurvivorsLosslessJsonDocument.TryParse(bytes, out SurvivorsLosslessJsonDocument document, out string error), Is.True, error);
            return document;
        }

        private static SurvivorsJsonScalarToken Locate(
            SurvivorsLosslessJsonDocument document,
            string collection,
            string recordId,
            string fieldId,
            GameContentFieldType fieldType)
        {
            var locator = new SurvivorsJsonRecordLocator("source", collection, recordId, "fixture");
            Assert.That(SurvivorsJsonRecordNavigator.TryLocateRecord(document, locator, out SurvivorsJsonNode record, out string error), Is.True, error);
            Assert.That(SurvivorsJsonRecordNavigator.TryReadDirectScalar(
                document,
                record,
                fieldId,
                fieldType,
                true,
                out SurvivorsJsonScalarToken token,
                out error), Is.True, error);
            return token;
        }

        private static SurvivorsJsonCollectionToken LocateCollection(
            SurvivorsLosslessJsonDocument document,
            string collection,
            string recordId,
            string fieldId)
        {
            var locator = new SurvivorsJsonRecordLocator("source", collection, recordId, "fixture");
            Assert.That(SurvivorsJsonRecordNavigator.TryLocateRecord(document, locator, out SurvivorsJsonNode record, out string error), Is.True, error);
            Assert.That(SurvivorsJsonRecordNavigator.TryReadDirectStringCollection(
                record,
                fieldId,
                true,
                out SurvivorsJsonCollectionToken token,
                out error), Is.True, error);
            return token;
        }

        private static GameContentFieldValue CollectionValue(params string[] values)
        {
            var collection = new GameContentOrderedCollectionValue(
                GameContentFieldType.String,
                (values ?? Array.Empty<string>())
                .Select((value, index) => new GameContentCollectionItem(
                    GameContentCollectionItemKey.Create(),
                    index,
                    GameContentFieldValue.FromString(value))));
            return GameContentFieldValue.FromOrderedScalarCollection(collection);
        }

        private static byte[] PatchCollection(
            SurvivorsLosslessJsonDocument document,
            SurvivorsJsonCollectionToken token,
            GameContentFieldValue value,
            out string proposed)
        {
            Assert.That(SurvivorsLosslessJsonPatcher.TryPatch(
                document,
                new Dictionary<string, SurvivorsJsonScalarToken>(StringComparer.Ordinal),
                new Dictionary<string, SurvivorsJsonCollectionToken>(StringComparer.Ordinal) { [token.FieldId] = token },
                new Dictionary<string, GameContentFieldValue>(StringComparer.Ordinal) { [token.FieldId] = value },
                out proposed,
                out byte[] bytes,
                out string error), Is.True, error);
            return bytes;
        }

        private static byte[] Patch(
            SurvivorsLosslessJsonDocument document,
            SurvivorsJsonScalarToken token,
            GameContentFieldValue value,
            out string proposed)
        {
            Assert.That(TryPatch(document, token, value, out proposed, out byte[] bytes, out string error), Is.True, error);
            return bytes;
        }

        private static bool TryPatch(
            SurvivorsLosslessJsonDocument document,
            SurvivorsJsonScalarToken token,
            GameContentFieldValue value,
            out string error)
        {
            return TryPatch(document, token, value, out _, out _, out error);
        }

        private static bool TryPatch(
            SurvivorsLosslessJsonDocument document,
            SurvivorsJsonScalarToken token,
            GameContentFieldValue value,
            out string proposed,
            out byte[] bytes,
            out string error)
        {
            return SurvivorsLosslessJsonPatcher.TryPatch(
                document,
                new Dictionary<string, SurvivorsJsonScalarToken>(StringComparer.Ordinal) { [token.FieldId] = token },
                new Dictionary<string, GameContentFieldValue>(StringComparer.Ordinal) { [token.FieldId] = value },
                out proposed,
                out bytes,
                out error);
        }
    }
}
