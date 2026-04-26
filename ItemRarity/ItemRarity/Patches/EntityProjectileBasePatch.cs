using HarmonyLib;
using ItemRarity.Attributes;
using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

[HarmonyPatch(typeof(EntityProjectileBase))]
public static class EntityProjectileBasePatch
{
    [HarmonyPatch("ApplyModifiers"), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void ApplyModifiers(EntityProjectileBase __instance, ref float damage, Entity target)
    {
        if (__instance.DamageType != EnumDamageType.PiercingAttack || __instance.WeaponStack is null)
            return;

        if (!Rarity.TryGetRarity(__instance.WeaponStack, out _))
            return;

        damage *= Attribute.PiercingPowerMultiplier.GetFloat(__instance.ProjectileStack, 1f);
    }
    
    [HarmonyPatch("DealDamage"), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void DealDamage(EntityProjectileBase __instance, Entity target, bool __result)
    {
        if (!__result || __instance.DamageType != EnumDamageType.PiercingAttack || __instance.WeaponStack is null)
            return;

        if (!Rarity.TryGetRarity(__instance.WeaponStack, out var rarityModel))
            return;

        if (rarityModel.HasEffect("thor") && ModCore.WeatherSystemServer != null)
        {
            var hitPoint = target.Pos;

            ModCore.WeatherSystemServer.SpawnLightningFlash(hitPoint.XYZ);

            target.IsOnFire = true;
        }
    }
}