using System.Linq;
using ItemRarity.Extensions;
using ItemRarity.Logging;
using ItemRarity.Rarities;
using ItemRarity.Tiers;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

public static class PatchHelpers
{
    public static void GetHeldItemName(ItemStack itemStack, ref string __result)
    {
        if (!Rarity.TryGetRarity(itemStack, out var rarity))
            return;

        var rarityName = rarity.IgnoreTranslation
            ? $"[{rarity.Name}]"
            : Lang.GetWithFallback($"itemrarity:{rarity.Key}", "itemrarity:unknown", rarity.Name);

        if (__result.Contains(rarityName))
            return;

        __result = $"<font color=\"{rarity.Color}\" weight=bold>{rarityName} {__result}</font>";
    }
    
    public static bool ConsumeCraftingIngredients(ItemSlot[] inSlots, ItemSlot outputSlot, IRecipeBase recipe)
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