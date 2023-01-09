using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.ElementDefinitions;

public class WaterElementDefinition : ElementDefinition
{
    public WaterElementDefinition(int x, int y) : base(x, y)
    {
    }

    public override ElementType Type => ElementType.Water;
    protected override Color InitialColor => Color.Aqua;
    protected override ElementForm InitialForm => ElementForm.Liquid;
}