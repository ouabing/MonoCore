# MonoCore

A simple game engine based on [MonoGame](https://monogame.net/)

### Dependencies

- Mono.Extended>=4.0.0
- FontStashSharp.MonoGame

### Features

- Simple AABB collision detection, and a complete physics engine integretion by [Aether.Physics2D](https://github.com/nkast/Aether.Physics2D)
- Simple task system you can schedule async game logic on the fly, there's also TweenTask if you want to throw a tween animation
- In game debug console with interactive C# runner, component inspector, and custom command support
- Animated text using just markdown-like syntax
- Smooth fluid simulation
- Layers for ordered drawing, Containers for managing component updating 
- Input action mapping
- Frame animation, Shaking, Spring and other useful 2D animations
- Simple GOAP for AI logic
- 2D lighting (WIP)
- Other utilities like Camera, Grid Map Generator, Color Palette and bunch of effects driven by shaders

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
  }

  // Container for updating component group
  public enum Container
  {
    Scene = 1,
    World = 2,
    UI = 3,
  }

  // Define the collision category masks
  public enum Category
  {
    // Must have
    All = int.MaxValue,
    None = 0,
    Default = 1,

    // Your custom values
    Human = 1 << 1,
    Bird = 1 << 2,
  }

  // Layer for drawing components
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


  // Fonts, the first font will be the default font
  public static readonly List<(string, string)> Fonts = [
    ("fusion8", "Content/Fonts/fusion8.ttf"),
    ("fusion10", "Content/Fonts/fusion10.ttf")
  ];

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
    // It's useful if you have multiple layers like Battleground and Menu
    // You can switch the input world to avoid triggering actions in both worlds
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
