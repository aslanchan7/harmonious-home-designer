using System;
using System.Collections.Generic;
using UnityEngine;

public static class Functional
{
    public static List<T> Map<T, U>(List<U> list, Func<U, T> func)
    {
        List<T> result = new List<T>(list.Count);
        foreach (U item in list)
            result.Add(func(item));

        return result;
    }

    public static bool ExistsColumnSatisfiesAll<T>(
        T[,] values,
        BoundingBox boundingBox,
        Func<T, bool> checkFunction
    )
    {
        GridSystem.Instance.WorldBoxToIndices(
            boundingBox,
            out Vector2Int starting,
            out Vector2Int ending
        );
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

            if (satisfied)
                return true;
        }

        return false;
    }

    public static bool ExistsRowSatisfiesAll<T>(
        T[,] values,
        BoundingBox boundingBox,
        Func<T, bool> checkFunction
    )
    {
        GridSystem.Instance.WorldBoxToIndices(
            boundingBox,
            out Vector2Int starting,
            out Vector2Int ending
        );
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

            if (satisfied)
                return true;
        }

        return false;
    }

    public static bool ExistsLineSatisfiesAll<T>(
        T[,] values,
        BoundingBox intersection,
        Func<T, bool> heightCheckFunction
    )
    {
        BoundingBox normalized = intersection.Normalize();
        if (intersection.IsHorizontalGap())
        {
            return ExistsRowSatisfiesAll(
                values,
                normalized,
                heightCheckFunction
            );
        }

        if (intersection.IsVerticalGap())
        {
            return ExistsColumnSatisfiesAll(
                values,
                normalized,
                heightCheckFunction
            );
        }

        throw new Exception(
            "Intersection must be horizontal or vertical, found intersection "
                + "with size: "
                + intersection.size
        );
    }

    public static bool SatisfiesAll<T>(
        T[,] values,
        BoundingBox boundingBox,
        Func<T, bool> checkFunction
    )
    {
        GridSystem.Instance.WorldBoxToIndices(
            boundingBox,
            out Vector2Int starting,
            out Vector2Int ending
        );
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

    public static bool Any<T>(
        IEnumerable<T> values,
        Func<T, bool> checkFunction
    )
    {
        foreach (T value in values)
        {
            if (checkFunction(value))
                return true;
        }
        return false;
    }

    public static bool All<T>(
        IEnumerable<T> values,
        Func<T, bool> checkFunction
    )
    {
        foreach (T value in values)
        {
            if (!checkFunction(value))
                return false;
        }
        return true;
    }

    public static int Count<T>(
        IEnumerable<T> values,
        Func<T, bool> checkFunction
    )
    {
        int count = 0;
        foreach (T value in values)
        {
            if (checkFunction(value))
                count++;
        }
        return count;
    }
}
