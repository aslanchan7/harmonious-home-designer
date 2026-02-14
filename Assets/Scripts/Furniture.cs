using System;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : MonoBehaviour
{
    [Header("Furniture Settings")]
    public string furnitureName;
    public Vector2Int Size;
    public float height;
    public bool canBeStackedOn; // If true, other furniture items can stack on top of this one
    public bool canStackOnOthers; // If ture, this furniture item can stack on top of others (naming variables is very hard...)
    public bool lockRotation;
    public bool acceptRotationPassThrough;

    private Furniture stackBase;
    private Furniture lastStackCandidate;
    private List<Furniture> carrying;

    [HideInInspector]
    public Vector2 LastValidPosition;

    [HideInInspector]
    public float LastValidElevation;

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
        get { return new(transform.position.x, transform.position.z); }
        set
        {
            transform.position = new Vector3(
                value.x,
                transform.position.y,
                value.y
            );
        }
    }
    public float DisplayElevation
    {
        get { return transform.position.y; }
        set
        {
            transform.position = new Vector3(
                transform.position.x,
                value,
                transform.position.z
            );
        }
    }
    public float DisplayRotation
    {
        get { return transform.eulerAngles.y; }
        set
        {
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                value % 360,
                transform.eulerAngles.z
            );
            ResetSize();
        }
    }

    public void InitializeState()
    {
        StartingSize = Size;
        height = Colliders[0].bounds.max.y;
        LastValidPosition = DisplayPosition;
        LastValidElevation = DisplayElevation;
        LastValidRotation = DisplayRotation;
        stackBase = null;
        lastStackCandidate = null;
        carrying = new List<Furniture>();
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
        ColliderOff();
        if (canBeStackedOn)
        {
            while (carrying.Count > 0)
            {
                Furniture furniture = carrying[carrying.Count - 1];
                furniture.ColliderOff();
                furniture.lastStackCandidate = null;
                furniture.DisplayElevation = 0;
                furniture.SetLocationAsValid();
            }
        }
        WinCondition.Instance.UpdateRuleCheck();
        Destroy(gameObject);
    }

    // Update lastValidPos and lastValidRotation;
    public void SetLocationAsValid()
    {
        LastValidPosition = DisplayPosition;
        LastValidElevation = DisplayElevation;
        LastValidRotation = DisplayRotation;
        AttachOrDetach();
        if (canBeStackedOn)
        {
            foreach (Furniture furniture in carrying)
            {
                furniture.lastStackCandidate = this;
                furniture.SetLocationAsValid();
            }
        }
        // TODO: Change the type of sfx played
        SFXManager.Instance.PlaySFX(SFXType.Place_Wood);
        ColliderOn();
    }

    public void ResetToValidLocation()
    {
        DisplayPosition = LastValidPosition;
        DisplayElevation = LastValidElevation;
        DisplayRotation = LastValidRotation;
        ColliderOn();
    }

    public void ColliderOff()
    {
        SetColliderEnabled(false);
        PlacedFurnituresSet(null);
    }

    public void ColliderOn()
    {
        SetColliderEnabled(true);
        PlacedFurnituresSet(this);
        WinCondition.Instance.UpdateRuleCheck();
    }

    public void PlacedFurnituresSet(Furniture furniture)
    {
        if (stackBase == null)
        {
            PlacedFurnitures.Instance.SetBase(GetBoundingBox(), furniture);
        }
        else
        {
            PlacedFurnitures.Instance.SetStack(GetBoundingBox(), furniture);
        }
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
        SetNormalMat();
        if (CheckValidPos())
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
        BoundingBox displayBox = GetDisplayBoundingBox();
        BoundingBox clamped = displayBox.Clamp();
        if (clamped != displayBox)
            return false;

        if (!canStackOnOthers)
        {
            lastStackCandidate = null;
            return PlacedFurnitures.Instance.SatisfiesAll(
                PlacedFurnitures.Instance.furnitureBaseGrid,
                displayBox,
                (Furniture f) => f == null
            );
        }

        PlacedFurnitures.Instance.BoundingBoxToIndices(
            displayBox,
            out Vector2Int starting,
            out _
        );

        lastStackCandidate = PlacedFurnitures.Instance.GetBase(starting);

        if (lastStackCandidate != null && !lastStackCandidate.canBeStackedOn)
        {
            lastStackCandidate = null;
            return false;
        }

        if (
            !PlacedFurnitures.Instance.SatisfiesAll(
                PlacedFurnitures.Instance.furnitureBaseGrid,
                displayBox,
                (Furniture f) => f == lastStackCandidate
            )
        )
        {
            lastStackCandidate = null;
            return false;
        }

        if (lastStackCandidate == null)
        {
            DisplayElevation = 0;
            return true;
        }

        if (
            PlacedFurnitures.Instance.SatisfiesAll(
                PlacedFurnitures.Instance.furnitureStackGrid,
                displayBox,
                (Furniture f) => f == null
            )
        )
        {
            DisplayElevation = lastStackCandidate.height;
            return true;
        }

        lastStackCandidate = null;
        return false;
    }

    public BoundingBox GetDisplayBoundingBox()
    {
        return BoundingBox.FromCenterAndSize(
            DisplayPosition,
            Mathf.Abs(DisplayRotation) < 0.1f
            || Mathf.Abs(Mathf.Abs(DisplayRotation) - 180f) < 0.1f
                ? StartingSize
                : new(StartingSize.y, StartingSize.x)
        );
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

    private void AttachOrDetach()
    {
        // Keep world position/rotation when parenting
        if (lastStackCandidate == stackBase)
            return;
        if (stackBase != null)
        {
            stackBase.carrying.Remove(this);
        }
        if (lastStackCandidate == null)
        {
            stackBase = lastStackCandidate;
            transform.SetParent(null, true);
            return;
        }
        stackBase = lastStackCandidate;
        stackBase.carrying.Add(this);
        transform.SetParent(lastStackCandidate.transform, true);
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
