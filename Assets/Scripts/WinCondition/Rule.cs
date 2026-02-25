using System;
using System.Collections.Generic;
using Bagua;
using UnityEngine;

[Serializable]
public class Rule
{
    public string name;
    public RuleFunction ruleFunction;
    public float basePoints;
    public Precondition precondition;
    public Action Check;

    public enum RuleType
    {
        RequiredArePresent,
        AtLeastOnePresent,
        UnacceptableAreNotPresent,
        DoorsAreClear,
        Accessible,
        BedIsNotFacingAnyDoor,
        BedIsNotNearAndTowardsBathroom,
        BedHeadIsAgainstWall,
        FurnituresInChaosZoneWithoutPeace,
        CommandPosition,
        BaguaBonus,
        StarWhenBonusExceed,
    }

    public enum ArgumentType
    {
        Furniture,
        FixedItem,
        Zone,
        Direction,
        Number,
    }

    [Flags]
    public enum Precondition
    {
        AtLeastOneRequiredFurniturePresent = 1,
        FirstStar = 1 << 1,
    }

    [Serializable]
    public struct RuleArgument
    {
        public string name;
        public ArgumentType type;
    }

    [Serializable]
    public struct RuleFunction
    {
        public RuleType ruleType;
        public RuleArgument[] arguments;
    }

    public Action RequiredArePresent()
    {
        return Present("Required");
    }

    public Action UnacceptableAreNotPresent()
    {
        return NotPresent("Unacceptable");
    }

    public Action DoorsAreClear()
    {
        return Clear(
            Functional.Map(
                WinCondition.Instance.GetFixedItems("Door"),
                (door) => door.GetNextToRelativeFace(Direction.UP, 1)
            )
        );
    }

    public Action Accessible()
    {
        List<string> furnitureNames = WinCondition
            .Instance
            .ruleSet
            .furnitureDict["Accessible"];
        WinCondition.Instance.AddMaxPoints(
            FSEnergyType.Functional,
            furnitureNames.Count * basePoints
        );
        return () =>
        {
            foreach (string furniture in furnitureNames)
            {
                List<Furniture> furnitureList =
                    WinCondition.Instance.GetFurnitures(furniture);
                if (furnitureList.Count == 0)
                    continue;
                if (furnitureList[0].requireAllDirectionsAccessible)
                {
                    InvertGranularRule(
                        furnitureList,
                        (furniture) =>
                            Functional.All(
                                furniture.accessibleDirections,
                                (direction) =>
                                {
                                    BoundingBox nextTo = furniture
                                        .GetNextToRelativeFace(direction, 1)
                                        .Clamp();
                                    return !nextTo.IsFlat()
                                        && !Functional.SatisfiesAll(
                                            WinCondition.Instance.dijkstraCells,
                                            nextTo,
                                            cell =>
                                                cell.distance
                                                == float.PositiveInfinity
                                        );
                                }
                            ),
                        FSEnergyType.Functional,
                        basePoints
                    );
                }
                else
                {
                    InvertGranularRule(
                        furnitureList,
                        (furniture) =>
                            Functional.Any(
                                furniture.accessibleDirections,
                                (direction) =>
                                {
                                    BoundingBox nextTo = furniture
                                        .GetNextToRelativeFace(direction, 1)
                                        .Clamp();
                                    return !nextTo.IsFlat()
                                        && !Functional.SatisfiesAll(
                                            WinCondition.Instance.dijkstraCells,
                                            nextTo,
                                            cell =>
                                                cell.distance
                                                == float.PositiveInfinity
                                        );
                                }
                            ),
                        FSEnergyType.Functional,
                        basePoints
                    );
                }
            }
        };
    }

