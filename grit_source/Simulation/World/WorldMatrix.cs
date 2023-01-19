using System;
using System.Collections;
using Grit.Simulation.Elements;
using Grit.Simulation.Elements.ElementDefinitions;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Grit.Simulation.World;

public static class WorldMatrix
{
    #region PUBLIC FIELDS

    /// <summary>
    /// Pixels that get drawn to the screen at the end of the frame.
    /// Public static because we don't want to pass an array reference every frame >:)
    /// </summary>
    public static Color[] PixelsToDraw { get; private set; }

    public static bool IsChunkCurrentlyDirty(int chunkX, int chunkY) => dirtyChunks[chunkX, chunkY].IsCurrentlyDirty;

    #endregion

    
    #region PRIVATE FIELDS

    /// <summary>
    /// Flattened 2D-array of the world.
    /// Get the element at [x, y] by [x + y * Width].
    /// Flattened arrays are used here instead of 2D-arrays for performance reasons.
    /// </summary>
    private static Element[] elementMatrix;
    
    /// <summary>
    /// All the chunks of the world.
    /// Every chunk can contain a single dirty rectangle.
    /// </summary>
    private static DirtyChunk[,] dirtyChunks;
    
    /// <summary>
    /// All cells that have been stepped already this frame.
    /// </summary>
    private static BitArray steppedCells;

    /// <summary>
    /// Array of dirty rectangles, used only for debugging.
    /// </summary>
    private static RectangleF[] debugDirtyRects;
    
    // Directly pulled from settings, but cached here to make the code a bit tidier.
    private static int worldWidth;
    private static int worldHeight;
    private static int chunkCountX;
    private static int chunkCountY;
    
    /// <summary>
    /// Shuffled array of access indexes for the element matrix.
    /// Prevents visually consistent updating of the world.
    /// </summary>
    private static int[] shuffledIndexes;
    
    private static bool IsPositionInsideMatrix(int x, int y) => x >= 0 && y >= 0 && x < worldWidth && y < worldHeight;

    #endregion


    #region PUBLIC METHODS

    public static void Initialize()
    {
        if (Settings.USE_WORLD_CHUNKING && (worldWidth % Settings.WORLD_CHUNK_SIZE != 0 || worldHeight % Settings.WORLD_CHUNK_SIZE != 0))
            throw new Exception($"World size is not dividable by ChunkSize {Settings.WORLD_CHUNK_SIZE}!");

        worldWidth = Settings.WORLD_WIDTH;
        worldHeight = Settings.WORLD_HEIGHT;
        
        chunkCountX = Settings.CHUNK_COUNT_X;
        chunkCountY = Settings.CHUNK_COUNT_Y;
        
        PixelsToDraw = new Color[worldWidth * worldWidth];
        steppedCells = new BitArray(worldWidth * worldHeight);
        
        if(Settings.USE_WORLD_CHUNKING)
            InitializeChunks();
        
        InitializeWorld();
        
        InitializeShuffledIndexes();
        
        ShuffleIndexArray();
    }

