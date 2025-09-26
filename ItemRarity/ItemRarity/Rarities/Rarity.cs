using System;
using System.Linq;
using ItemRarity.Config;
using ItemRarity.Logging;
using ItemRarity.Stats;
using ItemRarity.Stats.Modifiers;
using Vintagestory.API.Common;
using Attribute = ItemRarity.Attributes.Attribute;

namespace ItemRarity.Rarities;

public static class Rarity
{
    private static readonly IStatsModifier[] StatsModifiers =
    [
        new DurabilityModifier(), new MiningSpeedModifier(),
        new PiercingPowerModifier(), new AttackPowerModifier(),
        new ArmorFlatDamageReductionModifier(), new ShieldModifier()
    ];

    private static RaritiesConfig Config => ModCore.Config.Rarity;

    public static bool IsSuitableFor(ItemStack? itemStack, bool invalidIfRarityExists = true)
    {
        if (itemStack?.Attributes == null || itemStack.Collectible?.Attributes == null)
            return false;

        if (invalidIfRarityExists && itemStack.Attributes.HasAttribute(Attribute.ModAttributeId))
            return false;

        var collectible = itemStack.Collectible;

        if (collectible.Durability > 1) // Support any item that has durability
            return true;

        return false;
    }

    public static RarityModel GetRandomRarity()
    {
        var totalWeight = Config.Rarities.Values.Sum(i => i.Weight);
        var randomValue = Random.Shared.NextDouble() * totalWeight;
        var cumulativeWeight = 0f;

        foreach (var item in Config.Rarities)
        {
            cumulativeWeight += item.Value.Weight;
            if (randomValue < cumulativeWeight)
                return item.Value;
        }

        Logger.Error("Failed to get random rarity");

        return Config.Rarities.First().Value;
    }

    public static bool TryGetRarity(ItemStack? itemStack, out RarityModel rarityModel)
    {
        if (itemStack is not { Attributes: not null, Collectible: not null })
        {
            rarityModel = null!;
            return false;
        }

        var modAttribute = itemStack.Attributes.GetTreeAttribute(Attribute.ModAttributeId);

        if (modAttribute is null || !modAttribute.HasAttribute(Attribute.Rarity))
        {
            rarityModel = null!;
            return false;
        }

        var rarityKey = modAttribute.GetString(Attribute.Rarity);

        return Config.TryGetRarity(rarityKey, out rarityModel);
    }

    public static void ApplyRarity(ItemStack itemStack, RarityModel? rarityModel = null)
    {
        if (!IsSuitableFor(itemStack, false))
        {
            Logger.Warning($"Invalid item. Rarity is not supported for this item {itemStack.Collectible?.Code}");
            return;
        }

        rarityModel ??= GetRandomRarity();

        var modAttributes = itemStack.Attributes.GetOrAddTreeAttribute(Attribute.ModAttributeId);

        modAttributes.SetString(Attribute.Rarity, rarityModel.Key); // Use the key as the rarity.

        if (rarityModel.CustomAttributes is { Count: > 0 })
        {
            foreach (var customAttribute in rarityModel.CustomAttributes)
            {
                if (customAttribute.Key[0] == '$') // If $ is the first char, we want to save the attribute within the mod's attribute section.
                    modAttributes.SetFloat(customAttribute.Key[1..], customAttribute.Value);
                else
                    itemStack.Attributes.SetFloat(customAttribute.Key, customAttribute.Value);
            }
        }

        foreach (var modifier in StatsModifiers)
        {
            if (modifier.IsSuitable(itemStack))
                modifier.Apply(rarityModel, itemStack, modAttributes);
        }
    }
}