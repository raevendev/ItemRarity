using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.Common;

namespace ItemRarity.Items;

public sealed class ItemTierOutput : Item
{
    public override void OnCreatedByCrafting(ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe)
    {
        var tierItem = allInputslots.FirstOrDefault(s => s.Itemstack?.Collectible?.Code.PathStartsWith("tier") ?? false);
        var targetItem = allInputslots.FirstOrDefault(s => s.Itemstack?.Collectible?.Durability > 1);

        if (tierItem == null || targetItem == null)
        {
            outputSlot.Itemstack = null;
            return;
        }

        outputSlot.Itemstack = targetItem.Itemstack.Clone();
    }
}