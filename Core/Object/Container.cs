using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public class Container(Def.Container Name, int priority)
{
  public Def.Container Name { get; } = Name;
  public int Priority { get; } = priority;
  private List<Component> Components { get; } = [];

  public void Add(Component component)
  {
    component.LoadContent();
    Components.Add(component);
  }

  public void Remove(Component component)
  {
    Components.Remove(component);
  }

  public void Update(GameTime gameTime)
  {
    for (int i = 0; i < Components.Count; i++)
    {
      var component = Components[i];
      component.Update(gameTime);
    }
    ClearDead();
  }

  public void PostUpdate(GameTime gameTime)
  {

    for (int i = 0; i < Components.Count; i++)
    {
      var component = Components[i];
      component.PostUpdate(gameTime);
    }
    ClearDead();
  }

  public void ClearDead()
  {
    Components.RemoveAll(c => c.IsDead);
  }
}