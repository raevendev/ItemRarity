using ItemRarity.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace ItemRarity.Stats.Modifiers;

public sealed class ArmorFlatDamageReductionModifier : IStatsModifier
{
    public bool IsSuitable(ItemStack itemStack)
    {
        return itemStack.Collectible is ItemWearable { ProtectionModifiers: not null, IsArmor: true };
    }

    public void Apply(Rarity rarity, ItemStack itemStack, ITreeAttribute modAttributes)
    {
        var flatRedMul = rarity.ArmorFlatDamageReductionMultiplier.Random;
        var perTierFlatProtMul = rarity.ArmorPerTierFlatDamageProtectionLossMultiplier.Random;

        modAttributes.SetFloat(AttributesManager.ArmorFlatDamageReductionMultiplier, flatRedMul);
        modAttributes.SetFloat(AttributesManager.ArmorPerTierFlatDamageProtectionLossMultiplier, perTierFlatProtMul);
    }
}