using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

[HarmonyPatch]
public static class ItemShieldFromAttributesPatch
{
    [HarmonyPatch(typeof(ItemShieldFromAttributes), nameof(ItemShieldFromAttributes.GetHeldItemName)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void ItemShieldFromAttributes_GetHeldItemNamePatch(ItemShieldFromAttributes __instance, ItemStack itemStack, ref string __result)
    {
        PatchHelpers.GetHeldItemName(itemStack, ref __result);
    }
}