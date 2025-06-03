using System.Collections.Generic;
using ItemRarity.Converters.Json;
using Newtonsoft.Json;

namespace ItemRarity.Models;

public sealed class Tier
{
    [JsonProperty(Order = 0)]
    public required string Key { get; init; }

    [JsonProperty(Order = 50)]
    public required Dictionary<string, float> Rarities { get; init; }
}