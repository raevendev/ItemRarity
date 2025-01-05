﻿using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

/// <summary>
/// A Harmony patch class for modifying the behavior of the <see cref="EntityProjectile"/> class.
/// </summary>
[HarmonyPatch(typeof(EntityProjectile))]
public static class EntityProjectilePatch
{
    /// <summary>
    /// Modifies the behavior of a projectile's impact on an entity by applying rarity-based effects and enhancing damage properties.
    /// </summary>
    /// <param name="__instance">
    /// The instance of the <c>EntityProjectile</c> that is impacting the entity.</param>
    /// <param name="entity">The <c>Entity</c> being impacted by the projectile.</param>
    [HarmonyPrefix, HarmonyPatch("impactOnEntity"), HarmonyPriority(Priority.Last)]
    public static void ImpactEntityPatch(EntityProjectile __instance, Entity entity)
    {
        if (ModCore.WeatherSystemServer == null || !ModRarity.TryGetRarityTreeAttribute(__instance.ProjectileStack, out var modAttribute))
            return;

        if (__instance.DamageType == EnumDamageType.PiercingAttack)
        {
            __instance.Damage = modAttribute.GetFloat(ModAttributes.PiercingPower, 1f);
        }

        var itemRarity = ModCore.Config[modAttribute.GetString(ModAttributes.Rarity)];

        if (itemRarity.Value.HasEffect("thor"))
        {
            var hitPoint = entity.Pos;

            ModCore.WeatherSystemServer.SpawnLightningFlash(hitPoint.XYZ);

            entity.IsOnFire = true;
        }
    }
}