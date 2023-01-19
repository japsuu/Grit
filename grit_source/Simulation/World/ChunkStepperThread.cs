using System.Threading;

namespace Grit.Simulation.World;

public class ChunkStepperThreadxxx
{
    public volatile bool HasProcessedData;
    
    private volatile int[] chunksToStep;
    private Thread thread;

    private readonly MultithreadedSimulation simulation;
    /// <summary>
    /// Shuffled array of access indexes for the element matrix.
    /// Prevents visually consistent updating of the world.
    /// </summary>
    //private int[] shuffledIndexes;

    private static bool IsPositionInsideMatrix(int x, int y) => x >= 0 && y >= 0 && x < Settings.WORLD_WIDTH && y < Settings.WORLD_HEIGHT;
    
    private double latestDeltaTime;
    private int mainThreadChunkIndex;
    //private volatile int stepperThreadChunkIndex;
    private volatile bool shouldStop;
    private volatile bool shouldProcessData;
    private SpinWait spinWait;

    public ChunkStepperThreadxxx(MultithreadedSimulation simulation, int maxSteppedChunks)
    {
        this.simulation = simulation;
        chunksToStep = new int[maxSteppedChunks];
        
        //InitializeShuffledIndexes();
    }

    /// <summary>
    /// Called from main thread.
    /// </summary>
    public void AddChunk(int chunkIndex)
    {
        chunksToStep[mainThreadChunkIndex] = chunkIndex;
        mainThreadChunkIndex++;
    }

    /// <summary>
    /// Called from main thread.
    /// </summary>
    public void StartStepChunks(double deltaTime)
    {
        latestDeltaTime = deltaTime;
        //stepperThreadChunkIndex = mainThreadChunkIndex;
        //mainThreadChunkIndex = 0;
        HasProcessedData = false;
        shouldProcessData = true;
    }

    public void Start()
    {
        shouldStop = false;
        thread = new Thread(ChunkProcessLoop);
        thread.Start();
    }

    public void Stop()
    {
        shouldStop = true;
    }

    private void ChunkProcessLoop()
    {
        while (!shouldStop)
        {
            // Do not 'halt and catch fire', use SpinWait!
            while (!shouldProcessData) spinWait.SpinOnce();

            shouldProcessData = false;

            // Process data
            //for (int j = 0; j < stepperThreadChunkIndex; j++)
            for (int j = 0; j < chunksToStep.Length; j++)
            {
                int chunkIndex = chunksToStep[j];

                if (simulation.AllChunks[chunkIndex].IsCurrentlyDirty)
                {
                    // Loop the dirty rect bottom to top
                    for (int y = simulation.AllChunks[chunkIndex].DirtyRectMinY; y <= simulation.AllChunks[chunkIndex].DirtyRectMaxY; y++)
                    {
                        // Generate a random X access pattern, to avoid visual consistencies.
                        //int dirtyRectWidth = simulation.DirtyChunks[chunkIndex].DirtyRectMaxX - simulation.DirtyChunks[chunkIndex].DirtyRectMinX + 1;
                        //RegenerateAndShuffleIndexes(dirtyRectWidth);

                        // Loop the dirty rect in random X update order.
                        // If we ever need to go back to consistent updating, use this ->
                        //for (int i = 0; i < dirtyRectWidth; i++)
                        for (int x = simulation.AllChunks[chunkIndex].DirtyRectMinX; x <= simulation.AllChunks[chunkIndex].DirtyRectMaxX; x++)
                        {
                            // Calculate x position from the shuffled access index.
                            //int x = simulation.DirtyChunks[chunkIndex].DirtyRectMinX + shuffledIndexes[i];

                            // Skip if this cell has been stepped already.
                            if (simulation.SteppedCells[x + y * Settings.WORLD_WIDTH])
                                continue;

                            // Skip if position outside the matrix. Happens when a cell at the edge of the matrix gets dirtied.
                            if (!IsPositionInsideMatrix(x, y))
                                continue;

                            // Finally, handle the step for this cell.
                            //(int newX, int newY) = HandleStep(x, y, deltaTime);
                            (int newX, int newY) = simulation.ElementMatrix[x + y * Settings.WORLD_WIDTH].Step(simulation, x, y, latestDeltaTime);

                            // Set the cell's new position as stepped, so we won't visit it again causing multiple updates per frame.
                            simulation.SteppedCells[newX + newY * Settings.WORLD_WIDTH] = true;
                        }
                    }
                }

                // We're done with this chunk, tell it to construct their dirty rect.
                simulation.AllChunks[chunkIndex].ConstructDirtyRectangle();
            }

            HasProcessedData = true;
        }
    }
    
    /*/// <summary>
    /// Initializes shuffledIndexes based on if we use chunking or not.
    /// </summary>
    private void InitializeShuffledIndexes()
    {
        shuffledIndexes = new int[Settings.WORLD_CHUNK_SIZE];
        
        for (int i = 0; i < shuffledIndexes.Length; i++)
        {
            shuffledIndexes[i] = i;
        }
    }
    
    /// <summary>
    /// Shuffles the x-axis access indexes using the Fisher-Yates algorithm.
    /// </summary>
    private void RegenerateAndShuffleIndexes(int newLength)
    {
        // Regenerate
        for (int i = 0; i < newLength; i++)
        {
            shuffledIndexes[i] = i;
        }
        
        // Shuffle
        int n = newLength;
        while (n > 1)
        {
            // TODO: Use pregenerated random values instead of generating them on the fly.
            int k = RandomFactory.SeedlessRandom.Next(n--);
            (shuffledIndexes[n], shuffledIndexes[k]) = (shuffledIndexes[k], shuffledIndexes[n]);
        }
    }*/
}