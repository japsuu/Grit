
using System;

namespace Grit.Simulation.World;

/// <summary>
/// Unsafe representation of a chunk, used to keep track of dirty rects.
/// </summary>
public class DirtyChunk
{
    
    // Values constructed at the end of a frame from the dirty changes.
    public bool IsCurrentlyDirty;
    public int ConstructedMinX;
    public int ConstructedMinY;
    public int ConstructedMaxX;
    public int ConstructedMaxY;

    // Internal values used to determine dirty changes.
    private bool internalIsDirty;
    private int internalMinX;
    private int internalMinY;
    private int internalMaxX;
    private int internalMaxY;
    
    private readonly int chunkPosX;
    private readonly int chunkPosY;

    public DirtyChunk(int chunkPosX, int chunkPosY)
    {
        this.chunkPosX = chunkPosX;
        this.chunkPosY = chunkPosY;
        
        SetEverythingClean();
    }

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
        // Write the public values
        IsCurrentlyDirty = internalIsDirty;
        ConstructedMinX = internalMinX;
        ConstructedMinY = internalMinY;
        ConstructedMaxX = internalMaxX;
        ConstructedMaxY = internalMaxY;
        
        internalIsDirty = false;
        internalMinX = int.MaxValue;
        internalMinY = int.MaxValue;
        internalMaxX = int.MinValue;
        internalMaxY = int.MinValue;
    }

    public void SetEverythingDirty()
    {
        // Dirty internally
        internalIsDirty = true;
        internalMinX = chunkPosX;
        internalMinY = chunkPosY;
        internalMaxX = chunkPosX + Settings.CHUNK_SIZE - 1;
        internalMaxY = chunkPosY + Settings.CHUNK_SIZE - 1;
        
        // Propagate
        IsCurrentlyDirty = internalIsDirty;
        ConstructedMinX = internalMinX;
        ConstructedMinY = internalMinY;
        ConstructedMaxX = internalMaxX;
        ConstructedMaxY = internalMaxY;
    }

    public void SetEverythingClean()
    {
        // Clean internally
        internalIsDirty = false;
        internalMinX = int.MaxValue;
        internalMinY = int.MaxValue;
        internalMaxX = int.MinValue;
        internalMaxY = int.MinValue;
        
        // Propagate
        IsCurrentlyDirty = internalIsDirty;
        ConstructedMinX = internalMinX;
        ConstructedMinY = internalMinY;
        ConstructedMaxX = internalMaxX;
        ConstructedMaxY = internalMaxY;
    }
}