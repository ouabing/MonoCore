using G;
using nkast.Aether.Physics2D.Dynamics;

public static class CategoryExtensions
{
  // 将 GameCategory 转换为 Aether.Physics2D.Category
  public static Category ToPhysicsCategory(this Def.Physics.Cat gameCategory)
  {
    return (Category)gameCategory; // 直接类型转换，因为它们都是位标志类型
  }

  // 将 Aether.Physics2D.Category 转换为 GameCategory
  public static Def.Physics.Cat ToGameCategory(this Category PhysicsCategory)
  {
    return (Def.Physics.Cat)PhysicsCategory; // 直接类型转换
  }

  public static bool ContainsAny(this Category category, Def.Physics.Cat another)
  {
    return ((int)category & (int)another) != 0;
  }

  public static bool ContainsAll(this Category category, Def.Physics.Cat another)
  {
    return ((int)category & (int)another) == (int)another;
  }
}