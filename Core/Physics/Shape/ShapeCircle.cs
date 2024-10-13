using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public class ShapeCircle(CircleF circle) : BaseShape
{
  public override ShapeType Type => ShapeType.Circle;
  public CircleF Circle { get; set; } = circle;

  public CircleF GetTransformedCircle(Vector2 position, Vector2 scale)
  {
    float scaledRadius = Circle.Radius * scale.X;
    Vector2 transformedPosition = position + Circle.Position * scale;
    return new CircleF(transformedPosition, scaledRadius);
  }
}
