using System.Collections.Generic;
using System.Linq;

namespace ItemRarity.Config;

/// <summary>
/// Represents the configuration.
/// </summary>
public sealed class ModConfig
{
    /// <summary>
    /// Gets the dictionary of item rarities, where the key is the rarity name and the value is the corresponding <see cref="ItemRarityConfig"/>.
    /// </summary>
    public Dictionary<string, ItemRarityConfig> Rarities { get; } = new()
    {
        {
            "cursed", new()
            {
                Name = "Cursed",
                Color = "#606060",
                Rarity = 8,
                DurabilityMultiplier = 0.5f,
                MiningSpeedMultiplier = 0.5f,
                AttackPowerMultiplier = 0.5f,
                PiercingPowerMultiplier = 0.5f
            }
        },

        {
            "common", new()
            {
                Name = "Common",
                Color = "#FFFFFF",
                Rarity = 40,
                DurabilityMultiplier = 1f,
                MiningSpeedMultiplier = 1f,
                AttackPowerMultiplier = 1f,
                PiercingPowerMultiplier = 1f
            }
        },

        {
            "uncommon", new()
            {
                Name = "Uncommon",
                Color = "#36FF00",
                Rarity = 30,
                DurabilityMultiplier = 1.1f,
                MiningSpeedMultiplier = 1.1f,
                AttackPowerMultiplier = 1.1f,
                PiercingPowerMultiplier = 1.1f
            }
        },

        {
            "rare", new()
            {
                Name = "Rare",
                Color = "#13DBE8",
                Rarity = 20,
                DurabilityMultiplier = 1.2f,
                MiningSpeedMultiplier = 1.2f,
                AttackPowerMultiplier = 1.2f,
                PiercingPowerMultiplier = 1.2f
            }
        },

        {
            "epic", new()
            {
                Name = "Epic",
                Color = "#8413E8",
                Rarity = 12,
                DurabilityMultiplier = 1.4f,
                MiningSpeedMultiplier = 1.3f,
                AttackPowerMultiplier = 1.3f,
                PiercingPowerMultiplier = 1.3f
            }
        },

        {
            "legendary", new()
            {
                Name = "Legendary",
                Color = "#E08614",
                Rarity = 8,
                DurabilityMultiplier = 1.6f,
                MiningSpeedMultiplier = 1.5f,
                AttackPowerMultiplier = 1.5f,
                PiercingPowerMultiplier = 1.5f
            }
        },

        {
            "unique", new()
            {
                Name = "Unique",
                Color = "#EC290E",
                Rarity = 2,
                DurabilityMultiplier = 2f,
                MiningSpeedMultiplier = 1.9f,
                AttackPowerMultiplier = 1.9f,
                PiercingPowerMultiplier = 1.9f,
                Effects = ["Thor"]
            }
        }
    };

    /// <summary>
    /// Indexer to retrieve a rarity configuration by its name.
    /// </summary>
    /// <param name="rarity">The name of the rarity to retrieve.</param>
    /// <returns>
    /// A tuple containing the rarity name and the corresponding <see cref="ItemRarityConfig"/>.
    /// </returns>
    public ItemRarityInfos this[string rarity]
    {
        get
        {
            if (Rarities.TryGetValue(rarity, out var result))
                return (rarity, result);

            var first = Rarities.First();
            return (first.Key, first.Value);
        }
    }
}