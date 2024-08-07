using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public class Flash(float duration = 0.15f)
{
  private float Duration = duration;
  private float? Timer;
  public bool IsVisible { get; private set; }

  public void Update(GameTime gameTime)
  {
    if (Timer is null)
    {
      return;
    }
    Timer += gameTime.GetElapsedSeconds();
    if (Timer >= Duration)
    {
      IsVisible = true;
      Timer = null;
    }
  }

  public void Start(float? duration = null)
  {
    Timer = 0;
    if (duration != null)
    {
      Duration = duration.Value;
    }
    IsVisible = false;
  }

  public void Stop()
  {
    Timer = null;
    IsVisible = true;
  }
}