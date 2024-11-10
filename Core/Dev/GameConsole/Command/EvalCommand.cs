using System;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace G;

public class EvalCommand : ConsoleCommand
{
  private ScriptState<object>? state;
  public EvalCommand() : base("eval", "Evaluate any C# expression", "eval [expression]", [])
  {
  }

  public async System.Threading.Tasks.Task Eval(GameConsole console, string expression)
  {
    expression = expression.Trim();
    if (expression.Length == 0)
    {
      return;
    }
    if (expression.ToLower() == "exit")
    {
      state = null;
      console.ExitEvalMode();
      return;
    }

    try
    {
      if (state == null)
      {
        var assembly = await GetExecutingAssemblyAsync();
        state = await CSharpScript.RunAsync(
          expression,
          ScriptOptions.Default
                       .WithReferences(assembly)
                       .WithImports("System", "G")
        );
      }
      else
      {
        state = await state.ContinueWithAsync(expression);
      }
      console.Print(state?.ReturnValue?.ToString() ?? "");
    }
    catch (CompilationErrorException e)
    {
      console.PrintError(string.Join(Environment.NewLine, e.Diagnostics));
    }
  }

  private async System.Threading.Tasks.Task<Assembly> GetExecutingAssemblyAsync()
  {
    return await System.Threading.Tasks.Task.Run(() => Assembly.GetExecutingAssembly());
  }

  public override async void Execute(GameConsole console, string[] args)
  {

    var expression = string.Join(" ", args).Trim();

    if (expression.Length == 0)
    {
      // enter interactive mode
      console.Print("entering interactive mode. type 'exit' to exit.");
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
      var result = await CSharpScript.EvaluateAsync(
        expression,
        ScriptOptions.Default
                     .WithReferences(Assembly.GetExecutingAssembly())
                     .WithImports("System", "G")
      );
      console.Print(result?.ToString() ?? "");
    }
    catch (CompilationErrorException e)
    {
      console.PrintError(string.Join(Environment.NewLine, e.Diagnostics));
    }
  }
}
