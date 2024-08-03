using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public abstract class Cursor : IBox
{
  public bool IsDead { get; set; }
  public Vector2 Position => Core.Input.CursorPositionInWorld;
  public Vector2 Origin { get; } = Vector2.Zero;

  public Vector2 PreviousPosition => Core.Input.PreviousCursorPositionInWorld;
  public Vector2 Velocity => Vector2.Zero;
  public RectangleF Box => new(Position.X, Position.Y, 1, 1);

  public abstract void OnCollision(Collision collision, IBox opponent);

  public abstract void UpdatePhysics(GameTime gameTime);
}