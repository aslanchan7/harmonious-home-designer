using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergySegmentController : MonoBehaviour
{
    [Header("References")]
    public Slider energySegment;

    [SerializeField]
    private Transform segmentFill;

    [SerializeField]
    private Button hoverDetection;

    [Header("Tooltip")]
    [SerializeField] private RectTransform tooltip;
    [SerializeField] private float tooltipOffsetY;
    [SerializeField] private float tooltipScale;
    private FSEnergyDictionaryItem fSEnergyDictionaryItem;

    [Header("Settings")]
    [SerializeField]
    private int energyValue = 0;

    [SerializeField]
    private FSEnergyType energyType;

    [SerializeField]
    private bool polarity = true;

    public void Initialize(
        FSEnergyType fsEnergyType,
        bool polarity,
        Vector2 size,
        int maxE,
        FSEnergyDictionary fSEnergyDictionary
    )
    {
        // GameObject newEnergySegmentGameObject = Instantiate(
        //     gameObject,
        //     transform
        // );
        // EnergySegmentController newEnergySegment =
        //     newEnergySegmentGameObject.GetComponent<EnergySegmentController>();

        energyType = fsEnergyType;
        SetPolarity(polarity);
        SetSize(size);

        energySegment.maxValue = maxE;
        float sliderWidth = energySegment.GetComponent<RectTransform>()
            .sizeDelta.x;
        float buttonWidth =
            sliderWidth - energyValue * (sliderWidth / maxE);
        hoverDetection.GetComponent<RectTransform>()
            .offsetMin = new Vector2(buttonWidth, 5);


        // Set tooltip image
        fSEnergyDictionaryItem = fSEnergyDictionary.dictionary.Find(x => x.energyType == this.energyType);
        if(fSEnergyDictionaryItem == null)
        {
            Debug.LogError($"Missing icon for energy type: {energyType}");
        }
        tooltip.GetComponent<Image>().sprite = fSEnergyDictionaryItem.energyTooltip;

        // Set tooltip pos & scale
        tooltip.localPosition = new(0f, tooltipOffsetY);
        tooltip.localScale = new(tooltipScale, tooltipScale, tooltipScale);
    
        // Determine color of segment fill
        segmentFill.GetComponent<Image>().color = fSEnergyDictionaryItem.color;
    }

    // void Awake()
    // {
    //     energySegment.maxValue = FSBarController.maxE;
    //     float sliderWidth = energySegment.GetComponent<RectTransform>().sizeDelta.x;
    //     float buttonWidth = sliderWidth - energyValue * (sliderWidth / energySegment.maxValue);
    //     hoverDetection.GetComponent<RectTransform>().offsetMin = new Vector2(buttonWidth, 5);
    // }

    public FSEnergyType GetEnergyType()
    {
        return energyType;
    }

    public void SetEnergyType(FSEnergyType sourceType)
    {
        Debug.LogWarning("Deprecated function! Do not use this function");
        // energyType = sourceType;
        // switch (energyType)
        // {
        //     case FSEnergyType.Toilet:
        //         segmentFill.GetComponent<Image>().color = fSEnergyDictionaryItem.color;
        //         break;
        //     case FSEnergyType.Chaos:
        //         segmentFill.GetComponent<Image>().color = Color.purple;
        //         break;
        //     case FSEnergyType.Death:
        //         segmentFill.GetComponent<Image>().color = Color.darkGray;
        //         break;
        //     default:
        //         segmentFill.GetComponent<Image>().color = Color.black;
        //         break;
        // }
    }

    void Update()
    {
        float sliderWidth = energySegment
            .GetComponent<RectTransform>()
            .sizeDelta.x;
        float buttonWidth =
            sliderWidth - energyValue * (sliderWidth / energySegment.maxValue);
        hoverDetection.GetComponent<RectTransform>().offsetMin = new Vector2(
            buttonWidth,
            5
        );
    }

    public void SetPolarity(bool sourcePolarity)
    {
        polarity = sourcePolarity;
        if (polarity)
        {
            energySegment.direction = Slider.Direction.LeftToRight;
        }
        else
        {
            energySegment.direction = Slider.Direction.RightToLeft;
        }
    }

    public int GetValue()
    {
        return energyValue;
    }

    // Set the velue of energy in the segment
    public void SetValue(int value)
    {
        energyValue = value;
        energySegment.value = energyValue;
    }

    public void SetSize(Vector2 size)
    {
        energySegment.GetComponent<RectTransform>().sizeDelta = size;
    }
}
