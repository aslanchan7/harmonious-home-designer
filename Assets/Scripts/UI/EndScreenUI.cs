using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenUI : MonoBehaviour
{
    public static EndScreenUI Instance;
    [SerializeField] CanvasGroup elementsGroup;
    [SerializeField] CanvasGroup fadePanel;
    [SerializeField] float fadeAnimTime;

    void Awake()
    {
        // TODO: Make singleton
    }

    public void ContinueGame()
    {
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
}
