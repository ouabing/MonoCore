using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace G;

public class Screen(GraphicsDeviceManager graphics, int width, int height, int windowedModeWidth, int windowedModeHeight)
{
  public GraphicsDeviceManager Graphics { get; private set; } = graphics;

  // The game's resolution
  public int Width { get; private set; } = width;
  public int Height { get; private set; } = height;
  public Vector2 Center => new(Width / 2, Height / 2);
  public int WindowedModeWidth { get; private set; } = windowedModeWidth;
  public int WindowedModeHeight { get; private set; } = windowedModeHeight;
  public GameWindow Window { get; private set; }

  // The game's real display resolution, a multiple of the game's resolution
  public int DisplayWidth { get; private set; }
  public int DisplayHeight { get; private set; }

  // The offset to center the game in the window
  public Vector2 Offset { get; private set; } = Vector2.Zero;
  public Matrix Transform => Matrix.CreateTranslation(Offset.X, Offset.Y, 0);

  public void Initialize(GameWindow window)
  {
    Window = window;
    UpdateDisplaySize(WindowedModeWidth, WindowedModeHeight, false);

    Graphics!.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    Graphics!.IsFullScreen = false;
    Graphics.PreferredBackBufferWidth = DisplayWidth;
    Graphics.PreferredBackBufferHeight = DisplayHeight;
    Graphics.HardwareModeSwitch = false;
    Graphics.ApplyChanges();
  }

  public void SetWindow()
  {
    UpdateDisplaySize(Def.Screen.WindowedModeWidth, Def.Screen.WindowedModeHeight, false);
    Graphics.IsFullScreen = false;
    Graphics.PreferredBackBufferWidth = DisplayWidth;
    Graphics.PreferredBackBufferHeight = DisplayHeight;
    Graphics.ApplyChanges();
  }

  public void SetFullscreen()
  {
    UpdateDisplaySize(Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width, Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height, true);
    Graphics.PreferredBackBufferWidth = DisplayWidth;
    Graphics.PreferredBackBufferHeight = DisplayHeight;
    Graphics.IsFullScreen = true;
    Graphics.ApplyChanges();
  }

  private void UpdateDisplaySize(int windowWidth, int windowHeight, bool isFullScreen)
  {
    var scale = Math.Min((float)windowWidth / Width, (float)windowHeight / Height);
    DisplayWidth = (int)(Width * scale);
    DisplayHeight = (int)(Height * scale);
    if (isFullScreen)
    {
      Offset = new Vector2((windowWidth - DisplayWidth) / 2, (windowHeight - DisplayHeight) / 2);
    }
    else
    {
      Offset = Vector2.Zero;
    }
  }
}