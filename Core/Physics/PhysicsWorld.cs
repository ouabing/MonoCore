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
  }

  public void PostUpdate(GameTime gameTime)
  {
    Collisions = [];
  }

  private void CheckCollision(IBox a, IBox b)
  {
    if (!a.BoxAbs.Intersects(b.BoxAbs))
    {
      return;
    }

    var collision = new Collision(a, b);
    Collisions.Add(collision);
    a.OnCollision(collision, b);
    b.OnCollision(collision, a);
  }
}