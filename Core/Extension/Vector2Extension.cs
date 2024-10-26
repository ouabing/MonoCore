using System;
using Microsoft.Xna.Framework;

namespace G;

public static class Vector2Extensions
{
  public static Vector2 Rounded(this Vector2 v)
  {
    return new((int)MathF.Round(v.X), (int)MathF.Round(v.Y));
  }
}