using System;
using System.Diagnostics;
using System.Threading;
using Grit.Simulation.Elements;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Grit.Simulation;

public abstract class Simulation
{
    public event Action<Color[]> TransferFramebuffer;
    public event Action<RectangleF[]> TransferDirtyRects;

    /// <summary>
    /// Pixels that get drawn to the screen at the end of the frame.
    /// </summary>
    protected readonly Color[] FrameBuffer;

    protected virtual RectangleF[] GetDirtyRects() => null;


    public abstract Element GetElementAt(int index);

    
    /// <summary>
    /// Swaps two elements with each other.
    /// </summary>
    public abstract void SwapElementsAt(int x1, int y1, int x2, int y2);

    
    /// <summary>
    /// Places the given element to the given position in the matrix.
    /// </summary>
    public abstract void SetElementAt(int setX, int setY, Element newElement);
    
    
    protected Simulation()
    {
        FrameBuffer = new Color[Settings.WORLD_WIDTH * Settings.WORLD_HEIGHT];
    }

    
    public void FixedUpdate()
    {
        HandleUpdateSimulation();
        
        TransferDirtyRects?.Invoke(GetDirtyRects());
                
        TransferFramebuffer?.Invoke(FrameBuffer);
    }

    
    protected abstract void HandleUpdateSimulation();


    /// <summary>
    /// Forces all cells to step next update.
    /// </summary>
    public abstract void ForceStepAll();


    /// <summary>
    /// Forces all cells to skip their step next update.
    /// </summary>
    public abstract void ForceSkipAll();
}