    /// <summary>
    /// Steps through all dirty chunks and through their dirty rects, updating required cells.
    /// </summary>
    public static void StepDirtyChunks(float deltaTime)
    {
        // Reset the stepped cells.
        steppedCells.SetAll(false);
        
        if (Settings.USE_WORLD_CHUNKING)
        {
            // Loop all chunks (left to right, bottom to top)
            for (int chunkY = chunkCountY - 1; chunkY >= 0; chunkY--)
            {
                // NOTE: Can later be randomized with shuffled X-access indices, if needed.
                for (int chunkX = 0; chunkX < chunkCountX; chunkX++)
                {
                    if (dirtyChunks[chunkX, chunkY].IsCurrentlyDirty)
                    {
                        // Loop the dirty rect bottom to top
                        for (int y = dirtyChunks[chunkX, chunkY].DirtyRectMinY; y <= dirtyChunks[chunkX, chunkY].DirtyRectMaxY; y++)
                        {
                            // Generate a random X access pattern, to avoid visual consistencies.
                            int dirtyRectWidth = dirtyChunks[chunkX, chunkY].DirtyRectMaxX - dirtyChunks[chunkX, chunkY].DirtyRectMinX + 1;
                            RegenerateAndShuffleXIndexes(dirtyRectWidth);
                            
                            // Loop the dirty rect in random X update order.
                            // If we ever need to go back to consistent updating, use this -> for (int dirtyRectX = chunks[chunkX, chunkY].ConstructedMinX; dirtyRectX <= chunks[chunkX, chunkY].ConstructedMaxX; dirtyRectX++)
                            for (int i = 0; i < dirtyRectWidth; i++)
                            {
                                // Calculate x position from the shuffled access index.
                                int x = dirtyChunks[chunkX, chunkY].DirtyRectMinX + shuffledIndexes[i];
                                
                                // Skip if this cell has been stepped already.
                                if (steppedCells[x + y * Settings.WORLD_WIDTH])
                                    continue;
                                
                                // Skip if position outside the matrix. Happens when a cell at the edge of the matrix gets dirtied.
                                if (!IsPositionInsideMatrix(x, y))
                                    continue;
                                
                                // Finally, handle the step for this cell.
                                //(int newX, int newY) = HandleStep(x, y, deltaTime);
                                (int newX, int newY) = elementMatrix[x + y * worldWidth].Step(elementMatrix, x, y, deltaTime);
                                
                                // Set the cell's new position as stepped, so we won't visit it again causing multiple updates per frame.
                                steppedCells[newX + newY * Settings.WORLD_WIDTH] = true;
                            }
                        }
                    }
                    
                    // We're done with this chunk, tell it to construct their dirty rect.
                    dirtyChunks[chunkX, chunkY].ConstructDirtyRectangle();
                }
            }
        }
        else
        {
            // Loop the world (random X access, bottom to top)
            for (int y = worldHeight - 1; y >= 0; y--)
            {
                foreach (int x in shuffledIndexes)
                {
                    if(steppedCells[x + y * Settings.WORLD_WIDTH])
                        continue;
                    
                    (int newX, int newY) = elementMatrix[x + y * worldWidth].Step(elementMatrix, x, y, deltaTime);

                    steppedCells[newX + newY * Settings.WORLD_WIDTH] = true;
                }
        
                ShuffleIndexArray();
            }
        }
    }

    /// <summary>
    /// Selects random cells from the world, and updates them, ignoring all dirtying logic.
    /// Used for infrequent things, like interactions with neighbouring cells; for example, melting.
    /// </summary>
    public static void StepRandomTicks(float deltaTime)
    {
        for (int i = 0; i < Settings.RANDOM_TICKS_PER_FRAME; i++)
        {
            (int x, int y) = RandomFactory.RandomPosInWorld();
            
            if (steppedCells[x + y * Settings.WORLD_WIDTH])
                continue;
            
            // No need to manually set the random cell dirty, since we don't want consequent updates to happen.
            // If HandleStep creates a new cell/causes movement, dirtying will be handled internally.
            (int newX, int newY) = elementMatrix[x + y * worldWidth].RandomStep(elementMatrix, x, y, deltaTime);
            
            steppedCells[newX + newY * Settings.WORLD_WIDTH] = true;
        }
    }

    /// <summary>
    /// Construct all dirty rectangles.
    /// </summary>
    /// <returns>Dirty rects for debug drawing.</returns>
    public static RectangleF[] GetDebugDirtyRects()
    {
        for (int y = 0; y < chunkCountY; y++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                RectangleF constructedDirtyRect = new(
                    dirtyChunks[x, y].DirtyRectMinX, 
                    dirtyChunks[x, y].DirtyRectMinY, 
                    dirtyChunks[x, y].DirtyRectMaxX - dirtyChunks[x, y].DirtyRectMinX + 1,
                    dirtyChunks[x, y].DirtyRectMaxY - dirtyChunks[x, y].DirtyRectMinY + 1);
                        
                debugDirtyRects[x + y * chunkCountY] = constructedDirtyRect;
            }
        }

