using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public class Spring(float targetAmount = 0, float stiffness = 100, float damping = 10)
{
  // Spring amount
  public float Amount { get; private set; } = targetAmount;
  private float TargetAmount = targetAmount;
  // The higher stiffness the more aggressively the spring will go back to its initial value
  private float Stiffness = stiffness;
  // The lower the dampening the longer the spring will oscillate for.
  private float Damping = damping;
  private float Velocity;

  public void Update(GameTime gameTime)
  {
    var acc = -Stiffness * (Amount - TargetAmount) - Damping * Velocity;
    var dt = gameTime.GetElapsedSeconds();
    Velocity += acc * dt;
    Amount += Velocity * dt;
  }

  public void Pull(float amount, float? stiffness = null, float? damping = null)
  {
    if (stiffness != null)
    {
      Stiffness = stiffness.Value;
    }
    if (damping != null)
    {
      Damping = damping.Value;
    }

    Amount += amount;
  }

  public void Push(float amount, float? stiffness = null, float? damping = null)
  {
    if (stiffness != null)
    {
      Stiffness = stiffness.Value;
    }
    if (damping != null)
    {
      Damping = damping.Value;
    }

    TargetAmount = amount;
  }

  public void Stop()
  {
    Amount = TargetAmount;
  }

  public bool IsActivated()
  {
    return Amount != TargetAmount;
  }
}