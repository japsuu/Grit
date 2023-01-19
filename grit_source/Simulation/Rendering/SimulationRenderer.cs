
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Grit.Simulation.Rendering;

public class SimulationRenderer
{
    private readonly Simulation simulation;
    private readonly Texture2D canvas;
    
    private Vector2 cursorScreenPos;
    private Vector2 cursorWorldPos;
    private string objectUnderCursor;
    
    private Color[] frameBuffer;
    private RectangleF[] dirtyRects;

    private void TransferFramebuffer(Color[] buffer)
    {
        frameBuffer = buffer;
    }

    private void TransferDirtyRects(RectangleF[] rects) => dirtyRects = rects;


    public SimulationRenderer(Simulation simulation, Texture2D canvas)
    {
        this.simulation = simulation;
        this.canvas = canvas;
        frameBuffer = new Color[Settings.WORLD_WIDTH * Settings.WORLD_HEIGHT];
        simulation.TransferFramebuffer += TransferFramebuffer;
        simulation.TransferDirtyRects += TransferDirtyRects;
    }


    /// <summary>
    /// Should be called at the end of Update().
    /// </summary>
    public void Update()
    {
        if (Settings.DRAW_CURSOR_POS)
        {
            cursorWorldPos = InputManager.MouseWorldPos;
            cursorScreenPos = InputManager.Mouse.Position.ToVector2();
        }

        if (Settings.DRAW_HOVERED_ELEMENT)
        {
            Point elementPos = cursorWorldPos.ToPoint();
            objectUnderCursor = Grit.ScreenBounds.Contains(cursorWorldPos) ?
                simulation.GetElementAt(elementPos.X + elementPos.Y * Settings.WORLD_WIDTH).ToString() :
                "none";
        }
        
        canvas.SetData(frameBuffer, 0, Settings.WORLD_WIDTH * Settings.WORLD_HEIGHT);
    }

    
    /// <summary>
    /// Draws all elements which should be affected by the camera matrix.
    /// </summary>
    public void DrawWorld(SpriteBatch spriteBatch)
    {
        // Draw chunk borders
        if (Settings.DRAW_CHUNK_BORDERS)
        {
            if (dirtyRects != null)
            {
                for (int x = 0; x < Settings.CHUNK_COUNT_X; x++)
                {
                    for (int y = 0; y < Settings.CHUNK_COUNT_Y; y++)
                    {
                        bool dirty = !dirtyRects[x + y * Settings.CHUNK_COUNT_X].IsEmpty;
                        Color color = dirty ? Color.Orange : Color.DarkGray;
                        spriteBatch.DrawRectangle(x * Settings.WORLD_CHUNK_SIZE, y * Settings.WORLD_CHUNK_SIZE, Settings.WORLD_CHUNK_SIZE, Settings.WORLD_CHUNK_SIZE, color, 0.5f);
                    }
                }
            }
        }
        
        // Draw world
        spriteBatch.Draw(canvas, new Rectangle(0, 0, Settings.WORLD_WIDTH, Settings.WORLD_HEIGHT), Color.White);

        // Draw dirty rects
        if (Settings.DRAW_DIRTY_RECTS)
        {
            if(dirtyRects != null)
            {
                foreach (RectangleF dirtyRect in dirtyRects)
                {
                    if (!dirtyRect.IsEmpty)
                        spriteBatch.DrawRectangle(dirtyRect, Color.Red, 1f);
                }
            }
        }
    }


    /// <summary>
    /// Draws all elements which should NOT be affected by the camera matrix.
    /// </summary>
    public void DrawUi(SpriteBatch spriteBatch)
    {
        if (Settings.DRAW_CURSOR_POS)
        {
            string cPos = cursorWorldPos.ToString();
            spriteBatch.DrawRectangle(cursorScreenPos + new Vector2(15, 5), new Size2(100, 1), Color.Blue, 8f);
            spriteBatch.DrawString(Grit.DebugFont, cPos, cursorScreenPos + new Vector2(20, 0), Color.Red);
        }

        if (Settings.DRAW_HOVERED_ELEMENT)
        {
            // Draw top bar (object under the cursor)
            spriteBatch.DrawRectangle(new Vector2(0, 0), new Size2(Settings.WORLD_WIDTH, 1), Color.Blue, 20f);
            spriteBatch.DrawString(Grit.DebugFont, objectUnderCursor, new Vector2(5, 5), Color.Red);
        }
    }

    
    public void Dispose()
    {
        canvas.Dispose();
    }
}