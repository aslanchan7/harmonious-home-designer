using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;

// enum RotateDirection
// {
//     Left = -1,
//     Right = 1,
// }

public class PlayerControls : MonoBehaviour
{
    [HideInInspector] public Vector3 MousePos;
    // private Vector2 tilePointedAt;
    private Vector3 normalDirection;
    private GameObject gameObjectPointedAt;
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
    [SerializeField] private InputActionReference rightClickAction;
    [SerializeField] private InputActionReference rotateAction;

    [Header("References")]
    [SerializeField] private MouseIndicator mouseIndicator;

    // private WaitForFixedUpdate waitForFixedUpdate = new();

    private void Start()
    {
        mousePositionAction = InputSystem.actions.FindAction("Point");
        RaycastMouse();
        // CreateFurnitureGhost();
    }

    private void OnEnable()
    {
        clickAction.action.performed += OnClick;
        rightClickAction.action.performed += OnRightClick;
        rotateAction.action.performed += OnRotate;
    }

    private void OnDisable()
    {
        clickAction.action.performed -= OnClick;
        rightClickAction.action.performed -= OnRightClick;
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
        selectedFurniture.SetGhostMaterial();
        mouseIndicator.SetSize(selectedFurniture.Size.x, selectedFurniture.Size.y);
        Vector2 pos = new(mouseIndicator.transform.position.x, mouseIndicator.transform.position.z);

        while (clickAction.action.ReadValue<float>() != 0)
        {
            pos = new(GridSystem.Instance.mouseIndicator.transform.position.x, GridSystem.Instance.mouseIndicator.transform.position.z);
            Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity, GridSystem.Instance.PlacementLayer))
            {
                selectedFurniture.MoveGhost(pos);
            }

            yield return null;
        }

        mouseIndicator.SetSize(1, 1);
        selectedFurniture.TryPlace(pos);
        selectedFurniture.SetNormalMat();
    }

    private void OnRightClick(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("This is not implemented! Also right-click is mapped to rotate.");
        Debug.Log(gameObjectPointedAt);
    }

    private void OnRotate(InputAction.CallbackContext callbackContext)
    {
        if (selectedFurniture != null)
        {
            selectedFurniture.SetRotation(selectedFurniture.transform.eulerAngles.y + 90);
            mouseIndicator.Rotate();
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
            gameObjectPointedAt = hit.collider.gameObject;
        }
    }
    
    public bool NormalIsAlongY()
    {
        return normalDirection == Vector3.up || normalDirection == Vector3.down;
    }
}