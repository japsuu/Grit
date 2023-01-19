
using System;

namespace Grit.Simulation.World;

/// <summary>
/// Contains a single Dirty Rect.
/// </summary>
public class DirtyChunk
{

    #region PUBLIC FIELDS

    // Dirty rect values, constructed at the end of a frame from the dirty changes.
    public volatile bool IsCurrentlyDirty;
    public volatile int DirtyRectMinX;
    public volatile int DirtyRectMinY;
    public volatile int DirtyRectMaxX;
    public volatile int DirtyRectMaxY;
    
    public readonly int ChunkMatrixPosX;
    public readonly int ChunkMatrixPosY;

    #endregion


    #region PRIVATE FIELDS

    // Internal values used to determine dirty changes.
    private volatile bool internalIsDirty;
    private volatile int internalMinX;
    private volatile int internalMinY;
    private volatile int internalMaxX;
    private volatile int internalMaxY;

    #endregion

    
    #region PUBLIC METHODS

    public DirtyChunk(int chunkMatrixPosX, int chunkMatrixPosY)
    {
        ChunkMatrixPosX = chunkMatrixPosX;
        ChunkMatrixPosY = chunkMatrixPosY;
        
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
        ConstructDirtyRect();
        
        CleanInternally();
    }

    public void SetEverythingDirty()
    {
        // Dirty internally
        internalIsDirty = true;
        internalMinX = ChunkMatrixPosX * Settings.WORLD_CHUNK_SIZE;
        internalMinY = ChunkMatrixPosY * Settings.WORLD_CHUNK_SIZE;
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