using Microsoft.Xna.Framework;
namespace G;

public class Debugger : Component
{
  public Vector2 FpsPosition { get; set; } = new(10, 10);
  public void Enable()
  {
    Core.Container.Add(Def.Container.Debugger, this);
    Core.Layer.Add(Def.Layer.Debugger, this);
  }

  public void Disable()
  {
    Core.Container.Remove(Def.Container.Debugger, this);
    Core.Layer.Remove(Def.Layer.Debugger, this);
  }

  public override void LoadContent()
  {
    base.LoadContent();
  }

  public override void Update(GameTime gameTime)
  {
  }

  public override void Draw(GameTime gameTime)
  {
    var fps = 1 / gameTime.ElapsedGameTime.TotalSeconds;
    var font = Core.Font.Get(10);
    font.DrawText(Core.Sb, $"FPS: {fps:0}", FpsPosition, Palette.White);
    base.Draw(gameTime);
  }
}