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
    }
}
