using System;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace G;

public class InspectorRow(string name, Func<Component, string> rule)
{
  public string Name { get; set; } = name;
  public Func<Component, string> Rule { get; set; } = rule;

  public void Draw(Component component, SpriteFontBase font, Vector2 position, Color color)
  {
    font.DrawText(Core.Sb, $"{Name}: {Rule(component)}", position, color);
  }
}