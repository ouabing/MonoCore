using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace G;

public class Palette
{
  public static readonly Color Black = FromArgb("#000000");
  public static readonly Color White = FromArgb("#D2EAD6");
  public static readonly Color Red = Color.Red;

  public static Color GetColor(string name)
  {
    switch (name)
    {
      case "Black":
        return Black;
      case "White":
        return White;
      default:
        throw new KeyNotFoundException($"Color {name} not found.");
    }
  }


  static void SetTheme(string theme)
  {
    // TODO
  }

  static Color FromArgb(ReadOnlySpan<char> colorAsHex)
  {
    int red = 0;
    int green = 0;
    int blue = 0;
    int alpha = 255;

    if (!colorAsHex.IsEmpty)
    {
      //Skip # if present
      if (colorAsHex[0] == '#')
        colorAsHex = colorAsHex.Slice(1);

      if (colorAsHex.Length == 6)
      {
        //#RRGGBB
        red = ParseInt(colorAsHex.Slice(0, 2));
        green = ParseInt(colorAsHex.Slice(2, 2));
        blue = ParseInt(colorAsHex.Slice(4, 2));
      }
      else if (colorAsHex.Length == 3)
      {
        //#RGB
        Span<char> temp = stackalloc char[2];
        temp[0] = temp[1] = colorAsHex[0];
        red = ParseInt(temp);

        temp[0] = temp[1] = colorAsHex[1];
        green = ParseInt(temp);

        temp[0] = temp[1] = colorAsHex[2];
        blue = ParseInt(temp);
      }
      else if (colorAsHex.Length == 4)
      {
        //#ARGB
        Span<char> temp = stackalloc char[2];
        temp[0] = temp[1] = colorAsHex[0];
        alpha = ParseInt(temp);

        temp[0] = temp[1] = colorAsHex[1];
        red = ParseInt(temp);

        temp[0] = temp[1] = colorAsHex[2];
        green = ParseInt(temp);

        temp[0] = temp[1] = colorAsHex[3];
        blue = ParseInt(temp);
      }
      else if (colorAsHex.Length == 8)
      {
        //#AARRGGBB
        alpha = ParseInt(colorAsHex.Slice(0, 2));
        red = ParseInt(colorAsHex.Slice(2, 2));
        green = ParseInt(colorAsHex.Slice(4, 2));
        blue = ParseInt(colorAsHex.Slice(6, 2));
      }
    }

    return FromRgba(red / 255f, green / 255f, blue / 255f, alpha / 255f);
  }

  private static Color FromRgba(float r, float g, float b, float a)
  {
    return new Color(r, g, b, a);
  }

  static int ParseInt(ReadOnlySpan<char> s) =>
  int.Parse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
}