using System.Collections.Generic;
using ItemRarity.Converters.Json;
using ItemRarity.Tiers;
using Newtonsoft.Json;

namespace ItemRarity.Config;

public sealed class TiersConfig
{
    [JsonProperty(Order = 0)]
    public bool EnableTiers { get; set; }

    [JsonProperty(Order = 1)]
    public bool AllowTierUpgrade { get; set; } = true;

    [JsonProperty(Order = 50), JsonConverter(typeof(TiersJsonConverter))]
    public Dictionary<int, TierModel> Tiers { get; init; } = new();

    public TierModel? this[int tierKey] => Tiers.GetValueOrDefault(tierKey);

    public bool TryGetTier(int tierKey, out TierModel tierModel)
    {
        return Tiers.TryGetValue(tierKey, out tierModel!);
    }
}