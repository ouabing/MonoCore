using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;


namespace G;

public enum OriginType { TopLeft, Center }

public abstract class Component : IBox, IShakable
{
  public Vector2 Position { get; set; }
  public Vector2 PreviousPosition { get; set; }
  public Vector2 Velocity { get; set; }
  public Vector2 Size { get; set; }
  public virtual RectangleF Box { get; set; }
  public float Rotation { get; set; }
  public float Scale { get; set; } = 1f;
  public Shaker? Shaker { get; set; }
  public bool IsDead { get; set; }
  public OriginType OriginType { get; set; } = OriginType.Center;
  public Effect? CurrentFX { get; protected set; }

  // Set this to true if you want to draw primitives
  public bool EnableDrawPrimitives { get; protected set; }
  public int Z { get; set; }

  public Texture2D? Texture { get; protected set; }

  public Vector2 Origin
  {
    get
    {
      return OriginType switch
      {
        OriginType.TopLeft => Vector2.Zero,
        OriginType.Center => Size / 2f,
        _ => throw new NotImplementedException(),
      };
    }
  }

  public Vector2 TopLeft => OriginType switch
  {
    OriginType.TopLeft => Position,
    OriginType.Center => Position - Size / 2f,
    _ => throw new NotImplementedException()
  };

  public Vector2 TopRight => OriginType switch
  {
    OriginType.TopLeft => new Vector2(Position.X + Size.X, Position.Y),
    OriginType.Center => new Vector2(Position.X + Size.X / 2f, Position.Y - Size.Y / 2f),
    _ => throw new NotImplementedException()
  };

  public Vector2 BottomLeft => OriginType switch
  {
    OriginType.TopLeft => Position + new Vector2(0, Size.Y),
    OriginType.Center => new Vector2(Position.X - Size.X / 2f, Position.Y + Size.Y / 2f),
    _ => throw new NotImplementedException()
  };

  public Vector2 BottomRight => OriginType switch
  {
    OriginType.TopLeft => Position + Size,
    OriginType.Center => new Vector2(Position.X + Size.X / 2f, Position.Y + Size.Y / 2f),
    _ => throw new NotImplementedException()
  };

  public Vector2 Center => OriginType switch
  {
    OriginType.TopLeft => Position + Size / 2f,
    OriginType.Center => Position,
    _ => throw new NotImplementedException()
  };

  public Vector2 BottomCenter => OriginType switch
  {
    OriginType.TopLeft => Position + new Vector2(Size.X / 2, Size.Y),
    OriginType.Center => Position + new Vector2(0, Size.Y / 2),
    _ => throw new NotImplementedException()
  };

  public virtual void LoadContent()
  {
  }

  public virtual void PreUpdate(GameTime gameTime)
  {
  }

  public abstract void Update(GameTime gameTime);
  public virtual void PostUpdate(GameTime gameTime)
  {
  }

  public virtual void DrawPrimitives(GameTime gameTime)
  {
  }

  public virtual void Draw(GameTime gameTime)
  {
    if (Core.DebugComponent)
    {
      if (Box.Width > 0 && Box.Height > 0)
      {
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
        IBox box = this;
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
        box.DrawBox(gameTime);
      }
      var font = Core.Font(FontSize.Mini);
      font.DrawText(Core.Sb, $"({(int)Position.X},{(int)Position.Y})", TopLeft, Palette.White);
    }
  }

  public virtual void OnCollision(Collision collision, IBox opponent)
  {
  }

  public virtual void UpdatePhysics(GameTime gameTime)
  {
    PreviousPosition = Position;
    Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
  }

  public virtual void Die()
  {
    IsDead = true;
  }
}