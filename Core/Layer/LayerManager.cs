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
  public List<Effect> GlobalFXs { get; private set; } = [];
  private List<RenderTarget2D> renderTargets { get; set; } = [];

  public void Initialize()
  {
    foreach (Def.Layer layer in Enum.GetValues<Def.Layer>())
    {
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

  public void ApplyGlobalFX(Effect effect)
  {
    GlobalFXs.Add(effect);
    RecreateRenderTargets();
  }

  public void RemoveGlobalFX(Effect effect)
  {
    GlobalFXs.Remove(effect);
    RecreateRenderTargets();
  }

  private void RecreateRenderTargets()
  {
    for (int i = 0; i < renderTargets.Count; i++)
    {
      renderTargets[i].Dispose();
    }
    renderTargets.Clear();
    for (int i = 0; i < GlobalFXs.Count - 1; i++)
    {
      renderTargets.Add(new RenderTarget2D(Core.Graphics!.GraphicsDevice, Core.ScreenWidth, Core.ScreenHeight));
    }
  }

  public void ApplyFX(Def.Layer toLayer, Effect effect, string canvas = "Main")
  {
    if (!Layers.TryGetValue(toLayer, out Layer? layer))
    {
      throw new KeyNotFoundException($"Layer {toLayer} not found.");
    }

    layer.ApplyFX(canvas, effect);
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
        var lastRenderTarget = canvas.RenderTarget;
        if (GlobalFXs.Count > 0)
        {
          for (int i = 0; i < GlobalFXs.Count - 1; i++)
          {
            var fx = GlobalFXs[i];
            var renderTarget = renderTargets[i];
            Core.GraphicsDevice.SetRenderTarget(renderTarget);
            Core.Sb!.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp, effect: fx);
            Core.Sb.Draw(lastRenderTarget, new Rectangle(0, 0, canvas.Width, canvas.Height), Color.White);
            Core.Sb.End();
            lastRenderTarget = renderTarget;
          }

          Core.GraphicsDevice.SetRenderTarget(null);
          Core.Sb!.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp, effect: GlobalFXs.Last());
          Core.Sb.Draw(lastRenderTarget, new Rectangle(0, 0, Core.TargetScreenWidth, Core.TargetScreenHeight), Color.White);
          Core.Sb.End();
        }
        else
        {
          Core.Sb!.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
          Core.Sb.Draw(canvas.RenderTarget, new Rectangle(0, 0, Core.TargetScreenWidth, Core.TargetScreenHeight), Color.White);
          Core.Sb.End();
        }

      }
      layer.ClearDead();
    }
  }
}