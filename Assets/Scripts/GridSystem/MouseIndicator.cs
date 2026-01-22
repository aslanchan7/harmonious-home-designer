using UnityEngine;

public class MouseIndicator : MonoBehaviour
{
    [Header("Variables")]
    public Vector2Int Size;
    public Vector3 Pos;

    [Header("References")]
    [SerializeField] PlayerControls playerControls;
    [SerializeField] GridSystem gridSystem;

    void Start()
    {
        // Set starting size
        transform.localScale = new(Size.x, Size.y, 1f);

        // Set starting position based on grid size and mouseIndicator size 
        Pos.x = (gridSystem.Size.x + this.Size.x) % 2 == 0 ? 0f : 0.5f;
        Pos.z = (gridSystem.Size.y + this.Size.y) % 2 == 0 ? 0f : 0.5f;
        Pos.y = 0.05f;
    }

    void Update()
    {
        // Move mouse based on mousePos in playerControls
        MoveMouseIndicator(playerControls.MousePos);
    }

    public void MoveMouseIndicator(Vector3 mousePos)
    {
        // We want to move the mouseIndicator on the x and z axis independently
        // mousePos is the position of the player's cursor in world space

        Pos.x += mousePos.x > Pos.x + (gridSystem.grid.cellSize.x / 2f) ? 1f : 0f;
        Pos.x -= mousePos.x < Pos.x - (gridSystem.grid.cellSize.x / 2f) ? 1f : 0f;
        Pos.x = Mathf.Clamp(Pos.x, (-gridSystem.Size.x + this.Size.x) / 2f, (gridSystem.Size.x - this.Size.x) / 2f);

        Pos.z += mousePos.z > Pos.z + (gridSystem.grid.cellSize.z / 2f) ? 1f : 0f;
        Pos.z -= mousePos.z < Pos.z - (gridSystem.grid.cellSize.z / 2f) ? 1f : 0f;
        Pos.z = Mathf.Clamp(Pos.z, (-gridSystem.Size.y + this.Size.y) / 2f, (gridSystem.Size.y - this.Size.y) / 2f);

        transform.position = Pos;
    }

    public void SetSize(int x, int y)
    {
        // Re-set position offset
        Pos.x += (this.Size.x + x) % 2 == 0 ? 0f : 0.5f;
        Pos.z += (this.Size.y + y) % 2 == 0 ? 0f : 0.5f;
        Pos.y = 0.05f;
        transform.position = Pos;

        transform.localScale = new(x, y, 1f);
        Size = new(x, y);

        // Re-clamp position
        Pos.x = Mathf.Clamp(Pos.x, (-gridSystem.Size.x + this.Size.x) / 2f, (gridSystem.Size.x - this.Size.x) / 2f);
        Pos.z = Mathf.Clamp(Pos.z, (-gridSystem.Size.y + this.Size.y) / 2f, (gridSystem.Size.y - this.Size.y) / 2f);

    }
}
