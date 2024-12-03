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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  private Effect SineEffect { get; set; }
  private Effect PixelationEffect { get; set; }
  private Effect PaletteCyclingEffect { get; set; }
  private Effect VHSEffect { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  private Vector3[] PaletteColors { get; set; } = [];
  private int PaletteColorsCount { get; set; }
  public bool IsPixelationEffectActive { get; set; }
  public bool IsVHSEffectActive { get; set; }
  public bool IsPaletteCyclingActive { get; set; }
  public bool IsSineEffectActive { get; set; }
  private float originalPixelationTexelSize;
  private float pixelationTexelSize;
  private float pixelationDuration;
  private float pixelationTimer;
  private float sineTimer;
  private float sineDuration;
  private float sineAmplitude;
  private float sineFrequency;
  private float paletteCyclingTimer;
  private float paletteCyclingSpeed;

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
    VHSEffect = LoadEffect("MonoCore/Shader/Effect/VHS");
    PixelationEffect = LoadEffect("MonoCore/Shader/Effect/Pixelation");
    SineEffect = LoadEffect("MonoCore/Shader/Effect/Sine");
    PaletteCyclingEffect = LoadEffect("MonoCore/Shader/Effect/PaletteCycling");
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

  public void EnablePaletteCycling(float speed = 6f / 60f)
  {
    if (IsPaletteCyclingActive)
    {
      return;
    }
    var total = 64;
    Vector3[] colors = new Vector3[total];
    var index = 0;
    colors[index] = Palette.Black.ToVector3();
    index++;
    foreach (var color in Palette.Grey)
    {
      colors[index] = color.ToVector3();
      index++;
    }
    colors[index] = Palette.White.ToVector3();
    index++;

    foreach (var color in Palette.Red)
    {
      colors[index] = color.ToVector3();
      index++;
    }

    foreach (var color in Palette.Yellow)
    {
      colors[index] = color.ToVector3();
      index++;
    }

    foreach (var color in Palette.Blue)
    {
      colors[index] = color.ToVector3();
      index++;
    }

    foreach (var color in Palette.Purple)
    {
      colors[index] = color.ToVector3();
      index++;
    }

    foreach (var color in Palette.Green)
    {
      colors[index] = color.ToVector3();
      index++;
    }

    PaletteColorsCount = index;
    while (index < total)
    {
      colors[index] = new Vector3(0.0f, 0.0f, 0.0f);
      index++;
    }
    PaletteColors = colors;
    PaletteCyclingEffect.Parameters["colors"].SetValue(colors);
    PaletteCyclingEffect.Parameters["colorCount"].SetValue(PaletteColorsCount);
    PaletteCyclingEffect.Parameters["time"].SetValue(0f);
    PaletteCyclingEffect.Parameters["speed"].SetValue(paletteCyclingSpeed);
    paletteCyclingTimer = 0;
    paletteCyclingSpeed = speed;
    Core.Layer.ApplyGlobalFX(PaletteCyclingEffect);
    IsPaletteCyclingActive = true;
  }

  public void DisablePaletteCycling()
  {
    if (!IsPaletteCyclingActive)
    {
      return;
    }
    Core.Layer.RemoveGlobalFX(PaletteCyclingEffect);
    PaletteColors = [];
    IsPaletteCyclingActive = false;
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
    if (IsPaletteCyclingActive)
    {
      UpdatePaletteCycling(gameTime);
    }
  }

  private void UpdatePaletteCycling(GameTime gameTime)
  {
    PaletteCyclingEffect.Parameters["colors"].SetValue(PaletteColors);
    PaletteCyclingEffect.Parameters["colorCount"].SetValue(PaletteColorsCount);
    PaletteCyclingEffect.Parameters["time"].SetValue(paletteCyclingTimer);
    PaletteCyclingEffect.Parameters["speed"].SetValue(paletteCyclingSpeed);
    paletteCyclingTimer += gameTime.GetElapsedSeconds();
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