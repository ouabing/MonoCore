using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace G;

public class EffectManager(ContentManager contentManager)
{
  private ContentManager ContentManager { get; } = contentManager;
  private Dictionary<string, Effect> EffectCache { get; } = [];
  private Effect SineEffect { get; set; }
  private Effect PixelationEffect { get; set; }
  private Effect VHSEffect { get; set; }
  public bool IsPixelationEffectActive { get; set; }
  public bool IsVHSEffectActive { get; set; }
  public bool IsSineEffectActive { get; set; }
  private float originalPixelationTexelSize;
  private float pixelationTexelSize;
  private float pixelationDuration;
  private float pixelationTimer;
  private float sineTimer;
  private float sineDuration;
  private float sineAmplitude;
  private float sineFrequency;

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

  public void LoadContent()
  {
    // Preload VHS effect
    VHSEffect = LoadEffect("MonoCore/Shader/Effect/VHS");
    PixelationEffect = LoadEffect("MonoCore/Shader/Effect/Pixelation");
    SineEffect = LoadEffect("MonoCore/Shader/Effect/Sine");
  }

  public void EnableVHS(
    float blurAmount = 1.0f,
    float scanlineIntensity = 0.5f,
    float chromaticAberrationAmount = 0.0005f,
    float noiseIntensity = 0.001f
  )
  {
    if (IsVHSEffectActive)
    {
      return;
    }
    VHSEffect.Parameters["blurAmount"].SetValue(blurAmount);
    VHSEffect.Parameters["scanlineIntensity"].SetValue(scanlineIntensity);
    VHSEffect.Parameters["chromaticAberrationAmount"].SetValue(chromaticAberrationAmount);
    VHSEffect.Parameters["noiseIntensity"].SetValue(noiseIntensity);
    Core.Layer.ApplyGlobalFX(VHSEffect);
    IsVHSEffectActive = true;
  }

  public void DisableVHS()
  {
    if (!IsVHSEffectActive)
    {
      return;
    }
    Core.Layer.RemoveGlobalFX(VHSEffect);
    IsVHSEffectActive = false;
  }


  public void Pixelate(float texelSize = 32.0f, float duration = 0.5f)
  {
    if (IsPixelationEffectActive)
    {
      return;
    }
    pixelationDuration = duration;
    originalPixelationTexelSize = texelSize;
    pixelationTexelSize = texelSize;
    pixelationTimer = duration / texelSize;
    IsPixelationEffectActive = true;
    PixelationEffect.Parameters["texelSize"].SetValue(GetNormalizedTexelSizeVec(texelSize));
    Core.Layer.ApplyGlobalFX(PixelationEffect);
  }

  public void Sine(float frequency = 60.0f, float amplitude = 0.05f, float duration = 1f)
  {
    if (IsSineEffectActive)
    {
      return;
    }
    IsSineEffectActive = true;
    sineAmplitude = amplitude;
    sineFrequency = frequency;
    sineTimer = 0;
    sineDuration = duration;
    Core.Layer.ApplyGlobalFX(SineEffect);
  }

  public void Update(GameTime gameTime)
  {
    if (IsPixelationEffectActive)
    {
      UpdatePixelation(gameTime);
    }
    if (IsVHSEffectActive)
    {
      UpdateVHS(gameTime);
    }
    if (IsSineEffectActive)
    {
      UpdateSine(gameTime);
    }
  }

  private void UpdateSine(GameTime gameTime)
  {
    if (sineTimer >= sineDuration)
    {
      IsSineEffectActive = false;
      Core.Layer.RemoveGlobalFX(SineEffect);
      return;
    }
    SineEffect.Parameters["frequency"].SetValue(sineFrequency);
    SineEffect.Parameters["time"].SetValue(sineTimer * 10);
    SineEffect.Parameters["amplitude"].SetValue((sineDuration - sineTimer) / sineDuration * sineAmplitude);
    sineTimer += gameTime.GetElapsedSeconds();
  }

  private void UpdateVHS(GameTime gameTime)
  {
    VHSEffect?.Parameters["time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
  }

  private static Vector2 GetNormalizedTexelSizeVec(float texelSize)
  {
    return new Vector2(
      texelSize / Core.Screen.DisplayWidth,
      texelSize / Core.Screen.DisplayHeight
    );
  }

  private void UpdatePixelation(GameTime gameTime)
  {
    var dt = gameTime.GetElapsedSeconds();
    if (pixelationTexelSize == 1.0f)
    {
      IsPixelationEffectActive = false;
      Core.Layer.RemoveGlobalFX(PixelationEffect);
      return;
    }

    pixelationTimer -= dt;
    if (pixelationTimer <= 0)
    {
      pixelationTexelSize -= 1.0f;
      pixelationTimer = pixelationDuration / originalPixelationTexelSize;
      PixelationEffect.Parameters["texelSize"].SetValue(GetNormalizedTexelSizeVec(pixelationTexelSize));
    }
  }
}