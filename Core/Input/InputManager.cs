using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace G;

public class InputManager
{
  public bool LeftClicked { get; set; }
  public bool RightClicked { get; set; }
  public bool LeftDown { get; set; }
  private readonly Dictionary<Def.Input.Action, bool> Activated = [];

  private MouseState lastMouse;

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

  public bool IsActive(Def.Input.Action action)
  {
    return Activated.ContainsKey(action);
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
    lastMouse = Mouse.GetState();
  }
  public bool Hover(Rectangle r)
  {
    return r.Contains(new Vector2(lastMouse.X, lastMouse.Y));
  }

  private bool IsKeyPressed(string key)
  {
    var mouse = Mouse.GetState();
    var gamepad = GamePad.GetState(PlayerIndex.One);
    switch (key)
    {
      case "W":
        return Keyboard.GetState().IsKeyDown(Keys.W);
      case "A":
        return Keyboard.GetState().IsKeyDown(Keys.A);
      case "S":
        return Keyboard.GetState().IsKeyDown(Keys.S);
      case "D":
        return Keyboard.GetState().IsKeyDown(Keys.D);
      case "Space":
        return Keyboard.GetState().IsKeyDown(Keys.Space);
      case "Enter":
        return Keyboard.GetState().IsKeyDown(Keys.Enter);
      case "Escape":
        return Keyboard.GetState().IsKeyDown(Keys.Escape);
      case "Left":
        return Keyboard.GetState().IsKeyDown(Keys.Left);
      case "Right":
        return Keyboard.GetState().IsKeyDown(Keys.Right);
      case "Up":
        return Keyboard.GetState().IsKeyDown(Keys.Up);
      case "Down":
        return Keyboard.GetState().IsKeyDown(Keys.Down);
      case "MouseLeftPressed":
        return lastMouse.LeftButton != ButtonState.Pressed && mouse.LeftButton == ButtonState.Pressed;
      case "MouseLeftDown":
        return mouse.LeftButton == ButtonState.Pressed;
      case "MouseLeftReleased":
        return lastMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton != ButtonState.Pressed;
      case "MouseRightPressed":
        return lastMouse.LeftButton != ButtonState.Pressed && mouse.RightButton == ButtonState.Pressed;
      case "MouseRightDown":
        return mouse.RightButton == ButtonState.Pressed;
      case "MouseRightReleased":
        return lastMouse.RightButton == ButtonState.Pressed && mouse.RightButton != ButtonState.Pressed;
      case "StickLeftX-":
        return gamepad.ThumbSticks.Left.X < 0.5f;
      case "StickLeftX+":
        return gamepad.ThumbSticks.Left.X > 0.5f;
      case "StickLeftY-":
        return gamepad.ThumbSticks.Left.Y < 0.5f;
      case "StickLeftY+":
        return gamepad.ThumbSticks.Left.Y > 0.5f;
      case "StickRightX-":
        return gamepad.ThumbSticks.Right.X < 0.5f;
      case "StickRightX+":
        return gamepad.ThumbSticks.Right.X > 0.5f;
      case "StickRightY-":
        return gamepad.ThumbSticks.Right.Y < 0.5f;
      case "StickRightY+":
        return gamepad.ThumbSticks.Right.Y > 0.5f;
    }
    throw new NotImplementedException($"Key {key} not implemented");
  }

#pragma warning disable CA1822 // Mark members as static
  public Vector2 CursorPosition
  {
    get
    {
      return Mouse.GetState().Position.ToVector2();
    }
  }

  public Vector2 CursorPositionInWorld
  {
    get
    {
      var scale = Core.GraphicsDevice.Viewport.Width / Core.ScreenWidth;
      return CursorPosition / scale;
    }
  }
#pragma warning restore CA1822 // Mark members as static

  public Vector2 PreviousCursorPosition
  {
    get
    {
      return lastMouse.Position.ToVector2();
    }
  }

  public Vector2 PreviousCursorPositionInWorld
  {
    get
    {
      var scale = Core.GraphicsDevice.Viewport.Width / Core.ScreenWidth;
      return PreviousCursorPosition / scale;
    }
  }

  public bool IsCursorValid()
  {
    var cp = CursorPosition;
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
    return rect.Contains(CursorPosition);
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