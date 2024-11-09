namespace G;

public abstract class ConsoleCommand(string name, string description, string usage, string[] aliases)
{
  public string Name { get; } = name;
  public string Description { get; } = description;
  public string Usage { get; } = usage;
  public string[] Aliases { get; } = aliases;
  public bool IsEnabled { get; set; } = true;

  public abstract void Execute(GameConsole console, string[] args);

  public void PrintHelp(GameConsole console)
  {
    console.Print($"usage: {Usage}");
    if (Description.Length > 0)
    {
      console.Print("");
      console.Print(Description);
    }
  }

  public void PrintArgumentError(GameConsole console, string argument)
  {
    console.PrintError($"{Name}: invalid argument: {argument}");
  }

  public static void PrintInvalidNumberOfArgumentsError(GameConsole console)
  {
    console.PrintError("resume: invalid number of arguments");
  }
}