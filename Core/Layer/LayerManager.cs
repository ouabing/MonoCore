using System;
using System.Collections.Generic;
using System.IO;
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
  private List<Dictionary<(int, int), RenderTarget2D>> renderTargets { get; set; } = [];

  public void Initialize()
  {
    foreach (Def.Layer layer in Enum.GetValues<Def.Layer>())
    {
      Def.LayerConfig.TryGetValue(layer, out var config);
      var w = Core.Screen.Width;
      var h = Core.Screen.Height;
      if (config == null)
      {
        CreateLayer(layer, w, h, false);
      }
      else
      {
        var isCameraFixed = config.TryGetValue("IsCameraFixed", out var isFixed) && (bool)isFixed;
        if (layer == Def.Layer.Debugger)
        {
          w = config.TryGetValue("Width", out var width) ? (int)width : Core.Screen.DisplayWidth;
          h = config.TryGetValue("Height", out var height) ? (int)height : Core.Screen.DisplayHeight;
        }
        else
        {
          w = config.TryGetValue("Width", out var width) ? (int)width : Core.Screen.Width;
          h = config.TryGetValue("Height", out var height) ? (int)height : Core.Screen.Height;
        }
        CreateLayer(layer, w, h, isCameraFixed);
      }
    }
  }

  public void CreateLayer(Def.Layer layer, int width, int height, bool isCameraFixed = false)
  {
    Layers[layer] = new Layer(layer, (int)layer, isCameraFixed, width, height);
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
    foreach (var renderTargetDict in renderTargets)
    {
      foreach (var renderTarget in renderTargetDict.Values)
      {
        renderTarget.Dispose();
      }
    }
    renderTargets.Clear();
    for (int i = 0; i < GlobalFXs.Count - 1; i++)
    {
      Dictionary<(int, int), RenderTarget2D> dict = [];
      renderTargets.Add(dict);

      foreach (var layer in Layers.Values)
      {
        renderTargets[i][(layer.Width, layer.Height)] = new RenderTarget2D(Core.Graphics!.GraphicsDevice, layer.Width, layer.Height);
      }
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

  public string TakeScreenshot(string folder)
  {
    int w = Core.GraphicsDevice.PresentationParameters.BackBufferWidth;
    int h = Core.GraphicsDevice.PresentationParameters.BackBufferHeight;
    var gametime = new GameTime();
    Draw(gametime);
    int[] backBuffer = new int[w * h];
    Core.GraphicsDevice.GetBackBufferData(backBuffer);

    Texture2D texture = new(Core.GraphicsDevice, w, h, false, Core.GraphicsDevice.PresentationParameters.BackBufferFormat);
    texture.SetData(backBuffer);

    if (!Directory.Exists(folder))
    {
      Directory.CreateDirectory(folder);
    }

    var filename = FileHelper.ResolvePath(Path.Combine(folder, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.png"));
    Stream stream = File.OpenWrite(filename);

    texture.SaveAsPng(stream, w, h);
    stream.Close();
    stream.Dispose();

    texture.Dispose();
    return filename;
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

    // Apply the viewport offsets
    var transform = Core.Screen.Transform;

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
            var renderTargetDict = renderTargets[i];
            var renderTarget = renderTargetDict[(layer.Width, layer.Height)];
            Core.GraphicsDevice.SetRenderTarget(renderTarget);
            Core.Sb!.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp, effect: fx);
            Core.Sb.Draw(lastRenderTarget, new Rectangle(0, 0, canvas.Width, canvas.Height), Color.White);
            Core.Sb.End();
            lastRenderTarget = renderTarget;
          }

          Core.GraphicsDevice.SetRenderTarget(null);
          Core.Sb!.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp, effect: GlobalFXs.Last(), transformMatrix: transform);
          Core.Sb.Draw(lastRenderTarget, new Rectangle(0, 0, Core.Screen.DisplayWidth, Core.Screen.DisplayHeight), Color.White);
          Core.Sb.End();
        }
        else
        {
          Core.Sb!.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp, transformMatrix: transform);
          Core.Sb.Draw(canvas.RenderTarget, new Rectangle(0, 0, Core.Screen.DisplayWidth, Core.Screen.DisplayHeight), Color.White);
          Core.Sb.End();
        }

      }
      layer.ClearDead();
    }
  }
}