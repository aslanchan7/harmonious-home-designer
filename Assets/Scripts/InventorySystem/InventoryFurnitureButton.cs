using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class InventoryFurnitureButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public InventoryItem inventoryItem;
    [SerializeField] Image icon;

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<RectTransform>().localScale *= 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponent<RectTransform>().localScale /= 1.1f;
    }

    void OnEnable()
    {
        icon.sprite = inventoryItem.Icon;
        icon.GetComponent<RectTransform>().sizeDelta = new(128, 128);
    }
}
