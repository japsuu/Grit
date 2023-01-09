namespace Grit.Simulation;

public static class MathG
{
    public static float Lerp(float a, float b, float t)
    {
        return a * (1 - t) + b * t;
    }
}