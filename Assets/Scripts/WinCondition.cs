using System;
using System.Collections.Generic;
using UnityEngine;

public class WinCondition : MonoBehaviour
{
    public static WinCondition Instance;

    public FSBarController BarController;

    private Zone MainDoor;
    private Zone BathroomDoor;
    private Zone Bathroom;
    private Zone Window;
    private Zone[] Walls;

    public HashSet<string> requiredFurnitures = new HashSet<string>();
    public Dictionary<string, Furniture> furnitures =
        new Dictionary<string, Furniture>();
    public Dictionary<string, List<Rule>> associatedRules =
        new Dictionary<string, List<Rule>>();
    public PlacedFurnitures.DijkstraCell[,] dijkstraCells;
    public List<Rule> ruleBook;

    private int maxPoints = 0;

    public Func<bool> DoorIsClear(Zone door)
    {
        return () =>
        {
            PlacedFurnitures.Instance.BoundingBoxToIndices(
                door.Box,
                out Vector2Int starting,
                out _
            );
            return PlacedFurnitures.Instance.GetBase(starting) == null;
        };
    }

    public bool BedIsFacingDoor()
    {
        Furniture bed = furnitures["Bed"];
        Direction bedFoot = bed.GetFace(Direction.DOWN);
        Direction doorInsideFace = MainDoor.GetFace(Direction.DOWN);
        BoundingBox bedBox = bed.GetBoundingBox();
        BoundingBox doorBoxFlat = MainDoor.GetNextToFace(Direction.UP, 0);

        return bedFoot.IsOppositeTo(doorInsideFace)
            && bedBox.InLineWithFaceOf(doorBoxFlat, doorInsideFace)
            && bedBox.ToFaceOf(doorBoxFlat, doorInsideFace)
            && PlacedFurnitures.Instance.ExistsLineSatisfiesAll(
                PlacedFurnitures.Instance.furnitureBaseGrid,
                bedBox.IntersectionOrGapWith(doorBoxFlat),
                (Furniture furniture) =>
                    furniture == null || furniture.height < bed.height
            );
    }

    public bool BedHeadIsFacingBathroom()
    {
        Furniture bed = furnitures["Bed"];
        BoundingBox doorBoxFlat = BathroomDoor.GetNextToFace(Direction.UP, 0);
        Direction doorInsideFace = BathroomDoor.GetFace(Direction.DOWN);
        BoundingBox bedBox = bed.GetBoundingBox();
        Direction bedHead = bed.GetFace(Direction.UP);
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
        Direction insideFace = zone.GetFace(zoneFace);
        BoundingBox zoneBoxFlat = zone.GetNextToFace(zoneFace.ToOpposite(), 0);
        return furnitureRotatedFace.IsOppositeTo(insideFace)
            && furnitureBox.InLineWithFaceOf(zoneBoxFlat, insideFace)
            && furnitureBox.TouchesToFaceOf(zoneBoxFlat, insideFace);
    }

    public bool BedIsAgainstSolidWall()
    {
        Furniture bed = furnitures["Bed"];
        BoundingBox bedBox = bed.GetBoundingBox();
        Direction bedHead = bed.GetFace(Direction.UP);
        bool isAgainst = false;
        foreach (Zone wall in Walls)
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

    public Func<bool> FurnitureIsInZone(String furniture, Zone zone)
    {
        return () =>
        {
            return furnitures[furniture].GetBoundingBox().Intersects(zone.Box);
        };
    }

    public bool BedIsAccessible()
    {
        if (dijkstraCells == null)
            return false;
        Furniture bed = furnitures["Bed"];
        BoundingBox bedBox = bed.GetBoundingBox();
        BoundingBox bedLeft = bedBox
            .GetNextToFace(bed.GetFace(Direction.LEFT), 1)
            .Clamp();
        BoundingBox bedRight = bedBox
            .GetNextToFace(bed.GetFace(Direction.RIGHT), 1)
            .Clamp();
        return !PlacedFurnitures.Instance.SatisfiesAll(
                dijkstraCells,
                bedLeft,
                (cell) => cell.distance == float.PositiveInfinity
            )
            || !PlacedFurnitures.Instance.SatisfiesAll(
                dijkstraCells,
                bedRight,
                (cell) => cell.distance == float.PositiveInfinity
            );
    }

    public bool CommandPosition()
    {
        Furniture bed = furnitures["Bed"];
        Vector2 bedHeadPosition = bed.GetNextToFace(Direction.UP, -1).center;
        Vector2 bedFootAngle = bed.GetFace(Direction.DOWN).ToVector();
        Vector2 doorFacePosition = MainDoor
            .GetNextToFace(Direction.UP, 0)
            .center;
        Vector3 doorEyeLevel = new(doorFacePosition.x, 2, doorFacePosition.y);
        Vector2 doorToBedHead = bedHeadPosition - doorFacePosition;
        float distance = Vector2.Distance(doorFacePosition, bedHeadPosition);
        Ray ray = new(doorEyeLevel, new(doorToBedHead.x, 0, doorToBedHead.y));

        return !(
            Vector2.Angle(-doorToBedHead, bedFootAngle) >= 60
            || Physics.Raycast(ray, out RaycastHit hit, distance)
                && hit.collider.CompareTag("Furniture")
        );
    }

    public Func<bool> FurnitureIsPresent(string furniture)
    {
        return () => furnitures.ContainsKey(furniture);
    }

    public Func<bool> Invert(Func<bool> ruleCheck)
    {
        return () => !ruleCheck();
    }

    public void TryPathfindFrom(Zone door)
    {
        dijkstraCells = DoorIsClear(door)()
            ? PlacedFurnitures.Instance.Dijkstra(door.Box, 0)
            : null;
    }

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }

