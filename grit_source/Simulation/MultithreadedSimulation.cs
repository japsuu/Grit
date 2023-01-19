using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grit.Simulation.Elements;
using Grit.Simulation.Elements.ElementDefinitions;
using Grit.Simulation.World;
using MonoGame.Extended;

namespace Grit.Simulation;

public class MultithreadedSimulation : Simulation
{
    /// <summary>
    /// Flattened 2D-array of the world.
    /// Get the element at [x, y] by [x + y * Width].
    /// Flattened arrays are used here instead of 2D-arrays for performance reasons.
    /// </summary>
    public Element[] ElementMatrix;
    
    /// <summary>
    /// All the chunks of the world.
    /// Every chunk can contain a single dirty rectangle.
    /// </summary>BUG: Is this really needed? We can just track the actually dirty chunks.
    public DirtyChunk[] AllChunks;
    
    /// <summary>
    /// All cells that have been stepped already this frame.
    /// </summary>
    public readonly bool[] SteppedCells;
    
    private RectangleF[] debugDirtyRects;
    
    /// <summary>
    /// Shuffled array of access indexes for the element matrix.
    /// Prevents visually consistent updating of the world.
    /// </summary>
    private int[] shuffledIndexes;

    private HashSet<int>[] chunksToStep;
    
    private readonly bool isInitialized;


    protected override RectangleF[] GetDirtyRects() => BuildDirtyRects();
    
    
    public override Element GetElementAt(int index) => ElementMatrix[index];
    
    
    public bool IsChunkCurrentlyDirty(int chunkX, int chunkY) => AllChunks[chunkX + chunkY * Settings.CHUNK_COUNT_X].IsCurrentlyDirty;

    private static bool IsPositionInsideMatrix(int x, int y) => x >= 0 && y >= 0 && x < Settings.WORLD_WIDTH && y < Settings.WORLD_HEIGHT;
    

    public override void ForceStepAll()
    {
        SetEveryChunkDirtyState(true);
    }

    public override void ForceSkipAll()
    {
        SetEveryChunkDirtyState(false);
    }


    public MultithreadedSimulation(int targetTps) : base(targetTps)
    {
        SteppedCells = new bool[Settings.WORLD_WIDTH * Settings.WORLD_HEIGHT];
        
        InitializeShuffledIndexes();
        InitializeChunks();
        InitializeWorld();
        
        if (Settings.DRAW_DIRTY_RECTS)
            InitializeDirtyRects();
        
        InitializeStepperThreads();

        isInitialized = true;
    }

    
    public override void SwapElementsAt(int x1, int y1, int x2, int y2)
    {
        // Do not allow an cell to swap itself.
        if (x1 == x2 && y1 == y2)
        {
            throw new Exception("Tried to swap an element with itself.");
        }
        
        // Swap operations
        int index1 = x1 + y1 * Settings.WORLD_WIDTH;
        int index2 = x2 + y2 * Settings.WORLD_WIDTH;
        (ElementMatrix[index2], ElementMatrix[index1]) = (ElementMatrix[index1], ElementMatrix[index2]);
        (FrontBuffer[index2], FrontBuffer[index1]) = (FrontBuffer[index1], FrontBuffer[index2]);
        
        // Dirty the set elements and all (max 12, when diagonal swap) surrounding elements.
        int minX = Math.Max(0, Math.Min(x1 - 1, x2 - 1));
        int minY = Math.Max(0, Math.Min(y1 - 1, y2 - 1));
        int maxX = Math.Min(Settings.WORLD_WIDTH - 1, Math.Max(x1 + 1, x2 + 1));
        int maxY = Math.Min(Settings.WORLD_WIDTH - 1, Math.Max(y1 + 1, y2 + 1));
        for (int y = minY; y < maxY + 1; y++)
        {
            for (int x = minX; x < maxX + 1; x++)
            {
                int chunkX = x / Settings.WORLD_CHUNK_SIZE;
                int chunkY = y / Settings.WORLD_CHUNK_SIZE;
                int chunkIndex = chunkX + chunkY * Settings.CHUNK_COUNT_X;
                
                AllChunks[chunkIndex].SetDirtyAt(x, y);
                chunksToStep[chunkX % 2 + chunkY % 2 * 2].Add(chunkIndex);
            }
        }
    }

    
    public override void SetElementAt(int setX, int setY, Element newElement)
    {
        int index = setX + setY * Settings.WORLD_WIDTH;
        
        // Skip if we are trying to replace an element with itself.
        if(newElement.Id == ElementMatrix[index].Id)
            return;
        
        ElementMatrix[index] = newElement;
        FrontBuffer[index] = newElement.GetColor();    //BUG: WARN: NOTE: TODO: Move this so that dirty pixels get redrawn/recolored! Move to Element.cs.Draw()?

        // Dirty the set element and all 8 surrounding elements.
        int minX = Math.Max(0, setX - 1);
        int minY = Math.Max(0, setY - 1);
        int maxX = Math.Min(Settings.WORLD_WIDTH - 1, setX + 1);
        int maxY = Math.Min(Settings.WORLD_WIDTH - 1, setY + 1);
        for (int y = minY; y < maxY + 1; y++)
        {
            for (int x = minX; x < maxX + 1; x++)
            {
                int chunkX = x / Settings.WORLD_CHUNK_SIZE;
                int chunkY = y / Settings.WORLD_CHUNK_SIZE;
                int chunkIndex = chunkX + chunkY * Settings.CHUNK_COUNT_X;
                
                AllChunks[chunkIndex].SetDirtyAt(x, y);
                chunksToStep[chunkX % 2 + chunkY % 2 * 2].Add(chunkIndex);
            }
        }
    }
    
    
    /// <summary>
    /// Dirties/Cleans all chunks.
    /// Dirtying every chunk will force all cells to update next frame.
    /// Cleaning every chunk will skip all updates next frame.
    /// </summary>
    private void SetEveryChunkDirtyState(bool shouldBeDirty)
    {
        for (int y = 0; y < Settings.CHUNK_COUNT_Y; y++)
        {
            for (int x = 0; x < Settings.CHUNK_COUNT_X; x++)
            {
                if(shouldBeDirty)
                {
                    AllChunks[x + y * Settings.CHUNK_COUNT_X].SetEverythingDirty();
                }
                else
                {
                    AllChunks[y + y * Settings.CHUNK_COUNT_X].SetEverythingClean();
                }
            }
        }
    }


