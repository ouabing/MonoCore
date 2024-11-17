using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public class CGATheme : ITheme
{
  public string Name { get; } = "CGA";
  // 基本颜色
  public Color White { get; } = Palette.FromRgba("#FFFFFF"); // 白色
  public Color Black { get; } = Palette.FromRgba("#000000"); // 黑色

  // 灰色
  public List<Color> Grey { get; } = Palette.FromRgbas([
    "#000000", // 黑色
    "#2E2E2E", // 深灰
    "#5C5C5C", // 中灰
    "#8C8C8C", // 浅灰
    "#C0C0C0", // 银灰
    "#E0E0E0", // 很浅的灰
    "#FFFFFF"  // 白色
  ]);

  // 红色
  public List<Color> Red { get; } = Palette.FromRgbas([
    "#800000", // 深红
    "#B22222", // 火砖红
    "#FF4500", // 橙红
    "#FF6347", // 番茄红
    "#FF7F7F", // 浅红
    "#FFA07A"  // 淡珊瑚色
  ]);

  // 黄色
  public List<Color> Yellow { get; } = Palette.FromRgbas([
    "#FFD700", // 金色
    "#FFFF00", // 纯黄
    "#FFE135", // 香蕉黄
    "#FFD700", // 深金黄
    "#FFC125", // 柠檬黄
    "#FFF8DC"  // 玉米色
  ]);

  // 绿色
  public List<Color> Green { get; } = Palette.FromRgbas(
  [
    "#006400", // 深绿
    "#228B22", // 森林绿
    "#32CD32", // 酸橙绿
    "#7CFC00", // 草坪绿
    "#ADFF2F", // 黄绿
    "#98FB98"  // 苍绿
  ]);

  // 蓝色
  public List<Color> Blue { get; } = Palette.FromRgbas(
  [
    "#000080", // 海军蓝
    "#0000FF", // 纯蓝
    "#4169E1", // 皇家蓝
    "#4682B4", // 钢蓝
    "#87CEEB", // 天蓝
    "#ADD8E6"  // 淡蓝
  ]);

  // 紫色
  public List<Color> Purple { get; } = Palette.FromRgbas(
  [
    "#4B0082", // 靛蓝
    "#800080", // 紫罗兰
    "#8A2BE2", // 蓝紫
    "#9370DB", // 浅紫
    "#D8BFD8", // 蓟色
    "#EE82EE"  // 紫红
  ]);
}