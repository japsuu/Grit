using System;
using Microsoft.Xna.Framework;

namespace Grit.Simulation;

// private static bool[] fastPregenBools;
// private static int fastPregenBoolIndex;
// fastPregenBools = new bool[Settings.WORLD_WIDTH * Settings.WORLD_HEIGHT];
// for (int i = 0; i < fastPregenBools.Length; i++)
// {
//     fastPregenBools[i] = SeedlessRandom.NextDouble() < 0.5d;
// }

public static class RandomFactory
{
    public static Random SeededRandom;
    public static Random SeedlessRandom;

    public static FastNoiseLite SeedlessFnl;
    
    public static void Initialize(int seed)
    {
        SeededRandom = new Random(seed);
        SeedlessRandom = new Random();

        SeedlessFnl = new FastNoiseLite();
        
        SeedlessFnl.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        SeedlessFnl.SetFrequency(0.02f);
    }
    
    

    /// <summary>
    /// Returns random color between min and max.
    /// </summary>
    public static Color RandomColor(Color min, Color max)
    {
        int r = SeedlessRandom.Next(min.R, max.R);
        int g = SeedlessRandom.Next(min.G, max.G);
        int b = SeedlessRandom.Next(min.B, max.B);
        int a = SeedlessRandom.Next(min.A, max.A);

        return new Color(r, g, b, a);
    }

    /// <summary>
    /// Returns random color between min and max, based on Fast Noise Lite -noise.
    /// </summary>
    public static Color RandomColorFnl(Color min, Color max, int x, int y)
    {
        float noise = GetNoise(x, y);
        int r = (int)MathG.Lerp(min.R, max.R, noise);
        int g = (int)MathG.Lerp(min.G, max.G, noise);
        int b = (int)MathG.Lerp(min.B, max.B, noise);
        int a = (int)MathG.Lerp(min.A, max.A, noise);

        return new Color(r, g, b, a);
    }

    /// <summary>
    /// Returns noise value between 0-1.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static float GetNoise(int x, int y) => (SeedlessFnl.GetNoise(x, y) + 1) / 2;

    public static bool RandomBool() => SeedlessRandom.NextDouble() < 0.5;
}