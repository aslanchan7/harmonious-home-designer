using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum FurnitureCategory
{
    Required = 1,
    Unacceptable = 2,
    Accessible = 4,
    LightSource = 8,
}

[Flags]
public enum FixedItemCategory
{
    Door = 1,
}

[Flags]
public enum ZoneCategory
{
    Chaos = 1,
    Bagua = 2,
}

[Serializable]
public class FurnitureEntry
{
    public string name;
    public FurnitureCategory categories;
}

[Serializable]
public class FixedItemEntry
{
    public string name;
    public DirectedBox boundingBox;
    public FixedItemCategory categories;
    public bool showInEditor;
}

[Serializable]
public class ZoneEntry
{
    public string name;
    public BoundingBox zone;
    public ZoneCategory categories;
    public bool showInEditor;
}

[CreateAssetMenu(fileName = "New Rule Set", menuName = "Rule Set")]
public class RuleSet : ScriptableObject, ISerializationCallbackReceiver
{
    public Vector2Int roomSize;
    public FurnitureEntry[] furnitures;
    public FixedItemEntry[] fixedItems;

    public ZoneEntry[] zones;

    [Header("Refer to Rule.cs for the rule functions' parameters")]
    public Rule[] rules;

    public Dictionary<string, List<string>> furnitureDict;
    public Dictionary<string, List<DirectedBox>> fixedItemDict;
    public Dictionary<string, List<BoundingBox>> zoneDict;

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        furnitureDict = new();
        foreach (FurnitureEntry entry in furnitures)
        {
            if (!furnitureDict.ContainsKey(entry.name))
                furnitureDict.Add(entry.name, new List<string>());
            foreach (
                FurnitureCategory category in Enum.GetValues(
                    typeof(FurnitureCategory)
                )
            )
            {
                if (!entry.categories.HasFlag(category))
                    continue;

                string categoryString = category.ToString();
                if (!furnitureDict.ContainsKey(categoryString))
                {
                    furnitureDict.Add(
                        categoryString,
                        new List<string>(new string[] { entry.name })
                    );
                }
                else
                {
                    furnitureDict[categoryString].Add(entry.name);
                }
            }
        }

        fixedItemDict = new();
        foreach (FixedItemEntry entry in fixedItems)
        {
            fixedItemDict.Add(
                entry.name,
                new List<DirectedBox>(new DirectedBox[] { entry.boundingBox })
            );
            foreach (
                FixedItemCategory category in Enum.GetValues(
                    typeof(FixedItemCategory)
                )
            )
            {
                if (!entry.categories.HasFlag(category))
                    continue;

                string categoryString = category.ToString();
                if (!fixedItemDict.ContainsKey(categoryString))
                {
                    fixedItemDict.Add(
                        categoryString,
                        new List<DirectedBox>(
                            new DirectedBox[] { entry.boundingBox }
                        )
                    );
                }
                else
                {
                    fixedItemDict[categoryString].Add(entry.boundingBox);
                }
            }
        }

        float halfWidth = roomSize.x / 2.0f;
        float halfHeight = roomSize.y / 2.0f;
        List<DirectedBox> walls = new(
            new DirectedBox[]
            {
                new DirectedBox(
                    new(-halfWidth, halfHeight),
                    new(roomSize.x, 0),
                    180
                ),
                new DirectedBox(
                    new(halfWidth, -halfHeight),
                    new(0, roomSize.y),
                    -90
                ),
                new DirectedBox(
                    new(-halfWidth, -halfHeight),
                    new(roomSize.x, 0),
                    0
                ),
                new DirectedBox(
                    new(-halfWidth, -halfHeight),
                    new(0, roomSize.y),
                    90
                ),
            }
        );
        fixedItemDict.Add("TopWall", new(new DirectedBox[] { walls[0] }));
        fixedItemDict.Add("RightWall", new(new DirectedBox[] { walls[1] }));
        fixedItemDict.Add("BottomWall", new(new DirectedBox[] { walls[2] }));
        fixedItemDict.Add("LeftWall", new(new DirectedBox[] { walls[3] }));
        fixedItemDict.Add("Wall", walls);

