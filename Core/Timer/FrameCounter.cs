
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public class FrameCounter : Component
{
  public long TotalFrames { get; private set; }
  public float TotalSeconds { get; private set; }
  public float AverageFramesPerSecond { get; private set; }
  public float CurrentFramesPerSecond { get; private set; }

  public override int Z => 0;

  public const int MaximumSamples = 100;

  private readonly Queue<float> sampleBuffer = new();

  public override void Update(GameTime gameTime)
  {
    var deltaTime = gameTime.GetElapsedSeconds();
    CurrentFramesPerSecond = 1.0f / deltaTime;

    sampleBuffer.Enqueue(CurrentFramesPerSecond);

    if (sampleBuffer.Count > MaximumSamples)
    {
      sampleBuffer.Dequeue();
      AverageFramesPerSecond = sampleBuffer.Average(i => i);
    }
    else
    {
      AverageFramesPerSecond = CurrentFramesPerSecond;
    }

    TotalFrames++;
    TotalSeconds += deltaTime;
  }

  public override void Draw(GameTime gameTime)
  {
    var font = Core.Font(FontSize.Medium);
    font.DrawText(Core.Sb, $"FPS: {AverageFramesPerSecond}", new Vector2(10, 10), Color.White);
  }

  public override void LoadContent()
  {
  }
}