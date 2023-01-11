using System;
using System.Collections;
using System.Diagnostics;
using Grit.Simulation.Elements;
using Grit.Simulation.Elements.ElementDefinitions;
using Grit.Simulation.Elements.Movable;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Grit.Simulation.World;

public static class WorldMatrix
{
    // Public static because we don't want to pass an array reference every frame :)
    public static Color[] PixelsToDraw;
    
    private static int worldWidth;
    private static int worldHeight;
    
    /// <summary>
    /// World. Get element at [x, y] by [x + y * Width].
    /// TODO: Research whether 2D-array or flattened array has better performance.
    /// </summary>
    private static ElementDefinition[] matrix;
    private static DirtyChunk[,] chunks;
    
    // All pixels that have been stepped already this frame.
    private static BitArray steppedPixels;

    // Debug dirty rectangles.
    private static RectangleF[] dirtyRects;

    private static int chunkCountX;
    private static int chunkCountY;
    
    // Shuffled array of access indexes for the world's x-axis. Prevents visually consistent updating of the world.
    private static int[] shuffledXIndexes;

    public static bool IsChunkCurrentlyDirty(int chunkX, int chunkY) => chunks[chunkX, chunkY].IsCurrentlyDirty;
    
    private static bool InsideMatrixBounds(int x, int y) => x >= 0 && y >= 0 && x < worldWidth && y < worldHeight;

    public static void Initialize()
    {
        if (Settings.USE_CHUNKS && (worldWidth % Settings.CHUNK_SIZE != 0 || worldHeight % Settings.CHUNK_SIZE != 0))
            throw new Exception($"World size is not dividable by ChunkSize {Settings.CHUNK_SIZE}!");

        worldWidth = Settings.WORLD_WIDTH;
        worldHeight = Settings.WORLD_HEIGHT;
        
        chunkCountX = Settings.CHUNK_COUNT_X;
        chunkCountY = Settings.CHUNK_COUNT_Y;
        
        PixelsToDraw = new Color[worldWidth * worldWidth];
        steppedPixels = new BitArray(worldWidth * worldHeight);
        
        if(Settings.USE_CHUNKS)
            InitializeChunks();
        
        InitializeWorld();
        
        InitializeXIndexes();
        
        ShuffleXIndexes();
    }

    private static void InitializeChunks()
    {
        chunks = new DirtyChunk[chunkCountX, chunkCountY];

        if (Settings.DRAW_DIRTY_RECTS)
        {
            dirtyRects = new RectangleF[chunkCountX * chunkCountY];
        }
        
        for (int x = 0; x < chunkCountX; x++)
        {
            for (int y = 0; y < chunkCountY; y++)
            {
                chunks[x, y] = new DirtyChunk(x, y);
            }
        }
    }

