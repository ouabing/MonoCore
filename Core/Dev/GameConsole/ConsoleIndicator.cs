using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace G;

public class ConsoleIndicator : Component
{
  private AnimationLogic animation;

  public ConsoleIndicator()
  {
    animation = new AnimationLogic(0.1f, 8, AnimationLoopMode.Loop, [
      () => {
        Rotation = 0f;
        Scale = new(1, 1);
      },
      () => {
        Rotation = MathHelper.ToRadians(45);
        Scale = new(1f, 0.8f);
      },
      () => {
        Rotation = MathHelper.ToRadians(90);
        Scale = new(1f, 0.5f);
      },
      () => {
        Rotation = MathHelper.ToRadians(135);
        Scale = new(1f, 0.8f);
      },
      () => {
        Rotation = MathHelper.ToRadians(180);
        Scale = new(1f, 1f);
      },
      () => {
        Rotation = MathHelper.ToRadians(225);
        Scale = new(1f, 0.8f);
      },
      () => {
        Rotation = MathHelper.ToRadians(270);
        Scale = new(1f, 0.5f);
      },
      () => {
        Rotation = MathHelper.ToRadians(315);
        Scale = new(1f, 0.8f);
      },
    ]);
  }

  public void Reset()
  {
    animation.Reset();
  }

  public override void LoadContent()
  {
    Size = new Vector2(1, 1);
    base.LoadContent();
  }

  public override void Update(GameTime gameTime)
  {
    animation.Update(gameTime);
  }

  public void Draw(GameTime gameTime, int size, int x, int y)
  {
    Core.Sb.Draw(Core.Texture.PixelTexture, new(x + size / 4f, y + size / 2f), null, Palette.ConsoleTheme.White, Rotation, Origin, new Vector2(Scale.X * 2, Scale.Y * size), SpriteEffects.None, 0);
    base.Draw(gameTime);
  }
}