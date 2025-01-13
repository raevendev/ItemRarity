using System.Collections.Generic;
using System.Linq;

namespace ItemRarity.Config;

/// <summary>
/// Represents the configuration.
/// </summary>
public sealed class ModConfig
{
    public Dictionary<string, ItemRarityConfig> Rarities { get; init; } = new();
    
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
                        ArmorFlatDamageReductionMultiplier = 0.5F,
                        ArmorPerTierFlatDamageProtectionLossMultiplier = 0.5F,
                        ArmorRelativeProtectionMultiplier = 0.5F,
                        ArmorPerTierRelativeProtectionLossMultiplier = 0.5F,
                        ShieldProtectionMultiplier = 0.5F
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
                        ArmorFlatDamageReductionMultiplier = 1F,
                        ArmorPerTierFlatDamageProtectionLossMultiplier = 1F,
                        ArmorRelativeProtectionMultiplier = 1F,
                        ArmorPerTierRelativeProtectionLossMultiplier = 1F,
                        ShieldProtectionMultiplier = 1F
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
                        ArmorFlatDamageReductionMultiplier = 1.1F,
                        ArmorPerTierFlatDamageProtectionLossMultiplier = 1.1F,
                        ArmorRelativeProtectionMultiplier = 1.1F,
                        ArmorPerTierRelativeProtectionLossMultiplier = 1.1F,
                        ShieldProtectionMultiplier = 1.1F
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
                        ArmorFlatDamageReductionMultiplier = 1.2F,
                        ArmorPerTierFlatDamageProtectionLossMultiplier = 1.2F,
                        ArmorRelativeProtectionMultiplier = 1.2F,
                        ArmorPerTierRelativeProtectionLossMultiplier = 1.2F,
                        ShieldProtectionMultiplier = 1.2F
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
                        ArmorFlatDamageReductionMultiplier = 1.3F,
                        ArmorPerTierFlatDamageProtectionLossMultiplier = 1.3F,
                        ArmorRelativeProtectionMultiplier = 1.3F,
                        ArmorPerTierRelativeProtectionLossMultiplier = 1.3F,
                        ShieldProtectionMultiplier = 1.3F
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
                        ArmorFlatDamageReductionMultiplier = 1.5F,
                        ArmorPerTierFlatDamageProtectionLossMultiplier = 1.5F,
                        ArmorRelativeProtectionMultiplier = 1.5F,
                        ArmorPerTierRelativeProtectionLossMultiplier = 1.5F,
                        ShieldProtectionMultiplier = 1.6F
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
                        ArmorFlatDamageReductionMultiplier = 1.9F,
                        ArmorPerTierFlatDamageProtectionLossMultiplier = 1.9F,
                        ArmorRelativeProtectionMultiplier = 1.9F,
                        ArmorPerTierRelativeProtectionLossMultiplier = 1.9F,
                        ShieldProtectionMultiplier = 1.9F,
                        Effects = ["Thor"]
                    }
                }
            }
        };
    }
}