using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ItemRarity.Logging;
using ItemRarity.Rarities;
using Vintagestory.API.Common;

// ReSharper disable MemberCanBePrivate.Global

namespace ItemRarity.Tiers;

public static class Tier
{
    public static IEnumerable<(RarityModel, float Value)> GetRaritiesByTier(TierModel tierModel, Predicate<RarityModel>? includeRarity = null)
    {
        return (includeRarity != null
            ? tierModel.Rarities.Select(r => (ModCore.Config.Rarity[r.Key], r.Value)).Where(r => includeRarity(r.Item1!))
            : tierModel.Rarities.Select(r => (ModCore.Config.Rarity[r.Key], r.Value)))!;
    }

    public static RarityModel GetRandomRarityByTier(TierModel tierModel, IEnumerable<(RarityModel, float Value)>? rarities = null)
    {
        var tierRarities = (rarities ?? GetRaritiesByTier(tierModel)).ToImmutableArray();
        var totalWeight = tierRarities.Sum(i => i.Value);
        var randomValue = Random.Shared.NextDouble() * totalWeight;
        var cumulativeWeight = 0f;

        foreach (var rarity in tierRarities)
        {
            cumulativeWeight += rarity.Value;
            if (randomValue < cumulativeWeight)
            {
                return rarity.Item1;
            }
        }

        Logger.Error("Failed to get random rarity by tier");

        return ModCore.Config.Rarity.Rarities.First().Value;
    }

    public static void ApplyTier(ItemStack itemStack, int tierLevel)
    {
        if (ModCore.Config.Tier.TryGetTier(tierLevel, out var tierModel))
            ApplyTier(itemStack, tierModel);
        else
            Logger.Error($"Failed to get tier for level {tierLevel}");
    }

    public static void ApplyTier(ItemStack itemStack, TierModel tierModel)
    {
        var tierRarity = GetRandomRarityByTier(tierModel);
        Rarity.ApplyRarity(itemStack, tierRarity);
    }

    public static void ApplyTierUpgrade(ItemStack inputItem, ItemStack outputItem, int tierLevel)
    {
        if (ModCore.Config.Tier.TryGetTier(tierLevel, out var tierModel))
            ApplyTierUpgrade(inputItem, outputItem, tierModel);
        else
            Logger.Error($"Failed to get tier for level {tierLevel}");
    }

    public static void ApplyTierUpgrade(ItemStack inputItem, ItemStack outputItem, TierModel tierModel)
    {
        if (!Rarity.TryGetRarity(inputItem, out var currentRarity))
        {
            Logger.Warning($"Failed to upgrade Rarity for item {inputItem.Collectible.Code}");
            return;
        }

        var rarities = GetRaritiesByTier(tierModel, model => model.Level > currentRarity.Level);
        var upgradedRarity = GetRandomRarityByTier(tierModel, rarities);
        Rarity.ApplyRarity(outputItem, upgradedRarity);
    }
}