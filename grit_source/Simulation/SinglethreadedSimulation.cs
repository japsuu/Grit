using System;
using Grit.Simulation.Elements;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Grit.Simulation;

public class SinglethreadedSimulation : Simulation
{
    private bool skipStepNextFrame;
    private bool forceStepNextFrame;
    private readonly bool isInitialized;

    protected override Color[] FrameBuffer { get; }
    protected override RectangleF[] DirtyRects { get; }


    public SinglethreadedSimulation()
    {
        //TODO

        isInitialized = true;
    }
    
    
    public override Element GetElementAt(int index)
    {
        throw new NotImplementedException();
    }

    
    public override void SwapElementsAt(int x1, int y1, int x2, int y2)
    {
        throw new NotImplementedException();
    }

    
    public override void SetElementAt(int setX, int setY, Element newElement)
    {
        throw new NotImplementedException();
    }

    
    protected override void HandleUpdateSimulation()
    {
        throw new NotImplementedException();
    }

    
    public override void ForceStepAll()
    {
        skipStepNextFrame = false;
        forceStepNextFrame = true;
    }

    
    public override void ForceSkipAll()
    {
        skipStepNextFrame = true;
        forceStepNextFrame = false;
    }
}