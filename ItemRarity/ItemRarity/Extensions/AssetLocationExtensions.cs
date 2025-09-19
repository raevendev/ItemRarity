using System.Globalization;
using Vintagestory.API.Common;

namespace ItemRarity.Extensions;

public static class AssetLocationExtensions
{
    public static int EndVariantInteger(this AssetLocation assetLocation, int defaultValue = 0)
    {
        return int.TryParse(assetLocation.EndVariant(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }
}