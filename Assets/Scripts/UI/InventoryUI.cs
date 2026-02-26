using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] private GameObject inventoryFurnitureButtonPrefab;
    [SerializeField] GameObject MenuButtonsUI;
    [SerializeField] Transform buttonsChild;

    [Header("Animation Settings")]
    [SerializeField] float fadeOutAnimTime = 0.5f;
    private float targetCameraPosY;

    void Start()
    {
        targetCameraPosY = Camera.main.transform.position.y;
    }

    public void InstantiateInventoryButton(InventoryItem item)
    {
        GameObject instantiated = Instantiate(
            inventoryFurnitureButtonPrefab,
            buttonsChild
        );

        instantiated.GetComponent<InventoryFurnitureButton>().inventoryItem =
            item;

        // Change the onClick functionality of the button to actually instantiate the correct furniture item when clicked
        instantiated
            .GetComponent<Button>()
            .onClick.AddListener(
                delegate ()
                {
                    InventoryManager.Instance.TryPlaceFurniture(item.Prefab);
                }
            );

        // Change the size of buttonsChild to account for scroll view
        UpdateScrollViewSize();
    }

    void Update()
    {
        UpdateScrollViewSize();
    }

    private void UpdateScrollViewSize()
    {
        RectTransform content = inventoryPanel.GetComponent<ScrollRect>().content;
        GridLayoutGroup buttonsGrid = buttonsChild.GetComponent<GridLayoutGroup>();
        int numberOfButtons = 0;
        for (int i = 0; i < buttonsChild.childCount; i++)
        {
            numberOfButtons += buttonsChild.GetChild(i).gameObject.activeSelf ? 1 : 0;
        }
        int rows = (numberOfButtons + 1) / buttonsGrid.constraintCount;
        // float height = buttonsGrid.padding.top + (rows * buttonsGrid.cellSize.y) 
        //                 + ((rows - 1) * buttonsGrid.spacing.y) + (buttonsGrid.spacing.y / 2f) + buttonsGrid.padding.bottom;
        float height = (rows * buttonsGrid.cellSize.y)
                        + ((rows - 1) * buttonsGrid.spacing.y) + (buttonsGrid.spacing.y / 2f) + buttonsGrid.padding.bottom;
        content.sizeDelta = new(content.sizeDelta.x, height);
    }

    public void SetFurnitureButtonActive(InventoryItem item)
    {
        for (int i = 0; i < buttonsChild.childCount; i++)
        {
            GameObject obj = buttonsChild.GetChild(i).gameObject;
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

        UISFX.Play(SFXAction.UI_BoxClose);

        // simultaneously zoom in camera
        Camera.main.transform.LeanMoveLocalY(targetCameraPosY, fadeOutAnimTime);

        // simultaneously fade in MenuButtonsUI
        MenuButtonsUI.SetActive(true);
        MenuButtonsUI.GetComponent<CanvasGroup>().LeanAlpha(1f, fadeOutAnimTime);
    }
}
