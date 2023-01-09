using System;
using System.Collections;
using Grit.Simulation.Elements;
using Grit.Simulation.Elements.ElementDefinitions;
using Grit.Simulation.Elements.Movable;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Grit.Simulation.World;

public static class WorldMatrix
{
    public static int WorldWidth;
    public static int WorldHeight;
    
    public static Color[] PixelsToDraw;
    
    /// <summary>
    /// World. Get element at [x, y] by [x + y * Width].
    /// TODO: Research whether 2D-array or flattened array has better performance.
    /// </summary>
    public static ElementDefinition[] Matrix;
    public static DirtyChunk[,] Chunks;

    public static RectangleF[] DirtyRects;
    
    // All pixels that have been stepped already this frame.
    private static BitArray steppedPixels;

    private static int chunkCountX;
    private static int chunkCountY;
    
    // Shuffled array of access indexes for the world's x-axis. Prevents visually consistent updating of the world.
    private static int[] shuffledXIndexes;
    
    private static bool InsideMatrixBounds(int x, int y) => x >= 0 && y >= 0 && x < WorldWidth && y < WorldHeight;

    public static void Initialize(int worldWidth, int worldHeight)
    {
        if (Settings.USE_CHUNKS && (worldWidth % Settings.CHUNK_SIZE != 0 || worldHeight % Settings.CHUNK_SIZE != 0))
            throw new Exception($"World size is not dividable by ChunkSize {Settings.CHUNK_SIZE}!");

        WorldWidth = worldWidth;
        WorldHeight = worldHeight;
        PixelsToDraw = new Color[WorldWidth * WorldWidth];
        
        steppedPixels = new BitArray(WorldWidth * WorldHeight);
        shuffledXIndexes = new int[WorldWidth];

        if(Settings.USE_CHUNKS)
            InitializeChunks();
        
        InitializeWorld();
        
        ShuffleXAccessIndexes();
    }

    private static void InitializeChunks()
    {
        chunkCountX = WorldWidth / Settings.CHUNK_SIZE;
        chunkCountY = WorldHeight / Settings.CHUNK_SIZE;
        Chunks = new DirtyChunk[chunkCountX, chunkCountY];

        if (Settings.DRAW_DIRTY_RECTS)
        {
            DirtyRects = new RectangleF[chunkCountX * chunkCountY];
        }
        
        for (int x = 0; x < chunkCountX; x++)
        {
            for (int y = 0; y < chunkCountY; y++)
            {
                Chunks[x, y] = new DirtyChunk();
            }
        }
    }

    private static void InitializeWorld()
    {
        Matrix = new ElementDefinition[WorldWidth * WorldHeight];
        
        for (int x = 0; x < WorldWidth; x++)
        {
            shuffledXIndexes[x] = x;

            for (int y = 0; y < WorldHeight; y++)
            {
                SetElementAt(x, y, new AirElementDefinition(x, y));

                if (y == Settings.WORLD_HEIGHT - 50)
                {
                    SetElementAt(x, y, new StoneElementDefinition(x, y));
                }
            }
        }
    }

