using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [HideInInspector]
    public Vector3 MousePos;
    private Furniture selectedFurniture;
    private Furniture lastRotatedFurniture;

    [Header("Actions")]
    private InputAction mousePositionAction;

    [SerializeField]
    private InputActionReference clickAction;

    [SerializeField]
    private InputActionReference rotateAction;

    [SerializeField]
    private InputActionReference deleteAction;

    [Header("References")]
    [SerializeField]
    private MouseIndicator mouseIndicator;

    private readonly Vector2[] evenSizeSnapOffset =
    {
        Vector2.down,
        Vector2.right,
        Vector2.up,
        Vector2.left,
    };
    private readonly Vector2[] oddSizeSnapOffset =
    {
        new Vector2(-0.5f, -0.5f),
        new Vector2(-0.5f, 0.5f),
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, -0.5f),
    };
    private int rotateSnapOffsetIndex;

    private void Start()
    {
        mousePositionAction = InputSystem.actions.FindAction("Point");
        rotateSnapOffsetIndex = 0;
    }

    private void OnEnable()
    {
        clickAction.action.performed += OnClick;
        rotateAction.action.performed += OnRotate;
        deleteAction.action.performed += OnDelete;
    }

    private void OnDisable()
    {
        clickAction.action.performed -= OnClick;
        rotateAction.action.performed -= OnRotate;
        deleteAction.action.performed += OnDelete;
    }

    private void Update()
    {
        RaycastMouse();
    }

    private void OnClick(InputAction.CallbackContext callbackContext)
    {
        Furniture raycastFurniture = RaycastFurniture();
        if (raycastFurniture != null)
        {
            selectedFurniture = raycastFurniture;
            GridSystem.Instance.ShowGridVisualizer();
            StartCoroutine(DragUpdate());
        }
        else
        {
            selectedFurniture = null;
            GridSystem.Instance.HideGridVisualizer();
        }
    }

    private IEnumerator DragUpdate()
    {
        selectedFurniture.ColliderOff();
        mouseIndicator.Size = selectedFurniture.Size;

        while (clickAction.action.ReadValue<float>() != 0)
        {
            selectedFurniture.MoveGhost(mouseIndicator.Position);

            yield return null;
        }

        selectedFurniture.TryPlace();
        selectedFurniture = null;
        GridSystem.Instance.HideGridVisualizer();
        mouseIndicator.Size = new(1, 1);
    }

    private void OnRotate(InputAction.CallbackContext callbackContext)
    {
        // Is dragging
        if (selectedFurniture != null)
        {
            selectedFurniture.DisplayRotation += 90;
            mouseIndicator.Rotate();
            return;
        }

        Furniture hoverFurniture = RaycastFurniture();
        if (hoverFurniture == null)
            return;
        hoverFurniture.DisplayRotation += 90;

        if (hoverFurniture.Size.x == hoverFurniture.Size.y)
        {
            mouseIndicator.Rotate();
            hoverFurniture.ColliderOff();
            hoverFurniture.SetLocationAsValid();
            return;
        }

        bool sizeIsEven =
            (hoverFurniture.Size.x + hoverFurniture.Size.y) % 2 == 0;
        hoverFurniture.ColliderOff();
        if (sizeIsEven && hoverFurniture.CheckValidPos())
        {
            mouseIndicator.Rotate();
            hoverFurniture.SetLocationAsValid();
            return;
        }

        Vector2 currentPosition = hoverFurniture.LastValidPosition;
        Vector2[] offsetsToCheck = sizeIsEven
            ? evenSizeSnapOffset
            : oddSizeSnapOffset;
        if (
            lastRotatedFurniture == null
            || hoverFurniture != lastRotatedFurniture
        )
        {
            rotateSnapOffsetIndex = 0;
            lastRotatedFurniture = hoverFurniture;
        }

        for (int i = 0; i < 4; i++)
        {
            Vector2 testPosition =
                currentPosition
                + offsetsToCheck[(rotateSnapOffsetIndex + i) % 4];
            hoverFurniture.DisplayPosition = testPosition;
            if (hoverFurniture.CheckValidPos())
            {
                // Make sure that if the furniture is rotated consecutively,
                // it would favor the offset direction opposite to the
                // current offset
                rotateSnapOffsetIndex = (rotateSnapOffsetIndex + i + 2) % 4;
                mouseIndicator.Rotate();
                hoverFurniture.SetLocationAsValid();
                return;
            }
        }

        hoverFurniture.ResetToValidLocation();
    }

    public Furniture RaycastFurniture()
    {
        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (
            Physics.Raycast(ray, out hit)
            && hit.collider.CompareTag("Furniture")
        )
        {
            return hit.collider.transform.parent.GetComponent<Furniture>();
        }
        return null;
    }

    private void OnDelete(InputAction.CallbackContext callbackContext)
    {
        Furniture hoverFurniture = RaycastFurniture();
        if (hoverFurniture == null)
            return;

        InventoryItem item =
            InventoryManager.Instance.inventorySO.inventoryList.Find(x =>
                hoverFurniture.furnitureName.Equals(
                    x.Prefab.GetComponent<Furniture>().furnitureName
                )
            );

        if (item != null)
        {
            if (item.CurrentPlacedCount == item.MaxPlacements)
            {
                InventoryManager.Instance.inventoryUI.SetFurnitureButtonActive(
                    item
                );
            }

            item.CurrentPlacedCount--;
        }

        hoverFurniture.DestroyPrefab();
        selectedFurniture = null;
        GridSystem.Instance.HideGridVisualizer();
        mouseIndicator.Size = new(1, 1);
    }

    public void RaycastMouse()
    {
        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (
            Physics.Raycast(
                ray,
                out hit,
                float.PositiveInfinity,
                GridSystem.Instance.PlacementLayer
            )
        )
        {
            MousePos = hit.point;
        }
    }
}
