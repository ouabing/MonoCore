namespace G;

public class ClearCommand : ConsoleCommand
{
  public ClearCommand() : base("clear", "Clear console history", "clear", [])
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
      console.Clear();
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