using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.ElementDefinitions;

public class StoneElement : Element
{
    protected override Color InitialColor => 
        RandomFactory.RandomColorFnl(
            new Color(90, 90, 90, 255), 
            new Color(180, 180, 180, 255), 
            StartX, 
            StartY);

    public override ushort Id => 2;
    protected override InteractionType InitialInteractionType => InteractionType.Solid;

    public StoneElement(int x, int y) : base(x, y)
    {
    }
}