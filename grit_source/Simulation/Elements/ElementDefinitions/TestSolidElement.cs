using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.ElementDefinitions;

public class TestSolidElement : Element
{
    public TestSolidElement(int x, int y) : base(x, y)
    {
    }

    public override ushort Id => 99;
    protected override InteractionType InitialInteractionType => InteractionType.Solid;
    protected override Color InitialColor => Color.Brown;


    public override void Tick(Simulation simulation, int startX, int startY)
    {
        
    }
}