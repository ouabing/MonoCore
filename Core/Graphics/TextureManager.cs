using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace G;

public class TextureManager(ContentManager contentManager)
{
  public Texture2D PixelTexture { get; private set; }
  private ContentManager ContentManager { get; } = contentManager;
  private Dictionary<string, Texture2D> TextureCache { get; } = [];
  private Dictionary<string, Texture2D> BorderedTextureCache { get; } = [];

  public void LoadContent()
  {
    PixelTexture = new Texture2D(Core.Graphics!.GraphicsDevice, 1, 1);
    PixelTexture.SetData([Color.White]);
  }

  public Texture2D LoadTextureFromStream(string path)
  {
    if (TextureCache.TryGetValue(path, out Texture2D? value))
    {
      return value;
    }
    using var fileStream = new FileStream(path, FileMode.Open);
    var texture = Texture2D.FromStream(Core.GraphicsDevice, fileStream);
    TextureCache[path] = texture;
    return texture;
  }

  public Texture2D LoadTexture(string path)
  {
    if (TextureCache.TryGetValue(path, out Texture2D? value))
    {
      return value;
    }
    var texture = ContentManager!.Load<Texture2D>(path);
    TextureCache[path] = texture;
    return texture;
  }

  public Texture2D LoadBorderedTexture(string path)
  {
    if (BorderedTextureCache.TryGetValue(path, out Texture2D? value))
    {
      return value;
    }

    var borderedSprite = CreateBorderedTexture(LoadTexture(path));
    BorderedTextureCache[path] = borderedSprite;
    return borderedSprite;
  }

  private static Texture2D CreateBorderedTexture(Texture2D texture)
  {
    var colors = new Color[texture.Width * texture.Height];
    var width = texture.Width + 2;
    var height = texture.Height + 2;
    var resultColors = new Color[width * height];
    texture.GetData(colors);
    for (var x = 0; x < width; x++)
    {
      for (var y = 0; y < height; y++)
      {
        if (y == 0 || y == (height - 1) || x == 0 || x == (width - 1))
        {
          resultColors[x + y * width] = Color.Transparent;
          continue;
        }
        var index = x - 1 + (y - 1) * texture.Width;
        resultColors[x + y * width] = colors[index];
      }
    }
    var indexes = new List<int>();
    for (var x = 0; x < width; x++)
    {
      for (var y = 0; y < height; y++)
      {
        var index = x + y * width;
        var color = resultColors[index];
        if (color.A != 0)
        {
          continue;
        }

        if (
          ((x < (width - 1)) && resultColors[index + 1].A != 0) ||
          ((x > 0) && resultColors[index - 1].A != 0) ||
          ((y < (height - 1)) && resultColors[index + width].A != 0) ||
          ((y > 0) && resultColors[index - width].A != 0)
        )
        {
          indexes.Add(index);
        }
      }
    }

    foreach (var index in indexes)
    {
      resultColors[index] = Palette.White;
    }

    Texture2D newTexture = new(Core.Graphics!.GraphicsDevice, width, height);
    newTexture.SetData(resultColors);
    return newTexture;
  }
}