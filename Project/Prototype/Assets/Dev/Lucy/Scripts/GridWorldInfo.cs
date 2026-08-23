using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct GridWorldInfo
{
    public readonly int minFertilityToSpread;
    public readonly int maxFertility;
    public readonly int minWaterForFertility;
    public readonly int minGroundWaterToSpread;
    public readonly int maxGroundWater;

}
