using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Deucarian.GameContentAuthoring.Editor;

namespace Deucarian.TemplateGameSurvivors.Editor
{
    internal enum SurvivorsJsonTokenKind
    {
        ObjectStart,
        ObjectEnd,
        ArrayStart,
        ArrayEnd,
        PropertyName,
        String,
        Number,
        True,
        False,
        Null,
        Colon,
        Comma,
        Whitespace
    }

    internal enum SurvivorsJsonValueKind
    {
        Object,
        Array,
        String,
        Number,
        Boolean,
        Null
    }

    internal sealed class SurvivorsJsonTokenSpan
    {
        public SurvivorsJsonTokenSpan(SurvivorsJsonTokenKind kind, int start, int length)
        {
            Kind = kind;
            Start = start;
            Length = length;
        }

        public SurvivorsJsonTokenKind Kind { get; }
        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;
    }

    internal sealed class SurvivorsJsonProperty
    {
        public SurvivorsJsonProperty(string name, SurvivorsJsonNode value)
        {
            Name = name ?? string.Empty;
            Value = value;
        }

        public string Name { get; }
        public SurvivorsJsonNode Value { get; }
    }

    internal sealed class SurvivorsJsonNode
    {
        public SurvivorsJsonNode(
            SurvivorsJsonValueKind kind,
            int start,
            int end,
            string stringValue = null,
            bool booleanValue = false,
            IEnumerable<SurvivorsJsonProperty> properties = null,
            IEnumerable<SurvivorsJsonNode> elements = null)
        {
            Kind = kind;
            Start = start;
            End = end;
            StringValue = stringValue ?? string.Empty;
            BooleanValue = booleanValue;
            Properties = properties == null ? Array.Empty<SurvivorsJsonProperty>() : properties.ToArray();
            Elements = elements == null ? Array.Empty<SurvivorsJsonNode>() : elements.ToArray();
        }

        public SurvivorsJsonValueKind Kind { get; }
        public int Start { get; }
        public int End { get; }
        public int Length => End - Start;
        public string StringValue { get; }
        public bool BooleanValue { get; }
        public IReadOnlyList<SurvivorsJsonProperty> Properties { get; }
        public IReadOnlyList<SurvivorsJsonNode> Elements { get; }
    }

    internal sealed class SurvivorsLosslessJsonDocument
    {
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        private SurvivorsLosslessJsonDocument(
            byte[] originalBytes,
            string text,
            bool hasUtf8Bom,
            SurvivorsJsonNode root,
            IReadOnlyList<SurvivorsJsonTokenSpan> tokens)
        {
            OriginalBytes = originalBytes;
            Text = text;
            HasUtf8Bom = hasUtf8Bom;
            Root = root;
            Tokens = tokens;
        }

        public byte[] OriginalBytes { get; }
        public string Text { get; }
        public bool HasUtf8Bom { get; }
        public SurvivorsJsonNode Root { get; }
        public IReadOnlyList<SurvivorsJsonTokenSpan> Tokens { get; }

        public static bool TryParse(
            byte[] sourceBytes,
            out SurvivorsLosslessJsonDocument document,
            out string error)
        {
            document = null;
            error = string.Empty;
            if (sourceBytes == null)
            {
                error = "JSON source bytes are required.";
                return false;
            }

            bool hasBom = sourceBytes.Length >= 3 &&
                          sourceBytes[0] == 0xEF &&
                          sourceBytes[1] == 0xBB &&
                          sourceBytes[2] == 0xBF;
            int offset = hasBom ? 3 : 0;
            string text;
            try
            {
                text = StrictUtf8.GetString(sourceBytes, offset, sourceBytes.Length - offset);
            }
            catch (DecoderFallbackException exception)
            {
                error = "JSON source is not strict UTF-8: " + exception.Message;
                return false;
            }

            var parser = new Parser(text);
            if (!parser.TryParse(out SurvivorsJsonNode root, out error)) return false;
            document = new SurvivorsLosslessJsonDocument(
                (byte[])sourceBytes.Clone(),
                text,
                hasBom,
                root,
                parser.Tokens.ToArray());
            return true;
        }

