using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using ItemRarity.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

/// <summary>
/// A Harmony patch class for modifying the behavior of the <see cref="CollectibleObject"/> class.
/// Always use <see cref="Priority.Last"/> if possible, so we *should* always be the last modifying an item's properties.
/// </summary>
[HarmonyPatch(typeof(CollectibleObject))]
public static class CollectibleObjectPatch
{
    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetHeldItemInfo)), HarmonyPriority(Priority.Last)]
    public static void GetHeldItemInfoPatch(CollectibleObject __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        if (inSlot is not { Itemstack: not null })
            return;

        if (!RarityManager.TryGetRarity(inSlot.Itemstack, out var rarity))
            return;

        FixItemInfos(rarity, inSlot.Itemstack, __instance, dsc);
    }

    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetHeldItemName)), HarmonyPriority(Priority.Last)]
    public static void GetHeldItemNamePatch(CollectibleObject __instance, ItemStack itemStack, ref string __result)
    {
        if (!RarityManager.TryGetRarity(itemStack, out var rarity))
            return;

        var rarityName = rarity.IgnoreTranslation
            ? $"[{rarity.Name}]"
            : Lang.GetWithFallback($"itemrarity:{rarity.Key}", "itemrarity:unknown", rarity.Name);

        if (__result.Contains(rarityName))
            return;

        __result = $"<font color=\"{rarity.Color}\" weight=bold>{rarityName} {__result}</font>";
    }

    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetMaxDurability)), HarmonyPriority(Priority.Last)]
    public static void GetMaxDurabilityPatch(CollectibleObject __instance, ItemStack itemstack, ref int __result)
    {
        if (!RarityManager.TryGetRarity(itemstack, out var rarityInfos))
            return;

        __result = (int)(__result * rarityInfos.DurabilityMultiplier);
    }

    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetAttackPower)), HarmonyPriority(Priority.Last)]
    public static void GetAttackPowerPatch(CollectibleObject __instance, ItemStack withItemStack, ref float __result)
    {
        if (__instance is ItemWearable || !RarityManager.TryGetRarity(withItemStack, out var rarity))
            return;

        __result *= rarity.AttackPowerMultiplier;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetMiningSpeed)), HarmonyPriority(Priority.Last)]
    public static void GetMiningSpeedPatch(CollectibleObject __instance, IItemStack itemstack, BlockSelection blockSel, Block block, IPlayer forPlayer,
        ref float __result, ref ICoreAPI ___api)
    {
        if (!RarityManager.TryGetRarity(itemstack as ItemStack, out var rarityInfos))
            return;

        __result *= rarityInfos.MiningSpeedMultiplier;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.ConsumeCraftingIngredients)), HarmonyPriority(Priority.Last)]
    public static void ConsumeCraftingIngredientsPatch(CollectibleObject __instance, ItemSlot[] slots, ItemSlot outputSlot, GridRecipe matchingRecipe)
    {
        if (outputSlot is not { Itemstack: not null } || !RarityManager.IsSuitableFor(outputSlot.Itemstack))
            return;

        if (!ModCore.Config.Tier.EnableTiers)
            RarityManager.SetRandomRarity(outputSlot.Itemstack);
        else
        {
            var tierItem = slots.FirstOrDefault(s => s.Itemstack?.Collectible?.Code.PathStartsWith("tier") ?? false);

            if (tierItem == null)
            {
                ModLogger.Warning("Failed to find tier item when crafting");
                return;
            }

            RarityManager.SetRarityByTier(outputSlot.Itemstack, tierItem.Itemstack.Collectible.Code.EndVariant().ToUpper());

            foreach (var slot in slots.Where(s => s.Itemstack?.Collectible?.Code == outputSlot.Itemstack?.Collectible?.Code))
                slot.TakeOut(1);
        }
    }

    [HarmonyReversePatch, HarmonyPatch(nameof(CollectibleObject.GetHeldItemInfo))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void GetHeldItemInfoReversePatch(CollectibleObject __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
    }

    public static void FixItemInfos(Rarity rarityInfos, ItemStack itemStack, CollectibleObject collectible, StringBuilder sb)
    {
        if (itemStack.Collectible.MiningSpeed != null && itemStack.Collectible.MiningSpeed.Count > 0)
        {
            var lines = sb.ToString().Trim().Split(Environment.NewLine); // Split all lines

            sb.Clear();

            var miningSpeedLine = Lang.Get("item-tooltip-miningspeed") ?? string.Empty;
            var foundLine = Array.FindIndex(lines, line => line.StartsWith(miningSpeedLine, StringComparison.Ordinal));

            for (var i = 0; i < lines.Length; i++)
            {
                if (i != foundLine)
                {
                    sb.AppendLine(lines[i]);

                    continue;
                }

                sb.Append(miningSpeedLine);

                var num = 0;

                foreach (var miningSpeed in collectible.MiningSpeed)
                {
                    if (miningSpeed.Value <= 1.0)
                        continue;

                    if (num++ > 0)
                        sb.Append(", ");
                    sb.Append(Lang.Get(miningSpeed.Key.ToString()))
                        .Append(' ')
                        .Append((miningSpeed.Value * rarityInfos.MiningSpeedMultiplier).ToString("#.#"))
                        .Append('x');
                }

                sb.AppendLine();

                if (collectible.GetAttackPower(itemStack) > 0.5)
                {
                    sb.AppendLine(Lang.Get("Attack power: -{0} hp", collectible.GetAttackPower(itemStack).ToString("0.#")));
                }
            }
        }
        else if (collectible is ItemWearable { ProtectionModifiers: not null } wearable)
        {
            // var protectionModifier = modAttribute.GetTreeAttribute(ModAttributes.ProtectionModifiers);
            //
            // if (protectionModifier == null)
            // {
            //     sb.AppendLine("Missing flat protection modifier");
            //     return;
            // }
            //
            // var flatProtTranslation = Lang.Get("Flat damage reduction: {0} hp", wearable.ProtectionModifiers.FlatDamageReduction) ?? string.Empty;
            // // var relProtTranslation = Lang.Get("Percent protection: {0}%", 100.0 * wearable.ProtectionModifiers.RelativeProtection) ?? string.Empty;
            //
            // for (var i = 0; i < lines.Length; i++)
            // {
            //     var line = lines[i];
            //
            //     if (line.StartsWith(flatProtTranslation))
            //         sb.AppendLine(Lang.Get("Flat damage reduction: {0} hp", protectionModifier.GetFloat(ModAttributes.ArmorFlatDamageReduction).ToString("F")));
            //     // else if (line.StartsWith(relProtTranslation.Substring(0, 5)))
            //     //     sb.AppendLine(Lang.Get("Percent protection: {0}%", (protectionModifier.GetFloat(ModAttributes.ArmorRelativeProtection) * 100.0).ToString("F")));
            //     else
            //         sb.AppendLine(line);
            // }
        }
    }
}