using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject starTooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        starTooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        starTooltip.SetActive(false);
    }
}
