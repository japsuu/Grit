
using System;
using Grit.Simulation.Elements.ElementDefinitions;
using Grit.Simulation.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace Grit.Simulation;

public class SimulationManager
{
    #region PRIVATE FIELDS

    private readonly Simulation simulation;
    private readonly SimulationRenderer renderer;

    #endregion

    
    #region PRIVATE DEBUG FIELDS

    private bool isSnowingToggled;

    #endregion
    
    private static bool IsPositionInsideWorld(Point p) => p.X >= 0 && p.Y >= 0 && p.X < Settings.WORLD_WIDTH && p.Y < Settings.WORLD_HEIGHT;

    
    #region PUBLIC METHODS

    public SimulationManager(GraphicsDevice graphics)
    {
        if (Settings.WORLD_WIDTH % Settings.WORLD_CHUNK_SIZE != 0 || Settings.WORLD_HEIGHT % Settings.WORLD_CHUNK_SIZE != 0)
            throw new Exception($"World size is not dividable by ChunkSize {Settings.WORLD_CHUNK_SIZE}!");

        simulation = Settings.CHUNKING_ENABLED ? new MultithreadedSimulation(Settings.SIMULATION_TARGET_TPS) : new SinglethreadedSimulation(Settings.SIMULATION_TARGET_TPS);
        
        renderer = new SimulationRenderer(simulation, new Texture2D(graphics, Settings.WORLD_WIDTH, Settings.WORLD_HEIGHT));
    }

    public void Update(GameTime time)
    {
        HandleInput();

        if (isSnowingToggled)
            SpawnDebugSnow();
        
        renderer.Update();
    }


    public void Draw(SpriteBatch spriteBatch, Matrix cameraMatrix)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: cameraMatrix);

        renderer.DrawWorld(spriteBatch);

        spriteBatch.End();

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        renderer.DrawUi(spriteBatch);

        spriteBatch.End();
    }
    

    public void Dispose()
    {
        simulation.Dispose();
        renderer.Dispose();
    }

    #endregion

    
    #region PRIVATE METHODS

    private void SpawnDebugSnow()
    {
        // Spawn elements in random positions across the world.
        for (int i = 0; i < Settings.DEBUG_SNOW_PER_SECOND; i++)
        {
            int x = RandomFactory.SeedlessRandom.Next(Settings.WORLD_WIDTH - 1);
            int y = RandomFactory.SeedlessRandom.Next(Settings.WORLD_HEIGHT - 1);
                
            simulation.SetElementAt(x, y, new WaterElement(x, y));
        }
    }

    private void HandleInput()
    {
        Point mouseWorldPos = InputManager.MouseWorldPos.ToPoint();
        
        // Creating Elements with mouse input
        if(InputManager.IsMouseButtonDown(MouseButton.Left) && IsPositionInsideWorld(mouseWorldPos))
            simulation.SetElementAt(mouseWorldPos.X, mouseWorldPos.Y, new SandElement(mouseWorldPos.X, mouseWorldPos.Y));
            
        if(InputManager.IsMouseButtonDown(MouseButton.Right) && IsPositionInsideWorld(mouseWorldPos))
            simulation.SetElementAt(mouseWorldPos.X, mouseWorldPos.Y, new StoneElement(mouseWorldPos.X, mouseWorldPos.Y));
            
        if(InputManager.IsMouseButtonDown(MouseButton.Middle) && IsPositionInsideWorld(mouseWorldPos))
            simulation.SetElementAt(mouseWorldPos.X, mouseWorldPos.Y, new WaterElement(mouseWorldPos.X, mouseWorldPos.Y));

        
        // Keyboard input
        if (InputManager.WasKeyJustUp(Keys.X))
        {
            isSnowingToggled = !isSnowingToggled;
        }

        if (InputManager.IsKeyDown(Keys.C))
        {
            simulation.ForceSkipAll();
        }

        if (InputManager.IsKeyDown(Keys.V))
        {
            simulation.ForceStepAll();
        }

        if (InputManager.WasKeyJustUp(Keys.Multiply))
        {
            Game1.Instance.ChangeTargetFps(5);
        }

        if (InputManager.WasKeyJustUp(Keys.Divide))
        {
            Game1.Instance.ChangeTargetFps(-5);
        }

        
    }

    #endregion
}