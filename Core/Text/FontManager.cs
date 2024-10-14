using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;

namespace G;

public class FontManager : IDisposable
{
  private readonly Dictionary<string, FontSystem> fontSystems = [];
  private Dictionary<string, SpriteFontBase> FontCache { get; } = [];

  public void LoadContent()
  {
    foreach (var (name, path) in Def.Fonts)
    {
      var settings = new FontSystemSettings
      {
        // Disable anti-aliasing
        GlyphRenderer = (input, output, options) =>
        {
          var size = options.Size.X * options.Size.Y;

          for (var i = 0; i < size; i++)
          {
            var c = input[i];
            var ci = i * 4;

            if (c == 0)
            {
              output[ci] = output[ci + 1] = output[ci + 2] = output[ci + 3] = 0;
            }
            else
            {
              output[ci] = output[ci + 1] = output[ci + 2] = output[ci + 3] = 255;
            }
          }
        }
      };
      var system = new FontSystem(settings);
      fontSystems[name] = system;
      system.AddFont(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, path)));
    }
  }

  public SpriteFontBase Get(string font, int size)
  {
    var key = $"{font}-{size}";
    if (!FontCache.TryGetValue(key, out SpriteFontBase? f))
    {
      if (!fontSystems.TryGetValue(font, out FontSystem? fontSystem))
      {
        throw new ArgumentException($"Font {font} not found.");
      }
      f = fontSystem.GetFont(size);
      FontCache[key] = f;
    }
    return f;
  }

  public SpriteFontBase Get(int size)
  {
    var font = Def.Fonts[0].Item1;
    return Get(font, size);
  }

  public void Dispose()
  {
    foreach (var system in fontSystems.Values)
    {
      system.Dispose();
    }
    GC.SuppressFinalize(this);
  }
}