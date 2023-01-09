using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Grit.Simulation;
using Grit.UI;

namespace Grit
{
    public class Game1 : Game
    {
        public static SpriteFont DebugFont;
        
        private readonly StringBuilder stringBuilder;
        private readonly Process process;
        private readonly FrameCounter frameCounter;
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public Game1()
        {
            process = Process.GetCurrentProcess();
            graphics = new GraphicsDeviceManager(this);
            frameCounter = new FrameCounter();
            stringBuilder = new StringBuilder(64);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
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
                IsFixedTimeStep = true;
                TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0d / 60d);
            }
        }

        protected override void Initialize()
        {
            SimulationManager.Initialize(Settings.WORLD_WIDTH, Settings.WORLD_HEIGHT, GraphicsDevice.PresentationParameters.Bounds, GraphicsDevice);

            base.Initialize();
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            CalculateFps(gameTime);

            SimulationManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Blocks the draw call and game loop until textures transferred to GPU.
            GraphicsDevice.Textures[0] = null;
            
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            
            SimulationManager.Draw(spriteBatch);
            
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void CalculateFps(GameTime gameTime)
        {
            frameCounter.Update(gameTime.ElapsedGameTime.TotalSeconds);
            long usedMemory = process.PrivateMemorySize64 / 1048576;
            
            stringBuilder.Clear().AppendFormat("{0:F1} FPS @ {1} MB", frameCounter.CurrentFramesPerSecond, usedMemory);
            
            Window.Title = stringBuilder.ToString();
        }
    }
}