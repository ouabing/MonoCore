using System;
using System.Collections.Generic;
using System.Linq;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace G;

public class GameConsole
{
  public static int Width => Core.Screen.DisplayWidth;
  public static int Height => Core.Screen.DisplayHeight;
  public int PaddingX { get; set; } = 20;
  public int PaddingY { get; set; } = 20;
  public int LineSpacing { get; set; } = 4;
  public int FontSize { get; set; } = 16;
  public int LineHeight => Font.LineHeight;
  private int LinesPerScreen => (Height - 2 * PaddingY) / (LineHeight + LineSpacing);
  public const int MaxHistoryLines = 100;
  public List<string> History { get; } = [];
  public bool IsEnabled { get; private set; }
  public float CursorBlinkDuration { get; set; } = 0.5f;
  public Dictionary<string, ConsoleCommand> Commands { get; } = [];
  private float CursorTimer;
  public Color TextColor { get; set; } = Palette.White;
  public Color BackgroundColor { get; set; } = Palette.Black * 0.5f;
  public Vector2 CursorSize => new(4, LineHeight);
  public Vector2 ViewCursorSize => new(FontSize, LineHeight);
  public Color CursorColor { get; set; } = Palette.White;
  public ConsoleLine CurrentInput { get; private set; }
  public string Completion { get; private set; } = "";
  public Color CompletionColor { get; set; } = Palette.Grey[4];
  public string[] WelcomeMessages { get; set; } = [
    "[nihao :)](color=Green5;osc=-2,2,5)",
    "help: show all commands",
    "exit: close the console",
  ];
  public bool InEvalMode { get; set; }
  public bool InViewMode { get; set; }
  public Vector2 ViewCursorPos { get; set; }
  public bool IsLoading => currentTask != null && currentTask.IsCompleted != true;
  private readonly List<ConsoleLine> HistoryLines = [];
  private int CursorX { get; set; }
  private string Prompt => InEvalMode ? ">> " : "> ";
  private SpriteFontBase Font => Core.Font.Get("unscii8", FontSize);
  private float PromptWidth => Font.MeasureString(Prompt.ToString()).X;
  private float FontWidth => Font.MeasureString("A").X;
  private readonly ConsoleKeyBuffer KeyBuffer = new();
  private System.Threading.Tasks.Task? currentTask;
  private readonly ConsoleIndicator Indicator = new();

  public void LoadContent()
  {
    foreach (var message in WelcomeMessages)
    {
      HistoryLines.Add(new ConsoleLine("", message, Palette.Green[4], Width - 2 * PaddingX, Font, true, LineHeight, LineSpacing));
    }
    CurrentInput = StartNewInputLine();
    Indicator.LoadContent();
    RegisterCommand(new ClearCommand());
    RegisterCommand(new DebugCommand());
    RegisterCommand(new EvalCommand());
    RegisterCommand(new ExitCommand());
    RegisterCommand(new FullscreenCommand());
    RegisterCommand(new HelpCommand());
    RegisterCommand(new PauseCommand());
    RegisterCommand(new ResumeCommand());
    RegisterCommand(new ScreenshotCommand());
  }

  public void RegisterCommand(ConsoleCommand command)
  {
    Commands[command.Name] = command;
    foreach (var alias in command.Aliases)
    {
      Commands[alias] = command;
    }
  }

  public void Update(GameTime gameTime)
  {
    if (Core.Input.IsKeyPressed(Keys.OemTilde))
    {
      var keyState = Keyboard.GetState();
      if (!keyState.IsKeyDown(Keys.LeftShift) && !keyState.IsKeyDown(Keys.RightShift))
      {
        if (IsEnabled)
        {
          Disable();
        }
        else
        {
          Enable();
        }
      }
    }
    if (!IsEnabled)
    {
      return;
    }

    foreach (var line in HistoryLines)
    {
      line.Update(gameTime);
    }

    if (IsLoading)
    {
      Indicator.Update(gameTime);
      return;
    }
    if (InViewMode)
    {
      UpdateViewModeInput(gameTime);
    }
    else
    {
      UpdateInput(gameTime);
    }
  }

  public void Draw(GameTime gameTime)
  {
    if (!IsEnabled)
    {
      return;
    }
    DrawBackground(gameTime);
    DrawText(gameTime);
    DrawCursor(gameTime);
    DrawLabel(gameTime);
  }

  public void Clear()
  {
    HistoryLines.Clear();
  }

  public void PrintSuccess(string text)
  {
    Print(text, Palette.Green[4]);
  }

