
using System;
using System.Diagnostics;
using Grit.Simulation.Elements.ElementDefinitions;
using Grit.Simulation.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace Grit.Simulation;

/// <summary>
/// TODO: Remove this script, or transform it to an input provider for Simulation.cs.
/// </summary>
public class SimulationController
{
    private readonly Simulation simulation;

    
    private bool isSnowingToggled;

    #region PUBLIC METHODS

    public SimulationController()
    {
        simulation = new Simulation();
    }

    public void Update()
    {
        HandleInput();

        if (isSnowingToggled)
            SpawnDebugSnow();
    }

    public void FixedUpdate()
    {
        simulation.FixedUpdate();
    }


    public void Draw(SpriteBatch spriteBatch, Matrix cameraMatrix)
    {
        simulation.Draw(spriteBatch, cameraMatrix);
    }
    

    public void Dispose()
    {
        simulation.Dispose();
    }

    #endregion

    
    #region PRIVATE METHODS

    private void SpawnDebugSnow()
    {
        throw new NotImplementedException();
        // Spawn elements in random positions across the world.
        // for (int i = 0; i < Settings.DEBUG_SNOW_PER_SECOND; i++)
        // {
        //     int x = RandomFactory.SeedlessRandom.Next(Settings.WORLD_WIDTH - 1);
        //     int y = RandomFactory.SeedlessRandom.Next(Settings.WORLD_HEIGHT - 1);
        //         
        //     simulation.SetElementAt(x, y, new WaterElement(x, y));
        // }
    }

    private void HandleInput()
    {
        Point mouseWorldPos = InputManager.MousePixelWorldPosition;

        // Creating Elements with mouse input
        if (Grit.ScreenBounds.Contains(InputManager.Mouse.Position))
        {
            if(InputManager.IsMouseButtonDown(MouseButton.Left))
                simulation.SetElementAt(mouseWorldPos.X, mouseWorldPos.Y, new SandElement(mouseWorldPos.X, mouseWorldPos.Y));
            
            if(InputManager.IsMouseButtonDown(MouseButton.Right))
                simulation.SetElementAt(mouseWorldPos.X, mouseWorldPos.Y, new StoneElement(mouseWorldPos.X, mouseWorldPos.Y));
            
            if(InputManager.IsMouseButtonDown(MouseButton.Middle))
                simulation.SetElementAt(mouseWorldPos.X, mouseWorldPos.Y, new WaterElement(mouseWorldPos.X, mouseWorldPos.Y));
        }
        
        
        // Keyboard input
        if (InputManager.WasKeyJustUp(Keys.X))
        {
            isSnowingToggled = !isSnowingToggled;
        }

        if (InputManager.IsKeyDown(Keys.C))
        {
            simulation.ForceCleanAll();
        }

        if (InputManager.IsKeyDown(Keys.V))
        {
            simulation.ForceDirtyAll();
        }

        if (InputManager.WasKeyJustUp(Keys.Multiply))
        {
            Grit.Instance.ChangeTargetFps(5);
        }

        if (InputManager.WasKeyJustUp(Keys.Divide))
        {
            Grit.Instance.ChangeTargetFps(-5);
        }
    }

    #endregion
}