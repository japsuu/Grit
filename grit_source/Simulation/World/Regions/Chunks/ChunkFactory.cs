
using Grit.Simulation.Helpers;
using Microsoft.Xna.Framework;

namespace Grit.Simulation.World.Regions.Chunks;

public static class ChunkFactory
{
    private static Simulation hostSimulation;

    public static void Initialize(Simulation simulation)
    {
        hostSimulation = simulation;
    }

    public static Chunk GenerateChunk(Point worldPosition)
    {
        Chunk chunk = new(worldPosition, Settings.WORLD_CHUNK_SIZE, hostSimulation);

        return chunk;
    }
}