        public byte[] Encode(string text)
        {
            byte[] payload = StrictUtf8.GetBytes(text ?? string.Empty);
            if (!HasUtf8Bom) return payload;

            var bytes = new byte[payload.Length + 3];
            bytes[0] = 0xEF;
            bytes[1] = 0xBB;
            bytes[2] = 0xBF;
            Buffer.BlockCopy(payload, 0, bytes, 3, payload.Length);
            return bytes;
        }

        private sealed class Parser
        {
            private const int MaximumDepth = 256;
            private readonly string _text;
            private readonly List<SurvivorsJsonTokenSpan> _tokens = new List<SurvivorsJsonTokenSpan>();
            private int _position;
            private string _error = string.Empty;

            public Parser(string text)
            {
                _text = text ?? string.Empty;
            }

            public IReadOnlyList<SurvivorsJsonTokenSpan> Tokens => _tokens;

            public bool TryParse(out SurvivorsJsonNode root, out string error)
            {
                root = null;
                SkipWhitespace();
                if (!TryParseValue(0, out root))
                {
                    error = _error;
                    return false;
                }

                SkipWhitespace();
                if (_position != _text.Length)
                {
                    error = Error("Unexpected content after the root JSON value.");
                    root = null;
                    return false;
                }

                error = string.Empty;
                return true;
            }

            private bool TryParseValue(int depth, out SurvivorsJsonNode node)
            {
                node = null;
                if (depth > MaximumDepth) return Fail("JSON nesting exceeds the supported depth.");
                SkipWhitespace();
                if (_position >= _text.Length) return Fail("Expected a JSON value.");

                char current = _text[_position];
                if (current == '{') return TryParseObject(depth, out node);
                if (current == '[') return TryParseArray(depth, out node);
                if (current == '"') return TryParseString(false, out node);
                if (current == '-' || IsDigit(current)) return TryParseNumber(out node);
                if (current == 't') return TryParseLiteral("true", SurvivorsJsonTokenKind.True, SurvivorsJsonValueKind.Boolean, true, out node);
                if (current == 'f') return TryParseLiteral("false", SurvivorsJsonTokenKind.False, SurvivorsJsonValueKind.Boolean, false, out node);
                if (current == 'n') return TryParseLiteral("null", SurvivorsJsonTokenKind.Null, SurvivorsJsonValueKind.Null, false, out node);
                return Fail("Unexpected character while reading a JSON value.");
            }

            private bool TryParseObject(int depth, out SurvivorsJsonNode node)
            {
                int start = _position;
                AddSingle(SurvivorsJsonTokenKind.ObjectStart);
                _position++;
                SkipWhitespace();
                var properties = new List<SurvivorsJsonProperty>();
                if (Consume('}', SurvivorsJsonTokenKind.ObjectEnd))
                {
                    node = new SurvivorsJsonNode(SurvivorsJsonValueKind.Object, start, _position, properties: properties);
                    return true;
                }

                while (true)
                {
                    if (_position >= _text.Length || _text[_position] != '"')
                    {
                        node = null;
                        return Fail("Expected a quoted property name.");
                    }

                    if (!TryParseString(true, out SurvivorsJsonNode propertyName))
                    {
                        node = null;
                        return false;
                    }

                    SkipWhitespace();
                    if (!Consume(':', SurvivorsJsonTokenKind.Colon))
                    {
                        node = null;
                        return Fail("Expected ':' after a property name.");
                    }

                    if (!TryParseValue(depth + 1, out SurvivorsJsonNode value))
                    {
                        node = null;
                        return false;
                    }

                    properties.Add(new SurvivorsJsonProperty(propertyName.StringValue, value));
                    SkipWhitespace();
                    if (Consume('}', SurvivorsJsonTokenKind.ObjectEnd))
                    {
                        node = new SurvivorsJsonNode(SurvivorsJsonValueKind.Object, start, _position, properties: properties);
                        return true;
                    }

                    if (!Consume(',', SurvivorsJsonTokenKind.Comma))
                    {
                        node = null;
                        return Fail("Expected ',' or '}' after an object property.");
                    }

                    SkipWhitespace();
                    if (_position < _text.Length && _text[_position] == '}')
                    {
                        node = null;
                        return Fail("Trailing commas are not valid JSON.");
                    }
                }
            }

