using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public class Container(Def.Container Name, int priority)
{
  public Def.Container Name { get; } = Name;
  public int Priority { get; } = priority;
  private readonly List<Component> components = [];

  public void Add(Component component)
  {
    component.LoadContent();
    components.Add(component);
  }

  public void Remove(Component component)
  {
    components.Remove(component);
  }

  public void Update(GameTime gameTime)
  {
    foreach (var component in components)
    {
      component.PreUpdate(gameTime);
      component.Update(gameTime);
      component.PostUpdate(gameTime);
    }

    Clear();
  }

  public void Clear()
  {
    components.RemoveAll(c => c.IsDead);
  }
}