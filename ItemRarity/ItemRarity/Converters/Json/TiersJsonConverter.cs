using System;
using System.Collections.Generic;
using ItemRarity.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ItemRarity.Converters.Json;

public sealed class TiersJsonConverter : JsonConverter<Dictionary<string, Tier>>
{
    public override void WriteJson(JsonWriter writer, Dictionary<string, Tier>? value, JsonSerializer serializer)
    {
        if (value == null || value.Count == 0)
            return;
        writer.WriteStartArray();

        foreach (var tierConfig in value.Values)
            serializer.Serialize(writer, tierConfig);

        writer.WriteEndArray();
    }

    public override Dictionary<string, Tier> ReadJson(JsonReader reader, Type objectType, Dictionary<string, Tier>? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var array = JArray.Load(reader);
        var result = new Dictionary<string, Tier>(array.Count);

        foreach (var tierConfig in array)
        {
            var config = tierConfig.ToObject<Tier>();
            if (config == null)
                continue;
            result.Add(config.Key, config);
        }

        return result;
    }
}