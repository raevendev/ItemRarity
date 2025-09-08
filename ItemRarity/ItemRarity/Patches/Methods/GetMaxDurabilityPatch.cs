using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches.Methods;

[HarmonyPatch]
public static class GetMaxDurabilityPatch
{
    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetMaxDurability)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void CollectibleObject_GetMaxDurabilityPatch(CollectibleObject __instance, ItemStack itemstack, ref int __result)
    {
        GetMaxDurability(itemstack, ref __result);
    }

    [HarmonyPatch(typeof(ItemShield), nameof(ItemShield.GetMaxDurability)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void ItemShield_GetMaxDurabilityPatch(ItemShield __instance, ItemStack itemstack, ref int __result)
    {
        GetMaxDurability(itemstack, ref __result);
    }

    private static void GetMaxDurability(ItemStack itemStack, ref int __result)
    {
        if (!RarityManager.TryGetRarity(itemStack, out var rarityInfos))
            return;

        __result = (int)(__result * AttributesManager.GetStatsMultiplier(itemStack, AttributesManager.MaxDurabilityMultiplier));
    }
}