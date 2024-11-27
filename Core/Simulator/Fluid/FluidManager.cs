using System.Collections.Generic;

namespace G;

public class FluidManager
{
  public List<FluidSim> Fluids { get; private set; } = [];

  public void AddFluid(FluidSim fluid)
  {
    Fluids.Add(fluid);
  }
}