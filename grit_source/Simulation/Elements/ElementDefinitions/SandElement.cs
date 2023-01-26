using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.ElementDefinitions;

public class SandElement : Element
{
    protected override Color InitialColor => RandomFactory.RandomColor(new Color(189, 148, 66, 255), new Color(255, 204, 102, 255));
    public override ushort Id => 1;
    protected override ElementForm InitialForm => ElementForm.Solid;

    public SandElement(int x, int y) : base(x, y)
    {
    }

    public override (int newX, int newY) Tick(Simulation simulation, int worldRelativeX, int worldRelativeY)
    {
        int newX = worldRelativeX;
        int newY = worldRelativeY;
        int belowY = worldRelativeY + 1;

        // If at the bottom of the world, replace cell with air.
        //WARN: 500 is for debug.
        if (belowY >= 500)
        {
            simulation.SetElementAt(worldRelativeX, worldRelativeY, new AirElement(worldRelativeX, worldRelativeY));
            return (newX, newY);
        }

        // Below cell.
        if (simulation.GetElementAt(worldRelativeX, belowY).GetForm() != ElementForm.Solid)
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
            int leftX = worldRelativeX - 1;
            if (simulation.GetElementAt(leftX, belowY).GetForm() != ElementForm.Solid)
            {
                simulation.SwapElementsAt(worldRelativeX, worldRelativeY, leftX, belowY);
                newX = leftX;
                newY = belowY;
                return (newX, newY);
            }

            // Right bottom cell.
            int rightX = worldRelativeX + 1;
            if (simulation.GetElementAt(rightX, belowY).GetForm() != ElementForm.Solid)
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
            int rightX = worldRelativeX + 1;
            if (simulation.GetElementAt(rightX, belowY).GetForm() != ElementForm.Solid)
            {
                simulation.SwapElementsAt(worldRelativeX, worldRelativeY, rightX, belowY);
                newX = rightX;
                newY = belowY;
                return (newX, newY);
            }

            // Left bottom cell.
            int leftX = worldRelativeX - 1;
            if (simulation.GetElementAt(leftX, belowY).GetForm() != ElementForm.Solid)
            {
                simulation.SwapElementsAt(worldRelativeX, worldRelativeY, leftX, belowY);
                newX = leftX;
                newY = belowY;
                return (newX, newY);
            }
        }

        return (newX, newY);
    }
}