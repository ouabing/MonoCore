using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace G;

public class ConsoleKeyBuffer
{
  private Keys currentKey { get; set; } = Keys.None;
  private double initialPressTime;
  private double lastRepeatTime;
  private bool isKeyHeld;
  private readonly float initialDelay = 0.5f;
  private readonly float repeatInterval = 0.05f;
  private Keys[] previousPressedKeys = [];

  public Keys UpdateKeys(GameTime gameTime)
  {
    var state = Keyboard.GetState();
    var pressedKeys = state.GetPressedKeys();
    var diffKeys = pressedKeys.Except(previousPressedKeys).ToArray();

    previousPressedKeys = pressedKeys;

    var currentTime = gameTime.TotalGameTime.TotalSeconds;

    var keys = diffKeys.Length > 0 ? diffKeys : [];

    if (keys.Length == 0 && pressedKeys.Contains(currentKey))
    {
      keys = [currentKey];
    }
    if (keys.Length > 0)
    {
      Keys key = keys.Last();

      if (key != currentKey)
      {
        currentKey = key;
        initialPressTime = currentTime;
        lastRepeatTime = currentTime;
        isKeyHeld = true;

        return currentKey;
      }
      else if (isKeyHeld)
      {
        // test if the key has been held down long enough to start repeating
        if ((currentTime - initialPressTime) >= initialDelay)
        {
          // start repeating
          if ((currentTime - lastRepeatTime) >= repeatInterval)
          {
            lastRepeatTime = currentTime;
            return currentKey;
          }
        }
      }
    }
    else
    {
      // if no key is pressed, reset the state
      isKeyHeld = false;
      currentKey = Keys.None;
    }

    return Keys.None;

  }
}