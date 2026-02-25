using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance;

    [Header("References")]
    [HideInInspector]
    public Grid grid;

    [SerializeField]
    private GameObject gridVisualizer;
    public Transform mouseIndicator;

    [SerializeField]
    private PlayerControls playerControls;

    [Header("Layer Mask")]
    public LayerMask PlacementLayer;

    [Header("Grid Settings")]
    // [SerializeField] private Vector2Int defaultScale = new(10, 10);
    public Vector2Int Size;

    // Temporary
    [SerializeField]
    private GameObject testFurniturePrefab;

    public Vector3 CenterCellOffset = new(0.5f, 0.0f, 0.5f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }

        Instance = this;
    }

    private void Start()
    {
        // Check Size is consistent with gridVisualizer
        if (
            Mathf.Abs(Size.x - gridVisualizer.transform.localScale.x * 10) > 0.1
            || Mathf.Abs(Size.y - gridVisualizer.transform.localScale.z * 10)
                > 0.1
        )
        {
            Debug.LogError("GridSystem \"Size\" does match GridVisualizer");
        }

        // If grid size is even then set offset; otherwise set offset = 0;
        CenterCellOffset.x = Size.x % 2 == 0 ? 0.5f : 0.0f;
        CenterCellOffset.z = Size.y % 2 == 0 ? 0.5f : 0.0f;

        // Initially hide grid visualizer
        HideGridVisualizer();
    }

    public Vector2Int GetGridPosFromWorldPos(Vector3 pos)
    {
        // if grid is odd, pos + 0.5
        pos.x += 0.5f - CenterCellOffset.x;
        pos.z += 0.5f - CenterCellOffset.z;

        Vector3Int gridPos = grid.WorldToCell(pos);
        return new(gridPos.x, gridPos.z);
    }

    public Vector3 GetWorldPosFromGridPos(Vector2Int pos)
    {
        Vector3Int gridPos = new(pos.x, 0, pos.y);
        Vector3 worldPos = grid.CellToWorld(gridPos) + CenterCellOffset;
        return worldPos;
    }

    public void HideGridVisualizer()
    {
        gridVisualizer.SetActive(false);
    }

    public void ShowGridVisualizer()
    {
        gridVisualizer.SetActive(true);
    }

    public void WorldBoxToIndices(
        BoundingBox boundingBox,
        out Vector2Int starting,
        out Vector2Int ending
    )
    {
        Vector2 offset = (Vector2)Size / 2.0f;
        starting = Vector2Int.RoundToInt(boundingBox.position + offset);
        ending = Vector2Int.RoundToInt(boundingBox.opposite + offset);
    }

    public Vector2Int WorldToIndex(Vector2 position)
    {
        return Vector2Int.RoundToInt(position + (Vector2)Size / 2.0f);
    }

    public bool WorldBoxInBound(BoundingBox box)
    {
        Vector2 halfSize = (Vector2)Size / 2;
        return -halfSize.x - 0.01f <= box.x
            && box.oppositeX <= halfSize.x + 0.01f
            && -halfSize.y - 0.01f <= box.y
            && box.oppositeY <= halfSize.y + 0.01f;
    }

    public bool IndexInBound(Vector2Int vector)
    {
        return 0 <= vector.x
            && vector.x < Size.x
            && 0 <= vector.y
            && vector.y < Size.y;
    }
}
