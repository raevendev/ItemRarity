using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches.Methods;

[HarmonyPatch]
public static class GetHeldItemNamePatch
{
    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetHeldItemName)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void CollectibleObject_GetHeldItemNamePatch(CollectibleObject __instance, ItemStack itemStack, ref string __result)
    {
        GetHeldItemName(itemStack, ref __result);
    }

    [HarmonyPatch(typeof(ItemShield), nameof(ItemShield.GetHeldItemName)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void ItemShield_GetHeldItemNamePatch(CollectibleObject __instance, ItemStack itemStack, ref string __result)
    {
        GetHeldItemName(itemStack, ref __result);
    }

    private static void GetHeldItemName(ItemStack itemStack, ref string __result)
    {
        if (!RarityManager.TryGetRarity(itemStack, out var rarity))
            return;

        var rarityName = rarity.IgnoreTranslation
            ? $"[{rarity.Name}]"
            : Lang.GetWithFallback($"itemrarity:{rarity.Key}", "itemrarity:unknown", rarity.Name);

        if (__result.Contains(rarityName))
            return;

        __result = $"<font color=\"{rarity.Color}\" weight=bold>{rarityName} {__result}</font>";
    }
}