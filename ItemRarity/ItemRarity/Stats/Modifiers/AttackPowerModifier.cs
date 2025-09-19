using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ItemRarity.Stats.Modifiers;

public sealed class AttackPowerModifier : IStatsModifier
{
    public bool IsSuitable(ItemStack itemStack)
    {
        return itemStack.Collectible.AttackPower > 1f;
    }

    public void Apply(RarityModel rarityModel, ItemStack itemStack, ITreeAttribute modAttributes)
    {
        var mul = rarityModel.AttackPowerMultiplier.Random;

        modAttributes.SetFloat(AttributesManager.AttackPowerMultiplier, mul);
    }
}