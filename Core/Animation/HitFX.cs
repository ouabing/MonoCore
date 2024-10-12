using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public class HitFX
{
  public HitFX()
  {
    Add("Main", 0);
  }

  private Dictionary<string, Spring> springs { get; } = [];
  private Dictionary<string, Flash> flashes { get; } = [];
  private float duration = 4f;
  private double? timer { get; set; }

  public float GetSpringAmount(string name = "Main")
  {
    var spring = springs[name];
    if (spring is null)
    {
      throw new($"Spring {name} not found");
    }
    return spring.Amount;
  }

  public bool GetFlash(string name = "Main")
  {
    var flash = flashes[name];
    if (flash is null)
    {
      throw new($"Spring {name} not found");
    }
    return flash.IsVisible;
  }

  public void Update(GameTime gameTime)
  {
    if (timer == null)
    {
      return;
    }

    if (Core.Timer.Time - timer > duration)
    {
      timer = null;
      foreach (var spring in springs.Values)
      {
        spring.Stop();
      }
      foreach (var flash in flashes.Values)
      {
        flash.Stop();
      }
    }

    foreach (var spring in springs.Values)
    {
      spring.Update(gameTime);
    }
    foreach (var flash in flashes.Values)
    {
      flash.Update(gameTime);
    }
  }

  public void Add(
    string name,
    float springTargetAmount = 0,
    float springStiffness = 200,
    float springDamping = 2,
    float flashDuration = 0.15f
  )
  {
    springs.Add(name, new Spring(springTargetAmount, springStiffness, springDamping));
    flashes.Add(name, new Flash(flashDuration));
  }

  public void Start(
    string name = "Main",
    float springTargetAmount = 1,
    float? springStiffness = null,
    float? springDamping = null,
    float? flashDuration = null
  )
  {
    timer = Core.Timer.Time;
    springs[name].Pull(springTargetAmount, springStiffness, springDamping);
    flashes[name].Start(flashDuration);
  }
}