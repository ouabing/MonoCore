using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace G;

public class InputManager
{
  public bool LeftClicked { get; set; }
  public bool RightClicked { get; set; }
  public bool LeftDown { get; set; }
  public List<Def.Input.World> WorldQueue { get; } = [];
  private readonly Dictionary<Def.Input.Action, bool> Activated = [];

  private MouseState previousMouse;
  private KeyboardState previousKeyboard;

#pragma warning disable CA1822 // Mark members as static
  public void LoadContent()
  {
    try
    {
      var Texture = Core.Texture.LoadTexture("Cursor");
      Mouse.SetCursor(MouseCursor.FromTexture2D(Texture, 0, 0));
    }
    catch
    {
      Debug.WriteLine("Cursor texture not found in Content/Cursor.png, using default cursor", "MonoCore");
    }
  }
#pragma warning restore CA1822 // Mark members as static

  /*
   * Push and allow a defined world to receive input events
   */
  public void PushWorld(Def.Input.World world)
  {
    // Clear the activated actions to avoid remaining input event consuming from the previous world
    Activated.Clear();
    WorldQueue.Add(world);
  }

  public void PopWorld()
  {
    if (WorldQueue.Count > 0)
    {
      // Clear the activated actions to avoid remaining input event consuming from the previous world
      WorldQueue.RemoveAt(WorldQueue.Count - 1);
    }
    else
    {
      throw new InvalidOperationException("No input world is active");
    }
  }

  public bool IsActive(Def.Input.Action action, Def.Input.World world)
  {
    if (WorldQueue.Count == 0)
    {
      throw new InvalidOperationException("No input world is active");
    }
    if (world != WorldQueue.Last())
    {
      return false;
    }
    return Activated.ContainsKey(action);
  }

  /*
   * Consume an action to avoid propagating the same input to any other listeners
   * Useful for scenarios like, a UI button is on top of a game object,
   * when the button is clicked, the game object should not receive the click event
   */
  public bool Consume(Def.Input.Action action, Def.Input.World world)
  {
    if (!IsActive(action, world))
    {
      return false;
    }
    var toRemoveAction = new List<Def.Input.Action>();
    var keys = Def.Input.Bindings[action];
    foreach (var activatedAction in Activated.Keys)
    {
      if (Def.Input.Bindings[activatedAction].Intersect(keys).Any())
      {
        toRemoveAction.Add(activatedAction);
      }
    }
    foreach (var a in toRemoveAction)
    {
      Activated.Remove(a);
    }
    return true;
  }

  public void Update(GameTime gameTime)
  {
    Activated.Clear();
    foreach (var binding in Def.Input.Bindings)
    {
      var action = binding.Key;
      var keys = binding.Value;
      foreach (var key in keys)
      {
        if (IsKeyPressed(key))
        {
          Activated[action] = true;
          break;
        }
      }
    }
    previousMouse = Mouse.GetState();
    previousKeyboard = Keyboard.GetState();
  }

  public bool Hover(Rectangle r)
  {
    return r.Contains(new Vector2(previousMouse.X, previousMouse.Y));
  }

