namespace G;
public static class VectorExtensions
{
  public static nkast.Aether.Physics2D.Common.Vector2 ToAetherVector2(this Microsoft.Xna.Framework.Vector2 vec)
  {
    return new nkast.Aether.Physics2D.Common.Vector2(vec.X, vec.Y);
  }

  public static Microsoft.Xna.Framework.Vector2 ToVector2(this nkast.Aether.Physics2D.Common.Vector2 vec)
  {
    return new Microsoft.Xna.Framework.Vector2(vec.X, vec.Y);
  }
}
