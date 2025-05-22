using HarmonyLib;
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
    [HarmonyPrefix, HarmonyPatch("impactOnEntity"), HarmonyPriority(Priority.Last)]
    public static void ImpactEntityPatch(EntityProjectile __instance, Entity entity)
    {
        if (ModCore.WeatherSystemServer == null || !Rarity.TryGetRarityInfos(__instance.ProjectileStack, out var rarityInfos))
            return;

        if (__instance.DamageType == EnumDamageType.PiercingAttack)
        {
            __instance.Damage *= rarityInfos.Value.PiercingPowerMultiplier;
        }

        if (rarityInfos.Value.HasEffect("thor"))
        {
            var hitPoint = entity.Pos;

            ModCore.WeatherSystemServer.SpawnLightningFlash(hitPoint.XYZ);

            entity.IsOnFire = true;
        }
    }
}