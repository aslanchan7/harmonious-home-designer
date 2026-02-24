using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class HoverableButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Vector3 origPos;
    private Vector2 origSize;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        origPos = rectTransform.localPosition;
        origSize = rectTransform.sizeDelta;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.localPosition = new(origPos.x, origPos.y + (origSize.y * 0.05f), origPos.z);
        rectTransform.sizeDelta = origSize * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.localPosition = origPos;
        rectTransform.sizeDelta = origSize;
    }
}
