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
    public Dictionary<string, ItemRarityConfig> Rarities { get; init; } = new();

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

    public static ModConfig GetDefaultConfig()
    {
        return new()
        {
            Rarities = new()
            {
                {
                    "cursed", new()
                    {
                        Name = "Cursed",
                        Color = "#606060",
                        Rarity = 8,
                        DurabilityMultiplier = 0.5F,
                        MiningSpeedMultiplier = 0.5F,
                        AttackPowerMultiplier = 0.5F,
                        PiercingPowerMultiplier = 0.5F,
                        FlatDamageReductionMultiplier = 0.5F,
                        RelativeProtectionMultiplier = 0.5F
                    }
                },

                {
                    "common", new()
                    {
                        Name = "Common",
                        Color = "#FFFFFF",
                        Rarity = 40,
                        DurabilityMultiplier = 1F,
                        MiningSpeedMultiplier = 1F,
                        AttackPowerMultiplier = 1F,
                        PiercingPowerMultiplier = 1F,
                        FlatDamageReductionMultiplier = 1F,
                        RelativeProtectionMultiplier = 1F
                    }
                },

                {
                    "uncommon", new()
                    {
                        Name = "Uncommon",
                        Color = "#36FF00",
                        Rarity = 30,
                        DurabilityMultiplier = 1.1F,
                        MiningSpeedMultiplier = 1.1F,
                        AttackPowerMultiplier = 1.1F,
                        PiercingPowerMultiplier = 1.1F,
                        FlatDamageReductionMultiplier = 1.1F,
                        RelativeProtectionMultiplier = 1.1F
                    }
                },

                {
                    "rare", new()
                    {
                        Name = "Rare",
                        Color = "#13DBE8",
                        Rarity = 20,
                        DurabilityMultiplier = 1.2F,
                        MiningSpeedMultiplier = 1.2F,
                        AttackPowerMultiplier = 1.2F,
                        PiercingPowerMultiplier = 1.2F,
                        FlatDamageReductionMultiplier = 1.2F,
                        RelativeProtectionMultiplier = 1.2F
                    }
                },

                {
                    "epic", new()
                    {
                        Name = "Epic",
                        Color = "#8413E8",
                        Rarity = 12,
                        DurabilityMultiplier = 1.4F,
                        MiningSpeedMultiplier = 1.3F,
                        AttackPowerMultiplier = 1.3F,
                        PiercingPowerMultiplier = 1.3F,
                        FlatDamageReductionMultiplier = 1.3F,
                        RelativeProtectionMultiplier = 1.3F
                    }
                },

                {
                    "legendary", new()
                    {
                        Name = "Legendary",
                        Color = "#E08614",
                        Rarity = 8,
                        DurabilityMultiplier = 1.6F,
                        MiningSpeedMultiplier = 1.5F,
                        AttackPowerMultiplier = 1.5F,
                        PiercingPowerMultiplier = 1.5F,
                        FlatDamageReductionMultiplier = 1.5F,
                        RelativeProtectionMultiplier = 1.5F
                    }
                },

                {
                    "unique", new()
                    {
                        Name = "Unique",
                        Color = "#EC290E",
                        Rarity = 2,
                        DurabilityMultiplier = 2F,
                        MiningSpeedMultiplier = 1.9F,
                        AttackPowerMultiplier = 1.9F,
                        PiercingPowerMultiplier = 1.9F,
                        FlatDamageReductionMultiplier = 1.9F,
                        RelativeProtectionMultiplier = 1.9F,
                        Effects = ["Thor"]
                    }
                }
            }
        };
    }
}