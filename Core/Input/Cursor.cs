using System.Collections.Generic;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision;
using nkast.Aether.Physics2D.Dynamics;

namespace G;

public abstract class Cursor : Component
{
  public override Vector2 PreviousPosition => Core.Input.PreviousCursorWorldPosition;

  public override Body CreateRectangleBody(
    BodyType bodyType,
    Vector2 center,
    float width,
    float height,
    float density = 1,
    bool isSensor = false,
    Category categories = Category.Cat1,
    Category collidesWith = Category.All,
    Def.Physics.World world = Def.Physics.World.Main
  )
  {
    throw new System.NotImplementedException("Cannot create a physics body for cursor, use Cursor.QueryAABBs instead");
  }

  public override Body CreateCircleBody(
    BodyType bodyType,
    Vector2 center,
    float radius,
    float density = 1,
    bool isSensor = false,
    Category categories = Category.Cat1,
    Category collidesWith = Category.All,
    Def.Physics.World world = Def.Physics.World.Main
  )
  {
    throw new System.NotImplementedException("Cannot create a physics body for cursor, use Cursor.QueryAABBs instead");
  }

  public static List<Component> QueryAABBs(Category category = Category.All, bool fixedCamera = true, Def.Physics.World world = Def.Physics.World.Main)
  {
    var position = fixedCamera ? Core.Input.CursorScreenPosition : Core.Input.CursorWorldPosition;
    var upperBound = position.ToMeterVector2();
    AABB aabb = new()
    {
      LowerBound = upperBound + new Vector2(1f, 1f).ToMeterVector2(),
      UpperBound = upperBound
    };
    return Core.Physics.QueryAABBs(aabb, category, world);
  }
}