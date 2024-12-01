using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace G;

public class ImageCommand : ConsoleCommand
{
  private List<ImageCommandImage> images = [];
  public ImageCommand() : base(
    "image",
    "Load an image onto screen\n\n" +
    "commands:\n" +
    "  clear: clear all the rendered image\n",
    "image [path to image]",
    []
  )
  {
  }

  public override void Execute(GameConsole console, string[] args)
  {
    if (args.Length == 0)
    {
      PrintHelp(console);
      return;
    }
    var path = string.Join(" ", args);
    if (path == "--help")
    {
      PrintHelp(console);
      return;
    }
    if (path == "clear")
    {
      foreach (var i in images)
      {
        i.Die();
      }
      images.Clear();
      return;
    }

    if (!File.Exists(path))
    {
      console.PrintError($"image: file not found: {path}");
      return;
    }

    if (!IsImageFile(path))
    {
      console.PrintError($"image: invalid image file: {path}");
      return;
    }

    var image = new ImageCommandImage(path);
    image.LoadContent();
    Core.StandaloneComponents.Add(image);
  }

  public static bool IsImageFile(string fileName)
  {
    if (string.IsNullOrEmpty(fileName))
      return false;

    string extension = Path.GetExtension(fileName).ToLowerInvariant();

    string[] validExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp"];

    return Array.Exists(validExtensions, ext => ext == extension);
  }
}

public class ImageCommandImage(string path) : Component
{
  public override void LoadContent()
  {
    Texture = Core.Texture.LoadTextureFromStream(path);
    Position = new Vector2((Core.Screen.DisplayWidth - Texture.Width) / 2.0f, (Core.Screen.DisplayHeight - Texture.Height) / 2.0f);
    OriginType = OriginType.Center;
    base.LoadContent();
  }

  public override void Update(GameTime gameTime)
  {
  }

  public override void Draw(GameTime gameTime)
  {
    Core.Sb.Begin();
    Core.Sb.Draw(
      texture: Texture,
      position: Position,
      sourceRectangle: null,
      color: Color.White,
      rotation: Rotation,
      origin: Origin,
      scale: Scale,
      effects: SpriteEffects.None,
      layerDepth: 0
    );
    Core.Sb.End();
  }
}
