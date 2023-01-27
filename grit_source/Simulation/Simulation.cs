
using System;
using System.Diagnostics;
using Grit.Simulation.Elements;
using Grit.Simulation.Rendering;
using Grit.Simulation.World.Regions.Chunks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Grit.Simulation;

public class Simulation
{
    private readonly ChunkManager chunkManager;
    private readonly SimulationRenderer renderer;
    
    private static Point SnapPositionToChunkGrid(int worldRelativeX, int worldRelativeY) =>
        new(worldRelativeX & ~63, worldRelativeY & ~63);


    private static (int chunkRelativeX, int chunkRelativeY) GetPositionInsideContainingChunk(int worldRelativeX, int worldRelativeY)
    {
        return (worldRelativeX & 63, worldRelativeY & 63);
    }


    public Simulation()
    {
        chunkManager = new ChunkManager(this);
        renderer = new SimulationRenderer(this, chunkManager);
    }

    
    public void FixedUpdate()
    {
        HandleUpdateSimulation();
        
        renderer.FixedUpdate();
    }


    public void Draw(SpriteBatch spriteBatch, Matrix cameraMatrix)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: cameraMatrix);

        renderer.DrawWorld(spriteBatch);

        spriteBatch.End();

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        renderer.DrawUi(spriteBatch);

        spriteBatch.End();
    }


    public Element GetElementAt(int worldRelativeX, int worldRelativeY)
    {
        // Figure out the chunk at given position
        Point chunkPosition = SnapPositionToChunkGrid(worldRelativeX, worldRelativeY);

        if (chunkManager.GetChunkAt(chunkPosition, out Chunk chunk))
        {
            (int chunkRelativeX, int chunkRelativeY) = GetPositionInsideContainingChunk(worldRelativeX, worldRelativeY);

            return chunk.GetElementAt(chunkRelativeX, chunkRelativeY);
        }
        else
        {
            //System.Diagnostics.Debug.WriteLine($"Tried to get element outside of loaded chunks (cP:{chunkPosition})!");
            return null;
            throw new Exception("Tried to get element outside of loaded chunks!");
        }
    }


    public Element GetElementAt(int worldRelativeX, int worldRelativeY, out Chunk containingChunk)
    {
        // Figure out the chunk at given position
        Point chunkPosition = SnapPositionToChunkGrid(worldRelativeX, worldRelativeY);

        if (chunkManager.GetChunkAt(chunkPosition, out containingChunk))
        {
            (int chunkRelativeX, int chunkRelativeY) = GetPositionInsideContainingChunk(worldRelativeX, worldRelativeY);

            return containingChunk.GetElementAt(chunkRelativeX, chunkRelativeY);
        }
        else
        {
            //System.Diagnostics.Debug.WriteLine($"Tried to get element outside of loaded chunks (cP:{chunkPosition})!");
            return null;
            throw new Exception("Tried to get element outside of loaded chunks!");
        }
    }


    public void SetElementAt(int worldRelativeX, int worldRelativeY, Element newElement)
    {
        // Figure out the chunk at given position
        Point chunkPosition = SnapPositionToChunkGrid(worldRelativeX, worldRelativeY);

        if (chunkManager.GetChunkAt(chunkPosition, out Chunk chunk))
        {
            (int chunkRelativeX, int chunkRelativeY) = GetPositionInsideContainingChunk(worldRelativeX, worldRelativeY);
            
            chunk.SetElement(chunkRelativeX, chunkRelativeY, newElement);
            
            // Updating the dirty rect:
            // Dirty the set element and all 8 surrounding elements.
            int minX = worldRelativeX - 1;
            int minY = worldRelativeY - 1;
            int maxX = worldRelativeX + 1;
            int maxY = worldRelativeY + 1;
            for (int y = minY; y < maxY + 1; y++)
            {
                for (int x = minX; x < maxX + 1; x++)
                {
                    SetDirtyAt(x, y);
                }
            }
        }
        else
        {
            throw new Exception($"Tried to set element outside of loaded chunks (cP:{chunkPosition})!");
        }
    }

    
    public void SwapElementsAt(int x1, int y1, int x2, int y2)
    {
        // Do not allow an cell to swap itself.
        if (x1 == x2 && y1 == y2)
        {
            throw new Exception("Tried to swap an element with itself.");
        }
        
        Point chunkPosition1 = SnapPositionToChunkGrid(x1, y1);
        Point chunkPosition2 = SnapPositionToChunkGrid(x2, y2);

        if (chunkManager.GetChunkAt(chunkPosition1, out Chunk chunk1) && chunkManager.GetChunkAt(chunkPosition2, out Chunk chunk2))
        {
            // Swap operations
            (int chunkRelativeX1, int chunkRelativeY1) = GetPositionInsideContainingChunk(x1, y1);
            (int chunkRelativeX2, int chunkRelativeY2) = GetPositionInsideContainingChunk(x2, y2);
            chunk1.SwapElements(chunkRelativeX1, chunkRelativeY1, chunk2, chunkRelativeX2, chunkRelativeY2);
            
            // Updating the dirty rect:
            // Dirty the set elements and all (max 12, when diagonal swap) surrounding elements.
            int minX = Math.Min(x1 - 1, x2 - 1);
            int minY = Math.Min(y1 - 1, y2 - 1);
            int maxX = Math.Max(x1 + 1, x2 + 1);
            int maxY = Math.Max(y1 + 1, y2 + 1);
            for (int y = minY; y < maxY + 1; y++)
            {
                for (int x = minX; x < maxX + 1; x++)
                {
                    SetDirtyAt(x, y);
                }
            }
        }
        else
        {
            throw new Exception("Tried to swap element outside of loaded chunks!");
        }
    }


    public void SetDirtyAt(int worldX, int worldY)
    {
        // Convert world position to a chunk's position
        Point chunkToDirtyPosition = SnapPositionToChunkGrid(worldX, worldY);
        if (chunkManager.GetChunkAt(chunkToDirtyPosition, out Chunk chunkToDirty))
        {
            // Convert world position to a chunk relative position
            (int chunkRelativeX, int chunkRelativeY) = GetPositionInsideContainingChunk(worldX, worldY);
            chunkToDirty.SetDirtyAt(chunkRelativeX, chunkRelativeY);
        }
        else
        {
            throw new Exception("Tried to dirty element outside of loaded chunks!");
        }
    }


    // Called by Chunks when they update an element, which ends up outside of the originating chunk.
    public void SetSteppedAt(int worldX, int worldY)
    {
        // Convert world position to a chunk's position
        Point chunkPosition = SnapPositionToChunkGrid(worldX, worldY);
        if (chunkManager.GetChunkAt(chunkPosition, out Chunk chunkToDirty))
        {
            // Convert world position to a chunk relative position
            (int chunkRelativeX, int chunkRelativeY) = GetPositionInsideContainingChunk(worldX, worldY);
            chunkToDirty.SetSteppedAt(chunkRelativeX, chunkRelativeY);
        }
        else
        {
            return;
            throw new Exception("Tried to set element stepped outside of loaded chunks!");
        }
    }


    private void HandleUpdateSimulation()
    {
        chunkManager.Update();
        
        for (int i = 0; i < chunkManager.CurrentlyTickingChunks.Count; i++)
        {
            chunkManager.CurrentlyTickingChunks[i].ProcessTick();
            if(Settings.RANDOM_TICKS_ENABLED)
                chunkManager.CurrentlyTickingChunks[i].ProcessRandomTick();
        }
    }


    /// <summary>
    /// Forces all cells to step next update.
    /// </summary>
    public void ForceDirtyAll()
    {
        foreach (Chunk c in chunkManager.CurrentlyTickingChunks)
        {
            c.SetEverythingDirty();
        }
    }


    /// <summary>
    /// Forces all cells to skip their step next update.
    /// </summary>
    public void ForceCleanAll()
    {
        foreach (Chunk c in chunkManager.CurrentlyTickingChunks)
        {
            c.SetEverythingClean();
        }
    }

    public void Dispose()
    {
        chunkManager.Unload();
    }
}