
using System;
using System.Collections;
using Grit.Simulation.Elements;
using Microsoft.Xna.Framework;

namespace Grit.Simulation.World;

/// <summary>
/// Contains a single double-buffered dirty rectangle.
/// </summary>
public class Chunk
{
    // Dirty rect values, constructed at the end of a frame from the dirty changes.
    public bool IsCurrentlyDirty;
    public int DirtyRectMinX;
    public int DirtyRectMinY;
    public int DirtyRectMaxX;
    public int DirtyRectMaxY;
    
    public readonly Point ChunkWorldPos;

    
    // Internal values used to determine dirty changes.
    private bool internalIsDirty;
    private int internalMinX;
    private int internalMinY;
    private int internalMaxX;
    private int internalMaxY;

    private readonly Simulation simulation;
    
    /// <summary>
    /// Flattened 2D-array of the chunk contents.
    /// Flattened arrays are used here instead of 2D-arrays for performance reasons.
    /// </summary>
    private readonly Element[] cells;
    
    /// <summary>
    /// All cells that have been stepped already this frame.
    /// </summary>
    private readonly BitArray steppedCells;
    
    /// <summary>
    /// Shuffled array of access indexes for the element matrix.
    /// Prevents visually consistent updating of the world.
    /// </summary>
    private int[] shuffledIndexes;
    
    //private bool IsPositionInsideChunk(int x, int y) => x >= 0 && y >= 0 && x < worldWidth && y < worldHeight;


    public Chunk(Point chunkWorldPos, Simulation host)
    {
        ChunkWorldPos = chunkWorldPos;
        simulation = host;
        
        
        cells = new Element[Settings.WORLD_CHUNK_SIZE * Settings.WORLD_CHUNK_SIZE];
        steppedCells = new BitArray(Settings.WORLD_CHUNK_SIZE * Settings.WORLD_CHUNK_SIZE);
        
        
        InitializeShuffledIndexes();
        
        ShuffleIndexArray();
        
        SetEverythingClean();
    }

    private void InitializeShuffledIndexes()
    {
        shuffledIndexes = new int[Settings.WORLD_CHUNK_SIZE];
        for (int i = 0; i < shuffledIndexes.Length; i++)
        {
            shuffledIndexes[i] = i;
        }
    }


    public void SetContents(Element[] elements)
    {
        elements.CopyTo(cells, 0);
    }

    public void Update()
    {
        steppedCells.SetAll(false);

        if (IsCurrentlyDirty)
        {
            // Loop the dirty rect bottom to top
            for (int y = DirtyRectMinY; y <= DirtyRectMaxY; y++)
            {
                // Generate a random X access pattern, to avoid visual consistencies.
                int dirtyRectWidth = DirtyRectMaxX - DirtyRectMinX + 1;
                RegenerateAndShuffleXIndexes(dirtyRectWidth);

                // Loop the dirty rect in random X update order.
                // If we ever need to go back to consistent updating, use this -> for (int dirtyRectX = chunks[chunkX, chunkY].ConstructedMinX; dirtyRectX <= chunks[chunkX, chunkY].ConstructedMaxX; dirtyRectX++)
                for (int i = 0; i < dirtyRectWidth; i++)
                {
                    // Calculate x position from the shuffled access index.
                    int x = DirtyRectMinX + shuffledIndexes[i];

                    // Skip if this cell has been stepped already.
                    if (steppedCells[x + y * Settings.WORLD_CHUNK_SIZE])
                        continue;

                    // Skip if position outside the matrix. Happens when a cell at the edge of the matrix gets dirtied.
                    //if (!IsPositionInsideMatrix(x, y))
                    //    continue;

                    // Finally, handle the step for this cell.
                    //(int newX, int newY) = HandleStep(x, y, deltaTime);
                    (int newX, int newY) = cells[x + y * Settings.WORLD_CHUNK_SIZE].Tick(simulation, x, y);

                    // Set the cell's new position as stepped, so we won't visit it again causing multiple updates per frame.
                    steppedCells[newX + newY * Settings.WORLD_CHUNK_SIZE] = true;
                }
            }

            StepRandomTicks();
        }
        
        ConstructDirtyRectangle();
    }

    private void StepRandomTicks()
    {
        for (int i = 0; i < Settings.RANDOM_TICKS_PER_FRAME; i++)
        {
            (int x, int y) = RandomFactory.RandomPosInChunk();
            
            if (steppedCells[x + y * Settings.WORLD_CHUNK_SIZE])
                continue;
            
            // No need to manually set the random cell dirty, since we don't want consequent updates to happen.
            // If HandleStep creates a new cell/causes movement, dirtying will be handled internally.
            (int newX, int newY) = cells[x + y * Settings.WORLD_CHUNK_SIZE].RandomTick(simulation, x, y);
            
            steppedCells[newX + newY * Settings.WORLD_CHUNK_SIZE] = true;
        }
    }


    #region PUBLIC METHODS

    /// <summary>
    /// Sets the given position as dirty inside this chunk.
    /// </summary>
    public void SetDirtyAt(int x, int y)
    {
        // Resize the current dirty rect if needed.
        internalMinX = Math.Min(internalMinX, x);
        internalMinY = Math.Min(internalMinY, y);
        internalMaxX = Math.Max(internalMaxX, x);
        internalMaxY = Math.Max(internalMaxY, y);

        internalIsDirty = true;
    }

    /// <summary>
    /// Needs to be called after all the frame's updates are done.
    /// </summary>
    public void ConstructDirtyRectangle()
    {
        ConstructDirtyRect();
        
        CleanInternally();
    }

    public void SetEverythingDirty()
    {
        // Dirty internally
        internalIsDirty = true;
        internalMinX = ChunkWorldPos.X;
        internalMinY = ChunkWorldPos.Y;
        internalMaxX = internalMinX + Settings.WORLD_CHUNK_SIZE - 1;
        internalMaxY = internalMinY + Settings.WORLD_CHUNK_SIZE - 1;
        
        ConstructDirtyRect();
    }

    public void SetEverythingClean()
    {
        CleanInternally();
        
        ConstructDirtyRect();
    }

    #endregion

    
    #region PRIVATE METHODS

    private void CleanInternally()
    {
        internalIsDirty = false;
        internalMinX = int.MaxValue;
        internalMinY = int.MaxValue;
        internalMaxX = int.MinValue;
        internalMaxY = int.MinValue;
    }

    private void ConstructDirtyRect()
    {
        IsCurrentlyDirty = internalIsDirty;
        DirtyRectMinX = internalMinX;
        DirtyRectMinY = internalMinY;
        DirtyRectMaxX = internalMaxX;
        DirtyRectMaxY = internalMaxY;
    }

    #endregion
    
    
    /// <summary>
    /// Shuffles the x-axis access indexes using the Fisher-Yates algorithm.
    /// </summary>
    private void ShuffleIndexArray()
    {
        int n = shuffledIndexes.Length;
        while (n > 1)
        {
            int k = RandomFactory.SeedlessRandom.Next(n--);
            (shuffledIndexes[n], shuffledIndexes[k]) = (shuffledIndexes[k], shuffledIndexes[n]);
        }
    }
    
    /// <summary>
    /// Shuffles the x-axis access indexes using the Fisher-Yates algorithm.
    /// </summary>
    private void RegenerateAndShuffleXIndexes(int newLength)
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

    public void SetElementAtIndex(int index, Element newElement)
    {
        // Skip if we are trying to replace an element with itself.
        if(newElement.Id == cells[index].Id)
            return;
        
        
    }
}