  private bool IsKeyPressed(string key)
  {
    var mouse = Mouse.GetState();
    var gamepad = GamePad.GetState(PlayerIndex.One);
    switch (key)
    {
      case "D1":
        return Keyboard.GetState().IsKeyDown(Keys.D1);
      case "D1_Pressed":
        return IsKeyPressed(Keys.D1);
      case "D2":
        return Keyboard.GetState().IsKeyDown(Keys.D2);
      case "D2_Pressed":
        return IsKeyPressed(Keys.D2);
      case "D3":
        return Keyboard.GetState().IsKeyDown(Keys.D3);
      case "D3_Pressed":
        return IsKeyPressed(Keys.D3);
      case "D4":
        return Keyboard.GetState().IsKeyDown(Keys.D4);
      case "D4_Pressed":
        return IsKeyPressed(Keys.D4);
      case "D5":
        return Keyboard.GetState().IsKeyDown(Keys.D5);
      case "D5_Pressed":
        return IsKeyPressed(Keys.D5);
      case "D6":
        return Keyboard.GetState().IsKeyDown(Keys.D6);
      case "D6_Pressed":
        return IsKeyPressed(Keys.D6);
      case "D7":
        return Keyboard.GetState().IsKeyDown(Keys.D7);
      case "D7_Pressed":
        return IsKeyPressed(Keys.D7);
      case "D8":
        return Keyboard.GetState().IsKeyDown(Keys.D8);
      case "D8_Pressed":
        return IsKeyPressed(Keys.D8);
      case "D9":
        return Keyboard.GetState().IsKeyDown(Keys.D9);
      case "D9_Pressed":
        return IsKeyPressed(Keys.D9);
      case "A":
        return Keyboard.GetState().IsKeyDown(Keys.A);
      case "A_Pressed":
        return IsKeyPressed(Keys.A);
      case "B":
        return Keyboard.GetState().IsKeyDown(Keys.B);
      case "B_Pressed":
        return IsKeyPressed(Keys.B);
      case "C":
        return Keyboard.GetState().IsKeyDown(Keys.C);
      case "C_Pressed":
        return IsKeyPressed(Keys.C);
      case "D":
        return Keyboard.GetState().IsKeyDown(Keys.D);
      case "D_Pressed":
        return IsKeyPressed(Keys.D);
      case "E":
        return Keyboard.GetState().IsKeyDown(Keys.E);
      case "E_Pressed":
        return IsKeyPressed(Keys.E);
      case "F":
        return Keyboard.GetState().IsKeyDown(Keys.F);
      case "F_Pressed":
        return IsKeyPressed(Keys.F);
      case "G":
        return Keyboard.GetState().IsKeyDown(Keys.G);
      case "G_Pressed":
        return IsKeyPressed(Keys.G);
      case "H":
        return Keyboard.GetState().IsKeyDown(Keys.H);
      case "H_Pressed":
        return IsKeyPressed(Keys.H);
      case "I":
        return Keyboard.GetState().IsKeyDown(Keys.I);
      case "I_Pressed":
        return IsKeyPressed(Keys.I);
      case "J":
        return Keyboard.GetState().IsKeyDown(Keys.J);
      case "J_Pressed":
        return IsKeyPressed(Keys.J);
      case "K":
        return Keyboard.GetState().IsKeyDown(Keys.K);
      case "K_Pressed":
        return IsKeyPressed(Keys.K);
      case "L":
        return Keyboard.GetState().IsKeyDown(Keys.L);
      case "L_Pressed":
        return IsKeyPressed(Keys.L);
      case "M":
        return Keyboard.GetState().IsKeyDown(Keys.M);
      case "M_Pressed":
        return IsKeyPressed(Keys.M);
      case "N":
        return Keyboard.GetState().IsKeyDown(Keys.N);
      case "N_Pressed":
        return IsKeyPressed(Keys.N);
      case "O":
        return Keyboard.GetState().IsKeyDown(Keys.O);
      case "O_Pressed":
        return IsKeyPressed(Keys.O);
      case "P":
        return Keyboard.GetState().IsKeyDown(Keys.P);
      case "P_Pressed":
        return IsKeyPressed(Keys.P);
      case "Q":
        return Keyboard.GetState().IsKeyDown(Keys.Q);
      case "Q_Pressed":
        return IsKeyPressed(Keys.Q);
      case "R":
        return Keyboard.GetState().IsKeyDown(Keys.R);
      case "R_Pressed":
        return IsKeyPressed(Keys.R);
      case "S":
        return Keyboard.GetState().IsKeyDown(Keys.S);
      case "S_Pressed":
        return IsKeyPressed(Keys.S);
      case "T":
        return Keyboard.GetState().IsKeyDown(Keys.T);
      case "T_Pressed":
        return IsKeyPressed(Keys.T);
      case "U":
        return Keyboard.GetState().IsKeyDown(Keys.U);
      case "U_Pressed":
        return IsKeyPressed(Keys.U);
      case "V":
        return Keyboard.GetState().IsKeyDown(Keys.V);
      case "V_Pressed":
        return IsKeyPressed(Keys.V);
      case "W":
        return Keyboard.GetState().IsKeyDown(Keys.W);
      case "W_Pressed":
        return IsKeyPressed(Keys.W);
      case "X":
        return Keyboard.GetState().IsKeyDown(Keys.X);
      case "X_Pressed":
        return IsKeyPressed(Keys.X);
      case "Y":
        return Keyboard.GetState().IsKeyDown(Keys.Y);
      case "Y_Pressed":
        return IsKeyPressed(Keys.Y);
      case "Z":
        return Keyboard.GetState().IsKeyDown(Keys.Z);
      case "Z_Pressed":
        return IsKeyPressed(Keys.Z);
      case "Space":
        return Keyboard.GetState().IsKeyDown(Keys.Space);
      case "SpacePressed":
        return IsKeyPressed(Keys.Space);
      case "Enter":
        return Keyboard.GetState().IsKeyDown(Keys.Enter);
      case "EnterPressed":
        return IsKeyPressed(Keys.Enter);
      case "Escape":
        return Keyboard.GetState().IsKeyDown(Keys.Escape);
      case "EscapePressed":
        return IsKeyPressed(Keys.Escape);
      case "Left":
        return Keyboard.GetState().IsKeyDown(Keys.Left);
      case "LeftPressed":
        return IsKeyPressed(Keys.Left);
      case "Right":
        return Keyboard.GetState().IsKeyDown(Keys.Right);
      case "RightPressed":
        return IsKeyPressed(Keys.Right);
      case "Up":
        return Keyboard.GetState().IsKeyDown(Keys.Up);
      case "UpPressed":
        return IsKeyPressed(Keys.Up);
      case "Down":
        return Keyboard.GetState().IsKeyDown(Keys.Down);
      case "DownPressed":
        return IsKeyPressed(Keys.Down);
      case "MouseLeftPressed":
        return previousMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;
      case "MouseLeftDown":
        return mouse.LeftButton == ButtonState.Pressed;
      case "MouseLeftUp":
        return mouse.LeftButton == ButtonState.Released;
      case "MouseLeftReleased":
        return previousMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton != ButtonState.Pressed;
      case "MouseRightPressed":
        return previousMouse.RightButton == ButtonState.Pressed && mouse.RightButton == ButtonState.Released;
      case "MouseRightDown":
        return mouse.RightButton == ButtonState.Pressed;
      case "MouseRightUp":
        return mouse.RightButton == ButtonState.Released;
      case "MouseRightReleased":
        return previousMouse.RightButton == ButtonState.Pressed && mouse.RightButton != ButtonState.Pressed;
      case "StickLeftX-":
        return gamepad.IsConnected && gamepad.ThumbSticks.Left.X < -0.5f;
      case "StickLeftX+":
        return gamepad.IsConnected && gamepad.ThumbSticks.Left.X > 0.5f;
      case "StickLeftY-":
        return gamepad.IsConnected && gamepad.ThumbSticks.Left.Y < 0.5f;
      case "StickLeftY+":
        return gamepad.IsConnected && gamepad.ThumbSticks.Left.Y > 0.5f;
      case "StickRightX-":
        return gamepad.IsConnected && gamepad.ThumbSticks.Right.X < 0.5f;
      case "StickRightX+":
        return gamepad.IsConnected && gamepad.ThumbSticks.Right.X > 0.5f;
      case "StickRightY-":
        return gamepad.IsConnected && gamepad.ThumbSticks.Right.Y < 0.5f;
      case "StickRightY+":
        return gamepad.IsConnected && gamepad.ThumbSticks.Right.Y > 0.5f;
      case "ButtonAPressed":
        return gamepad.IsConnected && gamepad.Buttons.A == ButtonState.Pressed;
      case "ButtonBPressed":
        return gamepad.IsConnected && gamepad.Buttons.B == ButtonState.Pressed;
      case "ButtonXPressed":
        return gamepad.IsConnected && gamepad.Buttons.X == ButtonState.Pressed;
      case "ButtonYPressed":
        return gamepad.IsConnected && gamepad.Buttons.Y == ButtonState.Pressed;
    }
    throw new NotImplementedException($"Key {key} not implemented");
  }