    public Action BedIsNotFacingAnyDoor()
    {
        List<DirectedBox> doors = WinCondition.Instance.GetFixedItems("Door");
        WinCondition.Instance.AddMaxPoints(FSEnergyType.Functional, basePoints);
        return () =>
        {
            List<Furniture> bedList = WinCondition.Instance.GetFurnitures(
                "Bed"
            );
            GranularRule(
                bedList,
                (bed) =>
                    Functional.Any(
                        doors,
                        (door) =>
                        {
                            Direction bedFoot = bed.GetRelativeFace(
                                Direction.DOWN
                            );
                            Direction doorInsideFace = door.GetRelativeFace(
                                Direction.UP
                            );
                            BoundingBox bedBox = bed.GetBoundingBox();

                            return bedFoot.IsOppositeTo(doorInsideFace)
                                && bedBox.InLineWithFaceOf(door, doorInsideFace)
                                && bedBox.ToFaceOf(door, doorInsideFace)
                                && Functional.ExistsLineSatisfiesAll(
                                    PlacedFurnitures.Instance.furnitureBaseGrid,
                                    bedBox.IntersectionOrGapWith(door),
                                    (Furniture furniture) =>
                                        furniture == null
                                        || furniture.height < bed.height
                                );
                        }
                    ),
                FSEnergyType.Rule,
                basePoints
            );
        };
    }

    public Action BedIsNotNearAndTowardsBathroom()
    {
        BoundingBox bathroomPromixity = WinCondition.Instance.GetZones(
            "Bathroom Proximity"
        )[0];
        DirectedBox bathroomDoor = WinCondition.Instance.GetFixedItems(
            "Bathroom Door"
        )[0];
        WinCondition.Instance.AddMaxPoints(FSEnergyType.Functional, basePoints);
        return () =>
        {
            List<Furniture> bedList = WinCondition.Instance.GetFurnitures(
                "Bed"
            );
            GranularRule(
                bedList,
                (bed) =>
                    bathroomPromixity.Intersects(bed.GetBoundingBox())
                    && bed.GetRelativeFace(Direction.UP)
                        .IsOppositeTo(
                            bathroomDoor.GetRelativeFace(Direction.UP)
                        ),
                FSEnergyType.Toilet,
                basePoints
            );
        };
    }

    public Action BedHeadIsAgainstWall()
    {
        List<DirectedBox> walls = WinCondition.Instance.GetFixedItems("Wall");
        WinCondition.Instance.AddMaxPoints(FSEnergyType.Functional, basePoints);
        return () =>
        {
            List<Furniture> bedList = WinCondition.Instance.GetFurnitures(
                "Bed"
            );
            InvertGranularRule(
                bedList,
                (bed) =>
                    Functional.Any(
                        walls,
                        (wall) =>
                        {
                            Direction insideFace = wall.GetRelativeFace(
                                Direction.UP
                            );
                            BoundingBox bedBox = bed.GetBoundingBox();
                            return bed.GetRelativeFace(Direction.UP)
                                    .IsOppositeTo(insideFace)
                                && bedBox.InLineWithFaceOf(wall, insideFace)
                                && bedBox.TouchesToFaceOf(wall, insideFace);
                        }
                    ),
                FSEnergyType.Rule,
                basePoints
            );
        };
    }

    public Action CommandPosition()
    {
        DirectedBox door = WinCondition.Instance.GetFixedItems("Main Door")[0];
        WinCondition.Instance.AddMaxPoints(FSEnergyType.Functional, basePoints);
        return () =>
        {
            List<Furniture> bedList = WinCondition.Instance.GetFurnitures(
                "Bed"
            );
            GranularRule(
                bedList,
                (bed) =>
                {
                    Vector2 bedHeadPosition = bed.GetNextToRelativeFace(
                        Direction.UP,
                        -1
                    ).center;
                    Vector2 bedFootAngle = bed.GetRelativeFace(Direction.DOWN)
                        .ToVector();
                    Vector2 doorFacePosition = door.center;
                    Vector3 doorEyeLevel = new(
                        doorFacePosition.x,
                        1.5f,
                        doorFacePosition.y
                    );
                    Vector2 doorToBedHead = bedHeadPosition - doorFacePosition;
                    float distance = Vector2.Distance(
                        doorFacePosition,
                        bedHeadPosition
                    );
                    Ray ray = new(
                        doorEyeLevel,
                        new(doorToBedHead.x, 0, doorToBedHead.y)
                    );

                    return Vector2.Angle(-doorToBedHead, bedFootAngle) >= 60
                        || Physics.Raycast(ray, out RaycastHit hit, distance)
                            && hit.collider.CompareTag("Furniture");
                },
                FSEnergyType.Rule,
                basePoints
            );
        };
    }

