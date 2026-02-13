using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject OpenSubMenuButton;
    [SerializeField] List<Sprite> OpenSubMenuButtonSprites = new(2);
    [SerializeField] GameObject SubMenuButtons;
    [SerializeField] GameObject InventoryUI;
    [SerializeField] GameObject JournalUI;
    [SerializeField] GameObject CatalogueUI;

    public void ToggleSubMenuButtons()
    {
        SubMenuButtons.SetActive(!SubMenuButtons.activeSelf);
        OpenSubMenuButton.GetComponent<Image>().sprite = SubMenuButtons.activeSelf ? OpenSubMenuButtonSprites[0] : OpenSubMenuButtonSprites[1];
    }

    public void OpenInventoryUI()
    {
        InventoryUI.SetActive(true);
        gameObject.SetActive(false);

        Vector3 newCamPos = new(Camera.main.transform.position.x, Camera.main.transform.position.y + 2f, Camera.main.transform.position.z);
        Camera.main.transform.position = newCamPos;
    }

    public void OpenJournalUI()
    {
        JournalUI.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OpenCatalogueUI()
    {
        CatalogueUI.SetActive(true);
        gameObject.SetActive(false);
    }
}
