using HarmonyLib;
using ItemRarity.Attributes;
using ItemRarity.Rarities;
using Vintagestory.API.Common;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches.Methods;

[HarmonyPatch]
public static class GetAttackRangePatch
{
    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetAttackRange)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void CollectibleObject_GetAttackRangePatch(CollectibleObject __instance, ItemStack withItemStack, ref float __result)
    {
        if (!Rarity.TryGetRarity(withItemStack, out _))
            return;

        __result *= Attribute.AttackRangeMultiplier.GetFloat(withItemStack, 1f);
    }
}