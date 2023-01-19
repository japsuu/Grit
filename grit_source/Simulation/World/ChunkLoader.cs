using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Grit.Simulation.World;

public class ChunkLoader
{
    private Dictionary<Vector2Int, Chunk> LoadedChunks;

    public void Update(Vector2 cameraPos)
    {
        // Loop region positions around camera position.
        // If an region is missing from an position, create it. Load the region's chunks from memory.
        // If the chunks have not been visited before, generate them.
        // If an region is too far from the player, save it to the disk and unload it.
    }
    
    private void SaveRegionToDisk(Region region)
    {
        // Remove the chunks contained in region from LoadedChunks.
    }
    
    private Region LoadRegionFromDisk(Vector2Int regionPos)
    {
        // Add the chunks contained in region to LoadedChunks.
        return null;
    }
}