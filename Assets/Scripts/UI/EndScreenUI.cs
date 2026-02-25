using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenUI : MonoBehaviour
{
    [SerializeField] CanvasGroup elementsGroup;
    [SerializeField] CanvasGroup fadePanel;
    [SerializeField] float fadeAnimTime;
    public bool GameContinued;

    public void ContinueGame()
    {
        GameContinued = true;
        elementsGroup.LeanAlpha(0f, fadeAnimTime).setOnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void QuitToMainMenu()
    {
        fadePanel.alpha = 0f;
        fadePanel.LeanAlpha(1f, fadeAnimTime).setOnComplete(() =>
        {
            SceneManager.LoadScene(0);
        });
    }

    void OnEnable()
    {
        fadePanel.alpha = 0f;
        elementsGroup.alpha = 0f;

        elementsGroup.LeanAlpha(1f, fadeAnimTime).setOnComplete(() =>
        {
            gameObject.SetActive(true);
        });
    }
}
