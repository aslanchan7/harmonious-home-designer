using UnityEngine;

public class Furniture : MonoBehaviour
{
    public string furnitureName;
    public Vector2 LastValidPos;
    [Header("References")]
    public MeshRenderer MeshRenderer;
    public Material NormalMat, GhostMat, InvalidGhostMat;

    private void Start()
    {
        LastValidPos = new(transform.position.x, transform.position.z);
    }

    // placing furniture on the xz-plane
    public void SetPosition(Vector2 position)
    {
        this.transform.position = new Vector3(position.x, this.transform.position.y, position.y);
        // TODO snap y-position based on objects caught in downward raycast
        // ^ grid system subtask, i believe
        LastValidPos = position;
    }

    // rotating furniture on the y-axis
    public void SetRotation(float rotation)
    {
        this.transform.eulerAngles = new Vector3(transform.eulerAngles.x, rotation, transform.eulerAngles.z);
    }

    public void MoveGhost(Vector2 position)
    {
        // Visually move ghost furniture
        transform.position = new Vector3(position.x, transform.position.y, position.y);
        
        // Check if position is a valid pos for the object to move
        Vector2Int gridPos = GridSystem.Instance.GetGridPosFromWorldPos(new(position.x, 0, position.y));
        bool valid = GridSystem.Instance.CheckForEmptyGridPos(gridPos);
        MeshRenderer.material = valid ? GhostMat : InvalidGhostMat;
    }

    public void TryPlace(Vector2 position)
    {
        Vector2Int gridPos = GridSystem.Instance.GetGridPosFromWorldPos(new(position.x, 0, position.y));
        bool valid = GridSystem.Instance.CheckForEmptyGridPos(gridPos);
        if(valid)
        {
            SetPosition(position);
        } else
        {
            SetPosition(LastValidPos);
        }
        SetNormalMat();
    }

    public void SetGhostMaterial()
    {
        MeshRenderer.material = GhostMat;
        GetComponent<Collider>().enabled = false;
    }

    public void SetNormalMat()
    {
        MeshRenderer.material = NormalMat;
        GetComponent<Collider>().enabled = true;
    }

    public void SetInvalidGhostMat()
    {
        MeshRenderer.material = InvalidGhostMat;
    }

    // void Update()
    // {
    //     // TESTING SETPOSITION & SETROTATION, TODO: REMOVE AFTER PLAYER INPUTS IMPLEMENTED -----------------------------
    //     // -- testing SetPosition
    //     if (Input.GetKeyDown(KeyCode.A))
    //         SetPosition(new Vector2(transform.position.x - 1f, transform.position.z));
    //     if (Input.GetKeyDown(KeyCode.D))
    //         SetPosition(new Vector2(transform.position.x + 1f, transform.position.z));
    //     if (Input.GetKeyDown(KeyCode.W))
    //         SetPosition(new Vector2(transform.position.x, transform.position.z + 1f));
    //     if (Input.GetKeyDown(KeyCode.S))
    //         SetPosition(new Vector2(transform.position.x, transform.position.z - 1f));
    //     // -- testing SetRotation
    //     if (Input.GetKeyDown(KeyCode.R))
    //         SetRotation(transform.eulerAngles.y + 90f);
    //     // -------------------------------------------------------------------------------------------------------------
    // }
}
