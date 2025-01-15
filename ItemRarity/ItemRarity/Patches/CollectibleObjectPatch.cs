using System;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

/// <summary>
/// A Harmony patch class for modifying the behavior of the <see cref="CollectibleObject"/> class.
/// </summary>
[HarmonyPatch(typeof(CollectibleObject))]
public static class CollectibleObjectPatch
{
    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetHeldItemInfo)), HarmonyPriority(Priority.Last)]
    public static void GetHeldItemInfoPatch(CollectibleObject __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        var itemstack = inSlot.Itemstack;

        if (!ModRarity.TryGetRarityTreeAttribute(itemstack, out var modAttribute))
            return;

        if (!modAttribute.HasAttribute(ModAttributes.Rarity) || !modAttribute.HasAttribute(ModAttributes.MiningSpeed))
            return;

        FixItemInfos(itemstack, __instance, dsc, modAttribute);
    }

    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetHeldItemName)), HarmonyPriority(Priority.Last)]
    public static void GetHeldItemNamePatch(CollectibleObject __instance, ItemStack itemStack, ref string __result)
    {
        if (!ModRarity.TryGetRarityTreeAttribute(itemStack, out var modAttribute))
            return;

        var attributeRarity = modAttribute.GetString(ModAttributes.Rarity);
        var rarity = ModCore.Config[attributeRarity];
        var rarityName = rarity.Value.IgnoreTranslation
            ? $"[{rarity.Value.Name}]"
            : Lang.GetWithFallback($"itemrarity:{attributeRarity}", "itemrarity:unknown", rarity.Value.Name);

        if (__result.Contains(rarityName))
            return;

        __result = $"<font color=\"{rarity.Value.Color}\" weight=bold>{rarityName} {__result}</font>";
    }

    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetMaxDurability)), HarmonyPriority(Priority.Last)]
    public static void GetMaxDurabilityPatch(CollectibleObject __instance, ItemStack itemstack, ref int __result)
    {
        if (!ModRarity.TryGetRarityTreeAttribute(itemstack, out var modAttribute))
            return;

        if (!modAttribute.HasAttribute(ModAttributes.Rarity) || !modAttribute.HasAttribute(ModAttributes.MaxDurability))
            return;

        __result = modAttribute.GetInt(ModAttributes.MaxDurability);
    }

    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetAttackPower)), HarmonyPriority(Priority.Last)]
    public static void GetAttackPowerPatch(CollectibleObject __instance, ItemStack withItemStack, ref float __result)
    {
        if (!ModRarity.TryGetRarityTreeAttribute(withItemStack, out var modAttribute))
            return;

        if (!modAttribute.HasAttribute(ModAttributes.Rarity) || !modAttribute.HasAttribute(ModAttributes.AttackPower))
            return;

        __result = modAttribute.GetFloat(ModAttributes.AttackPower);
    }

    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetMiningSpeed)), HarmonyPriority(Priority.Last)]
    public static void GetMiningSpeedPatch(CollectibleObject __instance, IItemStack itemstack, BlockSelection blockSel, Block block, IPlayer forPlayer,
        ref float __result, ref ICoreAPI ___api)
    {
        if (!ModRarity.TryGetRarityTreeAttribute(itemstack as ItemStack, out var modAttribute))
            return;

        if (!modAttribute.HasAttribute(ModAttributes.Rarity) || !modAttribute.HasAttribute(ModAttributes.MiningSpeed))
            return;

        var miningSpeedAttribute = modAttribute.GetTreeAttribute(ModAttributes.MiningSpeed);

        var traitMiningSpeed = block.GetBlockMaterial(___api.World.BlockAccessor, blockSel.Position) switch // Same as the original method
        {
            EnumBlockMaterial.Ore or EnumBlockMaterial.Stone => forPlayer.Entity.Stats.GetBlended("miningSpeedMul"),
            _ => 1f
        };

        __result = miningSpeedAttribute.GetFloat(block.BlockMaterial.ToString(), __result) * traitMiningSpeed;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.ConsumeCraftingIngredients)), HarmonyPriority(Priority.Last)]
    public static void ConsumeCraftingIngredientsPatch(CollectibleObject __instance, ItemSlot[] slots, ItemSlot outputSlot, GridRecipe matchingRecipe)
    {
        var itemStack = outputSlot.Itemstack;

        if (itemStack == null || itemStack.Item?.Tool == null || itemStack.Attributes.HasAttribute(ModAttributes.Guid))
            return;

        var rarity = ModRarity.GetRandomRarity();

        itemStack.SetRarity(rarity.Key);
    }

    [HarmonyReversePatch, HarmonyPatch(nameof(CollectibleObject.GetHeldItemInfo))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void GetHeldItemInfoReversePatch(CollectibleObject __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
    }

    public static void FixItemInfos(ItemStack itemStack, CollectibleObject collectible, StringBuilder sb, ITreeAttribute modAttribute)
    {
        var lines = sb.ToString().Trim().Split(Environment.NewLine); // Split all lines

        sb.Clear();

        if (itemStack.Collectible.MiningSpeed != null && itemStack.Collectible.MiningSpeed.Count > 0)
        {
            var miningSpeedLine = Lang.Get("item-tooltip-miningspeed") ?? string.Empty;
            var foundLine = Array.FindIndex(lines, line => line.StartsWith(miningSpeedLine, StringComparison.Ordinal));
            var miningSpeeds = modAttribute.GetTreeAttribute(ModAttributes.MiningSpeed);

            for (var i = 0; i < lines.Length; i++)
            {
                if (i != foundLine)
                {
                    sb.AppendLine(lines[i]);
                    continue;
                }

                sb.Append(miningSpeedLine);

                var num = 0;

                foreach (var speed in miningSpeeds)
                {
                    var miningSpeedFactor = miningSpeeds.GetFloat(speed.Key);
                    if (miningSpeedFactor >= 1.1)
                    {
                        if (num++ > 0)
                            sb.Append(", ");
                        sb.Append(Lang.Get(speed.Key))
                            .Append(' ')
                            .Append(miningSpeedFactor.ToString("#.#"))
                            .Append('x');
                    }
                }

                sb.AppendLine();
            }
        }
        else if (collectible is ItemWearable { ProtectionModifiers: not null } wearable)
        {
            var protectionModifier = modAttribute.GetTreeAttribute(ModAttributes.ProtectionModifiers);

            if (protectionModifier == null)
            {
                sb.AppendLine("Missing flat protection modifier");
                return;
            }

            var flatProtTranslation = Lang.Get("Flat damage reduction: {0} hp", wearable.ProtectionModifiers.FlatDamageReduction) ?? string.Empty;
            // var relProtTranslation = Lang.Get("Percent protection: {0}%", 100.0 * wearable.ProtectionModifiers.RelativeProtection) ?? string.Empty;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (line.StartsWith(flatProtTranslation))
                    sb.AppendLine(Lang.Get("Flat damage reduction: {0} hp", protectionModifier.GetFloat(ModAttributes.ArmorFlatDamageReduction).ToString("F")));
                // else if (line.StartsWith(relProtTranslation.Substring(0, 5)))
                //     sb.AppendLine(Lang.Get("Percent protection: {0}%", (protectionModifier.GetFloat(ModAttributes.ArmorRelativeProtection) * 100.0).ToString("F")));
                else
                    sb.AppendLine(line);
            }
        }
    }
}