            private bool TryParseArray(int depth, out SurvivorsJsonNode node)
            {
                int start = _position;
                AddSingle(SurvivorsJsonTokenKind.ArrayStart);
                _position++;
                SkipWhitespace();
                var elements = new List<SurvivorsJsonNode>();
                if (Consume(']', SurvivorsJsonTokenKind.ArrayEnd))
                {
                    node = new SurvivorsJsonNode(SurvivorsJsonValueKind.Array, start, _position, elements: elements);
                    return true;
                }

                while (true)
                {
                    if (!TryParseValue(depth + 1, out SurvivorsJsonNode value))
                    {
                        node = null;
                        return false;
                    }

                    elements.Add(value);
                    SkipWhitespace();
                    if (Consume(']', SurvivorsJsonTokenKind.ArrayEnd))
                    {
                        node = new SurvivorsJsonNode(SurvivorsJsonValueKind.Array, start, _position, elements: elements);
                        return true;
                    }

                    if (!Consume(',', SurvivorsJsonTokenKind.Comma))
                    {
                        node = null;
                        return Fail("Expected ',' or ']' after an array value.");
                    }

                    SkipWhitespace();
                    if (_position < _text.Length && _text[_position] == ']')
                    {
                        node = null;
                        return Fail("Trailing commas are not valid JSON.");
                    }
                }
            }

            private bool TryParseString(bool propertyName, out SurvivorsJsonNode node)
            {
                int start = _position;
                _position++;
                var value = new StringBuilder();
                while (_position < _text.Length)
                {
                    char current = _text[_position++];
                    if (current == '"')
                    {
                        _tokens.Add(new SurvivorsJsonTokenSpan(
                            propertyName ? SurvivorsJsonTokenKind.PropertyName : SurvivorsJsonTokenKind.String,
                            start,
                            _position - start));
                        node = new SurvivorsJsonNode(
                            SurvivorsJsonValueKind.String,
                            start,
                            _position,
                            value.ToString());
                        return true;
                    }

                    if (current < 0x20)
                    {
                        node = null;
                        return Fail("Unescaped control character in JSON string.");
                    }

                    if (current == '\\')
                    {
                        if (!TryReadEscape(value))
                        {
                            node = null;
                            return false;
                        }
                        continue;
                    }

                    if (char.IsHighSurrogate(current))
                    {
                        if (_position >= _text.Length || !char.IsLowSurrogate(_text[_position]))
                        {
                            node = null;
                            return Fail("JSON string contains an unpaired high surrogate.");
                        }
                        value.Append(current);
                        value.Append(_text[_position++]);
                        continue;
                    }

                    if (char.IsLowSurrogate(current))
                    {
                        node = null;
                        return Fail("JSON string contains an unpaired low surrogate.");
                    }

                    value.Append(current);
                }

                node = null;
                return Fail("Unterminated JSON string.");
            }

            private bool TryReadEscape(StringBuilder value)
            {
                if (_position >= _text.Length) return Fail("Unterminated JSON escape sequence.");
                char escaped = _text[_position++];
                switch (escaped)
                {
                    case '"': value.Append('"'); return true;
                    case '\\': value.Append('\\'); return true;
                    case '/': value.Append('/'); return true;
                    case 'b': value.Append('\b'); return true;
                    case 'f': value.Append('\f'); return true;
                    case 'n': value.Append('\n'); return true;
                    case 'r': value.Append('\r'); return true;
                    case 't': value.Append('\t'); return true;
                    case 'u':
                        if (!TryReadHexCodeUnit(out char codeUnit)) return false;
                        if (char.IsHighSurrogate(codeUnit))
                        {
                            if (_position + 1 >= _text.Length || _text[_position] != '\\' || _text[_position + 1] != 'u')
                                return Fail("Escaped high surrogate is not followed by an escaped low surrogate.");
                            _position += 2;
                            if (!TryReadHexCodeUnit(out char low) || !char.IsLowSurrogate(low))
                                return Fail("Escaped high surrogate is not followed by a valid low surrogate.");
                            value.Append(codeUnit);
                            value.Append(low);
                            return true;
                        }
                        if (char.IsLowSurrogate(codeUnit))
                            return Fail("Escaped low surrogate has no leading high surrogate.");
                        value.Append(codeUnit);
                        return true;
                    default:
                        return Fail("Unsupported JSON escape sequence.");
                }
            }

