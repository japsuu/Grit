
namespace Grit.Simulation;

public static class Settings
{
    #region FPS settings

    public const bool UNLOCK_FRAMERATE = true;

    #endregion


    #region World generation settings

    public const int WORLD_WIDTH = 512;
    public const int WORLD_HEIGHT = 512;

    #endregion


    #region Chunk settings

    public const bool USE_WORLD_CHUNKING = true;
    public const int WORLD_CHUNK_SIZE = 64;
    
    public const int CHUNK_COUNT_X = WORLD_WIDTH / WORLD_CHUNK_SIZE;
    public const int CHUNK_COUNT_Y = WORLD_HEIGHT / WORLD_CHUNK_SIZE;

    #endregion


    #region Debug snow settings

    /// <summary>
    /// How many random pixels are created on the screen each frame.
    /// </summary>
    public const int DEBUG_SNOW_PER_SECOND = 100;

    #endregion


    #region Debug draw settings

    public const bool DRAW_CHUNK_BORDERS = false;
    public const bool DRAW_CURSOR_POS = false;
    public const bool DRAW_DIRTY_RECTS = false;

    #endregion


    #region Camera settings

    public const float CAMERA_MOVEMENT_SPEED = 200;

    #endregion


    #region World update settings

    /// <summary>
    /// Whether or not <see cref="RANDOM_TICKS_PER_FRAME"/> selections are made and updated each frame.
    /// </summary>
    public const bool RANDOM_TICKS_ENABLED = true;

    /// <summary>
    /// Whether or not to split the world updating to multiple threads.
    /// </summary>
    public const bool MULTITHREADING_ENABLED = true;
    
    /// <summary>
    /// How many additional random pixels are selected and updated each frame?
    /// </summary>
    public const int RANDOM_TICKS_PER_FRAME = 64;

    #endregion
}