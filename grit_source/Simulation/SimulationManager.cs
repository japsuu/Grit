using System;
using System.Text;
using Grit.Simulation.Elements.ElementDefinitions;
using Grit.Simulation.Elements.Movable;
using Grit.Simulation.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;

namespace Grit.Simulation;

public static class SimulationManager
{
    private static Rectangle screenBounds;
    private static Texture2D canvas;

    // Debug stuff
    private static bool isSnowingToggled;
    private static Vector2 cursorPos;
    private static string objectUnderCursor;

    public static void Initialize(int width, int height, Rectangle screenRect, GraphicsDevice graphics)
    {
        RandomFactory.Initialize(123456789);
        
        screenBounds = screenRect;
        canvas = new Texture2D(graphics, width, height);

        WorldMatrix.Initialize(width, height);
    }

    public static void Update(GameTime time)
    {
        HandleInput();

        WorldMatrix.StepAll(time.GetElapsedSeconds());
        
        Render();
    }
    
    private static void HandleInput()
    {
        MouseState mState = Mouse.GetState();
        KeyboardStateExtended kState = KeyboardExtended.GetState();
            
        Point mousePos = mState.Position;
        if(mState.LeftButton == ButtonState.Pressed && screenBounds.Contains(mousePos))
            WorldMatrix.SetElementAt(mousePos.X, mousePos.Y, new SandElementDefinition(mousePos.X, mousePos.Y));
            
        if(mState.RightButton == ButtonState.Pressed && screenBounds.Contains(mousePos))
            WorldMatrix.SetElementAt(mousePos.X, mousePos.Y, new StoneElementDefinition(mousePos.X, mousePos.Y));
            
        if(mState.MiddleButton == ButtonState.Pressed && screenBounds.Contains(mousePos))
            WorldMatrix.SetElementAt(mousePos.X, mousePos.Y, new WaterElementDefinition(mousePos.X, mousePos.Y));

        if (kState.WasKeyJustDown(Keys.X))
        {
            isSnowingToggled = !isSnowingToggled;
        }

        if (kState.IsKeyDown(Keys.D))
        {
            for (int x = 0; x < WorldMatrix.Chunks.GetLength(0); x++)
            {
                for (int y = 0; y < WorldMatrix.Chunks.GetLength(1); y++)
                {
                    DirtyChunk chunk = WorldMatrix.Chunks[x, y];
                    chunk.SetEverythingDirty(x * Settings.CHUNK_SIZE, y * Settings.CHUNK_SIZE);
                }
            }
        }

        if (kState.IsKeyDown(Keys.S))
        {
            for (int x = 0; x < WorldMatrix.Chunks.GetLength(0); x++)
            {
                for (int y = 0; y < WorldMatrix.Chunks.GetLength(1); y++)
                {
                    DirtyChunk chunk = WorldMatrix.Chunks[x, y];
                    chunk.SetEverythingClean();
                }
            }
        }

        if (isSnowingToggled)
        {
            for (int i = 0; i < Settings.SNOW_PER_SECOND; i++)
            {
                int x = RandomFactory.SeedlessRandom.Next(WorldMatrix.WorldWidth - 1);
                int y = RandomFactory.SeedlessRandom.Next(WorldMatrix.WorldHeight - 1);
                
                WorldMatrix.SetElementAt(x, y, new StoneElementDefinition(x, y));
            }
        }

        if (Settings.DRAW_CURSOR_POS)
        {
            if (screenBounds.Contains(mousePos))
            {
                cursorPos = mousePos.ToVector2();
                objectUnderCursor = WorldMatrix.Matrix[mousePos.X + mousePos.Y * Settings.WORLD_WIDTH].ToString();
            }
            else
            {
                objectUnderCursor = "none";
            }
        }
    }

    private static void Render()
    {
        canvas.SetData(WorldMatrix.PixelsToDraw, 0, WorldMatrix.WorldWidth * WorldMatrix.WorldHeight);
    }

    public static void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(canvas, new Rectangle(0, 0, WorldMatrix.WorldWidth, WorldMatrix.WorldHeight), Color.White);

        if (Settings.USE_CHUNKS && Settings.DRAW_CHUNK_BORDERS)
        {
            for (int x = 0; x < WorldMatrix.Chunks.GetLength(0); x++)
            {
                for (int y = 0; y < WorldMatrix.Chunks.GetLength(1); y++)
                {
                    // Draw chunk borders
                    Color color = WorldMatrix.Chunks[x, y].LastFrameWasDirty ? Color.Orange : Color.White;
                    spriteBatch.DrawRectangle(x * Settings.CHUNK_SIZE, y * Settings.CHUNK_SIZE, Settings.CHUNK_SIZE, Settings.CHUNK_SIZE, color, 0.5f);
                }
            }
        }

        if (Settings.USE_CHUNKS && Settings.DRAW_DIRTY_RECTS)
        {
            foreach (RectangleF dirtyRect in WorldMatrix.DirtyRects)
            {
                // Draw dirty rects
                spriteBatch.DrawRectangle(dirtyRect, Color.Red, 1f);
            }
        }

        if (Settings.DRAW_CURSOR_POS)
        {
            // Draw mouse pos
            string cPos = cursorPos.ToString();
            spriteBatch.DrawRectangle(cursorPos + new Vector2(15, 5), new Size2(100, 1), Color.Blue, 8f);
            spriteBatch.DrawString(Game1.DebugFont, cPos, cursorPos + new Vector2(20, 0), Color.Red);
            
            // Draw object under the cursor
            spriteBatch.DrawRectangle(new Vector2(0, 0), new Size2(Settings.WORLD_WIDTH, 1), Color.Blue, 20f);
            spriteBatch.DrawString(Game1.DebugFont, objectUnderCursor, new Vector2(5, 5), Color.Red);
        }
    }

    public static void Dispose()
    {
        canvas.Dispose();
    }
}