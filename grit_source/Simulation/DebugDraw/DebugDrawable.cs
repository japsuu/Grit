using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Grit.Simulation.DebugDraw;

public abstract class DebugDrawable
{
    public bool ShouldDraw => lifetime > 0;

    protected readonly Vector2 Position;
    protected readonly Color Color;
        
    private float lifetime;

    protected DebugDrawable(int worldX, int worldY, float lifetime, Color color)
    {
        this.lifetime = lifetime;
        Color = color;
        Position = new Vector2(worldX, worldY);
    }

    public void Update(SpriteBatch spriteBatch)
    {
        PerformDraw(spriteBatch);
            
        lifetime -= Globals.FrameLengthSeconds;
    }

    protected abstract void PerformDraw(SpriteBatch spriteBatch);
}