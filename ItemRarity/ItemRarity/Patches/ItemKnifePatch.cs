using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

[HarmonyPatch(typeof(ItemKnife))]
public static class ItemKnifePatch
{
    private static float CalculateHarvestingSpeed(float baseHarvestingSpeed)
    {
        return (float)(1.0 / ((baseHarvestingSpeed - 1.0) * 0.5 + 1.0));
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ItemKnife.OnHeldInteractStep))]
    public static void OnHeldInteractStepPatch(ItemKnife __instance, float secondsUsed, ItemSlot slot, EntityAgent byEntity,
        BlockSelection blockSel, EntitySelection entitySel, ref bool __result)
    {
        var itemStack = slot.Itemstack;

        if (!itemStack.Attributes.HasAttribute(ModAttributes.Guid))
            return;

        var modAttributes = itemStack.Attributes.GetTreeAttribute(ModAttributes.Guid);

        if (!modAttributes.HasAttribute(ModAttributes.Rarity) || !modAttributes.HasAttribute(ModAttributes.MiningSpeed))
            return;

        var miningSpeedAttribute = modAttributes.GetTreeAttribute(ModAttributes.MiningSpeed);
        var entityBehaviour = entitySel.Entity.GetBehavior<EntityBehaviorHarvestable>();
        var miningSpeed = CalculateHarvestingSpeed(miningSpeedAttribute.GetFloat(EnumBlockMaterial.Plant.ToString()))
            * entityBehaviour.GetHarvestDuration(byEntity) + 0.15000000596046448; // TODO: use a dedicated setting for that

        __result = secondsUsed < miningSpeed;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ItemKnife.OnHeldInteractStop))]
    public static void OnHeldInteractStopPatch(ItemKnife __instance, float secondsUsed, ItemSlot slot, EntityAgent byEntity,
        BlockSelection blockSel, EntitySelection entitySel)
    {
        var itemStack = slot.Itemstack;

        if (!itemStack.Attributes.HasAttribute(ModAttributes.Guid))
            return;

        var modAttributes = itemStack.Attributes.GetTreeAttribute(ModAttributes.Guid);

        if (!modAttributes.HasAttribute(ModAttributes.Rarity) || !modAttributes.HasAttribute(ModAttributes.MiningSpeed))
            return;

        var miningSpeedAttribute = modAttributes.GetTreeAttribute(ModAttributes.MiningSpeed);
        var entityBehaviour = entitySel.Entity.GetBehavior<EntityBehaviorHarvestable>();

        if (entityBehaviour == null || !entityBehaviour.Harvestable ||
            secondsUsed < CalculateHarvestingSpeed(miningSpeedAttribute.GetFloat(EnumBlockMaterial.Plant.ToString()))
            * entityBehaviour.GetHarvestDuration(byEntity) - 0.10000000149011612)
            return;

        entityBehaviour.SetHarvested(byEntity is EntityPlayer entityPlayer ? entityPlayer.Player : default);
        slot.Itemstack?.Collectible.DamageItem(byEntity.World, byEntity, slot, 3);
    }
}