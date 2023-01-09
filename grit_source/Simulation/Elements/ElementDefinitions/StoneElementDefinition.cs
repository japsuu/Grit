using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.ElementDefinitions;

public class StoneElementDefinition : ElementDefinition
{
    public override ElementType Type => ElementType.Stone;

    protected override Color InitialColor => 
        RandomFactory.RandomColorFnl(
            new Color(90, 90, 90, 255), 
            new Color(180, 180, 180, 255), 
            StartX, 
            StartY);

    protected override ElementForm InitialForm => ElementForm.Solid;

    public StoneElementDefinition(int x, int y) : base(x, y)
    {
    }
}