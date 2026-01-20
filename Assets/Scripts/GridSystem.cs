using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance;

    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject gridVisualizer;
    public Transform mouseIndicator;
    [SerializeField] private PlayerControls playerControls;
    
    [Header("Layer Mask")]
    public LayerMask PlacementLayer;

    [Header("Grid Settings")]
    [SerializeField] private Vector2Int defaultScale = new(10, 10);
    [HideInInspector] public Vector2Int Size;

    // Temporary
    [SerializeField] private GameObject testFurniturePrefab;
    
    private Vector3 centerCellOffset = new(0.5f, 0.0f, 0.5f);

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }

        Instance = this;
    }

    private void Start()
    {
        // If grid size is even then set offset; otherwise set offset = 0;
        centerCellOffset.x = Size.x % 2 == 0 ? 0.5f : 0.0f;
        centerCellOffset.z = Size.y % 2 == 0 ? 0.5f : 0.0f;

        // Set Grid Size
        Size = new((int)(transform.localScale.x * defaultScale.x), (int)(transform.localScale.z * defaultScale.y));

        // Initially hide grid visualizer
        HideGridVisualizer();
    }

    private void Update()
    {
        MoveMouseIndicator(playerControls.MousePos);
    }

    public void MoveMouseIndicator(Vector3 pos)
    {
        Vector3Int gridPos = grid.WorldToCell(pos);
        Vector3 yOffset = new(0.0f, 0.05f, 0.0f); // This is to prevent z-fighting of the MouseIndicator & the floor plane
        mouseIndicator.position = gridPos + centerCellOffset + yOffset;
    }

    public Vector2Int GetGridPosFromWorldPos(Vector3 pos)
    {
        Vector3Int gridPos = grid.WorldToCell(pos);
        return new(gridPos.x, gridPos.z);
    }

    public Vector3 GetWorldPosFromGridPos(Vector2Int pos)
    {
        Vector3Int gridPos = new(pos.x, 0, pos.y);
        Vector3 worldPos = grid.CellToWorld(gridPos) + centerCellOffset;
        return worldPos;
    }

    public bool CheckForEmptyGridPos(Vector2Int gridPos)
    {
        // Raycast to gridPos and check if hit furniture; if hit then return false, else return true
        Vector3 worldPos = GetWorldPosFromGridPos(gridPos);
        worldPos.y = 10000; // Set to high number
        if (Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit, float.PositiveInfinity))
        {
            // TODO: Check if able to place on hit object (for now, we assume objects can only place on floor)
            return hit.collider.CompareTag("Floor"); 
        }

        return true;
    }

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
        Vector2Int cellRangeX = new(-Size.x/2, Size.x/2);
        Vector2Int cellRangeY = new(-Size.y/2, Size.y/2);

        for (int i = cellRangeX.x; i < cellRangeX.y; i++)
        {
            for (int j = cellRangeY.x; j < cellRangeX.y; j++)
            {
                Vector2Int gridPos = new(i, j);

                if(CheckForEmptyGridPos(gridPos))
                {
                    Vector3 spawnPos = GetWorldPosFromGridPos(gridPos);

                    Instantiate(testFurniturePrefab, spawnPos, Quaternion.identity);

                    return;
                }
            }
        }
    }
}