  public void PrintError(string text)
  {
    Print(text, Palette.Red[4]);
  }

  public void Print(string text, Color color)
  {
    HistoryLines.Add(new ConsoleLine("", text, color, Width - 2 * PaddingX, Font, true, LineHeight, LineSpacing));
  }

  public void Print(string text)
  {
    HistoryLines.Add(new ConsoleLine("", text, TextColor, Width - 2 * PaddingX, Font, true, LineHeight, LineSpacing));
  }

  public void EnterEvalMode()
  {
    InEvalMode = true;
  }

  public void ExitEvalMode()
  {
    InEvalMode = false;
  }

  private void UpdateViewModeInput(GameTime gameTime)
  {
    var key = KeyBuffer.UpdateKeys(gameTime);
    var c = GetKeyChar(key);
    var cursorPos = GetCursorPosition();
    if (key == Keys.Up || c == "k")
    {
      ViewCursorPos = new Vector2(ViewCursorPos.X, Math.Max(PaddingY, ViewCursorPos.Y - LineHeight - LineSpacing));
    }
    else if (key == Keys.Down || c == "j")
    {
      ViewCursorPos = new Vector2(ViewCursorPos.X, Math.Min(cursorPos.Y, ViewCursorPos.Y + LineHeight + LineSpacing));
    }
    else if (key == Keys.Right || c == "l")
    {
      ViewCursorPos = new Vector2(Math.Min(Width - PaddingX, ViewCursorPos.X + FontWidth), ViewCursorPos.Y);
    }
    else if (key == Keys.Left || c == "h")
    {
      ViewCursorPos = new Vector2(Math.Max(PaddingX, ViewCursorPos.X - FontWidth), ViewCursorPos.Y);
    }
    else if (c == "i")
    {
      if (ViewCursorPos.Y == cursorPos.Y)
      {
        CursorX = ViewCursorPos.X < PaddingX + PromptWidth ? 0 : (int)((ViewCursorPos.X - PaddingX - PromptWidth) / FontWidth);
        if (CursorX > CurrentInput.Text.Length)
        {
          CursorX = CurrentInput.Text.Length;
        }
      }
      InViewMode = false;
    }
  }

  private void UpdateInput(GameTime gameTime)
  {
    var key = KeyBuffer.UpdateKeys(gameTime);
    var resetCursorTimer = key != Keys.None;

    if (key == Keys.Enter)
    {
      ExecuteLine();
    }
    else if (key == Keys.Tab)
    {
      if (!InEvalMode && Completion.Length > 0)
      {
        var newText = CurrentInput.Text + Completion[CurrentInput.Text.Length..];
        CurrentInput.UpdateText(newText);
        CursorX = CurrentInput.Text.Length;
      }
    }
    else if (key == Keys.Back)
    {
      if (CursorX > 0)
      {
        CursorX--;
        var text = CurrentInput.Text;
        var newText = text.Remove(CursorX, 1);
        CurrentInput.UpdateText(newText);
      }
    }
    else if (key == Keys.Left)
    {
      if (CursorX > 0)
      {
        CursorX--;
      }
    }
    else if (key == Keys.Right)
    {
      if (CursorX < CurrentInput.Text.Length)
      {
        CursorX++;
      }
    }
    else if (key == Keys.Up)
    {
      HistoryUp();
    }
    else if (key == Keys.Down)
    {
      HistoryDown();
    }
    else if (key == Keys.Escape)
    {
      EnterViewMode();
    }
    else if (key == Keys.OemTilde)
    {
      // do nothing
    }
    else
    {
      resetCursorTimer = false;
      var c = GetKeyChar(key);
      if (c.Length == 1)
      {

        resetCursorTimer = true;
        var newText = CurrentInput.Text.Insert(CursorX, c);
        CurrentInput.UpdateText(newText);
        CursorX++;
      }
    }

    if (resetCursorTimer)
    {
      CursorTimer = 0;
    }

    UpdateCompletion();
  }

  private void EnterViewMode()
  {
    InViewMode = true;
    ViewCursorPos = GetCursorPosition();
  }

  private void HistoryUp()
  {
    if (InEvalMode)
    {
      var command = Commands["eval"] as EvalCommand;
      var text = command?.HistoryUp(CurrentInput.Text);
      if (text != null)
      {
        CurrentInput.UpdateText(text);
        CursorX = CurrentInput.Text.Length;
      }
      return;
    }
    else
    {
      var text = HistoryUp(CurrentInput.Text);
      if (text != null)
      {
        CurrentInput.UpdateText(text);
        CursorX = CurrentInput.Text.Length;
      }
    }
  }

