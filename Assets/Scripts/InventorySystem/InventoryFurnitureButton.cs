using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryFurnitureButton : MonoBehaviour
{
    public InventoryItem inventoryItem;
    [SerializeField] TextMeshProUGUI text;

    void Update()
    {
        if(inventoryItem.CurrentPlacedCount == inventoryItem.MaxPlacements)
        {
            gameObject.SetActive(false);
        }

        UpdateText();
    }

    void UpdateText()
    {
        text.text = (inventoryItem.MaxPlacements - inventoryItem.CurrentPlacedCount).ToString();
    }
}
