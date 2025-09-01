using System.Collections.Generic;
using ItemRarity.Converters.Json;
using ItemRarity.Models;
using Newtonsoft.Json;

namespace ItemRarity.Config;

public sealed class TiersConfig
{
    [JsonProperty(Order = 0)]
    public bool EnableTiers { get; set; }

    [JsonProperty(Order = 50), JsonConverter(typeof(TiersJsonConverter))]
    public Dictionary<string, Tier> Tiers { get; init; } = new();

    public Tier? this[string tierKey] => Tiers.GetValueOrDefault(tierKey);

    public bool TryGetTier(string tierKey, out Tier tier)
    {
        return Tiers.TryGetValue(tierKey, out tier!);
    }
}