using System;
using Grit.Simulation.DebugDraw;
using Grit.Simulation.Helpers;
using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.ElementDefinitions;

public class TestLiquidElement : Element
{
    public TestLiquidElement(int x, int y) : base(x, y)
    {
    }

    public override ushort Id => 98;
    protected override InteractionType InitialInteractionType => InteractionType.Solid;
    protected override Color InitialColor => Color.Brown;


    private int dispersionRate = 4;

    private bool isFreeFalling;
    

    public override void Tick(Simulation simulation, int startX, int startY)
    {
        DebugDrawer.AddDrawable(new StringDrawable(startX, startY, 2f, $"{startX};{startY}", 0.1f, Color.Lime));
        int belowY = startY + 1;

        // Below cell.
        if (simulation.GetElementAt(startX, belowY).GetInteractionType() != InteractionType.Solid)
        {
            simulation.SwapElementsAt(startX, startY, startX, belowY, true, true);
            return;
        }

        // Randomly choose whether to prioritize left or right update
        bool prioritizeLeft = RandomFactory.RandomBool();
        if (prioritizeLeft)
        {
            // Left bottom cell.
            int leftX = startX - 1;
            if (simulation.GetElementAt(leftX, belowY).GetInteractionType() != InteractionType.Solid)
            {
                simulation.SwapElementsAt(startX, startY, leftX, belowY, true, true);
                return;
            }

            // Right bottom cell.
            int rightX = startX + 1;
            if (simulation.GetElementAt(rightX, belowY).GetInteractionType() != InteractionType.Solid)
            {
                simulation.SwapElementsAt(startX, startY, rightX, belowY, true, true);
                return;
            }
        }
        else
        {
            // Right bottom cell.
            int rightX = startX + 1;
            if (simulation.GetElementAt(rightX, belowY).GetInteractionType() != InteractionType.Solid)
            {
                simulation.SwapElementsAt(startX, startY, rightX, belowY, true, true);
                return;
            }

            // Left bottom cell.
            int leftX = startX - 1;
            if (simulation.GetElementAt(leftX, belowY).GetInteractionType() != InteractionType.Solid)
            {
                simulation.SwapElementsAt(startX, startY, leftX, belowY, true, true);
                return;
            }
        }
        
        return;
        
        Velocity.Y += Settings.GRAVITY_CELLS_PER_SECOND;
        
        if (IsFreeFalling)
        {
            Velocity.X *= 0.9f;
        }

        (int endX, int endY) = GetNextTargetPosition(startX, startY);
        
        // Iterate with Bresenham's line algorithm.
        int width = endX - startX;
        int height = endY - startY;
        int dx1 = 0;
        int dy1 = 0;
        int dx2 = 0;
        int dy2 = 0;
        dx1 = width < 0 ? -1 : width > 0 ? 1 : dx1;
        dy1 = height < 0 ? -1 : height > 0 ? 1 : dy1;
        dx2 = width < 0 ? -1 : width > 0 ? 1 : dx2;

        int longest = Math.Abs(width);
        int shortest = Math.Abs(height);

        if (longest <= shortest)
        {
            longest = Math.Abs(height);
            shortest = Math.Abs(width);
            if (height < 0) dy2 = -1;
            else if (height > 0) dy2 = 1;
            dx2 = 0;
        }
        
        // Last valid position without collisions
        int currentX = startX;
        int currentY = startY;
        
        // Next position to check for collisions
        int nextX = startX;
        int nextY = startY;
        
        // Skip the first iteration, to not yield the starting position.
        int numerator = longest >> 1;
        numerator += shortest;
        if (numerator >= longest)
        {
            numerator -= longest;
            nextX += dx1;
            nextY += dy1;
        }
        else
        {
            nextX += dx2;
            nextY += dy2;
        }
        
        for (int i = 1; i <= longest; i++)
        {
            //bool collided = TryMoveTo(simulation, initialX, initialY, lastValidX, lastValidY, worldRelativeX, worldRelativeY, i == 1, i == longest, 0);
            
            Element neighbor = simulation.GetElementAt(nextX, nextY);

            bool stop = false;

            switch (neighbor.GetInteractionType())
            {
                case InteractionType.Gas:
                {
                    simulation.SwapElementsAt(currentX, currentY, nextX, nextY, true, true);
                    break;
                }
            
                case InteractionType.Liquid:
                case InteractionType.Solid:
                {
                    stop = true;
                    break;
                }
            
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            if (stop)
            {
                Velocity.Y = Settings.GRAVITY_CELLS_PER_SECOND;
                break;
            }
            
            // Save this position as the latest valid position.
            currentX = nextX;
            currentY = nextY;
            
            // Continue iteration.
            numerator += shortest;
            if (numerator >= longest)
            {
                numerator -= longest;
                nextX += dx1;
                nextY += dy1;
            }
            else
            {
                nextX += dx2;
                nextY += dy2;
            }
        }
        
        /*applyHeatToNeighborsIfIgnited(matrix);
        modifyColor();
        spawnSparkIfIgnited(matrix);
        checkLifeSpan(matrix);
        takeEffectsDamage(matrix);
        stoppedMovingCount = didNotMove(formerLocation) ? stoppedMovingCount + 1 : 0;
        if (stoppedMovingCount > stoppedMovingThreshold) {
            stoppedMovingCount = stoppedMovingThreshold;
        }
        if (matrix.useChunks)  {
            if (isIgnited || !hasNotMovedBeyondThreshold()) {
                matrix.reportToChunkActive(this);
                matrix.reportToChunkActive((int) formerLocation.x, (int) formerLocation.y);
            }
        }*/
    }

    /// <returns>If the element was able to move to target position.</returns>
    protected bool TryMoveTo(Simulation simulation, int myInitialX, int myInitialY, int myCurrentX, int myCurrentY, int targetX, int targetY, bool isFirstIteration, bool isLastIteration, int recursionDepth)
    {
        Element neighbor = simulation.GetElementAt(targetX, targetY);

        switch (neighbor.GetInteractionType())
        {
            case InteractionType.Gas:
            {
                simulation.SwapElementsAt(myCurrentX, myCurrentY, targetX, targetY, true, true);
                return false;
            }
            
            case InteractionType.Liquid:
            case InteractionType.Solid:
            {
                return true;
            }
            
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}