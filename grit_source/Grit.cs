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
    public static GraphicsDevice Graphics { get; private set; }

    private readonly StringBuilder stringBuilder;
    private readonly Process process;
    private readonly FrameCounter frameCounter;
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private float previousScrollValue;
    private int currentTargetFps;
    private SimulationController simulationController;
    
    // Fixed update loop fields.
    private readonly Stopwatch stopwatch;
    private float accumulator;
    private float previousFrameTotalMilliseconds;
    private float fixedUpdateFrameLengthMilliseconds;
    private readonly float fixedUpdateTargetFrameLengthMilliseconds;
    private readonly float maximumFrameTimeToAllowFixedUpdate;

    public Grit()
    {
        Instance = this;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        process = Process.GetCurrentProcess();
        graphics = new GraphicsDeviceManager(this);
        frameCounter = new FrameCounter();
        stringBuilder = new StringBuilder(64);
        
        graphics.PreferredBackBufferWidth = Settings.WINDOW_WIDTH;
        graphics.PreferredBackBufferHeight = Settings.WINDOW_HEIGHT;

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

        stopwatch = new Stopwatch();
        fixedUpdateTargetFrameLengthMilliseconds = 1000f / Settings.FIXED_UPDATE_TARGET_TPS;
        maximumFrameTimeToAllowFixedUpdate = 1000f / Settings.MAXIMUM_FIXED_UPDATES_PER_FRAME;
        stopwatch.Start();
    }

    #region GAME LOGIC

    protected override void Initialize()
    {
        base.Initialize();

        Graphics = GraphicsDevice;
        ScreenBounds = Graphics.PresentationParameters.Bounds;

        ViewportAdapter wpa = new BoxingViewportAdapter(Window, Graphics, Settings.WINDOW_WIDTH, Settings.WINDOW_HEIGHT);
        
        MainCamera = new OrthographicCamera(wpa)
        {
            MinimumZoom = 1,
            MaximumZoom = 40
        };

        RandomFactory.Initialize(123456789);

        Globals.PlayerPosition = MainCamera.Center;
        
        simulationController = new SimulationController();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        DebugFont = Content.Load<SpriteFont>("DebugFont");
    }

    protected override void UnloadContent()
    {
        simulationController.Dispose();
        graphics.Dispose();
        spriteBatch.Dispose();
    }

    // Contains a fixed timestep implementation.
    // Explanation: https://gafferongames.com/post/fix_your_timestep/.
    protected override void Update(GameTime gameTime)
    {
        CalculateFps(gameTime);
        if (Settings.SYNCHRONIZE_FIXED_UPDATE_WITH_UPDATE)
        {
            Globals.FrameLengthMilliseconds = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Globals.FrameLengthSeconds = gameTime.GetElapsedSeconds();
            Globals.Time = gameTime;
            Globals.FixedFrameLengthSeconds = Globals.FrameLengthSeconds;
            Globals.FixedUpdateAlphaTime = 0;
            
            FixedUpdate();
        }
        else
        {
            float currentFrameTotalMilliseconds = stopwatch.ElapsedMilliseconds;
            float currentFrameLengthMilliseconds = currentFrameTotalMilliseconds - previousFrameTotalMilliseconds;

            // Only ran at the first frame
            if (previousFrameTotalMilliseconds == 0)
            {
                fixedUpdateFrameLengthMilliseconds = fixedUpdateTargetFrameLengthMilliseconds;
                previousFrameTotalMilliseconds = stopwatch.ElapsedMilliseconds;
            }
            
            if (currentFrameLengthMilliseconds > maximumFrameTimeToAllowFixedUpdate)
            {
                currentFrameLengthMilliseconds = maximumFrameTimeToAllowFixedUpdate;
            }
            previousFrameTotalMilliseconds = currentFrameTotalMilliseconds;

            accumulator += currentFrameLengthMilliseconds;

            while (accumulator >= fixedUpdateFrameLengthMilliseconds)
            {
                Globals.FixedFrameLengthSeconds = accumulator / 1000;
                Globals.FixedFrameLengthMilliseconds = accumulator;
                //Globals.FixedFrameLengthMilliseconds = (float)stopwatch.Elapsed.TotalMilliseconds;
                
                FixedUpdate();
                
                accumulator -= fixedUpdateFrameLengthMilliseconds;
            }
            
            Globals.FixedUpdateAlphaTime = accumulator / fixedUpdateFrameLengthMilliseconds;
            
            Globals.Time = gameTime;
            Globals.FrameLengthSeconds = currentFrameLengthMilliseconds / 1000;
            Globals.FrameLengthMilliseconds = currentFrameLengthMilliseconds;
        }

        InputManager.Update();
        
        simulationController.Update();
        
        UpdateCamera(gameTime);

        base.Update(gameTime);
    }

    private void FixedUpdate()
    {
        simulationController.FixedUpdate();
    }

    protected override void Draw(GameTime gameTime)
    {
        Graphics.Clear(Color.Black);

        // Blocks the draw call and game loop until textures transferred to GPU.
        Graphics.Textures[0] = null;

        simulationController.Draw(spriteBatch, MainCamera.GetViewMatrix());

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

        Globals.PlayerPosition = MainCamera.Center;
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