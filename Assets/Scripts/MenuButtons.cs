using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuButtons : MonoBehaviour
{
    [SerializeField] private string _titleSceneName;
    [SerializeField] private string _startGameSceneName;
    [SerializeField] private string _creditsSceneName;

    // Loads the main game scene
    public void Start()
    {
        SceneManager.LoadScene(_startGameSceneName);
    }

    // Exits the game from the main menu
    public void Quit()
    {
        Application.Quit();
    }

    // Loads Credits scene / swaps view to credits
    public void ViewCredits()
    {
        SceneManager.LoadScene(_creditsSceneName);
    }
    public void ReturnTitle()
    {
        SceneManager.LoadScene(_titleSceneName);
    }
}
