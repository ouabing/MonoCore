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
  private readonly Dictionary<int, List<Component>> componentsByZ = [];

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

  public void Add(Component component, int? overrideZ = null)
  {
    var z = overrideZ ?? component.Z;
    if (!componentsByZ.TryGetValue(z, out List<Component>? value))
    {
      value = [];
      componentsByZ[z] = value;
    }

    value.Add(component);
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
      var orderedByZ = componentsByZ.OrderBy(x => x.Key).ToList();
      // Note the order of components in the same Z level is not guaranteed
      foreach (var components in orderedByZ)
      {
        List<Component> inBatch = [];
        foreach (var component in components.Value)
        {
          // Draw primitives should be executed outside sprite batch
          if (component.EnableDrawPrimitives)
          {
            // Each time a SpriteBatch is ended, the RasterizerState will be reset
            // For 2D game, we don't need to cull any face
            Core.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            if (!IsCameraFixed)
            {
              component.BasicFX!.View = Core.Camera.GetMatrix();
            }
            foreach (var pass in component.BasicFX!.CurrentTechnique.Passes)
            {
              pass.Apply();
              component.DrawPrimitives(gameTime);
            }
          }

          // DrawPrimitives and Draw will both be executed
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
        if (inBatch.Count != 0)
        {
          Core.Sb!.Begin(samplerState: SamplerState.PointClamp, transformMatrix: transformMatrix, effect: canvas.FX);
          foreach (var component in inBatch)
          {
            component.Draw(gameTime);
          }
          Core.Sb.End();
        }
      }
      canvas.End();
    }

    componentsByZ.Clear();
  }

  public void ApplyFX(string canvasName, Effect? fx)
  {
    var canvas = Canvases.Find(x => x.Name == canvasName) ?? throw new ArgumentException($"Canvas with name {canvasName} does not exist.");
    canvas.ApplyFX(fx);
  }
}