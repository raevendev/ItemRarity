using System.Linq;
using HarmonyLib;
using ItemRarity.Extensions;
using ItemRarity.Rarities;
using ItemRarity.Tiers;
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
        if (outputSlot is not { Itemstack: not null })
            return;

        switch (ModCore.Config.Tier.EnableTiers)
        {
            case false when Rarity.IsSuitableFor(outputSlot.Itemstack):
                Rarity.ApplyRarity(outputSlot.Itemstack);
                break;
            case true when Rarity.IsSuitableFor(outputSlot.Itemstack, false):
            {
                var tierItem = inSlots.FirstOrDefault(s => s.Itemstack?.Collectible?.Code.PathStartsWith("tier") ?? false);

                if (tierItem == null)
                {
                    ModLogger.Warning("Failed to find tier item when crafting");
                    return;
                }

                if (Rarity.TryGetRarity(outputSlot.Itemstack, out _))
                    Tier.ApplyTierUpgrade(outputSlot.Itemstack, tierItem.Itemstack.Collectible.Code.EndVariantInteger());
                else
                    Tier.ApplyTier(outputSlot.Itemstack, tierItem.Itemstack.Collectible.Code.EndVariantInteger());

                foreach (var slot in inSlots.Where(s => s.Itemstack?.Collectible?.Code == outputSlot.Itemstack?.Collectible?.Code))
                    slot.TakeOut(1);
                break;
            }
        }
    }
}