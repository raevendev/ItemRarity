using ItemRarity.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ItemRarity.Stats;

public interface IStatsModifier
{
    bool IsSuitable(ItemStack itemStack);

    void Apply(Rarity rarity, ItemStack itemStack, ITreeAttribute modAttributes);
}