using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace G;

public class InputManager
{
  public bool LeftClicked { get; set; }
  public bool RightClicked { get; set; }
  public bool LeftDown { get; set; }

  private MouseState ms;

  public void Update(GameTime gameTime)
  {
    var lastMs = ms;
    ms = Mouse.GetState();
    LeftDown = ms.LeftButton == ButtonState.Pressed;
    LeftClicked = ms.LeftButton != ButtonState.Pressed && lastMs.LeftButton == ButtonState.Pressed;
    RightClicked = ms.RightButton != ButtonState.Pressed && lastMs.RightButton == ButtonState.Pressed;
  }
  public bool Hover(Rectangle r)
  {
    return r.Contains(new Vector2(ms.X, ms.Y));
  }

  public Vector2 CursorPosition
  {
    get
    {
      var cp = Mouse.GetState().Position;
      var scale = Core.GraphicsDevice.Viewport.Width / Core.ScreenWidth;
      return new Vector2(cp.X / scale, cp.Y / scale);
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

  public bool IsUp()
  {
    var kbState = Keyboard.GetState();
    var gamePadState = GamePad.GetState(PlayerIndex.One);
    return kbState.IsKeyDown(Keys.W) || gamePadState.ThumbSticks.Left.Y > 0.5f;
  }

  public bool IsDown()
  {
    var kbState = Keyboard.GetState();
    var gamePadState = GamePad.GetState(PlayerIndex.One);
    return kbState.IsKeyDown(Keys.S) || gamePadState.ThumbSticks.Left.Y < -0.5f;
  }

  public bool IsLeft()
  {
    var kbState = Keyboard.GetState();
    var gamePadState = GamePad.GetState(PlayerIndex.One);
    return kbState.IsKeyDown(Keys.A) || gamePadState.ThumbSticks.Left.X < -0.5f;
  }

  public bool IsRight()
  {
    var kbState = Keyboard.GetState();
    var gamePadState = GamePad.GetState(PlayerIndex.One);
    return kbState.IsKeyDown(Keys.D) || gamePadState.ThumbSticks.Left.X > 0.5f;
  }
}