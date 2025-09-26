using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

// ReSharper disable MemberCanBePrivate.Global

namespace ItemRarity.Attributes;

public static class Attribute
{
    public const string ModAttributeId = "itemrarity";

    public const string Rarity = "rarity";
    public const string Tier = "tier";

    public static readonly AttributeAccessor MaxDurabilityMultiplier = new("maxdurabilitymul");
    public static readonly AttributeAccessor MiningSpeedMultiplier = new("miningspeedmul");
    public static readonly AttributeAccessor AttackPowerMultiplier = new("attackpowermul");

    public static readonly AttributeAccessor PiercingPowerMultiplier = new("piercingdamagemul");
    // public static readonly AttributeAccessor ProtectionModifiers = new("protectionmodifiers");

    // public static readonly AttributeAccessor ArmorFlatDamageReduction = new("armorflatdamagereduction");
    public static readonly AttributeAccessor ArmorFlatDamageReductionMultiplier = new("armorflatdamagereductionmul");
    public static readonly AttributeAccessor ArmorPerTierFlatDamageProtectionLossMultiplier = new("armorpertierflatdamagereductionlossmul");

    // public static readonly AttributeAccessor ArmorRelativeProtection = new("armorrelativeprotection");
    // public static readonly AttributeAccessor ArmorRelativeProtectionMultiplier = new("armorrelativeprotectionmul");

    public static readonly AttributeAccessor ShieldDamageAbsorptionMultiplier = new("shielddamageabsorptionmul");
    public static readonly AttributeAccessor ShieldProjectileDamageAbsorptionMultiplier = new("shieldprojectiledamageabsorptionmul");

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
}