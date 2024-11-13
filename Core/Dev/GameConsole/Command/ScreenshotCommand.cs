using System;

namespace G;

public class ScreenshotCommand : ConsoleCommand
{
  public ScreenshotCommand() : base("screenshot", "Take a screenshot of current frame", "screenshot /path/to/folder/", [])
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
      PrintHelp(console);
      return;
    }
    if (args[0] == "--help")
    {
      PrintHelp(console);
      return;
    }

    try
    {
      var path = Core.Layer.TakeScreenshot(args[0]);
      console.Print($"screenshot saved: {path}");
    }
    catch (Exception e)
    {
      console.PrintError($"screenshot: {e.Message}");
    }
  }
}
