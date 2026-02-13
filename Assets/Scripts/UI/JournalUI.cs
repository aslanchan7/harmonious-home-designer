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

    void Start()
    {
        for (int i = 0; i < pagesContainer.transform.childCount; i++)
        {
            pages.Add(pagesContainer.transform.GetChild(i).gameObject);
            pagesContainer.transform.GetChild(i).gameObject.SetActive(false);
        }

        // This ensures the first page is always the first one to open
        pages[0].SetActive(true);
        
        // CloseJournalUI();
    }

    public void OpenJournalUI()
    {
        pagesContainer.SetActive(true);
        background.SetActive(true);

        closeNoteButton.SetActive(true);
        journalBG.SetActive(true);
    }

    public void CloseJournalUI()
    {
        gameObject.SetActive(false);
        MenuButtonsUI.SetActive(true);
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
