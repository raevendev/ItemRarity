using System;
using System.Linq;
using ItemRarity.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace ItemRarity;

public static class Rarity
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

        if (collectible.Durability > 0) // Support any item that has durability
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

    public static bool TryGetRarityInfos(ItemStack? itemStack, out ItemRarityInfos rarityInfos)
    {
        if (itemStack is not { Attributes: not null, Collectible: not null })
        {
            rarityInfos = default;
            return false;
        }

        var modAttribute = itemStack.Attributes.GetTreeAttribute(ModAttributes.Guid);

        if (modAttribute is null || !modAttribute.HasAttribute(ModAttributes.Rarity))
        {
            rarityInfos = default;
            return false;
        }

        rarityInfos = ModCore.Config[modAttribute.GetString(ModAttributes.Rarity)];
        return true;
    }

    /// <summary>
    /// Returns a random rarity based on the configured rarities and their associated weights.
    /// </summary>
    /// <returns>
    /// A tuple containing the key (rarity name) and value (<see cref="ItemRarityConfig"/>) of the randomly selected rarity.
    /// </returns>
    public static ItemRarityInfos GetRandomRarity()
    {
        if (ModCore.Config is { Rarities: null } || ModCore.Config.Rarities.Count == 0)
        {
            ModCore.LogWarning("No rarities defined or loaded in ModCore.Config.Rarities. Returning default rarity.");
            return (string.Empty, null!);
        }

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
        if (!IsSuitableFor(itemStack, false))
        {
            ModCore.LogWarning("Invalid item. Rarity is not supported for this item");
            return ModCore.Config[string.Empty];
        }

        var itemRarity = ModCore.Config[rarity];

        if (itemRarity.Value == null)
        {
            ModCore.LogError($"Invalid or null rarity value for rarity key '{rarity}'. Cannot apply rarity bonuses to item {itemStack.GetName()}.");
            return itemRarity;
        }

        var modAttributes = itemStack.Attributes.GetOrAddTreeAttribute(ModAttributes.Guid);

        modAttributes.SetString(ModAttributes.Rarity, itemRarity.Key); // Use the key as the rarity.

        if (itemStack.Collectible.Durability > 0) // Set max durability
        {
            modAttributes.SetInt(ModAttributes.MaxDurability, (int)(itemStack.Collectible.GetMaxDurability(itemStack) * itemRarity.Value.DurabilityMultiplier));
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

        if (itemStack.Collectible is ItemWearable { ProtectionModifiers: not null } wearable && wearable.IsArmor) // Set armor stats
        {
            var protectionModifier = modAttributes.GetOrAddTreeAttribute(ModAttributes.ProtectionModifiers);

            protectionModifier.SetFloat(ModAttributes.ArmorFlatDamageReduction,
                wearable.ProtectionModifiers.FlatDamageReduction * itemRarity.Value.ArmorFlatDamageReductionMultiplier);
            protectionModifier.SetFloat(ModAttributes.ArmorRelativeProtection,
                wearable.ProtectionModifiers.RelativeProtection); // TODO: Support relative protection

            var perTierRelativeProtectionLossAttribute = protectionModifier.GetOrAddTreeAttribute(ModAttributes.ArmorPerTierRelativeProtectionLoss);
            for (var i = 0; i < wearable.ProtectionModifiers.PerTierRelativeProtectionLoss.Length; i++)
            {
                perTierRelativeProtectionLossAttribute.SetFloat(i.ToString(),
                    wearable.ProtectionModifiers.PerTierRelativeProtectionLoss[i] / itemRarity.Value.ArmorPerTierRelativeProtectionLossMultiplier);
            }

            var perTierFlatDamageRedudctionLossAttribute = protectionModifier.GetOrAddTreeAttribute(ModAttributes.ArmorPerTierFlatDamageReductionLoss);
            for (var i = 0; i < wearable.ProtectionModifiers.PerTierFlatDamageReductionLoss.Length; i++)
            {
                perTierFlatDamageRedudctionLossAttribute.SetFloat(i.ToString(),
                    wearable.ProtectionModifiers.PerTierFlatDamageReductionLoss[i] / itemRarity.Value.ArmorPerTierFlatDamageProtectionLossMultiplier);
            }
        }

        if (itemStack.Collectible is ItemShield shield)
        {
            var itemAttribute = itemStack.ItemAttributes?["shield"];
            if (itemAttribute != null && itemAttribute.Exists)
            {
                modAttributes.SetFloat(ModAttributes.ShieldProjectileDamageAbsorption,
                    itemAttribute["projectileDamageAbsorption"].AsFloat() * itemRarity.Value.ShieldProtectionMultiplier);
                modAttributes.SetFloat(ModAttributes.ShieldDamageAbsorption,
                    itemAttribute["damageAbsorption"].AsFloat() * itemRarity.Value.ShieldProtectionMultiplier);
            }
        }


        return itemRarity;
    }

    public static ProtectionModifiers GetRarityProtectionModifiers(ItemStack itemStack)
    {
        if (itemStack.Collectible is not ItemWearable wearable)
        {
            ModCore.LogWarning("Mod is trying to get protection modifier for an unsupported item.");
            return new ProtectionModifiers { PerTierRelativeProtectionLoss = [], PerTierFlatDamageReductionLoss = [] };
        }

        if (TryGetRarityTreeAttribute(itemStack, out var treeAttribute) &&
            treeAttribute.HasAttribute(ModAttributes.ProtectionModifiers))
        {
            var protAttribute = treeAttribute.GetTreeAttribute(ModAttributes.ProtectionModifiers);
            var protModifiers = new ProtectionModifiers();
            protModifiers.FlatDamageReduction = protAttribute.GetFloat(ModAttributes.ArmorFlatDamageReduction);
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