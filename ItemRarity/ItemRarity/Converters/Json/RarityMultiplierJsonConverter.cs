using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
                var values = new List<float>();

                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType is JsonToken.Float or JsonToken.Integer)
                    {
                        values.Add(Convert.ToSingle(reader.Value, CultureInfo.InvariantCulture));
                    }
                }

                return values.Count switch
                {
                    1 => new RarityMultiplier { Min = values[0], Max = values[0] },
                    _ => new RarityMultiplier { Min = values.Min(), Max = values.Max() },
                };
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