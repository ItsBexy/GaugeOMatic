using static System.Math;

// ReSharper disable UnusedMember.Global

namespace GaugeOMatic.Utility;

public static class MiscMath
{
    /// <summary>
    /// calculate Y as a polynomial function of X. coefficient params are ordered from lowest to highest power.<br/>
    /// <remarks>helps indicators follow curved paths based on the tracker value. whats a bezier? never heard of it. go away</remarks>
    /// </summary>
    public static float PolyCalc(float x, params double[] coeff)
    {
        var result = 0f;
        for (var i = 0; i < coeff.Length; i++) result += (float)(Pow(x, i) * coeff[i]);
        return result;
    }

    public static float Radians(float deg) => deg * 0.0174532925199433F;
    public static float Degrees(float rad) => rad / 0.0174532925199433F;
}
