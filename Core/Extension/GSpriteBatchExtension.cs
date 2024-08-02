using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace G;

public static class GSpriteBatchExtension
{
  private static Texture2D? _blankTexture;

  public static Texture2D BlankTexture(this SpriteBatch s)
  {
    if (_blankTexture == null)
    {
      _blankTexture = new Texture2D(s.GraphicsDevice, 1, 1);
      _blankTexture.SetData(new[] { Color.White });
    }
    return _blankTexture;
  }
}