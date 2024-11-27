using System;
using FontStashSharp.RichText;

namespace G;

public class PaletteCommand : ConsoleCommand
{
  public PaletteCommand() : base("palette", "Print current theme", "palette", [])
  {
  }

  public override void Execute(GameConsole console, string[] args)
  {
    if (args.Length > 1)
    {
      PrintInvalidNumberOfArgumentsError(console);
      return;
    }

    if (args.Length == 0)
    {
      PrintTheme(console, Palette.Theme);
      return;
    }
    if (args[0] == "--help")
    {
      PrintHelp(console);
      return;
    }

    switch (args[0].ToLower())
    {
      case "apollo":
        PrintTheme(console, new ApolloTheme());
        return;
      case "cga":
        PrintTheme(console, new CGATheme());
        return;
      default:
        PrintArgumentError(console, args[0]);
        return;
    }
  }

  private static void PrintTheme(GameConsole console, ITheme theme)
  {
    console.Print($"  {theme.Name}");
    console.Print("");
    console.Print($"  white   black   grey    red     yellow  green   blue    purple");
    console.Print("");

    var max = Math.Max(theme.Grey.Count, Math.Max(theme.Red.Count, Math.Max(theme.Yellow.Count, Math.Max(theme.Green.Count, theme.Blue.Count))));

    for (int i = 0; i < max; i++)
    {
      var line = "";
      var colorLine = "";
      if (i == 0)
      {
        colorLine = $" {ToEffectText(theme.White.ToHexString(), true, console.LineSpacing)} {ToEffectText(theme.Black.ToHexString(), true, console.LineSpacing)} ";
        line = $" {ToEffectText(theme.White.ToHexString(), false, console.LineSpacing)} {ToEffectText(theme.Black.ToHexString(), false, console.LineSpacing)} ";
      }
      else
      {
        line = "                 ";
        colorLine = "                 ";
      }
      if (i < theme.Grey.Count)
      {
        colorLine += ToEffectText(theme.Grey[i].ToHexString(), true, console.LineSpacing) + " ";
        line += ToEffectText(theme.Grey[i].ToHexString(), false, console.LineSpacing) + " ";
      }
      else
      {
        line += "       ";
        colorLine += "       ";
      }
      if (i < theme.Red.Count)
      {
        colorLine += ToEffectText(theme.Red[i].ToHexString(), true, console.LineSpacing) + " ";
        line += ToEffectText(theme.Red[i].ToHexString(), false, console.LineSpacing) + " ";
      }
      else
      {
        line += "       ";
        colorLine += "       ";
      }
      if (i < theme.Yellow.Count)
      {
        line += ToEffectText(theme.Yellow[i].ToHexString(), false, console.LineSpacing) + " ";
        colorLine += ToEffectText(theme.Yellow[i].ToHexString(), true, console.LineSpacing) + " ";
      }
      else
      {
        line += "       ";
        colorLine += "       ";
      }
      if (i < theme.Green.Count)
      {
        line += ToEffectText(theme.Green[i].ToHexString(), false, console.LineSpacing) + " ";
        colorLine += ToEffectText(theme.Green[i].ToHexString(), true, console.LineSpacing) + " ";
      }
      else
      {
        line += "       ";
        colorLine += "       ";
      }
      if (i < theme.Blue.Count)
      {
        line += ToEffectText(theme.Blue[i].ToHexString(), false, console.LineSpacing) + " ";
        colorLine += ToEffectText(theme.Blue[i].ToHexString(), true, console.LineSpacing) + " ";
      }
      else
      {
        line += "       ";
        colorLine += "       ";
      }
      if (i < theme.Purple.Count)
      {
        line += ToEffectText(theme.Purple[i].ToHexString(), false, console.LineSpacing);
        colorLine += ToEffectText(theme.Purple[i].ToHexString(), true, console.LineSpacing);
      }
      console.Print(colorLine);
      console.Print(line);
      console.Print(colorLine);
    }
  }

  private static string ToEffectText(string hex, bool pureColor, int lineSpacing)
  {
    var colorWithoutAlpha = hex[0..7];
    if (pureColor)
    {
      return $"[        ](bgcolor={colorWithoutAlpha},{lineSpacing};color=white)";
    }
    else
    {
      var colorPureHex = hex[1..7];
      return $"[ {colorPureHex} ](bgcolor={colorWithoutAlpha},{lineSpacing};color=white)";
    }
  }
}
