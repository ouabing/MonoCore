using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace G;

public class Layer
{
  public Def.Layer Name { get; }
  public int Z { get; }
  public Color BackgroundColor { get; set; } = Color.Transparent;
  private bool IsCameraFixed { get; }
  public List<Canvas> Canvases { get; private set; } = [];
  private readonly List<Component> components = [];

  public Layer(Def.Layer name, int z, bool isCameraFixed)
  {
    Name = name;
    Z = z;
    IsCameraFixed = isCameraFixed;
    AddCanvas("Main", Core.ScreenWidth, Core.ScreenHeight, isCameraFixed);
  }

  public void AddCanvas(string name, int width, int height, bool isCameraFixed)
  {
    if (Canvases.FindIndex(x => x.Name == name) != -1)
    {
      throw new ArgumentException($"Canvas with name {name} already exists.");
    }
    Canvases.Add(new Canvas(name, width, height, BackgroundColor));
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
          var matrix = Matrix.CreateOrthographicOffCenter(0, Core.ScreenWidth, Core.ScreenHeight, 0, 0, 1);
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
            Core.Sb!.Begin(samplerState: SamplerState.PointClamp, effect: component.CurrentFX, transformMatrix: transformMatrix);
            component.Draw(gameTime);
            Core.Sb.End();
          }
        }
        else
        {
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
      if (Core.DebugComponent)
      {
        foreach (var component in orderedByZ)
        {
          Core.Sb!.Begin(samplerState: SamplerState.PointClamp, transformMatrix: transformMatrix, effect: canvas.FX);
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
          IBox box = component;
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
          box.DrawBox(gameTime);
          var font = Core.Font.Get(10);
          font.DrawText(Core.Sb, $"({(int)component.Position.X},{(int)component.Position.Y})", component.TopLeft, Palette.White);
          Core.Sb.End();
        }
      }
      canvas.End();
    }
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
}