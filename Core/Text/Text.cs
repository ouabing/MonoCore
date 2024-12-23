using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FontStashSharp;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;

public enum TextAlignment { Left, Center, Right, Justify }


/**
  * Markdown like text renderer inspired by a327ex
  *
  * You can use the following syntax to create and apply effects to text:
  * [Text Message A](color=Red;shake=10,2,60)Plain Text B[Text Message C](color=Blue)
  * This will create a text with 3 parts:
  * A: Red color and shakes for 2 seconds in 60fps
  * B: Plain text
  * C: Blue color
  *
  * The color is what you defined in the Palette class, or specified by theme
  */
public class Text
{
  public static readonly TextEffects DefaultTextEffects = new();
  private static readonly string Pattern = @"\[(.+?)\]\((.+?)\)";
  private static readonly string EffectPattern = @"(.*)\=(.*)";

  public TextEffects TextEffects { get; private set; }
  public float Width { get; private set; }
  public float Height { get; private set; }
  public Vector2 MeasuredSize { get; private set; }
  public bool IsFirstFrame { get; private set; } = true;
  private readonly string RawText;
  private readonly SpriteFontBase Font;
  private readonly TextAlignment Alignment;
  public List<EffectChar> Chars { get; } = [];
  private readonly float HeightMultiplier;
  private readonly int ShadowWidth;
  private readonly Color? ShadowColor;
  private readonly Color? DefaultColor;
  private readonly bool WrapWords;
  private readonly ITheme Theme;
  private const string LineSeparator = "\n";
  public Text(
    string rawText,
    SpriteFontBase font,
    float width,
    float height = 0,
    int shadowWidth = 0,
    Color? shadowColor = null,
    TextAlignment alignment = TextAlignment.Left,
    TextEffects? textEffects = null,
    float heightMultiplier = 1f,
    Color? defaultColor = null,
    bool wrapWords = true,
    ITheme? theme = null
  )
  {
    RawText = rawText;
    Font = font;
    Alignment = alignment;
    Width = width;
    Height = height;
    HeightMultiplier = heightMultiplier;
    ShadowWidth = shadowWidth;
    Theme = theme ?? Palette.Theme;
    ShadowColor = shadowColor ?? Theme.Black;
    DefaultColor = defaultColor ?? Theme.White;
    WrapWords = wrapWords;
    if (textEffects == null)
    {
      TextEffects = DefaultTextEffects;
    }
    else
    {
      TextEffects = textEffects!;
    }

    Parse();
    FormatChars();
  }

  public string ToPlainText()
  {
    var result = "";
    var lastLine = 0;
    foreach (var c in Chars)
    {
      if (c.Line != lastLine)
      {
        for (int i = 0; i < c.Line - lastLine; i++)
        {
          result += "\n";
        }
      }
      lastLine = c.Line;
      result += c.C;
    }
    return result;
  }

  private struct MatchedEffectText
  {
    public required int TextBlockStart;
    public required int TextBlockEnd;
    public required int EffectBlockStart;
    public required int EffectBlockEnd;
    public required List<CharEffectArg> Effects;

    public readonly int TextStart => TextBlockStart + 1;
    public readonly int TextEnd => TextBlockEnd - 1;
    public readonly int EffectStart => EffectBlockStart + 1;
    public readonly int EffectEnd => EffectBlockEnd - 1;
  }

  private void Parse()
  {
    List<MatchedEffectText> parsed = [];
    foreach (Match match in Regex.Matches(RawText, Pattern, RegexOptions.None, Regex.InfiniteMatchTimeout))
    {
      if (!match.Success)
      {
        continue;
      }

      var effectText = match.Groups[1].Value;
      var rawEffects = match.Groups[2].Value;

      var textBlockStart = match.Index;
      var textBlockEnd = match.Groups[1].Index + match.Groups[1].Length;
      var effectBlockStart = match.Groups[2].Index - 1;
      var effectBlockEnd = match.Index + match.Length;
      List<CharEffectArg> effects = [];
      foreach (string rawEffect in rawEffects.Split(";"))
      {
        foreach (Match effectMatch in Regex.Matches(rawEffect, EffectPattern, RegexOptions.None, Regex.InfiniteMatchTimeout))
        {
          if (!effectMatch.Success)
          {
            continue;
          }
          var effectName = effectMatch.Groups[1].Value;
          var effectArgs = effectMatch.Groups[2].Value;
          effects.Add(GenerateEffect(effectName, effectArgs.Split(","), textBlockStart, textBlockEnd));
        }
      }
      parsed.Add(new MatchedEffectText()
      {
        TextBlockStart = textBlockStart,
        TextBlockEnd = textBlockEnd,
        EffectBlockStart = effectBlockStart,
        EffectBlockEnd = effectBlockEnd,
        Effects = effects
      });
    };

    for (int i = 0; i < RawText.Length; i++)
    {
      char currentChar = RawText[i];
      string c = "";

      var foundIndex = parsed.FindIndex(x => i >= x.TextBlockStart && x.EffectBlockEnd > i);
      MatchedEffectText? found = foundIndex == -1 ? null : parsed[foundIndex];
      if (found != null)
      {
        MatchedEffectText matched = (MatchedEffectText)found;
        if (i == matched.TextBlockStart || i >= matched.TextBlockEnd)
        {
          continue;
        }
      }

      // check for surrogate pair
      if (char.IsHighSurrogate(currentChar) && i + 1 < RawText.Length && char.IsLowSurrogate(RawText[i + 1]))
      {
        char highSurrogate = currentChar;
        char lowSurrogate = RawText[i + 1];
        string fullCharacter = new([highSurrogate, lowSurrogate]);
        c = fullCharacter;
        i++; // skip low surrogate
      }
      else
      {
        c = currentChar.ToString();
      }

      Chars.Add(new EffectChar(c, found?.Effects ?? []));
    }
  }