    public Action FurnituresInChaosZoneWithoutPeace()
    {
        List<BoundingBox> chaosZones = WinCondition.Instance.GetZones("Chaos");
        float maxPoints = chaosZones.Count * basePoints;
        WinCondition.Instance.AddMaxPoints(FSEnergyType.Chaos, maxPoints);
        return () =>
        {
            GranularRule(
                chaosZones,
                (zone) =>
                {
                    bool hasFurnitureInZone = false;
                    GridSystem.Instance.WorldBoxToIndices(
                        zone,
                        out Vector2Int starting,
                        out Vector2Int ending
                    );

                    for (int j = starting.x; j < ending.x; j++)
                    {
                        for (int i = starting.y; i < ending.y; i++)
                        {
                            Vector2Int index = new(j, i);
                            Furniture baseFurniture =
                                PlacedFurnitures.Instance.GetBase(index);
                            Furniture stackFurniture =
                                PlacedFurnitures.Instance.GetStack(index);

                            if (baseFurniture == null)
                                continue;

                            if (
                                baseFurniture.energy.HasFlag(Energy.Peace)
                                || stackFurniture != null
                                    && stackFurniture.energy.HasFlag(
                                        Energy.Peace
                                    )
                            )
                                return false;

                            if (
                                !baseFurniture.energy.HasFlag(
                                    Energy.UnaffectedByChaos
                                )
                                && (
                                    stackFurniture == null
                                    || stackFurniture.energy.HasFlag(
                                        Energy.UnaffectedByChaos
                                    )
                                )
                            )
                            {
                                hasFurnitureInZone = true;
                            }
                        }
                    }

                    return hasFurnitureInZone;
                },
                FSEnergyType.Chaos,
                maxPoints
            );
        };
    }

    public Action BaguaBonus()
    {
        return () =>
        {
            HashSet<Furniture> encounteredFurnitures = new();
            foreach (LifeArea lifeArea in Enum.GetValues(typeof(LifeArea)))
            {
                Element element = lifeArea.GetElement();
                BoundingBox zone = WinCondition.Instance.GetZones(
                    lifeArea.ToString()
                )[0];

                GridSystem.Instance.WorldBoxToIndices(
                    zone,
                    out Vector2Int starting,
                    out Vector2Int ending
                );

                for (int j = starting.x; j < ending.x; j++)
                {
                    for (int i = starting.y; i < ending.y; i++)
                    {
                        Vector2Int index = new(j, i);
                        Furniture baseFurniture =
                            PlacedFurnitures.Instance.GetBase(index);
                        Furniture stackFurniture =
                            PlacedFurnitures.Instance.GetStack(index);

                        if (
                            baseFurniture != null
                            && !encounteredFurnitures.Contains(baseFurniture)
                        )
                        {
                            if (baseFurniture.lifeArea == lifeArea)
                            {
                                WinCondition.Instance.lifeAreaPoints[
                                    (int)lifeArea
                                ] += basePoints;
                                encounteredFurnitures.Add(baseFurniture);
                            }
                            else if (baseFurniture.element == element)
                            {
                                WinCondition.Instance.elementPoints[
                                    (int)element
                                ] += basePoints;
                                encounteredFurnitures.Add(baseFurniture);
                            }
                        }

                        if (
                            stackFurniture != null
                            && !encounteredFurnitures.Contains(stackFurniture)
                        )
                        {
                            if (stackFurniture.lifeArea == lifeArea)
                            {
                                WinCondition.Instance.lifeAreaPoints[
                                    (int)lifeArea
                                ] += basePoints;
                                encounteredFurnitures.Add(stackFurniture);
                            }
                            else if (stackFurniture.element == element)
                            {
                                WinCondition.Instance.elementPoints[
                                    (int)element
                                ] += basePoints;
                                encounteredFurnitures.Add(stackFurniture);
                            }
                        }
                    }
                }
            }
        };
    }

    public Action StarWhenBonusExceed(float minItemsSecondStar)
    {
        return () =>
        {
            float sum = 0;
            foreach (float point in WinCondition.Instance.lifeAreaPoints)
                sum += point;
            foreach (float point in WinCondition.Instance.elementPoints)
                sum += point;

            if (sum >= minItemsSecondStar * basePoints)
                WinCondition.Instance.stars++;
        };
    }

