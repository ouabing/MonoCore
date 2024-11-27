using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace G;

public class Canvas(
  string name,
  int width,
  int height,
  Color? backgroundColor = null
) : IDisposable
{
  public string Name { get; } = name;
  public int Width { get; private set; } = width;
  public int Height { get; private set; } = height;
  public Color BackgroundColor { get; } = backgroundColor ?? Color.Transparent;
  public RenderTarget2D RenderTarget { get; private set; } = new RenderTarget2D(
    Core.Graphics!.GraphicsDevice,
    width,
    height,
    false,
    SurfaceFormat.Color,
    DepthFormat.None,
    0,
    RenderTargetUsage.PreserveContents
  );
  public Effect? FX { get; private set; }

  public void Resize(int width, int height)
  {
    RenderTarget.Dispose();
    Width = width;
    Height = height;
    RenderTarget = new RenderTarget2D(
      Core.Graphics!.GraphicsDevice,
      width,
      height,
      false,
      SurfaceFormat.Color,
      DepthFormat.None,
      0,
      RenderTargetUsage.PreserveContents
    );
  }

  public void ApplyFX(Effect? fx)
  {
    FX = fx;
  }

  public void Begin()
  {
    Core.Graphics!.GraphicsDevice.SetRenderTarget(RenderTarget);
    Core.Graphics.GraphicsDevice.Clear(Color.Transparent);
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