using System.Collections.Generic;
using ItemRarity.Converters.Json;
using ItemRarity.Rarities;
using Newtonsoft.Json;

namespace ItemRarity.Config;

public sealed class RaritiesConfig
{
    [JsonProperty(Order = 0)]
    public bool ApplyRarityOnCraft { get; init; } = true;

    [JsonProperty(Order = 1)]
    public bool ApplyRarityOnItemDrop { get; init; } = true;

    [JsonProperty(Order = 50), JsonConverter(typeof(RaritiesJsonConverter))]
    public Dictionary<string, RarityModel> Rarities { get; init; } = new();

    public RarityModel? this[string rarityKey] => Rarities.GetValueOrDefault(rarityKey);

    public bool TryGetRarity(string rarityKey, out RarityModel rarityModel)
    {
        return Rarities.TryGetValue(rarityKey, out rarityModel!);
    }
}