  private void HistoryDown()
  {
    if (InEvalMode)
    {
      var command = Commands["eval"] as EvalCommand;
      var text = command?.HistoryDown(CurrentInput.Text);
      if (text != null)
      {
        CurrentInput.UpdateText(text);
        CursorX = CurrentInput.Text.Length;
      }
      return;
    }
    else
    {
      var text = HistoryDown(CurrentInput.Text);
      if (text != null)
      {
        CurrentInput.UpdateText(text);
        CursorX = CurrentInput.Text.Length;
      }
    }
  }

  private void UpdateCompletion()
  {
    if (InEvalMode)
    {
      return;
    }
    var curText = CurrentInput.Text;
    var founds = Commands.Keys.Where((key) => key.StartsWith(curText));
    if (!founds.Any())
    {
      Completion = "";
      return;
    }
    var firstFound = founds.First();

    if (firstFound == null)
    {
      Completion = "";
    }
    else
    {
      Completion = firstFound;
    }
  }

  public void ExecuteLine()
  {
    HistoryLines.Add(CurrentInput);

    var input = CurrentInput.Text.Trim();
    if (input.Length == 0)
    {
      StartNewInputLine();
      return;
    }

    var parts = input.Split(' ');
    if (InEvalMode)
    {
      EvalCommand? evalCommand = Commands["eval"] as EvalCommand;
      currentTask = evalCommand?.Eval(this, input);
    }
    else
    {
      AddHistory(input);
      var commandName = parts[0].Trim();
      var command = Commands.GetValueOrDefault(commandName);
      if (command == null)
      {
        Print($"command not found: {commandName}", Palette.Red[4]);
      }
      else
      {
        var args = parts.Skip(1).Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
        command.Execute(this, args);
      }
    }

    if (IsLoading)
    {
      Indicator.Reset();
    }

    CurrentInput.EnableEffect();

    StartNewInputLine();
  }

  private void AddHistory(string expression)
  {
    if (History.Count >= MaxHistoryLines)
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

  public void Enable()
  {
    IsEnabled = true;
    Core.Input.Disable();
  }

  public void Disable()
  {
    IsEnabled = false;
    Core.Input.Enable();
  }

  private void DrawBackground(GameTime gameTime)
  {
    Rectangle backgroundRect = new(0, 0, Width, Height);
    Core.Sb.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Core.Screen.Transform);
    Core.Sb.DrawRectangle(backgroundRect, TextColor);
    Core.Sb.FillRectangle(backgroundRect, BackgroundColor);
    Core.Sb.End();
  }

  private void DrawText(GameTime gameTime)
  {
    var x = PaddingX;
    var y = PaddingY;
    var offsetY = CalcScrollOffset();
    y -= offsetY * (LineHeight + LineSpacing);
    Core.Sb.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Core.Screen.Transform);
    foreach (var line in HistoryLines)
    {
      DrawLine(gameTime, line, x, ref y);
    }

