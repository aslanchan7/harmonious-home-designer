using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JournalUI : MonoBehaviour
{
    [SerializeField] GameObject notesContainer;
    [SerializeField] Button openNoteButton;
    [SerializeField] GameObject background;
    [SerializeField] GameObject closeNoteButton, prevNoteButton, nextNoteButton;
    private int noteIdx = 0;
    private List<GameObject> notes = new();

    void Start()
    {
        for (int i = 0; i < notesContainer.transform.childCount; i++)
        {
            notes.Add(notesContainer.transform.GetChild(i).gameObject);
        }
        
        CloseNotes();
    }

    public void OpenNotes()
    {
        notesContainer.SetActive(true);
        background.SetActive(true);

        closeNoteButton.SetActive(true);
        prevNoteButton.SetActive(true);
        nextNoteButton.SetActive(true);
    }

    public void CloseNotes()
    {
        notesContainer.SetActive(false);
        background.SetActive(false);

        closeNoteButton.SetActive(false);
        prevNoteButton.SetActive(false);
        nextNoteButton.SetActive(false);
    }

    public void ShowNextNote()
    {
        notes[noteIdx].SetActive(false);
        noteIdx = (noteIdx + 1) % notes.Count;
        notes[noteIdx].SetActive(true);
    }

    public void ShowPrevNote()
    {
        notes[noteIdx].SetActive(false);
        noteIdx = (noteIdx - 1) % notes.Count;
        notes[noteIdx].SetActive(true);
    }
}
