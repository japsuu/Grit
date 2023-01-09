using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.Movable;

public class AirElementDefinition : ElementDefinition
{
    public override ElementType Type => ElementType.Air;
    
    protected override Color InitialColor => new(0, 0, 0, 0);
    protected override ElementForm InitialForm => ElementForm.Gas;

    public AirElementDefinition(int x, int y) : base(x, y)
    {
    }
}