using UnityEngine;
using UnityEngine.UI;

public class TooltipDetection : MonoBehaviour
{
    [SerializeField] private Button hoverDetection;
    [SerializeField] private RectTransform tooltip;

    // Displays tooltip / description of energy when hovering over appropriate bar
    public void DisplayTooltip()
    {
        tooltip.gameObject.SetActive(true);
    }

    // Hides tooltip / description of energy when no longer hovering over appropriate bar
    public void HideTooltip()
    {
        tooltip.gameObject.SetActive(false);
    }
}
