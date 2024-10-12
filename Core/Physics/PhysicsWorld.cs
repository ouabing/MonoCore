using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public class DuplicateBoxException(string message) : System.Exception(message)
{
}

public class PhysicsWorld
{
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
    return IndexedCollisions[box] ?? [];
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
    var circleAAbs = new CircleF(shapeA!.Circle.Center + a.Position, shapeA.Circle.Radius);
    var circleBAbs = new CircleF(shapeB!.Circle.Center + a.Position, shapeB.Circle.Radius);

    return Vector2.Distance(circleAAbs.Center, circleBAbs.Center) <= circleAAbs.Radius + circleBAbs.Radius;
  }

  private static bool CheckCollisionCircleRectangle(IBox a, IBox b)
  {
    var shapeA = a.Shape as ShapeCircle;
    var shapeB = b.Shape as ShapeRectangle;

    var circleAAbs = new CircleF(shapeA!.Circle.Center + a.Position, shapeA.Circle.Radius);
    var rectBAbs = new RectangleF(shapeB!.Rectangle.X + b.Position.X, shapeB.Rectangle.Y + b.Position.Y, shapeB.Rectangle.Width, shapeB.Rectangle.Height);

    float closestX = Math.Max(
      rectBAbs.Left,
      Math.Min(circleAAbs.Center.X, rectBAbs.Right)
    );
    float closestY = Math.Max(rectBAbs.Top, Math.Min(circleAAbs.Center.Y, rectBAbs.Bottom));

    float distanceX = circleAAbs.Center.X - closestX;
    float distanceY = circleAAbs.Center.Y - closestY;
    float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

    return distanceSquared <= (circleAAbs.Radius * circleAAbs.Radius);
  }

  private static bool CheckCollisionRectangleRectangle(IBox a, IBox b)
  {
    var shapeA = a.Shape as ShapeRectangle;
    var shapeAAbs = new RectangleF(shapeA!.Rectangle.X + a.Position.X, shapeA.Rectangle.Y + a.Position.Y, shapeA.Rectangle.Width, shapeA.Rectangle.Height);
    var shapeB = b.Shape as ShapeRectangle;
    var shapeBAbs = new RectangleF(shapeB!.Rectangle.X + b.Position.X, shapeB.Rectangle.Y + b.Position.Y, shapeB.Rectangle.Width, shapeB.Rectangle.Height);
    return shapeAAbs.Intersects(shapeBAbs);
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

  private void ClearDead()
  {
    boxes.RemoveAll(c => c.IsDead);
  }
}