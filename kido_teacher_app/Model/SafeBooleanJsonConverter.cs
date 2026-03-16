using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace kido_teacher_app.Model
{
    internal sealed class SafeBooleanJsonConverter : JsonConverter<bool>
    {
        public override bool ReadJson(
            JsonReader reader,
            Type objectType,
            bool existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Boolean)
            {
                return Convert.ToBoolean(reader.Value);
            }

            if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
            {
                return Convert.ToDouble(reader.Value) != 0;
            }

            if (reader.TokenType == JsonToken.String)
            {
                var raw = (reader.Value as string)?.Trim();

                if (string.IsNullOrEmpty(raw))
                {
                    return false;
                }

                if (bool.TryParse(raw, out var parsedBool))
                {
                    return parsedBool;
                }

                if (int.TryParse(raw, out var parsedInt))
                {
                    return parsedInt != 0;
                }

                return false;
            }

            if (reader.TokenType == JsonToken.Null || reader.TokenType == JsonToken.Undefined)
            {
                return false;
            }

            var token = JToken.Load(reader);

            return token.Type switch
            {
                JTokenType.Boolean => token.Value<bool>(),
                JTokenType.Integer => token.Value<long>() != 0,
                JTokenType.Float => Math.Abs(token.Value<double>()) > double.Epsilon,
                JTokenType.String => TryParseString(token.Value<string>()),
                _ => false
            };
        }

        public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        private static bool TryParseString(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            raw = raw.Trim();

            if (bool.TryParse(raw, out var parsedBool))
            {
                return parsedBool;
            }

            if (int.TryParse(raw, out var parsedInt))
            {
                return parsedInt != 0;
            }

            return false;
        }
    }
}
