using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace G;

public enum OriginType { TopLeft, Center, Custom }

public abstract class Component : IBox
{
  public virtual BaseShape? Shape { get; set; }
  public virtual Vector2 Position { get; set; }
  // The offset is use to draw the object at a different position than the actual position.
  // Will not affect the collision box.
  public virtual Vector2 Offset { get; set; }
  public virtual Vector2 PreviousPosition { get; set; }
  public virtual Vector2 Velocity { get; set; }
  public Vector2 Size { get; set; }
  public float Rotation { get; set; }
  public Vector2 Scale { get; set; } = Vector2.One;
  public bool IsDead { get; set; }
  public float Opacity { get; set; } = 1f;
  public OriginType OriginType { get; set; } = OriginType.Center;
  public Effect? CurrentFX { get; protected set; }

  // Set this to true if you want to draw primitives
  public bool EnablePrimitiveBatch { get; protected set; }
  public bool EnableSpriteBatch { get; protected set; } = true;
  public int Z { get; set; }

  public Texture2D? Texture { get; protected set; }

  private bool _enableHitFX;
  public bool EnableHitFX
  {
    get { return _enableHitFX; }
    protected set
    {
      _enableHitFX = value;
      if (value)
      {
        HitFX = new();
        Core.Animation.AddHitable(this);
      }
      else
      {
        HitFX = null;
        Core.Animation.RemoveHitable(this);
      }
    }
  }

  private bool _enableShake;
  public bool EnableShake
  {
    get { return _enableShake; }
    protected set
    {
      _enableShake = value;
      if (value)
      {
        Shaker = new();
        Core.Animation.AddShakable(this);
      }
      else
      {
        Shaker = null;
        Core.Animation.RemoveShakable(this);
      }
    }
  }
  public Shaker? Shaker { get; set; }
  public HitFX? HitFX { get; protected set; }

  private bool contentLoaded;
  private Vector2 customOrigin;

  public Vector2 Origin
  {
    get
    {
      return OriginType switch
      {
        OriginType.TopLeft => Vector2.Zero,
        OriginType.Center => Size / 2f,
        OriginType.Custom => customOrigin,
        _ => throw new NotImplementedException(),
      };
    }
    set
    {
      if (OriginType == OriginType.Custom)
      {
        customOrigin = value;
      }
      else
      {
        throw new InvalidOperationException("OriginType must be set to Custom to set a custom origin.");
      }
    }
  }

  public Vector2 TopLeft => Position - Origin;
  public Vector2 TopRight => Position + new Vector2(Size.X, 0) - Origin;
  public Vector2 BottomLeft => Position + new Vector2(0, Size.Y) - Origin;
  public Vector2 BottomRight => Position + Size - Origin;
  public Vector2 Center => Position + Size / 2f - Origin;
  public Vector2 BottomCenter => Position + new Vector2(Size.X / 2, Size.Y) - Origin;

  public Def.Category Category { get; set; } = Def.Category.Default;
  public Def.Category CollidesWith { get; set; } = Def.Category.All;


  public virtual void LoadContent()
  {
    if (contentLoaded)
    {
      return;
    }
    contentLoaded = true;
  }

  public abstract void Update(GameTime gameTime);
  public virtual void PostUpdate(GameTime gameTime)
  {
  }

  public virtual void Draw(GameTime gameTime)
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

  public virtual void OnCollisionEnter(GameTime gameTime, Collision collision, IBox opponent)
  {
  }

  public virtual void OnCollisionStay(GameTime gameTime, Collision collision, IBox opponent)
  {
  }

  public virtual void OnCollisionExit(GameTime gameTime, Collision collision, IBox opponent)
  {
  }

  public bool BelongsTo(Def.Category category)
  {
    return (Category & category) != Def.Category.None;
  }
}