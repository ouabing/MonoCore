namespace G;

public interface IShakable
{
  public Shaker? Shaker { get; set; }
  public Shaker CreateShaker()
  {
    Shaker ??= new Shaker();
    return Shaker;
  }
}