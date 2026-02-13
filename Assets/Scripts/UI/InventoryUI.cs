using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] private GameObject inventoryFurnitureButtonPrefab;
    [SerializeField] GameObject MenuButtonsUI;

    [Header("Animation Settings")]
    [SerializeField] float fadeOutAnimTime = 0.5f;

    public void InstantiateInventoryButton(InventoryItem item)
    {
        GameObject instantiated = Instantiate(
            inventoryFurnitureButtonPrefab,
            inventoryPanel.transform
        );
        // TODO: Edit the sprite of the button to include an image of the furniture item
        instantiated.GetComponent<InventoryFurnitureButton>().inventoryItem =
            item;

        // Change the onClick functionality of the button to actually instantiate the correct furniture item when clicked
        instantiated
            .GetComponent<Button>()
            .onClick.AddListener(
                delegate()
                {
                    InventoryManager.Instance.TryPlaceFurniture(item.Prefab);
                }
            );
    }

    public void SetFurnitureButtonActive(InventoryItem item)
    {
        for (int i = 0; i < inventoryPanel.transform.childCount; i++)
        {
            GameObject obj = inventoryPanel.transform.GetChild(i).gameObject;
            if (
                obj.GetComponent<InventoryFurnitureButton>().inventoryItem
                == item
            )
            {
                obj.SetActive(true);
                return;
            }
        }
    }

    public void CloseInventoryUI()
    {
        // fade out inventory
        gameObject.GetComponent<CanvasGroup>().LeanAlpha(0f, fadeOutAnimTime).setOnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        // simultaneously zoom in camera
        Camera.main.transform.LeanMoveLocalY(Camera.main.transform.position.y - 2f, fadeOutAnimTime);

        // simultaneously fade in MenuButtonsUI
        MenuButtonsUI.SetActive(true);
        MenuButtonsUI.GetComponent<CanvasGroup>().LeanAlpha(1f, fadeOutAnimTime);
    }
}
