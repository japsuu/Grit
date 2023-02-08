using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.ElementDefinitions;

public class WaterElement : Element
{
    public WaterElement(int x, int y) : base(x, y)
    {
    }

    protected override Color InitialColor => Color.Aqua;
    public override ushort Id => 3;
    protected override InteractionType InitialInteractionType => InteractionType.Liquid;

    public override void Tick(Simulation simulation, int startX, int startY)
    {
        int belowY = startY + 1;
        int leftX = startX - 1;
        int rightX = startX + 1;
        
        // If at the bottom of the world, replace cell with air.
        //if (belowY >= 500)
        //{
        //    simulation.SetElementAt(worldRelativeX, worldRelativeY, new AirElement(worldRelativeX, worldRelativeY));
        //    return (newX, newY);
        //}

        // Below cell.
        if (simulation.GetElementAt(startX, belowY).GetInteractionType() == InteractionType.Gas)
        {
            simulation.SwapElementsAt(startX, startY, startX, belowY, true, true);
            return;
        }

        // Randomly choose whether to prioritize left or right update
        bool prioritizeLeft = RandomFactory.RandomBool();
        if (prioritizeLeft)
        {
            // Left bottom cell.
            if (simulation.GetElementAt(leftX, belowY).GetInteractionType() == InteractionType.Gas)
            {
                simulation.SwapElementsAt(startX, startY, leftX, belowY, true, true);
                return;
            }
                    
            // Right bottom cell.
            if (simulation.GetElementAt(rightX, belowY).GetInteractionType() == InteractionType.Gas)
            {
                simulation.SwapElementsAt(startX, startY, rightX, belowY, true, true);
                return;
            }
        }
        else
        {
            // Right bottom cell.
            if (simulation.GetElementAt(rightX, belowY).GetInteractionType() == InteractionType.Gas)
            {
                simulation.SwapElementsAt(startX, startY, rightX, belowY, true, true);
                return;
            }
                    
            // Left bottom cell.
            if (simulation.GetElementAt(leftX, belowY).GetInteractionType() == InteractionType.Gas)
            {
                simulation.SwapElementsAt(startX, startY, leftX, belowY, true, true);
                return;
            }
        }

        if (prioritizeLeft)
        {
            // Left cell.
            if (simulation.GetElementAt(leftX, startY).GetInteractionType() == InteractionType.Gas)
            {
                simulation.SwapElementsAt(startX, startY, leftX, startY, true, true);
                return;
            }
                    
            // Right cell.
            if (simulation.GetElementAt(rightX, startY).GetInteractionType() == InteractionType.Gas)
            {
                simulation.SwapElementsAt(startX, startY, rightX, startY, true, true);
                return;
            }
        }
        else
        {
            // Right cell.
            if (simulation.GetElementAt(rightX, startY).GetInteractionType() == InteractionType.Gas)
            {
                simulation.SwapElementsAt(startX, startY, rightX, startY, true, true);
                return;
            }
                    
            // Left cell.
            if (simulation.GetElementAt(leftX, startY).GetInteractionType() == InteractionType.Gas)
            {
                simulation.SwapElementsAt(startX, startY, leftX, startY, true, true);
                return;
            }
        }
    }
}