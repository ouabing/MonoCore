
using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

/**
  * Simple wind simulator, blows in a random direction every cycleDuration seconds
  */
public class WindSim(float cycleDuration = 1f)
{
  private float timer = cycleDuration;
  private float windTimer;
  private readonly float CycleDuration = cycleDuration;
  // Range [0, 1]
  public float Force { get; private set; }
  public bool Started { get; private set; }
  public Vector2 Direction { get; private set; }

  private void CreateWind()
  {
    // Only support horizontal wind for now
    var x = Core.Random.NextPick([0.5f, 0.5f], [-1, 1]);
    Direction = new Vector2(x, 0);
    Started = true;
  }

  public void UpdateWind(GameTime gameTime)
  {
    if (!Started)
    {
      return;
    }
    windTimer += gameTime.GetElapsedSeconds();

    float phase = windTimer / CycleDuration * MathHelper.TwoPi;

    Force = (float)((Math.Sin(phase) + 1) / 2);

    if (windTimer > CycleDuration)
    {
      Started = false;
      windTimer = 0;
    }
  }

  public void Update(GameTime gameTime)
  {
    if (timer > CycleDuration)
    {
      timer = 0;
      CreateWind();
    }
    UpdateWind(gameTime);
    timer += gameTime.GetElapsedSeconds();
  }
}
