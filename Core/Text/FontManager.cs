using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;

namespace G;

public class FontManager : IDisposable
{
  private string currentFont = "";
  private readonly Dictionary<string, FontSystem> fontSystems = [];
  private Dictionary<string, SpriteFontBase> FontCache { get; } = [];

  public void LoadContent()
  {
    foreach (var (name, path) in Def.Fonts)
    {
      fontSystems[name] = new FontSystem();
      fontSystems[name].AddFont(File.ReadAllBytes(path));
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