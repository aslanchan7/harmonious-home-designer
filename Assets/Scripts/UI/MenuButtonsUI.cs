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

    [Header("Animation Settings")]
    [SerializeField] float fadeAnimTime = 1f;
    [SerializeField] float inventoryAnimTime = 0.5f;
    [SerializeField] float journalAnimTime = 0.5f;
    [SerializeField] float catalogueAnimTime = 0.5f;

    public void ToggleSubMenuButtons()
    {
        // SubMenuButtons.SetActive(!SubMenuButtons.activeSelf);
        if(SubMenuButtons.activeSelf)
        {
            StartHideSubMenuButtonsAnim();
        } else
        {
            StartShowSubMenuButtonsAnim();
            UISFX.Play(SFXAction.UI_BoxOpen);
        }
    }

    public void OpenInventoryUI()
    {
        // fade in inventoryUI
        InventoryUI.GetComponent<CanvasGroup>().alpha = 0f;
        InventoryUI.SetActive(true);
        InventoryUI.GetComponent<CanvasGroup>().LeanAlpha(1f, inventoryAnimTime);
        
        UISFX.Play(SFXAction.UI_Open);

        // simultaneously zoom out camera
        Camera.main.transform.LeanMoveLocalY(Camera.main.transform.position.y + 2f, inventoryAnimTime);
        
        FadeOutMenuButtonsUI(inventoryAnimTime);
    }

    public void OpenJournalUI()
    {
        // fade in journal ui
        JournalUI.GetComponent<CanvasGroup>().alpha = 0f;
        JournalUI.SetActive(true);
        JournalUI.GetComponent<CanvasGroup>().LeanAlpha(1f, journalAnimTime);
        
        UISFX.Play(SFXAction.UI_OpenBook);
        
        FadeOutMenuButtonsUI(journalAnimTime);
    }

    public void OpenCatalogueUI()
    {
        // fade in catalogue ui
        CatalogueUI.GetComponent<CanvasGroup>().alpha = 0f;
        CatalogueUI.SetActive(true);
        CatalogueUI.GetComponent<CanvasGroup>().LeanAlpha(1f, catalogueAnimTime);

        UISFX.Play(SFXAction.UI_Open);

        FadeOutMenuButtonsUI(catalogueAnimTime);
    }

    private void StartShowSubMenuButtonsAnim()
    {
        SubMenuButtons.GetComponent<CanvasGroup>().alpha = 0f;
        SubMenuButtons.SetActive(true);
        OpenSubMenuButton.GetComponent<Image>().sprite = OpenSubMenuButtonSprites[0];
        SubMenuButtons.GetComponent<CanvasGroup>().LeanAlpha(1f, fadeAnimTime);
    }

    private void StartHideSubMenuButtonsAnim()
    {
        OpenSubMenuButton.GetComponent<Image>().sprite = OpenSubMenuButtonSprites[1];
        SubMenuButtons.GetComponent<CanvasGroup>().LeanAlpha(0f, fadeAnimTime).setOnComplete(() =>
        {
            SubMenuButtons.SetActive(false);
        });
    }

    private void FadeOutMenuButtonsUI(float time)
    {
        gameObject.GetComponent<CanvasGroup>().LeanAlpha(0f, time).setOnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
