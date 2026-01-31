using System;
using System.Data;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class WinCondition : MonoBehaviour
{
    public static WinCondition Instance;
    private bool doneInitialCheck;

    public Furniture Bed;
    public FSBarController BarController; 

    private Zone MainDoor;
    private Zone BathroomDoor;
    private Zone Bathroom;
    private Zone Window;
    private Zone[] walls;

    public HeightGrid.DijkstraCell[,] dijkstraCells;

    [Header("Checks")]
    public bool bedIsFacingDoor;
    public bool bedIsAccessible;
    public bool mainDoorIsObstructed;
    public bool bathroomDoorIsObstructed;
    public bool bedHeadIsFacingBathroom;
    public bool bedIsNearBathroom;
    public bool bedIsNearWindow;
    public bool bedIsAgainstSolidWall;

    public bool DoorIsObstructed(Zone door)
    {
        return !Physics.Raycast(
            new(
                door.boundingBox.x + 0.5f,
                10f,
                door.boundingBox.y + 0.5f
            ),
            Vector3.down,
            out RaycastHit hit,
            100f
        ) || !hit.collider.CompareTag("Floor");
    }

    public bool BedIsFacingDoor()
    {
        Direction bedFoot = Bed.GetRotatedFace(Direction.DOWN);
        Direction doorInsideFace = MainDoor.GetRotatedFace(Direction.DOWN);
        BoundingBox bedBox = Bed.GetLastValidBoundingBox();
        BoundingBox doorBoxFlat = MainDoor.boundingBox.GetNextToFace(
            MainDoor.GetRotatedFace(Direction.UP), 0
        );

        return bedFoot.IsOppositeTo(doorInsideFace)
                && bedBox.InLineWithFaceOf(doorBoxFlat, doorInsideFace)
                && bedBox.ToFaceOf(doorBoxFlat, doorInsideFace)
                && GridSystem.Instance.heightGrid.ExistsLineSatisfiesAll(
                   GridSystem.Instance.heightGrid.heightValues,
                    bedBox.IntersectionOrGapWith(doorBoxFlat),
                    (float height) => height < Bed.height
                );
    }

    public bool BedHeadIsFacingBathroom()
    {
        BoundingBox doorBoxFlat = BathroomDoor.boundingBox.GetNextToFace(
            BathroomDoor.GetRotatedFace(Direction.UP), 0
        );
        Direction doorInsideFace = BathroomDoor.GetRotatedFace(Direction.DOWN);
        BoundingBox bedBox = Bed.GetLastValidBoundingBox();
        Direction bedHead = Bed.GetRotatedFace(Direction.UP);
        return bedHead.IsOppositeTo(doorInsideFace)
               && bedBox.ToFaceOf(doorBoxFlat, doorInsideFace);
    }

    private bool IsAgainst(
        BoundingBox furnitureBox,
        Direction furnitureRotatedFace,
        Zone zone,
        Direction zoneFace
    )
    {
        Direction insideFace = zone.GetRotatedFace(zoneFace);
        BoundingBox zoneBoxFlat = zone.boundingBox.GetNextToFace(
            insideFace.ToOpposite(),
            0
        );
        return furnitureRotatedFace.IsOppositeTo(insideFace)
                && furnitureBox.InLineWithFaceOf(zoneBoxFlat, insideFace)
                && furnitureBox.TouchesToFaceOf(zoneBoxFlat, insideFace);
    }

    public bool BedIsAgainstSolidWall()
    {
        BoundingBox bedBox = Bed.GetLastValidBoundingBox();
        Direction bedHead = Bed.GetRotatedFace(Direction.UP);
        bool isAgainst = false;
        foreach (Zone wall in walls)
        {
            if (IsAgainst(bedBox, bedHead, wall, Direction.UP))
            {
                isAgainst = true;
                break;
            }
        }
        return isAgainst
                && !IsAgainst(bedBox, bedHead, Window, Direction.LEFT)
                && !IsAgainst(bedBox, bedHead, MainDoor, Direction.DOWN)
                && !IsAgainst(bedBox, bedHead, BathroomDoor, Direction.DOWN);
    }

    public bool FurnitureIsInZone(Furniture furniture, Zone zone)
    {
        return furniture.GetLastValidBoundingBox().Intersects(zone.boundingBox);
    }

    public bool BedIsAccessible()
    {
        if (mainDoorIsObstructed)
            return false;

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
        return !GridSystem.Instance.heightGrid.SatisfiesAll(
            dijkstraCells,
            bedLeft,
            (cell) => cell.distance == float.PositiveInfinity
        ) || !GridSystem.Instance.heightGrid.SatisfiesAll(
            dijkstraCells,
            bedRight,
            (cell) => cell.distance == float.PositiveInfinity
        );
    }

    public void TryPathfindFrom(Zone door)
    {
        if (DoorIsObstructed(door))
            return;
        dijkstraCells = GridSystem.Instance.heightGrid.Dijkstra(
            door.boundingBox, 0
        );
    }

    public bool InitializeEnergy(
        bool ruleState,
        bool ruleFlipped,
        FSEnergyType energyType,
        int amount,
        bool polarity
    ) {
        if (ruleFlipped ^ ruleState)
        {
            BarController.AddEnergy(energyType, amount, polarity);
        }
        return ruleState;
    }

    public bool AdjustEnergy(
        bool initialRuleState,
        bool newRuleState,
        bool ruleFlipped,
        FSEnergyType energyType,
        int amount,
        bool polarity
    )
    {
        if (ruleFlipped)
        {
            if (newRuleState && !initialRuleState)
            {
                BarController.RemoveEnergy(energyType, amount, polarity);
            }
            if (!newRuleState && initialRuleState)
            {
                BarController.AddEnergy(energyType, amount, polarity);
            }
        }
        else
        {
            if (newRuleState && !initialRuleState)
            {
                BarController.AddEnergy(energyType, amount, polarity);
            }
            if (!newRuleState && initialRuleState)
            {
                BarController.RemoveEnergy(energyType, amount, polarity);
            }
        }
        return newRuleState;
    }

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }

        Instance = this;
        doneInitialCheck = false;
    }

    public void Start()
    {
        MainDoor = new Zone(
            new (2, -2.5f),
            new (1, 1),
            -180
        );
        BathroomDoor = new Zone(
            new (-3, -1.5f),
            new (1, 1),
            -90
        );
        Bathroom = new Zone(
            new (-2.5f, 0),
            new (2, 6),
            0
        );
        Window = new Zone(
            new (2.5f, 1.5f),
            new (2, 3),
            0
        );
        Vector2Int roomSize = GridSystem.Instance.Size;
        walls = new Zone[] {
            new Zone(
                new (0, roomSize.y / 2.0f - 0.5f),
                new (roomSize.x, 1),
                180
            ),
            new Zone(
                new (roomSize.x / 2.0f - 0.5f, 0),
                new (1, roomSize.y),
                -90
            ),
            new Zone(
                new (0, -roomSize.y / 2.0f + 0.5f),
                new (roomSize.x, 1),
                0
            ),
            new Zone(
                new (-roomSize.x / 2.0f + 0.5f, 0),
                new (1, roomSize.y),
                90
            )
        };
        UpdateRuleCheck();
    }

    public void UpdateRuleCheck()
    {
        try {
            if (!doneInitialCheck)
            {
                InitialRuleCheck();
                return;
            }
            TryPathfindFrom(MainDoor);
            
            mainDoorIsObstructed = AdjustEnergy(
                mainDoorIsObstructed,
                DoorIsObstructed(MainDoor),
                true,
                FSEnergyType.Luck,
                50,
                true
            );
            bathroomDoorIsObstructed = AdjustEnergy(
                bathroomDoorIsObstructed,
                DoorIsObstructed(BathroomDoor),
                true,
                FSEnergyType.Luck,
                50,
                true
            );
            bedIsFacingDoor = AdjustEnergy(
                bedIsFacingDoor,
                BedIsFacingDoor(),
                true,
                FSEnergyType.Luck,
                10,
                true
            );
            bedHeadIsFacingBathroom = AdjustEnergy(
                bedHeadIsFacingBathroom,
                BedHeadIsFacingBathroom(),
                true,
                FSEnergyType.Luck,
                10,
                true
            );
            bedIsNearBathroom = AdjustEnergy(
                bedIsNearBathroom,
                FurnitureIsInZone(Bed, Bathroom),
                false,
                FSEnergyType.Chaos,
                10,
                false
            );
            bedIsNearWindow = AdjustEnergy(
                bedIsNearWindow,
                FurnitureIsInZone(Bed, Window),
                false,
                FSEnergyType.Chaos,
                10,
                false
            );
            bedIsAccessible = AdjustEnergy(
                bedIsAccessible,
                BedIsAccessible(),
                false,
                FSEnergyType.Luck,
                50,
                true
            );
            bedIsAgainstSolidWall = AdjustEnergy(
                bedIsAgainstSolidWall,
                BedIsAccessible(),
                false,
                FSEnergyType.Luck,
                20,
                true
            );
        } catch (NullReferenceException _)
        {
            
        }
    }

    public void InitialRuleCheck()
    {
        try {
            TryPathfindFrom(MainDoor);
            

            mainDoorIsObstructed = InitializeEnergy(
                DoorIsObstructed(MainDoor),
                true,
                FSEnergyType.Luck,
                50,
                true
            );
            bathroomDoorIsObstructed = InitializeEnergy(
                DoorIsObstructed(BathroomDoor),
                true,
                FSEnergyType.Luck,
                50,
                true
            );
            bedIsFacingDoor = InitializeEnergy(
                BedIsFacingDoor(),
                true,
                FSEnergyType.Luck,
                10,
                true
            );
            bedHeadIsFacingBathroom = InitializeEnergy(
                BedHeadIsFacingBathroom(),
                true,
                FSEnergyType.Luck,
                10,
                true
            );
            bedIsNearBathroom = InitializeEnergy(
                FurnitureIsInZone(Bed, Bathroom),
                false,
                FSEnergyType.Chaos,
                10,
                false
            );
            bedIsNearWindow = InitializeEnergy(
                FurnitureIsInZone(Bed, Window),
                false,
                FSEnergyType.Chaos,
                10,
                false
            );
            bedIsAccessible = InitializeEnergy(
                BedIsAccessible(),
                false,
                FSEnergyType.Luck,
                50,
                true
            );
            bedIsAgainstSolidWall = InitializeEnergy(
                BedIsAccessible(),
                false,
                FSEnergyType.Luck,
                20,
                true
            );
            doneInitialCheck = true;
        } catch (NullReferenceException _)
        {
            
        }
    }
}
 