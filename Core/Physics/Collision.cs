using System;

namespace G;

public class Collision(IBox a, IBox b)
{
  public IBox A { get; } = a;
  public IBox B { get; } = b;

  public IBox GetOpponent(IBox self)
  {
    if (self == A)
    {
      return B;
    }
    else if (self == B)
    {
      return A;
    }

    throw new ArgumentException("The box is not in the collision.");
  }

  // public Vector2 GetCorrectionVector(IBox self)
  // {
  //   RectangleF selfBox = self.BoxAbs;
  //   RectangleF opponentBox = GetOpponent(self).BoxAbs;

  //   float leftOverlap = opponentBox.Right - selfBox.Left;
  //   float rightOverlap = selfBox.Right - opponentBox.Left;
  //   float topOverlap = opponentBox.Bottom - selfBox.Top;
  //   float bottomOverlap = selfBox.Bottom - opponentBox.Top;

  //   float minOverlap = Math.Min(Math.Min(leftOverlap, rightOverlap), Math.Min(topOverlap, bottomOverlap));

  //   if (minOverlap == leftOverlap) return new Vector2(leftOverlap, 0);
  //   else if (minOverlap == rightOverlap) return new Vector2(-rightOverlap, 0);
  //   else if (minOverlap == topOverlap) return new Vector2(0, topOverlap);
  //   else if (minOverlap == bottomOverlap) return new Vector2(0, -bottomOverlap);

  //   return Vector2.Zero;
  // }
}