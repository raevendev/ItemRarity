using ItemRarity.Attributes;
using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ItemRarity.Stats.Modifiers;

public sealed class AttackRangeModifier : IStatsModifier
{
    public bool IsSuitable(ItemStack itemStack)
    {
        return itemStack.Collectible.AttackRange >= 1f;
    }

    public void Apply(RarityModel rarityModel, ItemStack itemStack, ITreeAttribute modAttributes)
    {
        var mul = rarityModel.AttackRangeMultiplier.Random;

        Attribute.AttackRangeMultiplier.SetFloat(modAttributes, mul);
    }
}