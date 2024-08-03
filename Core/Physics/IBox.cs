using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public interface IBox
{
  public bool IsDead { get; set; }
  public abstract Vector2 Position { get; }
  public abstract Vector2 Origin { get; }
  public abstract Vector2 PreviousPosition { get; }
  public abstract Vector2 Velocity { get; }

  // Relative to the Position.
  public abstract RectangleF Box { get; }
  public RectangleF BoxAbs => new(Box.Position.X + Position.X, Box.Position.Y + Position.Y, Box.Width, Box.Height);

  public abstract void UpdatePhysics(GameTime gameTime);

  public abstract void OnCollision(Collision collision, IBox opponent);

  public virtual void DrawBox(GameTime gameTime)
  {
    Core.Sb.DrawRectangle(BoxAbs, Color.Red, 1);
  }
}