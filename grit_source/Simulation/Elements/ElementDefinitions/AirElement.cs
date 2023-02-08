using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.ElementDefinitions;

public class AirElement : Element
{
    protected override Color InitialColor => new(0, 0, 0, 0);
    public override ushort Id => 0;
    protected override InteractionType InitialInteractionType => InteractionType.Gas;

    public AirElement(int x, int y) : base(x, y)
    {
    }
}