using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace G;

public enum FluidDiffuseType
{
  Gauss = 0,
  DensityGradient = 1,
  Random = 2,
  Wind = 3
}

public class FluidSim : Component, IDisposable
{
  public Texture2D? BoundaryTexture { get; set; }

  private Effect advectionEffect;
  private Effect vorticityEffect;
  private Effect clearEffect;
  private Effect pressureEffect;
  private Effect splatEffect;
  private Effect gradientSubtractEffect;
  private RenderTarget2D velocityField;
  private RenderTarget2D pressureField;
  private RenderTarget2D tempField;
  private RenderTarget2D dyeField;
  public bool LinearFiltering { get; set; }

  // Radius in pixel
  public float SplatRadius { get; set; } = 0.25f;
  public float TexelSize { get; set; } = 1.0f;
  // 0 ~ 0.4f
  public float VelocityDissipation { get; set; } = 0.2f;
  // 0 ~ 4.0f
  public float DensityDissipation { get; set; } = 1.0f;
  public int PressureIterations { get; set; } = 20;
  // 0 ~ 1
  public float PressureClearValue { get; set; } = 0.8f;
  // 0 ~ 50
  public float CurlAmount { get; set; } = 30f;
  public float SplatDuration { get; set; } = 10.0f / 60;
  public float MaxDensity { get; set; } = 1.0f;
  public bool Debug { get; set; }
  public int FramesPerStep { get; set; } = 1;
  private int FramesCounter { get; set; }
  private float splatTimer;

  public override void LoadContent()
  {
    EnableSpriteBatch = false;
    int width = Core.Screen.Width;
    int height = Core.Screen.Height;

    // We need SurfaceFormat.Vector4 to store velocity that has value larger than 255
    velocityField = new RenderTarget2D(
      Core.GraphicsDevice,
      width,
      height,
      false,
      SurfaceFormat.HalfVector4,
      DepthFormat.None,
      0,
      RenderTargetUsage.PreserveContents
    );
    pressureField = new RenderTarget2D(
      Core.GraphicsDevice,
      width,
      height,
      false,
      SurfaceFormat.HalfVector4,
      DepthFormat.None,
      0,
      RenderTargetUsage.PreserveContents
    );
    tempField = new RenderTarget2D(
      Core.GraphicsDevice,
      width,
      height,
      false,
      SurfaceFormat.HalfVector4,
      DepthFormat.None,
      0,
      RenderTargetUsage.PreserveContents
    );
    dyeField = new RenderTarget2D(
      Core.GraphicsDevice,
      width,
      height,
      false,
      SurfaceFormat.HalfVector4,
      DepthFormat.None,
      0,
      RenderTargetUsage.PreserveContents
    );

    Core.GraphicsDevice.SetRenderTarget(dyeField);
    Core.GraphicsDevice.Clear(Color.Transparent);
    Core.GraphicsDevice.SetRenderTarget(null);

    splatEffect = Core.Effect.LoadEffect("MonoCore/Shader/Fluid/Splat").Clone();
    clearEffect = Core.Effect.LoadEffect("MonoCore/Shader/Fluid/FluidClear").Clone();
    advectionEffect = Core.Effect.LoadEffect("MonoCore/Shader/Fluid/Advection").Clone();
    vorticityEffect = Core.Effect.LoadEffect("MonoCore/Shader/Fluid/Vorticity").Clone();
    pressureEffect = Core.Effect.LoadEffect("MonoCore/Shader/Fluid/FluidPressure").Clone();
    gradientSubtractEffect = Core.Effect.LoadEffect("MonoCore/Shader/Fluid/GradientSubtract").Clone();
  }

  public override void Update(GameTime gameTime)
  {
    if (Debug)
    {
      var mouseState = Mouse.GetState();

      if (mouseState.LeftButton == ButtonState.Pressed)
      {
        if (splatTimer <= 0)
        {
          var rand = new GRandom();
          SplatOnScreenPosition(
            new Vector2((float)mouseState.X / Core.Screen.DisplayWidth, (float)mouseState.Y / Core.Screen.DisplayHeight),
            GenerateSplatVelocity(),
            new Color(rand.NextSingle() * 0.3f, 0.3f, 0.3f, 1.0f)
          );
          splatTimer = SplatDuration;
        }
        else
        {
          splatTimer -= gameTime.GetElapsedSeconds();
        }
      }
    }
  }

  private static Vector2 GenerateSplatVelocity()
  {
    var rand = new GRandom();
    return new Vector2(rand.NextSingle(-1, 1), rand.NextSingle(-1, 1)) * 400f;
  }

  public override void Draw(GameTime gameTime)
  {
    FramesCounter++;
    if (FramesCounter >= FramesPerStep)
    {
      FramesCounter = 0;
      Simulate(gameTime);
    }
    DrawResult(dyeField);

    base.Draw(gameTime);
  }

