using HarmonyLib;
using ItemRarity.Attributes;
using ItemRarity.Rarities;
using Vintagestory.API.Common;

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

    private static void GetMaxDurability(ItemStack itemStack, ref int __result)
    {
        if (!Rarity.TryGetRarity(itemStack, out _))
            return;

        __result = (int)(__result * Attribute.MaxDurabilityMultiplier.GetFloat(itemStack, 1f));
    }
}