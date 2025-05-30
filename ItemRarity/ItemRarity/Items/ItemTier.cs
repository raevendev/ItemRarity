using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace ItemRarity.Items;

public sealed class ItemTier : Item
{
    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        var tierLevel = inSlot.Itemstack.Collectible.Code.EndVariant().ToUpper();

        if (!ModCore.Config.Tiers.TryGetValue(tierLevel, out var tierConfig))
            return;

        var totalWeight = tierConfig.Values.Sum();

        dsc.AppendLine(Lang.Get("itemrarity:item-tier-info"));

        foreach (var kvp in tierConfig.OrderByDescending(kvp => kvp.Value))
        {
            var rarityKey = kvp.Key;
            var chancePercent = kvp.Value / totalWeight * 100;

            if (ModCore.Config.Rarities.TryGetValue(rarityKey, out var rarity))
                dsc.AppendLine(
                    $"  <font color=\"{rarity.Color}\">{Lang.GetWithFallback($"itemrarity:{rarityKey}", "itemrarity:unknown", rarity.Name)}</font>: {chancePercent:F2}%");
            else
                dsc.AppendLine($"   §c{rarityKey}§7: {chancePercent:F2}% (undefined rarity)");
        }
    }
}