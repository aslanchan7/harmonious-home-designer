using UnityEngine;

public class CatalogueUI : MonoBehaviour
{
    [SerializeField] GameObject MenuButtonsUI;
    [SerializeField] float fadeOutAnimTime = 0.5f;

    public void CloseCatalogueUI()
    {
        // fade out journal ui
        gameObject.GetComponent<CanvasGroup>().LeanAlpha(0f, fadeOutAnimTime).setOnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        // fade in menu buttons
        MenuButtonsUI.SetActive(true);
        MenuButtonsUI.GetComponent<CanvasGroup>().LeanAlpha(1f, fadeOutAnimTime);
    }
}
