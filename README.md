# MonoCore

A simple game engine based on [MonoGame](https://monogame.net/)

### Dependencies

- Mono.Extended
- Mono.Extended.Tweening
- FontStashSharp.MonoGame

### Game Specific Configs

```c#
public class Def
{
  #region Core Config

  // Screen related
  public static class Screen
  {
    public static readonly ITheme Theme = new ApolloTheme();
    public static Color BackgroundColor => Palette.Black;
    public static readonly int TargetScreenWidth = 1440;
    public static readonly int TargetScreenHeight = 768;
    public static readonly int ScreenWidth = 480;
    public static readonly int ScreenHeight = 256;
    public static readonly int PPU = 16;
  }

  // Container for update component group
  public enum Container
  {
    Scene = 1,
    World = 2,
    UI = 3,
  }

  // PhysicsWorld for collision detection and other physics simulation stuff
  public enum PhysicsWorld
  {
    Main = 1
  }

  // Layer for draws components
  public enum Layer
  {
    DevUI = 0,
    Universe = 10,
    Background = 100,
  }

  public static readonly Dictionary<Layer, Dictionary<string, object>> LayerConfig = new()
  {
    [Layer.DevUI] = new Dictionary<string, object>
    {
      { "IsCameraFixed", true }
    }
  };

  // Input related
  public static class Input
  {

    // Define input action for binding, which may be a specific operation
    public enum Action
    {
      CursorPressed = 1,
      Left = 2,
    }

    // Define the input separation layers
    // It's useful if you have multiple layer like Battleground and Menu
    // You can switch the input world to avoid trigger actions in both worlds
    public enum World
    {
      Battleground = 1,
      Menu = 2,
    }

    public static readonly Dictionary<Action, List<string>> Bindings = new()
    {
      { Action.CursorPressed, ["MouseLeftPressed"] },
      { Action.Left, ["Left", "StickLeftX-"] },
    };
  }
  #endregion Core Config
}
```
