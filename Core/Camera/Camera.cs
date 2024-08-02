using Microsoft.Xna.Framework;
using MonoGame.Extended;
namespace G;

public class Camera : IShakable
{
  public Vector2 Position { get; private set; }
  public float Zoom { get; set; } = 1;
  public Vector2 Velocity { get; private set; } = new(0);
  public RectangleF Bounds { get; set; }
  public float Rotation { get; set; }
  public Shaker? Shaker { get; set; }

  public void LoadContent()
  {
    Reset();
    Core.AddShakable(this);
  }

  public void SetPosition(Vector2 position)
  {
    Position = position;
  }

  public void Reset()
  {
    Position = new Vector2(Core.ScreenWidth / 2, Core.ScreenHeight / 2);
    Bounds = new RectangleF(Position.X, Position.Y, 0, 0);
  }

  public void MoveUp()
  {
    Velocity = new Vector2(Velocity.X, -Def.Camera.Velocity);
  }

  public void MoveDown()
  {
    Velocity = new Vector2(Velocity.X, Def.Camera.Velocity);
  }

  public void MoveLeft()
  {
    Velocity = new Vector2(-Def.Camera.Velocity, Velocity.Y);
  }

  public void MoveRight()
  {
    Velocity = new Vector2(Def.Camera.Velocity, Velocity.Y);
  }

  public Matrix GetMatrix()
  {
    return Matrix.CreateTranslation(new Vector3(-Position + (Shaker?.Amount ?? Vector2.Zero), 0f)) *
           Matrix.CreateRotationZ(Rotation) *
           Matrix.CreateScale(new Vector3(Zoom, Zoom, 1f)) *
           Matrix.CreateTranslation(new Vector3(Core.ScreenWidth * 0.5f, Core.ScreenHeight * 0.5f, 0f));
  }

  public void Update(GameTime gameTime)
  {
    Position += Velocity * gameTime.GetElapsedSeconds();
    // Position = Vector2.Clamp(Position, new Vector2(Bounds.Left, Bounds.Top), new Vector2(Bounds.Right, Bounds.Bottom));

    if (Velocity.Length() > 0)
    {
      Velocity *= Def.Camera.Deceleration;
      if (Velocity.Length() < Def.Camera.MinVelocity)
      {
        Velocity = Vector2.Zero;
      }
    }
  }

  public void Follow(Sprite target, GameTime gameTime)
  {
    Position = Vector2.Round(target.Center);
  }
}