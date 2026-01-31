using System;
using Unity.Mathematics;
using UnityEngine;

public enum Direction
{
    UP = 0,
    RIGHT = 90,
    DOWN = 180,
    LEFT = 270
}

public static class DirectionExtension
{
    public static Direction FromAngle(float angle)
    {
        return (Direction) Mathf.RoundToInt((angle + 360) % 360);
    }

    public static Direction Rotate(this Direction direction, float angle)
    {
        return (Direction) (int) (((int) direction + angle + 360) % 360);
    }

    public static Direction ToOpposite(this Direction direction)
    {
        return (Direction) (((int) direction + 180) % 360);
    }

    public static bool IsOppositeTo(this Direction direction, Direction other)
    {
        return Math.Abs((int) direction - (int) other) == 180;
    }

    public static Vector2Int ToVector(this Direction direction)
    {
        switch (direction)
        {
            case Direction.UP:
                return Vector2Int.up;
            case Direction.RIGHT:
                return Vector2Int.right;
            case Direction.DOWN:
                return Vector2Int.down;
            default:
                return Vector2Int.left;
        }
    }
}