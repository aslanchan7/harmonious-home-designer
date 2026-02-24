using UnityEngine;

public class Zone
{
    [Header("Furniture Settings")]
    public string furnitureName;
    public Vector2 Position;
    [HideInInspector] public float Rotation;
    public Vector2Int Size;
    public BoundingBox Box;

    public Zone(Vector2 position, Vector2Int size, float rotation)
    {
        Position = position;
        Size = size;
        Rotation = rotation;
        Box = BoundingBox.FromCenterAndSize(Position, Size);
    }

    public Direction GetFace(Direction face)
    {
        return face.Rotate(Rotation);
    }

    public BoundingBox GetNextToFace(Direction face, int width)
    {
        return Box.GetNextToFace(GetFace(face), width);
    }
}
