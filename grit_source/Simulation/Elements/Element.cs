using System;
using Grit.Simulation.Helpers;
using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements;


public abstract class Element
{
    public enum InteractionType : ushort
    {
        Solid,
        Liquid,
        Gas
    }
    
    public abstract ushort Id { get; }
    
    protected abstract InteractionType InitialInteractionType { get; }
    
    protected abstract Color InitialColor { get; }

    protected readonly int StartX;
    protected readonly int StartY;

    protected bool IsFreeFalling;

    /// <summary>
    /// cell/s.
    /// </summary>
    protected Vector2 Velocity;

    // Store the decimal parts of the velocity component.
    private float velocityDecimalPartX;
    private float velocityDecimalPartY;


    protected Element(int x, int y)
    {
        StartX = x;
        StartY = y;
    }
    
    
    public virtual void Tick(Simulation simulation, int startX, int startY)
    {
        
    }

    
    public virtual bool RandomTick(Simulation simulation, int worldRelativeX, int worldRelativeY)
    {
        return false;
    }

    
    public virtual Color GetColor()
    {
        return InitialColor;
    }
    
    
    public virtual InteractionType GetInteractionType()
    {
        return InitialInteractionType;
    }


    // Simple velocity solver.
    // Allows for sub-cell velocities.
    protected (int nextX, int nextY) GetNextTargetPosition(int currentX, int currentY)
    {
        float frameVelocityX = Velocity.X * Globals.FixedFrameLengthSeconds;
        float frameVelocityY = Velocity.Y * Globals.FixedFrameLengthSeconds;
        
        int truncatedVelocityX = (int)Math.Truncate(frameVelocityX);
        int truncatedVelocityY = (int)Math.Truncate(frameVelocityY);
        
        float velocityDecimalX = frameVelocityX - truncatedVelocityX;
        float velocityDecimalY = frameVelocityY - truncatedVelocityY;

        velocityDecimalPartX += velocityDecimalX;
        velocityDecimalPartY += velocityDecimalY;

        int velocityX = (int)frameVelocityX;
        int velocityY = (int)frameVelocityY;

        if (velocityDecimalPartX >= 1)
        {
            velocityDecimalPartX--;
            velocityX++;
        }

        if (velocityDecimalPartY >= 1)
        {
            velocityDecimalPartY--;
            velocityY++;
        }

        int nextX = currentX + velocityX;
        int nextY = currentY + velocityY;

        if (MathG.ManhattanDistance(currentX, currentY, nextX, nextY) >= Settings.WORLD_CHUNK_SIZE)
        {
            Logger.Write(Logger.LogType.WARN, this, $"Element with too high velocity ([{currentX},{currentY}] -> [{nextX},{nextY}])!");
        }
        
        return (nextX, nextY);
    }
    
    // Heat at which the element catches fire.
    //protected float FlammabilityResistanceCelsius;
    
    //protected float CurrentHeatCelsius;
    
    
    // public virtual bool ReceiveHeat(Simulation simulation, int worldRelativeX, int worldRelativeY, int heatDegreesCelsius)
    // {
    //     CurrentHeatCelsius += heatDegreesCelsius;
    //     if (CurrentHeatCelsius >= FlammabilityResistanceCelsius)
    //     {
    //         throw new NotImplementedException();
    //     }
    //     return true;
    // }
}