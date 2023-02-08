using System;
using System.Collections.Generic;
using Grit.Simulation.Helpers;
using Microsoft.Xna.Framework;

namespace Grit.Simulation.World.Regions.Chunks;

public class ChunkManager
{
    // NOTE: These Lists could later be converted into HashSets for better performance, if needed.
    public readonly List<Chunk> CurrentlyLoadedChunks;
    public readonly List<Chunk> CurrentlyTickingChunks;
    
    private readonly Dictionary<Point, Chunk> currentlyLoadedChunkMapping;
    private readonly Dictionary<Point, Chunk> currentlyTickingChunkMapping;

    private readonly int halfAChunkSize;

    
    private bool IsChunkCurrentlyTicking(Point chunkPos) => currentlyTickingChunkMapping.ContainsKey(chunkPos);
    
    public bool GetChunkAt(Point worldPosition, out Chunk chunk)
    {
        return currentlyLoadedChunkMapping.TryGetValue(worldPosition, out chunk);
    }


    public ChunkManager()
    {
        halfAChunkSize = Settings.WORLD_CHUNK_SIZE / 2;
        currentlyLoadedChunkMapping = new Dictionary<Point, Chunk>();
        currentlyTickingChunkMapping = new Dictionary<Point, Chunk>();
        CurrentlyLoadedChunks = new List<Chunk>();
        CurrentlyTickingChunks = new List<Chunk>();
    }


    public void FixedUpdate()
    {
        // Unload old chunks
        for (int i = 0; i < CurrentlyLoadedChunks.Count; i++)
        {
            Point pos = CurrentlyLoadedChunks[i].Rectangle.Location;
            
            bool shouldBeUnloaded = (Globals.PlayerPosition.X - pos.X - halfAChunkSize) * (Globals.PlayerPosition.X - pos.X - halfAChunkSize) +
                                  (Globals.PlayerPosition.Y - pos.Y - halfAChunkSize) * (Globals.PlayerPosition.Y - pos.Y - halfAChunkSize) > 
                                  Settings.CHUNK_LOAD_RADIUS_SQUARED;

            if (!shouldBeUnloaded) continue;
            
            if (CurrentlyLoadedChunks[i].ReadyToUnload)
            {
                UnloadChunk(CurrentlyLoadedChunks[i]);
            }
            else
            {
                CurrentlyLoadedChunks[i].DecrementLifetime();
            }
        }
        
        //Debug.WriteLine($"Loop:X:{minX},Y:{minY} -> X:{maxX},Y:{maxY}.");

        // Could be made faster by:
        // Calculating distance with Manhattan distance instead of Pythagorean theorem (https://stackoverflow.com/a/6182469/11451794),
        // Using symmetry to obtain the positions in the other quadrants,
        // Using Bresenham's circle algorithm (?).
        
        // Load new chunks and update existing ones
        int minX = (int)Math.Round((Globals.PlayerPosition.X - Settings.CHUNK_LOAD_RADIUS) / Settings.WORLD_CHUNK_SIZE) * Settings.WORLD_CHUNK_SIZE;
        int maxX = (int)Math.Round((Globals.PlayerPosition.X + Settings.CHUNK_LOAD_RADIUS) / Settings.WORLD_CHUNK_SIZE) * Settings.WORLD_CHUNK_SIZE;
        int minY = (int)Math.Round((Globals.PlayerPosition.Y - Settings.CHUNK_LOAD_RADIUS) / Settings.WORLD_CHUNK_SIZE) * Settings.WORLD_CHUNK_SIZE;
        int maxY = (int)Math.Round((Globals.PlayerPosition.Y + Settings.CHUNK_LOAD_RADIUS) / Settings.WORLD_CHUNK_SIZE) * Settings.WORLD_CHUNK_SIZE;
        for (int x = minX; x <= maxX; x += Settings.WORLD_CHUNK_SIZE)
        {
            for (int y = minY; y <= maxY; y += Settings.WORLD_CHUNK_SIZE)
            {
                // If inside the load radius
                // bool shouldLoad = (Math.Sqrt(Math.Pow(Globals.PlayerPosition.x - x, 2) + Math.Pow(Globals.PlayerPosition.y - y, 2)) <= Settings.CHUNK_LOAD_RADIUS)
                bool shouldBeLoaded = (Globals.PlayerPosition.X - x - halfAChunkSize) * (Globals.PlayerPosition.X - x - halfAChunkSize) +
                                  (Globals.PlayerPosition.Y - y - halfAChunkSize) * (Globals.PlayerPosition.Y - y - halfAChunkSize) <= 
                                  Settings.CHUNK_LOAD_RADIUS_SQUARED;
                
                if(!shouldBeLoaded) continue;

                Point chunkPos = new(x, y);

                // If loaded
                if (currentlyLoadedChunkMapping.TryGetValue(chunkPos, out Chunk loadedChunk))
                {
                    //bool shouldTick = Math.Sqrt(Math.Pow(Globals.PlayerPosition.x - x, 2) + Math.Pow(Globals.PlayerPosition.y - y, 2)) <= Settings.CHUNK_TICK_RADIUS;
                    bool shouldTick = (Globals.PlayerPosition.X - x - halfAChunkSize) * (Globals.PlayerPosition.X - x - halfAChunkSize) +
                                      (Globals.PlayerPosition.Y - y - halfAChunkSize) * (Globals.PlayerPosition.Y - y - halfAChunkSize) <= 
                                      Settings.CHUNK_TICK_RADIUS_SQUARED;
                        
                    if (!shouldTick && IsChunkCurrentlyTicking(chunkPos))
                    {
                        StopTickingChunk(loadedChunk);
                    }
                    else if(shouldTick && !IsChunkCurrentlyTicking(chunkPos))
                    {
                        StartTickingChunk(loadedChunk);
                    }
                }
                else
                {
                    loadedChunk = LoadChunkAt(chunkPos);
                }
                
                loadedChunk.KeepAlive();
            }
        }
    }


    private void StartTickingChunk(Chunk chunk)
    {
        currentlyTickingChunkMapping.Add(chunk.Rectangle.Location, chunk);
        CurrentlyTickingChunks.Add(chunk);
    }
    
    
    private void StopTickingChunk(Chunk chunk)
    {
        currentlyTickingChunkMapping.Remove(chunk.Rectangle.Location);
        CurrentlyTickingChunks.Remove(chunk);
    }


    private Chunk LoadChunkAt(Point worldPosition)
    {
        // TODO: Check if chunk exists on disk. If yes, load from disk, if no, generate.
        Chunk chunk = ChunkFactory.GenerateChunk(worldPosition);
        
        currentlyLoadedChunkMapping.Add(worldPosition, chunk);
        CurrentlyLoadedChunks.Add(chunk);
        
        return chunk;
    }
    
    
    private void UnloadChunk(Chunk chunk)
    {
        CurrentlyLoadedChunks.Remove(chunk);
        CurrentlyTickingChunks.Remove(chunk);
        currentlyLoadedChunkMapping.Remove(chunk.Rectangle.Location);
        currentlyTickingChunkMapping.Remove(chunk.Rectangle.Location);
        
        chunk.Dispose();
        //TODO: Save the chunk to the disk
    }

    public void Unload()
    {
        //TODO: Save all chunks to the disk

        foreach (KeyValuePair<Point,Chunk> valuePair in currentlyLoadedChunkMapping)
        {
            Chunk chunk = valuePair.Value;
            chunk.Dispose();
        }
    }
}