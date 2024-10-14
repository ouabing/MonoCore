using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace G;

/**
 * Manager all the drawing layer
 * LayerManager -- Global layer manager, which renders all Layers
 *   Layer -- Logically defined layer, draws all the components to Canvas
 *     Canvas -- The actual RenderTarget
 */
public class LayerManager(Color backgroundColor)
{
  public Color BackgroundColor { get; private set; } = backgroundColor;
  public Dictionary<Def.Layer, Layer> Layers { get; private set; } = [];

  public void Initialize()
  {
    foreach (Def.Layer layer in Enum.GetValues<Def.Layer>())
    {
      CreateLayer(layer);
      var isCameraFixed = Def.LayerConfig.TryGetValue(layer, out var config) && config.TryGetValue("IsCameraFixed", out var isFixed) && (bool)isFixed;
      CreateLayer(layer, isCameraFixed);
    }
  }

  public void CreateLayer(Def.Layer layer, bool isCameraFixed = false)
  {
    Layers[layer] = new Layer(layer, (int)layer, isCameraFixed);
  }

  public void Add(Def.Layer toLayer, Component component)
  {
    if (!Layers.TryGetValue(toLayer, out Layer? layer))
    {
      throw new KeyNotFoundException($"Layer {toLayer} not found.");
    }

    layer.Add(component);
  }

  public void Remove(Def.Layer toLayer, Component component)
  {
    if (!Layers.TryGetValue(toLayer, out Layer? layer))
    {
      throw new KeyNotFoundException($"Layer {toLayer} not found.");
    }

    layer.Remove(component);
  }

  public void Draw(GameTime gameTime)
  {
    var layers = Layers.Values.OrderBy(layer => layer.Z).Reverse();

    foreach (var layer in layers)
    {
      layer.Draw(gameTime);
    }

    Core.Graphics!.GraphicsDevice.SetRenderTarget(null);
    Core.Graphics!.GraphicsDevice.Clear(BackgroundColor);

    foreach (var layer in layers)
    {
      foreach (var canvas in layer.Canvases)
      {
        Core.Sb!.Begin(samplerState: SamplerState.PointClamp);
        Core.Sb.Draw(canvas.RenderTarget, new Rectangle(0, 0, Core.TargetScreenWidth, Core.TargetScreenHeight), Color.White);
        Core.Sb.End();
      }
      layer.ClearDead();
    }

  }
}