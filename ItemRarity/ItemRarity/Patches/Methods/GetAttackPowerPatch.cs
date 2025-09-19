using HarmonyLib;
using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches.Methods;

[HarmonyPatch]
public static class GetAttackPowerPatch
{
    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetAttackPower)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void CollectibleObject_GetAttackPowerPatch(CollectibleObject __instance, ItemStack withItemStack, ref float __result)
    {
        if (__instance is ItemWearable || !Rarity.TryGetRarity(withItemStack, out var rarity))
            return;

        __result *= AttributesManager.GetStatsMultiplier(withItemStack, AttributesManager.AttackPowerMultiplier);
    }
}