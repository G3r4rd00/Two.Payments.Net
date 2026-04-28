using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Two.Payments.Infrastructure.Serialization
{
    internal sealed class FlexibleIntJsonConverter : JsonConverter<int>
    {
        public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override int ReadJson(
            JsonReader reader,
            Type objectType,
            int existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                return Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture);
            }

            if (reader.TokenType == JsonToken.Float)
            {
                var number = Convert.ToDecimal(reader.Value, CultureInfo.InvariantCulture);
                return decimal.ToInt32(decimal.Round(number, 0, MidpointRounding.AwayFromZero));
            }

            if (reader.TokenType == JsonToken.String)
            {
                if (int.TryParse((string)reader.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
                {
                    return intValue;
                }

                if (decimal.TryParse((string)reader.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
                {
                    return decimal.ToInt32(decimal.Round(decimalValue, 0, MidpointRounding.AwayFromZero));
                }
            }

            throw new JsonSerializationException(
                $"Unexpected token {reader.TokenType} when parsing integer value.");
        }
    }
}
