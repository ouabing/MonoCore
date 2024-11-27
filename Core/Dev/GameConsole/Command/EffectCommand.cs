namespace G;

public class EffectCommand : ConsoleCommand
{
  public EffectCommand() : base(
    "effect",
    "Run onetime effect\n\n" +
    "available effects:\n" +
    "  vhs: toggle VHS shader\n" +
    "  pixelation: run pixelation shader animation\n\n",
    "effect pixelation [-q | --quit]",
    []
  )
  {
  }

  public override void Execute(GameConsole console, string[] args)
  {
    if (args.Length > 2)
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

    if (args.Length == 2)
    {
      if (args[1] == "-q" || args[1] == "--quit")
      {
        console.Disable();
      }
      else
      {
        PrintArgumentError(console, args[1]);
      }
    }

    switch (args[0].ToLower())
    {
      case "vhs":
        if (Core.Effect.IsVHSEffectActive)
        {
          Core.Effect.DisableVHS();
        }
        else
        {
          Core.Effect.EnableVHS();
        }
        return;
      case "pixelation":
        Core.Effect.Pixelate();
        return;
      default:
        PrintArgumentError(console, args[0]);
        return;
    }
  }
}
