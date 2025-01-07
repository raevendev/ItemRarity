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
    /// <summary>
    /// A Harmony patch applied to the <c>GetHeldItemInfo</c> method of the <c>CollectibleObject</c> class. 
    /// This patch appends additional mining speed information to the item's description.
    /// </summary>
    /// <param name="__instance">The instance of the <c>CollectibleObject</c> invoking the method.</param>
    /// <param name="inSlot">The <c>ItemSlot</c> containing the item for which information is being retrieved.</param>
    /// <param name="dsc">
    /// A <c>StringBuilder</c> object to which the item's description, including any additional information, is appended.
    /// </param>
    /// <param name="world">The <c>IWorldAccessor</c> representing the game world in which the item exists.</param>
    /// <param name="withDebugInfo">A boolean indicating whether debug information should be included in the item's description.</param>
    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetHeldItemInfo)), HarmonyPriority(0)]
    public static void GetHeldItemInfoPatch(CollectibleObject __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        var itemstack = inSlot.Itemstack;

        if (!ModRarity.TryGetRarityTreeAttribute(itemstack, out var modAttribute))
            return;

        if (!modAttribute.HasAttribute(ModAttributes.Rarity) || !modAttribute.HasAttribute(ModAttributes.MiningSpeed))
            return;

        FixItemInfos(itemstack, __instance, dsc, modAttribute);
    }

    /// <summary>
    /// A Harmony patch applied to the <c>GetHeldItemName</c> method of the <c>CollectibleObject</c> class. 
    /// This patch modifies the display name of a held item to include its rarity, formatted with a specific color and style.
    /// </summary>
    /// <param name="__instance">The instance of the <c>CollectibleObject</c> invoking the method.</param>
    /// <param name="itemStack">The <c>ItemStack</c> representing the held item whose name is being queried.</param>
    /// <param name="__result">A reference to the resulting item name.</param>
    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetHeldItemName)), HarmonyPriority(0)]
    public static void GetHeldItemNamePatch(CollectibleObject __instance, ItemStack itemStack, ref string __result)
    {
        if (!ModRarity.TryGetRarityTreeAttribute(itemStack, out var modAttribute))
            return;

        var attributeRarity = modAttribute.GetString(ModAttributes.Rarity);
        var rarity = ModCore.Config[attributeRarity];
        var rarityName = Lang.GetWithFallback($"itemrarity:{attributeRarity}", "itemrarity:unknown", rarity.Value.Name);

        if (__result.Contains(rarityName))
            return;

        __result = $"<font color=\"{rarity.Value.Color}\" weight=bold>{rarityName} {__result}</font>";
    }

    /// <summary>
    /// A Harmony patch applied to the <c>GetMaxDurability</c> method of the <c>CollectibleObject</c> class. 
    /// This patch modifies the maximum durability of an item based on its rarity.
    /// </summary>
    /// <param name="__instance">The instance of the <c>CollectibleObject</c> invoking the method.</param>
    /// <param name="itemstack">The <c>ItemStack</c> representing the item whose durability is being queried.</param>
    /// <param name="__result">A reference to the resulting maximum durability.</param>
    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetMaxDurability)), HarmonyPriority(0)]
    public static void GetMaxDurabilityPatch(CollectibleObject __instance, ItemStack itemstack, ref int __result)
    {
        if (!ModRarity.TryGetRarityTreeAttribute(itemstack, out var modAttribute))
            return;

        if (!modAttribute.HasAttribute(ModAttributes.Rarity) || !modAttribute.HasAttribute(ModAttributes.MaxDurability))
            return;

        __result = modAttribute.GetInt(ModAttributes.MaxDurability);
    }

    /// <summary>
    /// A Harmony patch applied to the <c>GetAttackPower</c> method of the <c>CollectibleObject</c> class. 
    /// This patch modifies the attack power of an item based on its rarity.
    /// </summary>
    /// <param name="__instance">The instance of the <c>CollectibleObject</c> invoking the method.</param>
    /// <param name="withItemStack">The <c>ItemStack</c> representing the item being used to attack.</param>
    /// <param name="__result">A reference to the resulting attack power.</param>
    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetAttackPower)), HarmonyPriority(0)]
    public static void GetAttackPowerPatch(CollectibleObject __instance, ItemStack withItemStack, ref float __result)
    {
        if (!ModRarity.TryGetRarityTreeAttribute(withItemStack, out var modAttribute))
            return;

        if (!modAttribute.HasAttribute(ModAttributes.Rarity) || !modAttribute.HasAttribute(ModAttributes.AttackPower))
            return;

        __result = modAttribute.GetFloat(ModAttributes.AttackPower);
    }

    /// <summary>
    /// A Harmony patch applied to the <c>GetMiningSpeed</c> method of the <c>CollectibleObject</c> class. 
    /// This patch modifies the mining speed of an item based on its rarity.
    /// </summary>
    /// <param name="__instance">The instance of the <c>CollectibleObject</c> invoking the method.</param>
    /// <param name="itemstack">The <c>IItemStack</c> representing the item being used to mine.</param>
    /// <param name="blockSel">The <c>BlockSelection</c> providing details about the block being targeted.</param>
    /// <param name="block">The <c>Block</c> being mined.</param>
    /// <param name="forPlayer">The <c>IPlayer</c> instance representing the player performing the mining action.</param>
    /// <param name="__result">A reference to the resulting mining speed.</param>
    /// <param name="___api">A reference to the <c>ICoreAPI</c> for this collectible</param>
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

    /// <summary>
    /// A Harmony patch applied to the <c>ConsumeCraftingIngredients</c> method of the <c>CollectibleObject</c> class. 
    /// This patch assigns a random rarity to crafted items when they are created.
    /// </summary>
    /// <param name="__instance">The instance of the <c>CollectibleObject</c> that invoked the method.</param>
    /// <param name="slots">An array of <c>ItemSlot</c> objects representing the crafting ingredients.</param>
    /// <param name="outputSlot">The <c>ItemSlot</c> where the crafted item will be placed.</param>
    /// <param name="matchingRecipe">The <c>GridRecipe</c> object that matched the crafting process.</param>
    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.ConsumeCraftingIngredients)), HarmonyPriority(0)]
    public static void ConsumeCraftingIngredientsPatch(CollectibleObject __instance, ItemSlot[] slots, ItemSlot outputSlot, GridRecipe matchingRecipe)
    {
        var itemStack = outputSlot.Itemstack;

        if (itemStack == null || itemStack.Item?.Tool == null || itemStack.Attributes.HasAttribute(ModAttributes.Guid))
            return;

        var rarity = ModRarity.GetRandomRarity();

        itemStack.SetRarity(rarity.Key);
    }

    /// <summary>
    /// A Harmony reverse patch for the <c>GetHeldItemInfo</c> method of the <c>CollectibleObject</c> class. 
    /// This method provides a callable version of the original method.
    /// </summary>
    /// <param name="__instance">The instance of the <c>CollectibleObject</c> whose information is being retrieved.</param>
    /// <param name="inSlot">The <c>ItemSlot</c> containing the item for which information is being queried.</param>
    /// <param name="dsc">
    /// A <c>StringBuilder</c> object to which the item's original description, or any additional information, is appended.
    /// </param>
    /// <param name="world">The <c>IWorldAccessor</c> representing the game world in which the item exists.</param>
    /// <param name="withDebugInfo">
    /// A boolean indicating whether debug information should be included in the item's description.
    /// </param>
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
        else if (collectible is ItemWearable wearable)
        {
            var protectionModifier = modAttribute.GetTreeAttribute(ModAttributes.ProtectionModifiers);

            if (protectionModifier == null)
            {
                sb.AppendLine("Missing flat protection modifier");
                return;
            }

            var flatProtTranslation = Lang.Get("Flat damage reduction: {0} hp", wearable.ProtectionModifiers.FlatDamageReduction) ?? string.Empty;
            var relProtTranslation = Lang.Get("Percent protection: {0}%", 100.0 * wearable.ProtectionModifiers.RelativeProtection) ?? string.Empty;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (line.StartsWith(flatProtTranslation))
                    sb.AppendLine(Lang.Get("Flat damage reduction: {0} hp", protectionModifier.GetFloat(ModAttributes.FlatDamageReduction).ToString("F")));
                // else if (line.StartsWith(relProtTranslation.Substring(0, 5)))
                //     sb.AppendLine(Lang.Get("Percent protection: {0}%", (protectionModifier.GetFloat(ModAttributes.RelativeProtection) * 100.0).ToString("F")));
                else
                    sb.AppendLine(line);
            }
        }
    }
}