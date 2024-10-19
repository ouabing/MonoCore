using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public class PhysicsManager
{
  private Dictionary<Def.PhysicsWorld, PhysicsWorld> PhysicsWorlds { get; } = [];

  public PhysicsManager()
  {
    foreach (Def.PhysicsWorld world in Enum.GetValues<Def.PhysicsWorld>())
    {
      CreateWorld(world);
    }
  }

  public PhysicsWorld GetWorld(Def.PhysicsWorld world)
  {
    if (!PhysicsWorlds.TryGetValue(world, out PhysicsWorld? value))
    {
      throw new KeyNotFoundException($"Physics world {world} not found.");
    }
    return value;
  }

  public void Pause(Def.PhysicsWorld world)
  {
    GetWorld(world).Pause = true;
  }

  public void Resume(Def.PhysicsWorld world)
  {
    GetWorld(world).Pause = false;
  }

  public void Add(Def.PhysicsWorld world, IBox box)
  {
    GetWorld(world).Add(box);
  }

  public void Remove(Def.PhysicsWorld world, IBox box)
  {
    GetWorld(world).Remove(box);
  }

  public bool Raycast(Def.PhysicsWorld world, Vector2 origin, Vector2 direction, out IBox? hitbox, out Vector2 hitPoint)
  {
    return GetWorld(world).Raycast(origin, direction, out hitbox, out hitPoint);
  }

  public List<Collision> GetCollisions(Def.PhysicsWorld world, IBox box)
  {
    return GetWorld(world).GetCollisions(box);
  }

  /**
   * Smooth the correction vectors of the box against type T
   * There may be jumps if you apply
   */
  public Vector2 GetSmoothCorrectionVector<T>(Def.PhysicsWorld world, IBox box)
  {
    return GetWorld(world).GetSmoothCorrectionVector<T>(box);
  }

  public void CreateWorld(Def.PhysicsWorld world)
  {
    if (PhysicsWorlds.ContainsKey(world))
    {
      throw new ArgumentException($"Physics world {world} already exists.");
    }
    PhysicsWorlds[world] = new PhysicsWorld();
  }

  public void Update(GameTime gameTime)
  {
    foreach (var physicsWorld in PhysicsWorlds.Values)
    {
      physicsWorld.Update(gameTime);
    }
  }

  public void PostUpdate(GameTime gameTime)
  {
    foreach (var physicsWorld in PhysicsWorlds.Values)
    {
      physicsWorld.PostUpdate(gameTime);
    }
  }
}