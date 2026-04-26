using System.Text;
using HarmonyLib;
using ItemRarity.Attributes;
using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

[HarmonyPatch]
public static class ItemShieldPatch
{
    [HarmonyPatch(typeof(ItemShield), nameof(ItemShield.GetHeldItemInfo)), HarmonyPrefix, HarmonyPriority(Priority.Last)]
    public static bool ItemShield_GetHeldItemInfoPatch(ItemShield __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        if (inSlot is not { Itemstack: not null })
            return true;

        if (!Rarity.TryGetRarity(inSlot.Itemstack, out _))
            return true;

        var itemAttribute = inSlot.Itemstack.ItemAttributes?["shield"];

        if (itemAttribute is not { Exists: true })
            return true;

        CollectibleObjectPatch.GetHeldItemInfoReversePatch(__instance, inSlot, dsc, world, withDebugInfo);

        if (itemAttribute["protectionChance"]["active-projectile"].Exists)
        {
            var projectileDamageAbsorptionMul = Attribute.ShieldProjectileDamageAbsorptionMultiplier.GetFloat(inSlot.Itemstack, 1f);
            var num1 = itemAttribute["protectionChance"]["active-projectile"].AsFloat();
            var num2 = itemAttribute["protectionChance"]["passive-projectile"].AsFloat();
            var num3 = itemAttribute["projectileDamageAbsorption"].AsFloat(2F) * projectileDamageAbsorptionMul;
            dsc.AppendLine("<strong>" + Lang.Get("Projectile protection") + "</strong>");
            dsc.AppendLine(Lang.Get("shield-stats", (int)(100.0 * num1), (int)(100.0 * num2), num3.ToString("#.#")));
            dsc.AppendLine();
        }

        var damageAbsorptionMul = Attribute.ShieldDamageAbsorptionMultiplier.GetFloat(inSlot.Itemstack, 1f);
        var num4 = itemAttribute["damageAbsorption"].AsFloat(2F) * damageAbsorptionMul;
        var num5 = itemAttribute["protectionChance"]["active"].AsFloat();
        var num6 = itemAttribute["protectionChance"]["passive"].AsFloat();
        dsc.AppendLine("<strong>" + Lang.Get("Melee attack protection") + "</strong>");
        dsc.AppendLine(Lang.Get("shield-stats", (int)(100.0 * num5), (int)(100.0 * num6), num4.ToString("#.#")));
        dsc.AppendLine();

        return false;
    }
}