  private void Simulate(GameTime gameTime)
  {
    float dt = gameTime.GetElapsedSeconds();
    var texelVec = new Vector2(
      TexelSize / velocityField.Width,
      TexelSize / velocityField.Height
    );

    // Vorticity
    ApplyEffect(vorticityEffect, velocityField, tempField, parameters =>
    {
      parameters["dt"].SetValue(dt);
      parameters["texelSize"].SetValue(texelVec);
      parameters["curlAmount"].SetValue(CurlAmount);
      if (BoundaryTexture != null)
      {
        parameters["BoundarySampler"].SetValue(BoundaryTexture);
        parameters["enableBoundary"].SetValue(1.0f);
      }
      else
      {
        parameters["enableBoundary"].SetValue(0.0f);
      }
    });
    Swap(ref velocityField, ref tempField);

    // Clear Pressure
    ApplyEffect(clearEffect, pressureField, tempField, parameters =>
    {
      parameters["value"].SetValue(PressureClearValue);
    });
    Swap(ref pressureField, ref tempField);

    // Pressure Solve
    for (var i = 0; i < PressureIterations; i++)
    {
      ApplyEffect(pressureEffect, pressureField, tempField, parameters =>
      {
        if (BoundaryTexture != null)
        {
          parameters["BoundarySampler"].SetValue(BoundaryTexture);
          parameters["enableBoundary"].SetValue(1.0f);
        }
        else
        {
          parameters["enableBoundary"].SetValue(0.0f);
        }
        parameters["texelSize"].SetValue(texelVec);
        parameters["VelocitySampler"].SetValue(velocityField);
      });
      Swap(ref pressureField, ref tempField);
    }

    // Gradient Subtract
    ApplyEffect(gradientSubtractEffect, velocityField, tempField, parameters =>
    {
      parameters["texelSize"].SetValue(texelVec);
      parameters["VelocitySampler"].SetValue(velocityField);
      parameters["PressureSampler"].SetValue(pressureField);
    });
    Swap(ref velocityField, ref tempField);

    // Advect Velocity Field
    ApplyEffect(advectionEffect, velocityField, tempField, parameters =>
    {
      parameters["dyeTexelSize"].SetValue(texelVec);
      parameters["texelSize"].SetValue(texelVec);
      parameters["dt"].SetValue(dt);
      parameters["dissipation"].SetValue(VelocityDissipation);
      parameters["linearFiltering"].SetValue(LinearFiltering ? 1.0f : 0.0f);
      parameters["SourceSampler"].SetValue(velocityField);
      parameters["VelocitySampler"].SetValue(velocityField);

      if (BoundaryTexture != null)
      {
        parameters["BoundarySampler"].SetValue(BoundaryTexture);
        parameters["enableBoundary"].SetValue(1.0f);
      }
      else
      {
        parameters["enableBoundary"].SetValue(0.0f);
      }
    });
    Swap(ref velocityField, ref tempField);

    // Advect Dye Field
    ApplyEffect(advectionEffect, dyeField, tempField, parameters =>
    {
      parameters["dyeTexelSize"].SetValue(texelVec);
      parameters["texelSize"].SetValue(texelVec);
      parameters["dt"].SetValue(dt);
      parameters["dissipation"].SetValue(DensityDissipation);
      parameters["linearFiltering"].SetValue(LinearFiltering ? 1.0f : 0.0f);
      parameters["SourceSampler"].SetValue(dyeField);
      parameters["VelocitySampler"].SetValue(velocityField);
      if (BoundaryTexture != null)
      {
        parameters["BoundarySampler"].SetValue(BoundaryTexture);
        parameters["enableBoundary"].SetValue(1.0f);
      }
      else
      {
        parameters["enableBoundary"].SetValue(0.0f);
      }
    });
    Swap(ref dyeField, ref tempField);
  }

  private void DrawResult(RenderTarget2D texture)
  {
    var previousRenderTargets = Core.GraphicsDevice.GetRenderTargets();
    RenderTarget2D? previousRenderTarget = previousRenderTargets.Length > 0 ? (RenderTarget2D)previousRenderTargets[0].RenderTarget : null;
    var width = previousRenderTarget?.Width ?? Core.Screen.DisplayWidth;
    var height = previousRenderTarget?.Height ?? Core.Screen.DisplayHeight;
    // Render Result
    Core.Sb.Begin(
      SpriteSortMode.Immediate,
      BlendState.AlphaBlend,
      samplerState: SamplerState.LinearClamp
    );
    Core.Sb.Draw(texture, new Rectangle(0, 0, width, height), Color.White * MaxDensity);
    Core.Sb.End();
  }

