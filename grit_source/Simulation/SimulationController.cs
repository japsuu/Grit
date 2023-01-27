
using System;
using Grit.Simulation.Elements.ElementDefinitions;
using Grit.Simulation.Rendering;
using Grit.Simulation.World.Regions.Chunks;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.ImGui.Standard;

namespace Grit.Simulation;

/// <summary>
/// TODO: Remove this script, or transform it to an input provider for Simulation.cs.
/// </summary>
public class SimulationController
{
    private readonly Simulation simulation;
    private readonly SimulationRenderer renderer;
    private readonly ChunkManager chunkManager;
    
    private float previousScrollValue;
    private bool isSnowingToggled;

    #region PUBLIC METHODS

    public SimulationController()
    {
        chunkManager = new ChunkManager();
        simulation = new Simulation(chunkManager);
        renderer = new SimulationRenderer(simulation, chunkManager);
        ChunkFactory.Initialize(simulation);
    }

    public void Update()
    {
        HandleInput();
        
        UpdateCamera();

        if (isSnowingToggled)
            SpawnDebugSnow();
    }

    public void FixedUpdate()
    {
        chunkManager.FixedUpdate();
        
        simulation.FixedUpdate();
        
        renderer.FixedUpdate();
    }


    public void Draw(SpriteBatch spriteBatch, ImGUIRenderer imGuiRenderer, Matrix cameraMatrix)
    {
        // Camera matrix
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: cameraMatrix);

        renderer.DrawWithMatrix(spriteBatch);

        spriteBatch.End();

        // No camera matrix
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        renderer.DrawWithoutMatrix(spriteBatch);

        spriteBatch.End();
        
        
        // ImGUI
        imGuiRenderer.BeginLayout(Globals.Time);
        
        ImGui.Checkbox("Draw Chunk Borders", ref Settings.DrawChunkBorders);
        ImGui.Checkbox("Draw Cursor Pos", ref Settings.DrawCursorPos);
        ImGui.Checkbox("Draw Cursor Hovered Element", ref Settings.DrawCursorHoveredElement);
        ImGui.Checkbox("Draw Dirty Rects", ref Settings.DrawDirtyRects);
        ImGui.Checkbox("Flash Dirty Chunks", ref Settings.FlashDirtyChunks);
        ImGui.Checkbox("Draw Chunk Load Radius", ref Settings.DrawChunkLoadRadius);
        ImGui.Checkbox("Draw Chunk Tick Radius", ref Settings.DrawChunkTickRadius);
        ImGui.Checkbox("Draw Random Ticks", ref Settings.DrawRandomTicks);
        ImGui.Spacing();
        ImGui.DragFloat("Camera speed", ref Settings.CameraMovementSpeed, 1f, 5f, 1000f);

        renderer.DrawImGui(imGuiRenderer);

        imGuiRenderer.EndLayout();
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
        if (!ImGui.GetIO().WantCaptureMouse)
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
        }
        
        if (!ImGui.GetIO().WantCaptureKeyboard)
        {
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
    }
    
    private void UpdateCamera()
    {
        Grit.MainCamera.Move(GetCameraMovementDirection() * Settings.CameraMovementSpeed * Globals.FrameLengthSeconds);

        // Logarithmic zoom gang.
        if (!ImGui.GetIO().WantCaptureMouse)
        {
            if (previousScrollValue < Mouse.GetState().ScrollWheelValue)
                Grit.MainCamera.ZoomIn(Grit.MainCamera.Zoom * 10f * Globals.FrameLengthSeconds);
            else if (previousScrollValue > Mouse.GetState().ScrollWheelValue)
                Grit.MainCamera.ZoomOut(Grit.MainCamera.Zoom * 10f * Globals.FrameLengthSeconds);

            previousScrollValue = Mouse.GetState().ScrollWheelValue;
        }

        Globals.PlayerPosition = Grit.MainCamera.Center;
    }

    private static Vector2 GetCameraMovementDirection()
    {
        Vector2 movementDirection = Vector2.Zero;

        if (ImGui.GetIO().WantCaptureKeyboard)
            return movementDirection;

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