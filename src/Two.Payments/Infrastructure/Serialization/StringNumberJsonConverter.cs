using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Two.Payments.Infrastructure.Serialization
{
    internal sealed class StringNumberJsonConverter : JsonConverter<string>
    {
        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                writer.WriteNull();
                return;
            }

            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
            {
                throw new JsonSerializationException($"Value '{value}' is not a valid numeric string.");
            }

            writer.WriteValue(number);
        }

        public override string ReadJson(
            JsonReader reader,
            Type objectType,
            string existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
            {
                return Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            }

            if (reader.TokenType == JsonToken.String)
            {
                return (string)reader.Value;
            }

            throw new JsonSerializationException(
                $"Unexpected token {reader.TokenType} when parsing numeric string value.");
        }
    }
}
