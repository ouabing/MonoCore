using System;
using System.Collections.Generic;
using System.Text;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace G;

public class ConsoleLine
{
  public Color Color { get; set; }
  public string Prompt { get; private set; }
  public string Text { get; private set; }
  public Text? EffectText { get; private set; }
  public List<string> WrappedLines { get; private set; }
  public DateTime Timestamp { get; private set; }
  public SpriteFontBase Font { get; private set; }
  public float MaxLineWidth { get; private set; }
  public bool AllowEffect { get; private set; }
  public int LineHeight { get; private set; }
  public int LineSpacing { get; private set; }


  public ConsoleLine(
    string prompt,
    string text,
    Color color,
    float maxLineWidth,
    SpriteFontBase font,
    bool allowEffect,
    int lineHeight,
    int lineSpacing
  )
  {
    Prompt = prompt;
    Color = color;
    Timestamp = DateTime.Now;
    Font = font;
    MaxLineWidth = maxLineWidth;
    AllowEffect = allowEffect;
    LineHeight = lineHeight;
    LineSpacing = lineSpacing;

    if (AllowEffect)
    {
      EffectText = new Text(
        prompt + text,
        font,
        maxLineWidth,
        heightMultiplier: (float)(LineHeight + LineSpacing) / LineHeight,
        defaultColor: color,
        wrapWords: false,
        theme: Palette.ConsoleTheme
      );
      Text = EffectText.ToPlainText();
    }
    else
    {
      Text = text;
    }

    WrappedLines = WrapText(prompt + Text, maxLineWidth);
  }

  public void EnableEffect()
  {
    if (!AllowEffect)
    {
      AllowEffect = true;
      EffectText = new Text(
        Prompt + Text,
        Font,
        MaxLineWidth,
        heightMultiplier: (float)(LineHeight + LineSpacing) / LineHeight,
        defaultColor: Color,
        wrapWords: false,
        theme: Palette.ConsoleTheme
      );
      Text = EffectText.ToPlainText();
      WrappedLines = WrapText(Prompt + Text, MaxLineWidth);
    }
  }

  public void Update(GameTime gameTime)
  {
    if (AllowEffect)
    {
      EffectText?.Update(gameTime);
    }
  }

  public void Draw(GameTime gameTime, int x, ref int y)
  {
    if (AllowEffect)
    {
      EffectText!.Draw(gameTime, new Vector2(x, y), OriginType.TopLeft);
      y += (int)EffectText!.MeasuredSize.Y;
    }
    else
    {
      foreach (var wrappedLine in WrappedLines)
      {
        Font.DrawText(Core.Sb, wrappedLine, new Vector2(x, y), Color);
        y += LineHeight + LineSpacing;
      }
    }
  }

  public void UpdateText(string text)
  {
    if (AllowEffect)
    {
      EffectText = new Text(
        Prompt + text,
        Font,
        MaxLineWidth,
        heightMultiplier: (float)(LineHeight + LineSpacing) / LineHeight,
        defaultColor: Color,
        wrapWords: false,
        theme: Palette.ConsoleTheme
      );
      Text = EffectText.ToPlainText();
    }
    else
    {
      Text = text;
    }
    WrappedLines = WrapText(Prompt + Text, MaxLineWidth);
  }

  private List<string> WrapText(string text, float maxLineWidth)
  {
    List<string> lines = [];
    StringBuilder currentLine = new();
    var currentIndex = 0;

    while (currentIndex < text.Length)
    {
      if (Font.MeasureString(currentLine).X >= maxLineWidth)
      {
        currentLine.Remove(currentLine.Length - 1, 1);
        lines.Add(currentLine.ToString());
        currentLine.Clear();
      }
      else if (text[currentIndex] == '\n')
      {
        lines.Add(currentLine.ToString());
        currentLine.Clear();
        currentIndex++;
      }
      else
      {
        currentLine.Append(text[currentIndex]);
        currentIndex++;
      }
    }

    if (currentLine.Length > 0)
    {
      lines.Add(currentLine.ToString());
    }

    if (lines.Count == 0)
    {
      lines.Add("");
    }

    return lines;
  }
}