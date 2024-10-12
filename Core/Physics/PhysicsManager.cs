using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public class PhysicsManager
{
  private Dictionary<Def.PhysicsWorld, PhysicsWorld> PhysicsWorlds { get; } = [];

  public PhysicsManager()
  {
    foreach (Def.PhysicsWorld world in Enum.GetValues(typeof(Def.PhysicsWorld)))
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

  public void Add(Def.PhysicsWorld world, IBox box)
  {
    GetWorld(world).Add(box);
  }

  public void Remove(Def.PhysicsWorld world, IBox box)
  {
    GetWorld(world).Remove(box);
  }

  public List<Collision> GetCollisions(Def.PhysicsWorld world, IBox box)
  {
    return GetWorld(world).GetCollisions(box);
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