using System;
using System.Collections.Generic;
namespace G;

public class AStar<T> where T : ANode
{
  public List<T>? FindPath(
    T startNode,
    Func<T, bool> isGoalMet,
    Func<T, float> heuristic,
    Func<T, List<T>> getNeighbors
  )
  {
    var openList = new List<T>();
    var closedList = new HashSet<T>();

    openList.Add(startNode);

    while (openList.Count > 0)
    {
      T currentNode = AStar<T>.GetLowestFScoreNode(openList);

      if (isGoalMet(currentNode))
      {
        // Bingo
        return AStar<T>.ReconstructPath(currentNode);
      }

      openList.Remove(currentNode);
      closedList.Add(currentNode);

      foreach (var neighbor in getNeighbors(currentNode))
      {
        if (closedList.Contains(neighbor))
        {
          continue;
        }

        float tentativeG = currentNode.G + currentNode.DistanceTo(neighbor);

        if (!openList.Contains(neighbor))
        {
          neighbor.H = heuristic(neighbor);
          openList.Add(neighbor);
        }
        else if (tentativeG >= neighbor.G)
        {
          continue;
        }

        neighbor.Parent = currentNode;
        neighbor.G = tentativeG;
      }
    }

    return null;
  }

  private static T GetLowestFScoreNode(List<T> openList)
  {
    T lowestFScoreNode = openList[0];
    foreach (var node in openList)
    {
      if (node.F < lowestFScoreNode.F)
      {
        lowestFScoreNode = node;
      }
    }
    return lowestFScoreNode;
  }

  private static List<T> ReconstructPath(T currentNode)
  {
    var path = new List<T>();
    while (currentNode != null)
    {
      path.Add(currentNode);
      currentNode = currentNode.Parent as T;
    }
    path.Reverse();
    return path;
  }
}