    public static void StepAll(float deltaTime)
    {
        steppedPixels.SetAll(false);
        
        if (Settings.USE_CHUNKS)
        {
            // Loop all chunks (left to right, bottom to top)
            for (int chunkY = chunkCountY - 1; chunkY >= 0; chunkY--)
            {
                //TODO: Can be randomized with shuffled X-access indices
                for (int chunkX = 0; chunkX < chunkCountX; chunkX++)
                {
                    // If the chunk is dirty
                    if (Chunks[chunkX, chunkY].LastFrameWasDirty)
                    {
                        // Loop the dirty rect (left to right, bottom to top)
                        //for (int dirtyRectY = Chunks[chunkX, chunkY].LastFrameDirtyMinY; dirtyRectY <= Chunks[chunkX, chunkY].LastFrameDirtyMaxY + 1; dirtyRectY++)
                        for (int dirtyRectY = Chunks[chunkX, chunkY].LastFrameDirtyMinY; dirtyRectY <= Chunks[chunkX, chunkY].LastFrameDirtyMaxY; dirtyRectY++)
                        {
                            //TODO: Can be randomized with shuffled X-access indices
                            //for (int dirtyRectX = Chunks[chunkX, chunkY].LastFrameDirtyMinX; dirtyRectX < Chunks[chunkX, chunkY].LastFrameDirtyMaxX + 1; dirtyRectX++)
                            for (int dirtyRectX = Chunks[chunkX, chunkY].LastFrameDirtyMinX; dirtyRectX < Chunks[chunkX, chunkY].LastFrameDirtyMaxX; dirtyRectX++)
                            {
                                if(steppedPixels[dirtyRectX + dirtyRectY * Settings.WORLD_WIDTH])
                                    continue;
                                
                                (int newX, int newY) = HandleStep(dirtyRectX, dirtyRectY, deltaTime);

                                steppedPixels[newX + newY * Settings.WORLD_WIDTH] = true;
                            }
                        }

                        // Mark the chunk as clean
                        //Chunks[chunkX, chunkY].Clean();
                    }
                    
                    if (Settings.DRAW_DIRTY_RECTS)
                    {
                        DirtyRects[chunkX + chunkY * chunkCountY] = 
                            new RectangleF(
                                Chunks[chunkX, chunkY].LastFrameDirtyMinX, 
                                Chunks[chunkX, chunkY].LastFrameDirtyMinY, 
                                Chunks[chunkX, chunkY].LastFrameDirtyMaxX + 1 - Chunks[chunkX, chunkY].LastFrameDirtyMinX, 
                                Chunks[chunkX, chunkY].LastFrameDirtyMaxY + 1 - Chunks[chunkX, chunkY].LastFrameDirtyMinY);
                    }
                    
                    Chunks[chunkX, chunkY].AfterReadDirty();
                }
            }
        }
        else
        {
            // Loop the world (left to right, bottom to top)
            for (int y = WorldHeight - 1; y >= 0; y--)
            {
                foreach (int x in shuffledXIndexes)
                {
                    if(steppedPixels[x + y * Settings.WORLD_WIDTH])
                        continue;
                    
                    (int newX, int newY) = HandleStep(x, y, deltaTime);

                    steppedPixels[newX + newY * Settings.WORLD_WIDTH] = true;
                }
            }
        }
    }

