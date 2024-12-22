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
    /// <summary>
    /// Modifies the behavior of a projectile's impact on an entity by applying rarity-based effects and enhancing damage properties.
    /// </summary>
    /// <param name="__instance">
    /// The instance of the <c>EntityProjectile</c> that is impacting the entity.</param>
    /// <param name="entity">The <c>Entity</c> being impacted by the projectile.</param>
    [HarmonyPrefix, HarmonyPatch("impactOnEntity")]
    public static void PatchImpactEntity(EntityProjectile __instance, Entity entity)
    {
        if (ItemRarityModSystem.WeatherSystemServer == null || !__instance.ProjectileStack.Attributes.HasAttribute(ModAttributes.Guid))
            return;

        var modAttributes = __instance.ProjectileStack.Attributes.GetTreeAttribute(ModAttributes.Guid);

        if (__instance.DamageType == EnumDamageType.PiercingAttack)
        {
            __instance.Damage = modAttributes.GetFloat(ModAttributes.PiercingPower, 1f);
        }

        var itemRarity = ItemRarityModSystem.Config[modAttributes.GetString(ModAttributes.Rarity)];

        if (itemRarity.Value.HasEffect("thor"))
        {
            var hitPoint = entity.Pos;

            ItemRarityModSystem.WeatherSystemServer.SpawnLightningFlash(hitPoint.XYZ);

            entity.IsOnFire = true;
        }
    }
}