  private CharEffectArg GenerateEffect(string name, string[] args, int textBlockStart, int textBlockEnd)
  {
    switch (name)
    {
      case "bgcolor":
        var color = Palette.GetColor(Theme, args[0]);
        var paddingVertical = args.Length > 1 ? int.Parse(args[1]) : 0;
        return new EffectCharBackgroundColorArg(color, paddingVertical);
      case "color":
        return new EffectCharColorArg(Palette.GetColor(Theme, args[0]));
      case "shake":
        var intensity = args.Length > 0 ? float.Parse(args[0]) : 10;
        var duration = args.Length > 1 ? float.Parse(args[1]) : 1;
        var frequency = args.Length > 2 ? int.Parse(args[2]) : 60;
        return new EffectCharShakeArg(intensity, duration, frequency);
      case "osc":
        var min = args.Length > 0 ? float.Parse(args[0]) : 0;
        var max = args.Length > 1 ? float.Parse(args[1]) : 10;
        var speed = args.Length > 2 ? float.Parse(args[2]) : 1;
        var delay = args.Length > 3 ? float.Parse(args[3]) : 0.1f;
        return new EffectCharOscillateArg(min, max, speed, delay);
      case "grad":
        var startColor = Palette.GetColor(Theme, args[0]);
        var endColor = Palette.GetColor(Theme, args[1]);
        return new EffectCharGradientArg(startColor, endColor, textBlockEnd - textBlockStart - 2);
      case "blink":
        var blinkInterval = args.Length > 0 ? float.Parse(args[0]) : 0.1f;
        var blinkDuration = args.Length > 1 ? float.Parse(args[1]) : 2f;
        return new EffectCharBlinkArg(blinkInterval, blinkDuration);
      default:
        throw new NotImplementedException("Effect not implemented.");
    }
  }

