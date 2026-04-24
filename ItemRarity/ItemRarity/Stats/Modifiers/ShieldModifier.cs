using ItemRarity.Attributes;
using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace ItemRarity.Stats.Modifiers;

public sealed class ShieldModifier : IStatsModifier
{
    public bool IsSuitable(ItemStack itemStack)
    {
        return itemStack.Collectible is ItemShield or ItemShieldFromAttributes;
    }

    public void Apply(RarityModel rarityModel, ItemStack itemStack, ITreeAttribute modAttributes)
    {
        var damageAbsorptionMul = rarityModel.ShieldProtectionMultiplier.Random;
        var projectileDamageAbsorptionMul = rarityModel.ShieldProtectionMultiplier.Random;

        Attribute.ShieldDamageAbsorptionMultiplier.SetFloat(modAttributes, damageAbsorptionMul);
        Attribute.ArmorPerTierFlatDamageProtectionLossMultiplier.SetFloat(modAttributes, projectileDamageAbsorptionMul);
    }
}