// ReSharper disable InconsistentNaming

using System.Linq;
using HarmonyLib;
using Vintagestory.API.Common;

namespace ItemRarity.Patches;

[HarmonyPatch(typeof(GridRecipe))]
public static class GridRecipePatch
{
    [HarmonyPrefix, HarmonyPatch("MatchesShapeLess"), HarmonyPriority(Priority.Last)]
    public static bool MatchesShapeLessPatch(GridRecipe __instance, ref bool __result, ItemSlot[] suppliedSlots, int gridWidth)
    {
        if (!ModCore.Config.Tier.EnableTiers || !__instance.Name.Path.Contains("tier-recipe-output"))
            return true;

        var num = suppliedSlots.Length / gridWidth;
        if (gridWidth < __instance.Width || num < __instance.Height)
            return false;

        // Filter to non-empty slots
        var nonEmptySlots = suppliedSlots.Where(s => s.Itemstack?.Collectible != null).ToArray();
        if (nonEmptySlots.Length != 2)
        {
            __result = false;
            return false;
        }
        
        var hasTierItem = nonEmptySlots.Any(s => s.Itemstack.Collectible.Code.Path.StartsWith("tier"));
        var hasTargetItem = nonEmptySlots.Any(s => RarityManager.IsSuitableFor(s.Itemstack));

        __result = hasTierItem && hasTargetItem;
        return false;
    }
}