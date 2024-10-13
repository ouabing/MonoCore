using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace G;

public class DuplicateBoxException(string message) : System.Exception(message)
{
}

public class PhysicsWorld
{
  public bool Pause { get; set; }
  private readonly List<IBox> boxes = [];
  private List<Collision> Collisions { get; } = [];
  private Dictionary<IBox, List<Collision>> IndexedCollisions { get; } = [];

  public void Add(IBox box)
  {
    if (boxes.Contains(box))
    {
      throw new DuplicateBoxException($"Box {box} already exists in the world.");
    }
    boxes.Add(box);
  }

  public void Remove(IBox box)
  {
    boxes.Remove(box);
  }

  public void Update(GameTime gameTime)
  {
    if (Pause)
    {
      return;
    }
    for (int i = 0; i < boxes.Count; i++)
    {
      boxes[i].UpdatePhysics(gameTime);
    }

    for (int i = 0; i < boxes.Count; i++)
    {
      for (int j = i + 1; j < boxes.Count; j++)
      {
        CheckCollision(boxes[i], boxes[j]);
      }
    }
    ClearDead();
  }

  public void PostUpdate(GameTime gameTime)
  {
    Collisions.Clear();
    IndexedCollisions.Clear();
    ClearDead();
  }

  public List<Collision> GetCollisions(IBox box)
  {
    return IndexedCollisions.GetValueOrDefault(box, []);
  }

  private void CheckCollision(IBox a, IBox b)
  {
    var hasCollision = false;
    if (a.Shape.Type == ShapeType.Circle)
    {
      if (b.Shape.Type == ShapeType.Circle)
      {
        hasCollision = CheckCollisionCircleCircle(a, b);
      }
      else if (b.Shape.Type == ShapeType.Rectangle)
      {
        hasCollision = CheckCollisionCircleRectangle(a, b);
      }
    }
    else if (a.Shape.Type == ShapeType.Rectangle)
    {
      if (b.Shape.Type == ShapeType.Circle)
      {
        hasCollision = CheckCollisionCircleRectangle(b, a);
      }
      else if (b.Shape.Type == ShapeType.Rectangle)
      {
        hasCollision = CheckCollisionRectangleRectangle(b, a);
      }
    }

    if (!hasCollision)
    {
      return;
    }

    var collision = new Collision(a, b);
    SaveCollision(collision);
    a.OnCollision(collision, b);
    b.OnCollision(collision, a);
  }

  private static bool CheckCollisionCircleCircle(IBox a, IBox b)
  {
    var shapeA = a.Shape as ShapeCircle;
    var shapeB = b.Shape as ShapeCircle;
    var circleA = shapeA!.GetTransformedCircle(a.Position, a.Scale);
    var circleB = shapeB!.GetTransformedCircle(b.Position, b.Scale);

    return Vector2.Distance(circleA.Center, circleB.Center) <= circleA.Radius + circleB.Radius;
  }

  private static bool CheckCollisionCircleRectangle(IBox a, IBox b)
  {
    var shapeA = a.Shape as ShapeCircle;
    var shapeB = b.Shape as ShapeRectangle;

    var circleA = shapeA!.GetTransformedCircle(a.Position, a.Scale);
    var rectBVertices = shapeB!.GetTransformedRectangleVertices(b.Position, b.Scale, b.Rotation);

    if (IsPointInPolygon(circleA.Center, rectBVertices))
    {
      return true;
    }

    // check if any of the rectangle's edges intersect with the circle
    for (int i = 0; i < rectBVertices.Length; i++)
    {
      Vector2 start = rectBVertices[i];
      Vector2 end = rectBVertices[(i + 1) % rectBVertices.Length]; // 循环访问最后一条边

      // calculate the closest point on the segment to the circle's center
      Vector2 closestPoint = GetClosestPointOnSegment(circleA.Center, start, end);

      // calculate the distance between the closest point and the circle's center
      float distanceSquared = Vector2.DistanceSquared(circleA.Center, closestPoint);

      // if the distance is less than the circle's radius, they intersect
      if (distanceSquared <= circleA.Radius * circleA.Radius)
      {
        return true;
      }
    }

    // if none of the edges intersected, check if the circle is inside the rectangle
    return false;

  }

