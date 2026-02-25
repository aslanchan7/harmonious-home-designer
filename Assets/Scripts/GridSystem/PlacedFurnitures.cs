using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlacedFurnitures : MonoBehaviour
{
    public static PlacedFurnitures Instance;

    public Vector2Int Size;
    public Furniture[,] furnitureBaseGrid;
    public Furniture[,] furnitureStackGrid;

    public readonly struct DijkstraCell
    {
        public readonly float distance;
        public readonly Vector2Int offsetOrPosition;

        public DijkstraCell(
            float distanceFromSource,
            Vector2Int lastOffsetOrPosition
        )
        {
            distance = distanceFromSource;
            offsetOrPosition = lastOffsetOrPosition;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }

        Instance = this;
    }

    private void Start()
    {
        furnitureBaseGrid = new Furniture[Size.y, Size.x];
        furnitureStackGrid = new Furniture[Size.y, Size.x];
    }

    public void SetBase(BoundingBox boundingBox, Furniture newFurniture)
    {
        GridSystem.Instance.WorldBoxToIndices(
            boundingBox,
            out Vector2Int starting,
            out Vector2Int ending
        );
        for (int i = starting.y; i < ending.y; i++)
        {
            for (int j = starting.x; j < ending.x; j++)
            {
                furnitureBaseGrid[i, j] = newFurniture;
            }
        }
    }

    public Furniture GetBase(Vector2Int vector)
    {
        return furnitureBaseGrid[vector.y, vector.x];
    }

    public void SetStack(BoundingBox boundingBox, Furniture newFurniture)
    {
        GridSystem.Instance.WorldBoxToIndices(
            boundingBox,
            out Vector2Int starting,
            out Vector2Int ending
        );
        for (int i = starting.y; i < ending.y; i++)
        {
            for (int j = starting.x; j < ending.x; j++)
            {
                furnitureStackGrid[i, j] = newFurniture;
            }
        }
    }

    public Furniture GetStack(Vector2Int vector)
    {
        return furnitureStackGrid[vector.y, vector.x];
    }

    public DijkstraCell[,] Dijkstra(BoundingBox startSquare, float maxHeight)
    {
        Vector2Int start = GridSystem.Instance.WorldToIndex(
            startSquare.position
        );
        List<DijkstraCell> unvisited = new List<DijkstraCell>();
        DijkstraCell[,] cells = new DijkstraCell[Size.y, Size.x];
        bool[,] visited = new bool[Size.y, Size.x];
        CellComparer cellComparer = new CellComparer();
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        for (int i = 0; i < Size.y; i++)
        {
            for (int j = 0; j < Size.x; j++)
            {
                cells[i, j] = new DijkstraCell(
                    float.PositiveInfinity,
                    Vector2Int.zero
                );
                if (
                    furnitureBaseGrid[i, j] == null
                    || furnitureBaseGrid[i, j].height <= maxHeight
                )
                {
                    unvisited.Add(
                        new DijkstraCell(
                            float.PositiveInfinity,
                            new Vector2Int(j, i)
                        )
                    );
                }
                else
                {
                    visited[i, j] = true;
                }
            }
        }

        if (furnitureBaseGrid[start.y, start.x] != null)
        {
            // Starting point is blocked.
            return cells;
        }
        cells[start.y, start.x] = new DijkstraCell(0, Vector2Int.zero);
        unvisited.RemoveAt(
            unvisited.BinarySearch(
                new DijkstraCell(float.PositiveInfinity, start),
                cellComparer
            )
        );
        unvisited.Add(new DijkstraCell(0, start));

        while (unvisited.Count > 0)
        {
            DijkstraCell currentCell = unvisited[unvisited.Count - 1];
            if (currentCell.distance == float.PositiveInfinity)
                break;
            unvisited.RemoveAt(unvisited.Count - 1);

            Vector2Int position = currentCell.offsetOrPosition;
            foreach (Vector2Int offset in directions)
            {
                Vector2Int adjacent = position + offset;
                if (
                    GridSystem.Instance.IndexInBound(adjacent)
                    && !visited[adjacent.y, adjacent.x]
                )
                {
                    float newDistance = currentCell.distance + 1;
                    if (newDistance < cells[adjacent.y, adjacent.x].distance)
                    {
                        unvisited.RemoveAt(
                            unvisited.BinarySearch(
                                new DijkstraCell(
                                    cells[adjacent.y, adjacent.x].distance,
                                    adjacent
                                ),
                                cellComparer
                            )
                        );
                        cells[adjacent.y, adjacent.x] = new DijkstraCell(
                            newDistance,
                            -offset
                        );
                        DijkstraCell newCell = new DijkstraCell(
                            newDistance,
                            adjacent
                        );
                        unvisited.Insert(
                            ~unvisited.BinarySearch(newCell, cellComparer),
                            newCell
                        );
                    }
                }
            }

            visited[position.y, position.x] = true;
        }

        return cells;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = Size.y - 1; i >= 0; i--)
        {
            for (int j = 0; j < Size.x; j++)
            {
                sb.Append(
                    furnitureBaseGrid[i, j] != null
                        ? furnitureBaseGrid[i, j].height
                        : 0
                );
                sb.Append(' ');
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private class CellComparer : IComparer<DijkstraCell>
    {
        public int Compare(DijkstraCell left, DijkstraCell right)
        {
            if (left.distance < right.distance)
            {
                return 1;
            }
            else if (left.distance > right.distance)
            {
                return -1;
            }
            else if (left.offsetOrPosition.y < right.offsetOrPosition.y)
            {
                return -1;
            }
            else if (left.offsetOrPosition.y > right.offsetOrPosition.y)
            {
                return 1;
            }
            else
            {
                return left.offsetOrPosition.x.CompareTo(
                    right.offsetOrPosition.x
                );
            }
        }
    }
}