    private static void InitializeWorld()
    {
        matrix = new ElementDefinition[worldWidth * worldHeight];
        
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                int index = x + y * worldWidth;
        
                matrix[index] = new AirElementDefinition(x, y);
                PixelsToDraw[index] = matrix[index].GetColor();

                if (y == Settings.WORLD_HEIGHT - 50)
                {
                    matrix[index] = new StoneElementDefinition(x, y);
                    PixelsToDraw[index] = matrix[index].GetColor();
                }
            }
        }
    }

    public static void StepDirtyChunks(float deltaTime)
    {
        steppedPixels.SetAll(false);
        
        if (Settings.USE_CHUNKS)
        {
            // Loop all chunks (left to right, bottom to top)
            for (int chunkY = chunkCountY - 1; chunkY >= 0; chunkY--)
            {
                //TODO: Can be randomized with shuffled X-access indices. Not yet needed though.
                for (int chunkX = 0; chunkX < chunkCountX; chunkX++)
                {
                    // If the chunk is dirty
                    if (chunks[chunkX, chunkY].IsCurrentlyDirty)
                    {
                        // Loop the dirty rect (left to right, bottom to top)
                        for (int dirtyRectY = chunks[chunkX, chunkY].ConstructedMinY; dirtyRectY <= chunks[chunkX, chunkY].ConstructedMaxY; dirtyRectY++)
                        {
                            // We randomize the X update order, to not cause visual consistencies.
                            int dirtyRectWidth = chunks[chunkX, chunkY].ConstructedMaxX - chunks[chunkX, chunkY].ConstructedMinX + 1;
                            RegenerateAndShuffleXIndexes(dirtyRectWidth);
                            
                            // If we ever need to go back to consistent updating, use this -> for (int dirtyRectX = chunks[chunkX, chunkY].ConstructedMinX; dirtyRectX <= chunks[chunkX, chunkY].ConstructedMaxX; dirtyRectX++)
                            for (int i = 0; i < dirtyRectWidth; i++)
                            {
                                int dirtyRectX = chunks[chunkX, chunkY].ConstructedMinX + shuffledXIndexes[i];
                                
                                //WARN: Remove when update logic finished
                                if (!InsideMatrixBounds(dirtyRectX, dirtyRectY)) continue;
                                
                                if (steppedPixels[dirtyRectX + dirtyRectY * Settings.WORLD_WIDTH])
                                    continue;
                                
                                //Debug.WriteLine($"{dirtyRectX},{dirtyRectY} looped");
                                
                                (int newX, int newY) = HandleStep(dirtyRectX, dirtyRectY, deltaTime);
                                
                                steppedPixels[newX + newY * Settings.WORLD_WIDTH] = true;
                            }
                        }
                    }
                    
                    chunks[chunkX, chunkY].ConstructDirtyRectangle();
                }
            }
        }
        else
        {
            // Loop the world (left to right, bottom to top)
            for (int y = worldHeight - 1; y >= 0; y--)
            {
                foreach (int x in shuffledXIndexes)
                {
                    if(steppedPixels[x + y * Settings.WORLD_WIDTH])
                        continue;
                    
                    (int newX, int newY) = HandleStep(x, y, deltaTime);

                    steppedPixels[newX + newY * Settings.WORLD_WIDTH] = true;
                }
        
                ShuffleXIndexes();
            }
        }
    }

    public static void StepRandomTicks(float deltaTime)
    {
        for (int i = 0; i < Settings.RANDOM_TICKS_PER_FRAME; i++)
        {
            (int x, int y) = RandomFactory.RandomPosInWorld();
            
            if (steppedPixels[x + y * Settings.WORLD_WIDTH])
                continue;
            
            // No need to manually set the random pixel dirty, since we don't want consequent updates to happen.
            // If HandleStep creates a new pixel/causes movement, dirtying will be handled internally.
            (int newX, int newY) = HandleStep(x, y, deltaTime);
                                
            steppedPixels[newX + newY * Settings.WORLD_WIDTH] = true;
        }
    }

    public static RectangleF[] GatherDirtyRects()
    {
        for (int y = 0; y < chunkCountY; y++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                RectangleF constructedDirtyRect = new(
                    chunks[x, y].ConstructedMinX, 
                    chunks[x, y].ConstructedMinY, 
                    chunks[x, y].ConstructedMaxX - chunks[x, y].ConstructedMinX + 1,
                    chunks[x, y].ConstructedMaxY - chunks[x, y].ConstructedMinY + 1);
                        
                dirtyRects[x + y * chunkCountY] = constructedDirtyRect;
            }
        }

        return dirtyRects;
    }

    public static void SetElementAt(int setX, int setY, ElementDefinition elementDefinition)
    {
        int index = setX + setY * worldWidth;
        //WARN: Remove when testing done TODO
        if(elementDefinition.Type == matrix[index].Type) return;
        
        matrix[index] = elementDefinition;
        PixelsToDraw[index] = elementDefinition.GetColor();

        if (Settings.USE_CHUNKS)
        {
            // Dirty the set element and all 8 surrounding elements
            //WARN: TODO: Only set dirty the 4 "corner" cells, no need to dirty every cell.
            int minX = Math.Max(0, setX - 1);
            int minY = Math.Max(0, setY - 1);
            int maxX = Math.Min(Settings.WORLD_WIDTH - 1, setX + 1);
            int maxY = Math.Min(Settings.WORLD_WIDTH - 1, setY + 1);
            for (int y = minY; y < maxY + 1; y++)
            {
                for (int x = minX; x < maxX + 1; x++)
                {
                    int chunkX = x / Settings.CHUNK_SIZE;
                    int chunkY = y / Settings.CHUNK_SIZE;
                    chunks[chunkX, chunkY].SetDirtyAt(x, y);
                }
            }
        }
    }

    private static void SwapElementsAt(int x1, int y1, int x2, int y2)
    {
        //TODO: Remove when update logic is finished.
        if (x1 == x2 && y1 == y2)
        {
            throw new Exception("Tried to swap an element with itself.");
        }
        
        int index1 = x1 + y1 * worldWidth;
        int index2 = x2 + y2 * worldWidth;
        (matrix[index2], matrix[index1]) = (matrix[index1], matrix[index2]);
        (PixelsToDraw[x2 + y2 * worldWidth], PixelsToDraw[x1 + y1 * worldWidth]) = (PixelsToDraw[x1 + y1 * worldWidth], PixelsToDraw[x2 + y2 * worldWidth]);
        
        if (Settings.USE_CHUNKS)
        {
            // Dirty the set elements and all (max 14, when diagonal swap) surrounding elements.
            //WARN: TODO: Only set dirty the 4 "corner" cells, no need to dirty every cell.
            int minX = Math.Max(0, Math.Min(x1 - 1, x2 - 1));
            int minY = Math.Max(0, Math.Min(y1 - 1, y2 - 1));
            int maxX = Math.Min(Settings.WORLD_WIDTH - 1, Math.Max(x1 + 1, x2 + 1));
            int maxY = Math.Min(Settings.WORLD_WIDTH - 1, Math.Max(y1 + 1, y2 + 1));
            for (int y = minY; y < maxY + 1; y++)
            {
                for (int x = minX; x < maxX + 1; x++)
                {
                    int chunkX = x / Settings.CHUNK_SIZE;
                    int chunkY = y / Settings.CHUNK_SIZE;
                    chunks[chunkX, chunkY].SetDirtyAt(x, y);
                }
            }
        }
    }

    /// <summary>
    /// Designed to be used externally, do not waste internal function calls on this!
    /// </summary>
    public static ElementDefinition GetElementAt(int index)
    {
        return matrix[index];
    }

    /// <summary>
    /// Handles element specific update stepping.
    /// </summary>
    /// <param name="x">X position of the element. Guaranteed to be inside of the matrix</param>
    /// <param name="y">Y position of the element. Guaranteed to be inside of the matrix</param>
    /// <param name="deltaTime">Time since the last frame</param>
    private static (int newX, int newY) HandleStep(int x, int y, float deltaTime)
    {
        // NOTE: This function could be split in to multiple smaller functions: HandleMovement(), HandleHeat(), etc.
        // NOTE: This would increase clarity, but worsen the performance.
        // NOTE: This function could also be relocated inside ElementDefinition classes as an abstract function, for more cleaner implementation (https://youtu.be/5Ka3tbbT-9E?t=262).
        // NOTE: The function could apply the required changes itself, or even return the new position and handle the applying elsewhere.
        // NOTE: I'm not sure how this would affect the performance of the program. 
        
        ElementDefinition elementDefinition = matrix[x + y * worldWidth];
        int newX = x;
        int newY = y;

        switch (elementDefinition.Type)
        {
            case ElementType.Air:
            {
                break;
            }
            
            case ElementType.Sand:
            {
                int belowY = y + 1;
        
                // If at the bottom of the world, replace cell with air.
                if (belowY >= Settings.WORLD_HEIGHT)
                {
                    SetElementAt(x, y, new AirElementDefinition(x, y));
                    break;
                }

                // Below cell.
                if (matrix[x + belowY * worldWidth].GetForm() != ElementForm.Solid)
                {
                    SwapElementsAt(x, y, x, belowY);
                    newY = belowY;
                    break;
                }

                // Randomly choose whether to prioritize left or right update
                bool prioritizeLeft = RandomFactory.RandomBool();
                if (prioritizeLeft)
                {
                    // Left bottom cell.
                    int leftX = x - 1;
                    if (leftX > -1 && matrix[leftX + belowY * worldWidth].GetForm() != ElementForm.Solid)
                    {
                        SwapElementsAt(x, y, leftX, belowY);
                        newX = leftX;
                        newY = belowY;
                        break;
                    }

                    // Right bottom cell.
                    int rightX = x + 1;
                    if (rightX < Settings.WORLD_WIDTH && matrix[rightX + belowY * worldWidth].GetForm() != ElementForm.Solid)
                    {
                        SwapElementsAt(x, y, rightX, belowY);
                        newX = rightX;
                        newY = belowY;
                        break;
                    }
                }
                else
                {
                    // Right bottom cell.
                    int rightX = x + 1;
                    if (rightX < Settings.WORLD_WIDTH && matrix[rightX + belowY * worldWidth].GetForm() != ElementForm.Solid)
                    {
                        SwapElementsAt(x, y, rightX, belowY);
                        newX = rightX;
                        newY = belowY;
                        break;
                    }
                    
                    // Left bottom cell.
                    int leftX = x - 1;
                    if (leftX > -1 && matrix[leftX + belowY * worldWidth].GetForm() != ElementForm.Solid)
                    {
                        SwapElementsAt(x, y, leftX, belowY);
                        newX = leftX;
                        newY = belowY;
                        break;
                    }
                }

                // Stay put.
                break;
            }
            
            case ElementType.Stone:
            {
                break;
            }
            
            case ElementType.Water:
            {
                int belowY = y + 1;
                int leftX = x - 1;
                int rightX = x + 1;
        
                // If at the bottom of the world, replace cell with air.
                if (belowY >= Settings.WORLD_HEIGHT)
                {
                    SetElementAt(x, y, new AirElementDefinition(x, y));
                    break;
                }

                // Below cell.
                if (matrix[x + belowY * worldWidth].GetForm() == ElementForm.Gas)
                {
                    SwapElementsAt(x, y, x, belowY);
                    newY = belowY;
                    break;
                }

                // Randomly choose whether to prioritize left or right update
                bool prioritizeLeft = RandomFactory.RandomBool();
                if (prioritizeLeft)
                {
                    // Left bottom cell.
                    if (leftX > -1 && matrix[leftX + belowY * worldWidth].GetForm() == ElementForm.Gas)
                    {
                        SwapElementsAt(x, y, leftX, belowY);
                        newX = leftX;
                        newY = belowY;
                        break;
                    }
                    
                    // Right bottom cell.
                    if (rightX < Settings.WORLD_WIDTH && matrix[rightX + belowY * worldWidth].GetForm() == ElementForm.Gas)
                    {
                        SwapElementsAt(x, y, rightX, belowY);
                        newX = rightX;
                        newY = belowY;
                        break;
                    }
                }
                else
                {
                    // Right bottom cell.
                    if (rightX < Settings.WORLD_WIDTH && matrix[rightX + belowY * worldWidth].GetForm() == ElementForm.Gas)
                    {
                        SwapElementsAt(x, y, rightX, belowY);
                        newX = rightX;
                        newY = belowY;
                        break;
                    }
                    
                    // Left bottom cell.
                    if (leftX > -1 && matrix[leftX + belowY * worldWidth].GetForm() == ElementForm.Gas)
                    {
                        SwapElementsAt(x, y, leftX, belowY);
                        newX = leftX;
                        newY = belowY;
                        break;
                    }
                }

                if (prioritizeLeft)
                {
                    // Left cell.
                    if (leftX > -1 && matrix[leftX + y * worldWidth].GetForm() == ElementForm.Gas)
                    {
                        SwapElementsAt(x, y, leftX, y);
                        newX = leftX;
                        break;
                    }
                    
                    // Right cell.
                    if (rightX < Settings.WORLD_WIDTH && matrix[rightX + y * worldWidth].GetForm() == ElementForm.Gas)
                    {
                        SwapElementsAt(x, y, rightX, y);
                        newX = rightX;
                        break;
                    }
                }
                else
                {
                    // Right cell.
                    if (rightX < Settings.WORLD_WIDTH && matrix[rightX + y * worldWidth].GetForm() == ElementForm.Gas)
                    {
                        SwapElementsAt(x, y, rightX, y);
                        newX = rightX;
                        break;
                    }
                    
                    // Left cell.
                    if (leftX > -1 && matrix[leftX + y * worldWidth].GetForm() == ElementForm.Gas)
                    {
                        SwapElementsAt(x, y, leftX, y);
                        newX = leftX;
                        break;
                    }
                }

                // Stay put.
                break;
            }
            
            default:
                throw new Exception($"Step logic for the specified element type ({elementDefinition.Type}) not specified!");
        }

        return (newX, newY);
    }

    private static void InitializeXIndexes()
    {
        if (Settings.USE_CHUNKS)
        {
            shuffledXIndexes = new int[Settings.CHUNK_SIZE];
        }
        else
        {
            shuffledXIndexes = new int[worldWidth];
        }
        
        for (int i = 0; i < shuffledXIndexes.Length; i++)
        {
            shuffledXIndexes[i] = i;
        }
    }
    
    /// <summary>
    /// Shuffles the x-axis access indexes using the Fisher-Yates algorithm.
    /// </summary>
    private static void ShuffleXIndexes()
    {
        int n = shuffledXIndexes.Length;
        while (n > 1)
        {
            int k = RandomFactory.SeedlessRandom.Next(n--);
            (shuffledXIndexes[n], shuffledXIndexes[k]) = (shuffledXIndexes[k], shuffledXIndexes[n]);
        }
    }
    
    /// <summary>
    /// Shuffles the x-axis access indexes using the Fisher-Yates algorithm.
    /// </summary>
    /// <returns>newLength</returns>
    private static int RegenerateAndShuffleXIndexes(int newLength)
    {
        // Regenerate
        for (int i = 0; i < newLength; i++)
        {
            shuffledXIndexes[i] = i;
        }
        
        // Shuffle
        int n = newLength;
        while (n > 1)
        {
            int k = RandomFactory.SeedlessRandom.Next(n--);
            (shuffledXIndexes[n], shuffledXIndexes[k]) = (shuffledXIndexes[k], shuffledXIndexes[n]);
        }

        return newLength;
    }

    public static void SetEveryChunkDirty()
    {
        for (int y = 0; y < chunkCountY; y++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                chunks[x, y].SetEverythingDirty();
            }
        }
    }

    public static void SetEveryChunkClean()
    {
        for (int y = 0; y < chunkCountY; y++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                chunks[x, y].SetEverythingClean();
            }
        }
    }
}