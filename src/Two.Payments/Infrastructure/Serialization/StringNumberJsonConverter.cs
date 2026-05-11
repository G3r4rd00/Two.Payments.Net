using Newtonsoft.Json;
using System;

namespace Two.Payments.Infrastructure.Serialization
{
    /// <summary>
    /// Serializa/Deserializa números que en Two se esperan como string con punto decimal.
    /// Acepta "1", "1.0", "1,00" al deserializar pero siempre serializa con punto y dos decimales.
    /// </summary>
    public class StringNumberJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(string);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var s = (reader.Value ?? string.Empty).ToString();
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d))
                return d.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("es-ES"), out d))
                return d.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            return s; // deja tal cual si no se puede convertir, para que lo valide la API
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var s = value as string;
            if (string.IsNullOrWhiteSpace(s)) { writer.WriteNull(); return; }
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d))
            {
                writer.WriteValue(d.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                return;
            }
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("es-ES"), out d))
            {
                writer.WriteValue(d.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                return;
            }
            writer.WriteValue(s);
        }
    }
}
