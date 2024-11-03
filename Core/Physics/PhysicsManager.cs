using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using nkast.Aether.Physics2D.Collision;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;

namespace G;

public class PhysicsManager
{
  public delegate float RayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction);
  public World World { get; private set; }
  public bool IsPaused { get; private set; }
  public bool EnableDebug { get; set; }
  private List<Body> BodiesToRemove { get; } = [];
  public void CreateWorld()
  {
    World = new World(Def.Physics.Gravity.ToAetherVector2());
    World.ContactManager.BeginContact += BeginContact;
    World.ContactManager.EndContact += EndContact;
    World.ContactManager.PreSolve += PreSolve;
    World.ContactManager.PostSolve += PostSolve;
  }


  // Query simpoe AABB, if you don't want to use the entire physics engine,
  // you can simply use this function to check AABB collision by hand.
  // This method only returns the first overlapped AABB
  //
  // Note: by the word AABB, the result is not precise if you have rotation on your body
  public Component? QueryFirstAABB(Component component, Category category = Category.All)
  {
    var transform = component.Body.GetTransform();
    component.Body.FixtureList[0].Shape.ComputeAABB(out AABB aabb, ref transform, 0);
    Component? result = null;
    World.QueryAABB(fixture =>
    {
      if (fixture.Tag is Component other)
      {
        if (other == component)
        {
          return true;
        }
        if (other.Categories.ContainsAny(category))
        {
          result = other;
          return false;
        }
      }
      return true;
    }, ref aabb);
    return result;
  }

  // Query simpoe AABB, if you don't want to use the entire physics engine,
  // you can simply use this function to check AABB collision by hand.
  // This method returns all the overlapped AABBs
  //
  // Note: by the word AABB, the result is not precise if you have rotation on your body
  public List<Component> QueryAABBs(Component component, Category category = Category.All)
  {
    var body = component.Body;
    if (body == null)
    {
      return [];
    }
    var transform = body.GetTransform();
    var fixture = body.FixtureList[0];
    body.FixtureList[0].Shape.ComputeAABB(out AABB aabb, ref transform, 0);
    var result = new List<Component>();
    World.QueryAABB(fixture =>
    {
      if (fixture.Tag is Component other)
      {
        if (other == component)
        {
          return true;
        }
        if (other.Categories.ContainsAny(category))
        {
          result.Add(other);
        }
      }
      return true;
    }, ref aabb);

    return result;
  }

  public List<Component> QueryAABBs(AABB aabb, Category category = Category.All, Component? ignoreComponent = null)
  {
    var result = new List<Component>();
    World.QueryAABB(fixture =>
    {
      if (fixture.Tag is Component other)
      {
        if (other == ignoreComponent)
        {
          return true;
        }
        if (other.Categories.ContainsAny(category))
        {
          result.Add(other);
        }
      }
      return true;
    }, aabb);
    return result;
  }

  public void Pause()
  {
    IsPaused = true;
  }

  public void Resume()
  {
    IsPaused = false;
  }

  // Remove a body from the world
  // If the world is locked, remove after the world step
  public void Remove(Body body)
  {
    if (World.IsLocked)
    {
      BodiesToRemove.Add(body);
      return;
    }
    World.Remove(body);
  }


  // Summary:
  //     Ray-cast the world for all fixtures in the path of the ray. Your callback controls
  //     whether you get the closest point, any point, or n-points. The ray-cast ignores
  //     shapes that contain the starting point. Inside the callback:
  //     return -1: ignore this fixture and continue
  //     return 0: terminate the ray cast
  //     return fraction: clip the ray to this point
  //     return 1: don't clip the ray and continue
  public void RayCast(RayCastCallback callback, Vector2 point1, Vector2 point2)
  {
    World.RayCast((fixture, point, normal, fraction) =>
    {
      return callback(fixture, point.ToPixelVector2(), normal.ToPixelVector2(), fraction);
    }, point1.ToMeterVector2(), point2.ToMeterVector2());
  }

  public bool RayCast(
    Vector2 point1,
    Vector2 point2,
    out Fixture? fixture,
    out Vector2 point,
    out Vector2 normal,
    out float fraction,
    Category category = Category.All
  )
  {
    Fixture? fixtureResult = null;
    var pointResult = Vector2.Zero;
    var normalResult = Vector2.Zero;
    var fractionResult = float.MaxValue;
    World.RayCast((f, hitpoint, normal, fraction) =>
    {
      if (f.CollisionCategories.ContainsAny(category))
      {
        if (fraction < fractionResult)
        {
          fixtureResult = f;
          pointResult = hitpoint.ToPixelVector2();
          normalResult = normal.ToPixelVector2();
          fractionResult = fraction;

          // Clip the ray and try to find closer point
          return fraction;
        }
      }
      return 1;
    }, point1.ToMeterVector2(), point2.ToMeterVector2());

    fixture = fixtureResult;
    point = pointResult;
    normal = normalResult;
    fraction = fractionResult;
    return fixtureResult != null;
  }

  public bool RayCast(
    Vector2 point1,
    Vector2 point2,
    out Fixture? fixture,
    out Vector2 point,
    Category category = Category.All
  )
  {
    return RayCast(point1, point2, out fixture, out point, out _, out _, category);
  }

  public bool RayCast(
    Vector2 point1,
    Vector2 point2,
    out Fixture? fixture,
    Category category = Category.All
  )
  {
    return RayCast(point1, point2, out fixture, out _, out _, out _, category);
  }

  public void Update(GameTime gameTime)
  {
    if (IsPaused)
    {
      return;
    }
    World.Step(gameTime.GetElapsedSeconds());
    BodiesToRemove.ForEach(Remove);
    BodiesToRemove.Clear();
  }

  public List<(Component?, Fixture)> DebugFixtures()
  {
    return World.BodyList.SelectMany(body =>
    {
      return body.FixtureList.Select(fixture =>
      {
        return (fixture.Tag as Component, fixture);
      });
    }).ToList();
  }

  private bool BeginContact(Contact contact)
  {
    if (EnableDebug)
    {
      Debug.WriteLine("BeginContact: " + contact.FixtureA.Tag + " " + contact.FixtureB.Tag);
    }
    if (contact.FixtureA.Tag is Component a && contact.FixtureB.Tag is Component b)
    {
      if (a.IsDead || b.IsDead)
      {
        return false;
      }

      return a.BeginContact(contact.FixtureA, contact.FixtureB, contact) &&
             b.BeginContact(contact.FixtureB, contact.FixtureA, contact);
    }
    return false;
  }

  private void EndContact(Contact contact)
  {
    if (EnableDebug)
    {
      Debug.WriteLine("EndContact: " + contact.FixtureA.Tag + " " + contact.FixtureB.Tag);
    }
    if (contact.FixtureA.Tag is Component a && contact.FixtureB.Tag is Component b)
    {
      if (a.IsDead || b.IsDead)
      {
        return;
      }

      a.EndContact(contact.FixtureA, contact.FixtureB, contact);
      b.EndContact(contact.FixtureB, contact.FixtureA, contact);
    }
  }

  private void PreSolve(Contact contact, ref Manifold oldManifold)
  {
    if (contact.FixtureA.Tag is Component a && contact.FixtureB.Tag is Component b)
    {
      if (a.IsDead || b.IsDead)
      {
        contact.Enabled = false;
        return;
      }
      a.PreSolve(contact.FixtureA, contact.FixtureB, contact, ref oldManifold);
      b.PreSolve(contact.FixtureB, contact.FixtureA, contact, ref oldManifold);
    }
  }

  private void PostSolve(Contact contact, ContactVelocityConstraint impulse)
  {
    if (contact.FixtureA.Tag is Component a && contact.FixtureB.Tag is Component b)
    {
      if (a.IsDead || b.IsDead)
      {
        return;
      }
      a.PostSolve(contact.FixtureA, contact.FixtureB, contact, impulse);
      b.PostSolve(contact.FixtureB, contact.FixtureA, contact, impulse);
    }
  }

}