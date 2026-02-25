using UnityEngine;

[System.Serializable]
public class BoundingBox
{
    public Vector2 position;
    public Vector2Int size;

    public BoundingBox(Vector2 position, Vector2Int size)
    {
        this.position = position;
        this.size = size;
    }

    public Vector2 opposite
    {
        get => position + size;
    }

    public float x
    {
        get => position.x;
    }

    public float y
    {
        get => position.y;
    }

    public int sizeX
    {
        get => size.x;
    }

    public int sizeY
    {
        get => size.y;
    }

    public float oppositeX
    {
        get => position.x + size.x;
    }

    public float oppositeY
    {
        get => position.y + size.y;
    }

    public Vector2 center
    {
        get => position + (Vector2)size / 2;
    }

    public static BoundingBox FromCenterAndSize(Vector2 center, Vector2Int size)
    {
        return new BoundingBox(center - (Vector2)size / 2, size);
    }

    public BoundingBox GetNextToFace(Direction direction, int width)
    {
        switch (direction)
        {
            case Direction.UP:
                return new BoundingBox(new(x, oppositeY), new(sizeX, width));
            case Direction.RIGHT:
                return new BoundingBox(new(oppositeX, y), new(width, sizeY));
            case Direction.DOWN:
                return new BoundingBox(new(x, y - width), new(sizeX, width));
            default:
                return new BoundingBox(new(x - width, y), new(width, sizeY));
        }
    }

    public BoundingBox Normalize()
    {
        float newX = x;
        float newY = y;
        int newSizeX = sizeX;
        int newSizeY = sizeY;
        if (sizeX < 0)
        {
            newX = oppositeX;
            newSizeX = -sizeX;
        }
        if (sizeY < 0)
        {
            newY = oppositeY;
            newSizeY = -sizeY;
        }
        return new(new(newX, newY), new(newSizeX, newSizeY));
    }

    public BoundingBox Clamp()
    {
        float maxX = GridSystem.Instance.Size.x / 2.0f;
        float maxY = GridSystem.Instance.Size.y / 2.0f;
        float newX = x;
        float newY = y;
        int newSizeX = sizeX;
        int newSizeY = sizeY;
        if (newX < -maxX)
        {
            newSizeX -= (int)(-maxX - newX);
            newX = -maxX;
        }
        if (newX + newSizeX > maxX)
        {
            newSizeX -= (int)(newX + newSizeX - maxX);
        }
        if (newY < -maxY)
        {
            newSizeY -= (int)(-maxY - newY);
            newY = -maxY;
        }
        if (newY + newSizeY > maxY)
        {
            newSizeY -= (int)(newY + newSizeY - maxY);
        }
        return new(new(newX, newY), new(newSizeX, newSizeY));
    }

    public bool ColumnConstains(BoundingBox other)
    {
        return x <= other.x && other.oppositeX <= oppositeX;
    }

    public bool RowConstains(BoundingBox other)
    {
        return y <= other.y && other.oppositeY <= oppositeY;
    }

    public bool ColumnIntersects(BoundingBox other)
    {
        return position.x < other.oppositeX && other.x < oppositeX;
    }

    public bool RowIntersects(BoundingBox other)
    {
        return position.y < other.oppositeY && other.y < oppositeY;
    }

    public bool Contains(BoundingBox other)
    {
        return ColumnConstains(other) && RowConstains(other);
    }

    public bool Intersects(BoundingBox other)
    {
        return ColumnIntersects(other) && RowIntersects(other);
    }

    /// <summary>
    /// Finds the intersection with another bounding box. The size of the
    /// resulting bounding box can be negative, which indicates a gap in
    /// the corresponding x/y dimension.
    /// </summary>
    /// <param name="other">The other bounding box to intersect with.</param>
    /// <returns>
    /// A bounding box indicating the intersection, possibly negative
    /// in size.
    /// </returns>
    public BoundingBox IntersectionOrGapWith(BoundingBox other)
    {
        float maxX = Mathf.Max(x, other.x);
        float minOppositeX = Mathf.Min(oppositeX, other.oppositeX);
        int deltaX = Mathf.RoundToInt(minOppositeX - maxX);
        float maxY = Mathf.Max(y, other.y);
        float minOppositeY = Mathf.Min(oppositeY, other.oppositeY);
        int deltaY = Mathf.RoundToInt(minOppositeY - maxY);

        return new BoundingBox(
            new Vector2(maxX, maxY),
            new Vector2Int(deltaX, deltaY)
        );
    }

    public bool ToTopOf(BoundingBox other)
    {
        return y >= other.oppositeY;
    }

    public bool ToBottomOf(BoundingBox other)
    {
        return oppositeY <= other.y;
    }

    public bool ToLeftOf(BoundingBox other)
    {
        return oppositeX <= other.x;
    }

    public bool ToRightOf(BoundingBox other)
    {
        return x >= other.oppositeX;
    }

    public bool TouchesToTopOf(BoundingBox other)
    {
        return y == other.oppositeY;
    }

    public bool TouchesToBottomOf(BoundingBox other)
    {
        return oppositeY == other.y;
    }

    public bool TouchesToLeftOf(BoundingBox other)
    {
        return oppositeX == other.x;
    }

    public bool TouchesToRightOf(BoundingBox other)
    {
        return x == other.oppositeX;
    }

    public bool InLineWithFaceOf(BoundingBox other, Direction otherDirection)
    {
        switch (otherDirection)
        {
            case Direction.UP:
            case Direction.DOWN:
                return ColumnIntersects(other);
            case Direction.RIGHT:
            case Direction.LEFT:
                return RowIntersects(other);
            default:
                return false;
        }
    }

    public bool ToFaceOf(BoundingBox other, Direction otherDirection)
    {
        switch (otherDirection)
        {
            case Direction.UP:
                return ToTopOf(other);
            case Direction.RIGHT:
                return ToRightOf(other);
            case Direction.DOWN:
                return ToBottomOf(other);
            case Direction.LEFT:
                return ToLeftOf(other);
            default:
                return false;
        }
    }

    public bool TouchesToFaceOf(BoundingBox other, Direction otherDirection)
    {
        switch (otherDirection)
        {
            case Direction.UP:
                return TouchesToTopOf(other);
            case Direction.RIGHT:
                return TouchesToRightOf(other);
            case Direction.DOWN:
                return TouchesToBottomOf(other);
            case Direction.LEFT:
                return TouchesToLeftOf(other);
            default:
                return false;
        }
    }

    public bool IsFlat()
    {
        return sizeX == 0 || sizeY == 0;
    }

    public bool IsFlatAndVertical()
    {
        return sizeX == 0;
    }

    public bool IsFlatAndHorizontal()
    {
        return sizeY == 0;
    }

    public bool IsZero()
    {
        return sizeX == 0 && sizeY == 0;
    }

    public bool IsHorizontalGap()
    {
        return sizeX <= 0 && sizeY >= 0;
    }

    public bool IsVerticalGap()
    {
        return sizeX >= 0 && sizeY <= 0;
    }

    public static bool operator ==(BoundingBox box1, BoundingBox box2)
    {
        return box1.position == box2.position && box1.size == box2.size;
    }

    public static bool operator !=(BoundingBox box1, BoundingBox box2)
    {
        return box1.position != box2.position || box1.size != box2.size;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        BoundingBox casted = (BoundingBox)obj;
        return casted.position == position && casted.size == size;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
