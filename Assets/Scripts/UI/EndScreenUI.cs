using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenUI : MonoBehaviour
{
    [SerializeField] CanvasGroup elementsGroup;
    [SerializeField] CanvasGroup fadePanel;
    [SerializeField] float fadeAnimTime;

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
