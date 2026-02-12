using UnityEngine;

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
        get { return position + size; }
    }

    public float x
    {
        get { return position.x; }
        set { position.x = value; }
    }

    public float y
    {
        get { return position.y; }
        set { position.y = value; }
    }

    public int sizeX
    {
        get { return size.x; }
        set { size.x = value; }
    }

    public int sizeY
    {
        get { return size.y; }
        set { size.y = value; }
    }

    public float oppositeX
    {
        get { return position.x + size.x; }
    }

    public float oppositeY
    {
        get { return position.y + size.y; }
    }

    public Vector2 center
    {
        get { return position + (Vector2) size / 2; }
    }

    public static BoundingBox FromCenterAndSize(Vector2 center, Vector2Int size)
    {
        return new BoundingBox(
          center - (Vector2) size / 2,
          size
        );
    }

    public BoundingBox GetNextToFace(Direction direction, int width)
    {
        switch (direction)
        {
            case Direction.UP:
                return new BoundingBox(
                    new (x, oppositeY), 
                    new (sizeX, width)
                );
            case Direction.RIGHT:
                return new BoundingBox(
                    new (oppositeX, y),
                    new (width, sizeY)
                );
            case Direction.DOWN:
                return new BoundingBox(
                    new (x, y - width), 
                    new (sizeX, width)
                );
            default:
                return new BoundingBox(
                    new (x - width, y), 
                    new (width, sizeY)
                );
        }
    }

    public void Normalize()
    {
        if (sizeX < 0)
        {
            x = oppositeX;
            sizeX = -sizeX;
        }
        if (sizeY < 0)
        {
            y = oppositeY;
            sizeY = -sizeY;
        }
    }

    public void Clamp()
    {
        float maxX = GridSystem.Instance.Size.x / 2.0f;
        float maxY = GridSystem.Instance.Size.y / 2.0f;
        if (x < -maxX)
        {
            sizeX -= (int) (-maxX - x);
            x = -maxX;
        }
        if (oppositeX > maxX)
        {
            sizeX -= (int) (oppositeX - maxX);
        }
        if (y < -maxY)
        {
            sizeY -= (int) (-maxY - y);
            y = -maxY;
        }
        if (oppositeY > maxY)
        {
            sizeY -= (int) (oppositeY - maxY);
        }
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
        return x  == other.oppositeX;
    }
    
    public bool InLineWithFaceOf(
        BoundingBox other,
        Direction otherDirection
    )
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
}