using System;
using System.Linq;
using ItemRarity.Extensions;
using ItemRarity.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace ItemRarity;

public static class RarityManager
{
    public const string GuidAttribute = "FE327889-1DD2-430C-BCCA-D94FB132E968"; // Ensure (almost) unique tree attribute key
    public const string RarityAttribute = "rarity";

    public static bool IsSuitableFor(ItemStack? itemStack, bool invalidIfRarityExists = true)
    {
        if (itemStack?.Attributes == null || itemStack.Collectible?.Attributes == null)
            return false;

        if (invalidIfRarityExists && itemStack.Attributes.HasAttribute(GuidAttribute))
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

        treeAttribute = itemStack.Attributes.GetTreeAttribute(ModAttributes.Guid);
        return treeAttribute != null;
    }

    public static bool TryGetRarity(ItemStack? itemStack, out Rarity rarityInfos)
    {
        if (itemStack is not { Attributes: not null, Collectible: not null })
        {
            rarityInfos = null!;
            return false;
        }

        var modAttribute = itemStack.Attributes.GetTreeAttribute(ModAttributes.Guid);

        if (modAttribute is null || !modAttribute.HasAttribute(ModAttributes.Rarity))
        {
            rarityInfos = null!;
            return false;
        }

        return ModCore.Config.Rarity.TryGetRarity(modAttribute.GetString(ModAttributes.Rarity), out rarityInfos);
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

        var modAttributes = itemStack.Attributes.GetOrAddTreeAttribute(ModAttributes.Guid);

        modAttributes.SetString(ModAttributes.Rarity, rarity.Key); // Use the key as the rarity.

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

        if (itemStack.Collectible.Durability > 0) // Set max durability
        {
            modAttributes.SetInt(ModAttributes.MaxDurability,
                (int)(itemStack.Collectible.GetMaxDurability(itemStack) * rarity.DurabilityMultiplier.Max)); // TODO: REPLACE
        }

        if (itemStack.Collectible.MiningSpeed != null && itemStack.Collectible.MiningSpeed.Count > 0) // Set mining speed
        {
            var miningSpeed = modAttributes.GetOrAddTreeAttribute(ModAttributes.MiningSpeed);
            foreach (var kv in itemStack.Collectible.MiningSpeed)
            {
                miningSpeed.SetFloat(kv.Key.ToString(), kv.Value * GlobalConstants.ToolMiningSpeedModifier * rarity.MiningSpeedMultiplier.Random);
            }
        }

        if (itemStack.Collectible.Attributes.KeyExists("damage")) // Set piercing damages
        {
            var damages = itemStack.Collectible.Attributes["damage"].AsFloat();
            modAttributes.SetFloat(ModAttributes.PiercingPower, damages * rarity.PiercingPowerMultiplier.Random);
        }

        if (itemStack.Collectible.AttackPower > 1f) // Set attack power
        {
            modAttributes.SetFloat(ModAttributes.AttackPower, itemStack.Collectible.AttackPower * rarity.AttackPowerMultiplier.Random);
        }

        if (itemStack.Collectible is ItemWearable { ProtectionModifiers: not null, IsArmor: true } wearable) // Set armor stats
        {
            var protectionModifier = modAttributes.GetOrAddTreeAttribute(ModAttributes.ProtectionModifiers);

            protectionModifier.SetFloat(ModAttributes.ArmorFlatDamageReduction,
                wearable.ProtectionModifiers.FlatDamageReduction * rarity.ArmorFlatDamageReductionMultiplier.Random);
            protectionModifier.SetFloat(ModAttributes.ArmorRelativeProtection,
                wearable.ProtectionModifiers.RelativeProtection); // TODO: Support relative protection

            var perTierRelativeProtectionLossAttribute = protectionModifier.GetOrAddTreeAttribute(ModAttributes.ArmorPerTierRelativeProtectionLoss);
            for (var i = 0; i < wearable.ProtectionModifiers.PerTierRelativeProtectionLoss.Length; i++)
            {
                perTierRelativeProtectionLossAttribute.SetFloat(i.ToString(),
                    wearable.ProtectionModifiers.PerTierRelativeProtectionLoss[i] / rarity.ArmorPerTierRelativeProtectionLossMultiplier.Random);
            }

            var perTierFlatDamageRedudctionLossAttribute = protectionModifier.GetOrAddTreeAttribute(ModAttributes.ArmorPerTierFlatDamageReductionLoss);
            for (var i = 0; i < wearable.ProtectionModifiers.PerTierFlatDamageReductionLoss.Length; i++)
            {
                perTierFlatDamageRedudctionLossAttribute.SetFloat(i.ToString(),
                    wearable.ProtectionModifiers.PerTierFlatDamageReductionLoss[i] / rarity.ArmorPerTierFlatDamageProtectionLossMultiplier.Random);
            }
        }

        if (itemStack.Collectible is ItemShield shield)
        {
            var itemAttribute = itemStack.ItemAttributes?["shield"];
            if (itemAttribute is { Exists: true })
            {
                modAttributes.SetFloat(ModAttributes.ShieldProjectileDamageAbsorption,
                    itemAttribute["projectileDamageAbsorption"].AsFloat() * rarity.ShieldProtectionMultiplier.Random);
                modAttributes.SetFloat(ModAttributes.ShieldDamageAbsorption,
                    itemAttribute["damageAbsorption"].AsFloat() * rarity.ShieldProtectionMultiplier.Random);
            }
        }
    }

