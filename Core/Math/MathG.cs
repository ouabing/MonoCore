using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public class MathG
{
  public static List<Vector2> Bezier(Vector2 P0, Vector2 P1, Vector2 P2)
  {
    var result = new List<Vector2>();
    for (float t = 0; t <= 1; t += 0.01f)
    {
      Vector2 point = (1 - t) * (1 - t) * P0 + 2 * (1 - t) * t * P1 + t * t * P2;
      result.Add(point);
    }
    return result;
  }
}