    private RectangleF[] BuildDirtyRects()
    {
        for (int y = 0; y < Settings.CHUNK_COUNT_Y; y++)
        {
            for (int x = 0; x < Settings.CHUNK_COUNT_X; x++)
            {
                int chunkIndex = x + y * Settings.CHUNK_COUNT_X;
                
                if (AllChunks[chunkIndex].IsCurrentlyDirty)
                {
                    debugDirtyRects[x + y * Settings.CHUNK_COUNT_Y] = new RectangleF(
                        AllChunks[chunkIndex].DirtyRectMinX, 
                        AllChunks[chunkIndex].DirtyRectMinY, 
                        AllChunks[chunkIndex].DirtyRectMaxX - AllChunks[chunkIndex].DirtyRectMinX + 1,
                        AllChunks[chunkIndex].DirtyRectMaxY - AllChunks[chunkIndex].DirtyRectMinY + 1);
                }
                else
                {
                    debugDirtyRects[x + y * Settings.CHUNK_COUNT_Y] = RectangleF.Empty;
                }
            }
        }

        return debugDirtyRects;
    }


    private void InitializeStepperThreads()
    {
        chunksToStep = new[]
        {
            new HashSet<int>(Settings.QUARTER_OF_ALL_CHUNKS),
            new HashSet<int>(Settings.QUARTER_OF_ALL_CHUNKS),
            new HashSet<int>(Settings.QUARTER_OF_ALL_CHUNKS),
            new HashSet<int>(Settings.QUARTER_OF_ALL_CHUNKS),
        };

        // Assign chunks to threads in a specific pattern, so that neighbouring chunks never get updated at the same time.
        // This is done to prevent updating the same pixel on different threads simultaneously.
        // for (int chunkY = 0; chunkY < Settings.CHUNK_COUNT_Y; chunkY++)
        // {
        //     for (int chunkX = 0; chunkX < Settings.CHUNK_COUNT_X; chunkX++)
        //     {
        //         int chunkIndex = chunkX + chunkY * Settings.CHUNK_COUNT_X;
        //         stepperThreads[].AddChunk(chunkIndex);
        //     }
        // }
    }


    // //BUG: Call from Swap() and Set()?
    // public void RegisterDirtyChunk(int chunkIndex)
    // {
    //     chunksToStep[AllChunks[chunkIndex].ChunkMatrixPosX % 2 + AllChunks[chunkIndex].ChunkMatrixPosY % 2 * 2].Add(chunkIndex);
    // }


