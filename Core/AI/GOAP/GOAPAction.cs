namespace G;
using System;
using System.Collections.Generic;

public class GOAPAction
{
  /// optional name for the Action. Used for debugging purposes
  public string Name { get; set; }

  /// The cost of performing the action.  Figure out a weight that suits the action.  Changing it will affect what actions are
  /// chosen during planning
  public int Cost { get; set; } = 1;


  internal HashSet<Tuple<string, bool>> PreConditions = new HashSet<Tuple<string, bool>>();

  internal HashSet<Tuple<string, bool>> PostConditions = new HashSet<Tuple<string, bool>>();



  public GOAPAction(string name)
  {
    Name = name;
  }


  public GOAPAction(string name, int cost) : this(name)
  {
    Cost = cost;
  }


  public void SetPrecondition(string conditionName, bool value)
  {
    PreConditions.Add(new Tuple<string, bool>(conditionName, value));
  }


  public void SetPostcondition(string conditionName, bool value)
  {
    PostConditions.Add(new Tuple<string, bool>(conditionName, value));
  }


  /// called before the Planner does its planning. Gives the Action an opportunity to set its score or to opt out if it isnt of use.
  /// For example, if the Action is to pick up a gun but there are no guns in the world returning false would keep the Action from being
  /// considered by the ActionPlanner.
  public virtual bool Validate()
  {
    return true;
  }


  public override string ToString()
  {
    return $"[Action] {this.Name} - cost: {this.Cost}";
  }

}
