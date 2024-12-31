using System;
using ItemRarity.Config;
using Vintagestory.API.Common;

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
    public static ItemRarityInfos SetRarity(this ItemStack itemStack, string rarity)
    {
        return ModRarity.SetRarity(itemStack, rarity);
    }
}