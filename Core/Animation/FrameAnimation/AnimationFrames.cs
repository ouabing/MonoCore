using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace G;

/**
  * Simple frame animation just like what other engines do.
  * Give a texture, specify the frame size and frames you want to include, done.
 */
public class AnimationFrames
{
  public AnimationFrames(Texture2D texture, int frameWidth, int frameHeight, Point[] framePositions)
  {
    Texture = texture;
    FrameWidth = frameWidth;
    FrameHeight = frameHeight;
    FramePositions = framePositions;
  }

  public AnimationFrames(Texture2D texture, int frameWidth, int frameHeight, int frameCount)
  {
    Texture = texture;
    FrameWidth = frameWidth;
    FrameHeight = frameHeight;
    FramePositions = new Point[frameCount];
    for (int i = 0; i < frameCount; i++)
    {
      FramePositions[i] = new Point(i, 0);
    }
  }

  public AnimationFrames(Texture2D texture, int frameWidth, int frameHeight) :
      this(texture, frameWidth, frameHeight, texture.Width / frameWidth)
  {
  }

  public int FrameCount => FramePositions.Length;

  private Texture2D Texture { get; set; }
  private int FrameWidth { get; set; }
  private int FrameHeight { get; set; }
  private Point[] FramePositions { get; set; }

  public void DrawFrame(Vector2 position, int frameIndex, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
  {
    var frame = FramePositions[frameIndex];
    Core.Sb!.Draw(
      Texture,
      position,
      new Rectangle(frame.X * FrameWidth, frame.Y * FrameHeight, FrameWidth, FrameHeight),
      color,
      rotation,
      origin,
      scale,
      effects,
      layerDepth
    );
  }
}