    public static ProtectionModifiers GetRarityProtectionModifiers(ItemStack itemStack)
    {
        if (itemStack.Collectible is not ItemWearable wearable)
        {
            ModLogger.Warning("Mod is trying to get protection modifier for an unsupported item.");
            return new ProtectionModifiers { PerTierRelativeProtectionLoss = [], PerTierFlatDamageReductionLoss = [] };
        }

        if (TryGetRarityTreeAttribute(itemStack, out var treeAttribute) &&
            treeAttribute.HasAttribute(ModAttributes.ProtectionModifiers))
        {
            var protAttribute = treeAttribute.GetTreeAttribute(ModAttributes.ProtectionModifiers);
            var protModifiers = new ProtectionModifiers
            {
                FlatDamageReduction = protAttribute.GetFloat(ModAttributes.ArmorFlatDamageReduction)
            };
            //protModifiers.RelativeProtection = protAttribute.GetFloat(ModAttributes.ArmorRelativeProtection);

            var perTierRelativeProtectionLossAttribute = protAttribute.GetTreeAttribute(ModAttributes.ArmorPerTierRelativeProtectionLoss);
            protModifiers.PerTierRelativeProtectionLoss = new float[perTierRelativeProtectionLossAttribute.Count];
            for (var i = 0; i < perTierRelativeProtectionLossAttribute.Count; i++)
                protModifiers.PerTierRelativeProtectionLoss[i] = perTierRelativeProtectionLossAttribute.GetFloat(i.ToString());

            var perTierFlatDamageRedudctionLossAttribute = protAttribute.GetTreeAttribute(ModAttributes.ArmorPerTierFlatDamageReductionLoss);
            protModifiers.PerTierFlatDamageReductionLoss = new float[perTierFlatDamageRedudctionLossAttribute.Count];
            for (var i = 0; i < perTierFlatDamageRedudctionLossAttribute.Count; i++)
                protModifiers.PerTierFlatDamageReductionLoss[i] = perTierFlatDamageRedudctionLossAttribute.GetFloat(i.ToString());

            protModifiers.RelativeProtection = wearable.ProtectionModifiers.RelativeProtection;
            protModifiers.ProtectionTier = wearable.ProtectionModifiers.ProtectionTier;
            protModifiers.HighDamageTierResistant = wearable.ProtectionModifiers.HighDamageTierResistant;

            return protModifiers;
        }

        return wearable.ProtectionModifiers;
    }
}