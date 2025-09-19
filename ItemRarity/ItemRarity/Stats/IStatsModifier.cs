using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ItemRarity.Stats;

public interface IStatsModifier
{
    bool IsSuitable(ItemStack itemStack);

    void Apply(RarityModel rarityModel, ItemStack itemStack, ITreeAttribute modAttributes);
}