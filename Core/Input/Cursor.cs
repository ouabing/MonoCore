using System.Collections.Generic;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision;
using nkast.Aether.Physics2D.Dynamics;

namespace G;

public abstract class Cursor : Component
{
  public override Vector2 PreviousPosition => Core.Input.PreviousCursorWorldPosition;

  public static List<Component> QueryAABBs(Category category = Category.All, bool fixedCamera = true, Def.Physics.World world = Def.Physics.World.Main)
  {
    var position = fixedCamera ? Core.Input.CursorScreenPosition : Core.Input.CursorWorldPosition;
    var upperBound = position.ToMeterVector2();
    AABB aabb = new()
    {
      LowerBound = upperBound + new Vector2(1f, 1f).ToMeterVector2(),
      UpperBound = upperBound
    };
    return Core.Physics.QueryAABBs(aabb, category);
  }
}