        zoneDict = new();
        foreach (ZoneEntry entry in zones)
        {
            zoneDict.Add(
                entry.name,
                new List<BoundingBox>(new BoundingBox[] { entry.zone })
            );
            foreach (
                ZoneCategory category in Enum.GetValues(typeof(ZoneCategory))
            )
            {
                if (!entry.categories.HasFlag(category))
                    continue;

                string categoryString = category.ToString();
                if (!zoneDict.ContainsKey(categoryString))
                {
                    zoneDict.Add(
                        categoryString,
                        new List<BoundingBox>(new BoundingBox[] { entry.zone })
                    );
                }
                else
                {
                    zoneDict[categoryString].Add(entry.zone);
                }
            }
        }

        int thirdWidth = roomSize.x / 3;
        int widthRemainder = roomSize.x % 3;
        int sideBaguaZoneWidth =
            widthRemainder == 2 ? thirdWidth + 1 : thirdWidth;
        int centerBaguaZoneWidth =
            widthRemainder == 1 ? thirdWidth + 1 : thirdWidth;

        int thirdHeight = roomSize.y / 3;
        int heightRemainder = roomSize.y % 3;
        int sideBaguaZoneHeight =
            heightRemainder == 2 ? thirdHeight + 1 : thirdHeight;
        int centerBaguaZoneHeight =
            heightRemainder == 1 ? thirdHeight + 1 : thirdHeight;

        List<BoundingBox> bagua = new(
            new BoundingBox[]
            {
                new BoundingBox(
                    new(-halfWidth, halfHeight - sideBaguaZoneHeight),
                    new(sideBaguaZoneWidth, sideBaguaZoneHeight)
                ),
                new BoundingBox(
                    new(
                        -halfWidth + sideBaguaZoneWidth,
                        halfHeight - sideBaguaZoneHeight
                    ),
                    new(centerBaguaZoneWidth, sideBaguaZoneHeight)
                ),
                new BoundingBox(
                    new(
                        halfWidth - sideBaguaZoneWidth,
                        halfHeight - sideBaguaZoneHeight
                    ),
                    new(sideBaguaZoneWidth, sideBaguaZoneHeight)
                ),
                new BoundingBox(
                    new(-halfWidth, -halfHeight + sideBaguaZoneHeight),
                    new(sideBaguaZoneWidth, centerBaguaZoneHeight)
                ),
                new BoundingBox(
                    new(
                        -halfWidth + sideBaguaZoneWidth,
                        -halfHeight + sideBaguaZoneHeight
                    ),
                    new(centerBaguaZoneWidth, centerBaguaZoneHeight)
                ),
                new BoundingBox(
                    new(
                        halfWidth - sideBaguaZoneWidth,
                        -halfHeight + sideBaguaZoneHeight
                    ),
                    new(sideBaguaZoneWidth, centerBaguaZoneHeight)
                ),
                new BoundingBox(
                    new(-halfWidth, -halfHeight),
                    new(sideBaguaZoneWidth, sideBaguaZoneHeight)
                ),
                new BoundingBox(
                    new(-halfWidth + sideBaguaZoneWidth, -halfHeight),
                    new(centerBaguaZoneWidth, sideBaguaZoneHeight)
                ),
                new BoundingBox(
                    new(halfWidth - sideBaguaZoneWidth, -halfHeight),
                    new(sideBaguaZoneWidth, sideBaguaZoneHeight)
                ),
            }
        );
        zoneDict.Add(
            "WealthAndProsperity",
            new(new BoundingBox[] { bagua[0] })
        );
        zoneDict.Add("FameAndReputation", new(new BoundingBox[] { bagua[1] }));
        zoneDict.Add("Relationships", new(new BoundingBox[] { bagua[2] }));
        zoneDict.Add("Family", new(new BoundingBox[] { bagua[3] }));
        zoneDict.Add("Health", new(new BoundingBox[] { bagua[4] }));
        zoneDict.Add(
            "ChildrenAndCreativity",
            new(new BoundingBox[] { bagua[5] })
        );
        zoneDict.Add(
            "KnowledgeAndSelfCultivation",
            new(new BoundingBox[] { bagua[6] })
        );
        zoneDict.Add("Career", new(new BoundingBox[] { bagua[7] }));
        zoneDict.Add(
            "TravelAndHelpfulPeople",
            new(new BoundingBox[] { bagua[8] })
        );
        zoneDict.Add("Bagua", bagua);
    }
}
