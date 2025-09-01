using System;
using System.Linq;
using SysRdm = System.Random;

namespace ItemRarity.Models;

public sealed class RarityMultiplier
{
    public float Min { get; init; }
    public float Max { get; init; }

    public float Random
    {
        get
        {
            var value = Math.Abs(Min - Max) < 1e-5f ? Max : SysRdm.Shared.NextSingle() * (Max - Min) + Min;
            return value;
        }
    }

    public static implicit operator RarityMultiplier(float[] values)
    {
        if (values == null || values.Length == 0)
            throw new ArgumentException("Array must have at least one element.");

        return values.Length switch
        {
            1 => new RarityMultiplier { Min = values[0], Max = values[0] },
            _ => new RarityMultiplier { Min = values.Min(), Max = values.Max() }
        };
    }

    public static implicit operator RarityMultiplier(float value)
    {
        return new RarityMultiplier { Min = value, Max = value };
    }
}