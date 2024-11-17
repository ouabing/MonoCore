using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace G;

public partial class Palette
{
  [GeneratedRegex(@"^#([A-Fa-f0-9]{6})|([A-Fa-f0-9]{8})$")]
  private static partial Regex HexRegex();
  public static ITheme Theme { get; set; } = new ApolloTheme();
  public static ITheme ConsoleTheme { get; set; } = new CGATheme();
  public static Color Black => Theme.Black;
  public static Color White => Theme.White;
  public static List<Color> Blue => Theme.Blue;
  public static List<Color> Green => Theme.Green;
  public static List<Color> Yellow => Theme.Yellow;
  public static List<Color> Red => Theme.Red;
  public static List<Color> Grey => Theme.Grey;
  public static List<Color> Purple => Theme.Purple;

  public static Color GetColor(string name)
  {
    return GetColor(Theme, name);
  }

  public static Color GetColor(ITheme theme, string name)
  {
    if (name.ToLower() == "black")
    {
      return theme.Black;
    }
    else if (name.ToLower() == "white")
    {
      return theme.White;
    }
    else if (name.ToLower().StartsWith("blue"))
    {
      var index = int.Parse(name[4..]);
      return theme.Blue[index];
    }
    else if (name.ToLower().StartsWith("green"))
    {
      var index = int.Parse(name[5..]);
      return theme.Green[index];
    }
    else if (name.ToLower().StartsWith("yellow"))
    {
      var index = int.Parse(name[6..]);
      return theme.Yellow[index];
    }
    else if (name.ToLower().StartsWith("red"))
    {
      var index = int.Parse(name[3..]);
      return theme.Red[index];
    }
    else if (name.ToLower().StartsWith("grey"))
    {
      var index = int.Parse(name[4..]);
      return theme.Grey[index];
    }
    else if (name.ToLower().StartsWith("purple"))
    {
      var index = int.Parse(name[6..]);
      return theme.Purple[index];
    }
    else if (HexRegex().IsMatch(name))
    {
      return FromRgba(name);
    }
    throw new ArgumentException($"Color {name} not found");
  }


  public static void SetTheme(ITheme theme)
  {
    Theme = theme;
  }

  public static List<Color> FromRgbas(List<string> colors)
  {
    var result = new List<Color>();
    foreach (var color in colors)
    {
      result.Add(FromRgba(color));
    }
    return result;
  }

  public static Color FromRgba(ReadOnlySpan<char> colorAsHex)
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

  public static Color FromRgba(float r, float g, float b, float a)
  {
    return new Color(r, g, b, a);
  }

  static int ParseInt(ReadOnlySpan<char> s) =>
  int.Parse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
}