using Microsoft.Xna.Framework;

namespace G;

public abstract class Cursor : Component
{
  public override Vector2 PreviousPosition => Core.Input.PreviousCursorWorldPosition;
}