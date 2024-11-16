
using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public class Oscillator(float min, float max, float speed, float delay)
{
  public float Value { get; private set; }
  public float Min { get; private set; } = min;
  public float Max { get; private set; } = max;
  public float Speed { get; private set; } = speed;
  public float Delay { get; private set; } = delay;
  public float Timer { get; private set; }

  public void Update(GameTime gameTime)
  {
    Value = MathHelper.Lerp(Min, Max, MathF.Sin((Timer + Delay) * Speed));
    Timer += gameTime.GetElapsedSeconds();
  }
}