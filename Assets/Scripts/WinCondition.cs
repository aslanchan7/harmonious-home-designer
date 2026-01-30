using UnityEngine;

public class WinCondition : MonoBehaviour
{
    public static WinCondition Instance;

    public Furniture Bed;
    public Furniture Door;
    public BoundingBox DoorBox;

    [Header("Checks")]
    public bool bedIsFacingDoor;
    public bool bedIsAccessible;
    public bool doorIsObstructed;

    public bool DoorIsObstructed()
    {
        doorIsObstructed =
                !Physics.Raycast(
                    new(
                        DoorBox.x + 0.5f,
                        10f,
                        DoorBox.y + 0.5f
                    ),
                    Vector3.down,
                    out RaycastHit hit,
                    100f
                ) || !hit.collider.CompareTag("Floor");
        return doorIsObstructed;
    }

    public bool BedIsFacingDoor()
    {
        Direction bedFoot = Bed.GetRotatedFace(Direction.DOWN);
        Direction doorInsideFace = Door.GetRotatedFace(Direction.DOWN);
        BoundingBox bedBox = Bed.GetLastValidBoundingBox();
        BoundingBox doorBox = Door.GetLastValidBoundingBox();

        bedIsFacingDoor = bedFoot.IsOppositeTo(doorInsideFace)
                && bedBox.ToFaceOf(doorBox, doorInsideFace)
                && GridSystem.Instance.heightGrid.ExistsLineSatisfiesAll(
                    GridSystem.Instance.heightGrid.heightValues,
                    bedBox.IntersectionOrGapWith(doorBox),
                    (float height) => height < Bed.height
                );
        return bedIsFacingDoor;
    }

    public bool BedIsAccessible()
    {
        if (doorIsObstructed)
            return false;

        HeightGrid.DijkstraCell[,] dijkstraCells
                = GridSystem.Instance.heightGrid.Dijkstra(DoorBox, 0);
        BoundingBox bedBox = Bed.GetLastValidBoundingBox();
        BoundingBox bedLeft = bedBox.GetNextToFace(
            Bed.GetRotatedFace(Direction.LEFT),
            1
        );
        BoundingBox bedRight = bedBox.GetNextToFace(
            Bed.GetRotatedFace(Direction.RIGHT),
            1
        );
        bedLeft.Clamp();
        bedRight.Clamp();
        bedIsAccessible = !GridSystem.Instance.heightGrid.SatisfiesAll(
            dijkstraCells,
            bedLeft,
            (cell) => cell.distance == float.PositiveInfinity
        ) || !GridSystem.Instance.heightGrid.SatisfiesAll(
            dijkstraCells,
            bedRight,
            (cell) => cell.distance == float.PositiveInfinity
        );
        return bedIsAccessible;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }

        Instance = this;
    }

    public void UpdateRuleCheck()
    {
        Vector2 doorPosition = Door.LastValidPosition;
        Vector2 doorInsideFace = Door.GetRotatedFace(Direction.DOWN).ToVector();
        DoorBox = BoundingBox.FromCenterAndSize(
            doorPosition + doorInsideFace / 2,
            new(1, 1)
        );
        DoorIsObstructed();
        BedIsFacingDoor();
        BedIsAccessible();
    }
}
