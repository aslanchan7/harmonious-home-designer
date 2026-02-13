using UnityEngine;

public class CatalogueUI : MonoBehaviour
{
    [SerializeField] GameObject MenuButtonsUI;

    public void CloseCatalogueUI()
    {
        gameObject.SetActive(false);
        MenuButtonsUI.SetActive(true);
    }
}
