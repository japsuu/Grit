
using System;

namespace Grit.Simulation.World;

/// <summary>
/// Unsafe representation of a chunk, used to keep track of dirty rects.
/// </summary>
public class DirtyChunk
{
    private const int DIRTY_GRACE_FRAMES = 3;
    
    public bool LastFrameWasDirty;

    public int LastFrameDirtyMinX;
    public int LastFrameDirtyMinY;
    public int LastFrameDirtyMaxX;
    public int LastFrameDirtyMaxY;

    private bool thisFrameIsDirty;
    private int thisFrameDirtyMinX;
    private int thisFrameDirtyMinY;
    private int thisFrameDirtyMaxX;
    private int thisFrameDirtyMaxY;

    private int currentGraceFrames = 0;

    /// <summary>
    /// Sets the given position as dirty inside this chunk.
    /// </summary>
    public void SetDirtyAt(int x, int y)
    {
        // Determine if the current dirty rect needs to be resized
        thisFrameDirtyMinX = Math.Min(thisFrameDirtyMinX, x);
        thisFrameDirtyMinY = Math.Min(thisFrameDirtyMinY, y);
        thisFrameDirtyMaxX = Math.Max(thisFrameDirtyMaxX, x);
        thisFrameDirtyMaxY = Math.Max(thisFrameDirtyMaxY, y);

        thisFrameIsDirty = true;
        
        currentGraceFrames = DIRTY_GRACE_FRAMES;
    }

    /// <summary>
    /// Needs to be called after each update round.
    /// </summary>
    public void AfterReadDirty()
    {
        //LastFrameWasDirty = true;
        LastFrameWasDirty = thisFrameIsDirty;
        LastFrameDirtyMinX = thisFrameDirtyMinX;
        LastFrameDirtyMinY = thisFrameDirtyMinY;
        LastFrameDirtyMaxX = thisFrameDirtyMaxX;
        LastFrameDirtyMaxY = thisFrameDirtyMaxY;
        if (currentGraceFrames > 0)
        {
            currentGraceFrames -= 1;
        }
        else
        {
            thisFrameIsDirty = false;
            thisFrameDirtyMinX = int.MaxValue;
            thisFrameDirtyMinY = int.MaxValue;
            thisFrameDirtyMaxX = int.MinValue;
            thisFrameDirtyMaxY = int.MinValue;
        }
    }

    public void SetEverythingDirty(int myX, int myY)
    {
        thisFrameDirtyMinX = myX;
        thisFrameDirtyMinY = myY;
        thisFrameDirtyMaxX = myX + Settings.CHUNK_SIZE - 1;
        thisFrameDirtyMaxY = myY + Settings.CHUNK_SIZE - 1;
        thisFrameIsDirty = true;
        LastFrameDirtyMinX = thisFrameDirtyMinX;
        LastFrameDirtyMinY = thisFrameDirtyMinY;
        LastFrameDirtyMaxX = thisFrameDirtyMaxX;
        LastFrameDirtyMaxY = thisFrameDirtyMaxY;
        LastFrameWasDirty = thisFrameIsDirty;
    }

    public void SetEverythingClean()
    {
        thisFrameIsDirty = false;
        thisFrameDirtyMinX = int.MaxValue;
        thisFrameDirtyMinY = int.MaxValue;
        thisFrameDirtyMaxX = int.MinValue;
        thisFrameDirtyMaxY = int.MinValue;
        LastFrameWasDirty = thisFrameIsDirty;
        LastFrameDirtyMinX = thisFrameDirtyMinX;
        LastFrameDirtyMinY = thisFrameDirtyMinY;
        LastFrameDirtyMaxX = thisFrameDirtyMaxX;
        LastFrameDirtyMaxY = thisFrameDirtyMaxY;
    }
}