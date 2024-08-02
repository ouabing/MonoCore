using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;


public class Shaker
{
  private class ShakeObject
  {
    public double Time;
    public double CreatedAt { get; }
    public bool Shaking = true;
    private float Amplitude { get; set; }
    public float Duration { get; }
    private float Frequency { get; set; }
    private readonly float[] samples;

    public ShakeObject(float amplitude, float duration, float frequency)
    {
      Amplitude = amplitude;
      Duration = duration;
      Frequency = frequency;
      var sampleCount = (int)(duration * frequency);
      samples = new float[sampleCount];
      for (var i = 0; i < sampleCount; i++)
      {
        samples[i] = Core.Random.NextSingle(-1, 1);
      }
      CreatedAt = Core.Timer.Time;
    }

    public float GetNoise(int s)
    {
      if (s >= samples.Length)
      {
        return 0;
      }
      return samples[s];
    }

    private double GetDecay(double time)
    {
      if (time > Duration)
      {
        return 0;
      }
      return (Duration - time) / Duration;
    }

    public double GetAmplitude()
    {
      if (Shaking == false)
      {
        return 0;
      }
      double s = Time * Frequency;
      int s0 = (int)Math.Floor(s);
      int s1 = s0 + 1;
      var decay = GetDecay(Time);
      return Amplitude * (GetNoise(s0) + (s - s0) * (GetNoise(s1) - GetNoise(s0))) * decay;
    }
  }

  public Vector2 Amount { get; private set; }
  private Vector2 springShakeAmount;
  private Vector2 shakeAmount;
  private Vector2 lastShakeAmount;
  private readonly Spring SpringX = new();
  private readonly Spring SpringY = new();
  private float Duration;
  private double ShookAt;
  private bool Started;
  private bool Finished;
  private readonly List<ShakeObject> ShakesX = [];
  private readonly List<ShakeObject> ShakesY = [];


  // Shakes the object with a certain intensity towards angle r using a spring mechanism
  // k and d are stiffness and damping spring values
  // Spring(10, math.pi/4) -> shakes the object with 10 intensity diagonally
  public void Spring(float intensity, float duration, float radian = 0, float? stiffness = null, float? damping = null)
  {
    Started = true;
    ShookAt = Core.Timer.Time;
    Duration = duration;
    SpringX.Pull(-intensity * (float)Math.Cos(radian), stiffness, damping);
    SpringY.Pull(-intensity * (float)Math.Sin(radian), stiffness, damping);
  }

  // Shakes the object with a certain intensity for duration seconds and with the specified frequency
  // Higher frequency means jerkier movement, lower frequency means smoother movement
  // Shake(10, 1, 120) -> shakes the object with 10 intensity for 1 second and 120 frequency
  public void Shake(float intensity, float duration, int frequency)
  {
    Started = true;
    ShookAt = Core.Timer.Time;
    Duration = duration;
    ShakesX.Add(new ShakeObject(intensity, duration, frequency));
    ShakesY.Add(new ShakeObject(intensity, duration, frequency));
  }

  public void ShakeHorizontal(float intensity, float duration, int frequency)
  {
    Started = true;
    ShookAt = Core.Timer.Time;
    Duration = duration;
    ShakesX.Add(new ShakeObject(intensity, duration, frequency));
  }

  public void ShakeVertical(float intensity, float duration, int frequency)
  {
    Started = true;
    ShookAt = Core.Timer.Time;
    Duration = duration;
    ShakesY.Add(new ShakeObject(intensity, duration, frequency));
  }

  public void Update(GameTime gameTime)
  {
    if (!Started || Finished)
    {
      return;
    }

    if (Core.Timer.Time - ShookAt > Duration + 0.1)
    {
      ShookAt = 0;
      Finished = true;
    }

    shakeAmount = Vector2.Zero;
    springShakeAmount = Vector2.Zero;

    foreach (var shake in ShakesX)
    {
      UpdateShake(gameTime, shake, XY.X);
    }
    ShakesX.RemoveAll(s => !s.Shaking);
    SpringX.Update(gameTime);

    foreach (var shake in ShakesY)
    {
      UpdateShake(gameTime, shake, XY.Y);
    }
    ShakesY.RemoveAll(s => !s.Shaking);
    SpringY.Update(gameTime);

    springShakeAmount += new Vector2(SpringX.Amount, SpringY.Amount);

    Amount -= lastShakeAmount;
    lastShakeAmount = springShakeAmount + shakeAmount;
    Amount += lastShakeAmount;
  }

  // Return true if need to remove shake
  private void UpdateShake(GameTime gameTime, ShakeObject shake, XY xy)
  {
    shake.Time = Core.Timer.Time - shake.CreatedAt;
    if (shake.Time > shake.Duration)
    {
      shake.Shaking = false;
    }

    if (xy == XY.X)
    {
      shakeAmount.X += (float)shake.GetAmplitude();
    }
    else if (xy == XY.Y)
    {
      shakeAmount.Y += (float)shake.GetAmplitude();
    }

  }
}