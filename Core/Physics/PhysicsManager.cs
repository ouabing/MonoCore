using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using nkast.Aether.Physics2D.Dynamics;

namespace G;

public class PhysicsManager
{
  public delegate float RayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction);
  public World World { get; private set; }
  public bool IsPaused { get; private set; }
  private List<Body> BodiesToRemove { get; } = [];
  public void CreateWorld()
  {
    World = new World(Def.Physics.Gravity.ToAetherVector2());
  }

  public void Pause()
  {
    IsPaused = true;
  }

  public void Resume()
  {
    IsPaused = false;
  }

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
      return callback(fixture, point.ToVector2(), normal.ToVector2(), fraction);
    }, point1.ToAetherVector2(), point2.ToAetherVector2());
  }

  public bool RayCast(
    Vector2 point1,
    Vector2 point2,
    out Fixture? fixture,
    out Vector2 point,
    out Vector2 normal,
    out float fraction,
    Def.Physics.Cat category = Def.Physics.Cat.Default
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
          pointResult = hitpoint.ToVector2();
          normalResult = normal.ToVector2();
          fractionResult = fraction;

          // Clip the ray and try to find closer point
          return fraction;
        }
      }
      return 1;
    }, point1.ToAetherVector2(), point2.ToAetherVector2());

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
    Def.Physics.Cat category = Def.Physics.Cat.Default
  )
  {
    return RayCast(point1, point2, out fixture, out point, out _, out _, category);
  }

  public bool RayCast(
    Vector2 point1,
    Vector2 point2,
    out Fixture? fixture,
    Def.Physics.Cat category = Def.Physics.Cat.Default
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
    BodiesToRemove.ForEach(body => Remove(body));
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
}