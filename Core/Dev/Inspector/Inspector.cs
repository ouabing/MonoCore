using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
namespace G;

public class Inspector : Component
{
  public const int FontSize = 10;
  private int Width { get; set; } = 240;
  private int Height { get; set; }
  public Component? Watching { get; private set; }
  public List<InspectorRow> Rows { get; } = [];
  public override void LoadContent()
  {
    Height = Core.Screen.DisplayHeight;
    Opacity = 0.5f;
    base.LoadContent();
  }

  public void Watch(Component component)
  {
    Watching = component;
    RebuildRows();
  }

  public void Unwatch()
  {
    Watching = null;
    Rows.Clear();
  }

  public void RebuildRows()
  {
    Rows.Clear();
    if (Watching == null)
    {
      return;
    }
    Rows.Add(new InspectorRow("Type", (c) => c.GetType().ToString()));
    Rows.Add(new InspectorRow("Position", (c) => c.Position.ToString()));
    Rows.Add(new InspectorRow("Size", (c) => c.Size.ToString()));
    Rows.Add(new InspectorRow("Rotation", (c) => c.Rotation.ToString()));
    Rows.Add(new InspectorRow("Scale", (c) => c.Scale.ToString()));
    Rows.Add(new InspectorRow("Origin", (c) => c.Origin.ToString()));
    Rows.Add(new InspectorRow("OriginType", (c) => c.OriginType.ToString()));
    Rows.Add(new InspectorRow("Z", (c) => c.Z.ToString()));
    Rows.Add(new InspectorRow("IsDead", (c) => c.IsDead.ToString()));
    Rows.Add(new InspectorRow("LinearVelocity", (c) => c.LinearVelocity.ToString()));
    Rows.Add(new InspectorRow("AngularVelocity", (c) => c.AngularVelocity.ToString()));
    Rows.Add(new InspectorRow("Physics Categories", (c) => c.Categories.ToString()));
    Rows.Add(new InspectorRow("Physics CollidesWith", (c) => c.CollidesWith.ToString()));
  }

  public void AddRow(InspectorRow row)
  {
    Rows.Add(row);
  }

  public void RemoveRow(string name)
  {
    Rows.RemoveAll(r => r.Name == name);
  }

  public override void Update(GameTime gameTime)
  {
    if (Watching == null)
    {
      return;
    }
  }

  public override void Draw(GameTime gameTime)
  {
    if (Watching == null)
    {
      return;
    }
    var font = Core.Font.Get(FontSize);
    var position = new Vector2(Core.Screen.DisplayWidth - Width, 0);
    Core.Sb.Begin(samplerState: SamplerState.PointClamp);
    Core.Sb.FillRectangle(new Rectangle((int)position.X, (int)position.Y, Width, Height), Palette.Black * Opacity);
    font.DrawText(Core.Sb, "INSPECTOR", position + new Vector2(FontSize, FontSize), Palette.Green[4]);
    for (var i = 0; i < Rows.Count; i++)
    {
      Rows[i].Draw(Watching, font, position + new Vector2(FontSize, (i + 3) * FontSize), Palette.Green[4]);
    }
    Core.Sb.End();
    base.Draw(gameTime);
  }
}