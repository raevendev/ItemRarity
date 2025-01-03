using System;
using System.Linq;
using ItemRarity.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace ItemRarity;

public static class ModRarity
{
    public static bool IsValidForRarity(ItemStack? itemStack, bool invalidIfRarityExists = true)
    {
        if (itemStack == null || itemStack.Attributes == null || itemStack.Collectible == null)
            return false;

        if (invalidIfRarityExists && itemStack.Attributes.HasAttribute(ModAttributes.Guid))
            return false;

        var collectible = itemStack.Collectible;

        if (collectible.Durability > 0) // Support any item that has durabiltiy
            return true;

        return false;
    }

    /// <summary>
    /// Returns a random rarity based on the configured rarities and their associated weights.
    /// </summary>
    /// <returns>
    /// A tuple containing the key (rarity name) and value (<see cref="ItemRarityConfig"/>) of the randomly selected rarity.
    /// </returns>
    public static ItemRarityInfos GetRandomRarity()
    {
        var totalWeight = ModCore.Config.Rarities.Values.Sum(i => i.Rarity);
        var randomValue = Random.Shared.NextDouble() * totalWeight;
        var cumulativeWeight = 0f;

        foreach (var item in ModCore.Config.Rarities)
        {
            cumulativeWeight += item.Value.Rarity;
            if (randomValue < cumulativeWeight)
                return (item.Key, item.Value);
        }

        var first = ModCore.Config.Rarities.First();
        return (first.Key, first.Value);
    }

    public static ItemRarityInfos SetRandomRarity(ItemStack itemStack)
    {
        var rarity = GetRandomRarity();
        return SetRarity(itemStack, rarity.Key);
    }

    public static ItemRarityInfos SetRarity(this ItemStack itemStack, string rarity)
    {
        if (!IsValidForRarity(itemStack))
            throw new Exception("Invalid item. Rarity is not supported for this item");

        var itemRarity = ModCore.Config[rarity];
        var modAttributes = itemStack.Attributes.GetOrAddTreeAttribute(ModAttributes.Guid);

        modAttributes.SetString(ModAttributes.Rarity, itemRarity.Key); // Use the key as the rarity.

        if (itemStack.Collectible.Durability > 0) // Set max durability
        {
            modAttributes.SetInt(ModAttributes.MaxDurability, (int)(itemStack.Collectible.Durability * itemRarity.Value.DurabilityMultiplier));
        }

        if (itemStack.Collectible.MiningSpeed != null && itemStack.Collectible.MiningSpeed.Count > 0) // Set mining speed
        {
            var miningSpeed = modAttributes.GetOrAddTreeAttribute(ModAttributes.MiningSpeed);
            foreach (var kv in itemStack.Collectible.MiningSpeed)
            {
                miningSpeed.SetFloat(kv.Key.ToString(), kv.Value * GlobalConstants.ToolMiningSpeedModifier * itemRarity.Value.MiningSpeedMultiplier);
            }
        }

        if (itemStack.Collectible.Attributes.KeyExists("damage")) // Set piercing damages
        {
            var damages = itemStack.Collectible.Attributes["damage"].AsFloat();
            modAttributes.SetFloat(ModAttributes.PiercingPower, damages * itemRarity.Value.PiercingPowerMultiplier);
        }

        if (itemStack.Collectible.AttackPower > 1f) // Set attack power
        {
            modAttributes.SetFloat(ModAttributes.AttackPower, itemStack.Collectible.AttackPower * itemRarity.Value.AttackPowerMultiplier);
        }

        if (itemStack.Collectible is ItemWearable wearable && wearable.IsArmor)
        {
            var protectionModifier = itemStack.Attributes.GetTreeAttribute("protectionModifiers");
            if (protectionModifier == null)
            {
                ModCore.ServerApi?.Logger.Warning("Mod is trying to set protection modifier for item but the item is missing attributes.");
                return itemRarity;
            }

            var flatDamageProtection = protectionModifier.GetFloat("flatDamageReduction");
            protectionModifier.SetFloat("flatDamageReduction", flatDamageProtection * itemRarity.Value.FlatDamageReductionMultiplier);
        }

        return itemRarity;
    }
}