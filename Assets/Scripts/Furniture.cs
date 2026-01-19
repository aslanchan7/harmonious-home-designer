using UnityEngine;

public class Furniture : MonoBehaviour
{
    public string furnitureName;

    // placing furniture on the xz-plane
    public void SetPosition(Vector2 position)
    {
        this.transform.position = new Vector3(position.x, this.transform.position.y, position.y);
        // TODO snap y-position based on objects caught in downward raycast
        // ^ grid system subtask, i believe
    }

    // rotating furniture on the y-axis
    public void SetRotation(float rotation)
    {
        this.transform.eulerAngles = new Vector3(transform.eulerAngles.x, rotation, transform.eulerAngles.z);
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
