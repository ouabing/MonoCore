using System;
using System.Text;


namespace G;
#pragma warning disable CS0661
#pragma warning disable CS0659
public struct GOAPWorldState(GOAPActionPlanner planner, long values, long dontcare) : IEquatable<GOAPWorldState>
#pragma warning restore CS0659
#pragma warning restore CS0661
{
  // Bitmask for conditions that are true
  public long Values { get; set; } = values;

  // Bitmask for conditions that we don't care about
  public long DontCare { get; set; } = dontcare;
  public readonly long Care => DontCare ^ -1L;
  public readonly long CaredValues => Values & Care;

  internal GOAPActionPlanner Planner = planner;


  public static GOAPWorldState Create(GOAPActionPlanner planner)
  {
    return new GOAPWorldState(planner, 0, -1);
  }

  public bool Set(string conditionName, bool value)
  {
    return Set(Planner.FindConditionNameIndex(conditionName), value);
  }


  internal bool Set(int conditionId, bool value)
  {
    Values = value ? (Values | (1L << conditionId)) : (Values & ~(1L << conditionId));
    DontCare ^= 1 << conditionId;
    return true;
  }


  public readonly bool Equals(GOAPWorldState other)
  {
    var care = DontCare ^ -1L;
    return (Values & care) == (other.Values & care);
  }


  public readonly string Describe(GOAPActionPlanner planner)
  {
    var sb = new StringBuilder();
    for (var i = 0; i < GOAPActionPlanner.MaxConditions; i++)
    {
      if ((this.DontCare & (1L << i)) == 0)
      {
        var val = planner.ConditionNames[i];
        if (val == null)
          continue;

        var set = (this.Values & (1L << i)) != 0L;

        if (sb.Length > 0)
          sb.Append(", ");
        sb.Append(set ? val.ToUpper() : val);
      }
    }
    return sb.ToString();
  }

  public override readonly bool Equals(object? obj)
  {
    return obj is GOAPWorldState state && Equals(state);
  }

  public static bool operator ==(GOAPWorldState left, GOAPWorldState right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(GOAPWorldState left, GOAPWorldState right)
  {
    return !(left == right);
  }
}
