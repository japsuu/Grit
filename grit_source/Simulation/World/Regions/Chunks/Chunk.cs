using System;
using System.Collections;
using System.Collections.Generic;
using Grit.Simulation.Elements;
using Grit.Simulation.Elements.ElementDefinitions;
using Grit.Simulation.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Grit.Simulation.World.Regions.Chunks;

public class Chunk
{
    public enum LoadState
    {
        Ticking,
        Loaded,
        Unloading
    }
    
    public readonly Texture2D Canvas;

    public readonly Rectangle Rectangle;

    public RectangleF DirtyRect => dirtyRectangle.AsRectangleF(Rectangle.Location.X, Rectangle.Location.Y);

    public bool IsDirty => dirtyRectangle.Active;

    public float Lifetime { get; private set; }

    public bool ReadyToUnload => Lifetime <= 0f;

    public LoadState State { get; private set; }
    
    public readonly BitArray RandomTickSteppedCells;
    
    private readonly Color[] colorBuffer;
    private readonly Element[] cells;
    private readonly int[,] shuffledIndexes;
    private readonly Simulation simulation;
    private readonly BitArray steppedCells;
    private readonly DirtyRectangle dirtyRectangle;

    public Chunk(Point position, int dimensions, Simulation host)
    {
        Rectangle = new Rectangle(position.X, position.Y, dimensions, dimensions);
        simulation = host;
        colorBuffer = new Color[dimensions * dimensions];
        cells = new Element[dimensions * dimensions];
        shuffledIndexes = new int[dimensions, dimensions];
        steppedCells = new BitArray(dimensions * dimensions);
        dirtyRectangle = new DirtyRectangle();
        Canvas = new Texture2D(Grit.Graphics, dimensions, dimensions);
        RandomTickSteppedCells = new BitArray(dimensions * dimensions);

        InitializeShuffledIndexes();
        
        ShuffleIndexArray();
        
        InitializeContents();
    }


    // Called by ChunkLoader
    public void KeepAlive()
    {
        Lifetime = Settings.UNLOADED_CHUNK_LIFETIME;
        State = LoadState.Loaded;
    }


    // Called by ChunkLoader
    public void DecrementLifetime()
    {
        Lifetime -= Globals.FixedFrameLengthSeconds;
        State = LoadState.Unloading;
    }


    public void SetEverythingDirty() => dirtyRectangle.SetEverythingDirty();
    public void SetEverythingClean() => dirtyRectangle.SetEverythingClean();


    public void SetSteppedAt(int x, int y)
    {
        Logger.Write(Logger.LogType.INFO, this, $"SetSteppedAt {x};{y}");
        steppedCells[x + y * Rectangle.Width] = true;
    }

    
    public void ProcessTick()
    {
        State = LoadState.Ticking;
        steppedCells.SetAll(false);
        
        if (dirtyRectangle.Active)
        {
            //TODO: Remove the following debug flag:
            // Whether or not to loop in random X order.
            if (true)
            {
                // Loop the dirty rect bottom to top
                int width = Rectangle.Width;
                foreach ((int x, int y) in LoopDirtyRectangle())
                {
                    int index = x + y * width;
                
                    // Skip if this cell has been stepped already.
                    if (steppedCells[index])
                        continue;
                                
                    // Finally, handle the step for this cell.
                    //(int newX, int newY) = HandleStep(x, y, deltaTime);
                    cells[index].Tick(simulation, Rectangle.X + x, Rectangle.Y + y);
                }
            }
            else
            {
                for (int y = dirtyRectangle.MinY; y <= dirtyRectangle.MaxY; y++)
                {
                    for (int x = dirtyRectangle.MinX; x <= dirtyRectangle.MaxX; x++)
                    {
                        int index = x + y * Rectangle.Width;
                    
                        // Skip if this cell has been stepped already.
                        if (steppedCells[index])
                            continue;
                                
                        // Finally, handle the step for this cell.
                        //(int newX, int newY) = HandleStep(x, y, deltaTime);
                        cells[index].Tick(simulation, Rectangle.X + x, Rectangle.Y + y);
                    }
                }
            }
        }
        
        dirtyRectangle.Update();
    }

    
    /// <summary>
    /// Selects random cells from the chunk, and updates them.
    /// Used for infrequent things, like interactions with neighbouring cells; for example, melting.
    /// <seealso cref="Settings.RANDOM_TICKS_PER_FRAME"/>
    /// </summary>
    public void ProcessRandomTick()
    {
        if(Settings.DrawRandomTicks)
            RandomTickSteppedCells.SetAll(false);
            
        for (int i = 0; i < Settings.RANDOM_TICKS_PER_FRAME; i++)
        {
            (int x, int y) = RandomFactory.RandomPosInChunk();

            int index = x + y * Rectangle.Width;
            //if (steppedCells[index])
            //    continue;
            
            // No need to manually set the random cell dirty, since we don't want consequent updates to happen.
            // If HandleStep creates a new cell/causes movement, dirtying will be handled internally.
            bool wasTicked = cells[index].RandomTick(simulation, x, y);
            
            if(Settings.DrawRandomTicks)
                RandomTickSteppedCells.Set(index, true);
        }
    }


