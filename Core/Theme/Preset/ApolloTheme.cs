using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public class ApolloTheme : ITheme
{

  public Color White { get; } = Palette.FromRgba("#ebede9");
  public Color Black { get; } = Palette.FromRgba("#090a14");

  public List<Color> Grey { get; } = Palette.FromRgbas([
    "#090a14",
    "#10141f",
    "#151d28",
    "#202e37",
    "#394a50",
    "#577277",
    "#819796",
    "#a8b5b2",
    "#c7cfcc"
  ]);

  public List<Color> Red { get; } = Palette.FromRgbas([
    "#241527",
    "#411d31",
    "#752438",
    "#a53030",
    "#cf573c",
    "#da863e",
  ]);

  public List<Color> Yellow { get; } = Palette.FromRgbas([
    "#4d2b32",
    "#7a4841",
    "#ad7757",
    "#c09473",
    "#d7b594",
    "#e7d5b3",
  ]);

  public List<Color> Green { get; } = Palette.FromRgbas([
    "#19332d",
    "#25562e",
    "#468232",
    "#75a743",
    "#a8ca58",
    "#d0da91",
  ]);


  public List<Color> Blue { get; } = Palette.FromRgbas([
    "#172038",
    "#253a5e",
    "#3c5e8b",
    "#4f8fba",
    "#73bed3",
    "#a4dddb"

  ]);

  public List<Color> Purple { get; } = Palette.FromRgbas([
    "#1e1d39",
    "#402751",
    "#7a367b",
    "#a23e8c",
    "#c65197",
    "#df84a5"
  ]);

}