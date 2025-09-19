using System;
using System.Collections.Generic;
using ItemRarity.Rarities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ItemRarity.Converters.Json;

public sealed class RaritiesJsonConverter : JsonConverter<Dictionary<string, RarityModel>>
{
    public override void WriteJson(JsonWriter writer, Dictionary<string, RarityModel>? value, JsonSerializer serializer)
    {
        if (value == null || value.Count == 0)
            return;
        
        writer.WriteStartArray();

        foreach (var rarityConfig in value.Values)
            serializer.Serialize(writer, rarityConfig);

        writer.WriteEndArray();
    }

    public override Dictionary<string, RarityModel> ReadJson(JsonReader reader, Type objectType, Dictionary<string, RarityModel>? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var array = JArray.Load(reader);
        var result = new Dictionary<string, RarityModel>(array.Count);

        foreach (var rarityConfig in array)
        {
            var config = rarityConfig.ToObject<RarityModel>();
            if (config == null)
                continue;
            result.Add(config.Key, config);
        }

        return result;
    }
}