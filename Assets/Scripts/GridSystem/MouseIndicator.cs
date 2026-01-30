using UnityEngine;

public class MouseIndicator : MonoBehaviour
{
    private Vector2 _position;
    private Vector2Int _size;
    [Header("Variables")]
    [Header("References")]
    [SerializeField] PlayerControls playerControls;
    [SerializeField] GridSystem gridSystem;

    public Vector2 Position
    {
        get { return _position; }
        set
        {
            Vector2 clamped = Clamp(value);
            transform.position = new (clamped.x, 0.05f, clamped.y);
            _position = clamped;
        }
    }

    public Vector2Int Size
    {
        get { return _size; }
        set
        {
            Vector2 oldSize = _size;

            // Set scale of mouse indicator
            transform.localScale = new(value.x, value.y, 1);
            _size = value;

            // Re-set position offset
            Position += new Vector2(
                (oldSize.x + value.x) % 2 == 0 ? 0f : 0.5f,
                (oldSize.y + value.y) % 2 == 0 ? 0f : 0.5f
            );
        }
    }

    void Start()
    {
        // Set starting size
        Size = new (1, 1);

        // Set starting position based on grid size and mouseIndicator size 
        Position = new ((gridSystem.Size.x + this.Size.x) % 2 == 0 ? 0f : 0.5f,
                        (gridSystem.Size.y + this.Size.y) % 2 == 0 ? 0f : 0.5f);
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
        float cellSizeX = gridSystem.grid.cellSize.x;
        float cellSizeY = gridSystem.grid.cellSize.z;
        Vector2 deltaPosition = new (
            cellSizeX * Mathf.Round((mousePos.x - Position.x) / cellSizeX),
            cellSizeY * Mathf.Round((mousePos.z - Position.y) / cellSizeY)
        );
        if (deltaPosition != Vector2.zero)
            Position += deltaPosition;
    }

    public void Rotate()
    {
        // Swap x and y of Size
        Size = new(Size.y, Size.x);
    }

    private Vector2 Clamp(Vector2 vector2)
    {
        float maxX = (gridSystem.Size.x - this.Size.x) / 2f;
        float maxY = (gridSystem.Size.y - this.Size.y) / 2f;
        return new (
            Mathf.Clamp(vector2.x, -maxX, maxX),
            Mathf.Clamp(vector2.y, -maxY, maxY)
        );
    }
}
