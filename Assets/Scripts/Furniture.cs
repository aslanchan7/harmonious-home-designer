using System;
using UnityEngine;

public class Furniture : MonoBehaviour
{
    [Header("Furniture Settings")]
    public string furnitureName;
    public Vector2Int Size;
    [HideInInspector] public Vector2 LastValidPos;
    [HideInInspector] public float LastValidRotation;
    [HideInInspector] public Vector2Int StartingSize;
    private Vector2 centeringOffset = new(0f, 0f); // This is to center the furniture to the grid based on whether it is even/odd length

    [Header("References")]
    public SerializableTuple<MeshRenderer, Material>[] MeshRenderers;
    public Collider[] Colliders;
    public Material NormalMat, GhostMat, InvalidGhostMat;
    public Transform ShapeUnits;

    private void Start()
    {
        LastValidPos = new(transform.position.x, transform.position.z);
        LastValidRotation = transform.localRotation.eulerAngles.y;

        centeringOffset.x = Size.x % 2 == 0 ? 0.5f : 0f;
        centeringOffset.y = Size.y % 2 == 0 ? 0.5f : 0f;

        StartingSize = Size;
    }

    // placing furniture on the xz-plane
    public void SetPosition(Vector2 position)
    {
        this.transform.position = new Vector3(position.x, this.transform.position.y, position.y);
        // TODO snap y-position based on objects caught in downward raycast
        // ^ grid system subtask, i believe
        LastValidPos = position;
    }

    // rotating furniture on the y-axis
    public void SetRotation(float rotation)
    {
        this.transform.eulerAngles = new Vector3(transform.eulerAngles.x, rotation, transform.eulerAngles.z);
        Size = Mathf.Abs(transform.eulerAngles.y) < 0.1f || Mathf.Abs(Mathf.Abs(transform.eulerAngles.y) - 180f) < 0.1f ? StartingSize : new(StartingSize.y, StartingSize.x);
    }

    public void ResetToValidLocation()
    {
        transform.position = new Vector3(LastValidPos.x, this.transform.position.y, LastValidPos.y);
        transform.eulerAngles = new(transform.eulerAngles.x, LastValidRotation, transform.eulerAngles.z);

        // Reset size of furniture
        Size = Mathf.Abs(transform.eulerAngles.y) < 0.1f || Mathf.Abs(Mathf.Abs(transform.eulerAngles.y) - 180f) < 0.1f ? StartingSize : new(StartingSize.y, StartingSize.x); 
    }

    public void MoveGhost(Vector2 position)
    {
        // Visually move ghost furniture
        transform.position = new Vector3(position.x, transform.position.y, position.y);

        // Check if position is a valid pos for the object to move
        Vector2Int gridPos = GridSystem.Instance.GetGridPosFromWorldPos(new(position.x, 0, position.y));
        // bool valid = GridSystem.Instance.ValidPosForFurniture(this, gridPos);
        bool valid = CheckValidPos();
        
        // Set materials for mesh renderers
        foreach (SerializableTuple<MeshRenderer, Material> tuple in MeshRenderers)
        {
            tuple.Item1.material = valid ? GhostMat : InvalidGhostMat;
        }
    }

    public void TryPlace(Vector2 position)
    {
        Vector2Int gridPos = GridSystem.Instance.GetGridPosFromWorldPos(new(position.x, 0, position.y));
        
        // bool valid = GridSystem.Instance.ValidPosForFurniture(this, gridPos);

        bool valid = CheckValidPos();
        if (valid)
        {
            SetPosition(position);
            LastValidRotation = transform.eulerAngles.y;
        }
        else
        {
            ResetToValidLocation();
        }

        SetNormalMat();
    }

    public void SetGhostMaterial()
    {
        foreach (SerializableTuple<MeshRenderer, Material> tuple in MeshRenderers)
        {
            tuple.Item1.material = GhostMat;
        }

        foreach (Collider collider in Colliders)
        {
            collider.enabled = false;
        }
    }

    public void SetNormalMat()
    {
        foreach (SerializableTuple<MeshRenderer, Material> tuple in MeshRenderers)
        {
            tuple.Item1.material = tuple.Item2;
        }

        foreach (Collider collider in Colliders)
        {
            collider.enabled = true;
        }
    }

    public void SetInvalidGhostMat()
    {
        foreach (SerializableTuple<MeshRenderer, Material> tuple in MeshRenderers)
        {
            tuple.Item1.material = InvalidGhostMat;
        }
    }

    public bool CheckValidPos()
    {
        for(int i = 0; i < ShapeUnits.childCount; i++)
        {
            // raycast at shapeUnit
            if(!Physics.Raycast(ShapeUnits.GetChild(i).position, Vector3.down, out RaycastHit hit, 100f) || !hit.collider.CompareTag("Floor"))
            {
                return false;
            }
        }
        return true;
    }
}

/*
This class is just a way for me to be able to serialize tuples.
*/
[Serializable]
public class SerializableTuple<T1, T2>
{
    public T1 Item1;
    public T2 Item2;

    public SerializableTuple(T1 item1, T2 item2)
    {
        Item1 = item1;
        Item2 = item2;
    }

    public override string ToString()
    {
        return $"({Item1}, {Item2})";
    }
}