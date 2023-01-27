
namespace Grit.Simulation;

public static class Settings
{
    #region Update settings

    /// <summary>
    /// If enabled, we won't limit the frame rate.
    /// </summary>
    public const bool UNLOCK_FRAMERATE = true;

    /// <summary>
    /// If enabled, we won't limit the simulation rate.
    /// </summary>
    public const bool SYNCHRONIZE_FIXED_UPDATE_WITH_UPDATE = false;

    /// <summary>
    /// Dictates how many times per second the FixedUpdate-loop is ran.
    /// </summary>
    public const int FIXED_UPDATE_TARGET_TPS = 10;

    /// <summary>
    /// FixedUpdate calls are limited to this many times per Update call.
    /// </summary>
    public const int MAXIMUM_FIXED_UPDATES_PER_FRAME = 4;

    /// <summary>
    /// Whether or not <see cref="RANDOM_TICKS_PER_FRAME"/> selections are made and updated each frame.
    /// </summary>
    public const bool RANDOM_TICKS_ENABLED = true;

    /// <summary>
    /// How many additional random pixels are selected and updated each frame?
    /// </summary>
    public const int RANDOM_TICKS_PER_FRAME = 3;

    #endregion


    public const int WINDOW_WIDTH = 1280;
    public const int WINDOW_HEIGHT = 720;


    #region Chunk settings

    public const int WORLD_CHUNK_SIZE = 64;

    //TODO: Try using WINDOW_WIDTH here?
    public const int CHUNK_TICK_RADIUS = 128;
    public const int CHUNK_TICK_RADIUS_SQUARED = CHUNK_TICK_RADIUS * CHUNK_TICK_RADIUS;

    //TODO: Try using WINDOW_WIDTH here?
    public const int CHUNK_LOAD_RADIUS = 256;
    public const int CHUNK_LOAD_RADIUS_SQUARED = CHUNK_LOAD_RADIUS * CHUNK_LOAD_RADIUS;

    // How many seconds it takes for a chunk to unload, after it has left the load radius.
    public const float UNLOADED_CHUNK_LIFETIME = 2f;

    #endregion


    #region Debug snow settings

    /// <summary>
    /// How many random pixels are created on the screen each frame.
    /// </summary>
    public const int DEBUG_SNOW_PER_SECOND = 100;

    #endregion


    #region Debug draw settings

    public const bool DRAW_CHUNK_BORDERS = true;
    public const bool DRAW_CURSOR_POS = true;
    public const bool DRAW_HOVERED_ELEMENT = true;
    public const bool DRAW_DIRTY_RECTS = true;
    public const bool FLASH_DIRTY_CHUNKS = true;
    public const bool DRAW_CHUNK_LOAD_RADIUS = true;
    public const bool DRAW_CHUNK_TICK_RADIUS = true;
    public const bool DRAW_RANDOM_TICKS = true;

    #endregion


    #region Camera settings

    public const float CAMERA_MOVEMENT_SPEED = 200;

    #endregion
}