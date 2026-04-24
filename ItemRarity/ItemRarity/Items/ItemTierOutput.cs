using System.Linq;
using ItemRarity.Logging;
using Vintagestory.API.Common;

namespace ItemRarity.Items;

public sealed class ItemTierOutput : Item
{
    public override void OnCreatedByCrafting(ItemSlot[] allInputSlots, ItemSlot outputSlot, IRecipeBase byRecipe)
    {
        var tierItem = allInputSlots.FirstOrDefault(s => s.Itemstack?.Collectible?.Code.PathStartsWith("tier") ?? false);
        var targetItem = allInputSlots.FirstOrDefault(s => s.Itemstack?.Collectible?.Durability > 1);

        if (tierItem == null || targetItem == null)
        {
            outputSlot.Itemstack = null;
            return;
        }

        if (targetItem.Itemstack is null)
        {
            Logger.Error("targetItem.Itemstack is null");
            return;
        }

        outputSlot.Itemstack = targetItem.Itemstack.Clone();
    }
}