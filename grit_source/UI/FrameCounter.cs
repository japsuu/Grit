using System.Collections.Generic;
using System.Linq;

namespace Grit.UI;

public class FrameCounter
{
    public long TotalFrames { get; private set; }
    public double TotalSeconds { get; private set; }
    public double AverageFramesPerSecond { get; private set; }
    public double CurrentFramesPerSecond { get; private set; }

    public int Min = int.MaxValue;
    public int Max = int.MinValue;

    public const int MAXIMUM_SAMPLES_NORMAL = 10;
    public const int MAXIMUM_SAMPLES_LOW_FPS = 5;

    private readonly Queue<double> sampleBuffer = new();

    public bool Update(double deltaTime)
    {
        CurrentFramesPerSecond = 1.0 / deltaTime;

        sampleBuffer.Enqueue(CurrentFramesPerSecond);

        if (sampleBuffer.Count > (CurrentFramesPerSecond < 20 ? MAXIMUM_SAMPLES_LOW_FPS : MAXIMUM_SAMPLES_NORMAL))
        {
            sampleBuffer.Dequeue();
            AverageFramesPerSecond = sampleBuffer.Average(i => i);
        } 
        else
        {
            AverageFramesPerSecond = CurrentFramesPerSecond;
        }

        if (CurrentFramesPerSecond < Min)
            Min = (int)CurrentFramesPerSecond;

        if (CurrentFramesPerSecond > Max)
            Max = (int)CurrentFramesPerSecond;

        TotalFrames++;
        TotalSeconds += deltaTime;
        return true;
    }
}