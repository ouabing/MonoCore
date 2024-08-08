using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Xna.Framework;

namespace G;

/**
 * Simple task system works a bit like coroutines,
 * which manage all the game logics that need to be done in a sequence or batch.
 *
 * Add a blocking task:
 *   R.T.AddTask(new Task(
 *     actor: this,
 *     id: "FadeCharactor",
 *     update: (gameTime) =>
 *     {
 *       character.Opacity = MathHelper.Lerp(grid.Opacity, 1f, 0f);
 *       if (character.Opacity < 0.01f) {
 *         charactoer.Opacity = 0f;
 *         return true;
 *       }
 *       return false;
 *    },
 *    isBlocking: true
 *  ));
 *
 * This will block all the tasks marked as blockable until the task is completed.
 *
 * There is also a TweenTask which can do tweening for you.
 *
 */
public class TaskSystem
{
  private readonly List<Task> normalTasks = [];
  private readonly List<Task> highTasks = [];

  public bool IsCompleted => normalTasks.Count == 0 && highTasks.Count == 0;

  public Task? Find(string id, object actor)
  {
    return normalTasks.Find(t => t.ID == id && t.Actor == actor) ?? highTasks.Find(t => t.ID == id && t.Actor == actor);
  }

  public void CompleteTask(Task task)
  {
    if (!task.IsCompleted)
    {
      task.OnComplete?.Invoke();
    }
    highTasks.Remove(task);
    normalTasks.Remove(task);
  }

  public void AddTask(Task task)
  {
    if (task.IsUnique)
    {
      if (!task.Overwrite)
      {
        if (normalTasks.Find(t => t.ID == task.ID && t.Actor == task.Actor) != null)
        {
          return;
        }
        if (highTasks.Find(t => t.ID == task.ID && t.Actor == task.Actor) != null)
        {
          return;
        }
      }
      else
      {
        foreach (var t in highTasks.FindAll(t => t.ID == task.ID && t.Actor == task.Actor))
        {
          t.OnComplete?.Invoke();
        }
        highTasks.RemoveAll(t => t.ID == task.ID && t.Actor == task.Actor);

        foreach (var t in normalTasks.FindAll(t => t.ID == task.ID && t.Actor == task.Actor))
        {
          t.OnComplete?.Invoke();
        }
        normalTasks.RemoveAll(t => t.ID == task.ID && t.Actor == task.Actor);
      }
    }
    switch (task.Priority)
    {
      case TaskPriority.Normal:
        normalTasks.Add(task);
        break;
      case TaskPriority.High:
        highTasks.Add(task);
        break;
    }
  }

  public void Run(
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
    bool enableDebug = true)
  {
    AddTask(new Task(
      id,
      actor,
      update,
      before,
      after,
      isBlockable,
      isBlocking,
      priority,
      attack,
      decay,
      release,
      isUnique,
      overwrite,
      enableDebug
    ));
  }

  public void Tween<TTarget, TMember>(
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
    bool enableDebug = true
  ) where TTarget : class where TMember : struct
  {
    AddTask(new TweenTask<TTarget, TMember>(
      id,
      target,
      expression,
      toValue,
      autoReverse,
      repeatForever,
      repeat,
      repeatDelay,
      easingFunction,
      update,
      before,
      after,
      isBlockable,
      isBlocking,
      priority,
      attack,
      decay,
      release,
      isUnique,
      overwrite,
      enableDebug
    ));
  }

  public void Delay(float seconds)
  {
    AddTask(new Task(
      id: "TaskSystem.Delay",
      actor: this,
      decay: seconds,
      isBlocking: true
    ));
  }

  public void ClearAll()
  {
    normalTasks.Clear();
    highTasks.Clear();
  }

  public bool Update(GameTime gameTime)
  {
    var blocked = false;
    blocked = UpdateBlockingTasks(gameTime, highTasks);
    if (!blocked)
    {
      blocked = UpdateBlockingTasks(gameTime, normalTasks);
    }

    UpdateNoneBlockingTasks(gameTime, highTasks, blocked);
    UpdateNoneBlockingTasks(gameTime, normalTasks, blocked);

    highTasks.RemoveAll(task => task.IsCompleted);
    normalTasks.RemoveAll(task => task.IsCompleted);
    return blocked;
  }

  private static bool UpdateBlockingTasks(GameTime gameTime, List<Task> tasks)
  {
    for (int i = 0; i < tasks.Count; i++)
    {
      var task = tasks[i];
      if (task.IsBlocking)
      {
        task.Update(gameTime);
        return true;
      }
    }
    return false;
  }

  private static void UpdateNoneBlockingTasks(GameTime gameTime, List<Task> tasks, bool blocked)
  {
    // Any task added in the loop will be updated in the next frame
    for (int i = 0; i < tasks.Count; i++)
    {
      var task = tasks[i];
      if (task.IsBlocking)
      {
        continue;
      }
      if (blocked && task.IsBlockable)
      {
        continue;
      }
      task.Update(gameTime);
    }
  }
}