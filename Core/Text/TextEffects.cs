using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public class TextEffects
{
  public Action<GameTime, Text, EffectChar, EffectCharColorArg> ColorUpdater { get; set; } =
    (GameTime gameTime, Text text, EffectChar c, EffectCharColorArg arg) => c.Color = arg.Color;

  public Action<GameTime, Text, EffectChar, EffectCharBackgroundColorArg> BackgroundColorUpdater { get; set; } =
    (GameTime gameTime, Text text, EffectChar c, EffectCharBackgroundColorArg arg) =>
    {
      c.BackgroundPaddingVertical = arg.PaddingVertical;
      c.BackgroundColor = arg.Color;
    };

  public Action<GameTime, Text, EffectChar, EffectCharShakeArg> ShakeUpdater { get; set; } =
    (GameTime gameTime, Text text, EffectChar c, EffectCharShakeArg arg) =>
    {
      if (text.IsFirstFrame)
      {
        c.EnableShake = true;
        c.Shaker!.Shake(arg.Intensity, arg.Duration, arg.Frequency);
      }
    };

  public Action<GameTime, Text, EffectChar, EffectCharOscillateArg> OscillateUpdater { get; set; } =
    (GameTime gameTime, Text text, EffectChar c, EffectCharOscillateArg arg) =>
    {
      if (text.IsFirstFrame)
      {
        c.EnableOscillate(arg.Min, arg.Max, arg.Speed, arg.Delay * c.Index);
      }
      c.Osc!.Update(gameTime);
    };

  public Action<GameTime, Text, EffectChar, EffectCharGradientArg> GradientUpdater { get; set; } =
    (GameTime gameTime, Text text, EffectChar c, EffectCharGradientArg arg) =>
    {
      c.Color = Color.Lerp(arg.Start, arg.End, (float)(c.Index - arg.StartIndex) / (arg.EndIndex - arg.StartIndex));
    };

  public Action<GameTime, Text, EffectChar, EffectCharBlinkArg> BlinkUpdater { get; set; } =
    (GameTime gameTime, Text text, EffectChar c, EffectCharBlinkArg arg) =>
    {
      if (text.IsFirstFrame)
      {
        c.BlinkTimer = 0;
      }

      if (c.BlinkTimer >= arg.Duration)
      {
        c.Opacity = 1;
      }
      else
      {
        c.Opacity = c.BlinkTimer % arg.Interval < arg.Interval / 2 ? 0 : 1;
        c.BlinkTimer += gameTime.GetElapsedSeconds();
      }
    };
}