
using MonoGame.Extended;

namespace G;

public class ShapeRectangle(RectangleF rect) : BaseShape
{
  public override ShapeType Type => ShapeType.Rectangle;
  public RectangleF Rectangle { get; set; } = rect;
}