using System;
using ItemRarity.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace ItemRarity.Extensions;

/// <summary>
/// Contains extension methods for working with <see cref="ItemStack"/> objects.
/// </summary>
public static class ItemStackExtensions
{
    /// <summary>
    /// Sets the rarity of the specified <see cref="ItemStack"/> by updating its attributes based on the provided rarity key.
    /// </summary>
    /// <param name="itemStack">The <see cref="ItemStack"/> to which the rarity should be applied.</param>
    /// <param name="rarity">The key representing the rarity to be applied to the <see cref="ItemStack"/>.</param>
    /// <returns>
    /// A tuple containing the key and corresponding <see cref="ItemRarityConfig"/> of the applied rarity.
    /// </returns>
    /// <exception cref="Exception">Thrown if the <see cref="ItemStack"/> does not have a valid collectible item.</exception>
    public static (string Key, ItemRarityConfig Value) SetRarity(this ItemStack itemStack, string rarity)
    {
        if (itemStack.Collectible == null)
            throw new Exception("Missing Collectible in itemStack.");

        var itemRarity = ModCore.Config[rarity];
        var modAttributes = itemStack.Attributes.GetOrAddTreeAttribute(ModAttributes.Guid);

        modAttributes.SetString(ModAttributes.Rarity, itemRarity.Key); // Use the key as the rarity.

        if (itemStack.Collectible.Durability > 0) // Set max durability
        {
            modAttributes.SetInt(ModAttributes.MaxDurability, (int)(itemStack.Collectible.Durability * itemRarity.Value.DurabilityMultiplier));
        }

        if (itemStack.Collectible.MiningSpeed != null && itemStack.Collectible.MiningSpeed.Count > 0) // Set mining speed
        {
            var miningSpeed = modAttributes.GetOrAddTreeAttribute(ModAttributes.MiningSpeed);
            foreach (var kv in itemStack.Collectible.MiningSpeed)
            {
                miningSpeed.SetFloat(kv.Key.ToString(), kv.Value * GlobalConstants.ToolMiningSpeedModifier * itemRarity.Value.MiningSpeedMultiplier);
            }
        }

        if (itemStack.Collectible.Attributes.KeyExists("damage")) // Set piercing damages
        {
            var damages = itemStack.Collectible.Attributes["damage"].AsFloat();
            modAttributes.SetFloat(ModAttributes.PiercingPower, damages * itemRarity.Value.PiercingPowerMultiplier);
        }

        if (itemStack.Collectible.AttackPower > 1f) // Set attack power
        {
            modAttributes.SetFloat(ModAttributes.AttackPower, itemStack.Collectible.AttackPower * itemRarity.Value.AttackPowerMultiplier);
        }

        return itemRarity;
    }
}