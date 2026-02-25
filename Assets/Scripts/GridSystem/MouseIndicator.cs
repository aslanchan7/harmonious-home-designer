using UnityEngine;

public class MouseIndicator : MonoBehaviour
{
    private Vector2 _position;
    private float _elevation;
    private Vector2Int _size;

    private Vector2Int _lastSize;
    private Vector2 _lastMouseTileCenter;

    [Header("Variables")]
    [Header("References")]
    [SerializeField]
    PlayerControls playerControls;

    [SerializeField]
    GridSystem gridSystem;

    public Vector2 Position
    {
        get { return _position; }
        set
        {
            Vector2 clamped = Clamp(value);
            transform.position = new(
                clamped.x,
                transform.position.y,
                clamped.y
            );
            _position = clamped;
        }
    }

    public float Elevation
    {
        get { return _elevation; }
        set
        {
            transform.position = new(
                transform.position.x,
                value + 0.05f,
                transform.position.z
            );
            _elevation = value;
        }
    }

    public Vector2Int Size
    {
        get { return _size; }
        set
        {
            // Set scale of mouse indicator
            transform.localScale = new(value.x, value.y, 1);
            _size = value;
        }
    }

    void Start()
    {
        // Set starting size
        Size = new(1, 1);
        _lastSize = Size;

        // Set starting position based on grid size and mouseIndicator size
        Position = new(
            (gridSystem.Size.x + _size.x) % 2 == 0 ? 0f : 0.5f,
            (gridSystem.Size.y + _size.y) % 2 == 0 ? 0f : 0.5f
        );
        _lastMouseTileCenter = Position;

        Elevation = 0;
    }

    void Update()
    {
        // Move mouse based on mousePos in playerControls
        MoveMouseIndicator();
    }

    public void MoveMouseIndicator()
    {
        Furniture selectedFurniture = playerControls.SelectedFurniture;
        Furniture hoverFurniture = playerControls.HoverFurniture;
        Vector2 mouseTileCenter = playerControls.MouseTileCenter;

        // If is dragging
        if (selectedFurniture != null)
        {
            if (mouseTileCenter != _lastMouseTileCenter)
                Position += mouseTileCenter - _lastMouseTileCenter;

            // Check for rotation
            if (Size != _lastSize)
            {
                // Rotate 90deg clockwise around the mouse's tile.
                Vector2 mouseToFurniture = Position - mouseTileCenter;
                Vector2 rotatedMouseToFurniture = new(
                    mouseToFurniture.y,
                    -mouseToFurniture.x
                );
                Position += rotatedMouseToFurniture - mouseToFurniture;
            }
        }
        else if (hoverFurniture != null)
        {
            StickToFurniture(hoverFurniture);
        }
        else
        {
            ResetSizeAndPosition();
        }

        _lastSize = Size;
        _lastMouseTileCenter = mouseTileCenter;
    }

    public void SetSizeAndPosition(Vector2Int newSize, Vector2 newPosition)
    {
        Size = newSize;
        Position = newPosition;
    }

    public void ResetSizeAndPosition()
    {
        Size = Vector2Int.one;
        Position = playerControls.MouseTileCenter;
    }

    // Furniture should not be null
    public void StickToFurniture(Furniture furniture)
    {
        if (furniture.DisplayPosition != Position)
            Position = furniture.DisplayPosition;

        if (furniture.DisplayElevation != Elevation)
            Elevation = furniture.DisplayElevation;

        if (furniture.Size != Size)
            Size = furniture.Size;
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
        return new(
            Mathf.Clamp(vector2.x, -maxX, maxX),
            Mathf.Clamp(vector2.y, -maxY, maxY)
        );
    }
}
