
using System;
using Grit.Simulation.Helpers;
using MonoGame.Extended;

namespace Grit.Simulation.World;

/// <summary>
/// Double buffered dirty rectangle.
/// </summary>
public class DirtyRectangle
{
    // Dirty rect values, constructed at the end of a frame from the dirty changes.
    public bool Active;
    public int MinX;
    public int MinY;
    public int MaxX;
    public int MaxY;
    public int Width => MaxX - MinX;
    public int Height => MaxY - MinY;

    public RectangleF AsRectangleF(int posX, int posY)
    {
        return new RectangleF(
            posX + MinX, 
            posY + MinY, 
            Width + 1,
            Height + 1);
    }

    // Internal values used to determine dirty changes.
    private bool active;
    private int internalMinX;
    private int internalMinY;
    private int internalMaxX;
    private int internalMaxY;
    

    #region PUBLIC METHODS

    public DirtyRectangle()
    {
        SetEverythingClean();
    }

    
    public void SetDirtyAt(int x, int y)
    {
        // Resize the current dirty rect if needed.
        internalMinX = Math.Min(internalMinX, x);
        internalMinY = Math.Min(internalMinY, y);
        internalMaxX = Math.Max(internalMaxX, x);
        internalMaxY = Math.Max(internalMaxY, y);

        active = true;
    }

    /// <summary>
    /// Needs to be called after all the frame's updates are done.
    /// </summary>
    public void Update()
    {
        ConstructDirtyRect();
        
        CleanInternally();
    }

    public void SetEverythingDirty()
    {
        // Dirty internally
        active = true;
        internalMinX = 0;
        internalMinY = 0;
        internalMaxX = Settings.WORLD_CHUNK_SIZE - 1;
        internalMaxY = Settings.WORLD_CHUNK_SIZE - 1;
        
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
        active = false;
        internalMinX = int.MaxValue;
        internalMinY = int.MaxValue;
        internalMaxX = int.MinValue;
        internalMaxY = int.MinValue;
    }

    private void ConstructDirtyRect()
    {
        Active = active;
        MinX = internalMinX;
        MinY = internalMinY;
        MaxX = internalMaxX;
        MaxY = internalMaxY;
    }

    #endregion
}