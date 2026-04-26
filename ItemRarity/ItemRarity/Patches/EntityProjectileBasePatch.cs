using HarmonyLib;
using ItemRarity.Attributes;
using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

/// <summary>
/// A Harmony patch class for modifying the behavior of the <see cref="EntityProjectile"/> class.
/// </summary>
[HarmonyPatch]
public static class EntityProjectileBasePatch
{
    /// <summary>
    /// Applies rarity-based piercing power multipliers to projectile damage.
    /// </summary>
    /// <param name="__instance">The projectile instance being processed.</param>
    /// <param name="damage">The damage value to be modified.</param>
    /// <param name="target">The entity being hit by the projectile.</param>
    [HarmonyPatch(typeof(EntityProjectileBase), "ApplyModifiers"), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void EntityProjectileBase_ApplyModifiers(EntityProjectileBase __instance, ref float damage, Entity target)
    {
        if (__instance.DamageType != EnumDamageType.PiercingAttack || __instance.WeaponStack is null)
            return;

        if (!Rarity.TryGetRarity(__instance.WeaponStack, out _))
            return;

        damage *= Attribute.PiercingPowerMultiplier.GetFloat(__instance.ProjectileStack, 1f);
    }

    /// <summary>
    /// Triggers special rarity effects (e.g., lightning strike) on successful projectile hits.
    /// </summary>
    /// <param name="__instance">The projectile instance that hit the target.</param>
    /// <param name="target">The entity that was hit.</param>
    /// <param name="__result">Whether the damage deal by the original method was successful.</param>
    [HarmonyPatch(typeof(EntityProjectileBase), "DealDamage"), HarmonyPostfix, HarmonyPriority(Priority.Last)]
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