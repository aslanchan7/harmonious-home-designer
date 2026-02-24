using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryFurnitureButton : MonoBehaviour
{
    public InventoryItem inventoryItem;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Image icon;

    void OnEnable()
    {
        icon.sprite = inventoryItem.Icon;
        icon.GetComponent<RectTransform>().sizeDelta = new(128, 128);
    }

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
