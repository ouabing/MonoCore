namespace G;

public enum ShapeType
{
  Rectangle,
  Circle
}

public abstract class BaseShape
{
  public abstract ShapeType Type { get; }
}