using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;
using FontStashSharp.RichText;
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
  public static int PPU { get; } = Def.Screen.PPU;

  #endregion  Customizable section

  public static Vector2 ScreenCenter => new(ScreenWidth / 2, ScreenHeight / 2);

  // Physics & Simulations
  // By default there is only one world called Main
  public static Dictionary<Def.PhysicsWorld, PhysicsWorld> PhysicsWorlds { get; } = [];
  // Quick accessor for the default physics world
  public static WindSim Wind { get; } = new WindSim();

  public static bool DebugComponent { get; }
  public static Camera Camera { get; } = new Camera();
  public static LayerManager LayerManager { get; } = new LayerManager(Def.Screen.BackgroundColor);
  public static Timer Timer { get; } = new Timer();
  public static InputManager I { get; } = new InputManager();
  public static TaskSystem T { get; } = new TaskSystem();
  public static GraphicsDeviceManager? GraphicsManager { get; private set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  public static GraphicsDevice GraphicsDevice => GraphicsManager!.GraphicsDevice;
  public static TextureManager TextureManager { get; private set; }
  public static EffectManager EffectManager { get; private set; }
  public static ContentManager ContentManager { get; private set; }
  public static SpriteBatch Sb { get; private set; }
  public static PrimitiveBatch Pb { get; private set; }
  public static PrimitiveDrawing Pd { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  public static GRandom Random { get; } = new GRandom();

  private static FontSystem? fontSystem;
  private static Dictionary<FontSize, SpriteFontBase> FontCache { get; } = [];
  private static List<IShakable> Shakables { get; } = [];
  private static List<Container> Containers { get; } = [];

  public static SpriteFontBase Font(FontSize size = FontSize.Medium)
  {
    return FontCache[size];
  }

  public static RichTextLayout RichText(SpriteFontBase font, string text, int width, int height)
  {
    return new RichTextLayout()
    {
      Font = font,
      Text = text,
      Width = width,
      Height = height
    };
  }
  public static RichTextLayout RichText(SpriteFontBase font, string text, int width)
  {
    return new RichTextLayout()
    {
      Font = font,
      Text = text,
      Width = width,
    };
  }
  public static void Init(ContentManager contentManager, Game gameRoot)
  {
    ContentManager = contentManager;
    ContentManager.RootDirectory = "Content";
    GraphicsManager = new GraphicsDeviceManager(gameRoot)
    {
      PreferMultiSampling = false,
      GraphicsProfile = GraphicsProfile.HiDef
    };
    TextureManager = new TextureManager(contentManager);
    EffectManager = new EffectManager(contentManager);
    foreach (Def.PhysicsWorld world in Enum.GetValues(typeof(Def.PhysicsWorld)))
    {
      CreatePhysicsWorld(world);
    }

    foreach (Def.Container name in Enum.GetValues(typeof(Def.Container)))
    {
      CreateContainer(name);
    }
    Palette.SetTheme(Def.Screen.Theme);
  }

  public static void InitializeGraphics()
  {
    GraphicsManager!.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    GraphicsManager!.IsFullScreen = false;
    GraphicsManager.PreferredBackBufferWidth = TargetScreenWidth;
    GraphicsManager.PreferredBackBufferHeight = TargetScreenHeight;
    GraphicsManager.ApplyChanges();

    foreach (Def.Layer layer in Enum.GetValues(typeof(Def.Layer)))
    {
      LayerManager.CreateLayer(layer);
      var isCameraFixed = Def.LayerConfig.TryGetValue(layer, out var config) && config.TryGetValue("IsCameraFixed", out var isFixed) && (bool)isFixed;
      LayerManager.CreateLayer(layer, isCameraFixed);
    }
  }

  public static void LoadContent()
  {
    fontSystem = new FontSystem();
    fontSystem.AddFont(File.ReadAllBytes(@"Content/Font/zpix.ttf"));
    Sb = new SpriteBatch(GraphicsManager!.GraphicsDevice);
    Pb = new PrimitiveBatch(GraphicsManager!.GraphicsDevice);
    Pd = new PrimitiveDrawing(Pb);
    Camera.LoadContent();
    AddShakable(Camera);

    foreach (FontSize fontSize in Enum.GetValues(typeof(FontSize)))
    {
      FontCache[fontSize] = fontSystem.GetFont((int)fontSize);
    }
  }

  // Return true if the game is blocked
  public static bool Update(GameTime gameTime)
  {
    I.Update(gameTime);
    foreach (var physicsWorld in PhysicsWorlds.Values)
    {
      physicsWorld.Update(gameTime);
    }

    Timer.Update(gameTime);

    Wind.Update(gameTime);
    foreach (var shakable in Shakables)
    {
      shakable.Shaker!.Update(gameTime);
    }

    Camera.Update(gameTime);

    if (T.Update(gameTime))
    {
      return true;
    }

    foreach (var c in Containers)
    {
      c.Update(gameTime);
    }

    return false;
  }

  public static void Draw(GameTime gameTime)
  {
    LayerManager.Draw(gameTime);
  }

  private static void CreateContainer(Def.Container Name)
  {
    if (Containers.Exists(c => c.Name == Name))
    {
      throw new ArgumentException($"Container {Name} already exists.");
    }
    Containers.Add(new Container(Name, (int)Name));
    Containers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
  }

  public static void AddToContainer(Def.Container Name, Component component)
  {
    var container = Containers.Find(c => c.Name == Name) ?? throw new ArgumentException($"Container {Name} not found.");

    container.Add(component);
  }

  public static void RemoveFromContainer(Def.Container Name, Component component)
  {
    var container = Containers.Find(c => c.Name == Name) ?? throw new ArgumentException($"Container {Name} not found.");

    container.Remove(component);
  }

  public static void AddShakable(IShakable shakable)
  {
    shakable.CreateShaker();
    if (Shakables.Contains(shakable))
    {
      return;
    }
    Shakables.Add(shakable);
  }

  public static PhysicsWorld GetPhysicsWorld(Def.PhysicsWorld world)
  {
    if (!PhysicsWorlds.TryGetValue(world, out PhysicsWorld? value))
    {
      throw new KeyNotFoundException($"Physics world {world} not found.");
    }
    return value;
  }

  public static void CreatePhysicsWorld(Def.PhysicsWorld world)
  {
    if (PhysicsWorlds.ContainsKey(world))
    {
      throw new ArgumentException($"Physics world {world} already exists.");
    }
    PhysicsWorlds[world] = new PhysicsWorld();
  }
}