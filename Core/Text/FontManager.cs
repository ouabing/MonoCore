using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;

namespace G;

public class FontManager : IDisposable
{
  private readonly FontSystem fontSystem = new();
  private Dictionary<int, SpriteFontBase> FontCache { get; } = [];

  public void LoadContent()
  {
    foreach (var font in Def.Fonts)
    {
      AddFont(font);
    }
  }

  public void AddFont(string path)
  {

    fontSystem.AddFont(File.ReadAllBytes(path));
  }

  public SpriteFontBase Get(int size)
  {
    if (!FontCache.TryGetValue(size, out SpriteFontBase? font))
    {
      font = fontSystem.GetFont(size);
      FontCache[size] = font;
    }
    return font;
  }

  public void Dispose()
  {
    fontSystem.Dispose();
    GC.SuppressFinalize(this);
  }
}