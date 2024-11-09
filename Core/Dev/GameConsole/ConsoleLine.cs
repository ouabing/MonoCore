using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace G;

public class ConsoleLine
{
  public Color Color { get; set; }
  public string Prompt { get; private set; }
  public string Text { get; private set; }
  public List<string> WrappedLines { get; private set; }
  public DateTime Timestamp { get; private set; }
  public int FontSize { get; private set; }
  public float MaxLineWidth { get; private set; }

  public ConsoleLine(string prompt, string text, Color color, float maxLineWidth, int fontSize)
  {
    Prompt = prompt;
    Text = text;
    Color = color;
    Timestamp = DateTime.Now;
    FontSize = fontSize;
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
    var font = Core.Font.Get(FontSize);
    List<string> lines = [];
    StringBuilder currentLine = new();
    var currentIndex = 0;

    while (currentIndex < text.Length)
    {
      if (font.MeasureString(currentLine).X >= maxLineWidth)
      {
        currentLine.Remove(currentLine.Length - 1, 1);
        lines.Add(currentLine.ToString());
        currentLine.Clear();
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