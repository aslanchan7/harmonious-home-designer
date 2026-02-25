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

    private Furniture LastValidBase;
    private Furniture DisplayBase;

    private bool hasUnsetPlacedFurniture = true;

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
    public List<SerializableTuple<MeshRenderer, Material>> MeshRenderers;
    public Collider[] Colliders;
    public Material GhostMat,
        InvalidGhostMat;
    public Transform ShapeUnits;

    public ParticleSystem pickupVFX;
    public ParticleSystem placeVFX;

    public void PlayPickupVFX()
    {
        if (pickupVFX == null) return;
       Instantiate(placeVFX, transform.position + Vector3.up * 0.05f, Quaternion.identity);
    }

    public void PlayPlaceVFX()
    {
        if (placeVFX == null) return;
        Instantiate(placeVFX, transform.position + Vector3.up * 0.05f, Quaternion.identity);
    }

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
        LastValidBase = null;
        DisplayBase = null;
        carrying = new List<Furniture>();

        // Match MeshRenderers with Materials
        MeshRenderers.Clear();
        foreach (MeshRenderer meshRenderer in transform.GetChild(0).GetComponentsInChildren<MeshRenderer>())
        {
            SerializableTuple<MeshRenderer, Material> newTuple = new(meshRenderer, meshRenderer.material);
            MeshRenderers.Add(newTuple);
        }
    }

    void Start()
    {
        InitializeState();
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
        PlacedFurnituresUnset();
        if (canBeStackedOn)
        {
            while (carrying.Count > 0)
            {
                Furniture furniture = carrying[carrying.Count - 1];
                furniture.PlacedFurnituresUnset();
                furniture.DisplayBase = null;
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
        if (!hasUnsetPlacedFurniture)
        {
            throw new InvalidOperationException(
                "Trying to set this location as valid without unsetting the "
                    + "PlacedFurniture object first."
            );
        }
        LastValidPosition = DisplayPosition;
        LastValidElevation = DisplayElevation;
        LastValidRotation = DisplayRotation;
        if (canStackOnOthers)
        {
            AttachOrDetach();
        }
        TryRecursiveStack(
            (furniture) =>
            {
                furniture.PlacedFurnituresUnset();
                furniture.DisplayBase = this;
                furniture.SetLocationAsValid();
            }
        );
        // TODO: Change the type of sfx played
<<<<<<< Updated upstream
        SFXManager.Instance.PlaySFX(SFXType.Place_Wood);
=======
        // SFXManager.Instance.PlaySFX(SFXType.Place_Wood);
        SFXManager.Instance?.PlayFurnitureSFX(sfxCategory, SFXAction.Place);
        PlayPlaceVFX();
>>>>>>> Stashed changes
        PlacedFurnitureSet();
        WinCondition.Instance.UpdateRuleCheck();
    }

    public void ResetToValidLocation()
    {
        DisplayPosition = LastValidPosition;
        DisplayElevation = LastValidElevation;
        DisplayRotation = LastValidRotation;
        PlacedFurnitureSet();
    }

    public void PlacedFurnituresUnset()
    {
        SetColliderEnabled(false);
        PlacedFurnituresSet(null);
        hasUnsetPlacedFurniture = true;
    }

    public void PlacedFurnitureSet()
    {
        SetColliderEnabled(true);
        PlacedFurnituresSet(this);
        hasUnsetPlacedFurniture = false;
    }

    private void PlacedFurnituresSet(Furniture furniture)
    {
        if (LastValidBase == null)
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
        Material material = valid ? GhostMat : InvalidGhostMat;

        // Set materials for mesh renderers
        foreach (
            SerializableTuple<MeshRenderer, Material> tuple in MeshRenderers
        )
        {
            tuple.Item1.material = material;
        }

        TryRecursiveStack(
            (furniture) =>
            {
                foreach (
                    SerializableTuple<
                        MeshRenderer,
                        Material
                    > tuple in furniture.MeshRenderers
                )
                {
                    tuple.Item1.material = material;
                }
            }
        );
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
        TryRecursiveStack((furniture) => furniture.SetNormalMat());
    }

    public void SetColliderEnabled(bool enabled)
    {
        foreach (Collider collider in Colliders)
        {
            collider.enabled = enabled;
        }
        TryRecursiveStack((furniture) => furniture.SetColliderEnabled(enabled));
    }

    public bool CheckValidPos()
    {
        BoundingBox displayBox = GetDisplayBoundingBox();
        BoundingBox clamped = displayBox.Clamp();
        if (clamped != displayBox)
            return false;

        PlacedFurnitures.Instance.BoundingBoxToIndices(
            displayBox,
            out Vector2Int starting,
            out _
        );
        Furniture bottomLeftBase = PlacedFurnitures.Instance.GetBase(starting);
        bool wholeAreaHasSameBase = PlacedFurnitures.Instance.SatisfiesAll(
            PlacedFurnitures.Instance.furnitureBaseGrid,
            displayBox,
            (Furniture f) => f == bottomLeftBase
        );

        if (!canStackOnOthers)
        {
            return bottomLeftBase == null && wholeAreaHasSameBase;
        }

        if (
            bottomLeftBase != null && !bottomLeftBase.canBeStackedOn
            || !wholeAreaHasSameBase
        )
        {
            return false;
        }

        DisplayBase = bottomLeftBase;
        DisplayElevation = bottomLeftBase != null ? bottomLeftBase.height : 0;

        return bottomLeftBase == null
            || PlacedFurnitures.Instance.SatisfiesAll(
                PlacedFurnitures.Instance.furnitureStackGrid,
                displayBox,
                (Furniture f) => f == null
            );
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
        if (DisplayBase == LastValidBase)
            return;
        if (LastValidBase != null)
        {
            LastValidBase.carrying.Remove(this);
        }

        LastValidBase = DisplayBase;

        if (LastValidBase != null)
        {
            LastValidBase.carrying.Add(this);
        }

        transform.SetParent(
            LastValidBase != null ? LastValidBase.transform : null,
            true
        );
    }

    private delegate void FurnitureFunc(Furniture furniture);

    private void TryRecursiveStack(FurnitureFunc func)
    {
        if (canBeStackedOn)
        {
            foreach (Furniture furniture in carrying)
            {
                func(furniture);
            }
        }
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
