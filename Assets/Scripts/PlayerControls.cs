using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerControls : MonoBehaviour
{
    [HideInInspector] public Vector3 MousePos;
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
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, -0.5f)
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
        mouseIndicator.Size = new (1, 1);
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
        
        if ((selectedFurniture.Size.x + selectedFurniture.Size.y) % 2 == 0)
        {
            mouseIndicator.Rotate();
            selectedFurniture.SetLocationAsValid();
            return;
        }

        Vector2 currentPosition = selectedFurniture.LastValidPosition;
        bool rotateSuccess = false;
        selectedFurniture.SetColliderEnabled(false);
        for (int i = 0; i < 4; i++)
        {
            Vector2 testPosition = currentPosition + rotationSnapOffset[
                (rotateSnapOffsetIndex + i) % 4
            ];
            selectedFurniture.DisplayPosition = testPosition;
            if (selectedFurniture.CheckValidPos())
            {
                rotateSuccess = true;
                // Make sure that if the furniture is rotated consecutively,
                // it would favor the offset direction opposite to the
                // current offset
                rotateSnapOffsetIndex = (rotateSnapOffsetIndex + i + 2) % 4;
                mouseIndicator.Rotate();
                selectedFurniture.SetColliderEnabled(true);
                selectedFurniture.SetLocationAsValid();
                break;
            }
        }

        if (!rotateSuccess)
        {
            selectedFurniture.SetColliderEnabled(true);
            selectedFurniture.ResetToValidLocation();
        }
    }

    public void RaycastMouse()
    {
        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(
            ray,
            out hit,
            float.PositiveInfinity,
            GridSystem.Instance.PlacementLayer
        ))
        {
            MousePos = hit.point;
        }
    }
}