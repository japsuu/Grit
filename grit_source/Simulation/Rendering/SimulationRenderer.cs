
using System;
using Grit.Simulation.Elements;
using Grit.Simulation.World.Regions.Chunks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.ImGui.Standard;

namespace Grit.Simulation.Rendering;

public class SimulationRenderer
{
    private const float DIRTY_CHUNK_FLASH_DELAY = 0.5f;

    private readonly Simulation simulation;
    private readonly ChunkManager chunkManager;

    private string objectUnderCursor;
    private float dirtyChunkFlashTimer;
    private bool dirtyChunkShouldFlash;


    public SimulationRenderer(Simulation simulation, ChunkManager chunkManager)
    {
        this.simulation = simulation;
        this.chunkManager = chunkManager;
    }
    
    
    public void FixedUpdate()
    {
        for (int i = 0; i < chunkManager.CurrentlyLoadedChunks.Count; i++)
        {
            chunkManager.CurrentlyLoadedChunks[i].Render();
        }
        
        if (dirtyChunkFlashTimer <= 0)
        {
            dirtyChunkFlashTimer = DIRTY_CHUNK_FLASH_DELAY;
            dirtyChunkShouldFlash = !dirtyChunkShouldFlash;
        }
        
        if(Settings.FlashDirtyChunks)
            dirtyChunkFlashTimer -= Globals.FixedFrameLengthSeconds;
    }

    
    /// <summary>
    /// Draws all elements which should be affected by the camera matrix.
    /// </summary>
    public void DrawWithMatrix(SpriteBatch spriteBatch)
    {
        foreach (Chunk chunk in chunkManager.CurrentlyLoadedChunks)
        {
            // Draw the chunk
            spriteBatch.Draw(chunk.Canvas, chunk.Rectangle, Color.White);
            
            // Draw chunk borders
            if (Settings.DrawChunkBorders)
            {
                DrawChunkBorders(chunk, spriteBatch);
            }
            
            // Flash dirty chunks
            if (Settings.FlashDirtyChunks && chunk.IsDirty && chunk.State == Chunk.LoadState.Ticking && dirtyChunkShouldFlash)
            {
                spriteBatch.DrawRectangle(chunk.Rectangle, Color.Red, 2f);
            }
        
            
            // Debug draw chunk lifetime:
            //spriteBatch.DrawString(Grit.DebugFont, chunkManager.CurrentlyLoadedChunks[i].DebugLifetime.ToString(), chunkManager.CurrentlyLoadedChunks[i].Rectangle.Center.ToVector2(), Color.Blue);
            
            // Draw random ticks
            if (Settings.DrawRandomTicks && chunk.State == Chunk.LoadState.Ticking)
            {
                int chunkHeight = chunk.Rectangle.Height;
                int chunkWidth = chunk.Rectangle.Width;
                Point chunkPosition = chunk.Rectangle.Location;
                
                for (int y = 0; y < chunkHeight; y++)
                {
                    for (int x = 0; x < chunkWidth; x++)
                    {
                        int index = x + y * chunkWidth;
                        if (chunk.RandomTickSteppedCells[index])
                        {
                            RectangleF rect = new(chunkPosition.X + x, chunkPosition.Y + y, 1, 1);
                            spriteBatch.DrawRectangle(rect, Color.Red, 2f);
                        }
                    }
                }
            }
            
            // Draw dirty rects
            if (Settings.DrawDirtyRects)
            {
                if (!chunk.DirtyRect.IsEmpty && chunk.State == Chunk.LoadState.Ticking)
                    spriteBatch.DrawRectangle(chunk.DirtyRect, Color.Red, 1f);
            }
        }

        if (Settings.DrawChunkLoadRadius)
        {
            spriteBatch.DrawCircle(Globals.PlayerPosition, Settings.CHUNK_LOAD_RADIUS, 12, Color.Orange);
        }

        if (Settings.DrawChunkTickRadius)
        {
            spriteBatch.DrawCircle(Globals.PlayerPosition, Settings.CHUNK_TICK_RADIUS, 12, Color.Yellow);
        }

        spriteBatch.DrawCircle(Globals.PlayerPosition, 2, 8, Color.Lime);
    }


    /// <summary>
    /// Draws all elements which should NOT be affected by the camera matrix.
    /// </summary>
    public void DrawWithoutMatrix(SpriteBatch spriteBatch)
    {
        if (Settings.DrawCursorPos)
        {
            string cPos = InputManager.MousePixelWorldPosition.ToString();
            spriteBatch.DrawRectangle(InputManager.Mouse.Position.ToVector2() + new Vector2(15, 5), new Size2(100, 1), Color.Blue, 8f);
            spriteBatch.DrawString(Grit.DebugFont, cPos, InputManager.Mouse.Position.ToVector2() + new Vector2(20, 0), Color.Red);
        }

        if (Settings.DrawCursorHoveredElement)
        {
            Element element = simulation.GetElementAt(InputManager.MousePixelWorldPosition.X, InputManager.MousePixelWorldPosition.Y);
            objectUnderCursor = element != null ?
                element.ToString() :
                "none";
            
            // Draw top bar (object under the cursor)
            spriteBatch.DrawRectangle(new Vector2(0, 0), new Size2(Settings.WINDOW_WIDTH, 1), Color.Blue, 20f);
            spriteBatch.DrawString(Grit.DebugFont, objectUnderCursor, new Vector2(5, 5), Color.Red);
        }
    }

    public void DrawImGui(ImGUIRenderer imGuiRenderer)
    {
        
    }

    private void DrawChunkBorders(Chunk chunk, SpriteBatch spriteBatch)
    {
        Color color = chunk.State switch
        {
            Chunk.LoadState.Ticking => Color.Yellow,
            Chunk.LoadState.Loaded => Color.Orange,
            Chunk.LoadState.Unloading => Color.Red * (chunk.Lifetime / Settings.UNLOADED_CHUNK_LIFETIME),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        spriteBatch.DrawRectangle(chunk.Rectangle, color, 1f);
    }
}