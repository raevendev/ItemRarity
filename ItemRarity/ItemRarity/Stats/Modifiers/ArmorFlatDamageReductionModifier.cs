using ItemRarity.Rarities;
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

    public void Apply(RarityModel rarityModel, ItemStack itemStack, ITreeAttribute modAttributes)
    {
        var flatRedMul = rarityModel.ArmorFlatDamageReductionMultiplier.Random;
        var perTierFlatProtMul = rarityModel.ArmorPerTierFlatDamageProtectionLossMultiplier.Random;

        modAttributes.SetFloat(AttributesManager.ArmorFlatDamageReductionMultiplier, flatRedMul);
        modAttributes.SetFloat(AttributesManager.ArmorPerTierFlatDamageProtectionLossMultiplier, perTierFlatProtMul);
    }
}