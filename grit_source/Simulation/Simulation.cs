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

    protected abstract Color[] FrameBuffer { get; }

    protected abstract RectangleF[] DirtyRects { get; }
    
    
    protected Simulation()
    {
        
    }

    
    public void FixedUpdate()
    {
        HandleUpdateSimulation();
        
        TransferDirtyRects?.Invoke(DirtyRects);
                
        TransferFramebuffer?.Invoke(FrameBuffer);
    }


    /// <summary>
    /// Returns the element at the specified index.
    /// </summary>
    public abstract Element GetElementAt(int index);

    
    /// <summary>
    /// Swaps two elements with each other.
    /// </summary>
    public abstract void SwapElementsAt(int x1, int y1, int x2, int y2);

    
    /// <summary>
    /// Places the given element to the given position in the matrix.
    /// </summary>
    public abstract void SetElementAt(int setX, int setY, Element newElement);

    
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