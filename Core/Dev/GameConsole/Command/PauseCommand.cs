namespace G;

public class PauseCommand : ConsoleCommand
{
  public PauseCommand() : base("pause", "Pause the game loop", "pause", [])
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
      Core.Paused = true;
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