        Instance = this;
    }

    public void Start()
    {
        MainDoor = new Zone(new(2, -2.5f), new(1, 1), 180);
        BathroomDoor = new Zone(new(-3, -1.5f), new(1, 1), -90);
        Bathroom = new Zone(new(-2.5f, 0), new(2, 6), 0);
        Window = new Zone(new(2.5f, 1.5f), new(2, 3), 0);
        Vector2Int roomSize = GridSystem.Instance.Size;
        Walls = new Zone[]
        {
            new Zone(new(0, roomSize.y / 2.0f - 0.5f), new(roomSize.x, 1), 180),
            new Zone(new(roomSize.x / 2.0f - 0.5f, 0), new(1, roomSize.y), -90),
            new Zone(new(0, -roomSize.y / 2.0f + 0.5f), new(roomSize.x, 1), 0),
            new Zone(new(-roomSize.x / 2.0f + 0.5f, 0), new(1, roomSize.y), 90),
        };

        RegisterRule(
            new Rule(
                "Bed is present",
                FurnitureIsPresent("Bed"),
                50,
                FSEnergyType.Functional,
                new string[0]
            )
        );
        RegisterRule(
            new Rule(
                "Main door is not obstructed",
                DoorIsClear(MainDoor),
                50,
                FSEnergyType.Functional,
                new string[0]
            )
        );
        RegisterRule(
            new Rule(
                "Bathroom door is not obstructed",
                DoorIsClear(BathroomDoor),
                50,
                FSEnergyType.Functional,
                new string[0]
            )
        );
        RegisterRule(
            new Rule(
                "Bed is not facing main door",
                Invert(BedIsFacingDoor),
                10,
                FSEnergyType.Functional,
                new String[] { "Bed" }
            )
        );
        RegisterRule(
            new Rule(
                "Bed's head is not facing the bathroom",
                Invert(BedHeadIsFacingBathroom),
                10,
                FSEnergyType.Functional,
                new String[] { "Bed" }
            )
        );
        RegisterRule(
            new Rule(
                "Bed is not near bathroom",
                Invert(FurnitureIsInZone("Bed", Bathroom)),
                10,
                FSEnergyType.Chaos,
                new String[] { "Bed" }
            )
        );
        RegisterRule(
            new Rule(
                "Bed is not near window",
                Invert(FurnitureIsInZone("Bed", Window)),
                10,
                FSEnergyType.Chaos,
                new String[] { "Bed" }
            )
        );
        RegisterRule(
            new Rule(
                "Bed is accessible",
                BedIsAccessible,
                50,
                FSEnergyType.Functional,
                new String[] { "Bed" }
            )
        );
        RegisterRule(
            new Rule(
                "Bed is against solid wall",
                BedIsAgainstSolidWall,
                20,
                FSEnergyType.Functional,
                new String[] { "Bed" }
            )
        );
        RegisterRule(
            new Rule(
                "Bed is command position",
                CommandPosition,
                40,
                FSEnergyType.Functional,
                new String[] { "Bed" }
            )
        );

        UpdateRuleCheck();
    }

    public void AddFurnitureIfRequired(Furniture furniture)
    {
        if (
            requiredFurnitures.Contains(furniture.furnitureName)
            && !furnitures.ContainsKey(furniture.furnitureName)
        )
        {
            furnitures.Add(furniture.furnitureName, furniture);
        }
    }

    public void RemoveFurnitureIfRegistered(Furniture furniture)
    {
        if (
            requiredFurnitures.Contains(furniture.furnitureName)
            && furnitures.ContainsKey(furniture.furnitureName)
            && furnitures[furniture.furnitureName] == furniture
        )
        {
            furnitures.Remove(furniture.furnitureName);
        }
    }

    private void RegisterRule(Rule rule)
    {
        maxPoints += rule.points;
        ruleBook.Add(rule);
        foreach (string furnitureName in rule.furnitureDependencies)
        {
            if (!associatedRules.ContainsKey(furnitureName))
            {
                associatedRules.Add(furnitureName, new List<Rule>());
            }
            associatedRules[furnitureName].Add(rule);
            requiredFurnitures.Add(furnitureName);
        }

        BarController.AddEnergy(rule.energyType, rule.points, false);
    }

    public void UpdateRuleCheck()
    {
        TryPathfindFrom(MainDoor);

        foreach (Rule rule in ruleBook)
        {
            bool allDependenciesSatisfy = true;
            foreach (string dependency in rule.furnitureDependencies)
            {
                if (!furnitures.ContainsKey(dependency))
                {
                    allDependenciesSatisfy = false;
                    break;
                }
            }

            if (!allDependenciesSatisfy)
                continue;

            switch (rule.Check())
            {
                case Rule.StateChange.FALSE_TO_TRUE:
                    BarController.RemoveEnergy(
                        rule.energyType,
                        rule.points,
                        false
                    );
                    break;
                case Rule.StateChange.TRUE_TO_FALSE:
                    BarController.AddEnergy(
                        rule.energyType,
                        rule.points,
                        false
                    );
                    break;
            }
        }
    }
}