            private bool TryReadHexCodeUnit(out char value)
            {
                value = default;
                if (_position + 4 > _text.Length) return Fail("Incomplete JSON unicode escape.");
                int parsed = 0;
                for (int index = 0; index < 4; index++)
                {
                    int digit = HexValue(_text[_position + index]);
                    if (digit < 0) return Fail("Invalid JSON unicode escape.");
                    parsed = (parsed << 4) | digit;
                }
                _position += 4;
                value = (char)parsed;
                return true;
            }

            private bool TryParseNumber(out SurvivorsJsonNode node)
            {
                int start = _position;
                if (Peek('-')) _position++;
                if (_position >= _text.Length)
                {
                    node = null;
                    return Fail("Incomplete JSON number.");
                }

                if (Peek('0'))
                {
                    _position++;
                    if (_position < _text.Length && IsDigit(_text[_position]))
                    {
                        node = null;
                        return Fail("JSON numbers cannot contain leading zeroes.");
                    }
                }
                else
                {
                    if (!IsDigitOneToNine(_text[_position]))
                    {
                        node = null;
                        return Fail("Invalid JSON number.");
                    }
                    while (_position < _text.Length && IsDigit(_text[_position])) _position++;
                }

                if (Peek('.'))
                {
                    _position++;
                    int fractionStart = _position;
                    while (_position < _text.Length && IsDigit(_text[_position])) _position++;
                    if (_position == fractionStart)
                    {
                        node = null;
                        return Fail("JSON number fraction requires at least one digit.");
                    }
                }

                if (Peek('e') || Peek('E'))
                {
                    _position++;
                    if (Peek('+') || Peek('-')) _position++;
                    int exponentStart = _position;
                    while (_position < _text.Length && IsDigit(_text[_position])) _position++;
                    if (_position == exponentStart)
                    {
                        node = null;
                        return Fail("JSON number exponent requires at least one digit.");
                    }
                }

                _tokens.Add(new SurvivorsJsonTokenSpan(SurvivorsJsonTokenKind.Number, start, _position - start));
                node = new SurvivorsJsonNode(SurvivorsJsonValueKind.Number, start, _position);
                return true;
            }

            private bool TryParseLiteral(
                string literal,
                SurvivorsJsonTokenKind tokenKind,
                SurvivorsJsonValueKind valueKind,
                bool booleanValue,
                out SurvivorsJsonNode node)
            {
                int start = _position;
                if (_position + literal.Length > _text.Length ||
                    !string.Equals(_text.Substring(_position, literal.Length), literal, StringComparison.Ordinal))
                {
                    node = null;
                    return Fail("Invalid JSON literal.");
                }

                _position += literal.Length;
                _tokens.Add(new SurvivorsJsonTokenSpan(tokenKind, start, literal.Length));
                node = new SurvivorsJsonNode(valueKind, start, _position, booleanValue: booleanValue);
                return true;
            }

            private void SkipWhitespace()
            {
                int start = _position;
                while (_position < _text.Length)
                {
                    char current = _text[_position];
                    if (current != ' ' && current != '\t' && current != '\r' && current != '\n') break;
                    _position++;
                }
                if (_position > start)
                    _tokens.Add(new SurvivorsJsonTokenSpan(SurvivorsJsonTokenKind.Whitespace, start, _position - start));
            }

            private bool Consume(char expected, SurvivorsJsonTokenKind tokenKind)
            {
                if (_position >= _text.Length || _text[_position] != expected) return false;
                AddSingle(tokenKind);
                _position++;
                return true;
            }

            private void AddSingle(SurvivorsJsonTokenKind tokenKind)
            {
                _tokens.Add(new SurvivorsJsonTokenSpan(tokenKind, _position, 1));
            }

