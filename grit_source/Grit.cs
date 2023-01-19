using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Grit.Simulation;
using Grit.UI;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace Grit;

public class Grit : Game
{
    public static Grit Instance { get; private set; }
    public static SpriteFont DebugFont { get; private set; }
    public static OrthographicCamera MainCamera { get; private set; }
    public static Rectangle ScreenBounds { get; private set; }

    private readonly StringBuilder stringBuilder;
    private readonly Process process;
    private readonly FrameCounter frameCounter;
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private float previousScrollValue;
    private int currentTargetFps;
    private SimulationManager simulationManager;
    
    // Fixed update loop fields.
    private float accumulator;
    private float previousTime;
    private float fixedUpdateDeltaTime;
    private readonly float targetFixedUpdateDeltaTime;
    private readonly float fixedUpdateMinDeltaTime;
    private readonly float maximumFrameTime;

    public Grit()
    {
        Instance = this;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        process = Process.GetCurrentProcess();
        graphics = new GraphicsDeviceManager(this);
        frameCounter = new FrameCounter();
        stringBuilder = new StringBuilder(64);
        
        graphics.PreferredBackBufferWidth = Settings.WORLD_WIDTH;
        graphics.PreferredBackBufferHeight = Settings.WORLD_HEIGHT;

        graphics.SynchronizeWithVerticalRetrace = false;

        if (Settings.UNLOCK_FRAMERATE)
        {
            InactiveSleepTime = TimeSpan.Zero;
            IsFixedTimeStep = false;
        }
        else
        {
            SetTargetFps(60);
        }
        
        targetFixedUpdateDeltaTime = 1000f / Settings.FIXED_UPDATE_TARGET_TPS;
        fixedUpdateMinDeltaTime = 1000f / Settings.UPDATE_MINIMUM_FPS;
        maximumFrameTime = 1000f / Settings.MAXIMUM_FIXED_UPDATES_PER_FRAME;
    }

    #region GAME LOGIC

    protected override void Initialize()
    {
        base.Initialize();

        ScreenBounds = GraphicsDevice.PresentationParameters.Bounds;

        ViewportAdapter wpa = new BoxingViewportAdapter(Window, GraphicsDevice, Settings.WORLD_WIDTH, Settings.WORLD_HEIGHT);
        
        MainCamera = new OrthographicCamera(wpa)
        {
            MinimumZoom = 1,
            MaximumZoom = 40
        };

        RandomFactory.Initialize(123456789);

        simulationManager = new SimulationManager(GraphicsDevice);
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        DebugFont = Content.Load<SpriteFont>("DebugFont");
    }

    protected override void UnloadContent()
    {
        simulationManager.Dispose();
        graphics.Dispose();
        spriteBatch.Dispose();
    }

    // Contains a fixed timestep implementation.
    // Explanation: https://gafferongames.com/post/fix_your_timestep/.
    protected override void Update(GameTime gameTime)
    {
        CalculateFps(gameTime);
        Debug.WriteLine($"Dt:{Globals.DeltaTime}, FDt:{Globals.FixedUpdateDeltaTime}, FAt:{Globals.FixedUpdateAlphaTime}");
        if (Settings.SYNCHRONIZE_FIXED_UPDATE_WITH_UPDATE)
        {
            Globals.FrameLengthMilliseconds = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Globals.DeltaTime = gameTime.GetElapsedSeconds();
            Globals.Time = gameTime;
            Globals.FixedUpdateDeltaTime = Globals.DeltaTime;
            Globals.FixedUpdateAlphaTime = 0;
            
            FixedUpdate();
        }
        else
        {
            // Skip update altogether, if we are running at low enough FPS.
            // This is done to allow the FPS to recover.
            if (gameTime.ElapsedGameTime.TotalSeconds > fixedUpdateMinDeltaTime)
            {
                accumulator = 0;
                previousTime = 0;
                return;
            }
            
            if (previousTime == 0)
            {
                fixedUpdateDeltaTime = targetFixedUpdateDeltaTime;
                previousTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            }
            
            Globals.FrameLengthMilliseconds = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Globals.DeltaTime = gameTime.GetElapsedSeconds();
            Globals.Time = gameTime;
            Globals.FixedUpdateDeltaTime = fixedUpdateDeltaTime;
            
            float currentTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            float frameTime = currentTime - previousTime;
            if (frameTime > maximumFrameTime)
            {
                frameTime = maximumFrameTime;
            }
            previousTime = currentTime;

            accumulator += frameTime;

            while (accumulator >= fixedUpdateDeltaTime)
            {
                FixedUpdate();
                
                accumulator -= fixedUpdateDeltaTime;
            }
            
            Globals.FixedUpdateAlphaTime = accumulator / fixedUpdateDeltaTime;
        }

        InputManager.Update();
        
        simulationManager.Update();
        
        UpdateCamera(gameTime);

        base.Update(gameTime);
    }

