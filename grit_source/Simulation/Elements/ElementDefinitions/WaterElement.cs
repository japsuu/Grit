using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.ElementDefinitions;

public class WaterElement : Element
{
    public WaterElement(int x, int y) : base(x, y)
    {
    }

    protected override Color InitialColor => Color.Aqua;
    public override ushort Id => 3;
    protected override ElementForm InitialForm => ElementForm.Liquid;

    public override (int newX, int newY) Tick(Simulation simulation, int worldRelativeX, int worldRelativeY)
    {
        int newX = worldRelativeX;
        int newY = worldRelativeY;
        int belowY = worldRelativeY + 1;
        int leftX = worldRelativeX - 1;
        int rightX = worldRelativeX + 1;
        
        // If at the bottom of the world, replace cell with air.
        //WARN: 500 is for debug.
        if (belowY >= 500)
        {
            simulation.SetElementAt(worldRelativeX, worldRelativeY, new AirElement(worldRelativeX, worldRelativeY));
            return (newX, newY);
        }

        // Below cell.
        if (simulation.GetElementAt(worldRelativeX, belowY).GetForm() == ElementForm.Gas)
        {
            simulation.SwapElementsAt(worldRelativeX, worldRelativeY, worldRelativeX, belowY);
            newY = belowY;
            return (newX, newY);
        }

        // Randomly choose whether to prioritize left or right update
        bool prioritizeLeft = RandomFactory.RandomBool();
        if (prioritizeLeft)
        {
            // Left bottom cell.
            if (simulation.GetElementAt(leftX, belowY).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(worldRelativeX, worldRelativeY, leftX, belowY);
                newX = leftX;
                newY = belowY;
                return (newX, newY);
            }
                    
            // Right bottom cell.
            if (simulation.GetElementAt(rightX, belowY).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(worldRelativeX, worldRelativeY, rightX, belowY);
                newX = rightX;
                newY = belowY;
                return (newX, newY);
            }
        }
        else
        {
            // Right bottom cell.
            if (simulation.GetElementAt(rightX, belowY).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(worldRelativeX, worldRelativeY, rightX, belowY);
                newX = rightX;
                newY = belowY;
                return (newX, newY);
            }
                    
            // Left bottom cell.
            if (simulation.GetElementAt(leftX, belowY).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(worldRelativeX, worldRelativeY, leftX, belowY);
                newX = leftX;
                newY = belowY;
                return (newX, newY);
            }
        }

        if (prioritizeLeft)
        {
            // Left cell.
            if (simulation.GetElementAt(leftX, worldRelativeY).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(worldRelativeX, worldRelativeY, leftX, worldRelativeY);
                newX = leftX;
                return (newX, newY);
            }
                    
            // Right cell.
            if (simulation.GetElementAt(rightX, worldRelativeY).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(worldRelativeX, worldRelativeY, rightX, worldRelativeY);
                newX = rightX;
                return (newX, newY);
            }
        }
        else
        {
            // Right cell.
            if (simulation.GetElementAt(rightX, worldRelativeY).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(worldRelativeX, worldRelativeY, rightX, worldRelativeY);
                newX = rightX;
                return (newX, newY);
            }
                    
            // Left cell.
            if (simulation.GetElementAt(leftX, worldRelativeY).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(worldRelativeX, worldRelativeY, leftX, worldRelativeY);
                newX = leftX;
                return (newX, newY);
            }
        }
        
        return (newX, newY);
    }
}