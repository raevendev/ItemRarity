using HarmonyLib;
using ItemRarity.Attributes;
using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

[HarmonyPatch(typeof(ItemKnife))]
public static class ItemKnifePatch
{
    [HarmonyPatch(nameof(ItemKnife.GetKnifeHarvestingSpeed)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void GetKnifeHarvestingSpeed(ItemKnife __instance, ItemSlot slot, ref float __result)
    {
        if (!Rarity.TryGetRarity(slot.Itemstack, out _))
            return;

        __result = (float)(1.0 / (((__instance.GetMiningSpeeds(slot)[EnumBlockMaterial.Plant] * Attribute.MiningSpeedMultiplier.GetFloat(slot.Itemstack)) - 1.0) * 0.5 +
                                  1.0));
    }
}