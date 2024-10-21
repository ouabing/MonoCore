using System;
using Microsoft.Xna.Framework;

namespace G;

public enum CollisionState
{
  Enter,
  Stay,
  Exit
}

public class Collision(IBox a, IBox b, Vector2 correctionVector)
{
  public IBox A { get; } = a;
  public IBox B { get; } = b;
  public CollisionState State { get; set; } = CollisionState.Enter;
  public Vector2 CorrectionVector { get; set; } = correctionVector;

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

  public Vector2 GetCorrectionVector(IBox self)
  {
    if (self == A)
    {
      return CorrectionVector;
    }
    else if (self == B)
    {
      return -CorrectionVector;
    }

    throw new ArgumentException("The box is not in the collision.");
  }
}