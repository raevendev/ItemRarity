using System;
using System.Text;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

[HarmonyPatch(typeof(ItemShield))]
public static class ItemShieldPatch
{
    [HarmonyPostfix, HarmonyPatch(nameof(ItemShield.GetHeldItemName)), HarmonyPriority(Priority.Last)]
    public static void GetHeldItemNamePatch(ItemShield __instance, ItemStack itemStack, ref string __result)
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

    [HarmonyPrefix, HarmonyPatch(nameof(ItemShield.GetHeldItemInfo)), HarmonyPriority(Priority.Last)]
    public static bool GetHeldItemInfoPatch(ItemShield __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        var itemstack = inSlot.Itemstack;

        if (!ModRarity.TryGetRarityTreeAttribute(itemstack, out var modAttribute) || !modAttribute.HasAttribute(ModAttributes.Rarity))
            return true;

        var itemAttribute = itemstack.ItemAttributes?["shield"];

        if (itemAttribute == null || !itemAttribute.Exists)
            return true;

        if (itemAttribute["protectionChance"]["active-projectile"].Exists)
        {
            var num1 = itemAttribute["protectionChance"]["active-projectile"].AsFloat();
            var num2 = itemAttribute["protectionChance"]["passive-projectile"].AsFloat();
            var num3 = Math.Round(modAttribute.GetFloat(ModAttributes.ShieldProjectileDamageAbsorption), 2);
            dsc.AppendLine("<strong>" + Lang.Get("Projectile protection") + "</strong>");
            dsc.AppendLine(Lang.Get("shield-stats", (int)(100.0 * num1), (int)(100.0 * num2), num3));
            dsc.AppendLine();
        }

        var num4 = Math.Round(modAttribute.GetFloat(ModAttributes.ShieldDamageAbsorption), 2);
        var num5 = itemAttribute["protectionChance"]["active"].AsFloat();
        var num6 = itemAttribute["protectionChance"]["passive"].AsFloat();
        dsc.AppendLine("<strong>" + Lang.Get("Melee attack protection") + "</strong>");
        dsc.AppendLine(Lang.Get("shield-stats", (int)(100.0 * num5), (int)(100.0 * num6), num4));
        dsc.AppendLine();
        switch (__instance.Construction)
        {
            case "woodmetal":
                dsc.AppendLine(Lang.Get("shield-woodtype", Lang.Get("material-" + itemstack.Attributes.GetString("wood"))));
                dsc.AppendLine(Lang.Get("shield-metaltype", Lang.Get("material-" + itemstack.Attributes.GetString("metal"))));
                break;
            case "woodmetalleather":
                dsc.AppendLine(Lang.Get("shield-metaltype", Lang.Get("material-" + itemstack.Attributes.GetString("metal"))));
                break;
        }

        return false;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ItemShield.GetMaxDurability)), HarmonyPriority(Priority.Last)]
    public static void GetMaxDurabilityPatch(ItemShield __instance, ItemStack itemstack, ref int __result)
    {
        if (!ModRarity.TryGetRarityTreeAttribute(itemstack, out var modAttribute))
            return;

        if (!modAttribute.HasAttribute(ModAttributes.Rarity) || !modAttribute.HasAttribute(ModAttributes.MaxDurability))
            return;

        __result = modAttribute.GetInt(ModAttributes.MaxDurability);
    }
}