using System;
using System.Linq;
using ItemRarity.Models;
using ItemRarity.Stats;
using ItemRarity.Stats.Modifiers;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ItemRarity;

public static class RarityManager
{
    private static readonly IStatsModifier[] StatsModifiers =
    [
        new DurabilityModifier(), new MiningSpeedModifier(),
        new PiercingPowerModifier(), new AttackPowerModifier(),
        new ArmorFlatDamageReductionModifier(), new ShieldModifier()
    ];

    public const string RarityAttribute = "itemrarity";

    public static bool IsSuitableFor(ItemStack? itemStack, bool invalidIfRarityExists = true)
    {
        if (itemStack?.Attributes == null || itemStack.Collectible?.Attributes == null)
            return false;

        if (invalidIfRarityExists && itemStack.Attributes.HasAttribute(RarityAttribute))
            return false;

        var collectible = itemStack.Collectible;

        if (collectible.Durability > 1) // Support any item that has durability
            return true;

        return false;
    }

    public static bool TryGetRarityTreeAttribute(ItemStack? itemStack, out ITreeAttribute treeAttribute)
    {
        if (itemStack?.Attributes == null || itemStack.Collectible?.Attributes == null)
        {
            treeAttribute = null!;
            return false;
        }

        treeAttribute = itemStack.Attributes.GetTreeAttribute(AttributesManager.ModAttributeId);
        return treeAttribute != null;
    }

    public static bool TryGetRarity(ItemStack? itemStack, out Rarity rarityInfos)
    {
        if (itemStack is not { Attributes: not null, Collectible: not null })
        {
            rarityInfos = null!;
            return false;
        }

        var modAttribute = itemStack.Attributes.GetTreeAttribute(AttributesManager.ModAttributeId);

        if (modAttribute is null || !modAttribute.HasAttribute(AttributesManager.Rarity))
        {
            rarityInfos = null!;
            return false;
        }

        return ModCore.Config.Rarity.TryGetRarity(modAttribute.GetString(AttributesManager.Rarity), out rarityInfos);
    }

    public static Rarity? GetRandomRarity()
    {
        var totalWeight = ModCore.Config.Rarity.Rarities.Values.Sum(i => i.Weight);
        var randomValue = Random.Shared.NextDouble() * totalWeight;
        var cumulativeWeight = 0f;

        foreach (var item in ModCore.Config.Rarity.Rarities)
        {
            cumulativeWeight += item.Value.Weight;
            if (randomValue < cumulativeWeight)
                return item.Value;
        }

        return null;
    }

    public static Rarity? GetRandomRarityByTier(string tierKey)
    {
        var tier = ModCore.Config.Tier.Tiers[tierKey];
        var totalWeight = tier.Rarities.Sum(i => i.Value);
        var randomValue = Random.Shared.NextDouble() * totalWeight;
        var cumulativeWeight = 0f;

        foreach (var tierRarity in tier.Rarities)
        {
            cumulativeWeight += tierRarity.Value;
            if (randomValue < cumulativeWeight)
            {
                if (ModCore.Config.Rarity.TryGetRarity(tierRarity.Key, out var rarityConfig))
                    return rarityConfig;
                ModLogger.Warning($"No rarity defined or loaded for {tierRarity.Key}.");
                return null;
            }
        }

        return null;
    }

    public static Rarity? SetRarityByTier(ItemStack itemStack, string tier)
    {
        var rarity = GetRandomRarityByTier(tier);
        if (rarity is null)
            return null;
        SetRarity(itemStack, rarity);
        return rarity;
    }

    public static Rarity? SetRandomRarity(ItemStack itemStack)
    {
        var rarity = GetRandomRarity();
        if (rarity is null)
            return null;
        SetRarity(itemStack, rarity);
        return rarity;
    }

    public static void SetRarity(this ItemStack itemStack, Rarity rarity)
    {
        if (!IsSuitableFor(itemStack, false))
        {
            ModLogger.Warning($"Invalid item. Rarity is not supported for this item {itemStack.Collectible?.Code}");
            return;
        }

        var modAttributes = itemStack.Attributes.GetOrAddTreeAttribute(AttributesManager.ModAttributeId);

        modAttributes.SetString(AttributesManager.Rarity, rarity.Key); // Use the key as the rarity.

        if (rarity.CustomAttributes is { Count: > 0 })
        {
            foreach (var customAttribute in rarity.CustomAttributes)
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
                modifier.Apply(rarity, itemStack, modAttributes);
        }
    }
}