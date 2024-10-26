using Microsoft.Xna.Framework;

namespace G;

public static class PhysicsExtensions
{
  public static Vector2 ToMeters(this Vector2 pixelVector)
  {
    return pixelVector * 1 / Def.Physics.PixelsPerMeter;
  }

  public static Vector2 ToPixels(this Vector2 meterVector)
  {
    return meterVector * Def.Physics.PixelsPerMeter;
  }

  public static float ToMeters(this float pixels)
  {
    return pixels * 1 / Def.Physics.PixelsPerMeter;
  }

  public static float ToPixels(this float meters)
  {
    return meters * Def.Physics.PixelsPerMeter;
  }

  public static Microsoft.Xna.Framework.Vector2 ToPixelVector2(this nkast.Aether.Physics2D.Common.Vector2 vec)
  {
    return new Microsoft.Xna.Framework.Vector2(vec.X.ToPixels(), vec.Y.ToPixels());
  }

  public static nkast.Aether.Physics2D.Common.Vector2 ToMeterVector2(this Microsoft.Xna.Framework.Vector2 vec)
  {
    return new nkast.Aether.Physics2D.Common.Vector2(vec.X.ToMeters(), vec.Y.ToMeters());
  }

  public static nkast.Aether.Physics2D.Common.Vector2 ToAetherVector2(this Microsoft.Xna.Framework.Vector2 vec)
  {
    return new nkast.Aether.Physics2D.Common.Vector2(vec.X, vec.Y);
  }

  public static Microsoft.Xna.Framework.Vector2 ToVector2(this nkast.Aether.Physics2D.Common.Vector2 vec)
  {
    return new Microsoft.Xna.Framework.Vector2(vec.X, vec.Y);
  }
}