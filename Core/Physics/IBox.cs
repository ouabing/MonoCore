using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public interface IBox
{
  public bool IsDead { get; set; }
  public abstract BaseShape Shape { get; }
  public abstract Vector2 Position { get; }
  public abstract Vector2 Origin { get; }
  public abstract Vector2 PreviousPosition { get; }
  public abstract Vector2 Velocity { get; }

  public abstract void UpdatePhysics(GameTime gameTime);

  public abstract void OnCollision(Collision collision, IBox opponent);

  public virtual void DrawBox(GameTime gameTime)
  {
    if (Shape is ShapeRectangle rect)
    {
      if (rect.Rectangle.Width == 0 || rect.Rectangle.Height == 0)
      {
        return;
      }
      var boxAbs = new RectangleF(rect.Rectangle.X + Position.X, rect.Rectangle.Y + Position.Y, rect.Rectangle.Width, rect.Rectangle.Height);
      Core.Sb.DrawRectangle(boxAbs, Color.Red, 1);
    }
    else if (Shape is ShapeCircle circle)
    {
      if (circle.Circle.Radius == 0)
      {
        return;
      }
      var circleAbs = new CircleF(circle.Circle.Position + Position, circle.Circle.Radius);
      Core.Sb.DrawCircle(circleAbs, 16, Color.Red, 1);
    }
  }
}