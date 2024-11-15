namespace G;

public class FullscreenCommand : ConsoleCommand
{
  public FullscreenCommand() : base("fullscreen", "Toggle fullscreen", "fullscreen", [])
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
      if (Core.Graphics!.IsFullScreen)
      {
        Core.Screen.SetWindow();
      }
      else
      {
        Core.Screen.SetFullscreen();
      }
      return;
    }
    if (args[0] == "--help")
    {
      PrintHelp(console);
      return;
    }
    PrintArgumentError(console, args[0]);
  }
}