    private void FixedUpdate()
    {
        simulationManager.FixedUpdate();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // Blocks the draw call and game loop until textures transferred to GPU.
        GraphicsDevice.Textures[0] = null;

        simulationManager.Draw(spriteBatch, MainCamera.GetViewMatrix());

        base.Draw(gameTime);
    }

    #endregion

    #region PUBLIC METHODS

    /// <summary>
    /// Modifies the target FPS by the given amount.
    /// </summary>
    public void ChangeTargetFps(int amount)
    {
        if (Settings.UNLOCK_FRAMERATE) return;
        int targetFps = currentTargetFps + amount;
        if (targetFps is < 1 or > 5000) return;

        MaxElapsedTime = TimeSpan.FromMilliseconds(targetFps * 2000);
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / targetFps);
        currentTargetFps = targetFps;
    }

    #endregion

    #region PRIVATE METHODS

    private void SetTargetFps(int targetFps)
    {
        if (Settings.UNLOCK_FRAMERATE) return;
        MaxElapsedTime = TimeSpan.FromMilliseconds(targetFps * 2000);
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / targetFps);
        currentTargetFps = targetFps;
    }

    private void CalculateFps(GameTime gameTime)
    {
        frameCounter.Update(gameTime.ElapsedGameTime.TotalSeconds);
        long usedMemory = process.PrivateMemorySize64 / 1048576;

        stringBuilder.Clear().Append($"{frameCounter.CurrentFramesPerSecond:F1} FPS @ {usedMemory} MB");

        Window.Title = stringBuilder.ToString();
    }

    private void UpdateCamera(GameTime gameTime)
    {
        MainCamera.Move(GetCameraMovementDirection() * Settings.CAMERA_MOVEMENT_SPEED * gameTime.GetElapsedSeconds());

        // Logarithmic zoom gang.
        if (previousScrollValue < Mouse.GetState().ScrollWheelValue)
            MainCamera.ZoomIn(MainCamera.Zoom * 10f * gameTime.GetElapsedSeconds());
        else if (previousScrollValue > Mouse.GetState().ScrollWheelValue)
            MainCamera.ZoomOut(MainCamera.Zoom * 10f * gameTime.GetElapsedSeconds());

        previousScrollValue = Mouse.GetState().ScrollWheelValue;
    }

    private static Vector2 GetCameraMovementDirection()
    {
        Vector2 movementDirection = Vector2.Zero;
        KeyboardState state = Keyboard.GetState();

        if (state.IsKeyDown(Keys.Down) || state.IsKeyDown(Keys.S))
            movementDirection += Vector2.UnitY;

        if (state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.W))
            movementDirection -= Vector2.UnitY;

        if (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.A))
            movementDirection -= Vector2.UnitX;

        if (state.IsKeyDown(Keys.Right) || state.IsKeyDown(Keys.D))
            movementDirection += Vector2.UnitX;

        return movementDirection;
    }

    #endregion
}