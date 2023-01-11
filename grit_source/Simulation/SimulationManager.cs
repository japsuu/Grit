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
    private static RectangleF[] dirtyRects;

    // Debug stuff
    private static bool isSnowingToggled;
    private static Vector2 cursorScreenPos;
    private static Vector2 cursorWorldPos;
    private static string objectUnderCursor;

    public static void Initialize(int width, int height, Rectangle screenRect, GraphicsDevice graphics)
    {
        RandomFactory.Initialize(123456789);
        
        screenBounds = screenRect;
        canvas = new Texture2D(graphics, width, height);

        WorldMatrix.Initialize();
    }

    public static void Update(GameTime time)
    {
        HandleInput();

        WorldMatrix.StepDirtyChunks(time.GetElapsedSeconds());

        if(Settings.RANDOM_TICKS_ENABLED)
            WorldMatrix.StepRandomTicks(time.GetElapsedSeconds());
        
        if (Settings.DRAW_DIRTY_RECTS && Settings.USE_CHUNKS)
            dirtyRects = WorldMatrix.GatherDirtyRects();
        
        Render();
    }
    
    private static void HandleInput()
    {
        MouseState mState = Mouse.GetState();
        KeyboardStateExtended kState = KeyboardExtended.GetState();
        
        Point mouseWorldPos = Game1.MainCamera.ScreenToWorld(mState.Position.X, mState.Position.Y).ToPoint();
        
        if(mState.LeftButton == ButtonState.Pressed && screenBounds.Contains(mState.Position))
            WorldMatrix.SetElementAt(mouseWorldPos.X, mouseWorldPos.Y, new SandElementDefinition(mouseWorldPos.X, mouseWorldPos.Y));
            
        if(mState.RightButton == ButtonState.Pressed && screenBounds.Contains(mState.Position))
            WorldMatrix.SetElementAt(mouseWorldPos.X, mouseWorldPos.Y, new StoneElementDefinition(mouseWorldPos.X, mouseWorldPos.Y));
            
        if(mState.MiddleButton == ButtonState.Pressed && screenBounds.Contains(mState.Position))
            WorldMatrix.SetElementAt(mouseWorldPos.X, mouseWorldPos.Y, new WaterElementDefinition(mouseWorldPos.X, mouseWorldPos.Y));

        if (kState.WasKeyJustUp(Keys.X))
        {
            isSnowingToggled = !isSnowingToggled;
        }

        if (kState.WasKeyJustUp(Keys.C))
        {
            WorldMatrix.SetEveryChunkClean();
        }

        if (kState.WasKeyJustUp(Keys.V))
        {
            WorldMatrix.SetEveryChunkDirty();
        }

        if (kState.WasKeyJustUp(Keys.Multiply))
        {
            Game1.Instance.ChangeTargetFps(5);
        }

        if (kState.WasKeyJustUp(Keys.Divide))
        {
            Game1.Instance.ChangeTargetFps(-5);
        }

        if (isSnowingToggled)
        {
            for (int i = 0; i < Settings.SNOW_PER_SECOND; i++)
            {
                int x = RandomFactory.SeedlessRandom.Next(Settings.WORLD_WIDTH - 1);
                int y = RandomFactory.SeedlessRandom.Next(Settings.WORLD_HEIGHT - 1);
                
                WorldMatrix.SetElementAt(x, y, new WaterElementDefinition(x, y));
            }
        }

        if (Settings.DRAW_CURSOR_POS)
        {
            cursorScreenPos = mState.Position.ToVector2();
            cursorWorldPos = mouseWorldPos.ToVector2();
            if (screenBounds.Contains(cursorWorldPos))
            {
                objectUnderCursor = WorldMatrix.GetElementAt(mouseWorldPos.X + mouseWorldPos.Y * Settings.WORLD_WIDTH).ToString();
            }
            else
            {
                objectUnderCursor = "none";
            }
        }
    }

    private static void Render()
    {
        canvas.SetData(WorldMatrix.PixelsToDraw, 0, Settings.WORLD_WIDTH * Settings.WORLD_HEIGHT);
    }

    public static void DrawWorld(SpriteBatch spriteBatch)
    {
        if (Settings.USE_CHUNKS && Settings.DRAW_CHUNK_BORDERS)
        {
            for (int x = 0; x < Settings.CHUNK_COUNT_X; x++)
            {
                for (int y = 0; y < Settings.CHUNK_COUNT_Y; y++)
                {
                    // Draw chunk borders
                    Color color = WorldMatrix.IsChunkCurrentlyDirty(x, y) ? Color.Orange : Color.DarkGray;
                    spriteBatch.DrawRectangle(x * Settings.CHUNK_SIZE, y * Settings.CHUNK_SIZE, Settings.CHUNK_SIZE, Settings.CHUNK_SIZE, color, 0.5f);
                }
            }
        }
        
        spriteBatch.Draw(canvas, new Rectangle(0, 0, Settings.WORLD_WIDTH, Settings.WORLD_HEIGHT), Color.White);

        if (Settings.USE_CHUNKS && Settings.DRAW_DIRTY_RECTS)
        {
            foreach (RectangleF dirtyRect in dirtyRects)
            {
                // Draw dirty rects
                spriteBatch.DrawRectangle(dirtyRect, Color.Red, 1f);
            }
        }
    }

    public static void DrawUi(SpriteBatch spriteBatch)
    {
        if (Settings.DRAW_CURSOR_POS)
        {
            // Draw mouse pos
            string cPos = cursorWorldPos.ToString();
            spriteBatch.DrawRectangle(cursorScreenPos + new Vector2(15, 5), new Size2(100, 1), Color.Blue, 8f);
            spriteBatch.DrawString(Game1.DebugFont, cPos, cursorScreenPos + new Vector2(20, 0), Color.Red);
            
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