namespace G;

public class ResumeCommand : ConsoleCommand
{
  public ResumeCommand() : base("resume", "Resume the game loop", "resume", [])
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
      Core.Paused = false;
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
