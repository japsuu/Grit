using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Grit.Simulation.DebugDraw;

public class StringDrawable : DebugDrawable
{
    private readonly string text;
    private readonly float scale;

    public StringDrawable(int worldX, int worldY, float lifetime, string text, float scale, Color color) : base(worldX, worldY, lifetime, color)
    {
        this.scale = scale;
        this.text = text;
    }
        
    protected override void PerformDraw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(Grit.DebugFont, text, Position, Color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}