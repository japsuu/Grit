using System;
using System.Diagnostics;
using System.Threading;
using Grit.Simulation.Elements;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Grit.Simulation;

public abstract class Simulation
{
    private const double MAX_SECONDS_PER_TICK = 0.25;

    public event Action<Color[]> TransferFramebuffer;
    public event Action<RectangleF[]> TransferDirtyRects;

    /// <summary>
    /// Pixels that get drawn to the screen at the end of the frame.
    /// </summary>
    protected volatile Color[] FrontBuffer;

    private Thread simulationManagerThread;
    private volatile bool shouldTerminate;
    
    // Fixed update loop fields.
    private double previousTime;
    private double accumulator;
    private double alpha;

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
    
    
    public virtual void Dispose()
    {
        shouldTerminate = true;
    }
    
    
    protected Simulation(int targetTps)
    {
        FrontBuffer = new Color[Settings.WORLD_WIDTH * Settings.WORLD_HEIGHT];
        simulationManagerThread = new Thread(() => SimulationLoop(targetTps));
        simulationManagerThread.Start();
    }

    
    private void SimulationLoop(int targetTicksPerSecond)
    {
        
        // 0.02 = 50tps
        double fixedUpdateDeltaTime = 0.02;
        
        Stopwatch stopWatch = new();
        stopWatch.Start();

        // Fixed timestep implementation
        // Explanation: https://gafferongames.com/post/fix_your_timestep/.
        while (!shouldTerminate)
        {
            const bool unlockTimestep = false;
            if (unlockTimestep)
            {
                FixedUpdate(fixedUpdateDeltaTime, alpha);
                if(Settings.RANDOM_TICKS_ENABLED) StepRandomTicks(fixedUpdateDeltaTime, alpha);
                if (Settings.CHUNKING_ENABLED) TransferDirtyRects?.Invoke(GetDirtyRects());
                
                TransferFramebuffer?.Invoke(FrontBuffer);
            }
            else
            {
                double newTime = stopWatch.ElapsedMilliseconds;
                double frameTime = newTime - previousTime;
                if (frameTime > MAX_SECONDS_PER_TICK)
                {
                    frameTime = MAX_SECONDS_PER_TICK;
                }
                
                previousTime = newTime;

                accumulator += frameTime;

                while (accumulator >= fixedUpdateDeltaTime)
                {
                    FixedUpdate(fixedUpdateDeltaTime, alpha);
                    if(Settings.RANDOM_TICKS_ENABLED) StepRandomTicks(fixedUpdateDeltaTime, alpha);
                    if (Settings.CHUNKING_ENABLED) TransferDirtyRects?.Invoke(GetDirtyRects());
                
                    TransferFramebuffer?.Invoke(FrontBuffer);
                
                    accumulator -= fixedUpdateDeltaTime;
                }

                alpha = accumulator / fixedUpdateDeltaTime;
            }
        }
    }

    
    protected abstract void FixedUpdate(double dt, double a);


    /// <summary>
    /// Called after FixedUpdate
    /// </summary>
    protected abstract void StepRandomTicks(double dt, double a);


    /// <summary>
    /// Forces all cells to step next update.
    /// </summary>
    public abstract void ForceStepAll();


    /// <summary>
    /// Forces all cells to skip their step next update.
    /// </summary>
    public abstract void ForceSkipAll();
}