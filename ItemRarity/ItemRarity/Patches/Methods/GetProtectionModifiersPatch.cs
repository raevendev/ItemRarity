using HarmonyLib;
using ItemRarity.Attributes;
using ItemRarity.Rarities;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches.Methods;

[HarmonyPatch]
public static class GetProtectionModifiersPatch
{
    /// <summary>
    /// Postfix patch to inject rarity modifiers into the protection values of wearable items.
    /// Runs last to ensure custom stats are applied after all base calculations.
    /// </summary>
    /// <param name="__instance">The wearable item instance being processed.</param>
    /// <param name="slot">The equipment slot where the item is located.</param>
    /// <param name="__result">The protection modifiers to be updated with rarity bonuses.</param>
    [HarmonyPatch(typeof(CollectibleBehaviorWearable), nameof(CollectibleBehaviorWearable.GetProtectionModifiers)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void CollectibleBehaviorWearable_GetAttackPowerPatch(CollectibleBehaviorWearable __instance, ItemSlot slot, ref ProtectionModifiers __result)
    {
        if (slot is not { Itemstack: not null })
            return;

        if (!Rarity.TryGetRarity(slot.Itemstack, out _))
            return;

        var protectionModifiers = new ProtectionModifiers // TODO: For now, i created a new instance each time, this is really not ideal.
        {
            ProtectionTier = __result.ProtectionTier,
            HighDamageTierResistant = __result.HighDamageTierResistant,
            RelativeProtection = __result.RelativeProtection,
            FlatDamageReduction = __result.FlatDamageReduction * Attribute.ArmorFlatDamageReductionMultiplier.GetFloat(slot.Itemstack, 1f),
            PerTierFlatDamageReductionLoss = new float[__result.PerTierFlatDamageReductionLoss.Length],
            PerTierRelativeProtectionLoss = new float[__result.PerTierRelativeProtectionLoss.Length],
        };

        for (var i = 0; i < __result.PerTierFlatDamageReductionLoss.Length; i++)
            protectionModifiers.PerTierFlatDamageReductionLoss[i] = __result.PerTierFlatDamageReductionLoss[i] *
                                                                    Attribute.ArmorPerTierFlatDamageProtectionLossMultiplier.GetFloat(slot.Itemstack, 1f);

        __result = protectionModifiers;
    }
}