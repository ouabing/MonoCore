using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace G;

public class EvalCommand : ConsoleCommand
{
  private ScriptState<object>? state;
  public const int MaxHistory = 100;
  public List<string> History { get; } = [];
  public EvalCommand() : base("eval", "Enter interactive mode or directly evaluate any C# expression", "eval [expression]", [])
  {
  }

  private void AddHistory(string expression)
  {
    if (History.Count >= MaxHistory)
    {
      History.RemoveAt(0);
    }
    History.Add(expression);
  }

  public string? HistoryUp(string current)
  {
    if (History.Count == 0)
    {
      return null;
    }

    var index = History.FindLastIndex(x => x == current);
    if (index == 0)
    {
      return null;
    }
    if (index == -1)
    {
      return History[History.Count - 1];
    }
    return History[index - 1];
  }

  public string? HistoryDown(string current)
  {
    if (History.Count == 0)
    {
      return null;
    }
    var index = History.FindLastIndex(x => x == current);

    if (index == -1)
    {
      return null;
    }
    if (index == History.Count - 1)
    {
      return "";
    }
    return History[index + 1];
  }

  public async System.Threading.Tasks.Task Eval(GameConsole console, string expression)
  {
    expression = expression.Trim();
    if (expression.Length == 0)
    {
      return;
    }
    AddHistory(expression);

    var parts = expression.Split(' ');
    var first = parts[0].ToLower().Trim();
    var rest = parts.Length > 1 ? string.Join(" ", parts[1..]) : "";

    try
    {
      switch (first)
      {
        case "exit":
          state = null;
          console.ExitEvalMode();
          return;
        case "help":
          console.Print("exit                                         Exit interactive mode");
          console.Print("print                                        Print expression result");
          console.Print("watch [component]                            Watch a component for inspection");
          console.Print("watch+ HP (Component c) => (c as Enemy).HP   Add a custom field to the inspector, see InspectorRow");
          console.Print("watch- HP                                    Remove a custom field from the inspector");
          return;
        case "watch":
          await RunScript(console, rest);
          if (state.ReturnValue is not Component)
          {
            console.PrintError("watch can only be used with Component types");
          }
          else
          {
            Core.Inspector.Watch(state.ReturnValue as Component);
            console.Print($"start watching {state.ReturnValue.GetType().Name} in the inspector");
          }
          break;
        case "print":
          await RunScript(console, rest);

          var restResultString = state?.ReturnValue?.ToString();
          if (restResultString != null)
          {
            console.Print(restResultString);
          }
          break;
        case "watch+":
          var name = rest.Split(' ')[0].Trim();
          rest = rest.Substring(name.Length).Trim();
          await RunScript(console, rest);
          Core.Inspector.AddRow(new InspectorRow(name, state.ReturnValue as Func<Component, string>));
          break;
        case "watch-":
          var nameToRemove = rest.Split(' ')[0].Trim();
          Core.Inspector.RemoveRow(nameToRemove);
          break;
        default:
          await RunScript(console, expression);

          var resultString = state?.ReturnValue?.ToString();
          if (resultString != null)
          {
            console.Print(resultString);
          }
          break;
      }
    }
    catch (CompilationErrorException e)
    {
      console.PrintError(string.Join(Environment.NewLine, e.Diagnostics));
    }
  }

  private async System.Threading.Tasks.Task RunScript(GameConsole console, string expression)
  {
    if (state == null)
    {
      var options = await GetDefaultScriptOptionsAsync();
      state = await CSharpScript.RunAsync(
        expression,
        options
      );
    }
    else
    {
      state = await state.ContinueWithAsync(expression);
    }
  }

  private static async System.Threading.Tasks.Task<Assembly> GetExecutingAssemblyAsync()
  {
    return await System.Threading.Tasks.Task.Run(() => Assembly.GetExecutingAssembly());
  }

  private static async System.Threading.Tasks.Task<ScriptOptions> GetDefaultScriptOptionsAsync()
  {
    var assembly = await GetExecutingAssemblyAsync();
    return ScriptOptions.Default.WithReferences(assembly)
                                .WithImports(
                                  "System",
                                  "G",
                                  "Microsoft.Xna.Framework",
                                  "MonoGame.Extended",
                                  "System.Linq",
                                  "System.Collections.Generic"
                                );
  }

  public override async void Execute(GameConsole console, string[] args)
  {

    var expression = string.Join(" ", args).Trim();

    if (expression.Length == 0)
    {
      // enter interactive mode
      console.Print("entering interactive mode. type [help](bgcolor=blue1)  to see available commands.", Palette.ConsoleTheme.Yellow[5]);
      console.EnterEvalMode();
      return;
    }
    if (args[0] == "--help")
    {
      PrintHelp(console);
      return;
    }
    try
    {
      var options = await GetDefaultScriptOptionsAsync();
      var result = await CSharpScript.EvaluateAsync(
        expression,
        options
      );
      var resultString = result?.ToString();
      if (resultString != null)
      {
        console.Print(resultString);
      }
    }
    catch (CompilationErrorException e)
    {
      console.PrintError(string.Join(Environment.NewLine, e.Diagnostics));
    }
  }
}
