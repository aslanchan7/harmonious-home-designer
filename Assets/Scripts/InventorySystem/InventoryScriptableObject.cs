using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory List")]
public class InventoryScriptableObject : ScriptableObject
{
    public List<InventoryItem> inventoryList = new();
}

[System.Serializable]
public class InventoryItem
{
    public GameObject Prefab;
    public int MaxPlacements;
    public int CurrentPlacedCount = 0;
}

