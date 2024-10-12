using MonoGame.Extended;

namespace G;

public class ShapeCircle(CircleF circle) : BaseShape
{
  public override ShapeType Type => ShapeType.Circle;
  public CircleF Circle { get; set; } = circle;
}