  private static bool CheckCollisionRectangleRectangle(IBox a, IBox b)
  {
    var shapeA = a.Shape as ShapeRectangle;
    var shapeB = b.Shape as ShapeRectangle;

    var verticesA = shapeA!.GetTransformedRectangleVertices(a.Position, a.Scale, a.Rotation);
    var verticesB = shapeB!.GetTransformedRectangleVertices(b.Position, b.Scale, b.Rotation);

    // For rotated rectangles, we need to use SAT
    return CheckSATCollision(verticesA, verticesB);
  }


  private static bool IsPointInPolygon(Vector2 point, Vector2[] vertices)
  {
    bool result = false;
    int j = vertices.Length - 1;
    for (int i = 0; i < vertices.Length; i++)
    {
      if ((vertices[i].Y > point.Y) != (vertices[j].Y > point.Y) &&
          (point.X < (vertices[j].X - vertices[i].X) * (point.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) + vertices[i].X))
      {
        result = !result;
      }
      j = i;
    }
    return result;
  }

  private static Vector2 GetClosestPointOnSegment(Vector2 point, Vector2 start, Vector2 end)
  {
    // calculate the direction of the line
    Vector2 lineDirection = end - start;
    float lineLengthSquared = lineDirection.LengthSquared();

    // if the line is just a point, return that point
    if (lineLengthSquared == 0f)
    {
      return start;
    }

    // calculate the t value for the closest point
    float t = Vector2.Dot(point - start, lineDirection) / lineLengthSquared;
    t = Math.Clamp(t, 0f, 1f); // 将 t 限制在 [0, 1] 之间

    // return the closest point on the line
    return start + t * lineDirection;
  }

  private void SaveCollision(Collision collision)
  {
    Collisions.Add(collision);
    if (!IndexedCollisions.TryGetValue(collision.A, out List<Collision>? valueA))
    {
      valueA = [];
      IndexedCollisions[collision.A] = valueA;
    }
    valueA!.Add(collision);

    if (!IndexedCollisions.TryGetValue(collision.B, out List<Collision>? valueB))
    {
      valueB = [];
      IndexedCollisions[collision.B] = valueB;
    }
    valueB!.Add(collision);
  }

  #region SAT detection

  private static bool CheckSATCollision(Vector2[] verticesA, Vector2[] verticesB)
  {
    var axesA = GetAxes(verticesA);
    var axesB = GetAxes(verticesB);

    // Check for overlap on all axes
    foreach (var axis in axesA.Concat(axesB))
    {
      if (!IsOverlappingOnAxis(axis, verticesA, verticesB))
      {
        // if there's a gap on any axis, we can exit early
        return false;
      }
    }

    // all axes had overlap, so the rectangles are colliding
    return true;
  }

  private static Vector2[] GetAxes(Vector2[] vertices)
  {
    var axes = new Vector2[vertices.Length];
    for (int i = 0; i < vertices.Length; i++)
    {
      var edge = vertices[(i + 1) % vertices.Length] - vertices[i];
      axes[i] = Vector2.Normalize(new Vector2(-edge.Y, edge.X));
    }
    return axes;
  }

  private static bool IsOverlappingOnAxis(Vector2 axis, Vector2[] verticesA, Vector2[] verticesB)
  {
    var (minA, maxA) = ProjectVerticesOnAxis(axis, verticesA);
    var (minB, maxB) = ProjectVerticesOnAxis(axis, verticesB);

    return maxA >= minB && maxB >= minA;
  }

  private static (float min, float max) ProjectVerticesOnAxis(Vector2 axis, Vector2[] vertices)
  {
    float min = Vector2.Dot(axis, vertices[0]);
    float max = min;
    for (int i = 1; i < vertices.Length; i++)
    {
      float projection = Vector2.Dot(axis, vertices[i]);
      if (projection < min) min = projection;
      if (projection > max) max = projection;
    }
    return (min, max);
  }
  #endregion

  private void ClearDead()
  {
    boxes.RemoveAll(c => c.IsDead);
  }

}