using HarmonyLib;
using ItemRarity.Attributes;
using ItemRarity.Rarities;
using Vintagestory.API.Common;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches.Methods;

[HarmonyPatch]
public static class GetAttackPowerPatch
{
    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetAttackPower)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void CollectibleObject_GetAttackPowerPatch(CollectibleObject __instance, ItemStack itemStack, ref float __result)
    {
        if (!Rarity.TryGetRarity(itemStack, out _))
            return;

        __result *= Attribute.AttackPowerMultiplier.GetFloat(itemStack, 1f);
    }
}