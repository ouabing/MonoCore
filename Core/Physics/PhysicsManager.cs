using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public class PhysicsManager
{
  private PhysicsWorld world = new();

  public PhysicsManager()
  {
  }

  public void Pause()
  {
    world.Pause = true;
  }

  public void Resume()
  {
    world.Pause = false;
  }

  public void Add(IBox box)
  {
    world.Add(box);
  }

  public void Remove(IBox box)
  {
    world.Remove(box);
  }

  public bool Raycast(Vector2 origin, Vector2 direction, out IBox? hitbox, out Vector2 hitPoint, Def.Category collidesWith = Def.Category.All)
  {
    return world.Raycast(origin, direction, out hitbox, out hitPoint);
  }

  public List<Collision> GetCollisions(IBox box)
  {
    return world.GetCollisions(box);
  }

  /**
   * Smooth the correction vectors of the box against type T
   * There may be jumps if you apply
   */
  public Vector2 GetSmoothCorrectionVector<T>(IBox box)
  {
    return world.GetSmoothCorrectionVector<T>(box);
  }

  public void Update(GameTime gameTime)
  {
    world.Update(gameTime);
  }

  public void PostUpdate(GameTime gameTime)
  {
    world.PostUpdate(gameTime);
  }
}