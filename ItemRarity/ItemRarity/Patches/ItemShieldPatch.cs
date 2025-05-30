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
        if (!Rarity.TryGetRarityInfos(itemStack, out var rarityInfos))
            return;

        var rarityName = rarityInfos.Value.IgnoreTranslation
            ? $"[{rarityInfos.Value.Name}]"
            : Lang.GetWithFallback($"itemrarity:{rarityInfos.Key}", "itemrarity:unknown", rarityInfos.Value.Name);

        if (__result.Contains(rarityName))
            return;

        __result = $"<font color=\"{rarityInfos.Value.Color}\" weight=bold>{rarityName} {__result}</font>";
    }

    [HarmonyPrefix, HarmonyPatch(nameof(ItemShield.GetHeldItemInfo)), HarmonyPriority(Priority.Last)]
    public static bool GetHeldItemInfoPatch(ItemShield __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        if (inSlot is not { Itemstack: not null })
            return true;

        if (!Rarity.TryGetRarityInfos(inSlot.Itemstack, out var rarityInfos))
            return true;

        var itemAttribute = inSlot.Itemstack.ItemAttributes?["shield"];

        if (itemAttribute == null || !itemAttribute.Exists)
            return true;

        CollectibleObjectPatch.GetHeldItemInfoReversePatch(__instance, inSlot, dsc, world, withDebugInfo);

        if (itemAttribute["protectionChance"]["active-projectile"].Exists)
        {
            var num1 = itemAttribute["protectionChance"]["active-projectile"].AsFloat();
            var num2 = itemAttribute["protectionChance"]["passive-projectile"].AsFloat();
            var num3 = itemAttribute["projectileDamageAbsorption"].AsFloat(2F) * rarityInfos.Value.ShieldProtectionMultiplier;
            dsc.AppendLine("<strong>" + Lang.Get("Projectile protection") + "</strong>");
            dsc.AppendLine(Lang.Get("shield-stats", (int)(100.0 * num1), (int)(100.0 * num2), num3.ToString("#.#")));
            dsc.AppendLine();
        }

        var num4 = itemAttribute["damageAbsorption"].AsFloat(2F) * rarityInfos.Value.ShieldProtectionMultiplier;
        var num5 = itemAttribute["protectionChance"]["active"].AsFloat();
        var num6 = itemAttribute["protectionChance"]["passive"].AsFloat();
        dsc.AppendLine("<strong>" + Lang.Get("Melee attack protection") + "</strong>");
        dsc.AppendLine(Lang.Get("shield-stats", (int)(100.0 * num5), (int)(100.0 * num6), num4.ToString("#.#")));
        dsc.AppendLine();
        switch (__instance.Construction)
        {
            case "woodmetal":
                dsc.AppendLine(Lang.Get("shield-woodtype", Lang.Get("material-" + inSlot.Itemstack.Attributes.GetString("wood"))));
                dsc.AppendLine(Lang.Get("shield-metaltype", Lang.Get("material-" + inSlot.Itemstack.Attributes.GetString("metal"))));
                break;
            case "woodmetalleather":
                dsc.AppendLine(Lang.Get("shield-metaltype", Lang.Get("material-" + inSlot.Itemstack.Attributes.GetString("metal"))));
                break;
        }

        return false;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ItemShield.GetMaxDurability)), HarmonyPriority(Priority.Last)]
    public static void GetMaxDurabilityPatch(ItemShield __instance, ItemStack itemstack, ref int __result)
    {
        if (!Rarity.TryGetRarityTreeAttribute(itemstack, out var modAttribute))
            return;

        if (!modAttribute.HasAttribute(ModAttributes.Rarity) || !modAttribute.HasAttribute(ModAttributes.MaxDurability))
            return;

        __result = modAttribute.GetInt(ModAttributes.MaxDurability);
    }
}