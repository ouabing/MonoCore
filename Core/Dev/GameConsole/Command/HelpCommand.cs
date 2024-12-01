using System.Collections.Generic;
using System.Linq;

namespace G;

public class HelpCommand : ConsoleCommand
{
  public HelpCommand() : base("help", "Display a list of available commands", "help", [])
  {
  }

  public override void Execute(GameConsole console, string[] args)
  {
    if (args.Length > 1)
    {
      console.PrintError("help: invalid number of arguments");
      return;
    }

    if (args.Length == 0)
    {
      console.Print("");
      console.Print("[Commands:](color=white)");
      var longestName = console.Commands.Keys.Max(x => x.Length);
      var nameLength = longestName + 4;
      foreach (var c in console.Commands.Values)
      {
        var blanks = new string(' ', nameLength - c.Name.Length);
        var shortDesc = c.Description.Split("\n").First();
        console.Print($"  [{c.Name}](color=white;bgcolor=blue1){blanks}[{shortDesc}](color=white)");
      }
      console.Print("");
      return;
    }

    var name = args[0];
    var command = console.Commands.GetValueOrDefault(name);
    if (command == null)
    {
      console.Print($"help: command not found: {name}");
      return;
    }

    PrintHelp(console);
  }
}