            private bool Peek(char value)
            {
                return _position < _text.Length && _text[_position] == value;
            }

            private bool Fail(string message)
            {
                _error = Error(message);
                return false;
            }

            private string Error(string message)
            {
                return message + " Character offset " + _position.ToString(CultureInfo.InvariantCulture) + ".";
            }

            private static bool IsDigit(char value)
            {
                return value >= '0' && value <= '9';
            }

            private static bool IsDigitOneToNine(char value)
            {
                return value >= '1' && value <= '9';
            }

            private static int HexValue(char value)
            {
                if (value >= '0' && value <= '9') return value - '0';
                if (value >= 'a' && value <= 'f') return value - 'a' + 10;
                if (value >= 'A' && value <= 'F') return value - 'A' + 10;
                return -1;
            }
        }
    }

    internal sealed class SurvivorsJsonRecordLocator
    {
        public SurvivorsJsonRecordLocator(string sourceId, string collectionName, string recordId, string recordKind)
        {
            SourceId = sourceId ?? string.Empty;
            CollectionName = collectionName ?? string.Empty;
            RecordId = recordId ?? string.Empty;
            RecordKind = recordKind ?? string.Empty;
        }

        public string SourceId { get; }
        public string CollectionName { get; }
        public string RecordId { get; }
        public string RecordKind { get; }
    }

    internal sealed class SurvivorsJsonScalarToken
    {
        public SurvivorsJsonScalarToken(string fieldId, SurvivorsJsonNode node, GameContentFieldValue value)
        {
            FieldId = fieldId ?? string.Empty;
            Node = node;
            Value = value;
        }

        public string FieldId { get; }
        public SurvivorsJsonNode Node { get; }
        public GameContentFieldValue Value { get; }
    }

    internal static class SurvivorsJsonRecordNavigator
    {
        public static bool TryLocateRecord(
            SurvivorsLosslessJsonDocument document,
            SurvivorsJsonRecordLocator locator,
            out SurvivorsJsonNode record,
            out string error)
        {
            record = null;
            error = string.Empty;
            if (document == null || document.Root == null)
            {
                error = "A parsed JSON document is required.";
                return false;
            }
            if (locator == null || string.IsNullOrWhiteSpace(locator.CollectionName) || string.IsNullOrWhiteSpace(locator.RecordId))
            {
                error = "A complete record locator is required.";
                return false;
            }
            if (document.Root.Kind != SurvivorsJsonValueKind.Object)
            {
                error = "The JSON root must be an object.";
                return false;
            }

            if (!TryGetUniqueProperty(document.Root, locator.CollectionName, out SurvivorsJsonProperty collection, out error))
                return false;
            if (collection == null)
            {
                error = "Root collection '" + locator.CollectionName + "' is missing.";
                return false;
            }
            if (collection.Value.Kind != SurvivorsJsonValueKind.Array)
            {
                error = "Root collection '" + locator.CollectionName + "' must be an array.";
                return false;
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < collection.Value.Elements.Count; index++)
            {
                SurvivorsJsonNode candidate = collection.Value.Elements[index];
                if (candidate.Kind != SurvivorsJsonValueKind.Object)
                {
                    error = "Collection '" + locator.CollectionName + "' contains a non-object record at index " + index + ".";
                    return false;
                }
                if (!TryGetUniqueProperty(candidate, "id", out SurvivorsJsonProperty idProperty, out error)) return false;
                if (idProperty == null || idProperty.Value.Kind != SurvivorsJsonValueKind.String)
                {
                    error = "Record at index " + index + " in collection '" + locator.CollectionName + "' requires one direct string id.";
                    return false;
                }

                string candidateId = idProperty.Value.StringValue;
                if (!ids.Add(candidateId))
                {
                    error = "Collection '" + locator.CollectionName + "' contains duplicate record id '" + candidateId + "'.";
                    return false;
                }
                if (string.Equals(candidateId, locator.RecordId, StringComparison.Ordinal)) record = candidate;
            }

            if (record != null) return true;
            error = "Record '" + locator.RecordId + "' was not found in collection '" + locator.CollectionName + "'.";
            return false;
        }

