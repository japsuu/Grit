using Grit.Simulation.Helpers;
using Microsoft.Xna.Framework;

namespace Grit.Simulation.World.Regions;

/// <summary>
/// Represents a <see cref="SIZE_IN_CHUNKS"/> * <see cref="SIZE_IN_CHUNKS"/> area of chunks.
/// </summary>
public class Region
{
    // Size in one axis
    private const int SIZE_IN_CHUNKS = 4;
    private const int DIMENSIONS = SIZE_IN_CHUNKS * Settings.WORLD_CHUNK_SIZE;

    public readonly Point WorldPosition;

    public readonly Point[] ContainedChunks;

    public Region(Point worldPosition)
    {
        WorldPosition = worldPosition;
        
        int minX = worldPosition.X;
        int minY = worldPosition.Y;
        int maxX = minX + DIMENSIONS;
        int maxY = minY + DIMENSIONS;

        ContainedChunks = new Point[SIZE_IN_CHUNKS * SIZE_IN_CHUNKS];

        int index = 0;
        for (int y = minY; y < maxY; y += Settings.WORLD_CHUNK_SIZE)
        {
            for (int x = minX; x < maxX; x += Settings.WORLD_CHUNK_SIZE)
            {
                ContainedChunks[index] = new Point(x, y);
                index++;
            }
        }
    }
}