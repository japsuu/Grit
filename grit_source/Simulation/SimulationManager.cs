
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
    #region PRIVATE FIELDS

    private static Rectangle screenBounds;
    private static Texture2D canvas;
    private static RectangleF[] dirtyRects;

    #endregion

    
    #region PRIVATE DEBUG FIELDS

    private static bool isSnowingToggled;
    private static Vector2 cursorScreenPos;
    private static Vector2 cursorWorldPos;
    private static string objectUnderCursor;

    #endregion

    
    #region PUBLIC METHODS

    public static void Initialize(int width, int height, Rectangle screenRect, GraphicsDevice graphics)
    {
        screenBounds = screenRect;
        canvas = new Texture2D(graphics, width, height);
        
        RandomFactory.Initialize(123456789);

        WorldMatrix.Initialize();
    }

    public static void UpdateSimulation(GameTime time)
    {
        HandleInput();

        if (isSnowingToggled)
            SpawnDebugSnow();

        WorldMatrix.StepDirtyChunks(time.GetElapsedSeconds());

        if(Settings.RANDOM_TICKS_ENABLED)
            WorldMatrix.StepRandomTicks(time.GetElapsedSeconds());
        
        if (Settings.DRAW_DIRTY_RECTS && Settings.USE_WORLD_CHUNKING)
            dirtyRects = WorldMatrix.GetDebugDirtyRects();
        
        Render();
    }
    
    /// <summary>
    /// Draws all elements which should be affected by the camera matrix.
    /// </summary>
    public static void DrawWorld(SpriteBatch spriteBatch)
    {
        // Draw chunk borders
        if (Settings.USE_WORLD_CHUNKING && Settings.DRAW_CHUNK_BORDERS)
        {
            for (int x = 0; x < Settings.CHUNK_COUNT_X; x++)
            {
                for (int y = 0; y < Settings.CHUNK_COUNT_Y; y++)
                {
                    Color color = WorldMatrix.IsChunkCurrentlyDirty(x, y) ? Color.Orange : Color.DarkGray;
                    spriteBatch.DrawRectangle(x * Settings.WORLD_CHUNK_SIZE, y * Settings.WORLD_CHUNK_SIZE, Settings.WORLD_CHUNK_SIZE, Settings.WORLD_CHUNK_SIZE, color, 0.5f);
                }
            }
        }
        
        // Draw world
        spriteBatch.Draw(canvas, new Rectangle(0, 0, Settings.WORLD_WIDTH, Settings.WORLD_HEIGHT), Color.White);

        // Draw dirty rects
        if (Settings.USE_WORLD_CHUNKING && Settings.DRAW_DIRTY_RECTS)
        {
            foreach (RectangleF dirtyRect in dirtyRects)
            {
                spriteBatch.DrawRectangle(dirtyRect, Color.Red, 1f);
            }
        }
    }

    /// <summary>
    /// Draws all elements which should NOT be affected by the camera matrix.
    /// </summary>
    public static void DrawUi(SpriteBatch spriteBatch)
    {
        if (Settings.DRAW_CURSOR_POS)
        {
            // Draw mouse pos
            string cPos = cursorWorldPos.ToString();
            spriteBatch.DrawRectangle(cursorScreenPos + new Vector2(15, 5), new Size2(100, 1), Color.Blue, 8f);
            spriteBatch.DrawString(Game1.DebugFont, cPos, cursorScreenPos + new Vector2(20, 0), Color.Red);
            
            // Draw top bar (object under the cursor)
            spriteBatch.DrawRectangle(new Vector2(0, 0), new Size2(Settings.WORLD_WIDTH, 1), Color.Blue, 20f);
            spriteBatch.DrawString(Game1.DebugFont, objectUnderCursor, new Vector2(5, 5), Color.Red);
        }
    }

    public static void Dispose()
    {
        canvas.Dispose();
    }

    #endregion

    
    #region PRIVATE METHODS
    
    private static bool IsPositionInsideWorld(Point p) => p.X >= 0 && p.Y >= 0 && p.X < Settings.WORLD_WIDTH && p.Y < Settings.WORLD_HEIGHT;

    private static void SpawnDebugSnow()
    {
        // Spawn elements in random positions across the world.
        for (int i = 0; i < Settings.DEBUG_SNOW_PER_SECOND; i++)
        {
            int x = RandomFactory.SeedlessRandom.Next(Settings.WORLD_WIDTH - 1);
            int y = RandomFactory.SeedlessRandom.Next(Settings.WORLD_HEIGHT - 1);
                
            WorldMatrix.SetElementAt(x, y, new WaterElementDefinition(x, y));
        }
    }

    private static void HandleInput()
    {
        MouseState mState = Mouse.GetState();
        KeyboardStateExtended kState = KeyboardExtended.GetState();
        Point mouseWorldPos = Game1.MainCamera.ScreenToWorld(mState.Position.X, mState.Position.Y).ToPoint();
        
        // Creating Elements with mouse input
        if(mState.LeftButton == ButtonState.Pressed && IsPositionInsideWorld(mouseWorldPos))
            WorldMatrix.SetElementAt(mouseWorldPos.X, mouseWorldPos.Y, new SandElementDefinition(mouseWorldPos.X, mouseWorldPos.Y));
            
        if(mState.RightButton == ButtonState.Pressed && IsPositionInsideWorld(mouseWorldPos))
            WorldMatrix.SetElementAt(mouseWorldPos.X, mouseWorldPos.Y, new StoneElementDefinition(mouseWorldPos.X, mouseWorldPos.Y));
            
        if(mState.MiddleButton == ButtonState.Pressed && IsPositionInsideWorld(mouseWorldPos))
            WorldMatrix.SetElementAt(mouseWorldPos.X, mouseWorldPos.Y, new WaterElementDefinition(mouseWorldPos.X, mouseWorldPos.Y));

        
        // Keyboard input
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

    #endregion
}