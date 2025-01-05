﻿using System;
using System.Linq;
using ItemRarity.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
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

    public static bool TryGetRarityTreeAttribute(ItemStack? itemStack, out ITreeAttribute treeAttribute)
    {
        if (itemStack == null || itemStack.Attributes == null || itemStack.Collectible == null)
        {
            treeAttribute = null!;
            return false;
        }

        treeAttribute = itemStack.Attributes.GetTreeAttribute(ModAttributes.Guid);
        return treeAttribute != null;
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
        if (!IsValidForRarity(itemStack, false))
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

        if (itemStack.Collectible is ItemWearable wearable && wearable.IsArmor) // Set armor stats
        {
            var protectionModifier = modAttributes.GetOrAddTreeAttribute(ModAttributes.ProtectionModifiers);
            if (protectionModifier == null)
            {
                ModCore.ServerApi?.Logger.Warning("Mod is trying to set protection modifier for item but the item is missing attributes.");
                return itemRarity;
            }

            protectionModifier.SetFloat(ModAttributes.FlatDamageReduction,
                wearable.ProtectionModifiers.FlatDamageReduction * itemRarity.Value.FlatDamageReductionMultiplier);
            protectionModifier.SetFloat(ModAttributes.RelativeProtection,
                wearable.ProtectionModifiers.RelativeProtection * itemRarity.Value.RelativeProtectionMultiplier);

            var perTierRelativeProtectionLossAttribute = protectionModifier.GetOrAddTreeAttribute(ModAttributes.PerTierRelativeProtectionLoss);
            for (var i = 0; i < wearable.ProtectionModifiers.PerTierRelativeProtectionLoss.Length; i++)
            {
                perTierRelativeProtectionLossAttribute.SetFloat(i.ToString(),
                    wearable.ProtectionModifiers.PerTierRelativeProtectionLoss[i] * itemRarity.Value.RelativeProtectionMultiplier);
            }

            var perTierFlatDamageRedudctionLossAttribute = protectionModifier.GetOrAddTreeAttribute(ModAttributes.PerTierFlatDamageReductionLoss);
            for (var i = 0; i < wearable.ProtectionModifiers.PerTierFlatDamageReductionLoss.Length; i++)
            {
                perTierFlatDamageRedudctionLossAttribute.SetFloat(i.ToString(),
                    wearable.ProtectionModifiers.PerTierFlatDamageReductionLoss[i] * itemRarity.Value.FlatDamageReductionMultiplier);
            }
        }

        return itemRarity;
    }

    public static ProtectionModifiers GetRarityProtectionModifiers(ItemStack itemStack)
    {
        if (itemStack.Collectible is not ItemWearable wearable)
            throw new Exception("Mod is trying to get protection modifier for an unsupported item.");

        if (TryGetRarityTreeAttribute(itemStack, out var treeAttribute) &&
            treeAttribute.HasAttribute(ModAttributes.ProtectionModifiers))
        {
            var protAttribute = treeAttribute.GetTreeAttribute(ModAttributes.ProtectionModifiers);
            var protModifiers = new ProtectionModifiers();
            protModifiers.FlatDamageReduction = protAttribute.GetFloat(ModAttributes.FlatDamageReduction);
            protModifiers.RelativeProtection = protAttribute.GetFloat(ModAttributes.RelativeProtection);

            var perTierRelativeProtectionLossAttribute = protAttribute.GetTreeAttribute(ModAttributes.PerTierRelativeProtectionLoss);
            protModifiers.PerTierRelativeProtectionLoss = new float[perTierRelativeProtectionLossAttribute.Count];
            for (var i = 0; i < perTierRelativeProtectionLossAttribute.Count; i++)
                protModifiers.PerTierRelativeProtectionLoss[i] = perTierRelativeProtectionLossAttribute.GetFloat(i.ToString());

            var perTierFlatDamageRedudctionLossAttribute = protAttribute.GetTreeAttribute(ModAttributes.PerTierFlatDamageReductionLoss);
            protModifiers.PerTierFlatDamageReductionLoss = new float[perTierFlatDamageRedudctionLossAttribute.Count];
            for (var i = 0; i < perTierFlatDamageRedudctionLossAttribute.Count; i++)
                protModifiers.PerTierFlatDamageReductionLoss[i] = perTierFlatDamageRedudctionLossAttribute.GetFloat(i.ToString());

            protModifiers.ProtectionTier = wearable.ProtectionModifiers.ProtectionTier;
            protModifiers.HighDamageTierResistant = wearable.ProtectionModifiers.HighDamageTierResistant;

            return protModifiers;
        }

        return wearable.ProtectionModifiers;
    }
}