using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace G;

public class FluidManager
{
  public List<FluidSim> Fluids { get; private set; } = [];

  public void AddFluid(FluidSim fluid)
  {
    Fluids.Add(fluid);
  }

  public void DrawFluidBetweenZ(GameTime gameTime, RenderTarget2D target, float min, float max)
  {

  }
}