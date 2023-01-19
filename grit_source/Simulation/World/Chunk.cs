
using System;

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
    
    public readonly Vector2Int ChunkWorldPos;

    
    // Internal values used to determine dirty changes.
    private bool internalIsDirty;
    private int internalMinX;
    private int internalMinY;
    private int internalMaxX;
    private int internalMaxY;

    private Simulation simulation;


    public Chunk(Vector2Int chunkWorldPos, Simulation host)
    {
        ChunkWorldPos = chunkWorldPos;
        simulation = host;
        
        SetEverythingClean();
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
        internalMinX = ChunkWorldPos.x * Settings.WORLD_CHUNK_SIZE;
        internalMinY = ChunkWorldPos.y * Settings.WORLD_CHUNK_SIZE;
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
}