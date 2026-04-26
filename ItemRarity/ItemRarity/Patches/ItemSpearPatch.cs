using System.Text;
using HarmonyLib;
using ItemRarity.Attributes;
using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;
// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

[HarmonyPatch(typeof(ItemSpear))]
public static class ItemSpearPatch
{
    [HarmonyPatch(nameof(ItemSpear.GetHeldItemInfo)), HarmonyPrefix, HarmonyPriority(Priority.Last)]
    public static bool GetHeldItemInfoPatch(ItemSpear __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        if (!Rarity.TryGetRarity(inSlot.Itemstack, out _))
            return true;

        CollectibleObjectPatch.GetHeldItemInfoReversePatch(__instance, inSlot, dsc, world,
            withDebugInfo); // Call the original method to fill item infos before adding our own.

        var piercingDamages = 1.5f;

        if (inSlot.Itemstack!.Collectible.Attributes != null)
            piercingDamages = inSlot.Itemstack.Collectible.Attributes["damage"].AsFloat();

        piercingDamages *= Attribute.PiercingPowerMultiplier.GetFloat(inSlot.Itemstack, 1f);

        dsc.AppendLine(piercingDamages.ToString("0.#") + Lang.Get("piercing-damage-thrown"));

        return false;
    }
}