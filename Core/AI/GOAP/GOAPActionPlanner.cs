namespace G;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GOAPActionPlanner
{
  public const int MaxConditions = 64;

  public string[] ConditionNames { get; private set; } = new string[MaxConditions];

  private readonly List<GOAPAction> actions = [];

  private readonly GOAPWorldState[] preConditions = new GOAPWorldState[MaxConditions];

  private readonly GOAPWorldState[] postConditions = new GOAPWorldState[MaxConditions];

  private int numConditionNames;


  public GOAPActionPlanner()
  {
    numConditionNames = 0;
    for (var i = 0; i < MaxConditions; ++i)
    {
      ConditionNames[i] = "";
      preConditions[i] = GOAPWorldState.Create(this);
      postConditions[i] = GOAPWorldState.Create(this);
    }
  }


  /// <summary>
  /// convenince method for fetching a WorldState object
  /// </summary>
  /// <returns>The world state.</returns>
  public GOAPWorldState CreateWorldState()
  {
    return GOAPWorldState.Create(this);
  }


  public void AddAction(GOAPAction goapAction)
  {
    var actionId = FindActionIndex(goapAction);
    if (actionId == -1)
    {
      throw new KeyNotFoundException("could not find or create Action");
    }

    foreach (var preCondition in goapAction.PreConditions)
    {
      var conditionId = FindConditionNameIndex(preCondition.Item1);
      if (conditionId == -1)
      {
        throw new KeyNotFoundException("could not find or create conditionName");
      }

      preConditions[actionId].Set(conditionId, preCondition.Item2);
    }

    foreach (var postCondition in goapAction.PostConditions)
    {
      var conditionId = FindConditionNameIndex(postCondition.Item1);
      if (conditionId == -1)
      {
        throw new KeyNotFoundException("could not find conditionName");
      }

      postConditions[actionId].Set(conditionId, postCondition.Item2);
    }
  }


  public List<GOAPAction> Plan(GOAPWorldState startState, GOAPWorldState goalState)
  {
    var astar = new AStar<GOAPNode>();
    float heuristic(GOAPNode node)
    {
      long care = goalState.DontCare ^ -1L;
      long diff = (node.WorldState.Values & care) ^ (node.WorldState.Values & care);
      int dist = 0;

      for (var i = 0; i < MaxConditions; ++i)
      {
        if ((diff & (1L << i)) != 0)
        {
          dist++;
        }
      }
      return dist;
    }

    var currentNode = new GOAPNode
    {
      WorldState = startState,
      G = 0,
      H = 0,
    };
    currentNode.H = heuristic(currentNode);

    var path = astar.FindPath(
      currentNode,
      (node) => goalState.Equals(node.WorldState),
      heuristic,
      (node) => GetPossibleTransitions(node.WorldState)
    );
    return path?.Where(n => n.Action != null).Select(n => n.Action!).ToList() ?? [];
  }


  public string Describe()
  {
    var sb = new StringBuilder();
    for (var a = 0; a < actions.Count; ++a)
    {
      sb.AppendLine(actions[a].GetType().Name);

      var pre = preConditions[a];
      var pst = postConditions[a];
      for (var i = 0; i < MaxConditions; ++i)
      {
        if ((pre.DontCare & (1L << i)) == 0)
        {
          bool v = (pre.Values & (1L << i)) != 0;
          sb.AppendFormat("  {0}=={1}\n", ConditionNames[i], v ? 1 : 0);
        }
      }

      for (var i = 0; i < MaxConditions; ++i)
      {
        if ((pst.DontCare & (1L << i)) == 0)
        {
          bool v = (pst.Values & (1L << i)) != 0;
          sb.AppendFormat("  {0}:={1}\n", ConditionNames[i], v ? 1 : 0);
        }
      }
    }

    return sb.ToString();
  }


  internal int FindConditionNameIndex(string conditionName)
  {
    int idx;
    for (idx = 0; idx < this.numConditionNames; ++idx)
    {
      if (string.Equals(ConditionNames[idx], conditionName, System.StringComparison.Ordinal))
        return idx;
    }

    if (idx < MaxConditions - 1)
    {
      ConditionNames[idx] = conditionName;
      numConditionNames++;
      return idx;
    }

    return -1;
  }


  internal int FindActionIndex(GOAPAction goapAction)
  {
    var idx = actions.IndexOf(goapAction);
    if (idx > -1)
    {
      return idx;
    }

    actions.Add(goapAction);

    return actions.Count - 1;
  }


  internal List<GOAPNode> GetPossibleTransitions(GOAPWorldState fr)
  {
    var result = new List<GOAPNode>();
    for (var i = 0; i < actions.Count; ++i)
    {
      var action = actions[i];
      if (!action.Validate())
      {
        continue;
      }
      var pre = preConditions[i];
      bool met = pre.CaredValues == fr.CaredValues;
      if (met)
      {
        var node = new GOAPNode
        {
          Action = action,
          G = action.Cost,
          WorldState = ApplyPostConditions(this, i, fr)
        };
        result.Add(node);
      }
    }
    return result;
  }


  internal static GOAPWorldState ApplyPostConditions(GOAPActionPlanner ap, int actionnr, GOAPWorldState fr)
  {
    var pst = ap.postConditions[actionnr];
    long unaffected = pst.DontCare;
    long affected = pst.Care;

    fr.Values = (fr.Values & unaffected) | (pst.Values & affected);
    fr.DontCare &= pst.DontCare;
    return fr;
  }
}