  private bool IsKeyPressed(Keys key)
  {
    return previousKeyboard.IsKeyDown(key) && Keyboard.GetState().IsKeyUp(key);
  }

#pragma warning disable CA1822 // Mark members as static
  public Vector2 CursorScreenPosition
  {
    get
    {
      var scale = Core.GraphicsDevice.Viewport.Width / Core.ScreenWidth;
      return Mouse.GetState().Position.ToVector2() / scale;
    }
  }

  public Vector2 CursorWorldPosition
  {
    get
    {
      return Core.Camera.ScreenToWorld(CursorScreenPosition);
    }
  }
#pragma warning restore CA1822 // Mark members as static

  public Vector2 PreviousCursorScreenPosition
  {
    get
    {
      var scale = Core.GraphicsDevice.Viewport.Width / Core.ScreenWidth;
      return previousMouse.Position.ToVector2() / scale;
    }
  }

  public Vector2 PreviousCursorWorldPosition
  {
    get
    {
      return Core.Camera.PreviousScreenToWorld(PreviousCursorScreenPosition);
    }
  }

  public bool IsCursorValid()
  {
    var cp = CursorScreenPosition;
    if (cp.X < 0 || cp.X > Core.ScreenWidth || cp.Y < 0 || cp.Y > Core.ScreenHeight)
    {
      return false;
    }
    return true;
  }

  public bool IsCursorIn(RectangleF rect)
  {
    if (!IsCursorValid())
    {
      return false;
    }
    return rect.Contains(CursorScreenPosition);
  }

  public bool IsCursorIn(Rectangle rect)
  {
    return IsCursorIn((RectangleF)rect);
  }

  public bool IsCursorIn(Vector2 position, Vector2 size)
  {
    var rect = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
    return IsCursorIn(rect);
  }
}