using ItemRarity.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ItemRarity.Stats.Modifiers;

public sealed class PiercingPowerModifier : IStatsModifier
{
    public bool IsSuitable(ItemStack itemStack)
    {
        return itemStack.Collectible.Attributes.KeyExists("damage");
    }

    public void Apply(Rarity rarity, ItemStack itemStack, ITreeAttribute modAttributes)
    {
        var mul = rarity.PiercingPowerMultiplier.Random;

        modAttributes.SetFloat(AttributesManager.PiercingPowerMultiplier, mul);
    }
}