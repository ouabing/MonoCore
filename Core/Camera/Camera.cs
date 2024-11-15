using Microsoft.Xna.Framework;
using MonoGame.Extended;
namespace G;

public class Camera : Component
{
  private Vector2 _position;
  public override Vector2 Position
  {
    get => _position;
    set
    {
      PreviousPosition = _position;
      if (IsBoundsSet)
      {
        _position = Vector2.Clamp(
          value,
          new Vector2(Bounds!.Left + Core.Screen.Width / 2, Bounds.Top + Core.Screen.Height / 2),
          new Vector2(Bounds!.Right - Core.Screen.Width / 2, Bounds.Bottom - Core.Screen.Height / 2)
        );
      }
      else
      {
        _position = value;
      }
    }
  }
  public Vector2 Velocity { get; set; } = Vector2.Zero;

  public float Zoom { get; set; } = 1;
  private RectangleF bounds { get; set; }
  private bool IsBoundsSet;
  public RectangleF Bounds
  {
    get
    {
      return bounds;
    }
    set
    {
      IsBoundsSet = true;
      bounds = value;
    }
  }

  public override void LoadContent()
  {
    base.LoadContent();
    Reset();
    EnableShake = true;
  }

  public Vector2 ScreenToWorld(Vector2 screenPosition)
  {
    return Position - new Vector2(Core.Screen.Width, Core.Screen.Height) * 0.5f + screenPosition;
  }

  public void Reset()
  {
    Position = new(0, 0);
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
           Matrix.CreateTranslation(new Vector3(Core.Screen.Width * 0.5f, Core.Screen.Height * 0.5f, 0f));
  }

  public override void Update(GameTime gameTime)
  {
    Position += Velocity * gameTime.GetElapsedSeconds();

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
    Position = target.Center;
  }

}