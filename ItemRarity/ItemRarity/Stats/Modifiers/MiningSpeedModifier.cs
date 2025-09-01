using ItemRarity.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ItemRarity.Stats.Modifiers;

public sealed class MiningSpeedModifier : IStatsModifier
{
    public bool IsSuitable(ItemStack itemStack)
    {
        return itemStack.Collectible.MiningSpeed is { Count: > 0 };
    }

    public void Apply(Rarity rarity, ItemStack itemStack, ITreeAttribute modAttributes)
    {
        var mul = rarity.MiningSpeedMultiplier.Random;
        
        // var miningSpeedTree = modAttributes.GetOrAddTreeAttribute(AttributesManager.MiningSpeed);
        //
        // foreach (var kv in itemStack.Collectible.MiningSpeed)
        // {
        //     miningSpeedTree.SetFloat(kv.Key.ToString(), kv.Value * GlobalConstants.ToolMiningSpeedModifier * mul);
        // }

        modAttributes.SetFloat(AttributesManager.MiningSpeedMultiplier, mul);
    }
}