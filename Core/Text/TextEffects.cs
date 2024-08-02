using System;
using Microsoft.Xna.Framework;

namespace G;

public class TextEffects
{
  public Action<GameTime, Text, EffectChar, EffectCharColorArg> ColorUpdater { get; set; } =
    (GameTime gameTime, Text text, EffectChar c, EffectCharColorArg arg) => c.Color = arg.Color;

  public Action<GameTime, Text, EffectChar, EffectCharShakeArg> ShakeUpdater { get; set; } =
    (GameTime gameTime, Text text, EffectChar c, EffectCharShakeArg arg) =>
    {
      if (text.IsFirstFrame)
      {
        Core.AddShakable(c);
        c.Shaker!.Shake(arg.Intensity, arg.Duration, arg.Frequency);
      }
    };
}