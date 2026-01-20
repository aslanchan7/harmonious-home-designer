using UnityEngine;
using UnityEngine.InputSystem;
using System;

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

    [Header("References")]
    [SerializeField] private GridSystem gridSystem;

    [Header("Actions")]
    private InputAction mousePositionAction;
    [SerializeField] private InputActionReference clickAction;
    [SerializeField] private InputActionReference rightClickAction;
    [SerializeField] private InputActionReference rotateAction;

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
        // MoveGhostFurniture();
        // mouseIndicator.transform.position = mousePos;
        // gridSystem.MoveMouseIndicator(mousePos);
    }

    private void OnClick(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Left-Clicked!");
        // furnitureGhost.GetComponent<Collider>().enabled = true;
        // CreateFurnitureGhost();
        // TODO: Integrate placement mechanics with GridSystem and Furniture.
    }

    private void OnRightClick(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Right-clicked!");
        Debug.Log(gameObjectPointedAt);
        // if (gameObjectPointedAt.CompareTag("Furniture"))
        // {
        //     Destroy(gameObjectPointedAt);
        // }
        // TODO: Integrate removal mechanics with GridSystem and Furniture.
    }

    // TODO: 

    private void OnRotate(InputAction.CallbackContext callbackContext)
    {
        // furnitureGhost.SetRotation(furnitureGhost.transform.eulerAngles.y + 90);
        // TODO: access current furniture 
    }

    public void RaycastMouse()
    {
        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity, gridSystem.PlacementLayer))
        {
            MousePos = hit.point;
            normalDirection = hit.normal;
            gameObjectPointedAt = hit.collider.gameObject;

            // if (NormalIsAlongY())
            // {
            //     // tilePointedAt = GetTilePosition(hit.point);
            //     tilePointedAt = gridSystem.GetGridPos(hit.point);
            // }
            // else
            // {
            //     tilePointedAt = gridSystem.GetGridPos(hit.point - normalDirection / 2);
            // }
        }
        // Debug.Log(tilePointedAt);
    }

    /*
    private void CreateFurnitureGhost()
    {
        furnitureGhost = Instantiate(furniturePrefab);
        furnitureGhost.GetComponent<Collider>().enabled = false;
        MoveGhostFurniture();
    }

    private void MoveGhostFurniture()
    {
        Vector2 gridPos = gridSystem.GetGridPos(mousePos);
        furnitureGhost.SetPosition(gridPos);
    }
    */

    /*
    private void MoveFurnitureGhostToMouse()
    {
        if (NormalIsAlongY())
        {
            // Make sure that the mouse is pointing at the ground.
            if (Mathf.Abs(mousePos.y) < 1E-12)
            {
                furnitureGhost.SetPosition(
                    new Vector2(
                        tilePointedAt.x + 0.5f,
                        tilePointedAt.y + 0.5f
                    )
                );
            }
        }
        else
        {
            furnitureGhost.SetPosition(
                new Vector2(
                    tilePointedAt.x + 0.5f,
                    tilePointedAt.y + 0.5f
                ) + new Vector2(
                    normalDirection.x,
                    normalDirection.z
                )
            );
        }
    }

    public Vector2 GetTilePosition(Vector3 point)
    {
        // TODO: Integrate with GridSystem
        return new Vector2(MathF.Floor(point.x), MathF.Floor(point.z));
    }
    */

    public bool NormalIsAlongY()
    {
        return normalDirection == Vector3.up || normalDirection == Vector3.down;
    }
}