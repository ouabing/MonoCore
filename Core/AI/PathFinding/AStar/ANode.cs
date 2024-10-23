namespace G;

public abstract class ANode()
{
  public float G { get; set; }
  public float H { get; set; }
  public float F => G + H;

  public ANode? Parent { get; set; }

  public abstract bool IsSameNode(ANode node);
  public abstract float DistanceTo(ANode node);
}
