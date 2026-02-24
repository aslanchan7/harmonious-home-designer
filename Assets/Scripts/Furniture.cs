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
    public bool canBeStackedOn; // If true, other furniture items can stack on top of this one
    public bool canStackOnOthers; // If ture, this furniture item can stack on top of others (naming variables is very hard...)

    private Furniture _stackBase;
    private Furniture lastStackCandidate;

    [HideInInspector] public Vector2 LastValidPosition;
    [HideInInspector] public float LastValidRotation;
    [HideInInspector] public Vector2Int StartingSize;

    [Header("Audio")]
    public FurnitureSFXCategory sfxCategory = FurnitureSFXCategory.Default;

    [Header("References")]
    public SerializableTuple<MeshRenderer, Material>[] MeshRenderers;
    public Collider[] Colliders;
    public Material GhostMat, InvalidGhostMat;
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

    public void InitializeState()
    {
        _displayPosition = new(transform.position.x, transform.position.z);
        _displayRotation = transform.localRotation.eulerAngles.y;
        StartingSize = Size;
        height = Colliders[0].bounds.max.y;
        LastValidPosition = DisplayPosition;
        LastValidRotation = DisplayRotation;
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
        LastValidPosition = DisplayPosition;
        LastValidRotation = DisplayRotation;
        // TODO: Change the type of sfx played
        //SFXManager.Instance.PlaySFX(SFXType.Place_Wood);
        GridSystem.Instance.heightGrid.Set(GetLastValidBoundingBox(), height);
        WinCondition.Instance.UpdateRuleCheck();
        // SFXManager.Instance.PlaySFX(SFXType.Place_Wood);
        ColliderOn();
    }

    public void ResetToValidLocation()
    {
        DisplayPosition = LastValidPosition;
        DisplayRotation = LastValidRotation;
        ColliderOn();
    }

    public void ColliderOff()
    {
        SetColliderEnabled(false);
        GridSystem.Instance.placedFurnitures.Set(GetBoundingBox(), null);
    }

    public void ColliderOn()
    {
        SetColliderEnabled(true);
        GridSystem.Instance.placedFurnitures.Set(GetBoundingBox(), this);
        WinCondition.Instance.UpdateRuleCheck();
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
        foreach (SerializableTuple<MeshRenderer, Material> tuple in MeshRenderers)
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

            if (SFXManager.Instance != null)
            SFXManager.Instance.PlayFurnitureSFX(sfxCategory, SFXAction.Place);


            // Only MFurn can attach to WFurn
            if (canStackOnOthers && lastStackCandidate != null)
            {
                AttachToBase(lastStackCandidate);
            }
            else
            {
                DetachFromBase();
            }
        }
        else
        {
            ResetToValidLocation();
            if (SFXManager.Instance != null)
            SFXManager.Instance.PlayFurnitureSFX(sfxCategory, SFXAction.Invalid);
        }
    }


    public void SetNormalMat()
    {
        foreach (SerializableTuple<MeshRenderer, Material> tuple in MeshRenderers)
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
        // bool canStack = CompareTag("MFurn");

        Furniture bestWFurnFurniture = null;
        float bestWFurnTopY = float.NegativeInfinity;

        bool sawFloor = false;
        float bestFloorY = float.NegativeInfinity;

        for (int i = 0; i < ShapeUnits.childCount; i++)
        {
            Vector3 origin = ShapeUnits.GetChild(i).position;

            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 100f, ~0, QueryTriggerInteraction.Ignore);
            if (hits == null || hits.Length == 0)
            // raycast at shapeUnit
            // if (
            //     !Physics.Raycast(
            //         ShapeUnits.GetChild(i).position,
            //         Vector3.down,
            //         out RaycastHit hit,
            //         100f
            //     ) || !hit.collider.CompareTag("Floor")
            // )
            {
                lastStackCandidate = null;
                return false;
            }

            RaycastHit? best = null;
            float bestDist = float.PositiveInfinity;

            foreach (var h in hits)
            {
                if (h.collider == null) continue;
                if (h.collider.transform.IsChildOf(transform)) continue;

                if (h.distance < bestDist)
                {
                    bestDist = h.distance;
                    best = h;
                }
            }

            if (best == null)
            {
                lastStackCandidate = null;
                return false;
            }

            RaycastHit hit = best.Value;

            bool isFloor = hit.collider.CompareTag("Floor");
            bool hitCanBeStackedOn = false;
            Furniture hitFurnitureComponent = null;
            if(hit.collider.transform.parent != null)
            {
                if(hit.collider.transform.parent.TryGetComponent(out hitFurnitureComponent)) {
                    hitCanBeStackedOn = hitFurnitureComponent.canBeStackedOn;
                }
            } 

            if (!canStackOnOthers)
            {
                if (!isFloor)
                {
                    lastStackCandidate = null;
                    return false;
                }
            }
            else
            {
                if (!isFloor && !hitCanBeStackedOn)
                {
                    lastStackCandidate = null;
                    return false;
                }
            }

            if (isFloor)
            {
                sawFloor = true;
                bestFloorY = Mathf.Max(bestFloorY, hit.point.y);
            }

            if (canStackOnOthers && hitCanBeStackedOn)
            {
                if (hitFurnitureComponent != null)
                {
                    float topY = hit.collider.bounds.max.y;
                    if (topY > bestWFurnTopY)
                    {
                        bestWFurnTopY = topY;
                        bestWFurnFurniture = hitFurnitureComponent;
                    }
                }
            }
        }

        lastStackCandidate = canStackOnOthers ? bestWFurnFurniture : null;

        if (canStackOnOthers && bestWFurnFurniture != null)
        {
            transform.position = new Vector3(transform.position.x, bestWFurnTopY, transform.position.z);
        }
        else if (sawFloor)
        {
            transform.position = new Vector3(transform.position.x, bestFloorY, transform.position.z);
        }

        return true;
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

    private void SnapVerticalToSurface(RaycastHit hit)
    {
        float newY = transform.position.y;

        if (hit.collider.CompareTag("Floor"))
        {
            return;
        }

        if (hit.collider.CompareTag("WFurn"))
        {
            float topY = hit.collider.bounds.max.y;

            float myBottomOffset = GetMyBottomOffset();
            newY = topY + myBottomOffset;
        }

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private float GetMyBottomOffset()
    {
        if (Colliders != null && Colliders.Length > 0 && Colliders[0] != null)
        {
            return 0f; 
        }
        return 0f;
    }

    private void AttachToBase(Furniture baseFurniture)
    {
        if (baseFurniture == null) return;
        if (_stackBase == baseFurniture) return;

        // Keep world position/rotation when parenting
        Transform oldParent = transform.parent;
        transform.SetParent(baseFurniture.transform, true);

        _stackBase = baseFurniture;
    }

    private void DetachFromBase()
    {
        if (_stackBase == null) return;

        transform.SetParent(null, true);
        _stackBase = null;
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
