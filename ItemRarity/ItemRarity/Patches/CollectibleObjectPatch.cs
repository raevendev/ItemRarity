using System;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using ItemRarity.Extensions;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

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
    public static void PatchGetHeldItemInfo(CollectibleObject __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        if (__instance.MiningSpeed == null || __instance.MiningSpeed.Count == 0)
            return;

        var itemstack = inSlot.Itemstack;

        if (!itemstack.Attributes.HasAttribute(ModAttributes.Guid))
            return;

        var modAttributes = itemstack.Attributes.GetTreeAttribute(ModAttributes.Guid);

        if (!modAttributes.HasAttribute(ModAttributes.Rarity) || !modAttributes.HasAttribute(ModAttributes.MiningSpeed))
            return;

        var miningSpeed = modAttributes.GetTreeAttribute(ModAttributes.MiningSpeed);

        FixMiningSpeedInfos(dsc, miningSpeed);
    }

    /// <summary>
    /// A Harmony patch applied to the <c>GetHeldItemName</c> method of the <c>CollectibleObject</c> class. 
    /// This patch modifies the display name of a held item to include its rarity, formatted with a specific color and style.
    /// </summary>
    /// <param name="__instance">The instance of the <c>CollectibleObject</c> invoking the method.</param>
    /// <param name="itemStack">The <c>ItemStack</c> representing the held item whose name is being queried.</param>
    /// <param name="__result">A reference to the resulting item name.</param>
    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetHeldItemName)), HarmonyPriority(0)]
    public static void PatchGetHeldItemName(CollectibleObject __instance, ItemStack? itemStack, ref string __result)
    {
        if (itemStack == null || itemStack.Collectible?.Tool == null)
            return;

        if (!itemStack.Attributes.HasAttribute(ModAttributes.Guid))
            return;

        var modAttributes = itemStack.Attributes.GetTreeAttribute(ModAttributes.Guid);

        var attributeRarity = modAttributes.GetString(ModAttributes.Rarity);
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
    public static void PatchGetMaxDurability(CollectibleObject __instance, ItemStack itemstack, ref int __result)
    {
        if (!itemstack.Attributes.HasAttribute(ModAttributes.Guid))
            return;

        var modAttributes = itemstack.Attributes.GetTreeAttribute(ModAttributes.Guid);

        if (!modAttributes.HasAttribute(ModAttributes.Rarity) || !modAttributes.HasAttribute(ModAttributes.MaxDurability))
            return;

        __result = modAttributes.GetInt(ModAttributes.MaxDurability);
    }

    /// <summary>
    /// A Harmony patch applied to the <c>GetAttackPower</c> method of the <c>CollectibleObject</c> class. 
    /// This patch modifies the attack power of an item based on its rarity.
    /// </summary>
    /// <param name="__instance">The instance of the <c>CollectibleObject</c> invoking the method.</param>
    /// <param name="withItemStack">The <c>ItemStack</c> representing the item being used to attack.</param>
    /// <param name="__result">A reference to the resulting attack power.</param>
    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetAttackPower)), HarmonyPriority(0)]
    public static void PatchGetAttackPower(CollectibleObject __instance, ItemStack withItemStack, ref float __result)
    {
        if (!withItemStack.Attributes.HasAttribute(ModAttributes.Guid))
            return;

        var modAttributes = withItemStack.Attributes.GetTreeAttribute(ModAttributes.Guid);

        if (!modAttributes.HasAttribute(ModAttributes.Rarity) || !modAttributes.HasAttribute(ModAttributes.AttackPower))
            return;

        __result = modAttributes.GetFloat(ModAttributes.AttackPower);
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
    [HarmonyPostfix, HarmonyPatch(nameof(CollectibleObject.GetMiningSpeed)), HarmonyPriority(0)]
    public static void PatchGetMiningSpeed(CollectibleObject __instance, IItemStack itemstack, BlockSelection blockSel, Block block, IPlayer forPlayer,
        ref float __result)
    {
        if (!itemstack.Attributes.HasAttribute(ModAttributes.Guid))
            return;

        var modAttributes = itemstack.Attributes.GetTreeAttribute(ModAttributes.Guid);

        if (!modAttributes.HasAttribute(ModAttributes.Rarity) || !modAttributes.HasAttribute(ModAttributes.MiningSpeed))
            return;

        var miningSpeed = modAttributes.GetTreeAttribute(ModAttributes.MiningSpeed);

        __result = miningSpeed.GetFloat(block.BlockMaterial.ToString(), __result);
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
    public static void PatchConsumeCraftingIngredients(CollectibleObject __instance, ItemSlot[] slots, ItemSlot outputSlot, GridRecipe matchingRecipe)
    {
        var itemStack = outputSlot.Itemstack;

        if (itemStack == null || itemStack.Item?.Tool == null || itemStack.Attributes.HasAttribute(ModAttributes.Guid))
            return;

        var rarity = ModCore.GetRandomRarity();

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
    public static void ReversePatchGetHeldItemInfo(CollectibleObject __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
    }

    /// <summary>
    /// Modifies the item's tooltip to enhance mining speed information by adding details based on its rarity.
    /// </summary>
    /// <param name="sb">
    /// A <c>StringBuilder</c> containing the item's tooltip description, which will be updated with mining speed information based on its rarity.
    /// </param>
    /// <param name="miningSpeeds">
    /// An <c>ITreeAttribute</c> containing mining speed factors for various block materials. Each key represents a block material,
    /// and its corresponding value represents the mining speed factor.
    /// </param>
    private static void FixMiningSpeedInfos(StringBuilder sb, ITreeAttribute miningSpeeds)
    {
        var lines = sb.ToString().Trim().Split(Environment.NewLine);
        var miningSpeedLine = Lang.Get("item-tooltip-miningspeed") ?? string.Empty;

        // Find the line that starts with the mining speed line
        var foundLine = Array.FindIndex(lines, line => line.StartsWith(miningSpeedLine, StringComparison.Ordinal));

        if (foundLine < 0)
            return;

        var lineSpan = lines[foundLine].AsSpan();
        var includedLineStart = lineSpan.LastIndexOf('x'); // Find the last x (ex: 1.3x, 5.2x)
        var includedLineTotal = includedLineStart >= 0 ? lineSpan.Slice(includedLineStart + 1).ToString() : string.Empty;

        sb.Clear();

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

            sb.AppendLine(includedLineTotal);
        }
    }
}