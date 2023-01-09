namespace Grit.Simulation;

public static class Settings
{
    // FPS settings
    public const bool UNLOCK_FRAMERATE = false;
    
    // World generation settings
    public const int WORLD_WIDTH = 512;
    public const int WORLD_HEIGHT = 512;
    
    // Chunk settings
    public const bool USE_CHUNKS = true;
    public const int CHUNK_SIZE = 64;
    
    // Debug snow settings
    public const int SNOW_PER_SECOND = 100;
    
    // Debug draw settings
    public const bool DRAW_CHUNK_BORDERS = true;
    public const bool DRAW_CURSOR_POS = true;
    public const bool DRAW_DIRTY_RECTS = true;
}