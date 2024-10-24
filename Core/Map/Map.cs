using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;


public class Map : Component
{
  public int Width { get; private set; }
  public int Height { get; private set; }
  public int TileWidth { get; private set; }
  public int TileHeight { get; private set; }
  public int LayerCount { get; private set; }
  public List<Dictionary<int, Component>> Components { get; private set; }

  public int WidthInPixels => Width * TileWidth;
  public int HeightInPixels => Height * TileHeight;

  public Map(int width, int height, int tileWidth, int tileHeight, int layerCount)
  {
    Width = width;
    Height = height;
    TileWidth = tileWidth;
    TileHeight = tileHeight;
    LayerCount = layerCount;
    Components = [];
    for (var i = 0; i < layerCount; i++)
    {
      Components.Add([]);
    }
  }

  public void AddComponent(int mapLayer, int x, int y, Component component, Def.Container container, Def.Layer layer)
  {
    if (x < 0 || x >= Width || y < 0 || y >= Height || mapLayer < 0 || mapLayer >= LayerCount)
    {
      throw new System.ArgumentException($"Invalid position Layer: {mapLayer}, Position: ({x}, {y})");
    }

    var layerComponents = Components[mapLayer];

    var index = y * Width + x;
    if (layerComponents.TryGetValue(index, out Component? value))
    {
      value?.Die();
    }
    component.Position = new Vector2(x * TileWidth, y * TileHeight);
    layerComponents.Add(index, component);

    Core.Container.Add(container, component);
    Core.Layer.Add(layer, component);
  }

  public Component GetComponent(int mapLayer, int x, int y)
  {
    if (x < 0 || x >= Width || y < 0 || y >= Height || mapLayer < 0 || mapLayer >= LayerCount)
    {
      throw new System.ArgumentException($"Invalid position Layer: {mapLayer}, Position: ({x}, {y})");
    }

    var layerComponents = Components[mapLayer];
    var index = y * Width + x;
    return layerComponents[index];
  }

  public static Map LoadFromMapData(MapData mapData)
  {
    var map = new Map(mapData.Width, mapData.Height, mapData.TileWidth, mapData.TileHeight, mapData.LayerData.Count);
    for (int layerIndex = 0; layerIndex < mapData.LayerData.Count; layerIndex++)
    {
      var layer = mapData.LayerData[layerIndex];
      foreach (var item in layer)
      {
        var tileId = item.Value;
        if (tileId == 0)
        {
          continue;
        }
        int x = item.Key % mapData.Width;
        int y = item.Key / mapData.Width;
        var info = mapData.ComponentFactory[tileId](tileId);
        map.AddComponent(layerIndex, x, y, info.Item1, info.Item2, info.Item3);
      }
    }
    return map;
  }

  public override void LoadContent()
  {
    base.LoadContent();
  }

  public override void Update(GameTime gameTime)
  {
  }
}