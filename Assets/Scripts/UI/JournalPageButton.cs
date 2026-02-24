using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JournalPageButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Sprite hoverSprite, foldSprite;
    [SerializeField] Image image;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(image == null) return;
        image.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(image == null) return;
        image.sprite = foldSprite;
    }

    void OnEnable()
    {
        if(image == null) return;
        image.sprite = foldSprite;
    }
}
