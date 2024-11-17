using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public enum CharEffectType
{
  BackgroundColor,
  Color,
  Shake,
  Oscillate,
  Gradient,
  Blink
}

public abstract class CharEffectArg(CharEffectType type)
{
  public CharEffectType Type { get; } = type;
}

public class EffectCharColorArg(Color color) : CharEffectArg(CharEffectType.Color)
{
  public Color Color { get; } = color;
}
public class EffectCharBackgroundColorArg(Color color) : CharEffectArg(CharEffectType.BackgroundColor)
{
  public Color Color { get; } = color;
}

public class EffectCharShakeArg(float intensity, float duration, int frequency) : CharEffectArg(CharEffectType.Shake)
{
  public float Intensity { get; } = intensity;
  public float Duration { get; } = duration;
  public int Frequency { get; } = frequency;
}

public class EffectCharOscillateArg(float min, float max, float speed, float delay) : CharEffectArg(CharEffectType.Oscillate)
{
  public float Min { get; } = min;
  public float Max { get; } = max;
  public float Speed { get; } = speed;
  public float Delay { get; } = delay;
}

public class EffectCharGradientArg(Color start, Color end, int startIndex, int endIndex) : CharEffectArg(CharEffectType.Gradient)
{
  public Color Start { get; } = start;
  public Color End { get; } = end;
  public int StartIndex { get; } = startIndex;
  public int EndIndex { get; } = endIndex;
}

public class EffectCharBlinkArg(float interval, float duration) : CharEffectArg(CharEffectType.Blink)
{
  public float Interval { get; } = interval;
  public float Duration { get; } = duration;
}

public class EffectChar(string c, List<CharEffectArg> args) : Component
{
  public int Index { get; set; }
  public string C { get; set; } = c;
  public Color? BackgroundColor { get; set; }
  public Color? Color { get; set; }
  public int Line { get; set; }
  public Oscillator? Osc { get; set; }
  public List<CharEffectArg> Effects { get; } = args;
  public float BlinkTimer { get; set; }

  public void EnableOscillate(float min, float max, float speed, float delay)
  {
    Osc = new Oscillator(min, max, speed, delay);
  }

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