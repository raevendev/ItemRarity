using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ItemRarity.Stats.Modifiers;

public sealed class DurabilityModifier : IStatsModifier
{
    public bool IsSuitable(ItemStack itemStack)
    {
        return itemStack.Collectible.Durability > 0;
    }

    public void Apply(RarityModel rarityModel, ItemStack itemStack, ITreeAttribute modAttributes)
    {
        var mul = rarityModel.DurabilityMultiplier.Random;

        modAttributes.SetFloat(AttributesManager.MaxDurabilityMultiplier, mul);
    }
}