using System.Collections.Generic;

namespace Grit.Simulation.World;

/// <summary>
/// Represents a <see cref="SIZE_IN_CHUNKS"/> * <see cref="SIZE_IN_CHUNKS"/> area of chunks.
/// </summary>
public class Region
{
    // Size in one axis
    private const int SIZE_IN_CHUNKS = 4;
    private const int DIMENSIONS = SIZE_IN_CHUNKS * Settings.WORLD_CHUNK_SIZE;

    public readonly Vector2Int WorldPosition;

    public readonly Vector2Int[] ContainedChunks;

    public Region(Vector2Int worldPosition)
    {
        WorldPosition = worldPosition;
        
        int minX = worldPosition.x;
        int minY = worldPosition.y;
        int maxX = minX + DIMENSIONS;
        int maxY = minY + DIMENSIONS;

        ContainedChunks = new Vector2Int[SIZE_IN_CHUNKS * SIZE_IN_CHUNKS];

        int index = 0;
        for (int y = minY; y < maxY; y += Settings.WORLD_CHUNK_SIZE)
        {
            for (int x = minX; x < maxX; x += Settings.WORLD_CHUNK_SIZE)
            {
                ContainedChunks[index] = new Vector2Int(x, y);
                index++;
            }
        }
    }
}