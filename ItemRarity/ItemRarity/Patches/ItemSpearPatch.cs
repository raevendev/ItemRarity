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
    /// <summary>
    /// Modifies the held item information for a spear by adding additional details based on its rarity attributes.
    /// </summary>
    /// <param name="__instance">
    /// The instance of the <c>ItemSpear</c> being modified.</param>
    /// <param name="inSlot">The <c>ItemSlot</c> containing the spear's item stack.</param>
    /// <param name="dsc">A <c>StringBuilder</c> that holds the item's description.</param>
    /// <param name="world">The <c>IWorldAccessor</c> instance representing the world context.</param>
    /// <param name="withDebugInfo">A <c>bool</c> indicating whether debug information should be included.</param>
    /// <returns>
    /// Returns <c>false</c> to prevent the original method from running after appending the custom info, or <c>true</c> if no changes 
    /// were made and the original method should execute.
    /// </returns>
    [HarmonyPrefix, HarmonyPatch(nameof(ItemSpear.GetHeldItemInfo)), HarmonyPriority(Priority.Last)]
    public static bool PatchGetHeldItemInfo(ItemSpear __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        var itemstack = inSlot.Itemstack;

        if (!itemstack.Attributes.HasAttribute(ModAttributes.Guid))
            return true;

        var modAttributes = itemstack.Attributes.GetTreeAttribute(ModAttributes.Guid);

        if (!modAttributes.HasAttribute(ModAttributes.Rarity) || !modAttributes.HasAttribute(ModAttributes.PiercingPower))
            return true;

        CollectibleObjectPatch.GetHeldItemInfoReversePatch(__instance, inSlot, dsc, world,
            withDebugInfo); // Call the original method to fill item infos before adding our own.

        var piercingDamages = modAttributes.GetFloat(ModAttributes.PiercingPower);

        dsc.AppendLine(piercingDamages + Lang.Get("piercing-damage-thrown"));

        return false;
    }
}