    if (IsLoading)
    {
      Indicator.Draw(gameTime, FontSize, x, y);
    }
    else
    {
      DrawLine(gameTime, CurrentInput, x, ref y);
      DrawCompletion();
    }
    Core.Sb.End();
  }

  private int CalcScrollOffset()
  {
    var lines = 0;
    foreach (var line in HistoryLines)
    {
      lines += line.WrappedLines.Count;
    }
    var offset = lines - LinesPerScreen;
    if (offset < 0)
    {
      offset = 0;
    }
    if (InViewMode)
    {
      var cursorLineIndex = (int)ViewCursorPos.Y / (LineHeight + LineSpacing);

      if (cursorLineIndex < offset)
      {
        offset = cursorLineIndex;
      }
      else if (cursorLineIndex >= offset + LinesPerScreen)
      {
        offset = cursorLineIndex - LinesPerScreen + 1;
      }
    }
    return offset;
  }

  private void DrawCompletion()
  {
    if (InEvalMode)
    {
      return;
    }
    var cursorPos = GetCursorPosition();
    // Only show completion when cursor is at the end
    if (CursorX != CurrentInput.Text.Length)
    {
      return;
    }

    if (CurrentInput.Text.Length == 0 || Completion.Length == 0)
    {
      return;
    }

    if (!Completion.StartsWith(CurrentInput.Text))
    {
      return;
    }

    var diff = Completion[CurrentInput.Text.Length..];

    if (diff.Length == 0)
    {
      return;
    }

    Font.DrawText(Core.Sb, diff, new Vector2(cursorPos.X, cursorPos.Y), CompletionColor);

  }

  private void DrawLine(GameTime gameTime, ConsoleLine line, int x, ref int y)
  {
    line.Draw(gameTime, x, ref y);
  }

  private void DrawCursor(GameTime gameTime)
  {
    if (IsLoading)
    {
      return;
    }

    CursorTimer += gameTime.GetElapsedSeconds();
    var show = true;
    if (CursorTimer > 2 * CursorBlinkDuration)
    {
      CursorTimer = 0;
    }
    else if (CursorTimer > CursorBlinkDuration)
    {
      show = false;
    }
    if (!show)
    {
      return;
    }

    var cursorPos = InViewMode ? ViewCursorPos : GetCursorPosition();
    var size = InViewMode ? ViewCursorSize : CursorSize;
    RectangleF cursorRect = new(cursorPos.X, cursorPos.Y, size.X, size.Y);
    Core.Sb.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Core.Screen.Transform);
    Core.Sb.FillRectangle(cursorRect, CursorColor);
    Core.Sb.End();
  }

  private void DrawLabel(GameTime gameTime)
  {
    if (InViewMode)
    {
      Core.Sb.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Core.Screen.Transform);
      var label = "VIEW MODE";
      var labelSize = Font.MeasureString(label);
      Core.Sb.FillRectangle(new Rectangle(Width - PaddingX - (int)labelSize.X - 1, Height - PaddingY - LineHeight - 2, (int)labelSize.X + 2, LineHeight + 4), Palette.White);
      Font.DrawText(Core.Sb, label, new Vector2(Width - PaddingX - labelSize.X, Height - PaddingY - LineHeight), Palette.Black);
      Core.Sb.End();
    }
  }


  private static string GetKeyChar(Keys key)
  {
    KeyboardState keyboardState = Keyboard.GetState();
    bool isShiftPressed = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);

    if (key >= Keys.A && key <= Keys.Z)
    {
      char letter = (char)('A' + (key - Keys.A));
      return isShiftPressed ? letter.ToString() : letter.ToString().ToLower();
    }

    if (key >= Keys.D0 && key <= Keys.D9)
    {
      string[] shiftNumbers = [")", "!", "@", "#", "$", "%", "^", "&", "*", "("];
      int number = key - Keys.D0;
      return isShiftPressed ? shiftNumbers[number] : number.ToString();
    }

    if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
    {
      int number = key - Keys.NumPad0;
      return number.ToString();
    }

    return key switch
    {
      Keys.Space => " ",
      Keys.OemPeriod => isShiftPressed ? ">" : ".",
      Keys.OemComma => isShiftPressed ? "<" : ",",
      Keys.OemQuestion => isShiftPressed ? "?" : "/",
      Keys.OemSemicolon => isShiftPressed ? ":" : ";",
      Keys.OemQuotes => isShiftPressed ? "\"" : "'",
      Keys.OemOpenBrackets => isShiftPressed ? "{" : "[",
      Keys.OemCloseBrackets => isShiftPressed ? "}" : "]",
      Keys.OemMinus => isShiftPressed ? "_" : "-",
      Keys.OemPlus => isShiftPressed ? "+" : "=",
      Keys.OemPipe => isShiftPressed ? "|" : "\\",
      Keys.OemTilde => isShiftPressed ? "~" : "`",
      _ => string.Empty,
    };
  }

  private ConsoleLine StartNewInputLine()
  {
    CurrentInput = new ConsoleLine(Prompt, "", TextColor, Width - 2 * PaddingX, Font, false, LineHeight, LineSpacing);
    CursorX = 0;
    return CurrentInput;
  }

  private Vector2 GetCursorPosition()
  {
    float y = 0;
    foreach (var line in HistoryLines)
    {
      y += line.WrappedLines.Count;
    }
    y += CurrentInput.WrappedLines.Count - 1;

    float x = CursorX;

    var hasLineBreak = false;
    for (int i = 0; i < CurrentInput.WrappedLines.Count - 1; i++)
    {
      x -= CurrentInput.WrappedLines[i].Length - 1;
      hasLineBreak = true;
    }

    y -= CalcScrollOffset();
    x = PaddingX + (hasLineBreak ? 0 : PromptWidth) + x * Font.MeasureString("A").X;
    y = PaddingY + y * (LineHeight + LineSpacing);

    return new Vector2(x, y);
  }
}