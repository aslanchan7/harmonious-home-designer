using UnityEngine;

[System.Serializable]
public class DirectedBox : BoundingBox
{
    public float rotation;

    public DirectedBox(Vector2 position, Vector2Int size, float rotation)
        : base(position, size)
    {
        this.rotation = rotation;
    }

    public Direction GetRelativeFace(Direction face)
    {
        return face.Rotate(rotation);
    }

    public BoundingBox GetNextToRelativeFace(Direction face, int width)
    {
        return GetNextToFace(GetRelativeFace(face), width);
    }
}
