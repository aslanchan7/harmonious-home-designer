using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [HideInInspector]
    public Vector2 MouseTileCenter;

    [HideInInspector]
    public Vector3 MouseNormal;

    public Furniture HoverFurniture;
    public Furniture SelectedFurniture;
    private Furniture lastRotatedFurniture;

    [Header("Actions")]
    private InputAction mousePositionAction;

    [SerializeField]
    private InputActionReference clickAction;

    [SerializeField]
    private InputActionReference rotateAction;

    [SerializeField]
    private InputActionReference deleteAction;
    [SerializeField]
    private InputActionReference pauseAction;

    [Header("References")]
    [SerializeField]
    private MouseIndicator mouseIndicator;

    [SerializeField]
    private GameObject pauseMenu;

    private Coroutine dragUpdateCoroutine;

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
        pauseAction.action.performed += OnPause;
    }

    private void OnDisable()
    {
        clickAction.action.performed -= OnClick;
        rotateAction.action.performed -= OnRotate;
        // This was += seems like it should be -= if I am wrong just switch it back :D
        deleteAction.action.performed -= OnDelete;
        pauseAction.action.performed -= OnPause;
    }

    private void Update()
    {
        RaycastMouse();
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        if (pauseMenu.activeSelf)
            pauseMenu.GetComponent<PauseMenuUI>().ResumeGame();
        else
            pauseMenu.SetActive(true);
    }

    private void OnClick(InputAction.CallbackContext callbackContext)
    {
        if (HoverFurniture != null)
        {
            SelectedFurniture = HoverFurniture;

            SFXManager.Instance?.PlayFurnitureSFX(SelectedFurniture.SfxCategory, SFXAction.Pickup);

            GridSystem.Instance.ShowGridVisualizer();
            dragUpdateCoroutine = StartCoroutine(DragUpdate());
        }
        else
        {
            SelectedFurniture = null;
            GridSystem.Instance.HideGridVisualizer();
        }
    }

    private IEnumerator DragUpdate()
    {
        SelectedFurniture.PlacedFurnituresUnset();

        while (clickAction.action.ReadValue<float>() != 0)
        {
            SelectedFurniture.MoveGhost(mouseIndicator.Position);
            // Adjust mouse indicator's elevation;
            if (SelectedFurniture.DisplayElevation != mouseIndicator.Elevation)
                mouseIndicator.Elevation = SelectedFurniture.DisplayElevation;

            yield return null;
        }

        SelectedFurniture.TryPlace();
        SelectedFurniture = null;
        GridSystem.Instance.HideGridVisualizer();
    }

    private void OnRotate(InputAction.CallbackContext callbackContext)
    {
        if (SelectedFurniture != null)
        {
            if (SelectedFurniture.lockRotation)
                return;
            SelectedFurniture.DisplayRotation += 90;

            SFXManager.Instance?.PlayFurnitureSFX(SelectedFurniture.SfxCategory, SFXAction.Rotate);

            mouseIndicator.Rotate();
            return;
        }

        if (HoverFurniture == null)
            return;

        if (HoverFurniture.lockRotation)
        {
            PlacedFurnitures.Instance.BoundingBoxToIndices(
                HoverFurniture.GetBoundingBox(),
                out Vector2Int starting,
                out _
            );
            Furniture beneath = PlacedFurnitures.Instance.GetBase(starting);
            // beneath should not be null.
            if (beneath.lockRotation || !beneath.acceptRotationPassThrough)
                return;
            HoverFurniture = beneath;
        }
        HoverFurniture.DisplayRotation += 90;

        SFXManager.Instance?.PlayFurnitureSFX(HoverFurniture.SfxCategory, SFXAction.Rotate);

        if (HoverFurniture.Size.x == HoverFurniture.Size.y)
        {
            HoverFurniture.PlacedFurnituresUnset();
            HoverFurniture.SetLocationAsValid();
            return;
        }

        bool sizeIsEven =
            (HoverFurniture.Size.x + HoverFurniture.Size.y) % 2 == 0;
        HoverFurniture.PlacedFurnituresUnset();
        if (sizeIsEven && HoverFurniture.CheckValidPos())
        {
            HoverFurniture.SetLocationAsValid();
            return;
        }

        Vector2 currentPosition = HoverFurniture.LastValidPosition;
        Vector2[] offsetsToCheck = sizeIsEven
            ? evenSizeSnapOffset
            : oddSizeSnapOffset;
        if (
            lastRotatedFurniture == null
            || HoverFurniture != lastRotatedFurniture
        )
        {
            rotateSnapOffsetIndex = 0;
            lastRotatedFurniture = HoverFurniture;
        }

        for (int i = 0; i < 4; i++)
        {
            Vector2 testPosition =
                currentPosition
                + offsetsToCheck[(rotateSnapOffsetIndex + i) % 4];
            HoverFurniture.DisplayPosition = testPosition;
            if (HoverFurniture.CheckValidPos())
            {
                // Make sure that if the furniture is rotated consecutively,
                // it would favor the offset direction opposite to the
                // current offset
                rotateSnapOffsetIndex = (rotateSnapOffsetIndex + i + 2) % 4;
                HoverFurniture.SetLocationAsValid();
                return;
            }
        }

        HoverFurniture.ResetToValidLocation();
    }

    private void OnDelete(InputAction.CallbackContext callbackContext)
    {
        Furniture deletedFurniture;
        if (SelectedFurniture != null)
        {
            StopCoroutine(dragUpdateCoroutine);
            SelectedFurniture.TryPlace();
            deletedFurniture = SelectedFurniture;
            SelectedFurniture = null;
        }
        else
        {
            deletedFurniture = HoverFurniture;
            if (deletedFurniture == null)
                return;
        }

        InventoryItem item =
            InventoryManager.Instance.inventorySO.inventoryList.Find(x =>
                deletedFurniture.furnitureName.Equals(
                    x.Prefab.GetComponent<Furniture>().furnitureName
                )
            );
        
        UISFX.Play(SFXAction.UI_DeleteItem);

        deletedFurniture.DestroyPrefab();
        GridSystem.Instance.HideGridVisualizer();
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
            Vector2 position2d = new(hit.point.x, hit.point.z);
            Vector2 halfSize = (Vector2)GridSystem.Instance.Size / 2.0f;
            Vector2 centerCellOffset = new Vector2(
                GridSystem.Instance.CenterCellOffset.x,
                GridSystem.Instance.CenterCellOffset.z
            );
            Vector2Int index = Vector2Int.FloorToInt(position2d + halfSize);
            if (hit.collider.CompareTag("Furniture"))
            {
                HoverFurniture =
                    hit.transform.GetComponentInParent<Furniture>();
            }
            else
            {
                if (
                    Mathf.Abs(position2d.x) > halfSize.x
                    || Mathf.Abs(position2d.y) > halfSize.y
                )
                {
                    HoverFurniture = null;
                }
                else
                {
                    HoverFurniture = PlacedFurnitures.Instance.GetBase(index);
                }
            }
            MouseTileCenter =
                Vector2Int.RoundToInt(position2d - centerCellOffset)
                + centerCellOffset;
            MouseNormal = hit.normal;
        }
    }
}