        return debugDirtyRects;
    }

    /// <summary>
    /// Designed to be used externally, do not waste internal function calls on this!
    /// </summary>
    public static Element GetElementAt(int index)
    {
        return elementMatrix[index];
    }

    /// <summary>
    /// Places the given element in to the given position in the matrix.
    /// </summary>
    public static void SetElementAt(int setX, int setY, Element newElement)
    {
        int index = setX + setY * worldWidth;
        
        // Skip if we are trying to replace an element with itself.
        if(newElement.Id == elementMatrix[index].Id)
            return;
        
        elementMatrix[index] = newElement;
        PixelsToDraw[index] = newElement.GetColor();    //BUG: WARN: NOTE: TODO: Move this so that dirty pixels get redrawn/recolored! Move to Element.cs.Draw()?

        // Updating the dirty chunks.
        if (Settings.USE_WORLD_CHUNKING)
        {
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
                    dirtyChunks[chunkX, chunkY].SetDirtyAt(x, y);
                }
            }
        }
    }

    /// <summary>
    /// Swaps two elements with each other, while dirtying a maximum of 14 cells.
    /// </summary>
    /// <exception cref="Exception">pos1 == pos2</exception>
    public static void SwapElementsAt(int x1, int y1, int x2, int y2)
    {
        // Do not allow an cell to swap itself.
        if (x1 == x2 && y1 == y2)
        {
            throw new Exception("Tried to swap an element with itself.");
        }
        
        // Swap operations
        int index1 = x1 + y1 * worldWidth;
        int index2 = x2 + y2 * worldWidth;
        (elementMatrix[index2], elementMatrix[index1]) = (elementMatrix[index1], elementMatrix[index2]);
        (PixelsToDraw[x2 + y2 * worldWidth], PixelsToDraw[x1 + y1 * worldWidth]) = (PixelsToDraw[x1 + y1 * worldWidth], PixelsToDraw[x2 + y2 * worldWidth]);
        
        if (Settings.USE_WORLD_CHUNKING)
        {
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
                    dirtyChunks[chunkX, chunkY].SetDirtyAt(x, y);
                }
            }
        }
    }

    /// <summary>
    /// Dirties every chunk forcing all cells to update next frame.
    /// </summary>
    public static void SetEveryChunkDirty()
    {
        for (int y = 0; y < chunkCountY; y++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                dirtyChunks[x, y].SetEverythingDirty();
            }
        }
    }

    /// <summary>
    /// Cleans every chunk disabling all updates for the next frame.
    /// </summary>
    public static void SetEveryChunkClean()
    {
        for (int y = 0; y < chunkCountY; y++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                dirtyChunks[x, y].SetEverythingClean();
            }
        }
    }

    #endregion

    
    #region PRIVATE METHODS

    private static void InitializeChunks()
    {
        dirtyChunks = new DirtyChunk[chunkCountX, chunkCountY];

        if (Settings.DRAW_DIRTY_RECTS)
        {
            debugDirtyRects = new RectangleF[chunkCountX * chunkCountY];
        }
        
        for (int x = 0; x < chunkCountX; x++)
        {
            for (int y = 0; y < chunkCountY; y++)
            {
                dirtyChunks[x, y] = new DirtyChunk(x, y);
            }
        }
    }

    private static void InitializeWorld()
    {
        elementMatrix = new Element[worldWidth * worldHeight];
        
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                int index = x + y * worldWidth;
        
                elementMatrix[index] = new AirElement(x, y);
                PixelsToDraw[index] = elementMatrix[index].GetColor();

                // Generate some stone at the bottom of the world
                if (y >= Settings.WORLD_HEIGHT - 50)
                {
                    elementMatrix[index] = new StoneElement(x, y);
                    PixelsToDraw[index] = elementMatrix[index].GetColor();
                }
            }
        }
    }

    /// <summary>
    /// Initializes shuffledIndexes based on if we use chunking or not.
    /// </summary>
    private static void InitializeShuffledIndexes()
    {
        if (Settings.USE_WORLD_CHUNKING)
        {
            shuffledIndexes = new int[Settings.WORLD_CHUNK_SIZE];
        }
        else
        {
            shuffledIndexes = new int[worldWidth];
        }
        
        for (int i = 0; i < shuffledIndexes.Length; i++)
        {
            shuffledIndexes[i] = i;
        }
    }
    
    /// <summary>
    /// Shuffles the x-axis access indexes using the Fisher-Yates algorithm.
    /// </summary>
    private static void ShuffleIndexArray()
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
    private static void RegenerateAndShuffleXIndexes(int newLength)
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

    #endregion
}