using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.VectorDraw;

namespace G;

public static class Core
{
  #region  Customizable section

  // Resolution
  public static int TargetScreenWidth { get; } = Def.Screen.TargetScreenWidth;
  public static int TargetScreenHeight { get; } = Def.Screen.TargetScreenHeight;
  public static int ScreenWidth { get; } = Def.Screen.ScreenWidth;
  public static int ScreenHeight { get; } = Def.Screen.ScreenHeight;

  #endregion  Customizable section

  public static Vector2 ScreenCenter => new(ScreenWidth / 2, ScreenHeight / 2);

  public static ContainerManager Container { get; } = new();
  public static WindSim Wind { get; } = new WindSim();

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
    Texture = new TextureManager(contentManager);
    Effect = new EffectManager(contentManager);
    Physics.CreateWorlds();
    Palette.SetTheme(Def.Screen.Theme);
  }

  public static void InitializeGraphics()
  {
    if (graphicsInitialized)
    {
      throw new InvalidOperationException("Graphics already initialized");
    }
    graphicsInitialized = true;

    Graphics!.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    Graphics!.IsFullScreen = false;
    Graphics.PreferredBackBufferWidth = TargetScreenWidth;
    Graphics.PreferredBackBufferHeight = TargetScreenHeight;
    Graphics.ApplyChanges();
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
  }

  // Return true if the game is blocked
  public static bool Update(GameTime gameTime)
  {
    Input.Update(gameTime);
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
    Container.PostUpdate(gameTime);
  }

  public static void Draw(GameTime gameTime)
  {
    Layer.Draw(gameTime);
  }
}