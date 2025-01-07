using System;
using Newtonsoft.Json;

namespace ItemRarity.Config;

/// <summary>
/// Represents the configuration settings for an item's rarity, including multipliers for various item properties.
/// </summary>
public sealed class ItemRarityConfig
{
    public required string Name { get; init; }

    public required string Color { get; init; }

    public required float Rarity { get; init; }

    public float DurabilityMultiplier { get; init; } = 1F;

    public float MiningSpeedMultiplier { get; init; } = 1F;

    public float AttackPowerMultiplier { get; init; } = 1F;

    public float PiercingPowerMultiplier { get; init; } = 1F;

    public float FlatDamageReductionMultiplier { get; init; } = 1F;

    public float PerTierFlatDamageProtectionLossMultiplier { get; init; } = 1F;

    [JsonIgnore]
    public float RelativeProtectionMultiplier { get; init; } = 1F;

    public float PerTierRelativeProtectionLossMultiplier { get; init; } = 1F;

    /// <summary>
    /// Gets the effects applied to an item's based on its rarity.
    /// </summary>
    public string[] Effects { get; init; } = [];

    /// <summary>
    /// Gets which items are supported by this rarity
    /// </summary>
    public string[] SupportedItems { get; init; } = ["*", "NOT-YET-SUPPORTED"];

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