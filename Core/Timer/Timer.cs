using Microsoft.Xna.Framework;

namespace G;

public class Timer
{
  public double Time { get; private set; }

  public void Update(GameTime gameTime)
  {
    Time = gameTime.TotalGameTime.TotalSeconds;
  }
}