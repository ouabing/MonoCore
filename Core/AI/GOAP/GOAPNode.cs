using System;

namespace G;

public class GOAPNode : ANode
{
  /// The state of the world at this node.
  public GOAPWorldState WorldState { get; set; }

  /// the Action associated with this node
  public GOAPAction? Action { get; set; }


  #region IEquatable and IComparable

  public bool Equals(GOAPNode other)
  {
    var care = WorldState.DontCare ^ -1L;
    return (WorldState.Values & care) == (other.WorldState.Values & care);
  }


  public int CompareTo(GOAPNode other)
  {
    return F.CompareTo(other.F);
  }

  #endregion

  public GOAPNode Clone()
  {
    return (GOAPNode)MemberwiseClone();
  }

  public override string ToString()
  {
    return $"[cost: {G} | heuristic: {H}]: {Action}";
  }

  public override bool IsSameNode(ANode node)
  {
    if (node is GOAPNode goapNode)
    {
      var care = WorldState.DontCare ^ -1L;
      return (WorldState.Values & care) == (goapNode.WorldState.Values & care);
    }
    throw new ArgumentException("Node is not a GOAPNode");
  }

  public override float DistanceTo(ANode node)
  {
    if (node is GOAPNode goapNode)
    {
      return node.G;
    }
    throw new ArgumentException("Node is not a GOAPNode");
  }
}