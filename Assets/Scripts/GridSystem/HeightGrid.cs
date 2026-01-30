using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

public class HeightGrid
{
    public Vector2Int Size;
    public float[,] heightValues;

    public delegate bool HeightCheckFunction(float heightValue);

    public readonly struct DijkstraCell
    {
        public readonly float distance;
        public readonly Vector2Int offsetOrPosition;

        public DijkstraCell(float distanceFromSource, Vector2Int lastOffsetOrPosition)
        {
            distance = distanceFromSource;
            offsetOrPosition = lastOffsetOrPosition;
        }
    }

    public HeightGrid(Vector2Int size)
    {
        Size = size;
        heightValues = new float[size.y, size.x];
    }

    public void BoundingBoxToIndices(
        BoundingBox boundingBox,
        out Vector2Int starting,
        out Vector2Int ending
    )
    {
        Vector2 offset = (Vector2) Size / 2.0f;
        starting = Vector2Int.RoundToInt(boundingBox.position + offset); 
        ending = Vector2Int.RoundToInt(boundingBox.opposite + offset);
    }

    public void Set(BoundingBox boundingBox, float newHeightValue)
    {
        Vector2Int starting, ending;
        BoundingBoxToIndices(boundingBox, out starting, out ending);
        for (int i = starting.y; i < ending.y; i++)
        {
            for (int j = starting.x; j < ending.x; j++)
            {
                heightValues[i, j] = newHeightValue;
            }
        }
    }

    public float Get(Vector2Int vector)
    {
        return heightValues[vector.y, vector.x];
    }

    public bool ExistsColumnSatisfiesAll<T>(
        T[,] values,
        BoundingBox boundingBox,
        Func<T, bool> checkFunction
    )
    {
        Vector2Int starting, ending;
        BoundingBoxToIndices(boundingBox, out starting, out ending);
        for (int j = starting.x; j < ending.x; j++)
        {
            bool satisfied = true;
            for (int i = starting.y; i < ending.y; i++)
            {
                if (!checkFunction(values[i, j]))
                {
                    satisfied = false;
                    break;
                } 
            }

            if (satisfied) return true;
        }

        return false;
    }

    public bool ExistsRowSatisfiesAll<T>(
        T[,] values,
        BoundingBox boundingBox,
        Func<T, bool> checkFunction
    )
    {
        Vector2Int starting, ending;
        BoundingBoxToIndices(boundingBox, out starting, out ending);
        for (int i = starting.y; i < ending.y; i++)
        {
            bool satisfied = true;
            for (int j = starting.x; j < ending.x; j++)
            {
                if (!checkFunction(values[i, j]))
                {
                    satisfied = false;
                    break;
                } 
            }

            if (satisfied) return true;
        }

        return false;
    }

    public bool ExistsLineSatisfiesAll<T>(
        T[,] values,
        BoundingBox intersection,
        Func<T, bool> heightCheckFunction
    )
    {
        if (intersection.IsHorizontalGap())
        {
            intersection.Normalize();
            return ExistsRowSatisfiesAll(values, intersection, heightCheckFunction);
        }
        
        if (intersection.IsVerticalGap())
        {
            intersection.Normalize();
            return ExistsColumnSatisfiesAll(values, intersection, heightCheckFunction); 
        }

        throw new Exception(
            "Intersection must be horizontal or vertical, found intersection "
            + "with size: "
            + intersection.size
        );
    }


    public bool SatisfiesAll<T>(
        T[,] values,
        BoundingBox boundingBox,
        Func<T, bool> checkFunction
    )
    {
        Vector2Int starting, ending;
        BoundingBoxToIndices(boundingBox, out starting, out ending);
        for (int j = starting.x; j < ending.x; j++)
        {
            for (int i = starting.y; i < ending.y; i++)
            {
                if (!checkFunction(values[i, j]))
                    return false;
            }
        }

        return true;
    }

    public DijkstraCell[,] Dijkstra(BoundingBox startSquare, float maxHeight)
    {
        Vector2Int start;
        BoundingBoxToIndices(startSquare, out start, out _);
        List<DijkstraCell> unvisited = new List<DijkstraCell>();
        DijkstraCell[,] cells = new DijkstraCell[Size.y, Size.x];
        bool[,] visited = new bool[Size.y, Size.x];
        CellComparer cellComparer = new CellComparer();
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        for (int i = 0; i < Size.y; i++)
        {
            for (int j = 0; j < Size.x; j++)
            {
                cells[i, j] = new DijkstraCell(
                    float.PositiveInfinity,
                    Vector2Int.zero
                );
                if (heightValues[i, j] <= maxHeight)
                {
                    unvisited.Add(new DijkstraCell(
                        float.PositiveInfinity,
                        new Vector2Int(j, i)
                    ));
                } else
                {
                    visited[i, j] = true;
                }
            }
        }

        cells[start.y, start.x] = new DijkstraCell(
            0,
            Vector2Int.zero
        );
        unvisited.RemoveAt(unvisited.BinarySearch(new DijkstraCell(
            float.PositiveInfinity,
            start
        ), cellComparer));
        unvisited.Add(new DijkstraCell(
            0,
            start
        ));

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
                if (InBound(adjacent) && !visited[adjacent.y, adjacent.x])
                {
                    float newDistance = currentCell.distance + 1;
                    if (newDistance < cells[adjacent.y, adjacent.x].distance)
                    {
                        unvisited.RemoveAt(
                            unvisited.BinarySearch(new DijkstraCell(
                                cells[adjacent.y, adjacent.x].distance,
                                adjacent
                            ), cellComparer)
                        );
                        cells[adjacent.y, adjacent.x] = new DijkstraCell(
                            newDistance,
                            -offset
                        );
                        DijkstraCell newCell = new DijkstraCell(newDistance, adjacent);
                        unvisited.Insert(~unvisited.BinarySearch(newCell, cellComparer), newCell);
                    }
                }
            }

            visited[position.y, position.x] = true;
        }

        return cells;
    }

    private bool InBound(Vector2Int vector)
    {
        return 0 <= vector.x && vector.x < Size.x
               && 0 <= vector.y && vector.y < Size.y;
    }

    override public string ToString()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = Size.y - 1; i >= 0; i--)
        {
            for (int j = 0; j < Size.x; j++)
            {
                sb.Append(heightValues[i, j]);
                sb.Append(' ');
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private class CellComparer: IComparer<DijkstraCell>
    {
        public int Compare(DijkstraCell left, DijkstraCell right)
        {
            if (left.distance < right.distance)
            {
                return 1;
            } else if (left.distance > right.distance)
            {
                return -1;
            } else if (left.offsetOrPosition.y < right.offsetOrPosition.y)
            {
                return -1;
            } else if (left.offsetOrPosition.y > right.offsetOrPosition.y)
            {
                return 1;
            } else
            {
                return left.offsetOrPosition.x.CompareTo(right.offsetOrPosition.x);
            }
        }
    }
}