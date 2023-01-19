using Microsoft.Xna.Framework;

namespace Grit.Simulation;

public static class Globals
{
    /// <summary>
    /// Current GameTime.
    /// </summary>
    public static GameTime Time;
    
    /// <summary>
    /// How many seconds the last Update frame took.
    /// </summary>
    public static float DeltaTime;
    
    /// <summary>
    /// How many seconds the last FixedUpdate frame took.
    /// </summary>
    public static float FixedUpdateDeltaTime;
    
    /// <summary>
    /// How much more time is required before another whole physics step can be taken.
    /// Range [0,1].
    /// </summary>
    public static float FixedUpdateAlphaTime;
    
    /// <summary>
    /// How many milliseconds the last frame lasted.
    /// </summary>
    public static float FrameLengthMilliseconds;
}