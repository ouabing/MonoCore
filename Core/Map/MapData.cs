using System;
using System.Collections.Generic;

namespace G;

public class MapData(int width, int height, int tileWidth, int tileHeight, Dictionary<int, Func<int, (Component, Def.Container, Def.Layer)>> componentFactory, List<Dictionary<int, int>> layerData)
{

  public int Width { get; private set; } = width;
  public int Height { get; private set; } = height;
  public int TileWidth { get; private set; } = tileWidth;
  public int TileHeight { get; private set; } = tileHeight;
  public Dictionary<int, Func<int, (Component, Def.Container, Def.Layer)>> ComponentFactory { get; private set; } = componentFactory;
  public List<Dictionary<int, int>> LayerData { get; private set; } = layerData;

  public string PrintLayer(int layerIndex)
  {
    if (layerIndex >= LayerData.Count)
    {
      throw new ArgumentException($"Invalid layer index: {layerIndex}");
    }

    var result = "";

    for (int y = 0; y < Height; y++)
    {
      for (int x = 0; x < Width; x++)
      {
        var layer = LayerData[layerIndex];
        if (layer.TryGetValue(y * Width + x, out int id))
        {
          result += id + ",";
        }
        else
        {
          result += "0,";
        }
      }
      if (y < Height - 1)
      {
        result += "\n";
      }
    }
    return result;
  }

  public static Dictionary<int, int> LoadSingleLayerDataFromString(string data)
  {
    var layerData = new Dictionary<int, int>();
    data = data.Trim();
    var lines = data.Split('\n');
    var height = lines.Length;
    var width = 0;
    for (var y = 0; y < lines.Length; y++)
    {
      var line = lines[y].Trim();
      line = line.Trim(',');
      var ids = line.Split(',');
      var w = ids.Length;
      if (w > width)
      {
        width = w;
      }
    }

    for (var y = 0; y < height; y++)
    {
      var line = lines[y].Trim();
      var ids = line.Split(',');
      for (int x = 0; x < ids.Length; x++)
      {
        var idStr = ids[x].Trim();
        if (idStr == "" || idStr == "0")
        {
          continue;
        }
        var id = int.Parse(idStr);
        layerData[y * width + x] = id;
      }
    }
    return layerData;
  }
}