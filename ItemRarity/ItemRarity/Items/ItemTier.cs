using System.Linq;
using System.Text;
using ItemRarity.Extensions;
using ItemRarity.Logging;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace ItemRarity.Items;

public sealed class ItemTier : Item
{
    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        var tierLevel = inSlot.Itemstack.Collectible.Code.EndVariantInteger();

        if (tierLevel <= 0)
        {
            Logger.Error("Tier level out of range.");
            return;
        }

        if (!ModCore.Config.Tier.TryGetTier(tierLevel, out var tierConfig))
            return;

        var totalWeight = tierConfig.Rarities.Sum(r => r.Value);

        dsc.AppendLine(Lang.Get("itemrarity:item-tier-info"));

        foreach (var (rarityKey, value) in tierConfig.Rarities.OrderByDescending(kvp => kvp.Value))
        {
            var chancePercent = value / totalWeight * 100;

            if (ModCore.Config.Rarity.TryGetRarity(rarityKey, out var rarity))
                dsc.AppendLine(
                    $"  <font color=\"{rarity.Color}\">{Lang.GetWithFallback($"itemrarity:{rarityKey}", "itemrarity:unknown", rarity.Name)}</font>: {chancePercent:F2}%");
            else
                dsc.AppendLine($"   §c{rarityKey}§7: {chancePercent:F2}% (undefined rarity)");
        }
    }
}