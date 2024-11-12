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
  public List<string> WrappedLines { get; private set; }
  public DateTime Timestamp { get; private set; }
  public SpriteFontBase Font { get; private set; }
  public float MaxLineWidth { get; private set; }

  public ConsoleLine(string prompt, string text, Color color, float maxLineWidth, SpriteFontBase font)
  {
    Prompt = prompt;
    Text = text;
    Color = color;
    Timestamp = DateTime.Now;
    Font = font;
    MaxLineWidth = maxLineWidth;

    WrappedLines = WrapText(prompt + text, maxLineWidth);
  }

  public void UpdateText(string text)
  {
    Text = text;
    WrappedLines = WrapText(Prompt + text, MaxLineWidth);
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