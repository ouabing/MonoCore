using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public class DuplicateBoxException(string message) : System.Exception(message)
{
}

public class PhysicsWorld
{
  public bool Pause { get; set; }
  private readonly List<IBox> boxes = [];
  private List<Collision> Collisions { get; } = [];
  private List<Collision> LastCollisions { get; set; } = [];
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

    LastCollisions = new List<Collision>(Collisions);
    Collisions.Clear();
    IndexedCollisions.Clear();

    for (int i = 0; i < boxes.Count; i++)
    {
      for (int j = i + 1; j < boxes.Count; j++)
      {
        CheckCollision(gameTime, boxes[i], boxes[j]);
      }
    }

    foreach (var kv in IndexedCollisions)
    {
      var box = kv.Key;
      var collisions = kv.Value;

      foreach (var collision in collisions)
      {
        if (collision.State == CollisionState.Enter)
        {
          box.OnCollisionEnter(gameTime, collision, collision.GetOpponent(box));
        }
        else if (collision.State == CollisionState.Stay)
        {
          box.OnCollisionStay(gameTime, collision, collision.GetOpponent(box));
        }
      }
    }
    var exitedCollisions = LastCollisions.Except(Collisions).ToList();
    foreach (var collision in exitedCollisions)
    {
      collision.State = CollisionState.Exit;
      collision.A.OnCollisionExit(gameTime, collision, collision.GetOpponent(collision.A));
      collision.B.OnCollisionExit(gameTime, collision, collision.GetOpponent(collision.B));
    }

