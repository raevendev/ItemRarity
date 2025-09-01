using SysRdm = System.Random;

namespace ItemRarity.Extensions;

public static class ArrayExtensions
{
    public static float RandomBetween(this float[] array, float defaultValue = 1)
    {
        return array.Length switch
        {
            0 => defaultValue,
            1 => array[0],
            _ => SysRdm.Shared.NextSingle() * (array[^1] - array[0]) + array[0]
        };
    }
}