using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Collision;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;


namespace G;

public enum OriginType { TopLeft, Center, Custom }

public abstract class Component
{
  public virtual Body Body { get; set; }
  private Vector2 _position;
  public virtual Vector2 Position
  {
    get
    {
      return _position;
    }
    set
    {
      if (Body != null)
      {
        Body.Position = value.ToMeterVector2();
      }
      _position = value;
    }
  }
  // The offset is use to draw the object at a different position than the actual position.
  // Will not affect the collision box.
  public virtual Vector2 Offset { get; set; }
  public virtual Vector2 PreviousPosition { get; set; }
  public Vector2 Size { get; set; }
  private float _rotation;
  public float Rotation
  {
    get
    {
      return _rotation;
    }
    set
    {
      if (Body != null)
      {
        Body.Rotation = value;
      }
      _rotation = value;
    }
  }

  public Category Categories
  {
    get
    {
      if (Body == null)
      {
        return Category.None;
      }
      return Body.FixtureList[0].CollisionCategories;
    }
    set
    {
      if (Body != null)
      {
        return;
      }
      foreach (var fixture in Body!.FixtureList)
      {
        fixture.CollisionCategories = value;
      }
    }
  }

  public Category CollidesWith
  {
    get
    {
      if (Body == null)
      {
        return Category.None;
      }
      return Body.FixtureList[0].CollidesWith;
    }
    set
    {
      if (Body == null)
      {
        return;
      }
      foreach (var fixture in Body!.FixtureList)
      {
        fixture.CollidesWith = value;
      }
    }
  }
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

  public Body CreateRectangleBody(
    BodyType bodyType,
    Vector2 center,
    float width,
    float height,
    float density = 1,
    bool isSensor = false,
    Category categories = Category.Cat1,
    Category collidesWith = Category.All
  )
  {
    if (Body != null)
    {
      Core.Physics.Remove(Body);
    }
    Body = Core.Physics.World.CreateBody(Position.ToMeterVector2(), Rotation, bodyType);
    var fixture = Body.CreateRectangle(
      width.ToMeters(),
      height.ToMeters(),
      density,
      center.ToMeterVector2()
    );
    fixture.Tag = this;
    Body.Tag = this;
    if (isSensor)
    {
      fixture.IsSensor = true;
    }
    fixture.CollidesWith = collidesWith;
    fixture.CollisionCategories = categories;
    return Body;
  }

  public Body CreateCircleBody(
    BodyType bodyType,
    Vector2 center,
    float radius,
    float density = 1,
    bool isSensor = false,
    Category categories = Category.Cat1,
    Category collidesWith = Category.All
  )
  {
    if (Body != null)
    {
      Core.Physics.Remove(Body);
    }
    Body = Core.Physics.World.CreateBody(Position.ToMeterVector2(), Rotation, bodyType);
    var fixture = Body.CreateCircle(
      radius.ToMeters(),
      density,
      center.ToMeterVector2()
    );
    fixture.Tag = this;
    Body.Tag = this;
    if (isSensor)
    {
      fixture.IsSensor = true;
    }
    fixture.CollidesWith = collidesWith;
    fixture.CollisionCategories = categories;
    return Body;
  }

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
    if (Body != null)
    {
      _position = Body.Position.ToPixelVector2();
      _rotation = Body.Rotation;
    }
  }

  public virtual void Die()
  {
    if (Body != null)
    {
      Core.Physics.Remove(Body);
    }
    IsDead = true;
  }

  public virtual bool BeginContact(Fixture sender, Fixture other, Contact contact)
  {
    return true;
  }

  public virtual void EndContact(Fixture sender, Fixture other, Contact contact)
  {
  }

  public virtual void PreSolve(Fixture sender, Fixture other, Contact contact, ref Manifold oldManifold)
  {
  }

  public virtual void PostSolve(Fixture sender, Fixture other, Contact contact, ContactVelocityConstraint impulse)
  {
  }
}