using ItemRarity.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ItemRarity.Stats.Modifiers;

public sealed class DurabilityModifier : IStatsModifier
{
    public bool IsSuitable(ItemStack itemStack)
    {
        return itemStack.Collectible.Durability > 0;
    }

    public void Apply(Rarity rarity, ItemStack itemStack, ITreeAttribute modAttributes)
    {
        var mul = rarity.DurabilityMultiplier.Random;

        modAttributes.SetFloat(AttributesManager.MaxDurabilityMultiplier, mul);
    }
}