using Microsoft.Xna.Framework;
using MonoGame.Extended;
namespace G;

public class Camera : Component
{
  private Vector2 _position;
#pragma warning disable CA1822 // Mark members as static
  public Vector2 InitialPosition => new(Core.ScreenWidth * 0.5f, Core.ScreenHeight * 0.5f);
#pragma warning restore CA1822 // Mark members as static
  public override Vector2 Position
  {
    get => _position;
    set
    {
      PreviousPosition = _position;
      _position = value;
    }
  }

  public float Zoom { get; set; } = 1;
  public RectangleF Bounds { get; set; }

  public override void LoadContent()
  {
    base.LoadContent();
    Reset();
    EnableShake = true;
  }

  public void SetPosition(Vector2 position)
  {
    Position = position;
  }

  public Vector2 ScreenToWorld(Vector2 screenPosition)
  {
    return Position + screenPosition - InitialPosition;
  }

  public Vector2 PreviousScreenToWorld(Vector2 screenPosition)
  {
    return PreviousPosition + screenPosition - InitialPosition;
  }

  public void Reset()
  {
    Position = InitialPosition;
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

  public override void Update(GameTime gameTime)
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

  public void Follow(Component target, GameTime gameTime)
  {
    Position = Vector2.Round(target.Center);
  }
}