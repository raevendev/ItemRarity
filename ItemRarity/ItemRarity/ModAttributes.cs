using Vintagestory.API.Common;

namespace ItemRarity;

public static class ModAttributes
{
    public const string Guid = "FE327889-1DD2-430C-BCCA-D94FB132E968"; // Ensure (almost) unique tree attribute key

    public const string Rarity = "rarity";
    public const string MaxDurability = "maxdurability";
    public const string MiningSpeed = "miningspeed";
    public const string AttackPower = "attackpower";
    public const string PiercingPower = "piercingdamage";
    public const string ProtectionModifiers = "protectionmodifiers";
    public const string ArmorFlatDamageReduction = "armorflatdamagereduction";
    public const string ArmorRelativeProtection = "armorrelativeprotection";
    public const string ArmorPerTierRelativeProtectionLoss = "armorpertierrelativeprotectionloss";
    public const string ArmorPerTierFlatDamageReductionLoss = "armorpertierflatdamagereductionloss";
    public const string ShieldProjectileDamageAbsorption = "shieldprojectiledamageabsorption";
    public const string ShieldDamageAbsorption = "shielddamageabsorption";

    public static string GetRarity(ItemStack itemStack, string defaultValue = "unknown")
    {
        if (!ModRarity.TryGetRarityTreeAttribute(itemStack, out var attribute))
            return defaultValue;
        return attribute.GetString(Rarity);
    }

    public static ItemRarityInfos GetRarityInfo(ItemStack itemStack, string defaultValue = "unknown")
    {
        if (!ModRarity.TryGetRarityTreeAttribute(itemStack, out var attribute))
            return ModCore.Config[defaultValue];
        return ModCore.Config[attribute.GetString(Rarity, defaultValue)];
    }

    public static float GetMaxDurability(ItemStack itemStack, float defaultValue = 1F)
    {
        if (!ModRarity.TryGetRarityTreeAttribute(itemStack, out var attribute))
            return defaultValue;
        return attribute.GetFloat(MaxDurability, defaultValue);
    }

    public static float GetShieldProjectileDamageAbsorption(ItemStack itemStack, float defaultValue = 1F)
    {
        if (!ModRarity.TryGetRarityTreeAttribute(itemStack, out var attribute))
            return defaultValue;
        return attribute.GetFloat(ShieldProjectileDamageAbsorption, defaultValue);
    }

    public static float GetShieldDamageAbsorption(ItemStack itemStack, float defaultValue = 1F)
    {
        if (!ModRarity.TryGetRarityTreeAttribute(itemStack, out var attribute))
            return defaultValue;
        return attribute.GetFloat(ShieldDamageAbsorption, defaultValue);
    }
}