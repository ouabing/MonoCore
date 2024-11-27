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

  public float SplatRadius { get; set; } = 0.25f;
  public float TexelSize { get; set; } = 2.0f;
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
  public bool Debug { get; set; }
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
      SurfaceFormat.Vector4,
      DepthFormat.None
    );
    pressureField = new RenderTarget2D(
      Core.GraphicsDevice,
      width,
      height,
      false,
      SurfaceFormat.Vector4,
      DepthFormat.None
    );
    tempField = new RenderTarget2D(
      Core.GraphicsDevice,
      width,
      height,
      false,
      SurfaceFormat.Vector4,
      DepthFormat.None
    );
    dyeField = new RenderTarget2D(
      Core.GraphicsDevice,
      width,
      height,
      false,
      SurfaceFormat.Vector4,
      DepthFormat.None
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
          Splat(
            new Vector2((float)mouseState.X / Core.Screen.DisplayWidth, (float)mouseState.Y / Core.Screen.DisplayHeight),
            GenerateSplatVelocity(),
            new Color(rand.NextSingle() * 0.15f, 0.15f, 0.15f, 1.0f)
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
    return new Vector2(rand.NextSingle(-1, 1), rand.NextSingle(-1, 1)) * 100f;
  }

  public override void Draw(GameTime gameTime)
  {
    float dt = gameTime.GetElapsedSeconds();
    var texelVec = new Vector2(TexelSize / velocityField.Width, TexelSize / velocityField.Height);

    // Vorticity
    ApplyEffect(vorticityEffect, velocityField, tempField, parameters =>
    {
      parameters["dt"].SetValue(dt);
      parameters["texelSize"].SetValue(texelVec);
      parameters["curlAmount"].SetValue(CurlAmount);
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
    });
    Swap(ref dyeField, ref tempField);

    // Render Result
    Core.Sb.Begin(
      SpriteSortMode.Immediate,
      BlendState.AlphaBlend,
      samplerState: SamplerState.PointClamp
    );
    Core.Sb.Draw(dyeField, new Rectangle(0, 0, Core.Screen.DisplayWidth, Core.Screen.DisplayHeight), Color.White);
    Core.Sb.End();

    base.Draw(gameTime);
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

  public void Splat(Vector2 pos, Vector2 velocity, Color color)
  {
    ApplyEffect(splatEffect, velocityField, tempField, parameters =>
    {
      parameters["position"].SetValue(pos);
      parameters["color"].SetValue(new Vector4(velocity.X, velocity.Y, 0.0f, 1.0f));
      parameters["aspectRatio"].SetValue((float)Core.Screen.Width / Core.Screen.Height);
      parameters["radius"].SetValue(CorrectRadius(SplatRadius / 100.0f));
    });

    Swap(ref velocityField, ref tempField);

    ApplyEffect(splatEffect, dyeField, tempField, parameters =>
    {
      parameters["color"].SetValue(color.ToVector4());
      parameters["aspectRatio"].SetValue((float)Core.Screen.Width / Core.Screen.Height);
      parameters["radius"].SetValue(CorrectRadius(SplatRadius / 100.0f));
    });

    Swap(ref dyeField, ref tempField);
  }

  private static float CorrectRadius(float radius)
  {
    var aspectRatio = (float)Core.Screen.Width / Core.Screen.Height;
    if (aspectRatio > 1)
      radius *= aspectRatio;
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