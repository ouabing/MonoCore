using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
namespace G;

public class Light2D(
  Color lightColor,
  float radius,
  float maxIntensity
) : Component
{
  public Color LightColor { get; set; } = lightColor;
  public float MaxIntensity { get; set; } = maxIntensity;
  public float Radius { get; set; } = radius;
  public float NoiseFrequency { get; set; } = 0.3f;
  public bool IsAmbient { get; set; }
  public Vector2 Anchor { get; set; } = Vector2.Zero;
  public float PixelationSize { get; set; } = 0.006f;
  public bool IsCameraFixed { get; set; }
  private float noiseTimer;
  public bool Debug { get; set; }
  public Component? Following { get; protected set; }
  private Vector2 NormalizedPosition
  {
    get
    {
      var anchorPos = Position + Anchor;
      if (IsCameraFixed)
      {
        return new Vector2(anchorPos.X / Core.Screen.Width, anchorPos.Y / Core.Screen.Height);
      }
      return Core.Camera.WorldToScreen(anchorPos) / new Vector2(Core.Screen.Width, Core.Screen.Height);
    }
  }

  public void Follow(Component component, Vector2 anchor)
  {
    Following = component;
    Position = component.Center;
    Anchor = anchor;
  }

  public void Follow(Component component)
  {
    Following = component;
    Position = component.Center;
    Anchor = Vector2.Zero;
  }

  public override void LoadContent()
  {
    CurrentFX = Core.Effect.LoadEffect("MonoCore/Shader/Light/Light2D").Clone();
    UpdateParams(null);
    base.LoadContent();
  }

  public override void PostUpdate(GameTime gameTime)
  {
    if (Following != null)
    {
      Position = Following.Center;
    }
    UpdateParams(gameTime);
  }

  private void UpdateParams(GameTime? gameTime)
  {
    CurrentFX!.Parameters["lightColor"].SetValue(LightColor.ToVector4());
    CurrentFX.Parameters["lightPosition"].SetValue(NormalizedPosition);
    CurrentFX.Parameters["lightRadius"].SetValue(Radius / Core.Screen.Width);
    CurrentFX.Parameters["maxIntensity"].SetValue(MaxIntensity);
    CurrentFX.Parameters["pixelationSize"].SetValue(PixelationSize);
    if (Debug)
    {
      CurrentFX.Parameters["debug"].SetValue(1.0f);
    }
    else
    {
      CurrentFX.Parameters["debug"].SetValue(0.0f);
    }
    CurrentFX.Parameters["aspectRatio"].SetValue(Core.Screen.AspectRatio);
    if (gameTime != null)
    {
      noiseTimer += gameTime.GetElapsedSeconds();
      if (noiseTimer > NoiseFrequency)
      {
        noiseTimer = 0;
        CurrentFX.Parameters["time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
      }
    }
  }

  public override void Draw(GameTime gameTime)
  {
    throw new System.NotImplementedException();
  }

  public void DrawScreenRenderTarget(GameTime gameTime, RenderTarget2D renderTarget)
  {
    if (IsAmbient)
    {
      Core.Sb.Begin(
        SpriteSortMode.Immediate,
        blendState: BlendState.AlphaBlend,
        samplerState: SamplerState.PointClamp,
        rasterizerState: RasterizerState.CullNone,
        effect: null
      );
      Core.Sb.FillRectangle(new Rectangle(0, 0, Core.Screen.Width, Core.Screen.Height), LightColor * MaxIntensity);
      Core.Sb.End();
    }
    else
    {
      Core.Sb.Begin(
        SpriteSortMode.Immediate,
        blendState: BlendState.Additive,
        samplerState: SamplerState.PointClamp,
        rasterizerState: RasterizerState.CullNone,
        effect: CurrentFX
      );
      Core.Sb.Draw(renderTarget, new Rectangle(0, 0, Core.Screen.Width, Core.Screen.Height), Color.White);
      Core.Sb.End();
    }
  }

  public override void Update(GameTime gameTime)
  {
    throw new NotImplementedException();
  }
}