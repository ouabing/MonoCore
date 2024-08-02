using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace G;

public static class Core
{
  #region  Customizable section
  // Resolution
  public static int TargetScreenWidth { get; } = 1440;
  public static int TargetScreenHeight { get; } = 768;
  public static int ScreenWidth { get; } = 480;
  public static int ScreenHeight { get; } = 256;


  #endregion  Customizable section

  public static Vector2 ScreenCenter => new(ScreenWidth / 2, ScreenHeight / 2);

  // Physics & Simulations
  // By default there is only one world called Main
  public static Dictionary<string, PhysicsWorld> PhysicsWorlds { get; } = [];
  // Quick accessor for the default physics world
  public static PhysicsWorld PhysicsWorld => GetPhysicsWorld("Earth");
  public static WindSim Wind { get; } = new WindSim();

  public static bool DebugComponent { get; }
  public static Camera Camera { get; } = new Camera();
  public static LayerManager LayerManager { get; } = new LayerManager(Palette.Black);
  public static Timer Timer { get; } = new Timer();
  public static InputManager I { get; } = new InputManager();
  public static TaskSystem T { get; } = new TaskSystem();
  public static GraphicsDeviceManager? GraphicsManager { get; private set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  public static GraphicsDevice GraphicsDevice => GraphicsManager!.GraphicsDevice;
  public static TextureManager TextureManager { get; private set; }
  public static EffectManager EffectManager { get; private set; }
  public static ContentManager ContentManager { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  public static SpriteBatch? Sb { get; private set; }
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
    CreatePhysicsWorld("Earth");

    foreach (ContainerDef name in Enum.GetValues(typeof(ContainerDef)))
    {
      CreateContainer(name);
    }

  }

  public static void InitializeGraphics()
  {
    GraphicsManager!.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    GraphicsManager!.IsFullScreen = false;
    GraphicsManager.PreferredBackBufferWidth = TargetScreenWidth;
    GraphicsManager.PreferredBackBufferHeight = TargetScreenHeight;
    GraphicsManager.ApplyChanges();

    #region Define custom layers
    LayerManager.CreateLayer("Terrain", 0, false);
    LayerManager.CreateLayer("WorldObject", 1, false);
    LayerManager.CreateLayer("DevUI", 10000, true);
    #endregion Define custom layers
  }

  public static void LoadContent(ContentManager content)
  {
    fontSystem = new FontSystem();
    fontSystem.AddFont(File.ReadAllBytes(@"Content/Font/zpix.ttf"));
    Sb = CreateSpriteBatch();
    Camera.LoadContent();
    AddShakable(Camera);

    foreach (FontSize fontSize in Enum.GetValues(typeof(FontSize)))
    {
      FontCache[fontSize] = fontSystem.GetFont((int)fontSize);
    }
  }

  public static SpriteBatch CreateSpriteBatch()
  {
    return new SpriteBatch(GraphicsManager!.GraphicsDevice);
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

  private static void CreateContainer(ContainerDef Name)
  {
    if (Containers.Exists(c => c.Name == Name))
    {
      throw new ArgumentException($"Container {Name} already exists.");
    }
    Containers.Add(new Container(Name, (int)Name));
    Containers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
  }

  public static void AddToContainer(ContainerDef Name, Component component)
  {
    var container = Containers.Find(c => c.Name == Name) ?? throw new ArgumentException($"Container {Name} not found.");

    container.Add(component);
  }

  public static void RemoveFromContainer(ContainerDef Name, Component component)
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

  public static PhysicsWorld GetPhysicsWorld(string name)
  {
    if (!PhysicsWorlds.TryGetValue(name, out PhysicsWorld? value))
    {
      throw new KeyNotFoundException($"Physics world {name} not found.");
    }
    return value;
  }

  public static void CreatePhysicsWorld(string name)
  {
    if (PhysicsWorlds.ContainsKey(name))
    {
      throw new ArgumentException($"Physics world {name} already exists.");
    }
    PhysicsWorlds[name] = new PhysicsWorld();
  }
}