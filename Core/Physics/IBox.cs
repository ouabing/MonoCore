using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;

namespace G;

public interface IBox
{
  public bool IsDead { get; set; }
  // Which collision category this box belongs to
  public abstract Def.Category Category { get; set; }
  // Which collision categories this box can collide with
  public abstract Def.Category CollidesWith { get; set; }
  public abstract BaseShape? Shape { get; }
  public abstract Vector2 Position { get; }
  public abstract float Rotation { get; }
  public abstract Vector2 Scale { get; }
  public abstract Vector2 PreviousPosition { get; }
  public abstract Vector2 Velocity { get; }

  public abstract void UpdatePhysics(GameTime gameTime);

  public abstract void OnCollisionEnter(GameTime gameTime, Collision collision, IBox opponent);
  public abstract void OnCollisionStay(GameTime gameTime, Collision collision, IBox opponent);
  public abstract void OnCollisionExit(GameTime gameTime, Collision collision, IBox opponent);
  public abstract bool BelongsTo(Def.Category category);

  public virtual void DrawBox(GameTime gameTime)
  {
    if (Shape is ShapeRectangle rect)
    {
      if (rect.Rectangle.Width == 0 || rect.Rectangle.Height == 0)
      {
        return;
      }
      Core.Sb.DrawPolygon(Vector2.Zero, new Polygon(rect.GetTransformedRectangleVertices(Position, Scale, Rotation)), Color.Red, 1);
    }
    else if (Shape is ShapeCircle circle)
    {
      if (circle.Circle.Radius == 0)
      {
        return;
      }
      var circleAbs = circle.GetTransformedCircle(Position, Scale);
      Core.Sb.DrawCircle(circleAbs, 16, Color.Red, 1);
    }
  }
}