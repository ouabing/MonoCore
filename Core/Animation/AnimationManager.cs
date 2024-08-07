using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public class AnimationManager
{
  private List<Component> Shakables { get; } = [];
  private List<Component> Hitables { get; } = [];

  public void AddShakable(Component component)
  {
    if (component.Shaker is null || !component.EnableShake)
    {
      throw new($"Component {component} is not shakable");
    }
    if (Shakables.Contains(component))
    {
      throw new($"Component {component} is already added in shakables");
    }
    Shakables.Add(component);
  }

  public void RemoveShakable(Component component)
  {
    Shakables.Remove(component);
  }

  public void AddHitable(Component hitable)
  {
    if (Hitables.Contains(hitable))
    {
      return;
    }
    Hitables.Add(hitable);
  }

  public void RemoveHitable(Component hitable)
  {
    Hitables.Remove(hitable);
  }

  public void Update(GameTime gameTime)
  {
    foreach (var shakable in Shakables)
    {
      if (shakable.Shaker is null)
      {
        throw new($"Shaker is not initialized in {shakable}");
      }
      shakable.Shaker.Update(gameTime);
    }
    foreach (var hitable in Hitables)
    {
      if (hitable.HitFX == null)
      {
        throw new($"HitFX is not initialized in {hitable}");
      }
      hitable.HitFX.Update(gameTime);
    }
  }

  public void Clear()
  {
    Shakables.RemoveAll((x) => x.IsDead);
    Hitables.RemoveAll((x) => x.IsDead);
  }
}