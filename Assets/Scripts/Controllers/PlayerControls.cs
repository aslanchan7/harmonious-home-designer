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

    [Header("Actions")]
    private InputAction mousePositionAction;

    [SerializeField]
    private InputActionReference clickAction;

    [SerializeField]
    private InputActionReference rotateAction;

    [Header("References")]
    [SerializeField]
    private MouseIndicator mouseIndicator;

    [SerializeField]
    private TextMeshProUGUI bedText;

    [SerializeField]
    private TextMeshProUGUI dresserText;

    [SerializeField]
    private List<FurnitureInventory> inventoryList =
        new List<FurnitureInventory>();

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
        UpdateUIText();
    }

    private void OnEnable()
    {
        clickAction.action.performed += OnClick;
        rotateAction.action.performed += OnRotate;
    }

    private void OnDisable()
    {
        clickAction.action.performed -= OnClick;
        rotateAction.action.performed -= OnRotate;
    }

    private void Update()
    {
        RaycastMouse();
    }

    private void OnClick(InputAction.CallbackContext callbackContext)
    {
        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Furniture") && hit.collider != null)
            {
                selectedFurniture =
                    hit.collider.transform.parent.GetComponent<Furniture>();
                rotateSnapOffsetIndex = 0;
                GridSystem.Instance.ShowGridVisualizer();
                StartCoroutine(DragUpdate());
            }
            else
            {
                selectedFurniture = null;
                GridSystem.Instance.HideGridVisualizer();
            }
        }
    }

    private IEnumerator DragUpdate()
    {
        selectedFurniture.SetColliderEnabled(false);
        mouseIndicator.Size = selectedFurniture.Size;

        while (clickAction.action.ReadValue<float>() != 0)
        {
            selectedFurniture.MoveGhost(mouseIndicator.Position);

            yield return null;
        }

        selectedFurniture.TryPlace();
        selectedFurniture.SetNormalMat();
        selectedFurniture.SetColliderEnabled(true);
        mouseIndicator.Size = new(1, 1);
    }

    private void OnRotate(InputAction.CallbackContext callbackContext)
    {
        if (selectedFurniture == null)
            return;

        selectedFurniture.DisplayRotation += 90;

        if (clickAction.action.ReadValue<float>() != 0)
        {
            mouseIndicator.Rotate();
            return;
        }

        if (selectedFurniture.Size.x == selectedFurniture.Size.y)
        {
            mouseIndicator.Rotate();
            selectedFurniture.SetLocationAsValid();
            return;
        }

        bool sizeIsEven =
            (selectedFurniture.Size.x + selectedFurniture.Size.y) % 2 == 0;
        selectedFurniture.SetColliderEnabled(false);
        if (sizeIsEven && selectedFurniture.CheckValidPos())
        {
            selectedFurniture.SetColliderEnabled(true);
            mouseIndicator.Rotate();
            selectedFurniture.SetLocationAsValid();
            return;
        }

        Vector2 currentPosition = selectedFurniture.LastValidPosition;
        Vector2[] offsetsToCheck = sizeIsEven
            ? evenSizeSnapOffset
            : oddSizeSnapOffset;
        for (int i = 0; i < 4; i++)
        {
            Vector2 testPosition =
                currentPosition
                + offsetsToCheck[(rotateSnapOffsetIndex + i) % 4];
            selectedFurniture.DisplayPosition = testPosition;
            if (selectedFurniture.CheckValidPos())
            {
                // Make sure that if the furniture is rotated consecutively,
                // it would favor the offset direction opposite to the
                // current offset
                rotateSnapOffsetIndex = (rotateSnapOffsetIndex + i + 2) % 4;
                mouseIndicator.Rotate();
                selectedFurniture.SetColliderEnabled(true);
                selectedFurniture.SetLocationAsValid();
                return;
            }
        }

        selectedFurniture.SetColliderEnabled(true);
        selectedFurniture.ResetToValidLocation();
    }

    private void UpdateUIText()
    {
        FurnitureInventory bedItem = inventoryList.Find(x =>
            x.Prefab != null && x.Prefab.name.Contains("Bed")
        );
        FurnitureInventory dresserItem = inventoryList.Find(x =>
            x.Prefab != null && x.Prefab.name.Contains("Dresser")
        );

        if (bedText != null && bedItem != null)
            bedText.text =
                $"{bedItem.MaxPlacements - bedItem.CurrentPlacedCount}";

        if (dresserText != null && dresserItem != null)
            dresserText.text =
                $"{dresserItem.MaxPlacements - dresserItem.CurrentPlacedCount}";
    }

    public void PlaceFurnitureRandomly(GameObject furniturePrefab)
    {
        if (furniturePrefab == null)
            return;

        FurnitureInventory item = inventoryList.Find(x =>
            x.Prefab == furniturePrefab
        );

        if (item != null)
        {
            if (item.CurrentPlacedCount >= item.MaxPlacements)
            {
                Debug.LogWarning($"Limit reached for {furniturePrefab.name}!");
                return;
            }
        }

        GameObject newFurnitureObj = Instantiate(furniturePrefab);
        Furniture furnScript = newFurnitureObj.GetComponent<Furniture>();

        if (item != null)
        {
            item.CurrentPlacedCount++;
            UpdateUIText();
        }

        Vector2Int gridSize = GridSystem.Instance.Size;
        int halfX = gridSize.x / 2;
        int halfY = gridSize.y / 2;

        int randomX = UnityEngine.Random.Range(-halfX + 2, halfX - 2);
        int randomY = UnityEngine.Random.Range(-halfY + 2, halfY - 2);

        float posX = (furnScript.Size.x % 2 == 0) ? randomX : randomX + 0.5f;
        float posY = (furnScript.Size.y % 2 == 0) ? randomY : randomY + 0.5f;

        furnScript.DisplayPosition = new Vector2(posX, posY);
        furnScript.TryPlace();
    }

    public void DeleteSelected()
    {
        if (selectedFurniture == null)
            return;

        FurnitureInventory item = inventoryList.Find(x =>
            selectedFurniture.name.Contains(x.Prefab.name)
        );

        if (item != null)
        {
            item.CurrentPlacedCount--;
            UpdateUIText();
        }

        Destroy(selectedFurniture.gameObject);

        selectedFurniture = null;
        GridSystem.Instance.HideGridVisualizer();
        mouseIndicator.Size = new Vector2Int(1, 1);
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

        if (
            Keyboard.current.deleteKey.wasPressedThisFrame
            || Keyboard.current.backspaceKey.wasPressedThisFrame
        )
        {
            DeleteSelected();
        }
    }
}

[System.Serializable]
public class FurnitureInventory
{
    public GameObject Prefab;
    public int MaxPlacements;

    [HideInInspector]
    public int CurrentPlacedCount;
}
