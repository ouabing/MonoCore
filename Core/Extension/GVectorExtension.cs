using Microsoft.Xna.Framework;

namespace G;

public static class GVectorExtension
{
  public static Vector2 ToUnit(this Vector2 v)
  {
    Vector2 result = v / Core.PPU;
    result.Floor();
    return result;
  }

  public static Vector2 ToWorld(this Vector2 v)
  {
    return v * Core.PPU;
  }
}
