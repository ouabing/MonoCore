using nkast.Aether.Physics2D.Dynamics;

public static class CategoryExtensions
{

  public static bool ContainsAny(this Category category, Category another)
  {
    return (category & another) != Category.None;
  }

  public static bool ContainsAll(this Category category, Category another)
  {
    return (category & another) == another;
  }
}