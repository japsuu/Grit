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

public class Game1 : Game
{
    public static Game1 Instance { get; private set; }
    public static SpriteFont DebugFont { get; private set; }
    public static OrthographicCamera MainCamera { get; private set; }

    private readonly StringBuilder stringBuilder;
    private readonly Process process;
    private readonly FrameCounter frameCounter;
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private static float previousScrollValue;
    private int currentTargetFps;

    public Game1()
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
            SetTargetFps(100);
        }
    }

    #region GAME LOGIC

    protected override void Initialize()
    {
        base.Initialize();

        ViewportAdapter wpa =
            new BoxingViewportAdapter(Window, GraphicsDevice, Settings.WORLD_WIDTH, Settings.WORLD_HEIGHT);
        MainCamera = new OrthographicCamera(wpa)
        {
            MinimumZoom = 1,
            MaximumZoom = 40
        };

        SimulationManager.Initialize(Settings.WORLD_WIDTH, Settings.WORLD_HEIGHT,
            GraphicsDevice.PresentationParameters.Bounds, GraphicsDevice);
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        DebugFont = Content.Load<SpriteFont>("DebugFont");
    }

    protected override void UnloadContent()
    {
        SimulationManager.Dispose();
        graphics.Dispose();
        spriteBatch.Dispose();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        CalculateFps(gameTime);

        SimulationManager.UpdateSimulation(gameTime);

        UpdateCamera(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // Blocks the draw call and game loop until textures transferred to GPU.
        GraphicsDevice.Textures[0] = null;

        // Apply the camera transform matrix
        Matrix cameraMatrix = MainCamera.GetViewMatrix();

        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: cameraMatrix);

        SimulationManager.DrawWorld(spriteBatch);

        spriteBatch.End();

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        SimulationManager.DrawUi(spriteBatch);

        spriteBatch.End();

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

    private static void UpdateCamera(GameTime gameTime)
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