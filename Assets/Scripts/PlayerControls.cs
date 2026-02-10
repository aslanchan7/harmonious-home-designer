using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PlayerControls : MonoBehaviour
{
    [HideInInspector] public Vector3 MousePos;
    // private Vector2 tilePointedAt;
    private Vector3 normalDirection;
    // private GameObject gameObjectPointedAt;
    // public Furniture GhostFurniture;

    // [SerializeField] private LayerMask placementLayer;

    // [SerializeField]
    // private GameObject mouseIndicator;

    // [SerializeField] private Furniture furniturePrefab;

    // [Header("References")]
    // [SerializeField] private GridSystem gridSystem;
    private Furniture selectedFurniture;

    [Header("Actions")]
    private InputAction mousePositionAction;
    [SerializeField] private InputActionReference clickAction;
    [SerializeField] private InputActionReference rotateAction;

    [Header("References")]
    [SerializeField] private MouseIndicator mouseIndicator;

    private readonly Vector2[] rotationSnapOffset =
    {
        new Vector2(-0.5f, -0.5f),
        new Vector2(-0.5f, 0.5f),
        new Vector2(0.5f, -0.5f),
        new Vector2(0.5f, 0.5f)
    };
    private int rotateSnapOffsetIndex;

    // private WaitForFixedUpdate waitForFixedUpdate = new();

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
                selectedFurniture = hit.collider.transform.parent.GetComponent<Furniture>();
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
    if (selectedFurniture == null) yield break;

    selectedFurniture.SetGhostMaterial();
    mouseIndicator.SetSize(selectedFurniture.Size.x, selectedFurniture.Size.y);
    Vector2 pos = new(mouseIndicator.transform.position.x, mouseIndicator.transform.position.z);

    while (clickAction.action.ReadValue<float>() != 0)
    {
        if (selectedFurniture == null) 
        {
            GridSystem.Instance.HideGridVisualizer();
            mouseIndicator.SetSize(1, 1);
            yield break; 
        }

        pos = new Vector2(mouseIndicator.transform.position.x, mouseIndicator.transform.position.z);
        selectedFurniture.MoveGhost(pos);

        yield return null;
    }

    if (selectedFurniture != null)
    {
        mouseIndicator.SetSize(1, 1);
        selectedFurniture.TryPlace(pos);
        selectedFurniture.SetNormalMat();
    }
    
    selectedFurniture = null; 
}

    private void OnRotate(InputAction.CallbackContext callbackContext)
    {
        if (selectedFurniture == null)
            return;

        selectedFurniture.SetRotation(selectedFurniture.transform.eulerAngles.y + 90);
        if (clickAction.action.ReadValue<float>() != 0)
        {
            mouseIndicator.Rotate();
        } else if ((selectedFurniture.Size.x + selectedFurniture.Size.y) % 2 == 0)
        {
            mouseIndicator.Rotate();
            selectedFurniture.LastValidRotation = selectedFurniture.transform.eulerAngles.y;
        }
        else
        {
            Vector2 currentPosition = selectedFurniture.LastValidPos;
            bool rotateSuccess = false;
            foreach (Collider collider in selectedFurniture.Colliders)
            {
                collider.enabled = false;
            }
            for (int i = 0; i < 4; i++)
            {
                Vector2 testPosition = currentPosition + rotationSnapOffset[
                    (rotateSnapOffsetIndex + i) % 4
                ];
                selectedFurniture.transform.position = new Vector3(
                    testPosition.x,
                    selectedFurniture.transform.position.y,
                    testPosition.y
                );
                if (selectedFurniture.CheckValidPos())
                {
                    rotateSuccess = true;
                    rotateSnapOffsetIndex = (rotateSnapOffsetIndex + i + 1) % 4;
                    selectedFurniture.SetPosition(testPosition);
                    mouseIndicator.Rotate();
                    selectedFurniture.LastValidRotation = selectedFurniture.transform.eulerAngles.y;
                    break;
                }
            }

            if (!rotateSuccess)
            {
                selectedFurniture.ResetToValidLocation();
            }

            foreach (Collider collider in selectedFurniture.Colliders)
            {
                collider.enabled = true;
            }
        }
    }

    public void RaycastMouse()
    {
        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity, GridSystem.Instance.PlacementLayer))
        {
            MousePos = hit.point;
            normalDirection = hit.normal;
            // gameObjectPointedAt = hit.collider.gameObject;
        }

        if (Keyboard.current.deleteKey.wasPressedThisFrame || Keyboard.current.backspaceKey.wasPressedThisFrame)
    {
        DeleteSelected();
    }
    }
[SerializeField] private TextMeshProUGUI bedText;
[SerializeField] private TextMeshProUGUI dresserText;
[SerializeField] private List<FurnitureInventory> inventoryList = new List<FurnitureInventory>();
private void UpdateUIText()
    {
        FurnitureInventory bedItem = inventoryList.Find(x => x.Prefab != null && x.Prefab.name.Contains("Bed"));
        FurnitureInventory dresserItem = inventoryList.Find(x => x.Prefab != null && x.Prefab.name.Contains("Dresser"));

        if (bedText != null && bedItem != null)
            bedText.text = $"{bedItem.MaxPlacements - bedItem.CurrentPlacedCount}";
        
        if (dresserText != null && dresserItem != null)
            dresserText.text = $"{dresserItem.MaxPlacements - dresserItem.CurrentPlacedCount}";
    }
    public void PlaceFurnitureRandomly(GameObject furniturePrefab)
    {
        if (furniturePrefab == null) return;

        FurnitureInventory item = inventoryList.Find(x => x.Prefab == furniturePrefab);

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
        float posZ = (furnScript.Size.y % 2 == 0) ? randomY : randomY + 0.5f;

        furnScript.TryPlace(new Vector2(posX, posZ));
    }
    public void DeleteSelected()
{
    if (selectedFurniture == null) return;

    FurnitureInventory item = inventoryList.Find(x => selectedFurniture.name.Contains(x.Prefab.name));

    if (item != null)
    {
        item.CurrentPlacedCount--; 
        UpdateUIText();            
    }

    Destroy(selectedFurniture.gameObject);

    selectedFurniture = null;
    GridSystem.Instance.HideGridVisualizer();
    mouseIndicator.SetSize(1, 1);
}
}
[System.Serializable]
public class FurnitureInventory
{
    public GameObject Prefab;
    public int MaxPlacements;
    [HideInInspector] public int CurrentPlacedCount;
}

