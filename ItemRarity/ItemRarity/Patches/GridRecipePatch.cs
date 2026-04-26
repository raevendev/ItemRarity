// ReSharper disable InconsistentNaming

using System.Linq;
using HarmonyLib;
using ItemRarity.Rarities;
using Vintagestory.API.Common;

namespace ItemRarity.Patches;

[HarmonyPatch(typeof(RecipeBase))]
public static class GridRecipePatch
{
    [HarmonyPrefix, HarmonyPatch("MatchesShapeLess"), HarmonyPriority(Priority.Last)]
    public static bool MatchesShapeLessPatch(RecipeBase __instance, ref bool __result, ItemSlot[] suppliedSlots, IWorldAccessor world,
        IRecipeIngredient?[] ingredients)
    {
        if (!ModCore.Config.Tier.EnableTiers || __instance.Name?.Path.Contains("tier-recipe-output") == false)
            return true;
        
        // Filter to non-empty slots
        var nonEmptySlots = suppliedSlots.Where(s => s.Itemstack?.Collectible != null).ToArray();
        if (nonEmptySlots.Length != 2)
        {
            __result = false;
            return false;
        }

        var hasTierItem = nonEmptySlots.Any(s => s.Itemstack.Collectible.Code.Path.StartsWith("tier"));
        var hasTargetItem = nonEmptySlots.Any(s => Rarity.IsSuitableFor(s.Itemstack, false));
        
        // TODO: ignore lower tiers, should we apply a tier tag on items or base on rarity applied ??
        
        __result = hasTierItem && hasTargetItem;
        return false;
    }
}