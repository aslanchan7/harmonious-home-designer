using UnityEngine;
using UnityEngine.UI;

public class TooltipDetection : MonoBehaviour
{
    [SerializeField] private Button hoverDetection;
    [SerializeField] private Image segmentToolTip;
    private Color modifier;
    private Vector3 curtain;

    // Displays tooltip / description of energy when hovering over appropriate bar
    public void DisplayTooltip()
    {
        segmentToolTip.GetComponent<RectTransform>().localPosition = new(0f, segmentToolTip.GetComponent<RectTransform>().sizeDelta.y + 15);
        segmentToolTip.gameObject.SetActive(true);
    }

    // Hides tooltip / description of energy when no longer hovering over appropriate bar
    public void HideTooltip()
    {
        segmentToolTip.gameObject.SetActive(false);
    }
}
