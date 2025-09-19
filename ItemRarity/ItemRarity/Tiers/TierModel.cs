using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ItemRarity.Tiers;

public sealed class TierModel
{
    [JsonProperty(Order = 0)]
    public required int Level { get; init; }

    [JsonProperty(Order = 50)]
    public required Dictionary<string, float> Rarities { get; init; }

    public IEnumerable<KeyValuePair<string, float>> GetFilteredRarities(Predicate<string> filter)
    {
        for (var i = Rarities.Count - 1; i >= 0; i--)
        {
            var rarity = Rarities.ElementAt(i);

            yield return filter(rarity.Key) ? rarity : default;
        }
    }
}