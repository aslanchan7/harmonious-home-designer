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

    // public bool ValidPosForFurniture(Furniture furniture, Vector2 pos)
    // {
    //     for (int i = 0; i < furniture.Size.x; i++)
    //     {
    //         for (int j = 0; j < furniture.Size.y; j++)
    //         {
    //             // Raycast at some position based on gridPos and i and j
    //             Vector2Int gridPos = new((int)(pos.x - furniture.Size.x / 2.0f + i), (int)(pos.y - furniture.Size.y / 2.0f + j));
    //             Vector3 worldPos = GetWorldPosFromGridPos(gridPos);
    //             // Vector3 worldPos  = GetWorldPosFromGridPos(gridPos + new Vector2Int(i, j));
    //             worldPos.y = 10000;
    //             if (Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit, float.PositiveInfinity))
    //             {
    //                 // TODO: Check if able to place on hit object (for now, we assume objects can only place on floor)
    //                 if (!hit.collider.CompareTag("Floor")) return false;
    //             }
    //             else
    //             {
    //                 return false;
    //             }
    //         }
    //     }

    //     return true;
    // }

    public void HideGridVisualizer()
    {
        gridVisualizer.SetActive(false);
    }

    public void ShowGridVisualizer()
    {
        gridVisualizer.SetActive(true);
    }

    // Tester function
    public void CreateTestFurniture()
    {
        // for (int i = -Size.x / 2; i < Size.x / 2; i++)
        // {
        //     for (int j = -Size.y / 2; j < Size.y / 2; j++)
        //     {
        //         Vector2Int gridPos = new(i, j);

        //         if (ValidPosForFurniture(testFurniturePrefab.GetComponent<Furniture>(), gridPos))
        //         {
        //             GameObject instantiated = Instantiate(testFurniturePrefab);
        //             instantiated.GetComponent<Furniture>().TryPlace(gridPos);

        //             return;
        //         }
        //     }
        // }
    }
}