        public static bool TryGetUniqueProperty(
            SurvivorsJsonNode objectNode,
            string propertyName,
            out SurvivorsJsonProperty property,
            out string error)
        {
            property = null;
            error = string.Empty;
            if (objectNode == null || objectNode.Kind != SurvivorsJsonValueKind.Object)
            {
                error = "A JSON object is required to locate direct property '" + propertyName + "'.";
                return false;
            }

            SurvivorsJsonProperty[] matches = objectNode.Properties
                .Where(candidate => string.Equals(candidate.Name, propertyName, StringComparison.Ordinal))
                .ToArray();
            if (matches.Length > 1)
            {
                error = "Direct property '" + propertyName + "' appears more than once and is unsafe to edit.";
                return false;
            }
            property = matches.Length == 0 ? null : matches[0];
            return true;
        }

        public static bool TryReadDirectScalar(
            SurvivorsLosslessJsonDocument document,
            SurvivorsJsonNode record,
            string fieldId,
            GameContentFieldType fieldType,
            bool required,
            out SurvivorsJsonScalarToken token,
            out string error)
        {
            token = null;
            if (!TryGetUniqueProperty(record, fieldId, out SurvivorsJsonProperty property, out error)) return false;
            if (property == null)
            {
                if (!required) return true;
                error = "Required direct property '" + fieldId + "' is missing.";
                return false;
            }

            if (!TryReadValue(document, property.Value, fieldType, out GameContentFieldValue value, out error))
            {
                error = "Direct property '" + fieldId + "' " + error;
                return false;
            }
            token = new SurvivorsJsonScalarToken(fieldId, property.Value, value);
            return true;
        }

        private static bool TryReadValue(
            SurvivorsLosslessJsonDocument document,
            SurvivorsJsonNode node,
            GameContentFieldType fieldType,
            out GameContentFieldValue value,
            out string error)
        {
            value = null;
            error = string.Empty;
            if (document == null || node == null)
            {
                error = "has no scalar value.";
                return false;
            }

            switch (fieldType)
            {
                case GameContentFieldType.String:
                    if (node.Kind != SurvivorsJsonValueKind.String) break;
                    value = GameContentFieldValue.FromString(node.StringValue);
                    return true;
                case GameContentFieldType.Enum:
                    if (node.Kind != SurvivorsJsonValueKind.String) break;
                    value = GameContentFieldValue.FromEnum(node.StringValue);
                    return true;
                case GameContentFieldType.Boolean:
                    if (node.Kind != SurvivorsJsonValueKind.Boolean) break;
                    value = GameContentFieldValue.FromBoolean(node.BooleanValue);
                    return true;
                case GameContentFieldType.Integer:
                    if (node.Kind != SurvivorsJsonValueKind.Number) break;
                    string integerToken = document.Text.Substring(node.Start, node.Length);
                    if (long.TryParse(integerToken, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long integer))
                    {
                        value = GameContentFieldValue.FromInteger(integer);
                        return true;
                    }
                    error = "must use an invariant integer token.";
                    return false;
                case GameContentFieldType.Number:
                    if (node.Kind != SurvivorsJsonValueKind.Number) break;
                    string numberToken = document.Text.Substring(node.Start, node.Length);
                    if (double.TryParse(numberToken, NumberStyles.Float, CultureInfo.InvariantCulture, out double number) &&
                        !double.IsNaN(number) && !double.IsInfinity(number))
                    {
                        value = GameContentFieldValue.FromNumber(number);
                        return true;
                    }
                    error = "must use a finite invariant number token.";
                    return false;
            }

            error = "does not match the expected " + fieldType.ToString().ToLowerInvariant() + " scalar type.";
            return false;
        }
    }

    internal static class SurvivorsLosslessJsonPatcher
    {
        private sealed class Replacement
        {
            public int Start;
            public int Length;
            public string Text;
        }

