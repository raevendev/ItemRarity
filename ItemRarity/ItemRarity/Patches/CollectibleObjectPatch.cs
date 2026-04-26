using System;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Attribute = ItemRarity.Attributes.Attribute;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

[HarmonyPatch(typeof(CollectibleObject))]
public static class CollectibleObjectPatch
{
    [HarmonyReversePatch, HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetHeldItemInfo))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void GetHeldItemInfoReversePatch(CollectibleObject __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world,
        bool withDebugInfo)
    {
    }

    [HarmonyPatch(nameof(CollectibleObject.GetAttackPower)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void GetAttackPowerPatch(CollectibleObject __instance, ItemStack itemStack, ref float __result)
    {
        if (!Rarity.TryGetRarity(itemStack, out _))
            return;

        __result *= Attribute.AttackPowerMultiplier.GetFloat(itemStack, 1f);
    }

    [HarmonyPatch(nameof(CollectibleObject.GetMaxDurability)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void GetMaxDurabilityPatch(CollectibleObject __instance, ItemStack itemstack, ref int __result)
    {
        if (!Rarity.TryGetRarity(itemstack, out _))
            return;

        __result = (int)(__result * Attribute.MaxDurabilityMultiplier.GetFloat(itemstack, 1f));
    }

    [HarmonyPatch(nameof(CollectibleObject.GetMiningSpeed)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void GetMiningSpeedPatch(CollectibleObject __instance, IItemStack itemstack, BlockSelection blockSel, Block block, IPlayer forPlayer,
        ref float __result)
    {
        if (!Rarity.TryGetRarity(itemstack as ItemStack, out _))
            return;

        __result *= Attribute.MiningSpeedMultiplier.GetFloat(itemstack as ItemStack, 1f);
    }

    [HarmonyPatch(nameof(CollectibleObject.GetAttackRange)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void GetAttackRangePatch(CollectibleObject __instance, ItemStack withItemStack, ref float __result)
    {
        if (!Rarity.TryGetRarity(withItemStack, out _))
            return;

        __result *= Attribute.AttackRangeMultiplier.GetFloat(withItemStack, 1f);
    }

    [HarmonyPatch(nameof(CollectibleObject.GetHeldItemName)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void GetHeldItemNamePatch(CollectibleObject __instance, ItemStack itemStack, ref string __result)
    {
        PatchHelpers.GetHeldItemName(itemStack, ref __result);
    }
    
    [HarmonyPatch(nameof(CollectibleObject.ConsumeCraftingIngredients)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void ConsumeCraftingIngredientsPatch(CollectibleObject __instance, ItemSlot[] slots, ItemSlot outputSlot, GridRecipe matchingRecipe,
        ref bool __result)
    {
        __result = PatchHelpers.ConsumeCraftingIngredients(slots, outputSlot, matchingRecipe);
    }

    [HarmonyPatch(nameof(CollectibleObject.GetHeldItemInfo)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void GetHeldItemInfoPatch(CollectibleObject __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        if (inSlot is not { Itemstack: not null })
            return;

        if (!Rarity.TryGetRarity(inSlot.Itemstack, out var rarity))
            return;

        if (inSlot.Itemstack.Collectible.MiningSpeed is { Count: > 0 })
        {
            var lines = dsc.ToString().Trim().Split(Environment.NewLine); // Split all lines

            dsc.Clear();

            var miningSpeedLine = Lang.Get("item-tooltip-miningspeed") ?? string.Empty;
            var foundLine = Array.FindIndex(lines, line => line.StartsWith(miningSpeedLine, StringComparison.Ordinal));
            var miningSpeedMul = Attribute.MiningSpeedMultiplier.GetFloat(inSlot.Itemstack, 1f);

            for (var i = 0; i < lines.Length; i++)
            {
                if (i != foundLine)
                {
                    dsc.AppendLine(lines[i]);

                    continue;
                }

                dsc.Append(miningSpeedLine);

                var num = 0;

                foreach (var miningSpeed in inSlot.Itemstack.Collectible.MiningSpeed)
                {
                    if (miningSpeed.Value <= 1.0)
                        continue;

                    if (num++ > 0)
                        dsc.Append(", ");
                    dsc.Append(Lang.Get(miningSpeed.Key.ToString()))
                        .Append(' ')
                        .Append((miningSpeed.Value * miningSpeedMul).ToString("#.#"))
                        .Append('x');
                }

                dsc.AppendLine();

                if (inSlot.Itemstack.Collectible.GetAttackPower(inSlot.Itemstack) > 0.5)
                {
                    dsc.AppendLine(Lang.Get("Attack power: {0} damage", inSlot.Itemstack.Collectible.GetAttackPower(inSlot.Itemstack).ToString("0.#")));
                }
            }
        }
    }
}