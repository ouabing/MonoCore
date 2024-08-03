using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace G;

public class Canvas(
  string name,
  int width,
  int height,
  Color backgroundColor
) : IDisposable
{
  public string Name { get; } = name;
  public int Width { get; } = width;
  public int Height { get; } = height;
  public Color BackgroundColor { get; } = backgroundColor;
  public RenderTarget2D RenderTarget { get; } = new RenderTarget2D(Core.Graphics!.GraphicsDevice, width, height);
  public Effect? FX { get; private set; }

  public void ApplyFX(Effect? fx)
  {
    FX = fx;
  }

  public void Begin()
  {
    Core.Graphics!.GraphicsDevice.SetRenderTarget(RenderTarget);
    Core.Graphics.GraphicsDevice.Clear(BackgroundColor);
  }

#pragma warning disable CA1822 // Mark members as static
  public void End()
  {
    Core.Graphics!.GraphicsDevice.SetRenderTarget(null);
  }
#pragma warning restore CA1822 // Mark members as static

  public void Dispose()
  {
    RenderTarget.Dispose();
    GC.SuppressFinalize(this);
  }
}