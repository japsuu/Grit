using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.ElementDefinitions;

public class SandElement : Element
{
    protected override Color InitialColor => RandomFactory.RandomColor(new Color(189, 148, 66, 255), new Color(255, 204, 102, 255));
    public override ushort Id => 1;
    protected override InteractionType InitialInteractionType => InteractionType.Solid;

    public SandElement(int x, int y) : base(x, y)
    {
    }

    public override void Tick(Simulation simulation, int startX, int startY)
    {
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
    }
}