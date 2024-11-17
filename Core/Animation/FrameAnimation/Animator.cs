using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace G;

/**
  * Frame animation based on a sprite sheet.
 */
public class Animator
{
  private readonly AnimationLogic Logic;
  private readonly AnimationFrames Frames;
  public Animator(AnimationFrames frames, float delay, AnimationLoopMode loopMode, Action[] actions)
  {
    Frames = frames;
    Logic = new AnimationLogic(delay, frames.FrameCount, loopMode, actions);
  }

  public Animator(AnimationFrames frames, float delay, AnimationLoopMode loopMode)
  {
    Frames = frames;
    Logic = new AnimationLogic(delay, frames.FrameCount, loopMode, []);
  }

  public Animator(AnimationFrames frames, float[] delay, AnimationLoopMode loopMode, Action[] actions)
  {
    Frames = frames;
    Logic = new AnimationLogic(delay, frames.FrameCount, loopMode, actions);
  }

  public void Update(GameTime gameTime)
  {
    Logic.Update(gameTime);
  }

  public void Draw(Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
  {
    Frames.DrawFrame(position, Logic.Frame, color, rotation, origin, scale, effects, layerDepth);
  }
}