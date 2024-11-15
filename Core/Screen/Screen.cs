using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace G;

public class Screen
{
  public GraphicsDeviceManager Graphics { get; private set; }

  // The game's resolution
  public int Width { get; private set; }
  public int Height { get; private set; }
  public GameWindow Window { get; private set; }

  // The game's real display resolution, a multiple of the game's resolution
  public int DisplayWidth { get; private set; }
  public int DisplayHeight { get; private set; }

  // Current window resolution
  public int WindowWidth => Window.ClientBounds.Width;
  public int WindowHeight => Window.ClientBounds.Height;

  // The offset to center the game in the window
  public Vector2 Offset { get; private set; } = Vector2.Zero;

  public Screen(GraphicsDeviceManager graphics, int width, int height)
  {
    Graphics = graphics;
    Width = width;
    Height = height;
  }

  public void Initialize(GameWindow window)
  {
    Window = window;
    UpdateDisplaySize(Def.Screen.TargetScreenWidth, Def.Screen.TargetScreenHeight, false);

    Graphics!.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    Graphics!.IsFullScreen = false;
    Graphics.PreferredBackBufferWidth = DisplayWidth;
    Graphics.PreferredBackBufferHeight = DisplayHeight;
    Graphics.HardwareModeSwitch = false;
    Graphics.ApplyChanges();
  }

  public void SetWindow()
  {
    UpdateDisplaySize(Def.Screen.TargetScreenWidth, Def.Screen.TargetScreenHeight, false);
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
    // var supportedModes = Graphics.GraphicsDevice.Adapter.SupportedDisplayModes;
    // DisplayMode? chosenMode = null;
    // int bestScale = 0;
    // float fillRate = 0.0f;

    // // Find the best fitting display mode
    // foreach (var mode in supportedModes)
    // {
    //   int scaleX = mode.Width / Width;
    //   int scaleY = mode.Height / Height;
    //   int scale = Math.Min(scaleX, scaleY);

    //   if (scale * Width > mode.Width || scale * Height > mode.Height)
    //   {
    //     continue;
    //   }

    //   var newFillRate = Width * scale * Height * scale / (float)(mode.Width * mode.Height);
    //   if (newFillRate <= fillRate)
    //   {
    //     continue;
    //   }
    //   fillRate = newFillRate;
    //   bestScale = scale;
    //   chosenMode = mode;
    // }

    // if (chosenMode != null)
    // {
    //   DisplayWidth = (int)(Width * bestScale / chosenMode.AspectRatio);
    //   DisplayHeight = (int)(Height * bestScale / chosenMode.AspectRatio);
    //   Graphics.PreferredBackBufferWidth = DisplayWidth;
    //   Graphics.PreferredBackBufferHeight = DisplayHeight;
    //   Graphics.IsFullScreen = true;
    //   Graphics.ApplyChanges();
    //   UpdateDisplaySize(DisplayWidth, DisplayHeight, true);
    // }
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