using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace G;

public enum TaskPriority
{
  Normal,
  High,
}

public class Task(
  string id,
  object actor,
  Func<GameTime, bool>? update = null,
  Action? before = null,
  Action? after = null,
  bool isBlockable = true,
  bool isBlocking = false,
  TaskPriority priority = 0,
  float attack = 0,
  float decay = 0,
  float release = 0,
  bool isUnique = false,
  bool overwrite = false,
  bool enableDebug = false)
{
  public string ID { get; } = id;
  public object Actor { get; } = actor;
  public bool IsBlockable { get; } = isBlockable;
  public bool IsCompleted { get; protected set; }
  public bool IsStarted { get; protected set; }
  public bool IsBlocking { get; } = isBlocking;
  public bool IsUnique { get; } = overwrite || isUnique;
  public bool Overwrite { get; } = overwrite;
  public float Attack { get; } = attack;
  public float Decay { get; } = decay;
  public float Release { get; } = release;
  public double ElapsedTime { get; protected set; }
  public bool EnableDebug { get; } = enableDebug;
  public TaskPriority Priority { get; } = priority;
  public Func<GameTime, bool>? UpdateAction { get; } = update;
  public Action? Init { get; } = before;
  public Action? OnComplete { get; } = after;

  protected bool IsAlmostCompleted { get; set; }
  protected double ExcutionTime { get; set; }
  protected bool IsInitInvoked { get; set; }

  public virtual void Update(GameTime gameTime)
  {
    Debug.Assert(!IsCompleted, "Task is already completed");
    ElapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

    if (Actor is Component component)
    {
      if (component.IsDead)
      {
        return;
      }
    }

    if (!IsInitInvoked)
    {
      Debug.WriteLine($"[{Core.Timer.Time}] Task:init: " + ID);
      Init?.Invoke();
      IsInitInvoked = true;
    }

    if (Attack != 0 && ElapsedTime < Attack)
    {
      return;
    }
    Debug.Assert((UpdateAction != null) || (Decay > 0), "Task will run forever");
    if (!IsStarted)
    {
      IsStarted = true;
      if (EnableDebug)
      {
        Debug.WriteLine($"[{Core.Timer.Time}] Task:start: " + ID);
      }
    }
    if (!IsAlmostCompleted)
    {
      ExcutionTime += gameTime.ElapsedGameTime.TotalSeconds;
      IsAlmostCompleted = UpdateAction == null ? false : UpdateAction.Invoke(gameTime);

      if (Decay != 0 && ElapsedTime >= Decay + Attack)
      {
        IsAlmostCompleted = true;
      }
    }

    if (IsAlmostCompleted && Release == 0)
    {
      IsCompleted = true;
    }

    if (IsAlmostCompleted && Release != 0 && ElapsedTime >= Attack + Math.Min(Decay, ExcutionTime) + Release)
    {
      IsCompleted = true;
    }

    if (IsCompleted)
    {
      OnComplete?.Invoke();
      if (EnableDebug)
      {
        Debug.WriteLine($"[{Core.Timer.Time}] Task:complete: " + ID);
      }
    }
  }
}