    LastCollisions.Clear();
    ClearDead();
  }

  public void PostUpdate(GameTime gameTime)
  {
    ClearDead();
  }

  public List<Collision> GetCollisions(IBox box)
  {
    return IndexedCollisions.GetValueOrDefault(box, []);
  }

  public List<Collision> GetCollisions<T>(IBox box)
  {
    var collisions = GetCollisions(box);
    return collisions.Where(c => c.A is T || c.B is T).ToList();
  }

  public Vector2 GetSmoothCorrectionVector<T>(IBox box)
  {
    var collisions = GetCollisions<T>(box);
    if (collisions.Count == 0)
    {
      return Vector2.Zero;
    }

    var correctionVector = Vector2.Zero;
    foreach (var collision in collisions)
    {
      correctionVector += collision.GetCorrectionVector(box);
    }
    return correctionVector / collisions.Count;
  }

  public bool Raycast(Vector2 origin, Vector2 direction, out IBox? hitbox, out Vector2 hitPoint, Def.Category collidesWith = Def.Category.All)
  {
    float closestDistance = float.MaxValue;
    hitbox = null;
    hitPoint = Vector2.Zero;

    direction = Vector2.Normalize(direction);

    foreach (var box in boxes)
    {
      if (box.Shape == null)
      {
        continue;
      }
      if ((box.Category & collidesWith) == Def.Category.None)
      {
        continue;
      }
      if (box.Shape.Type == ShapeType.Rectangle)
      {
        var rectVertices = (box.Shape as ShapeRectangle)!.GetTransformedRectangleVertices(box.Position, box.Scale, box.Rotation);
        if (CheckRayIntersectsRectangle(origin, direction, rectVertices, out Vector2 intersection))
        {
          float distance = Vector2.Distance(origin, intersection);
          if (distance < closestDistance)
          {
            closestDistance = distance;
            hitbox = box;
            hitPoint = intersection;
          }
        }
      }
      else if (box.Shape.Type == ShapeType.Circle)
      {
        var circle = (box.Shape as ShapeCircle)!.GetTransformedCircle(box.Position, box.Scale);
        if (CheckRayIntersectsCircle(origin, direction, circle, out Vector2 intersection))
        {
          float distance = Vector2.Distance(origin, intersection);
          if (distance < closestDistance)
          {
            closestDistance = distance;
            hitbox = box;
            hitPoint = intersection;
          }
        }
      }
    }

    return hitbox != null;
  }

  private void CheckCollision(GameTime gameTime, IBox a, IBox b)
  {
    if ((a.Category & b.CollidesWith) == Def.Category.None || (b.Category & a.CollidesWith) == Def.Category.None)
    {
      return;
    }
    if (a.Shape == null || b.Shape == null)
    {
      return;
    }
    var hasCollision = false;
    var minimumTranslationVector = Vector2.Zero;
    if (a.Shape.Type == ShapeType.Circle)
    {
      if (b.Shape.Type == ShapeType.Circle)
      {
        var circleA = (a.Shape as ShapeCircle)!.GetTransformedCircle(a.Position, a.Scale);
        var circleB = (b.Shape as ShapeCircle)!.GetTransformedCircle(b.Position, b.Scale);
        hasCollision = CheckCollisionCircleCircle(circleA, circleB, out minimumTranslationVector);
      }
      else if (b.Shape.Type == ShapeType.Rectangle)
      {
        var circleA = (a.Shape as ShapeCircle)!.GetTransformedCircle(a.Position, a.Scale);
        var rectBVertices = (b.Shape as ShapeRectangle)!.GetTransformedRectangleVertices(b.Position, b.Scale, b.Rotation);
        hasCollision = CheckCollisionCircleRectangle(circleA, rectBVertices, out minimumTranslationVector);
      }
    }
    else if (a.Shape.Type == ShapeType.Rectangle)
    {
      if (b.Shape.Type == ShapeType.Circle)
      {
        var rectAVertices = (a.Shape as ShapeRectangle)!.GetTransformedRectangleVertices(a.Position, a.Scale, a.Rotation);
        var circleB = (b.Shape as ShapeCircle)!.GetTransformedCircle(b.Position, b.Scale);
        hasCollision = CheckCollisionCircleRectangle(circleB, rectAVertices, out Vector2 correctionVector);
        if (hasCollision)
        {
          minimumTranslationVector = -correctionVector;
        }
      }
      else if (b.Shape.Type == ShapeType.Rectangle)
      {
        var rectAVertices = (a.Shape as ShapeRectangle)!.GetTransformedRectangleVertices(a.Position, a.Scale, a.Rotation);
        var rectBVertices = (b.Shape as ShapeRectangle)!.GetTransformedRectangleVertices(b.Position, b.Scale, b.Rotation);
        hasCollision = CheckCollisionRectangleRectangle(ref rectBVertices, ref rectAVertices, out minimumTranslationVector);
      }
    }

    if (!hasCollision)
    {
      return;
    }

    var collision = new Collision(a, b, minimumTranslationVector);
    SaveCollision(collision);
  }

  private static bool CheckCollisionCircleCircle(CircleF circleA, CircleF circleB, out Vector2 minimumTranslationVector)
  {
    minimumTranslationVector = Vector2.Zero;
    if (Vector2.Distance(circleA.Center, circleB.Center) <= circleA.Radius + circleB.Radius)
    {
      minimumTranslationVector = Vector2.Normalize(circleA.Center - circleB.Center) * (circleA.Radius + circleB.Radius - Vector2.Distance(circleA.Center, circleB.Center));
      return true;
    }
    else
    {
      return false;
    }
  }

  private static bool CheckCollisionCircleRectangle(CircleF circleA, Vector2[] rectBVertices, out Vector2 minimumTranslationVector)
  {
    minimumTranslationVector = Vector2.Zero;

    if (IsPointInPolygon(circleA.Center, rectBVertices))
    {
      return true;
    }


    // check if any of the rectangle's edges intersect with the circle
    for (int i = 0; i < rectBVertices.Length; i++)
    {
      Vector2 start = rectBVertices[i];
      Vector2 end = rectBVertices[(i + 1) % rectBVertices.Length];

      // calculate the closest point on the segment to the circle's center
      Vector2 closestPoint = GetClosestPointOnSegment(circleA.Center, start, end);

      // calculate the distance between the closest point and the circle's center
      float distanceSquared = Vector2.DistanceSquared(circleA.Center, closestPoint);

      // if the distance is less than the circle's radius, they intersect
      if (distanceSquared <= circleA.Radius * circleA.Radius)
      {
        var correctVector = Vector2.Normalize(circleA.Center - closestPoint) * (circleA.Radius - Vector2.Distance(circleA.Center, closestPoint));
        if (minimumTranslationVector == Vector2.Zero || correctVector.LengthSquared() < minimumTranslationVector.LengthSquared())
        {
          minimumTranslationVector = correctVector;
        }
      }
    }
    if (minimumTranslationVector != Vector2.Zero)
    {
      return true;
    }

    // if none of the edges intersected, check if the circle is inside the rectangle
    return false;

  }

  private static bool CheckCollisionRectangleRectangle(ref Vector2[] verticesA, ref Vector2[] verticesB, out Vector2 minimumTranslationVector)
  {
    // For rotated rectangles, we need to use SAT
    return CheckSATCollision(verticesA, verticesB, out minimumTranslationVector);
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
    t = Math.Clamp(t, 0f, 1f);

    // return the closest point on the line
    return start + t * lineDirection;
  }

  private void SaveCollision(Collision collision)
  {
    var foundCollision = LastCollisions.Find(c => (c.A == collision.A && c.B == collision.B) || (c.A == collision.B && c.B == collision.A));
    if (foundCollision != null)
    {
      foundCollision.State = CollisionState.Stay;
      foundCollision.CorrectionVector = collision.CorrectionVector;
    }
    var finalCollision = foundCollision ?? collision;
    Collisions.Add(finalCollision);
    if (!IndexedCollisions.TryGetValue(finalCollision.A, out List<Collision>? valueA))
    {
      valueA = [];
      IndexedCollisions[finalCollision.A] = valueA;
    }
    valueA!.Add(finalCollision);

    if (!IndexedCollisions.TryGetValue(finalCollision.B, out List<Collision>? valueB))
    {
      valueB = [];
      IndexedCollisions[finalCollision.B] = valueB;
    }
    valueB!.Add(finalCollision);
  }

  #region SAT detection

  private static bool CheckSATCollision(Vector2[] verticesA, Vector2[] verticesB, out Vector2 minimumTranslationVector)
  {
    var axesA = GetAxes(verticesA);
    var axesB = GetAxes(verticesB);

    var overlapDepth = float.MaxValue;
    var overlapAxis = Vector2.Zero;
    minimumTranslationVector = Vector2.Zero;

    // Check for overlap on all axes
    foreach (var axis in axesA.Concat(axesB))
    {
      var (minA, maxA) = ProjectVerticesOnAxis(axis, verticesA);
      var (minB, maxB) = ProjectVerticesOnAxis(axis, verticesB);
      if (maxA <= minB || maxB <= minA)
      {
        return false;
      }
      var currentOverlapDepth = Math.Min(maxA, maxB) - Math.Max(minA, minB);

      if (currentOverlapDepth < overlapDepth)
      {
        overlapDepth = currentOverlapDepth;
        overlapAxis = axis;
      }
    }

    if (overlapDepth < 1e-3f)
    {
      return false;
    }

    // Calculate the center points of the two shapes
    var centerA = GetCentroid(verticesA);
    var centerB = GetCentroid(verticesB);

    // Determine the direction of the minimum translation vector
    var direction = centerB - centerA;

    // If the dot product is negative, it means the overlapAxis is pointing from B to A, so reverse the direction
    if (Vector2.Dot(direction, overlapAxis) < 0)
    {
      overlapAxis = -overlapAxis;
    }

    minimumTranslationVector = overlapAxis * overlapDepth;

    // all axes had overlap, so the rectangles are colliding
    return true;
  }

  private static Vector2 GetCentroid(Vector2[] vertices)
  {
    float sumX = 0;
    float sumY = 0;
    int count = vertices.Length;

    foreach (var vertex in vertices)
    {
      sumX += vertex.X;
      sumY += vertex.Y;
    }

    float centroidX = sumX / count;
    float centroidY = sumY / count;

    return new Vector2(centroidX, centroidY);
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

  #region Raycasting
  private static bool CheckRayIntersectsCircle(Vector2 origin, Vector2 direction, CircleF circle, out Vector2 intersection)
  {
    intersection = Vector2.Zero;

    Vector2 toCircle = circle.Center - origin;

    Vector2 rayDirNormalized = Vector2.Normalize(direction);

    float projectionLength = Vector2.Dot(toCircle, rayDirNormalized);

    Vector2 projectionPoint = origin + projectionLength * rayDirNormalized;

    float distanceToCenter = Vector2.Distance(projectionPoint, circle.Center);

    if (distanceToCenter > circle.Radius)
    {
      return false;
    }

    float offset = MathF.Sqrt(circle.Radius * circle.Radius - distanceToCenter * distanceToCenter);

    float t1 = projectionLength - offset;
    float t2 = projectionLength + offset;

    float tMin = Math.Min(t1, t2);
    float tMax = Math.Max(t1, t2);

    if (tMax < 0)
    {
      return false;
    }

    if (tMin >= 0)
    {
      intersection = origin + tMin * rayDirNormalized;
      return true;
    }

    intersection = origin + tMax * rayDirNormalized;
    return true;
  }
  private static bool CheckRayIntersectsRectangle(Vector2 origin, Vector2 direction, Vector2[] rectVertices, out Vector2 intersection)
  {
    intersection = Vector2.Zero;
    float tMin = float.MinValue;
    float tMax = float.MaxValue;

    var axes = GetAxes(rectVertices);

    axes = axes.Append(direction).ToArray();

    foreach (var axis in axes)
    {
      var (minA, maxA) = ProjectVerticesOnAxis(axis, rectVertices);
      float originProj = Vector2.Dot(origin, axis);
      float directionProj = Vector2.Dot(direction, axis);

      if (Math.Abs(directionProj) < 1e-5)
      {
        if (originProj < minA || originProj > maxA)
        {
          return false;
        }
      }
      else
      {
        float t1 = (minA - originProj) / directionProj;
        float t2 = (maxA - originProj) / directionProj;

        if (t1 > t2)
        {
          (t1, t2) = (t2, t1);
        }

        tMin = Math.Max(tMin, t1);
        tMax = Math.Min(tMax, t2);

        if (tMin > tMax)
        {
          return false;
        }
      }
    }

    if (tMin >= 0)
    {
      intersection = origin + tMin * direction;
      return true;
    }

    return false;
  }
  #endregion

  private void ClearDead()
  {
    boxes.RemoveAll(c => c.IsDead);
  }

}