  private void FormatChars()
  {
    var cx = 0f;
    var cy = 0f;
    var line = 0;
    var lineHeight = Font.LineHeight * HeightMultiplier;

    for (int i = 0; i < Chars.Count; i++)
    {
      var c = Chars[i];
      if (c.C == LineSeparator)
      {
        cx = 0;
        cy += lineHeight;
        line += 1;
      }
      else if (c.C == " " && WrapWords)
      {
        var wrapped = false;
        // only check for wrapping if this space is not inside effect delimiters
        if (c.Effects.Count == 0)
        {
          var fromSpaceX = cx;
          // go from next character to next space (the next word) to see if it fits this line
          var nextSpaceIndex = Chars.FindIndex(i + 1, x => x.C == " ");
          if (nextSpaceIndex == -1)
          {
            nextSpaceIndex = Chars.Count;
          }

          var nextWord = "";
          for (var j = i + 1; j < nextSpaceIndex; j++)
          {
            nextWord += Chars[j].C;
          }
          var measured = Font.MeasureString(nextWord);
          fromSpaceX += measured.X;

          // if the word doesn't fit then wrap line here
          if (fromSpaceX > Width)
          {
            cx = 0;
            cy += lineHeight;
            line += 1;
            wrapped = true;
          }
        }

        if (!wrapped)
        {
          c.Position = new Vector2(cx, cy);
          c.Line = line;
          c.Size = Font.MeasureString(c.C);

          if (cx > Width)
          {
            cx = 0;
            cy += lineHeight;
            line += 1;
          }
        }
        else
        {
          // set line separator to remove it in the next step, as it was already wrapped and doesn't need to be visually represented
          c.C = LineSeparator;
        }
      }
      else
      {
        c.Position = new Vector2(cx, cy);
        c.Line = line;
        c.Size = Font.MeasureString(c.C);

        cx += c.Size.X;
        if (cx > Width)
        {
          cx = 0;
          cy += lineHeight;
          line += 1;
        }
      }
    }

    // Remove line separators
    Chars.RemoveAll(c => c.C == LineSeparator);

    for (int i = 0; i < Chars.Count; i++)
    {
      Chars[i].Index = i;
    }

    // measure text to set alignments next

    var textWidth = 0f;
    List<float> lineWidths = [];

    var lastChar = Chars.Count > 0 ? Chars.Last() : null;

    for (int i = 0; i <= lastChar?.Line; i++)
    {
      var lineChars = Chars.Where(c => c.Line == i);
      var lineWidth = lineChars.Sum(c => c.Size.X);
      lineWidths.Add(lineWidth);
      if (lineWidth > textWidth)
      {
        textWidth = lineWidth;
      }
    }
    MeasuredSize = new Vector2(textWidth, (lastChar?.Position.Y ?? 0) + lineHeight);
    if (Height == 0)
    {
      Height = MeasuredSize.Y;
    }

    // Sets x of each character to match the given .text_alignment, unchanged if it is 'left'
    if (Alignment != TextAlignment.Left)
    {
      for (int i = 0; i <= lastChar?.Line; i++)
      {
        var lineWidth = lineWidths[i];
        var leftoverWidth = MeasuredSize.X - lineWidth;
        if (Alignment == TextAlignment.Center)
        {
          foreach (var c in Chars)
          {
            if (c.Line == i)
            {
              c.Position += new Vector2(leftoverWidth / 2, 0);
            }
          }
        }
        else if (Alignment == TextAlignment.Right)
        {
          foreach (var c in Chars)
          {
            if (c.Line == i)
            {
              c.Position += new Vector2(leftoverWidth / 2, 0);
            }
          }
        }
        else if (Alignment == TextAlignment.Justify)
        {
          var spaces = Chars.Where(c => c.Line == i && (c.C == " ")).ToList();
          var addedWidthToEachSpace = leftoverWidth / spaces.Count;
          var totalAddedWidth = 0f;
          foreach (var c in Chars)
          {
            if (c.Line != i)
            {
              continue;
            }
            if (c.C == " ")
            {
              c.Position += new Vector2(addedWidthToEachSpace, 0);
              totalAddedWidth += addedWidthToEachSpace;
            }
            else
            {
              c.Position += new Vector2(totalAddedWidth, 0);
            }
          }
        }
      }
    }
  }

  public void Update(GameTime gameTime)
  {
    foreach (var c in Chars)
    {
      foreach (var effect in c.Effects)
      {
        switch (effect.Type)
        {
          case CharEffectType.BackgroundColor:
            TextEffects.BackgroundColorUpdater(gameTime, this, c, (EffectCharBackgroundColorArg)effect);
            break;
          case CharEffectType.Shake:
            TextEffects.ShakeUpdater(gameTime, this, c, (EffectCharShakeArg)effect);
            break;
          case CharEffectType.Color:
            TextEffects.ColorUpdater(gameTime, this, c, (EffectCharColorArg)effect);
            break;
          case CharEffectType.Oscillate:
            TextEffects.OscillateUpdater(gameTime, this, c, (EffectCharOscillateArg)effect);
            break;
          case CharEffectType.Gradient:
            TextEffects.GradientUpdater(gameTime, this, c, (EffectCharGradientArg)effect);
            break;
          case CharEffectType.Blink:
            TextEffects.BlinkUpdater(gameTime, this, c, (EffectCharBlinkArg)effect);
            break;
        }
      }
    }
    if (IsFirstFrame)
    {
      IsFirstFrame = false;
    }
  }

  public void Draw(GameTime gameTime, Vector2 Position, OriginType originType = OriginType.Center)
  {
    foreach (var c in Chars)
    {
      var pos = Position + c.Position + (c.Shaker?.Amount ?? Vector2.Zero) + new Vector2(0, c.Osc?.Value ?? 0) - MeasuredSize / 2f;

      if (originType == OriginType.TopLeft)
      {
        pos += new Vector2(MeasuredSize.X / 2f, MeasuredSize.Y / 2f);
      }
      if (ShadowWidth > 0)
      {
        for (int i = 1; i <= ShadowWidth; i++)
        {
          Font.DrawText(
            Core.Sb,
            c.C,
            pos + new Vector2(i),
            (ShadowColor ?? Theme.Black) * c.Opacity
          );
        }
      }
      if (c.BackgroundColor != null)
      {
        Core.Sb.FillRectangle(
          new Rectangle(
            (int)pos.X,
            (int)pos.Y - c.BackgroundPaddingVertical,
            (int)c.Size.X,
            (int)c.Size.Y + c.BackgroundPaddingVertical * 2
          ),
          c.BackgroundColor.Value * c.Opacity
        );
      }
      Font.DrawText(
        Core.Sb,
        c.C,
        pos,
        (c.Color ?? DefaultColor ?? Theme.White) * c.Opacity
      );
    }
  }
}