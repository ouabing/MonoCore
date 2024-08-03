# MonoCore

Simple engine code based on MonoGame for rapid game idea prototyping (for myself)

### Dependencies

- Mono.Extended
- Mono.Extended.Tweening
- FontStashSharp.MonoGame

### Game Specific Configs

```c#
public class Def
{
  #region Core Config
  public static class Screen
  {
    public static readonly ITheme Theme = new ApolloTheme();
    public static Color BackgroundColor => Palette.Black;
    public static int TargetScreenWidth { get; } = 1440;
    public static int TargetScreenHeight { get; } = 768;
    public static int ScreenWidth { get; } = 480;
    public static int ScreenHeight { get; } = 256;
    public static int PPU { get; } = 16;
  }

  public enum Container
  {
    Scene = 1,
    World = 2,
    UI = 3,
  }

  public enum PhysicsWorld
  {
    Main = 1
  }

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
  #endregion Core Config
}
```
