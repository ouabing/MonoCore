using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.VectorDraw;

namespace G;

public static class Core
{

  public static ContainerManager Container { get; } = new();
  public static WindSim Wind { get; } = new WindSim();
  public static bool Paused { get; set; }
  public static Viewport Viewport { get; set; }

  private static bool enableDebug;
  public static bool EnableDebug
  {
    get
    {
      return enableDebug;
    }
    set
    {
      if (value)
      {
        Debugger.Enable();
      }
      else
      {
        Debugger.Disable();
      }
      enableDebug = value;
    }
  }
  public static bool EnablePositionDebug { get; set; }
  public static GameWindow Window { get; private set; } = null!;
  public static Screen Screen { get; private set; } = null!;
  public static PhysicsManager Physics { get; } = new();
  public static Camera Camera { get; } = new Camera();
  public static LayerManager Layer { get; } = new LayerManager(Def.Screen.BackgroundColor);
  public static Timer Timer { get; } = new Timer();
  public static InputManager Input { get; } = new InputManager();
  public static FontManager Font { get; } = new FontManager();
  public static TaskSystem Task { get; } = new TaskSystem();
  public static GraphicsDeviceManager? Graphics { get; private set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  public static GraphicsDevice GraphicsDevice => Graphics!.GraphicsDevice;
  public static TextureManager Texture { get; private set; }
  public static EffectManager Effect { get; private set; }
  public static ContentManager Content { get; private set; }
  public static SpriteBatch Sb { get; private set; }
  public static PrimitiveBatch Pb { get; private set; }
  public static PrimitiveDrawing Pd { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  public static GRandom Random { get; } = new GRandom();
  public static AnimationManager Animation { get; } = new();
  public static Debugger Debugger { get; } = new();
  public static Inspector Inspector { get; } = new();
  public static GameConsole Console { get; } = new();

  private static bool contentLoaded;
  private static bool graphicsInitialized;

  public static void Init(ContentManager contentManager, Game gameRoot)
  {
    Content = contentManager;
    Content.RootDirectory = "Content";
    Graphics = new GraphicsDeviceManager(gameRoot)
    {
      PreferMultiSampling = false,
      GraphicsProfile = GraphicsProfile.HiDef
    };
    Screen = new Screen(Graphics, Def.Screen.Width, Def.Screen.Height, Def.Screen.WindowedModeWidth, Def.Screen.WindowedModeHeight);
    Texture = new TextureManager(contentManager);
    Effect = new EffectManager(contentManager);
    Physics.CreateWorlds();
    Palette.SetTheme(Def.Screen.Theme);
  }

  public static void InitializeGraphics(GameWindow window)
  {
    if (graphicsInitialized)
    {
      throw new InvalidOperationException("Graphics already initialized");
    }
    graphicsInitialized = true;

    Window = window;
    Screen.Initialize(Window);
    // UpdateViewport();
    Layer.Initialize();
  }

  public static void LoadContent()
  {
    if (contentLoaded)
    {
      throw new InvalidOperationException("Content already loaded");
    }
    contentLoaded = true;
    Input.LoadContent();
    Font.LoadContent();
    Sb = new SpriteBatch(Graphics!.GraphicsDevice);
    Pb = new PrimitiveBatch(Graphics!.GraphicsDevice);
    Pd = new PrimitiveDrawing(Pb);
    Camera.LoadContent();
    Console.LoadContent();
    Inspector.LoadContent();
  }

  // Return true if the game is blocked
  public static bool Update(GameTime gameTime)
  {
    Console.Update(gameTime);
    Inspector.Update(gameTime);
    Input.Update(gameTime);
    if (Paused)
    {
      return false;
    }
    Timer.Update(gameTime);

    Wind.Update(gameTime);
    Animation.Update(gameTime);

    Camera.Update(gameTime);

    if (Task.Update(gameTime))
    {
      return true;
    }

    Container.Update(gameTime);
    Physics.Update(gameTime);

    PostUpdate(gameTime);

    return false;
  }

  public static void PostUpdate(GameTime gameTime)
  {
    Input.PostUpdate(gameTime);
    Container.PostUpdate(gameTime);
  }

  public static void Draw(GameTime gameTime)
  {
    Layer.Draw(gameTime);
    Inspector.Draw(gameTime);
    Console.Draw(gameTime);
  }
  // public static void UpdateViewport()
  // {
  //   int screenWidth = Graphics.PreferredBackBufferWidth;
  //   int screenHeight = Graphics.PreferredBackBufferHeight;

  //   int gameWidth = Def.Screen.TargetScreenWidth;
  //   int gameHeight = Def.Screen.TargetScreenHeight;

  //   float scaleX = (float)screenWidth / gameWidth;
  //   float scaleY = (float)screenHeight / gameHeight;
  //   float scale = Math.Min(scaleX, scaleY); // 保持比例缩放

  //   int viewportWidth = (int)(gameWidth * scale);
  //   int viewportHeight = (int)(gameHeight * scale);
  //   int viewportX = (screenWidth - viewportWidth) / 2;
  //   int viewportY = (screenHeight - viewportHeight) / 2;

  //   Viewport = new Viewport(viewportX, viewportY, viewportWidth, viewportHeight);
  // }

  // public static void SetFullscreen()
  // {
  //   Graphics!.IsFullScreen = true;
  //   var adapter = GraphicsAdapter.DefaultAdapter;
  //   Graphics.PreferredBackBufferWidth = adapter.CurrentDisplayMode.Width;
  //   Graphics.PreferredBackBufferHeight = adapter.CurrentDisplayMode.Height;

  //   Graphics.ApplyChanges();
  //   // UpdateViewport();
  // }

  // public static void SetWindow()
  // {
  //   Graphics!.IsFullScreen = false;
  //   Graphics.PreferredBackBufferWidth = Def.Screen.TargetScreenWidth;
  //   Graphics.PreferredBackBufferHeight = Def.Screen.TargetScreenHeight;

  //   Graphics.ApplyChanges();
  //   // UpdateViewport();
  // }
}