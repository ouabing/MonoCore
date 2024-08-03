using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public class DuplicateBoxException(string message) : System.Exception(message)
{
}

public class PhysicsWorld
{
  private readonly List<IBox> boxes = [];
  public List<Collision> Collisions { get; private set; } = [];
  public Dictionary<IBox, List<Collision>> IndexedCollisions { get; private set; } = [];

  public void Add(IBox box)
  {
    if (boxes.Contains(box))
    {
      throw new DuplicateBoxException("Box already exists in the world.");
    }
    boxes.Add(box);
  }

  public void Remove(IBox box)
  {
    boxes.Remove(box);
  }

  public void Update(GameTime gameTime)
  {
    foreach (var box in boxes)
    {
      box.UpdatePhysics(gameTime);
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
    if (!a.BoxAbs.Intersects(b.BoxAbs))
    {
      return;
    }

    var collision = new Collision(a, b);
    SaveCollision(collision);
    a.OnCollision(collision, b);
    b.OnCollision(collision, a);
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