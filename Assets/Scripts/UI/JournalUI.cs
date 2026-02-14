using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JournalUI : MonoBehaviour
{
    [SerializeField] GameObject pagesContainer;
    [SerializeField] GameObject background, journalBG;
    [SerializeField] GameObject closeNoteButton;
    [SerializeField] GameObject MenuButtonsUI;
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

    public void CloseJournalUI()
    {
        // fade out journal ui
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
        pages[noteIdx].SetActive(true);
    }

    public void ShowPrevPage()
    {
        pages[noteIdx].SetActive(false);
        noteIdx--;
        pages[noteIdx].SetActive(true);
    }
}
