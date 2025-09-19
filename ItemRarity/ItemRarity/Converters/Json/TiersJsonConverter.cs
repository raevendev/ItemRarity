using System;
using System.Collections.Generic;
using ItemRarity.Tiers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ItemRarity.Converters.Json;

public sealed class TiersJsonConverter : JsonConverter<Dictionary<int, TierModel>>
{
    public override void WriteJson(JsonWriter writer, Dictionary<int, TierModel>? value, JsonSerializer serializer)
    {
        if (value == null || value.Count == 0)
            return;
        writer.WriteStartArray();

        foreach (var tierConfig in value.Values)
            serializer.Serialize(writer, tierConfig);

        writer.WriteEndArray();
    }

    public override Dictionary<int, TierModel> ReadJson(JsonReader reader, Type objectType, Dictionary<int, TierModel>? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var array = JArray.Load(reader);
        var result = new Dictionary<int, TierModel>(array.Count);

        foreach (var tierConfig in array)
        {
            var config = tierConfig.ToObject<TierModel>();
            if (config == null)
                continue;
            result.Add(config.Level, config);
        }

        return result;
    }
}