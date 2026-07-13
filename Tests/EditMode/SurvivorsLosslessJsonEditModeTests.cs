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
