using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace G;

public class LightManager
{
  public List<Light2D> Lights { get; set; } = [];
  public Color AmbientColor { get; set; } = Color.White;
  public float AmbientIntensity { get; set; } = 1f;
  public Effect AmbientFX { get; set; }

  public void LoadContent()
  {
    AmbientFX = Core.Content.Load<Effect>("MonoCore/Shader/AmbientLight");
    AmbientFX.Parameters["ambientColor"].SetValue(AmbientColor.ToVector4());
    AmbientFX.Parameters["intensity"].SetValue(AmbientIntensity);
  }

  public void Add(Light2D light)
  {
    if (Lights.Contains(light))
    {
      throw new System.ArgumentException("Light already exists.");
    }
    if (light.ContentLoaded == false)
    {
      light.LoadContent();
    }
    Lights.Add(light);
  }

  public void Remove(Light2D light)
  {
    Lights.Remove(light);
  }

  public void Clear()
  {
    Lights.Clear();
  }

  public void ApplyAmbientLight(Color color, float intensity)
  {
    AmbientColor = color;
    AmbientIntensity = intensity;
    Core.Layer.ApplyGlobalFX(AmbientFX);
  }

  public void Update(GameTime gameTime)
  {
    AmbientFX.Parameters["ambientColor"].SetValue(AmbientColor.ToVector4());
    AmbientFX.Parameters["intensity"].SetValue(AmbientIntensity);
  }

  public void PostUpdate(GameTime gameTime)
  {
    Lights.RemoveAll(x => x.IsDead || x.Following?.IsDead == true);
    // Order by Z
    var lights = Lights.OrderBy(x => -x.Z);

    foreach (var light in Lights)
    {
      light.PostUpdate(gameTime);
    }
  }

  public void DrawLightBetweenZ(GameTime gameTime, RenderTarget2D currentScreenRenderTarget, float min, float max)
  {
    var lights = Lights.Where(x => x.Z <= max && x.Z > min);

    foreach (var light in lights)
    {
      light.DrawScreenRenderTarget(gameTime, currentScreenRenderTarget);
    }
  }
}