using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace G;

public class EffectManager(ContentManager contentManager)
{
  private ContentManager ContentManager { get; } = contentManager;
  private Dictionary<string, Effect> EffectCache { get; } = [];

  public Effect LoadEffect(string path)
  {

    if (EffectCache.TryGetValue(path, out Effect? value))
    {
      return value;
    }
    var effect = ContentManager!.Load<Effect>(path);
    EffectCache[path] = effect;
    return effect;
  }

  public void EnableVHS(
    float blurAmount,
    float scanlineIntensity,
    float chromaticAberrationAmount,
    float noiseIntensity
  )
  {
    var effect = LoadEffect("MonoCore/Shader/VHS");
    effect.Parameters["blurAmount"].SetValue(blurAmount);
    effect.Parameters["scanlineIntensity"].SetValue(scanlineIntensity);
    effect.Parameters["chromaticAberrationAmount"].SetValue(chromaticAberrationAmount);
    effect.Parameters["noiseIntensity"].SetValue(noiseIntensity);
    Core.Layer.ApplyGlobalFX(effect);
  }
}