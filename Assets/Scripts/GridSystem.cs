using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private Transform mouseIndicator;
    [SerializeField] private PlayerControls playerControls;
    
    [Header("Layer Mask")]
    public LayerMask PlacementLayer;

    // Temporary
    [SerializeField] private GameObject testFurniturePrefab;
    
    private Vector3 offset = new(0.5f, 0.0f, 0.5f);

    private void Update()
    {
        MoveMouseIndicator(playerControls.MousePos);
    }

    public void MoveMouseIndicator(Vector3 pos)
    {
        Vector3Int gridPos = grid.WorldToCell(pos);
        Vector3 yOffset = new(0.0f, 0.05f, 0.0f); // This is to prevent z-fighting of the MouseIndicator & the floor plane
        mouseIndicator.position = gridPos + offset + yOffset;
    }

    public Vector2 GetGridPos(Vector3 pos)
    {
        Vector3 gridPos = grid.WorldToCell(pos) + offset;
        return new(gridPos.x, gridPos.z);
    }

    // Tester function
    public void CreateTestFurniture()
    {
        // Spawn some test furniture in some empty space on the grid
        
    }
}
