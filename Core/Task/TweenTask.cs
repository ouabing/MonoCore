using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Tweening;

namespace G;
public class TweenTask<TTarget, TMember>(
  string id,
  TTarget target,
  Expression<Func<TTarget, TMember>> expression,
  TMember toValue,
  bool autoReverse = false,
  bool repeatForever = false,
  int repeat = 0,
  float repeatDelay = 0,
  Func<float, float>? easingFunction = null,
  Func<GameTime, bool>? update = null, Action? before = null, Action? after = null,
  bool isBlockable = true, bool isBlocking = false,
  TaskPriority priority = TaskPriority.Normal,
  float attack = 0, float decay = 0, float release = 0,
  bool isUnique = false, bool overwrite = false,
  bool enableDebug = true) :
  Task(id, target, update, before, after, isBlockable, isBlocking, priority, attack, decay, release, isUnique, overwrite, enableDebug), IDisposable where TTarget : class where TMember : struct
{

  private bool AutoReverse { get; } = autoReverse;
  private int Repeat { get; } = repeat;
  private bool RepeatForever { get; } = repeatForever;
  private float RepeatDelay { get; } = repeatDelay;
  private TTarget Target { get; } = target;
  private readonly Expression<Func<TTarget, TMember>> Expression = expression;
  private TMember ToValue { get; } = toValue;
  private Func<float, float>? EasingFunction = easingFunction;
  private Tweener tweener = new();
  private bool IsTweenerStarted { get; set; }
  private float? finishedAt;
  public override void Update(GameTime gameTime)
  {
    Debug.Assert(!IsCompleted, "TweenTask:Error:Task is already completed");

    if (Actor is Component component)
    {
      if (component.IsDead)
      {
        return;
      }
    }

    if (!IsInitInvoked)
    {
      Init?.Invoke();
      IsInitInvoked = true;
    }
    if (!IsTweenerStarted)
    {
      var tween = tweener.TweenTo(Target, Expression, ToValue, Decay, Attack)
             .OnBegin((tween) =>
             {
               if (IsStarted)
               {
                 return;
               }
               IsStarted = true;
               if (EnableDebug)
               {
                 Debug.WriteLine($"[{Core.Timer.Time}] TweenTask:start: {ID}");
               }
             })
             .OnEnd((tween) =>
             {
               // Let the tweener finish repeatings
               if (tween.IsAlive)
               {
                 return;
               }
               finishedAt = (float)Core.Timer.Time;
               IsAlmostCompleted = true;
               if (Release == 0)
               {
                 IsCompleted = true;
                 OnComplete?.Invoke();
                 if (EnableDebug)
                 {
                   Debug.WriteLine($"[{Core.Timer.Time}] TweenTask:complete: {ID}");
                 }
               }
             });
      if (EasingFunction != null)
      {
        tween.Easing(EasingFunction);
      }
      else
      {
        tween.Easing(EasingFunctions.SineInOut);
      }
      if (Repeat != 0)
      {
        tween.Repeat(Repeat, RepeatDelay);
      }
      if (AutoReverse)
      {
        tween.AutoReverse();
      }
      if (RepeatForever)
      {
        tween.RepeatForever(RepeatDelay);
      }
      IsTweenerStarted = true;
    }

    if (UpdateAction != null)
    {
      if (UpdateAction(gameTime))
      {
        tweener.CancelAndCompleteAll();
      }
    }

    if (IsCompleted == true)
    {
      return;
    }

    if (IsTweenerStarted && !IsAlmostCompleted)
    {
      tweener.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    if (IsAlmostCompleted && Release != 0 && Core.Timer.Time >= finishedAt + Release)
    {
      IsCompleted = true;
      OnComplete?.Invoke();
      if (EnableDebug)
      {
        Debug.WriteLine($"[{Core.Timer.Time}] Task:complete: {ID}");
      }
    }
  }
  public void Dispose()
  {
    tweener.Dispose();
    GC.SuppressFinalize(this);
  }
}