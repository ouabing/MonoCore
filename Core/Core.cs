using System;
using System.Collections.Generic;
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

  private static List<IShakable> Shakables { get; } = [];
  private static List<Container> Containers { get; } = [];

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
    Graphics!.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    Graphics!.IsFullScreen = false;
    Graphics.PreferredBackBufferWidth = TargetScreenWidth;
    Graphics.PreferredBackBufferHeight = TargetScreenHeight;
    Graphics.ApplyChanges();

    foreach (Def.Layer layer in Enum.GetValues(typeof(Def.Layer)))
    {
      Layer.CreateLayer(layer);
      var isCameraFixed = Def.LayerConfig.TryGetValue(layer, out var config) && config.TryGetValue("IsCameraFixed", out var isFixed) && (bool)isFixed;
      Layer.CreateLayer(layer, isCameraFixed);
    }
  }

  public static void LoadContent()
  {
    Input.LoadContent();
    Font.LoadContent();
    Sb = new SpriteBatch(Graphics!.GraphicsDevice);
    Pb = new PrimitiveBatch(Graphics!.GraphicsDevice);
    Pd = new PrimitiveDrawing(Pb);
    Camera.LoadContent();
    AddShakable(Camera);
  }

  // Return true if the game is blocked
  public static bool Update(GameTime gameTime)
  {
    Input.Update(gameTime);
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

    if (Task.Update(gameTime))
    {
      return true;
    }

    foreach (var c in Containers)
    {
      c.Update(gameTime);
    }

    PostUpdate(gameTime);

    return false;
  }

  public static void PostUpdate(GameTime gameTime)
  {
    foreach (var c in Containers)
    {
      c.PostUpdate(gameTime);
    }
    foreach (var physicsWorld in PhysicsWorlds.Values)
    {
      physicsWorld.PostUpdate(gameTime);
    }
  }

  public static void Draw(GameTime gameTime)
  {
    Layer.Draw(gameTime);
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

  public static void AddToPhysicsWorld(Def.PhysicsWorld world, IBox box)
  {
    GetPhysicsWorld(world).Add(box);
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