  private static Vector2 GetTextureScale(Texture2D texture)
  {
    return new Vector2((float)Core.Screen.DisplayWidth / texture.Width, (float)Core.Screen.DisplayHeight / texture.Height);
  }

  private static void ApplyEffect(Effect effect, RenderTarget2D source, RenderTarget2D target, Action<EffectParameterCollection> setup)
  {
    var previousRenderTargets = Core.GraphicsDevice.GetRenderTargets();
    RenderTarget2D? previousRenderTarget = previousRenderTargets.Length > 0 ? (RenderTarget2D)previousRenderTargets[0].RenderTarget : null;

    Core.GraphicsDevice.SetRenderTarget(target);
    Core.GraphicsDevice.Clear(Color.Transparent);

    setup(effect.Parameters);

    Core.Sb.Begin(
      SpriteSortMode.Immediate,
      BlendState.AlphaBlend,
      samplerState: SamplerState.LinearClamp,
      effect: effect
    );
    Core.Sb.Draw(source, Vector2.Zero, Color.White);
    Core.Sb.End();

    Core.GraphicsDevice.SetRenderTarget(previousRenderTarget);
  }

  private static void Swap(ref RenderTarget2D a, ref RenderTarget2D b)
  {
    (b, a) = (a, b);
  }

  public void Splat(Vector2 position, Vector2 velocity, Color color, bool followCamera)
  {
    var pos = followCamera ? position - Core.Camera.Position : position;
    SplatOnScreenPosition(
      new Vector2(pos.X / Core.Screen.Width, pos.Y / Core.Screen.Height),
      velocity,
      color
    );
  }

  public void SplatByTexture(Texture2D texture, Vector2 position, Vector2 velocity, Color color, bool followCamera)
  {
    var pos = followCamera ? position - Core.Camera.Position : position;
    var scale = GetTextureScale(texture);
    ApplyEffect(splatEffect, velocityField, tempField, parameters =>
    {
      parameters["position"].SetValue(new Vector2(pos.X / Core.Screen.Width, pos.Y / Core.Screen.Height));
      parameters["color"].SetValue(new Vector4(velocity.X, velocity.Y, 0.0f, 1.0f));
      parameters["aspectRatio"].SetValue((float)Core.Screen.Width / Core.Screen.Height);
      parameters["radius"].SetValue(CorrectRadius(SplatRadius / 100));
      parameters["splatByTexture"].SetValue(1.0f);
      parameters["TextureSampler"].SetValue(texture);
      if (BoundaryTexture != null)
      {
        parameters["BoundarySampler"].SetValue(BoundaryTexture);
        parameters["enableBoundary"].SetValue(1.0f);
      }
      else
      {
        parameters["enableBoundary"].SetValue(0.0f);
      }
    });

    Swap(ref velocityField, ref tempField);

    ApplyEffect(splatEffect, dyeField, tempField, parameters =>
    {
      parameters["color"].SetValue(color.ToVector4());
    });

    Swap(ref dyeField, ref tempField);
  }

  public void SplatOnScreenPosition(Vector2 pos, Vector2 velocity, Color color)
  {
    ApplyEffect(splatEffect, velocityField, tempField, parameters =>
    {
      parameters["position"].SetValue(pos);
      parameters["color"].SetValue(new Vector4(velocity.X, velocity.Y, 0.0f, 1.0f));
      parameters["aspectRatio"].SetValue((float)Core.Screen.Width / Core.Screen.Height);
      parameters["radius"].SetValue(CorrectRadius(SplatRadius / 100));
      parameters["splatByTexture"].SetValue(0.0f);
      if (BoundaryTexture != null)
      {
        parameters["BoundarySampler"].SetValue(BoundaryTexture);
        parameters["enableBoundary"].SetValue(1.0f);
      }
      else
      {
        parameters["enableBoundary"].SetValue(0.0f);
      }
    });

    Swap(ref velocityField, ref tempField);

    ApplyEffect(splatEffect, dyeField, tempField, parameters =>
    {
      parameters["color"].SetValue(color.ToVector4());
      parameters["splatByTexture"].SetValue(0.0f);
    });

    Swap(ref dyeField, ref tempField);
  }

  private static float CorrectRadius(float radius)
  {
    var aspectRatio = (float)Core.Screen.Width / Core.Screen.Height;
    if (aspectRatio > 1)
    {
      radius *= aspectRatio;
    }
    return radius;
  }

  public void Dispose()
  {
    if (!velocityField.IsDisposed)
    {
      velocityField.Dispose();
    }
    if (!tempField.IsDisposed)
    {
      tempField.Dispose();
    }
    if (!pressureField.IsDisposed)
    {
      pressureField.Dispose();
    }
    if (!dyeField.IsDisposed)
    {
      dyeField.Dispose();
    }
    GC.SuppressFinalize(this);
  }
}