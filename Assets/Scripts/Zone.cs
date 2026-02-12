using UnityEngine;

public class Zone
{
    [Header("Furniture Settings")]
    public string furnitureName;
    public Vector2 Position;
    [HideInInspector] public float Rotation;
    public Vector2Int Size;
    [HideInInspector] public BoundingBox boundingBox;

    public Zone(Vector2 position, Vector2Int size, float rotation)
    {
        Position = position;
        Size = size;
        Rotation = rotation;
        boundingBox = BoundingBox.FromCenterAndSize(Position, Size);
    }

    public Direction GetRotatedFace(Direction face)
    {
        return face.Rotate(Rotation);
    }
}
