using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ItemRarity.Stats.Modifiers;

public sealed class PiercingPowerModifier : IStatsModifier
{
    public bool IsSuitable(ItemStack itemStack)
    {
        return itemStack.Collectible.Attributes.KeyExists("damage");
    }

    public void Apply(RarityModel rarityModel, ItemStack itemStack, ITreeAttribute modAttributes)
    {
        var mul = rarityModel.PiercingPowerMultiplier.Random;

        modAttributes.SetFloat(AttributesManager.PiercingPowerMultiplier, mul);
    }
}