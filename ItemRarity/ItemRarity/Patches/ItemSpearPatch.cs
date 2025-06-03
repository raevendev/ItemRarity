using System.Text;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

/// <summary>
/// A Harmony patch class for modifying the behavior of the <see cref="ItemSpear"/> class.
/// </summary>
[HarmonyPatch(typeof(ItemSpear))]
public static class ItemSpearPatch
{
    [HarmonyPrefix, HarmonyPatch(nameof(ItemSpear.GetHeldItemInfo)), HarmonyPriority(Priority.Last)]
    public static bool GetHeldItemInfoPatch(ItemSpear __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        if (!RarityManager.TryGetRarity(inSlot.Itemstack, out var rarity))
            return true;

        CollectibleObjectPatch.GetHeldItemInfoReversePatch(__instance, inSlot, dsc, world,
            withDebugInfo); // Call the original method to fill item infos before adding our own.

        var piercingDamages = 1.5f;

        if (inSlot.Itemstack.Collectible.Attributes != null)
            piercingDamages = inSlot.Itemstack.Collectible.Attributes["damage"].AsFloat();

        piercingDamages *= rarity.PiercingPowerMultiplier;

        dsc.AppendLine(piercingDamages + Lang.Get("piercing-damage-thrown"));

        return false;
    }
}