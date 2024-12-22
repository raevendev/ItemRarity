using System;

namespace ItemRarity.Config;

/// <summary>
/// Represents the configuration settings for an item's rarity, including multipliers for various item properties.
/// </summary>
public sealed class ItemRarityConfig
{
    /// <summary>
    /// Gets the name of the rarity.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the color associated with the rarity.
    /// </summary>
    public required string Color { get; init; }

    /// <summary>
    /// Gets the numerical value representing the rarity.
    /// </summary>
    public required float Rarity { get; init; }

    /// <summary>
    /// Gets the multiplier applied to an item's durability based on its rarity.
    /// </summary>
    public float DurabilityMultiplier { get; init; } = 1F;

    /// <summary>
    /// Gets the multiplier applied to an item's mining speed based on its rarity.
    /// </summary>
    public float MiningSpeedMultiplier { get; init; } = 1F;

    /// <summary>
    /// Gets the multiplier applied to an item's attack power based on its rarity.
    /// </summary>
    public float AttackPowerMultiplier { get; init; } = 1F;

    /// <summary>
    /// Gets the multiplier applied to an item's piercing power based on its rarity.
    /// </summary>
    public float PiercingPowerMultiplier { get; init; } = 1F;

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