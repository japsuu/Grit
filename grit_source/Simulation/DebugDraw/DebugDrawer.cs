using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Grit.Simulation.DebugDraw;

public static class DebugDrawer
{
    private static readonly List<DebugDrawable> Drawables = new();
    
    /// <summary>
    /// Called internally.
    /// </summary>
    public static void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < Drawables.Count; i++)
        {
            DebugDrawable entry = Drawables[i];
            entry.Update(spriteBatch);

            if (!entry.ShouldDraw)
            {
                Drawables.Remove(entry);
            }
        }
    }

    public static void AddDrawable(DebugDrawable drawable)
    {
        Drawables.Add(drawable);
    }
}