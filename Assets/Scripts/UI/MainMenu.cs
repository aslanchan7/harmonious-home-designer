using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject credits;
    [SerializeField] CanvasGroup fadeOutPanel;

    [Header("Settings")]
    [SerializeField] float fadeOutAnimTime = 0.5f;
    [SerializeField] float creditsFadeAnimTime = 0.5f;

    void Start()
    {
        mainMenu.SetActive(true);
        credits.SetActive(false);
    }

    public void StartGame()
    {
        // Fade Out Animation
        fadeOutPanel.alpha = 0f;
        fadeOutPanel.LeanAlpha(1f, fadeOutAnimTime).setOnComplete(() =>
        {
            SceneManager.LoadScene(1);
        });
    }

    public void OpenCredits()
    {
        // Intialize initial values
        mainMenu.GetComponent<CanvasGroup>().alpha = 1f;
        credits.SetActive(true);
        credits.GetComponent<CanvasGroup>().alpha = 0f;

        // Tween alphas
        mainMenu.GetComponent<CanvasGroup>().LeanAlpha(0f, creditsFadeAnimTime).setOnComplete(() =>
        {
            mainMenu.SetActive(false);
        });
        credits.GetComponent<CanvasGroup>().LeanAlpha(1f, creditsFadeAnimTime);
    }

    public void QuitCredits()
    {
        // Intialize initial values
        credits.GetComponent<CanvasGroup>().alpha = 1f;
        mainMenu.SetActive(true);
        mainMenu.GetComponent<CanvasGroup>().alpha = 0f;

        // Tween alphas
        credits.GetComponent<CanvasGroup>().LeanAlpha(0f, creditsFadeAnimTime).setOnComplete(() =>
        {
            credits.SetActive(false);
        });
        mainMenu.GetComponent<CanvasGroup>().LeanAlpha(1f, creditsFadeAnimTime);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
