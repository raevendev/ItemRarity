using ItemRarity.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace ItemRarity.Stats.Modifiers;

public sealed class ShieldModifier : IStatsModifier
{
    public bool IsSuitable(ItemStack itemStack)
    {
        return itemStack.Collectible is ItemShield shield;
    }

    public void Apply(Rarity rarity, ItemStack itemStack, ITreeAttribute modAttributes)
    {
        var damageAbsorptionMul = rarity.ShieldProtectionMultiplier.Random;
        var projectileDamageAbsorptionMul = rarity.ShieldProtectionMultiplier.Random; // TODO: support projectile

        modAttributes.SetFloat(AttributesManager.ShieldDamageAbsorptionMultiplier, damageAbsorptionMul);
        modAttributes.SetFloat(AttributesManager.ArmorPerTierFlatDamageProtectionLossMultiplier, projectileDamageAbsorptionMul);
    }
}