        public static bool TryPatch(
            SurvivorsLosslessJsonDocument document,
            IReadOnlyDictionary<string, SurvivorsJsonScalarToken> tokens,
            IReadOnlyDictionary<string, GameContentFieldValue> changes,
            out string proposedText,
            out byte[] proposedBytes,
            out string error)
        {
            proposedText = null;
            proposedBytes = null;
            error = string.Empty;
            if (document == null)
            {
                error = "A parsed JSON document is required.";
                return false;
            }

            if (changes == null || changes.Count == 0)
            {
                proposedText = document.Text;
                proposedBytes = (byte[])document.OriginalBytes.Clone();
                return true;
            }

            var replacements = new List<Replacement>(changes.Count);
            foreach (KeyValuePair<string, GameContentFieldValue> change in changes)
            {
                if (tokens == null || !tokens.TryGetValue(change.Key, out SurvivorsJsonScalarToken token) || token?.Node == null)
                {
                    error = "No unambiguous scalar token exists for field '" + change.Key + "'.";
                    return false;
                }
                if (!TryEncodeValue(change.Value, token.Node.Kind, out string replacement, out error))
                {
                    error = "Field '" + change.Key + "': " + error;
                    return false;
                }
                replacements.Add(new Replacement
                {
                    Start = token.Node.Start,
                    Length = token.Node.Length,
                    Text = replacement
                });
            }

            Replacement[] ordered = replacements.OrderByDescending(value => value.Start).ToArray();
            for (int index = 1; index < ordered.Length; index++)
            {
                if (ordered[index - 1].Start < ordered[index].Start + ordered[index].Length)
                {
                    error = "Scalar replacement spans overlap.";
                    return false;
                }
            }

            var builder = new StringBuilder(document.Text);
            for (int index = 0; index < ordered.Length; index++)
            {
                Replacement replacement = ordered[index];
                builder.Remove(replacement.Start, replacement.Length);
                builder.Insert(replacement.Start, replacement.Text);
            }

            proposedText = builder.ToString();
            proposedBytes = document.Encode(proposedText);
            return true;
        }

        private static bool TryEncodeValue(
            GameContentFieldValue value,
            SurvivorsJsonValueKind expectedKind,
            out string encoded,
            out string error)
        {
            encoded = null;
            error = string.Empty;
            if (value == null)
            {
                error = "A scalar value is required.";
                return false;
            }

            switch (value.FieldType)
            {
                case GameContentFieldType.String:
                case GameContentFieldType.Enum:
                    if (expectedKind != SurvivorsJsonValueKind.String) break;
                    encoded = EncodeString(value.StringValue);
                    return true;
                case GameContentFieldType.Integer:
                    if (expectedKind != SurvivorsJsonValueKind.Number) break;
                    encoded = value.IntegerValue.ToString(CultureInfo.InvariantCulture);
                    return true;
                case GameContentFieldType.Number:
                    if (expectedKind != SurvivorsJsonValueKind.Number) break;
                    if (double.IsNaN(value.NumberValue) || double.IsInfinity(value.NumberValue))
                    {
                        error = "Number must be finite.";
                        return false;
                    }
                    encoded = value.NumberValue.ToString("R", CultureInfo.InvariantCulture);
                    return true;
                case GameContentFieldType.Boolean:
                    if (expectedKind != SurvivorsJsonValueKind.Boolean) break;
                    encoded = value.BooleanValue ? "true" : "false";
                    return true;
            }

            error = "Proposed value type does not match the original JSON scalar token.";
            return false;
        }

        private static string EncodeString(string value)
        {
            var builder = new StringBuilder((value ?? string.Empty).Length + 2);
            builder.Append('"');
            string safe = value ?? string.Empty;
            for (int index = 0; index < safe.Length; index++)
            {
                char current = safe[index];
                switch (current)
                {
                    case '"': builder.Append("\\\""); break;
                    case '\\': builder.Append("\\\\"); break;
                    case '\b': builder.Append("\\b"); break;
                    case '\f': builder.Append("\\f"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    default:
                        if (current < 0x20)
                            builder.Append("\\u").Append(((int)current).ToString("X4", CultureInfo.InvariantCulture));
                        else
                            builder.Append(current);
                        break;
                }
            }
            builder.Append('"');
            return builder.ToString();
        }
    }
}
