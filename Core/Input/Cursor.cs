using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public abstract class Cursor : IBox
{
  public bool IsDead { get; set; }
  public Vector2 Position => Core.Input.CursorWorldPosition;
  public Vector2 Origin { get; } = Vector2.Zero;
  public float Rotation { get; }
  public Vector2 Scale { get; } = new Vector2(1);

  public Vector2 PreviousPosition => Core.Input.PreviousCursorWorldPosition;
  public Vector2 Velocity => Vector2.Zero;
  public BaseShape Shape => new ShapeRectangle(new RectangleF(0, 0, 1, 1));

  public Def.Category CollisionCategory { get; set; } = Def.Category.Default;
  public Def.Category CollidesWith { get; set; } = Def.Category.All;

  public bool BelongsTo(Def.Category category)
  {
    return (CollisionCategory & category) != Def.Category.None;
  }

  public virtual void OnCollisionEnter(GameTime gameTime, Collision collision, IBox opponent)
  {
  }

  public virtual void OnCollisionExit(GameTime gameTime, Collision collision, IBox opponent)
  {
  }

  public virtual void OnCollisionStay(GameTime gameTime, Collision collision, IBox opponent)
  {
  }

  public abstract void UpdatePhysics(GameTime gameTime);
}