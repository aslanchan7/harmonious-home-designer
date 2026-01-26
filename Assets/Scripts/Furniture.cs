using System;
using UnityEngine;

public class Furniture : MonoBehaviour
{
    [Header("Furniture Settings")]
    public string furnitureName;
    public Vector2Int Size;
    [HideInInspector] public Vector2 LastValidPos;
    private Vector2 centeringOffset = new(0f, 0f); // This is to center the furniture to the grid based on whether it is even/odd length

    [Header("References")]
    // public MeshRenderer[] MeshRenderers;
    public SerializableTuple<MeshRenderer, Material>[] MeshRenderers;
    public Collider[] Colliders;
    public Material NormalMat, GhostMat, InvalidGhostMat;

    private void Start()
    {
        LastValidPos = new(transform.position.x, transform.position.z);

        centeringOffset.x = Size.x % 2 == 0 ? 0.5f : 0f;
        centeringOffset.y = Size.y % 2 == 0 ? 0.5f : 0f;
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
        Size = new(Size.y, Size.x);
    }

    public void MoveGhost(Vector2 position)
    {
        // Visually move ghost furniture
        transform.position = new Vector3(position.x, transform.position.y, position.y);

        // Check if position is a valid pos for the object to move
        Vector2Int gridPos = GridSystem.Instance.GetGridPosFromWorldPos(new(position.x, 0, position.y));
        bool valid = GridSystem.Instance.ValidPosForFurniture(this, gridPos);
        
        // Set materials for mesh renderers
        foreach (SerializableTuple<MeshRenderer, Material> tuple in MeshRenderers)
        {
            tuple.Item1.material = valid ? GhostMat : InvalidGhostMat;
        }
    }

    public void TryPlace(Vector2 position)
    {
        Vector2Int gridPos = GridSystem.Instance.GetGridPosFromWorldPos(new(position.x, 0, position.y));
        
        bool valid = GridSystem.Instance.ValidPosForFurniture(this, gridPos);
        if (valid)
        {
            SetPosition(position);
        }
        else
        {
            SetPosition(LastValidPos);
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

    // void Update()
    // {
    //     // TESTING SETPOSITION & SETROTATION, TODO: REMOVE AFTER PLAYER INPUTS IMPLEMENTED -----------------------------
    //     // -- testing SetPosition
    //     if (Input.GetKeyDown(KeyCode.A))
    //         SetPosition(new Vector2(transform.position.x - 1f, transform.position.z));
    //     if (Input.GetKeyDown(KeyCode.D))
    //         SetPosition(new Vector2(transform.position.x + 1f, transform.position.z));
    //     if (Input.GetKeyDown(KeyCode.W))
    //         SetPosition(new Vector2(transform.position.x, transform.position.z + 1f));
    //     if (Input.GetKeyDown(KeyCode.S))
    //         SetPosition(new Vector2(transform.position.x, transform.position.z - 1f));
    //     // -- testing SetRotation
    //     if (Input.GetKeyDown(KeyCode.R))
    //         SetRotation(transform.eulerAngles.y + 90f);
    //     // -------------------------------------------------------------------------------------------------------------
    // }
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