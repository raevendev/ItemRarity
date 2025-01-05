using System.Text;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

[HarmonyPatch(typeof(ItemWearable))]
public static class ItemWearablePatch
{
    [HarmonyPostfix, HarmonyPatch(nameof(ItemWearable.GetHeldItemInfo)), HarmonyPriority(Priority.Last)]
    public static void GetHeldItemInfoPatch(ItemWearable __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        var itemstack = inSlot.Itemstack;

        if (!ModRarity.TryGetRarityTreeAttribute(itemstack, out var modAttribute) || !modAttribute.HasAttribute(ModAttributes.Rarity))
            return;

        CollectibleObjectPatch.FixItemInfos(itemstack, __instance, dsc, modAttribute);
    }
}