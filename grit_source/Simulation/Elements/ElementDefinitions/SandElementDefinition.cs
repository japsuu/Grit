using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.Movable;

public class SandElementDefinition : ElementDefinition
{
    public override ElementType Type => ElementType.Sand;

    protected override Color InitialColor => RandomFactory.RandomColor(new Color(189, 148, 66, 255), new Color(255, 204, 102, 255));
    protected override ElementForm InitialForm => ElementForm.Solid;

    public SandElementDefinition(int x, int y) : base(x, y)
    {
    }
}