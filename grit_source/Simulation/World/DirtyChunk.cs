
using System;

namespace Grit.Simulation.World;

/// <summary>
/// Contains a single Dirty Rect.
/// </summary>
public class DirtyChunk
{

    #region PUBLIC FIELDS

    // Dirty rect values, constructed at the end of a frame from the dirty changes.
    public bool IsCurrentlyDirty;
    public int DirtyRectMinX;
    public int DirtyRectMinY;
    public int DirtyRectMaxX;
    public int DirtyRectMaxY;

    #endregion


    #region PRIVATE FIELDS

    // Internal values used to determine dirty changes.
    private bool internalIsDirty;
    private int internalMinX;
    private int internalMinY;
    private int internalMaxX;
    private int internalMaxY;
    
    private readonly int chunkPosX;
    private readonly int chunkPosY;

    #endregion

    
    #region PUBLIC METHODS

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
        ConstructDirtyRect();
        
        CleanInternally();
    }

    public void SetEverythingDirty()
    {
        // Dirty internally
        internalIsDirty = true;
        internalMinX = chunkPosX;
        internalMinY = chunkPosY;
        internalMaxX = chunkPosX + Settings.WORLD_CHUNK_SIZE - 1;
        internalMaxY = chunkPosY + Settings.WORLD_CHUNK_SIZE - 1;
        
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