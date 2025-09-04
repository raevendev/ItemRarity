using System.Linq;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

[HarmonyPatch(typeof(ItemWearable))]
public static class ItemWearablePatch
{
    [HarmonyPostfix, HarmonyPatch(nameof(ItemWearable.ConsumeCraftingIngredients)), HarmonyPriority(Priority.Last)]
    public static void ConsumeCraftingIngredientsPatch(CollectibleObject __instance, ItemSlot[] inSlots, ItemSlot outputSlot, GridRecipe recipe)
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