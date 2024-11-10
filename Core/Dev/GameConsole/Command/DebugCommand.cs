namespace G;

public class DebugCommand : ConsoleCommand
{
  public DebugCommand() : base("debug", "Display realtime debug info", "debug", [])
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
      Core.EnableDebug = !Core.EnableDebug;
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
