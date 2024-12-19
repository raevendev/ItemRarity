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
    public required float DurabilityMultiplier { get; init; }

    /// <summary>
    /// Gets the multiplier applied to an item's mining speed based on its rarity.
    /// </summary>
    public required float MiningSpeedMultiplier { get; init; }

    /// <summary>
    /// Gets the multiplier applied to an item's attack power based on its rarity.
    /// </summary>
    public required float AttackPowerMultiplier { get; init; }

    /// <summary>
    /// Gets the multiplier applied to an item's piercing power based on its rarity.
    /// </summary>
    public required float PiercingPowerMultiplier { get; init; }
}