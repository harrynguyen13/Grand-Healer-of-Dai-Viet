using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder2D : MonoBehaviour
{
    [SerializeField] private AStarGrid2D grid;

    public List<Vector2> FindPath(Vector2 startPosition, Vector2 targetPosition)
    {
        if (grid == null)
        {
            Debug.LogError("AStarPathfinder2D chưa có AStarGrid2D.");
            return new List<Vector2>();
        }

        AStarNode startNode = grid.GetNearestWalkableNode(startPosition);
        AStarNode targetNode = grid.GetNearestWalkableNode(targetPosition);

        if (startNode == null)
        {
            Debug.LogWarning("Không tìm được điểm bắt đầu hợp lệ cho A*.");
            return new List<Vector2>();
        }

        if (targetNode == null)
        {
            Debug.LogWarning("Không tìm được điểm đích hợp lệ cho A*.");
            return new List<Vector2>();
        }

        grid.ResetPathData();

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);
        startNode.parent = null;

        List<AStarNode> openSet = new List<AStarNode>();
        HashSet<AStarNode> closedSet = new HashSet<AStarNode>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            AStarNode currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                bool lowerFCost = openSet[i].fCost < currentNode.fCost;
                bool sameFCostLowerH = openSet[i].fCost == currentNode.fCost &&
                                       openSet[i].hCost < currentNode.hCost;

                if (lowerFCost || sameFCostLowerH)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (AStarNode neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        Debug.LogWarning("Không tìm được đường A*.");
        return new List<Vector2>();
    }

    public void BlockArea(Vector2 worldPosition, int radius)
    {
        if (grid == null)
        {
            Debug.LogError("AStarPathfinder2D chưa có AStarGrid2D.");
            return;
        }

        grid.BlockArea(worldPosition, radius);
    }

    private List<Vector2> RetracePath(AStarNode startNode, AStarNode endNode)
    {
        List<Vector2> path = new List<Vector2>();
        AStarNode currentNode = endNode;

        while (currentNode != startNode)
        {
            if (currentNode == null)
                break;

            path.Add(currentNode.worldPosition);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private int GetDistance(AStarNode nodeA, AStarNode nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distanceY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return distanceX + distanceY;
    }
}