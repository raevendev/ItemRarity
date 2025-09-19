using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

// ReSharper disable MemberCanBePrivate.Global

namespace ItemRarity;

public static class AttributesManager
{
    public const string ModAttributeId = "itemrarity";

    public const string Rarity = "rarity";

    public const string MaxDurabilityMultiplier = "maxdurabilitymul";
    public const string MiningSpeedMultiplier = "miningspeedmul";
    public const string AttackPowerMultiplier = "attackpowermul";

    public const string PiercingPowerMultiplier = "piercingdamagemul";
    public const string ProtectionModifiers = "protectionmodifiers";

    public const string ArmorFlatDamageReduction = "armorflatdamagereduction";
    public const string ArmorFlatDamageReductionMultiplier = "armorflatdamagereductionmul";
    public const string ArmorPerTierFlatDamageProtectionLossMultiplier = "armorpertierflatdamagereductionlossmul";

    public const string ArmorRelativeProtection = "armorrelativeprotection";
    public const string ArmorRelativeProtectionMultiplier = "armorrelativeprotectionmul";

    // public const string ArmorPerTierRelativeProtectionLoss = "armorpertierrelativeprotectionloss";
    // public const string ArmorPerTierFlatDamageReductionLoss = "armorpertierflatdamagereductionloss";
    // public const string ArmorPerTierFlatDamageReductionLossMultiplier = "armorpertierflatdamagereductionlossmul";
    public const string ShieldDamageAbsorptionMultiplier = "shielddamageabsorptionmul";
    public const string ShieldProjectileDamageAbsorptionMultiplier = "shieldprojectiledamageabsorptionmul";

    public static bool TryGetRarityTreeAttribute(ItemStack? itemStack, out ITreeAttribute treeAttribute)
    {
        if (itemStack?.Attributes == null || itemStack.Collectible?.Attributes == null)
        {
            treeAttribute = null!;
            return false;
        }

        treeAttribute = itemStack.Attributes.GetTreeAttribute(ModAttributeId);
        return treeAttribute != null;
    }

    public static float GetStatsMultiplier(ItemStack? itemStack, string attributeKey, float defaultValue = 1F)
    {
        if (itemStack == null)
            return defaultValue;
        return TryGetRarityTreeAttribute(itemStack, out var attribute) ? defaultValue : attribute.GetFloat(attributeKey, defaultValue);
    }
}