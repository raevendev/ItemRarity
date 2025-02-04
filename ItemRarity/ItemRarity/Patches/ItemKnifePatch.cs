using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

[HarmonyPatch(typeof(ItemKnife))]
public static class ItemKnifePatch
{
    private static float CalculateHarvestingSpeed(float baseHarvestingSpeed)
    {
        return (float)(1.0 / ((baseHarvestingSpeed - 1.0) * 0.5 + 1.0));
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ItemKnife.OnHeldInteractStep)), HarmonyPriority(Priority.Last)]
    public static void OnHeldInteractStepPatch(ItemKnife __instance, float secondsUsed, ItemSlot slot, EntityAgent byEntity,
        BlockSelection blockSel, EntitySelection entitySel, ref bool __result)
    {
        if (entitySel == null || slot is not { Itemstack: not null })
            return;

        if (!Rarity.TryGetRarityInfos(slot.Itemstack, out var rarityInfos))
            return;

        var entityBehaviour = entitySel.Entity.GetBehavior<EntityBehaviorHarvestable>();

        if (entityBehaviour is not { Harvestable: true })
            return;

        var miningSpeed = __instance.KnifeHarvestingSpeed / rarityInfos.Value.MiningSpeedMultiplier
                                                          * entityBehaviour.GetHarvestDuration(byEntity) + 0.15000000596046448f;
        __result = secondsUsed < miningSpeed;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ItemKnife.OnHeldInteractStop)), HarmonyPriority(Priority.Last)]
    public static void OnHeldInteractStopPatch(ItemKnife __instance, float secondsUsed, ItemSlot slot, EntityAgent byEntity,
        BlockSelection blockSel, EntitySelection entitySel)
    {
        if (entitySel == null || slot is not { Itemstack: not null })
            return;

        if (!Rarity.TryGetRarityInfos(slot.Itemstack, out var rarityInfos))
            return;

        var entityBehaviour = entitySel.Entity.GetBehavior<EntityBehaviorHarvestable>();

        if (entityBehaviour == null || !entityBehaviour.Harvestable ||
            secondsUsed < __instance.KnifeHarvestingSpeed / rarityInfos.Value.MiningSpeedMultiplier
                                                          * entityBehaviour.GetHarvestDuration(byEntity) - 0.10000000149011612)
            return;

        entityBehaviour.SetHarvested(byEntity is EntityPlayer entityPlayer ? entityPlayer.Player : default);
        slot.Itemstack?.Collectible.DamageItem(byEntity.World, byEntity, slot, 3);
    }
}