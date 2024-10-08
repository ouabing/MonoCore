
using Microsoft.Xna.Framework;

namespace G;

public class FrameCounter : Component
{
  private double frames;
  private double updates;
  private double elapsed;
  private double last;
  private double now;
  private readonly double msgFrequency = 1.0f;
  private string msg = "";

  public override void Update(GameTime gameTime)
  {
    now = gameTime.TotalGameTime.TotalSeconds;
    elapsed = now - last;
    if (elapsed > msgFrequency)
    {
      msg = $"FPS: {frames / elapsed}";
      elapsed = 0;
      frames = 0;
      updates = 0;
      last = now;
    }
    updates++;
  }

  public override void Draw(GameTime gameTime)
  {
    var font = Core.Font.Get(12);
    font.DrawText(Core.Sb, msg, new Vector2(10, 10), Palette.White);
    frames++;
  }
}