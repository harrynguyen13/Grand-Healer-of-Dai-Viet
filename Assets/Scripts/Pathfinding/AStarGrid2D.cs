using System.Collections.Generic;
using UnityEngine;

public class AStarGrid2D : MonoBehaviour
{
    [Header("Khu vực map dùng để tìm đường")]
    [SerializeField] private Vector2 worldMin = new Vector2(-40, -40);
    [SerializeField] private Vector2 worldMax = new Vector2(40, 40);

    [Header("Cấu hình lưới")]
    [SerializeField] private float cellSize = 0.5f;
    [SerializeField] private float obstacleCheckRadius = 0.35f;

    [Header("Layer vật cản")]
    [SerializeField] private LayerMask obstacleLayer;

    public AStarNode[,] Grid { get; private set; }

    private int gridSizeX;
    private int gridSizeY;

    private void Awake()
    {
        BuildGrid();
    }

    public void BuildGrid()
    {
        gridSizeX = Mathf.CeilToInt(
            (worldMax.x - worldMin.x) / cellSize
        );

        gridSizeY = Mathf.CeilToInt(
            (worldMax.y - worldMin.y) / cellSize
        );

        Grid = new AStarNode[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = new Vector2(
                    worldMin.x + x * cellSize + cellSize * 0.5f,
                    worldMin.y + y * cellSize + cellSize * 0.5f
                );

                bool blocked = Physics2D.OverlapCircle(
                    worldPoint,
                    obstacleCheckRadius,
                    obstacleLayer
                ) != null;

                Grid[x, y] = new AStarNode(
                    !blocked,
                    worldPoint,
                    x,
                    y
                );
            }
        }

        Debug.Log(
            $"A* Grid đã tạo: {gridSizeX} x {gridSizeY}"
        );
    }

    public AStarNode NodeFromWorldPoint(Vector2 worldPosition)
    {
        if (Grid == null)
            return null;

        int x = Mathf.FloorToInt(
            (worldPosition.x - worldMin.x) / cellSize
        );

        int y = Mathf.FloorToInt(
            (worldPosition.y - worldMin.y) / cellSize
        );

        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);

        return Grid[x, y];
    }

    public AStarNode GetNearestWalkableNode(
        Vector2 worldPosition,
        int searchRadius = 10
    )
    {
        AStarNode centerNode = NodeFromWorldPoint(worldPosition);

        if (centerNode == null)
            return null;

        if (centerNode.walkable)
            return centerNode;

        AStarNode nearestNode = null;
        int nearestDistance = int.MaxValue;

        for (int radius = 1; radius <= searchRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    int checkX = centerNode.gridX + x;
                    int checkY = centerNode.gridY + y;

                    if (!IsInsideGrid(checkX, checkY))
                        continue;

                    AStarNode node = Grid[checkX, checkY];

                    if (!node.walkable)
                        continue;

                    int distance =
                        Mathf.Abs(x) + Mathf.Abs(y);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestNode = node;
                    }
                }
            }

            if (nearestNode != null)
                return nearestNode;
        }

        return null;
    }

    public List<AStarNode> GetNeighbours(AStarNode node)
    {
        List<AStarNode> neighbours =
            new List<AStarNode>(8);

        int x = node.gridX;
        int y = node.gridY;

        // 4 hướng ngang và dọc
        AddNeighbour(neighbours, x + 1, y);
        AddNeighbour(neighbours, x - 1, y);
        AddNeighbour(neighbours, x, y + 1);
        AddNeighbour(neighbours, x, y - 1);

        // 4 hướng chéo
        AddDiagonalNeighbour(
            neighbours,
            x,
            y,
            1,
            1
        );

        AddDiagonalNeighbour(
            neighbours,
            x,
            y,
            -1,
            1
        );

        AddDiagonalNeighbour(
            neighbours,
            x,
            y,
            1,
            -1
        );

        AddDiagonalNeighbour(
            neighbours,
            x,
            y,
            -1,
            -1
        );

        return neighbours;
    }

    private void AddNeighbour(
        List<AStarNode> neighbours,
        int x,
        int y
    )
    {
        if (!IsInsideGrid(x, y))
            return;

        neighbours.Add(Grid[x, y]);
    }

    private void AddDiagonalNeighbour(
        List<AStarNode> neighbours,
        int currentX,
        int currentY,
        int offsetX,
        int offsetY
    )
    {
        int diagonalX = currentX + offsetX;
        int diagonalY = currentY + offsetY;

        if (!IsInsideGrid(diagonalX, diagonalY))
            return;

        int horizontalX = currentX + offsetX;
        int horizontalY = currentY;

        int verticalX = currentX;
        int verticalY = currentY + offsetY;

        if (!IsInsideGrid(horizontalX, horizontalY))
            return;

        if (!IsInsideGrid(verticalX, verticalY))
            return;

        AStarNode horizontalNode =
            Grid[horizontalX, horizontalY];

        AStarNode verticalNode =
            Grid[verticalX, verticalY];

        AStarNode diagonalNode =
            Grid[diagonalX, diagonalY];

        // Không cho đi chéo xuyên qua góc tường.
        // Muốn đi chéo thì cả hai ô cạnh bên đều phải đi được.
        if (!horizontalNode.walkable)
            return;

        if (!verticalNode.walkable)
            return;

        neighbours.Add(diagonalNode);
    }

    private bool IsInsideGrid(int x, int y)
    {
        return x >= 0 &&
               x < gridSizeX &&
               y >= 0 &&
               y < gridSizeY;
    }

    public void ResetPathData()
    {
        if (Grid == null)
            return;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Grid[x, y].gCost = int.MaxValue;
                Grid[x, y].hCost = 0;
                Grid[x, y].parent = null;
            }
        }
    }

    public void BlockArea(
        Vector2 worldPosition,
        int radius
    )
    {
        AStarNode centerNode =
            NodeFromWorldPoint(worldPosition);

        if (centerNode == null)
            return;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int checkX = centerNode.gridX + x;
                int checkY = centerNode.gridY + y;

                if (!IsInsideGrid(checkX, checkY))
                    continue;

                Grid[checkX, checkY].walkable = false;
            }
        }
    }

    public void SetWalkableAroundWorldPoint(
        Vector2 worldPosition,
        bool walkable,
        int radius
    )
    {
        AStarNode centerNode =
            NodeFromWorldPoint(worldPosition);

        if (centerNode == null)
            return;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int checkX = centerNode.gridX + x;
                int checkY = centerNode.gridY + y;

                if (!IsInsideGrid(checkX, checkY))
                    continue;

                Grid[checkX, checkY].walkable = walkable;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector2 center =
            (worldMin + worldMax) * 0.5f;

        Vector2 size =
            worldMax - worldMin;

        Gizmos.DrawWireCube(center, size);
    }
}