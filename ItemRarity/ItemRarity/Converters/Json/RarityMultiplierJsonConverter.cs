using System;
using System.Globalization;
using ItemRarity.Models;
using Newtonsoft.Json;

namespace ItemRarity.Converters.Json;

public sealed class RarityMultiplierJsonConverter : JsonConverter<RarityMultiplier>
{
    public override void WriteJson(JsonWriter writer, RarityMultiplier? value, JsonSerializer serializer)
    {
        if (value == null)
            return;

        if (Math.Abs(value.Min - value.Max) < 1e-5f)
        {
            writer.WriteValue(value.Min);
        }
        else
        {
            using (new JsonInlineArrayWriterScope(writer))
            {
                writer.WriteValue(value.Min);
                writer.WriteValue(value.Max);
            }
        }
    }

    public override RarityMultiplier ReadJson(JsonReader reader, Type objectType, RarityMultiplier? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.Float or JsonToken.Integer:
            {
                var val = Convert.ToSingle(reader.Value, CultureInfo.InvariantCulture);
                return new RarityMultiplier { Min = val, Max = val };
            }
            case JsonToken.StartArray:
            {
                reader.Read();
                var min = Convert.ToSingle(reader.Value, CultureInfo.InvariantCulture);

                reader.Read();
                var max = Convert.ToSingle(reader.Value, CultureInfo.InvariantCulture);

                while (reader.TokenType != JsonToken.EndArray && reader.Read())
                {
                }

                return new RarityMultiplier { Min = min, Max = max };
            }
            default:
                throw new JsonSerializationException($"Unexpected token {reader.TokenType}");
        }
    }
}

public readonly struct JsonInlineArrayWriterScope : IDisposable
{
    private readonly Formatting _previousFormatting;
    private readonly JsonWriter _writer;

    public JsonInlineArrayWriterScope(JsonWriter writer)
    {
        _writer = writer;
        _previousFormatting = writer.Formatting;

        writer.Formatting = Formatting.None;

        writer.WriteWhitespace(" ");
        writer.WriteStartArray();
    }

    public void Dispose()
    {
        _writer.WriteEndArray();
        _writer.Formatting = _previousFormatting;
    }
}