    private void StepChunk(int chunkIndex, double deltaTime)
    {
        if (AllChunks[chunkIndex].IsCurrentlyDirty)
        {
            // Loop the dirty rect bottom to top
            for (int y = AllChunks[chunkIndex].DirtyRectMinY; y <= AllChunks[chunkIndex].DirtyRectMaxY; y++)
            {
                // Generate a random X access pattern, to avoid visual consistencies.
                //int dirtyRectWidth = simulation.DirtyChunks[chunkIndex].DirtyRectMaxX - simulation.DirtyChunks[chunkIndex].DirtyRectMinX + 1;
                //RegenerateAndShuffleIndexes(dirtyRectWidth);
                // Loop the dirty rect in random X update order.
                // If we ever need to go back to consistent updating, use this ->
                //for (int i = 0; i < dirtyRectWidth; i++)
                for (int x = AllChunks[chunkIndex].DirtyRectMinX; x <= AllChunks[chunkIndex].DirtyRectMaxX; x++)
                {
                    // Calculate x position from the shuffled access index.
                    //int x = simulation.DirtyChunks[chunkIndex].DirtyRectMinX + shuffledIndexes[i];
                    // Skip if this cell has been stepped already.
                    if (SteppedCells[x + y * Settings.WORLD_WIDTH])
                        continue;

                    // Skip if position outside the matrix. Happens when a cell at the edge of the matrix gets dirtied.
                    if (!IsPositionInsideMatrix(x, y))
                        continue;

                    // Finally, handle the step for this cell.
                    //(int newX, int newY) = HandleStep(x, y, deltaTime);
                    (int newX, int newY) = ElementMatrix[x + y * Settings.WORLD_WIDTH].Step(this, x, y, deltaTime);

                    // Set the cell's new position as stepped, so we won't visit it again causing multiple updates per frame.
                    SteppedCells[newX + newY * Settings.WORLD_WIDTH] = true;
                }
            }
        }

        // We're done with this chunk, tell it to construct their dirty rect.
        AllChunks[chunkIndex].ConstructDirtyRectangle();
    }

    
    protected override void FixedUpdate(double dt, double a)
    {
        if(!isInitialized) return;
        
        // Reset the stepped cells.
        for (int i = 0; i < Settings.WORLD_WIDTH * Settings.WORLD_HEIGHT; i++)
        {
            SteppedCells[i] = false;
        }

        for (int i = 0; i < 4; i++)
        {
            int steppedChunksCount = chunksToStep[i].Count;
            
            Task[] tasks = new Task[steppedChunksCount];
            // for (int j = 0; j < steppedChunksCount; j++)
            // {
            //     int toStep = chunksToStep[i][j];
            //     double deltaTime = dt;
            //     tasks[j] = Task.Factory.StartNew(() => StepChunk(toStep, deltaTime));
            // }

            int index = 0;
            foreach (int s in chunksToStep[i])
            {
                int toStep = s;
                double deltaTime = dt;
                tasks[index] = Task.Factory.StartNew(() => StepChunk(toStep, deltaTime));
                index++;
            }

            Task.WaitAll(tasks);
            chunksToStep[i].Clear();
        }
    }
    
    
    protected override void StepRandomTicks(double dt, double a)
    {
        if(!isInitialized) return;
        
        for (int i = 0; i < Settings.RANDOM_TICKS_PER_FRAME; i++)
        {
            (int x, int y) = RandomFactory.RandomPosInWorld();
            
            if (SteppedCells[x + y * Settings.WORLD_WIDTH])
                continue;
            
            // No need to manually set the random cell dirty, since we don't want consequent updates to happen.
            // If HandleStep creates a new cell/causes movement, dirtying will be handled internally.
            (int newX, int newY) = ElementMatrix[x + y * Settings.WORLD_WIDTH].RandomStep(this, x, y, dt);
            
            SteppedCells[newX + newY * Settings.WORLD_WIDTH] = true;
        }
    }
    
    
    private void InitializeChunks()
    {
        AllChunks = new DirtyChunk[Settings.CHUNK_COUNT_X * Settings.CHUNK_COUNT_Y];

        for (int x = 0; x < Settings.CHUNK_COUNT_X; x++)
        {
            for (int y = 0; y < Settings.CHUNK_COUNT_Y; y++)
            {
                AllChunks[x + y * Settings.CHUNK_COUNT_X] = new DirtyChunk(x, y);
            }
        }
    }


    private void InitializeDirtyRects()
    {
        debugDirtyRects = new RectangleF[Settings.CHUNK_COUNT_X * Settings.CHUNK_COUNT_Y];
    }

    
    private void InitializeWorld()
    {
        ElementMatrix = new Element[Settings.WORLD_WIDTH * Settings.WORLD_HEIGHT];
        
        for (int x = 0; x < Settings.WORLD_WIDTH; x++)
        {
            for (int y = 0; y < Settings.WORLD_HEIGHT; y++)
            {
                int index = x + y * Settings.WORLD_WIDTH;
        
                ElementMatrix[index] = new AirElement(x, y);
                FrontBuffer[index] = ElementMatrix[index].GetColor();

                // Generate some stone at the bottom of the world
                if (y >= Settings.WORLD_HEIGHT - 50)
                {
                    ElementMatrix[index] = new StoneElement(x, y);
                    FrontBuffer[index] = ElementMatrix[index].GetColor();
                }
            }
        }
    }
    
    
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
    }
}