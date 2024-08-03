using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public enum CharEffectType
{
  Color,
  Shake
}

public abstract class CharEffectArg(CharEffectType type)
{
  public CharEffectType Type { get; } = type;
}

public class EffectCharColorArg(Color color) : CharEffectArg(CharEffectType.Color)
{
  public Color Color { get; } = color;
}

public class EffectCharShakeArg(float intensity, float duration, int frequency) : CharEffectArg(CharEffectType.Shake)
{
  public float Intensity { get; } = intensity;
  public float Duration { get; } = duration;
  public int Frequency { get; } = frequency;
}

public class EffectChar(string c, List<CharEffectArg> args) : Component
{
  public int Index { get; set; }
  public string C { get; set; } = c;
  public Color Color { get; set; }
  public int Line { get; set; }
  public List<CharEffectArg> Effects { get; } = args;

  public override void Draw(GameTime gameTime)
  {
  }

  public override void LoadContent()
  {
  }

  public override void Update(GameTime gameTime)
  {
  }
}