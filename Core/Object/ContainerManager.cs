using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public class ContainerManager
{
  public List<Container> Containers { get; } = [];

  public ContainerManager()
  {
    foreach (Def.Container name in Enum.GetValues<Def.Container>())
    {
      CreateContainer(name);
    }
  }

  public void CreateContainer(Def.Container name)
  {
    if (Containers.Exists(c => c.Name == name))
    {
      throw new ArgumentException($"Container {name} already exists.");
    }
    Containers.Add(new Container(name, (int)name));
    Containers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
  }

  public void Pause(Def.Container name)
  {
    var container = Containers.Find(c => c.Name == name) ?? throw new ArgumentException($"Container {name} not found.");
    container.Pause = true;
  }

  public void Resume(Def.Container name)
  {
    var container = Containers.Find(c => c.Name == name) ?? throw new ArgumentException($"Container {name} not found.");
    container.Pause = false;
  }

  public void Add(Def.Container name, Component component)
  {
    var container = Containers.Find(c => c.Name == name) ?? throw new ArgumentException($"Container {name} not found.");

    container.Add(component);
  }

  public void Remove(Def.Container name, Component component)
  {
    var container = Containers.Find(c => c.Name == name) ?? throw new ArgumentException($"Container {name} not found.");

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