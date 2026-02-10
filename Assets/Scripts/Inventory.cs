using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject inventoryPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("tab"))
        {
            if (inventoryPanel != null)
            {
                bool currentState = inventoryPanel.activeSelf;
                inventoryPanel.SetActive(!currentState);
            }
        }
    }
}