    public static void SetElementAt(int setX, int setY, ElementDefinition elementDefinition)
    {
        int index = setX + setY * WorldWidth;
        Matrix[index] = elementDefinition;
        PixelsToDraw[index] = elementDefinition.GetColor();

        if (Settings.USE_CHUNKS)
        {
            // Dirty the set element and all 8 surrounding elements
            //WARN: TODO: Only set dirty the 4 "corner" cells, no need to dirty every cell.
            int minX = Math.Max(0, setX - 1);
            int minY = Math.Max(0, setY - 1);
            int maxX = Math.Min(Settings.WORLD_WIDTH - 1, setX + 1);
            int maxY = Math.Min(Settings.WORLD_WIDTH - 1, setY + 1);
            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    int chunkX = x / Settings.CHUNK_SIZE;
                    int chunkY = y / Settings.CHUNK_SIZE;
                    Chunks[chunkX, chunkY].SetDirtyAt(x, y);
                }
            }
        }
    }

    public static void SwapElementsAt(int x1, int y1, int x2, int y2)
    {
        //TODO: Remove when update logic is finished.
        if (x1 == x2 && y1 == y2)
        {
            throw new Exception("Tried to swap an element with itself.");
        }
        
        int index1 = x1 + y1 * WorldWidth;
        int index2 = x2 + y2 * WorldWidth;
        (Matrix[index2], Matrix[index1]) = (Matrix[index1], Matrix[index2]);
        (PixelsToDraw[x2 + y2 * WorldWidth], PixelsToDraw[x1 + y1 * WorldWidth]) = (PixelsToDraw[x1 + y1 * WorldWidth], PixelsToDraw[x2 + y2 * WorldWidth]);

        // if (Settings.USE_CHUNKS)
        // {
        //     int chunk1X = x1 / Settings.CHUNK_SIZE;
        //     int chunk1Y = y1 / Settings.CHUNK_SIZE;
        //     Chunks[chunk1X, chunk1Y].SetDirtyAt(x1, y1);
        //     
        //     int chunk2X = x2 / Settings.CHUNK_SIZE;
        //     int chunk2Y = y2 / Settings.CHUNK_SIZE;
        //     Chunks[chunk2X, chunk2Y].SetDirtyAt(x2, y2);
        // }
        
        if (Settings.USE_CHUNKS)
        {
            // Dirty the set elements and all (max 14, when diagonal swap) surrounding elements.
            //WARN: TODO: Only set dirty the 4 "corner" cells, no need to dirty every cell.
            int minX = Math.Max(0, Math.Min(x1 - 1, x2 - 1));
            int minY = Math.Max(0, Math.Min(y1 - 1, y2 - 1));
            int maxX = Math.Min(Settings.WORLD_WIDTH - 1, Math.Max(x1 + 1, x2 + 1));
            int maxY = Math.Min(Settings.WORLD_WIDTH - 1, Math.Max(y1 + 1, y2 + 1));
            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    int chunkX = x / Settings.CHUNK_SIZE;
                    int chunkY = y / Settings.CHUNK_SIZE;
                    Chunks[chunkX, chunkY].SetDirtyAt(x, y);
                }
            }
        }
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
        
        ElementDefinition elementDefinition = Matrix[x + y * WorldWidth];
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
                if (Matrix[x + belowY * WorldWidth].GetForm() != ElementForm.Solid)
                {
                    SwapElementsAt(x, y, x, belowY);
                    newY = belowY;
                    break;
                }

                if (RandomFactory.RandomBool())
                {
                    // Left bottom cell.
                    int leftX = x - 1;
                    if (leftX > -1 && Matrix[leftX + belowY * WorldWidth].GetForm() != ElementForm.Solid)
                    {
                        SwapElementsAt(x, y, leftX, belowY);
                        newX = leftX;
                        newY = belowY;
                        break;
                    }
                }
                else
                {
                    // Right bottom cell.
                    int rightX = x + 1;
                    if (rightX < Settings.WORLD_WIDTH && Matrix[rightX + belowY * WorldWidth].GetForm() != ElementForm.Solid)
                    {
                        SwapElementsAt(x, y, rightX, belowY);
                        newX = rightX;
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
                if (Matrix[x + belowY * WorldWidth].GetForm() == ElementForm.Gas)
                {
                    SwapElementsAt(x, y, x, belowY);
                    newY = belowY;
                    break;
                }

                bool prioritizeLeft = RandomFactory.RandomBool();
                if (prioritizeLeft)
                {
                    // Left bottom cell.
                    if (leftX > -1 && Matrix[leftX + belowY * WorldWidth].GetForm() == ElementForm.Gas)
                    {
                        SwapElementsAt(x, y, leftX, belowY);
                        newX = leftX;
                        newY = belowY;
                        break;
                    }
                }
                else
                {
                    // Right bottom cell.
                    if (rightX < Settings.WORLD_WIDTH && Matrix[rightX + belowY * WorldWidth].GetForm() == ElementForm.Gas)
                    {
                        SwapElementsAt(x, y, rightX, belowY);
                        newX = rightX;
                        newY = belowY;
                        break;
                    }
                }

                if (prioritizeLeft)
                {
                    // Left cell.
                    if (leftX > -1 && Matrix[leftX + y * WorldWidth].GetForm() == ElementForm.Gas)
                    {
                        SwapElementsAt(x, y, leftX, y);
                        newX = leftX;
                        break;
                    }
                }
                else
                {
                    // Right cell.
                    if (rightX < Settings.WORLD_WIDTH && Matrix[rightX + y * WorldWidth].GetForm() == ElementForm.Gas)
                    {
                        SwapElementsAt(x, y, rightX, y);
                        newX = rightX;
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
    
    /// <summary>
    /// Shuffles the x-axis access indexes using the Fisher-Yates algorithm.
    /// </summary>
    private static void ShuffleXAccessIndexes()
    {
        int n = shuffledXIndexes.Length;
        while (n > 1)
        {
            int k = RandomFactory.SeedlessRandom.Next(n--);
            (shuffledXIndexes[n], shuffledXIndexes[k]) = (shuffledXIndexes[k], shuffledXIndexes[n]);
        }
    }
}