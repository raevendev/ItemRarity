using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ItemRarity.Models;

/// <summary>
/// Represents the configuration settings for an item's rarity, including multipliers for various item properties.
/// </summary>
public sealed class Rarity
{
    [JsonProperty(Order = 0)]
    public required string Key { get; init; }

    [JsonProperty(Order = 1)]
    public required string Name { get; init; }

    [JsonProperty(Order = 2)]
    public required string Color { get; init; }

    [JsonProperty(Order = 3)]
    public required float Weight { get; init; }

    [JsonProperty(Order = 4)]
    public float DurabilityMultiplier { get; init; } = 1F;

    [JsonProperty(Order = 5)]
    public float MiningSpeedMultiplier { get; init; } = 1F;

    [JsonProperty(Order = 6)]
    public float AttackPowerMultiplier { get; init; } = 1F;

    [JsonProperty(Order = 7)]
    public float PiercingPowerMultiplier { get; init; } = 1F;

    [JsonProperty(Order = 8)]
    public float ArmorFlatDamageReductionMultiplier { get; init; } = 1F;

    [JsonProperty(Order = 9)]
    public float ArmorPerTierFlatDamageProtectionLossMultiplier { get; init; } = 1F;

    [JsonIgnore]
    public float ArmorRelativeProtectionMultiplier { get; init; } = 1F;

    [JsonProperty(Order = 10)]
    public float ArmorPerTierRelativeProtectionLossMultiplier { get; init; } = 1F;

    [JsonProperty(Order = 11)]
    public float ShieldProtectionMultiplier { get; init; } = 1F;

    [JsonProperty(Order = 99)]
    public Dictionary<string, float> CustomAttributes { get; init; } = new();

    [JsonProperty(Order = 100)]
    public string[] Effects { get; init; } = [];

    [JsonProperty(Order = 101)]
    public bool IgnoreTranslation { get; init; } = false;

    public bool HasEffect(string value)
    {
        foreach (var effect in Effects)
        {
            if (effect.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                return true;
        }

        return false;
    }
}