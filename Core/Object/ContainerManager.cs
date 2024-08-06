using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public class ContainerManager
{
  public List<Container> Containers { get; } = [];

  public ContainerManager()
  {
    foreach (Def.Container name in Enum.GetValues(typeof(Def.Container)))
    {
      CreateContainer(name);
    }
  }

  public void CreateContainer(Def.Container Name)
  {
    if (Containers.Exists(c => c.Name == Name))
    {
      throw new ArgumentException($"Container {Name} already exists.");
    }
    Containers.Add(new Container(Name, (int)Name));
    Containers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
  }

  public void Add(Def.Container Name, Component component)
  {
    var container = Containers.Find(c => c.Name == Name) ?? throw new ArgumentException($"Container {Name} not found.");

    container.Add(component);
  }

  public void Remove(Def.Container Name, Component component)
  {
    var container = Containers.Find(c => c.Name == Name) ?? throw new ArgumentException($"Container {Name} not found.");

    container.Remove(component);
  }

  public void Update(GameTime gameTime)
  {
    foreach (var c in Containers)
    {
      c.Update(gameTime);
    }
  }

  public void PostUpdate(GameTime gameTime)
  {
    foreach (var c in Containers)
    {
      c.PostUpdate(gameTime);
    }
  }
}