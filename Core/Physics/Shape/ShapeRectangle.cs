
using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public class ShapeRectangle(RectangleF rect) : BaseShape
{
  public override ShapeType Type => ShapeType.Rectangle;
  public RectangleF Rectangle { get; set; } = rect;

  public Vector2[] GetTransformedRectangleVertices(Vector2 position, Vector2 scale, float rotation)
  {
    Vector2 scaledSize = new(Rectangle.Width * scale.X, Rectangle.Height * scale.Y);

    Vector2 transformedPosition = position + Rectangle.Position * scale;

    Vector2 topLeft = transformedPosition;
    Vector2 topRight = transformedPosition + new Vector2(scaledSize.X, 0);
    Vector2 bottomLeft = transformedPosition + new Vector2(0, scaledSize.Y);
    Vector2 bottomRight = transformedPosition + new Vector2(scaledSize.X, scaledSize.Y);

    Vector2 rotateCenter = position;

    topLeft = RotatePoint(topLeft, rotateCenter, rotation);
    topRight = RotatePoint(topRight, rotateCenter, rotation);
    bottomLeft = RotatePoint(bottomLeft, rotateCenter, rotation);
    bottomRight = RotatePoint(bottomRight, rotateCenter, rotation);

    return [topLeft, topRight, bottomRight, bottomLeft];
  }

  private static Vector2 RotatePoint(Vector2 point, Vector2 rotateCenter, float rotation)
  {
    float cos = MathF.Cos(rotation);
    float sin = MathF.Sin(rotation);

    Vector2 translatedPoint = point - rotateCenter;

    Vector2 rotatedPoint = new Vector2(
        translatedPoint.X * cos - translatedPoint.Y * sin,
        translatedPoint.X * sin + translatedPoint.Y * cos
    );

    return rotatedPoint + rotateCenter;
  }
}