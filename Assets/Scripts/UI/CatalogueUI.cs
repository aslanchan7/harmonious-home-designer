using System;
using System.Collections.Generic;
using UnityEngine;

public class CatalogueUI : MonoBehaviour
{
    [SerializeField] GameObject pagesContainer;
    [SerializeField] GameObject background;
    [SerializeField] RectTransform closeNoteButton;
    [SerializeField] GameObject MenuButtonsUI;
    [SerializeField] Vector2 CloseButtonCoverPos, CloseButtonOpenPos;
    private int noteIdx = 0;
    private List<GameObject> pages = new();

    [Header("Animation Settings")]
    [SerializeField] float fadeOutAnimTime = 0.5f;

    void Start()
    {
        for (int i = 0; i < pagesContainer.transform.childCount; i++)
        {
            pages.Add(pagesContainer.transform.GetChild(i).gameObject);
            pagesContainer.transform.GetChild(i).gameObject.SetActive(false);
        }

        // This ensures the first page is always the first one to open
        pages[0].SetActive(true);
    }

    public void CloseCatalogueUI()
    {
        // fade out catalogue ui
        gameObject.GetComponent<CanvasGroup>().LeanAlpha(0f, fadeOutAnimTime).setOnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        UISFX.Play(SFXAction.UI_Close);

        // fade in menu buttons
        MenuButtonsUI.SetActive(true);
        MenuButtonsUI.GetComponent<CanvasGroup>().LeanAlpha(1f, fadeOutAnimTime);
    }

    public void ShowNextPage()
    {
        pages[noteIdx].SetActive(false);
        noteIdx++;

        UISFX.Play(SFXAction.UI_PageForward);

        pages[noteIdx].SetActive(true);

        // UpdateCloseButtonPos();
    }

    public void ShowPrevPage()
    {
        pages[noteIdx].SetActive(false);
        noteIdx--;

        UISFX.Play(SFXAction.UI_PageBack);

        pages[noteIdx].SetActive(true);

        // UpdateCloseButtonPos();
    }

    void Update()
    {
        UpdateCloseButtonPos();
    }

    private void UpdateCloseButtonPos()
    {
        closeNoteButton.localPosition = noteIdx == 0 ? CloseButtonCoverPos : CloseButtonOpenPos;
    }
}
