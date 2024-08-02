using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace G;


public enum AnimationLoopMode { Once, Loop, PingPong }
public enum AnimationDirection { Forward, Backward }

/**
  * Standalone animation logic
  * Usage:
  *   -- Define the animation logic inside the component
  *   private AnimationLogic TiltAnim;
  *
  *   TiltAnim = new AnimationLogic(0.1f, 4, AnimationLoopMode.Loop, [
  *     () => { Rotation = MathHelper.ToRadians(0); },
  *     () => { Rotation = MathHelper.ToRadians(10); },
  *     () => { Rotation = MathHelper.ToRadians(0); },
  *     () => { Rotation = MathHelper.ToRadians(-10); }
  *   ]);
  *
  *
  * Then update the animation logic inside the component's Update, cute tilt animation just works!
  *
  */
public class AnimationLogic
{
  public int Frame { get; private set; }
  public bool IsFinished { get; private set; }
  private readonly float[] Delay;
  private readonly int Size;
  private readonly AnimationLoopMode LoopMode;
  private readonly Action[] Actions;
  private AnimationDirection Direction = AnimationDirection.Forward;
  private float Timer;

  public AnimationLogic(float delay, int size, AnimationLoopMode loopMode, Action[] actions)
  {
    Delay = Enumerable.Repeat(delay, size).ToArray();
    Size = size;
    LoopMode = loopMode;
    Actions = actions;
  }

  public AnimationLogic(float[] delay, int size, AnimationLoopMode loopMode, Action[] actions)
  {
    Delay = delay;
    Size = size;
    LoopMode = loopMode;
    Actions = actions;
  }

  public void Update(GameTime gameTime)
  {
    var dt = gameTime.GetElapsedSeconds();
    Timer += dt;
    var delay = Delay[Frame];

    if (Timer > delay)
    {
      Timer = 0;
      Frame += Direction == AnimationDirection.Forward ? 1 : -1;

      if (Frame >= Size || Frame < 0)
      {
        if (LoopMode == AnimationLoopMode.Once)
        {
          Frame = Size - 1;
          IsFinished = true;
        }
        else if (LoopMode == AnimationLoopMode.Loop)
        {
          Frame = 0;
        }
        else if (LoopMode == AnimationLoopMode.PingPong)
        {
          Direction = Direction == AnimationDirection.Forward ? AnimationDirection.Backward : AnimationDirection.Forward;
          Frame = Direction == AnimationDirection.Forward ? 1 : Size - 2;
        }
      }

      if (Frame >= 0 && Frame < Actions.Length)
      {
        Actions[Frame]?.Invoke();
      }
    }
  }
}