    public Action StarWhenBonusExceed(
        float minItemsSecondStar,
        float minItemsThirdStar
    )
    {
        return () =>
        {
            float sum = 0;
            foreach (float point in WinCondition.Instance.lifeAreaPoints)
                sum += point;
            foreach (float point in WinCondition.Instance.elementPoints)
                sum += point;

            if (sum >= minItemsThirdStar * basePoints)
                WinCondition.Instance.stars += 2;
            else if (sum >= minItemsSecondStar * basePoints)
                WinCondition.Instance.stars++;
        };
    }

    public Action Present(string furnitureCategory)
    {
        List<string> furnitureList = WinCondition
            .Instance
            .ruleSet
            .furnitureDict[furnitureCategory];
        float maxPoints = furnitureList.Count * basePoints;
        WinCondition.Instance.AddMaxPoints(FSEnergyType.Functional, maxPoints);
        return () =>
        {
            GranularRule(
                furnitureList,
                (string furniture) =>
                {
                    List<string> subList = WinCondition
                        .Instance
                        .ruleSet
                        .furnitureDict[furniture];
                    Debug.Log("checking presense of " + furniture);
                    Debug.Log("sublist count " + subList.Count);
                    foreach (var a in subList)
                        Debug.Log(a);
                    if (subList.Count == 0)
                        return WinCondition
                                .Instance.GetFurnitures(furniture)
                                .Count == 0;
                    else
                        return Functional.All(
                            subList,
                            furniture =>
                                WinCondition
                                    .Instance.GetFurnitures(furniture)
                                    .Count == 0
                        );
                },
                FSEnergyType.Functional,
                maxPoints
            );
        };
    }

    public Action AtLeastOnePresent(string furnitureCategory)
    {
        List<string> furnitureList = WinCondition
            .Instance
            .ruleSet
            .furnitureDict[furnitureCategory];
        WinCondition.Instance.AddMaxPoints(FSEnergyType.Functional, basePoints);
        return () =>
        {
            if (
                Functional.All(
                    furnitureList,
                    furniture =>
                        WinCondition.Instance.GetFurnitures(furniture).Count
                        == 0
                )
            )
                WinCondition.Instance.AddPoints(
                    FSEnergyType.Functional,
                    basePoints
                );
        };
    }

    public Action NotPresent(string furnitureCategory)
    {
        List<string> furnitureList = WinCondition
            .Instance
            .ruleSet
            .furnitureDict[furnitureCategory];
        float maxPoints = furnitureList.Count * basePoints;
        WinCondition.Instance.AddMaxPoints(FSEnergyType.Functional, maxPoints);
        return () =>
        {
            GranularRule(
                furnitureList,
                (string furniture) =>
                    WinCondition.Instance.GetFurnitures(furniture).Count != 0,
                FSEnergyType.Functional,
                maxPoints
            );
        };
    }

    public Action Clear(List<BoundingBox> zones)
    {
        float maxPoints = zones.Count * basePoints;
        WinCondition.Instance.AddMaxPoints(FSEnergyType.Functional, maxPoints);
        return () =>
        {
            InvertGranularRule(
                zones,
                (zone) =>
                    Functional.SatisfiesAll(
                        PlacedFurnitures.Instance.furnitureBaseGrid,
                        zone,
                        (Furniture furniture) => furniture == null
                    ),
                FSEnergyType.Functional,
                maxPoints
            );
        };
    }

    private void GranularRule<T>(
        List<T> list,
        Func<T, bool> func,
        FSEnergyType energyType,
        float basePoints
    )
    {
        if (list.Count == 0)
            return;
        WinCondition.Instance.AddPoints(
            energyType,
            Functional.Count(list, func) * basePoints / list.Count
        );
    }

    private void InvertGranularRule<T>(
        List<T> list,
        Func<T, bool> func,
        FSEnergyType energyType,
        float basePoints
    )
    {
        if (list.Count == 0)
            return;
        WinCondition.Instance.AddPoints(
            energyType,
            basePoints - Functional.Count(list, func) * basePoints / list.Count
        );
    }
}
