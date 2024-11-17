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
    PrintArgumentError(console, args[0]);
  }

  private void PrintTheme(GameConsole console, ITheme theme)
  {
    console.Print($"{theme.Name}");
    console.Print($"White:  {ToEffectText(theme.White.ToHexString())}");
    console.Print($"Black:  {ToEffectText(theme.Black.ToHexString())}");
    var line = "Grey:   ";
    for (int i = 0; i < theme.Grey.Count; i++)
    {
      line += ToEffectText(theme.Grey[i].ToHexString()) + " ";
    }
    console.Print(line);
    line = "Red:    ";
    for (int i = 0; i < theme.Red.Count; i++)
    {
      line += ToEffectText(theme.Red[i].ToHexString()) + " ";
    }
    console.Print(line);
    line = "Yellow: ";
    for (int i = 0; i < theme.Yellow.Count; i++)
    {
      line += ToEffectText(theme.Yellow[i].ToHexString()) + " ";
    }
    console.Print(line);
    line = "Green:  ";
    for (int i = 0; i < theme.Green.Count; i++)
    {
      line += ToEffectText(theme.Green[i].ToHexString()) + " ";
    }
    console.Print(line);
    line = "Blue:   ";
    for (int i = 0; i < theme.Blue.Count; i++)
    {
      line += ToEffectText(theme.Blue[i].ToHexString()) + " ";
    }
    console.Print(line);
    line = "Purple: ";
    for (int i = 0; i < theme.Purple.Count; i++)
    {
      line += ToEffectText(theme.Purple[i].ToHexString()) + " ";
    }
    console.Print(line);
  }

  private static string ToEffectText(string hex)
  {
    var colorWithoutAlpha = hex[..7];
    return $"[{colorWithoutAlpha}](bgcolor={colorWithoutAlpha};color=white)";
  }
}
