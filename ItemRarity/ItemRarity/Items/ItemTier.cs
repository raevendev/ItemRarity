﻿using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace ItemRarity.Items;

public sealed class ItemTier : Item
{
    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        var tierLevel = inSlot.Itemstack.Collectible.Code.EndVariant().ToUpper();

        if (!ModCore.Config.Tier.TryGetTier(tierLevel, out var tierConfig))
            return;

        var totalWeight = tierConfig.Rarities.Sum(r => r.Value);

        dsc.AppendLine(Lang.Get("itemrarity:item-tier-info"));

        foreach (var kvp in tierConfig.Rarities.OrderByDescending(kvp => kvp.Value))
        {
            var rarityKey = kvp.Key;
            var chancePercent = kvp.Value / totalWeight * 100;

            if (ModCore.Config.Rarity.TryGetRarity(rarityKey, out var rarity))
                dsc.AppendLine(
                    $"  <font color=\"{rarity.Color}\">{Lang.GetWithFallback($"itemrarity:{rarityKey}", "itemrarity:unknown", rarity.Name)}</font>: {chancePercent:F2}%");
            else
                dsc.AppendLine($"   §c{rarityKey}§7: {chancePercent:F2}% (undefined rarity)");
        }
    }
}