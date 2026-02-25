using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("References")]
    public InventoryScriptableObject inventorySO; // SO stands for scriptable object
    public InventoryUI inventoryUI;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Instantiate buttons for each furniture item from inventoryList
        foreach (var furniture in inventorySO.inventoryList)
        {
            inventoryUI.InstantiateInventoryButton(furniture);
            furniture.CurrentPlacedCount = 0;
        }
    }

    public void TryPlaceFurniture(GameObject furniturePrefab)
    {
        if (furniturePrefab == null)
            return;

        // GameObject newGameObject = Instantiate(furniturePrefab);
        Furniture newFurniture = furniturePrefab
            .GetComponent<Furniture>()
            .InstantiatePrefab();

        // newFurniture.InitializeState();

        Vector2Int gridSize = GridSystem.Instance.Size;
        int numXPositions = gridSize.x - newFurniture.Size.x + 1;
        int numYPositions = gridSize.y - newFurniture.Size.y + 1;
        Vector2 testPosition = (Vector2)(newFurniture.Size - gridSize) / 2;
        Vector2 rowIncrement = new(newFurniture.Size.x - gridSize.x - 1, 1);

        for (int i = 0; i < numYPositions; i++)
        {
            for (int j = 0; j < numXPositions; j++)
            {
                newFurniture.DisplayPosition = testPosition;
                if (newFurniture.CheckValidPos())
                {
                    WinCondition.Instance.AddFurniture(newFurniture);
                    newFurniture.SetLocationAsValid();

                    InventoryItem item = inventorySO.inventoryList.Find(x =>
                        x.Prefab == furniturePrefab
                    );
                    if (item != null)
                    {
                        if (item.CurrentPlacedCount >= item.MaxPlacements)
                        {
                            // Destory instantiated furniture if reached MaxPlacement
                            Debug.LogWarning(
                                $"Limit exceeded for {furniturePrefab.name}!"
                            );
                            Destroy(newFurniture.gameObject);
                            return;
                        }

                        item.CurrentPlacedCount++;
                    }

                    return;
                }

                testPosition += Vector2.right;
            }

            testPosition += rowIncrement;
        }

        // Destroy if no empty space.
        Destroy(newFurniture.gameObject);
    }
}
