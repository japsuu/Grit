using System;
using System.Collections.Generic;

namespace Grit.Simulation;

public static class MathG
{
    /// <summary>
    /// Returns the point between a and b based on the value of t.
    /// </summary>
    public static float Lerp(float a, float b, float t)
    {
        return a * (1 - t) + b * t;
    }
    
    
    /// <summary>
    /// Yields all positions of a straight line from [fromX,fromY] to [toX,toY].
    /// WARN: Returns ALL positions of the line, including the start and the end.
    /// Modified from: https://stackoverflow.com/a/11683720
    /// </summary>
    public static IEnumerable<(int x, int y)> IterateLine(int fromX, int fromY, int toX, int toY)
    {
        int width = toX - fromX;
        int height = toY - fromY;
        int dx1 = 0;
        int dy1 = 0;
        int dx2 = 0;
        int dy2 = 0;
        dx1 = width < 0 ? -1 : width > 0 ? 1 : dx1;
        dy1 = height < 0 ? -1 : height > 0 ? 1 : dy1;
        dx2 = width < 0 ? -1 : width > 0 ? 1 : dx2;

        int longest = Math.Abs(width);
        int shortest = Math.Abs(height);

        if (longest <= shortest)
        {
            longest = Math.Abs(height);
            shortest = Math.Abs(width);
            if (height < 0) dy2 = -1;
            else if (height > 0) dy2 = 1;
            dx2 = 0;
        }

        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            yield return (fromX, fromY);
            numerator += shortest;
            if (numerator >= longest)
            {
                numerator -= longest;
                fromX += dx1;
                fromY += dy1;
            }
            else
            {
                fromX += dx2;
                fromY += dy2;
            }
        }
    }
    
    
    public static int ManhattanDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
    }
}