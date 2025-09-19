using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ItemRarity.Stats.Modifiers;

public sealed class MiningSpeedModifier : IStatsModifier
{
    public bool IsSuitable(ItemStack itemStack)
    {
        return itemStack.Collectible.MiningSpeed is { Count: > 0 };
    }

    public void Apply(RarityModel rarityModel, ItemStack itemStack, ITreeAttribute modAttributes)
    {
        var mul = rarityModel.MiningSpeedMultiplier.Random;

        modAttributes.SetFloat(AttributesManager.MiningSpeedMultiplier, mul);
    }
}