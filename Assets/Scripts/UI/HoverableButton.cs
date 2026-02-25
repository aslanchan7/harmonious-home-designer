using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class HoverableButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Vector3 origPos;
    private Vector2 origSize;

    [Header("Hover SFX")]
    [SerializeField] private SFXAction[] hoverSounds;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        origPos = rectTransform.localPosition;
        origSize = rectTransform.sizeDelta;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayRandomHoverSFX();
        
        rectTransform.localPosition = new(origPos.x, origPos.y + (origSize.y * 0.05f), origPos.z);
        rectTransform.sizeDelta = origSize * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.localPosition = origPos;
        rectTransform.sizeDelta = origSize;
    }

    private void PlayRandomHoverSFX()
    {
        if (hoverSounds == null || hoverSounds.Length == 0)
            return;

        int index = Random.Range(0, hoverSounds.Length);
        UISFX.Play(hoverSounds[index]);
    }
}
