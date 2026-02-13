using System;
using UnityEngine;

public class Furniture : MonoBehaviour
{
    private Vector2 _displayPosition;
    private float _displayRotation;

    [Header("Furniture Settings")]
    public string furnitureName;
    public Vector2Int Size;
    public float height;

    [HideInInspector]
    public Vector2 LastValidPosition;

    [HideInInspector]
    public float LastValidRotation;

    [HideInInspector]
    public Vector2Int StartingSize;

    [Header("References")]
    public SerializableTuple<MeshRenderer, Material>[] MeshRenderers;
    public Collider[] Colliders;
    public Material GhostMat,
        InvalidGhostMat;
    public Transform ShapeUnits;

    public Vector2 DisplayPosition
    {
        get { return _displayPosition; }
        set
        {
            transform.position = new Vector3(
                value.x,
                transform.position.y,
                value.y
            );
            _displayPosition = value;
        }
    }
    public float DisplayRotation
    {
        get { return _displayRotation; }
        set
        {
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                value % 360,
                transform.eulerAngles.z
            );
            ResetSize();
            _displayRotation = value % 360;
        }
    }

    // private void Start()
    // {
    //     InitializeState();
    // }

    public void InitializeState()
    {
        _displayPosition = new(transform.position.x, transform.position.z);
        _displayRotation = transform.localRotation.eulerAngles.y;
        StartingSize = Size;
        height = Colliders[0].bounds.max.y;
        LastValidPosition = DisplayPosition;
        LastValidRotation = DisplayRotation;
        WinCondition.Instance.AddFurnitureIfRequired(this);
        GridSystem.Instance.placedFurnitures.Set(GetBoundingBox(), this);
        WinCondition.Instance.UpdateRuleCheck();
    }

    public Furniture InstantiatePrefab()
    {
        GameObject newGameObject = Instantiate(gameObject);
        Furniture newFurniture = newGameObject.GetComponent<Furniture>();
        newFurniture.InitializeState();
        return newFurniture;
    }

    public void DestroyPrefab()
    {
        WinCondition.Instance.RemoveFurnitureIfRegistered(this);
        GridSystem.Instance.placedFurnitures.Set(GetBoundingBox(), null);
        WinCondition.Instance.UpdateRuleCheck();
        Destroy(gameObject);
    }

    // Update lastValidPos and lastValidRotation;
    public void SetLocationAsValid()
    {
        GridSystem.Instance.placedFurnitures.Set(GetBoundingBox(), null);
        LastValidPosition = DisplayPosition;
        LastValidRotation = DisplayRotation;
        // TODO: Change the type of sfx played
        SFXManager.Instance.PlaySFX(SFXType.Place_Wood);
        GridSystem.Instance.placedFurnitures.Set(GetBoundingBox(), this);
        WinCondition.Instance.UpdateRuleCheck();
    }

    public void ResetToValidLocation()
    {
        DisplayPosition = LastValidPosition;
        DisplayRotation = LastValidRotation;
    }

    public void ResetSize()
    {
        Size =
            Mathf.Abs(transform.eulerAngles.y) < 0.1f
            || Mathf.Abs(Mathf.Abs(transform.eulerAngles.y) - 180f) < 0.1f
                ? StartingSize
                : new(StartingSize.y, StartingSize.x);
    }

    public void MoveGhost(Vector2 position)
    {
        // Visually move ghost furniture
        DisplayPosition = position;

        // Check if position is a valid pos for the object to move
        bool valid = CheckValidPos();

        // Set materials for mesh renderers
        foreach (
            SerializableTuple<MeshRenderer, Material> tuple in MeshRenderers
        )
        {
            tuple.Item1.material = valid ? GhostMat : InvalidGhostMat;
        }
    }

    public void TryPlace()
    {
        bool isValidPosition = CheckValidPos();
        SetColliderEnabled(true);
        SetNormalMat();
        if (isValidPosition)
        {
            SetLocationAsValid();
        }
        else
        {
            ResetToValidLocation();
        }
    }

    public void SetNormalMat()
    {
        foreach (
            SerializableTuple<MeshRenderer, Material> tuple in MeshRenderers
        )
        {
            tuple.Item1.material = tuple.Item2;
        }
    }

    public void SetColliderEnabled(bool enabled)
    {
        foreach (Collider collider in Colliders)
        {
            collider.enabled = enabled;
        }
    }

    public bool CheckValidPos()
    {
        for (int i = 0; i < ShapeUnits.childCount; i++)
        {
            // raycast at shapeUnit
            if (
                !Physics.Raycast(
                    ShapeUnits.GetChild(i).position,
                    Vector3.down,
                    out RaycastHit hit,
                    100f
                ) || !hit.collider.CompareTag("Floor")
            )
            {
                return false;
            }
        }
        return true;
    }

    public BoundingBox GetBoundingBox()
    {
        return BoundingBox.FromCenterAndSize(
            LastValidPosition,
            Mathf.Abs(LastValidRotation) < 0.1f
            || Mathf.Abs(Mathf.Abs(LastValidRotation) - 180f) < 0.1f
                ? StartingSize
                : new(StartingSize.y, StartingSize.x)
        );
    }

    public Direction GetFace(Direction face)
    {
        return face.Rotate(LastValidRotation);
    }

    public BoundingBox GetNextToFace(Direction face, int width)
    {
        return GetBoundingBox().GetNextToFace(GetFace(face), width);
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
