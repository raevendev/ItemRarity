using System.Linq;
using HarmonyLib;
using ItemRarity.Extensions;
using ItemRarity.Logging;
using ItemRarity.Rarities;
using ItemRarity.Tiers;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches.Methods;

[HarmonyPatch]
public static class ConsumeCraftingIngredientsPatch
{
    /// <summary>
    /// Overrides the ingredient consumption logic for general collectible objects during crafting.
    /// Replaces the original result with a custom implementation.
    /// </summary>
    /// <param name="__instance">The collectible object instance involved in the crafting.</param>
    /// <param name="slots">The input item slots containing the ingredients.</param>
    /// <param name="outputSlot">The output slot where the crafted item will be placed.</param>
    /// <param name="matchingRecipe">The recipe being executed.</param>
    /// <param name="__result">The final success status of the ingredient consumption.</param>
    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.ConsumeCraftingIngredients)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void CollectibleObject_ConsumeCraftingIngredientsPatch(CollectibleObject __instance, ItemSlot[] slots, ItemSlot outputSlot, GridRecipe matchingRecipe,
        ref bool __result)
    {
        __result = ConsumeCraftingIngredients(slots, outputSlot, matchingRecipe);
    }

    /// <summary>
    /// Overrides the ingredient consumption logic for wearable items during crafting.
    /// Replaces the original result with a custom implementation.
    /// </summary>
    /// <param name="__instance">The wearable item instance involved in the crafting.</param>
    /// <param name="inSlots">The input item slots containing the ingredients.</param>
    /// <param name="outputSlot">The output slot where the crafted item will be placed.</param>
    /// <param name="recipe">The recipe being executed.</param>
    /// <param name="bhHandling">The event handling status (modified by reference).</param>
    /// <param name="__result">The final success status of the ingredient consumption.</param>
    [HarmonyPatch(typeof(CollectibleBehaviorWearable), nameof(CollectibleBehaviorWearable.ConsumeCraftingIngredients)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void CollectibleBehaviorWearable_ConsumeCraftingIngredientsPatch(CollectibleBehaviorWearable __instance, ItemSlot[] inSlots, ItemSlot outputSlot, IRecipeBase recipe,
        ref EnumHandling bhHandling, ref bool __result)
    {
        __result = ConsumeCraftingIngredients(inSlots, outputSlot, recipe);
    }

    private static bool ConsumeCraftingIngredients(ItemSlot[] inSlots, ItemSlot outputSlot, IRecipeBase recipe)
    {
        if (outputSlot is not { Itemstack: not null })
            return false;

        switch (ModCore.Config.Tier.EnableTiers)
        {
            case false when Rarity.IsSuitableFor(outputSlot.Itemstack): // Without Tiers.
                Rarity.ApplyRarity(outputSlot.Itemstack);
                break;
            case true when Rarity.IsSuitableFor(outputSlot.Itemstack, false): // With Tiers.
            {
                var tierItem = inSlots.FirstOrDefault(s => s.Itemstack?.Collectible?.Code.PathStartsWith("tier") ?? false);

                if (tierItem == null)
                {
                    Logger.Warning("Failed to find tier item when applying tier");
                    return false;
                }

                var targetItem = inSlots.FirstOrDefault(s => s.Itemstack?.Collectible?.Durability > 1);

                if (targetItem == null)
                {
                    Logger.Warning("Failed to find target item when applying tier");
                    return false;
                }

                if (Rarity.TryGetRarity(targetItem.Itemstack, out _))
                    Tier.ApplyTierUpgrade(targetItem.Itemstack, outputSlot.Itemstack, tierItem.Itemstack.Collectible.Code.EndVariantInteger());
                else
                    Tier.ApplyTier(outputSlot.Itemstack, tierItem.Itemstack.Collectible.Code.EndVariantInteger());

                foreach (var slot in inSlots.Where(s => s.Itemstack?.Collectible?.Code == outputSlot.Itemstack?.Collectible?.Code))
                    slot.TakeOut(1);
                break;
            }
        }

        return false;
    }
}