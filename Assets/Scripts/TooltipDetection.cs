using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UI;
using UnityEngine.UIElements;
public class TooltipDetection : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button hoverDetection;
    [SerializeField] private UnityEngine.UI.Image segmentToolTip;
    private Color modifier;
    private UnityEngine.Vector3 curtain;

// Displays tooltip / description of energy when hovering over appropriate bar
    public void displayTooltip()
    {
        Debug.Log("mouse entered button");
        modifier = segmentToolTip.color;
        modifier.a = 1;
        curtain = segmentToolTip.GetComponent<RectTransform>().position;
        curtain.y = curtain.y - 500;
        segmentToolTip.color = modifier;
        segmentToolTip.GetComponent<RectTransform>().position = curtain;
    }

// Hides tooltip / description of energy when no longer hovering over appropriate bar
    public void hideTooltip()
    {
        Debug.Log("mouse exited button");
        modifier = segmentToolTip.color;
        modifier.a = 0;
        curtain = segmentToolTip.GetComponent<RectTransform>().position;
        curtain.y = curtain.y + 500;
        segmentToolTip.color = modifier;
        segmentToolTip.GetComponent<RectTransform>().position = curtain;
    }
}
