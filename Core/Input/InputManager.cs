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
  public bool IsDisabled { get; private set; }
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

  // Test if input is activated in any world
  public bool IsActive(Def.Input.Action action)
  {
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

  public void Disable()
  {
    IsDisabled = true;
  }

  public void Enable()
  {
    IsDisabled = false;
  }

  public void Update(GameTime gameTime)
  {
    Activated.Clear();

    if (IsDisabled)
    {
      return;
    }
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
  }

  public void PostUpdate(GameTime gameTime)
  {
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
    return key switch
    {
      "D1" => Keyboard.GetState().IsKeyDown(Keys.D1),
      "D1_Pressed" => IsKeyPressed(Keys.D1),
      "D2" => Keyboard.GetState().IsKeyDown(Keys.D2),
      "D2_Pressed" => IsKeyPressed(Keys.D2),
      "D3" => Keyboard.GetState().IsKeyDown(Keys.D3),
      "D3_Pressed" => IsKeyPressed(Keys.D3),
      "D4" => Keyboard.GetState().IsKeyDown(Keys.D4),
      "D4_Pressed" => IsKeyPressed(Keys.D4),
      "D5" => Keyboard.GetState().IsKeyDown(Keys.D5),
      "D5_Pressed" => IsKeyPressed(Keys.D5),
      "D6" => Keyboard.GetState().IsKeyDown(Keys.D6),
      "D6_Pressed" => IsKeyPressed(Keys.D6),
      "D7" => Keyboard.GetState().IsKeyDown(Keys.D7),
      "D7_Pressed" => IsKeyPressed(Keys.D7),
      "D8" => Keyboard.GetState().IsKeyDown(Keys.D8),
      "D8_Pressed" => IsKeyPressed(Keys.D8),
      "D9" => Keyboard.GetState().IsKeyDown(Keys.D9),
      "D9_Pressed" => IsKeyPressed(Keys.D9),
      "A" => Keyboard.GetState().IsKeyDown(Keys.A),
      "A_Pressed" => IsKeyPressed(Keys.A),
      "B" => Keyboard.GetState().IsKeyDown(Keys.B),
      "B_Pressed" => IsKeyPressed(Keys.B),
      "C" => Keyboard.GetState().IsKeyDown(Keys.C),
      "C_Pressed" => IsKeyPressed(Keys.C),
      "D" => Keyboard.GetState().IsKeyDown(Keys.D),
      "D_Pressed" => IsKeyPressed(Keys.D),
      "E" => Keyboard.GetState().IsKeyDown(Keys.E),
      "E_Pressed" => IsKeyPressed(Keys.E),
      "F" => Keyboard.GetState().IsKeyDown(Keys.F),
      "F_Pressed" => IsKeyPressed(Keys.F),
      "G" => Keyboard.GetState().IsKeyDown(Keys.G),
      "G_Pressed" => IsKeyPressed(Keys.G),
      "H" => Keyboard.GetState().IsKeyDown(Keys.H),
      "H_Pressed" => IsKeyPressed(Keys.H),
      "I" => Keyboard.GetState().IsKeyDown(Keys.I),
      "I_Pressed" => IsKeyPressed(Keys.I),
      "J" => Keyboard.GetState().IsKeyDown(Keys.J),
      "J_Pressed" => IsKeyPressed(Keys.J),
      "K" => Keyboard.GetState().IsKeyDown(Keys.K),
      "K_Pressed" => IsKeyPressed(Keys.K),
      "L" => Keyboard.GetState().IsKeyDown(Keys.L),
      "L_Pressed" => IsKeyPressed(Keys.L),
      "M" => Keyboard.GetState().IsKeyDown(Keys.M),
      "M_Pressed" => IsKeyPressed(Keys.M),
      "N" => Keyboard.GetState().IsKeyDown(Keys.N),
      "N_Pressed" => IsKeyPressed(Keys.N),
      "O" => Keyboard.GetState().IsKeyDown(Keys.O),
      "O_Pressed" => IsKeyPressed(Keys.O),
      "P" => Keyboard.GetState().IsKeyDown(Keys.P),
      "P_Pressed" => IsKeyPressed(Keys.P),
      "Q" => Keyboard.GetState().IsKeyDown(Keys.Q),
      "Q_Pressed" => IsKeyPressed(Keys.Q),
      "R" => Keyboard.GetState().IsKeyDown(Keys.R),
      "R_Pressed" => IsKeyPressed(Keys.R),
      "S" => Keyboard.GetState().IsKeyDown(Keys.S),
      "S_Pressed" => IsKeyPressed(Keys.S),
      "T" => Keyboard.GetState().IsKeyDown(Keys.T),
      "T_Pressed" => IsKeyPressed(Keys.T),
      "U" => Keyboard.GetState().IsKeyDown(Keys.U),
      "U_Pressed" => IsKeyPressed(Keys.U),
      "V" => Keyboard.GetState().IsKeyDown(Keys.V),
      "V_Pressed" => IsKeyPressed(Keys.V),
      "W" => Keyboard.GetState().IsKeyDown(Keys.W),
      "W_Pressed" => IsKeyPressed(Keys.W),
      "X" => Keyboard.GetState().IsKeyDown(Keys.X),
      "X_Pressed" => IsKeyPressed(Keys.X),
      "Y" => Keyboard.GetState().IsKeyDown(Keys.Y),
      "Y_Pressed" => IsKeyPressed(Keys.Y),
      "Z" => Keyboard.GetState().IsKeyDown(Keys.Z),
      "Z_Pressed" => IsKeyPressed(Keys.Z),
      "`_Pressed" => IsKeyPressed(Keys.OemTilde),
      "Space" => Keyboard.GetState().IsKeyDown(Keys.Space),
      "SpacePressed" => IsKeyPressed(Keys.Space),
      "Enter" => Keyboard.GetState().IsKeyDown(Keys.Enter),
      "EnterPressed" => IsKeyPressed(Keys.Enter),
      "Escape" => Keyboard.GetState().IsKeyDown(Keys.Escape),
      "EscapePressed" => IsKeyPressed(Keys.Escape),
      "Left" => Keyboard.GetState().IsKeyDown(Keys.Left),
      "LeftPressed" => IsKeyPressed(Keys.Left),
      "Right" => Keyboard.GetState().IsKeyDown(Keys.Right),
      "RightPressed" => IsKeyPressed(Keys.Right),
      "Up" => Keyboard.GetState().IsKeyDown(Keys.Up),
      "UpPressed" => IsKeyPressed(Keys.Up),
      "Down" => Keyboard.GetState().IsKeyDown(Keys.Down),
      "DownPressed" => IsKeyPressed(Keys.Down),
      "MouseLeftPressed" => previousMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released,
      "MouseLeftDown" => mouse.LeftButton == ButtonState.Pressed,
      "MouseLeftUp" => mouse.LeftButton == ButtonState.Released,
      "MouseLeftReleased" => previousMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton != ButtonState.Pressed,
      "MouseRightPressed" => previousMouse.RightButton == ButtonState.Pressed && mouse.RightButton == ButtonState.Released,
      "MouseRightDown" => mouse.RightButton == ButtonState.Pressed,
      "MouseRightUp" => mouse.RightButton == ButtonState.Released,
      "MouseRightReleased" => previousMouse.RightButton == ButtonState.Pressed && mouse.RightButton != ButtonState.Pressed,
      "StickLeftX-" => gamepad.IsConnected && gamepad.ThumbSticks.Left.X < -0.5f,
      "StickLeftX+" => gamepad.IsConnected && gamepad.ThumbSticks.Left.X > 0.5f,
      "StickLeftY-" => gamepad.IsConnected && gamepad.ThumbSticks.Left.Y < 0.5f,
      "StickLeftY+" => gamepad.IsConnected && gamepad.ThumbSticks.Left.Y > 0.5f,
      "StickRightX-" => gamepad.IsConnected && gamepad.ThumbSticks.Right.X < 0.5f,
      "StickRightX+" => gamepad.IsConnected && gamepad.ThumbSticks.Right.X > 0.5f,
      "StickRightY-" => gamepad.IsConnected && gamepad.ThumbSticks.Right.Y < 0.5f,
      "StickRightY+" => gamepad.IsConnected && gamepad.ThumbSticks.Right.Y > 0.5f,
      "ButtonAPressed" => gamepad.IsConnected && gamepad.Buttons.A == ButtonState.Pressed,
      "ButtonBPressed" => gamepad.IsConnected && gamepad.Buttons.B == ButtonState.Pressed,
      "ButtonXPressed" => gamepad.IsConnected && gamepad.Buttons.X == ButtonState.Pressed,
      "ButtonYPressed" => gamepad.IsConnected && gamepad.Buttons.Y == ButtonState.Pressed,
      _ => throw new NotImplementedException($"Key {key} not implemented"),
    };
  }

  public bool IsKeyPressed(Keys key)
  {
    return previousKeyboard.IsKeyDown(key) && Keyboard.GetState().IsKeyUp(key);
  }

#pragma warning disable CA1822 // Mark members as static
  public Vector2 CursorScreenPosition
  {
    get
    {
      var scale = Core.TargetScreenWidth / Core.ScreenWidth;
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
      return Core.Camera.ScreenToWorld(PreviousCursorScreenPosition);
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