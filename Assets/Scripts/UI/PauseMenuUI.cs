using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PauseMenuUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    [SerializeField] CanvasGroup fadeOutPanel;
    [SerializeField] float fadeAnimTime;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();   
    }

    public void ResumeGame()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.LeanAlpha(0f, fadeAnimTime).setOnComplete(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            gameObject.SetActive(false);
        });
    }

    public void ReturnToMainMenu()
    {
        fadeOutPanel.alpha = 0f;
        fadeOutPanel.LeanAlpha(1f, fadeAnimTime).setOnComplete(() =>
        {
            SceneManager.LoadScene(0);
        });
    }

    void OnEnable()
    {
        if(canvasGroup == null) return;
        canvasGroup.alpha = 0f;
        canvasGroup.LeanAlpha(1f, fadeAnimTime);
    }
}
