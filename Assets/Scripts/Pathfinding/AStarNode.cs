using UnityEngine;

public class AStarNode
{
    public bool walkable;
    public Vector2 worldPosition;

    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;

    public AStarNode parent;

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public AStarNode(bool walkable, Vector2 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}