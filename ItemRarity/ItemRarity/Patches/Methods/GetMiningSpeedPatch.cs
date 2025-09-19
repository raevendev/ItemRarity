using HarmonyLib;
using ItemRarity.Rarities;
using Vintagestory.API.Common;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches.Methods;

[HarmonyPatch]
public static class GetMiningSpeedPatch
{
    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetMiningSpeed)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void CollectibleObject_GetMiningSpeedPatch(CollectibleObject __instance, IItemStack itemstack, BlockSelection blockSel, Block block, IPlayer forPlayer,
        ref float __result, ref ICoreAPI ___api)
    {
        if (!Rarity.TryGetRarity(itemstack as ItemStack, out _))
            return;

        __result *= AttributesManager.GetStatsMultiplier((itemstack as ItemStack)!, AttributesManager.MiningSpeedMultiplier);
    }
}