    public void Render()
    {
        Canvas.SetData(colorBuffer, 0, Rectangle.Width * Rectangle.Height);
    }

    public Element GetElementAt(int chunkRelativeX, int chunkRelativeY)
    {
        return cells[chunkRelativeX + chunkRelativeY * Rectangle.Width];
    }


    public void SetDirtyAt(int x, int y)
    {
        dirtyRectangle.SetDirtyAt(x, y);
    }

    
    public void SetElement(int x, int y, Element newElement)
    {
        int chunkRelativeIndex = x + y * Rectangle.Width;
        cells[chunkRelativeIndex] = newElement;
        colorBuffer[chunkRelativeIndex] = newElement.GetColor();    //BUG: WARN: NOTE: TODO: Move this so that dirty pixels get redrawn/recolored! Move to Element.cs.Draw()?
    }

    
    public void SwapElements(int x, int y, Chunk otherChunk, int otherX, int otherY)
    {
        int chunkRelativeIndex = x + y * Rectangle.Width;
        int otherChunkRelativeIndex = otherX + otherY * Rectangle.Width;
        
        (otherChunk.cells[otherChunkRelativeIndex], cells[chunkRelativeIndex]) = (cells[chunkRelativeIndex], otherChunk.cells[otherChunkRelativeIndex]);
        (otherChunk.colorBuffer[otherChunkRelativeIndex], colorBuffer[chunkRelativeIndex]) = (colorBuffer[chunkRelativeIndex], otherChunk.colorBuffer[otherChunkRelativeIndex]);
    }


    private void InitializeContents()
    {
        for (int x = 0; x < Rectangle.Width; x++)
        {
            for (int y = 0; y < Rectangle.Height; y++)
            {
                int index = x + y * Rectangle.Width;
                int worldX = Rectangle.X + x;
                int worldY = Rectangle.Y + y;
                
                if (Rectangle.Location.Y + y > 200)
                {
                    cells[index] = new StoneElement(worldX, worldY);
                }
                else
                {
                    cells[index] = new AirElement(worldX, worldY);
                }
                
                colorBuffer[index] = cells[index].GetColor();
            }
        }
    }

    
    /// <summary>
    /// Initializes shuffledIndexes based on if we use chunking or not.
    /// </summary>
    private void InitializeShuffledIndexes()
    {
        for (int y = 0; y < shuffledIndexes.GetLength(1); y++)
        {
            for (int x = 0; x < shuffledIndexes.GetLength(0); x++)
            {
                shuffledIndexes[x, y] = x;
            }
        }
    }
    
    
    /// <summary>
    /// Shuffles the x-axis access indexes using the Fisher-Yates algorithm.
    /// </summary>
    private void ShuffleIndexArray()
    {
        int n = shuffledIndexes.GetLength(0);

        for (int y = 0; y < shuffledIndexes.GetLength(1); y++)
        {
            while (n > 1)
            {
                int k = RandomFactory.SeedlessRandom.Next(n--);
                (shuffledIndexes[n, y], shuffledIndexes[k, y]) = (shuffledIndexes[k, y], shuffledIndexes[n, y]);
            }
        }
    }
    
    
    /// <summary>
    /// Shuffles the x-axis access indexes using the Fisher-Yates algorithm.
    /// </summary>
    private void RegenerateAndShuffleXIndexes(int width, int height)
    {
        // Regenerate
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                shuffledIndexes[x, y] = x;
            }
        }
        
        // Shuffle
        for (int y = 0; y < height; y++)
        {
            int n = width;
            while (n > 1)
            {
                // TODO: Use pregenerated random values instead of generating them on the fly.
                int k = RandomFactory.SeedlessRandom.Next(n--);
                (shuffledIndexes[n, y], shuffledIndexes[k, y]) = (shuffledIndexes[k, y], shuffledIndexes[n, y]);
            }
        }
    }
    
    
    /// <summary>
    /// Shuffles the x-axis access indexes using the Fisher-Yates algorithm.
    /// </summary>
    private IEnumerable<(int x, int y)> LoopDirtyRectangle()
    {
        int width = dirtyRectangle.Width;
        int height = dirtyRectangle.Height;
        
        // Regenerate
        // Shuffle
        for (int y = height; y >= 0; y--)
        {
            for (int x = 0; x <= width; x++)
            {
                shuffledIndexes[x, y] = x;
            }
            
            int n = width;
            while (n > 1)
            {
                // TODO: Use pregenerated random values instead of generating them on the fly.
                int k = RandomFactory.SeedlessRandom.Next(n--);
                (shuffledIndexes[n, y], shuffledIndexes[k, y]) = (shuffledIndexes[k, y], shuffledIndexes[n, y]);
            }

            for (int x = 0; x <= width; x++)
            {
                yield return (dirtyRectangle.MinX + shuffledIndexes[x, y], dirtyRectangle.MinY + y);
            }
        }
    }

    public void Dispose()
    {
        Canvas.Dispose();
    }
}