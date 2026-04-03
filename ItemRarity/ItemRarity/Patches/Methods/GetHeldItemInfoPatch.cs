using System;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using ItemRarity.Rarities;
using Vintagestory;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;
using Attribute = ItemRarity.Attributes.Attribute;
using Logger = ItemRarity.Logging.Logger;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches.Methods;

[HarmonyPatch]
public static class GetHeldItemInfoPatch
{
    [HarmonyReversePatch, HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetHeldItemInfo))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void CollectibleObject_GetHeldItemInfoReversePatch(CollectibleObject __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world,
        bool withDebugInfo)
    {
    }

    [HarmonyReversePatch, HarmonyPatch(typeof(ItemWearable), nameof(ItemWearable.GetHeldItemInfo))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ItemWearable_GetHeldItemInfoReversePatch(ItemWearable __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world,
        bool withDebugInfo)
    {
    }

    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetHeldItemInfo)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void CollectibleObject_GetHeldItemInfoPatch(CollectibleObject __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        if (inSlot is not { Itemstack: not null })
            return;

        if (!Rarity.TryGetRarity(inSlot.Itemstack, out var rarity))
            return;

        FixItemInfos(rarity, inSlot.Itemstack, __instance, dsc);
    }

    [HarmonyPatch(typeof(ItemSpear), nameof(ItemSpear.GetHeldItemInfo)), HarmonyPrefix, HarmonyPriority(Priority.Last)]
    public static bool ItemSpear_GetHeldItemInfoPatch(ItemSpear __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        if (!Rarity.TryGetRarity(inSlot.Itemstack, out _))
            return true;

        CollectibleObject_GetHeldItemInfoReversePatch(__instance, inSlot, dsc, world,
            withDebugInfo); // Call the original method to fill item infos before adding our own.

        var piercingDamages = 1.5f;

        if (inSlot.Itemstack.Collectible.Attributes != null)
            piercingDamages = inSlot.Itemstack.Collectible.Attributes["damage"].AsFloat();

        piercingDamages *= Attribute.PiercingPowerMultiplier.GetFloat(inSlot.Itemstack, 1f);

        dsc.AppendLine(piercingDamages + Lang.Get("piercing-damage-thrown"));

        return false;
    }

    [HarmonyPatch(typeof(ItemShield), nameof(ItemShield.GetHeldItemInfo)), HarmonyPrefix, HarmonyPriority(Priority.Last)]
    public static bool ItemShield_GetHeldItemInfoPatch(ItemShield __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        if (inSlot is not { Itemstack: not null })
            return true;

        if (!Rarity.TryGetRarity(inSlot.Itemstack, out _))
            return true;

        var itemAttribute = inSlot.Itemstack.ItemAttributes?["shield"];

        if (itemAttribute == null || !itemAttribute.Exists)
            return true;

        CollectibleObject_GetHeldItemInfoReversePatch(__instance, inSlot, dsc, world, withDebugInfo);

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

    [HarmonyPatch(typeof(ItemWearable), nameof(ItemWearable.GetHeldItemInfo)), HarmonyPrefix, HarmonyPriority(Priority.Last)]
    public static bool ItemWearable_GetHeldItemInfoPatch(ItemWearable __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        if (inSlot is not { Itemstack: not null })
            return true;

        if (!Rarity.TryGetRarity(inSlot.Itemstack, out _))
            return true;

        if (inSlot.Itemstack.Item is not ItemWearable wearable)
            return true;

        ItemWearable_GetHeldItemInfoReversePatch(__instance, inSlot, dsc, world, withDebugInfo);

        var flatProtectionMul = Attribute.ArmorFlatDamageReductionMultiplier.GetFloat(inSlot.Itemstack, -1f);

        if (flatProtectionMul > 0)
        {
            var flatProtectionLine = Lang.Get("Flat damage reduction: {0} hp", wearable.ProtectionModifiers.FlatDamageReduction) ?? string.Empty;
            var lines = dsc.ToString().Trim().Split(Environment.NewLine);
            var foundLine = Array.FindIndex(lines, line => line.StartsWith(flatProtectionLine, StringComparison.CurrentCultureIgnoreCase));

            dsc.Clear();

            for (var i = 0; i < lines.Length; i++)
            {
                if (i != foundLine)
                {
                    dsc.AppendLine(lines[i]);
                    continue;
                }

                dsc.AppendLine(Lang.Get("Flat damage reduction: {0} hp", (wearable.ProtectionModifiers.FlatDamageReduction * flatProtectionMul).ToString("F")));
            }
        }

        return false;
    }

    private static void FixItemInfos(RarityModel rarityModelInfos, ItemStack itemStack, CollectibleObject collectible, StringBuilder sb)
    {
        if (itemStack.Collectible.MiningSpeed is { Count: > 0 })
        {
            var lines = sb.ToString().Trim().Split(Environment.NewLine); // Split all lines

            sb.Clear();

            var miningSpeedLine = Lang.Get("item-tooltip-miningspeed") ?? string.Empty;
            var foundLine = Array.FindIndex(lines, line => line.StartsWith(miningSpeedLine, StringComparison.Ordinal));
            var miningSpeedMul = Attribute.MiningSpeedMultiplier.GetFloat(itemStack, 1f);

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
                        .Append((miningSpeed.Value * miningSpeedMul).ToString("#.#"))
                        .Append('x');
                }

                sb.AppendLine();

                if (collectible.GetAttackPower(itemStack) > 0.5)
                {
                    sb.AppendLine(Lang.Get("Attack power: -{0} hp", collectible.GetAttackPower(itemStack).ToString("0.#")));
                }
            }
        }
    }
}