using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FontStashSharp;
using Microsoft.Xna.Framework;

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
  * The color is what you defined in the Palette class.
  */
public class Text
{
  public static readonly TextEffects DefaultTextEffects = new();
  private static readonly string Pattern = @"\[(.+)\]\((.+)\)";
  private static readonly string EffectPattern = @"(.*)\=(.*)";

  public TextEffects TextEffects { get; private set; }
  public float Width { get; private set; }
  public float Height { get; private set; }
  public Vector2 MeasuredSize { get; private set; }
  public bool IsFirstFrame { get; private set; } = true;
  private readonly string RawText;
  private readonly SpriteFontBase Font;
  private readonly TextAlignment Alignment;
  private readonly List<EffectChar> Chars = [];
  private readonly float HeightMultiplier;
  private readonly int ShadowWidth;
  private readonly Color? ShadowColor;
  public Text(
    string rawText,
    SpriteFontBase font,
    float width,
    float height = 0,
    int shadowWidth = 0,
    Color? shadowColor = null,
    TextAlignment alignment = TextAlignment.Left,
    TextEffects? textEffects = null,
    float heightMultiplier = 1f
  )
  {
    RawText = rawText;
    Font = font;
    Alignment = alignment;
    Width = width;
    Height = height;
    HeightMultiplier = heightMultiplier;
    ShadowWidth = shadowWidth;
    ShadowColor = shadowColor ?? Palette.Black;
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
          effects.Add(GenerateEffect(effectName, effectArgs.Split(",")));
        }
      }
      parsed.Add(new MatchedEffectText()
      {
        TextBlockStart = match.Index,
        TextBlockEnd = match.Groups[1].Index + match.Groups[1].Length,
        EffectBlockStart = match.Groups[2].Index - 1,
        EffectBlockEnd = match.Index + match.Length,
        Effects = effects
      });
    };

    for (int i = 0; i < RawText.Length; i++)
    {
      char currentChar = RawText[i];
      string c = "";

      var foundIndex = parsed.FindIndex(x => i >= x.TextBlockStart && x.EffectBlockEnd >= i);
      MatchedEffectText? found = foundIndex == -1 ? null : parsed[foundIndex];
      if (found != null)
      {
        MatchedEffectText matched = (MatchedEffectText)found;
        if (i == matched.TextBlockStart || i >= matched.TextBlockEnd)
        {
          continue;
        }
      }

      // 检查是否是代理对的高位字符
      if (char.IsHighSurrogate(currentChar) && i + 1 < RawText.Length && char.IsLowSurrogate(RawText[i + 1]))
      {
        char highSurrogate = currentChar;
        char lowSurrogate = RawText[i + 1];
        string fullCharacter = new(new char[] { highSurrogate, lowSurrogate });
        c = fullCharacter;
        i++; // 跳过低位代理字符
      }
      else
      {
        c = currentChar.ToString();
      }

      Chars.Add(new EffectChar(c, found?.Effects ?? []));
    }
  }

  private static CharEffectArg GenerateEffect(string name, string[] args)
  {
    switch (name)
    {
      case "color":
        return new EffectCharColorArg(Palette.GetColor(args[0]));
      case "shake":
        var intensity = args.Length > 0 ? float.Parse(args[0]) : 10;
        var duration = args.Length > 1 ? float.Parse(args[1]) : 1;
        var frequency = args.Length > 2 ? int.Parse(args[2]) : 60;
        return new EffectCharShakeArg(intensity, duration, frequency);
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
      if (c.C == "|")
      {
        cx = 0;
        cy += lineHeight;
        line += 1;
      }
      else if (c.C == " ")
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

          cx += c.Size.X;
          if (cx > Width)
          {
            cx = 0;
            cy += lineHeight;
            line += 1;
          }
        }
        else
        {
          // set | to remove it in the next step, as it was already wrapped and doesn't need to be visually represented
          c.C = "|";
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
    Chars.RemoveAll(c => c.C == "|");

    for (int i = 0; i < Chars.Count; i++)
    {
      Chars[i].Index = i;
    }

    // measure text to set alignments next

    var textWidth = 0f;
    List<float> lineWidths = [];

    for (int i = 0; i <= Chars.Last().Line; i++)
    {
      var lineChars = Chars.Where(c => c.Line == i);
      var lineWidth = lineChars.Sum(c => c.Size.X);
      lineWidths.Add(lineWidth);
      if (lineWidth > textWidth)
      {
        textWidth = lineWidth;
      }
    }
    MeasuredSize = new Vector2(textWidth, Chars.Last().Position.Y + lineHeight);
    if (Height == 0)
    {
      Height = MeasuredSize.Y;
    }

    // Sets x of each character to match the given .text_alignment, unchanged if it is 'left'
    if (Alignment != TextAlignment.Left)
    {
      for (int i = 0; i <= Chars.Last().Line; i++)
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
          var spaces = Chars.Where(c => c.Line == i && c.C == " ").ToList();
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
        if (effect.Type == CharEffectType.Shake)
        {
          TextEffects.ShakeUpdater(gameTime, this, c, (EffectCharShakeArg)effect);
        }
        else if (effect.Type == CharEffectType.Color)
        {
          TextEffects.ColorUpdater(gameTime, this, c, (EffectCharColorArg)effect);
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
      var pos = Position + c.Position + (c.Shaker?.Amount ?? Vector2.Zero) - MeasuredSize / 2f;

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
            ShadowColor ?? Palette.Black
          );
        }
      }
      Font.DrawText(
        Core.Sb,
        c.C,
        pos,
        c.Color ?? Palette.White
      );
    }
  }
}