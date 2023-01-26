
using System.Diagnostics;
using Grit.Simulation.Elements;
using Grit.Simulation.World.Regions.Chunks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Grit.Simulation.Rendering;

public class SimulationRenderer
{
    private string objectUnderCursor;

    private readonly Simulation simulation;
    private readonly ChunkManager chunkManager;


    public SimulationRenderer(Simulation simulation, ChunkManager chunkManager)
    {
        this.simulation = simulation;
        this.chunkManager = chunkManager;
    }


    /// <summary>
    /// Should be called at the end of Update().
    /// </summary>
    public void Update()
    {
        for (int i = 0; i < chunkManager.CurrentlyLoadedChunks.Count; i++)
        {
            chunkManager.CurrentlyLoadedChunks[i].Render();
        }
    }

    
    /// <summary>
    /// Draws all elements which should be affected by the camera matrix.
    /// </summary>
    public void DrawWorld(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < chunkManager.CurrentlyLoadedChunks.Count; i++)
        {
            // Draw world
            spriteBatch.Draw(chunkManager.CurrentlyLoadedChunks[i].Canvas, chunkManager.CurrentlyLoadedChunks[i].Rectangle, Color.White);
            
            // Draw chunk borders
            if (Settings.DRAW_CHUNK_BORDERS)
            {
                Color color = chunkManager.CurrentlyLoadedChunks[i].GetDebugRenderColor();
                spriteBatch.DrawRectangle(chunkManager.CurrentlyLoadedChunks[i].Rectangle, color, 1f);
            }
            
            // Debug draw chunk lifetimes:
            //spriteBatch.DrawString(Grit.DebugFont, chunkManager.CurrentlyLoadedChunks[i].DebugLifetime.ToString(), chunkManager.CurrentlyLoadedChunks[i].Rectangle.Center.ToVector2(), Color.Blue);
            
            // Draw dirty rects
            if (Settings.DRAW_DIRTY_RECTS)
            {
                RectangleF dirtyRect = chunkManager.CurrentlyLoadedChunks[i].GetDebugDirtyRect();
                if (!dirtyRect.IsEmpty)
                    spriteBatch.DrawRectangle(dirtyRect, Color.Red, 1f);
            }
        }
        
        spriteBatch.DrawCircle(Globals.PlayerPosition, 2, 8, Color.Lime);
    }


    /// <summary>
    /// Draws all elements which should NOT be affected by the camera matrix.
    /// </summary>
    public void DrawUi(SpriteBatch spriteBatch)
    {
        if (Settings.DRAW_CURSOR_POS)
        {
            string cPos = InputManager.MousePixelWorldPosition.ToString();
            spriteBatch.DrawRectangle(InputManager.Mouse.Position.ToVector2() + new Vector2(15, 5), new Size2(100, 1), Color.Blue, 8f);
            spriteBatch.DrawString(Grit.DebugFont, cPos, InputManager.Mouse.Position.ToVector2() + new Vector2(20, 0), Color.Red);
        }

        if (Settings.DRAW_HOVERED_ELEMENT)
        {
            Element element = simulation.GetElementAt(InputManager.MousePixelWorldPosition.X, InputManager.MousePixelWorldPosition.Y);
            objectUnderCursor = element != null ?
                element.ToString() :
                "none";
            
            // Draw top bar (object under the cursor)
            spriteBatch.DrawRectangle(new Vector2(0, 0), new Size2(Settings.WINDOW_WIDTH, 1), Color.Blue, 20f);
            spriteBatch.DrawString(Grit.DebugFont, objectUnderCursor, new Vector2(5, 5), Color.Red);
        }
    }
}