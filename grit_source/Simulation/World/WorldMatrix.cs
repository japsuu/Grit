namespace Grit.Simulation.World;

public static class WorldMatrixxx
{
    /*#region PUBLIC FIELDS

    public static bool IsChunkCurrentlyDirty(int chunkX, int chunkY) => DirtyChunks[chunkX + chunkY * chunkCountX].IsCurrentlyDirty;

    #endregion

    
    #region PRIVATE FIELDS

    /// <summary>
    /// All the chunks of the world.
    /// Every chunk can contain a single dirty rectangle.
    /// </summary>
    public static volatile DirtyChunk[] DirtyChunks;

    /// <summary>
    /// Array of dirty rectangles, used only for debugging.
    /// </summary>
    private static RectangleF[] debugDirtyRects;
    
    // Directly pulled from settings, but cached here to make the code a bit tidier.
    private static int worldWidth;
    private static int worldHeight;
    private static int chunkCountX;
    private static int chunkCountY;

    #endregion


    #region PUBLIC METHODS*/

    /*public static void Initialize()
    {
        worldWidth = Settings.WORLD_WIDTH;
        worldHeight = Settings.WORLD_HEIGHT;
        
        chunkCountX = Settings.CHUNK_COUNT_X;
        chunkCountY = Settings.CHUNK_COUNT_Y;
        
        PixelsToDraw = new Color[worldWidth * worldWidth];
        SteppedCells = new bool[worldWidth * worldHeight];
        
        if(Settings.USE_WORLD_CHUNKING)
            InitializeChunks();
        
        InitializeWorld();
        
        InitializeShuffledIndexes();
        
        ShuffleIndexArray();
    }*/

    /*/// <summary>
    /// Steps through all dirty chunks and through their dirty rects, updating required cells.
    /// </summary>
    public static void StepDirtyChunks(float deltaTime)
    {
        // Reset the stepped cells.
        for (int i = 0; i < worldWidth * worldHeight; i++)
        {
            SteppedCells[i] = false;
        }

        if (Settings.USE_WORLD_CHUNKING) return; //TODO: Implement multithreading for non-chunked world.
    }*/

    /*/// <summary>
    /// Selects random cells from the world, and updates them, ignoring all dirtying logic.
    /// Used for infrequent things, like interactions with neighbouring cells; for example, melting.
    /// </summary>
    public static void StepRandomTicks(float deltaTime)
    {
        for (int i = 0; i < Settings.RANDOM_TICKS_PER_FRAME; i++)
        {
            (int x, int y) = RandomFactory.RandomPosInWorld();
            
            if (SteppedCells[x + y * Settings.WORLD_WIDTH])
                continue;
            
            // No need to manually set the random cell dirty, since we don't want consequent updates to happen.
            // If HandleStep creates a new cell/causes movement, dirtying will be handled internally.
            (int newX, int newY) = ElementMatrix[x + y * worldWidth].RandomStep(ElementMatrix, x, y, deltaTime);
            
            SteppedCells[newX + newY * Settings.WORLD_WIDTH] = true;
        }
    }*/

    /*/// <summary>
    /// Construct all dirty rectangles.
    /// </summary>
    /// <returns>Dirty rects for debug drawing.</returns>
    public static RectangleF[] GetDebugDirtyRects()
    {
        for (int y = 0; y < chunkCountY; y++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                RectangleF constructedDirtyRect;
                if (DirtyChunks[x + y * chunkCountX].IsCurrentlyDirty)
                {
                    constructedDirtyRect = new RectangleF(
                        DirtyChunks[x + y * chunkCountX].DirtyRectMinX, 
                        DirtyChunks[x + y * chunkCountX].DirtyRectMinY, 
                        DirtyChunks[x + y * chunkCountX].DirtyRectMaxX - DirtyChunks[x + y * chunkCountX].DirtyRectMinX + 1,
                        DirtyChunks[x + y * chunkCountX].DirtyRectMaxY - DirtyChunks[x + y * chunkCountX].DirtyRectMinY + 1);
                }
                else
                {
                    constructedDirtyRect = RectangleF.Empty;
                }
                        
                debugDirtyRects[x + y * chunkCountY] = constructedDirtyRect;
            }
        }

        return debugDirtyRects;
    }*/

    // /// <summary>
    // /// Designed to be used externally, do not waste internal function calls on this!
    // /// </summary>
    // public static Element GetElementAt(int index)
    // {
    //     return ElementMatrix[index];
    // }

    /*/// <summary>
    /// Places the given element in to the given position in the matrix.
    /// </summary>
    public static void SetElementAt(int setX, int setY, Element newElement)
    {
        int index = setX + setY * worldWidth;
        
        // Skip if we are trying to replace an element with itself.
        if(newElement.Id == ElementMatrix[index].Id)
            return;
        
        ElementMatrix[index] = newElement;
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
                    DirtyChunks[chunkX + chunkY * chunkCountX].SetDirtyAt(x, y);
                }
            }
        }
    }*/

    /*/// <summary>
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
        (ElementMatrix[index2], ElementMatrix[index1]) = (ElementMatrix[index1], ElementMatrix[index2]);
        (PixelsToDraw[index2], PixelsToDraw[index1]) = (PixelsToDraw[index1], PixelsToDraw[index2]);
        
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
                    DirtyChunks[chunkX + chunkY * chunkCountX].SetDirtyAt(x, y);
                }
            }
        }
    }*/

    /*#endregion

    
    #region PRIVATE METHODS

    private static void InitializeChunks()
    {
        DirtyChunks = new DirtyChunk[chunkCountX * chunkCountY];

        if (Settings.DRAW_DIRTY_RECTS)
        {
            debugDirtyRects = new RectangleF[chunkCountX * chunkCountY];
        }
        
        for (int x = 0; x < chunkCountX; x++)
        {
            for (int y = 0; y < chunkCountY; y++)
            {
                DirtyChunks[x + y * chunkCountX] = new DirtyChunk(x, y);
            }
        }
    }

    private static void InitializeWorld()
    {
        ElementMatrix = new Element[worldWidth * worldHeight];
        
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                int index = x + y * worldWidth;
        
                ElementMatrix[index] = new AirElement(x, y);
                PixelsToDraw[index] = ElementMatrix[index].GetColor();

                // Generate some stone at the bottom of the world
                if (y >= Settings.WORLD_HEIGHT - 50)
                {
                    ElementMatrix[index] = new StoneElement(x, y);
                    PixelsToDraw[index] = ElementMatrix[index].GetColor();
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

    #endregion*/
}