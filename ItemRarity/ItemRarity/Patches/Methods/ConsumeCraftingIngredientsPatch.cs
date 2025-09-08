using System.Linq;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches.Methods;

[HarmonyPatch]
public static class ConsumeCraftingIngredientsPatch
{
    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.ConsumeCraftingIngredients)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void CollectibleObject_ConsumeCraftingIngredientsPatch(CollectibleObject __instance, ItemSlot[] slots, ItemSlot outputSlot, GridRecipe matchingRecipe)
    {
        ConsumeCraftingIngredients(slots, outputSlot, matchingRecipe);
    }

    [HarmonyPatch(typeof(ItemWearable), nameof(ItemWearable.ConsumeCraftingIngredients)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void ItemWearable_ConsumeCraftingIngredientsPatch(ItemWearable __instance, ItemSlot[] inSlots, ItemSlot outputSlot, GridRecipe recipe)
    {
        ConsumeCraftingIngredients(inSlots, outputSlot, recipe);
    }

    private static void ConsumeCraftingIngredients(ItemSlot[] inSlots, ItemSlot outputSlot, GridRecipe recipe)
    {
        if (outputSlot is not { Itemstack: not null } || !RarityManager.IsSuitableFor(outputSlot.Itemstack))
            return;

        if (!ModCore.Config.Tier.EnableTiers)
            RarityManager.SetRandomRarity(outputSlot.Itemstack);
        else
        {
            var tierItem = inSlots.FirstOrDefault(s => s.Itemstack?.Collectible?.Code.PathStartsWith("tier") ?? false);

            if (tierItem == null)
            {
                ModLogger.Warning("Failed to find tier item when crafting");
                return;
            }

            RarityManager.SetRarityByTier(outputSlot.Itemstack, tierItem.Itemstack.Collectible.Code.EndVariant().ToUpper());

            foreach (var slot in inSlots.Where(s => s.Itemstack?.Collectible?.Code == outputSlot.Itemstack?.Collectible?.Code))
                slot.TakeOut(1);
        }
    }
}