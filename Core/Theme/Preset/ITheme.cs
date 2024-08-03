using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace G;

public interface ITheme
{
  public abstract Color Black { get; }
  public abstract Color White { get; }
  public abstract List<Color> Grey { get; }
  public abstract List<Color> Red { get; }
  public abstract List<Color> Yellow { get; }
  public abstract List<Color> Blue { get; }
  public abstract List<Color> Purple { get; }
  public abstract List<Color> Green { get; }
}