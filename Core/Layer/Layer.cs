using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using nkast.Aether.Physics2D.Collision.Shapes;

namespace G;

public class Layer
{
  public Def.Layer Name { get; }
  public int Z { get; }
  public int Width { get; private set; } = Core.Screen.Width;
  public int Height { get; private set; } = Core.Screen.Width;
  public RenderTarget2D RenderTarget { get; private set; } = new RenderTarget2D(
    Core.Graphics!.GraphicsDevice,
    Core.Screen.Width,
    Core.Screen.Height,
    false,
    SurfaceFormat.Color,
    DepthFormat.None,
    0,
    RenderTargetUsage.PreserveContents
  );
  public Color BackgroundColor { get; set; } = Color.Transparent;
  private bool IsCameraFixed { get; }
  public List<Canvas> Canvases { get; private set; } = [];
  private readonly List<Component> components = [];

  public Layer(Def.Layer name, int z, bool isCameraFixed, int width, int height)
  {
    Name = name;
    Z = z;
    IsCameraFixed = isCameraFixed;
    Width = width;
    Height = height;
    AddCanvas("Main", Width, Height);
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

  public void AddCanvas(string name, int width, int height)
  {
    if (Canvases.FindIndex(x => x.Name == name) != -1)
    {
      throw new ArgumentException($"Canvas with name {name} already exists.");
    }
    Canvases.Add(new Canvas(name, width, height));
  }

  public void RemoveCanvas(string name)
  {
    var index = Canvases.FindIndex(x => x.Name == name);
    if (index == -1)
    {
      throw new ArgumentException($"Canvas with name {name} does not exist.");
    }
    Canvases.RemoveAt(index);
  }

  public Canvas? GetCanvas(string name)
  {
    return Canvases.Find(x => x.Name == name);
  }

  public void Add(Component component)
  {
    if (components.FindIndex(x => x == component) != -1)
    {
      throw new ArgumentException($"Component {component} already exists.");
    }
    components.Add(component);
  }

  public void Remove(Component component)
  {
    if (components.FindIndex(x => x == component) == -1)
    {
      return;
    }
    components.Remove(component);
  }

  public void Draw(GameTime gameTime)
  {
    if (Canvases.Count == 0)
    {
      throw new InvalidOperationException("No canvas added.");
    }

    Matrix? transformMatrix = IsCameraFixed ? null : Core.Camera.GetMatrix();
    foreach (var canvas in Canvases)
    {
      canvas.Begin();
      var orderedByZ = components.OrderBy(x => -x.Z).ToList();
      List<Component> inBatch = [];
      // Note the order of components in the same Z level is not guaranteed
      foreach (var component in orderedByZ)
      {
        if (component.Opacity == 0)
        {
          continue;
        }
        // Draw primitives should be executed outside sprite batch
        if (component.EnablePrimitiveBatch)
        {
          FlushBatch(ref gameTime, ref inBatch, ref transformMatrix, canvas.FX);
          var matrix = Matrix.CreateOrthographicOffCenter(0, Core.Screen.Width, Core.Screen.Height, 0, 0, 1);
          var view = IsCameraFixed ? Matrix.Identity : Core.Camera.GetMatrix();
          // Each time a SpriteBatch is ended, the RasterizerState will be reset
          // For 2D game, we don't need to cull any face
          Core.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
          Core.Pb!.Begin(
            ref matrix,
            ref view
          );
          component.Draw(gameTime);
          Core.Pb.End();
        }
        else if (component.EnableSpriteBatch)
        {
          if (component.CurrentFX == null)
          {
            inBatch.Add(component);
          }
          else
          {
            // Component.CurrentEffect will override the canvas's default effect
            Core.Sb!.Begin(
              samplerState: SamplerState.PointClamp,
              effect: component.CurrentFX,
              transformMatrix: transformMatrix,
              blendState: BlendState.AlphaBlend
            );
            component.Draw(gameTime);
            Core.Sb.End();
          }
        }
        else
        {
          FlushBatch(ref gameTime, ref inBatch, ref transformMatrix, canvas.FX);
          Core.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
          component.Draw(gameTime);
        }

      }
      if (inBatch.Count != 0)
      {
        Core.Sb!.Begin(samplerState: SamplerState.PointClamp, transformMatrix: transformMatrix, effect: canvas.FX);
        foreach (var component in inBatch)
        {
          component.Draw(gameTime);
        }
        Core.Sb.End();
      }

      // Draw shape and position for debugging
      if (Core.EnableDebug)
      {
        foreach (var component in orderedByZ)
        {
          Core.Sb!.Begin(samplerState: SamplerState.PointClamp, transformMatrix: transformMatrix, effect: canvas.FX);
          DrawFixtures(component);
          if (Core.EnablePositionDebug)
          {
            var font = Core.Font.Get(10);
            font.DrawText(Core.Sb, $"({(int)component.Position.X},{(int)component.Position.Y})", component.TopLeft, Palette.White);
          }
          Core.Sb.End();
        }
      }
      canvas.End();
    }
  }

  private static void FlushBatch(ref GameTime gameTime, ref List<Component> inBatch, ref Matrix? transformMatrix, Effect? effect)
  {
    if (inBatch.Count != 0)
    {
      Core.Sb!.Begin(
        samplerState: SamplerState.PointClamp,
        transformMatrix: transformMatrix,
        effect: effect,
        blendState: BlendState.AlphaBlend
      );
      foreach (var component in inBatch)
      {
        component.Draw(gameTime);
      }
      Core.Sb.End();
      inBatch.Clear();
    }
  }

  public void PostUpdate(GameTime gameTime)
  {
    ClearDead();
  }

  public void ClearDead()
  {
    components.RemoveAll(c => c.IsDead);
  }

  public void ApplyFX(string canvasName, Effect? fx)
  {
    var canvas = Canvases.Find(x => x.Name == canvasName) ?? throw new ArgumentException($"Canvas with name {canvasName} does not exist.");
    canvas.ApplyFX(fx);
  }

  private static void DrawFixtures(Component component)
  {
    if (component.Body == null)
    {
      return;
    }
    if (component.Body.FixtureList.Count == 0)
    {
      return;
    }
    foreach (var fixture in component.Body.FixtureList)
    {
      if (fixture.Shape is PolygonShape poly)
      {
        var vectors = poly.Vertices.Select(v =>
        {
          return component.Body.GetWorldPoint(v).ToPixelVector2();
        }).ToArray();
        Core.Sb.DrawPolygon(Vector2.Zero, new Polygon(vectors), Color.Red, 1);
      }
      else if (fixture.Shape is CircleShape circle)
      {
        var center = component.Body.GetWorldPoint(circle.Position);
        Core.Sb.DrawCircle(new CircleF(center.ToPixelVector2(), circle.Radius), 16, Color.Red, 1);
      }
    }
  }
}