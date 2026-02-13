using UnityEngine;
using UnityEngine.UI;

public class EnergySegmentController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform segmentFill;
    [SerializeField] private Slider energySegment;
    [SerializeField] private Button hoverDetection;

    [Header("Settings")]
    [SerializeField] private int energyValue = 0;
    [SerializeField] private FSEnergyType energyType;
    [SerializeField] private bool polarity = true;

    void Awake()
    {
        energySegment.maxValue = FSBarController.maxE;
        float sliderWidth = energySegment.GetComponent<RectTransform>().sizeDelta.x;
        float buttonWidth = sliderWidth - energyValue * (sliderWidth / energySegment.maxValue);
        hoverDetection.GetComponent<RectTransform>().offsetMin = new Vector2(buttonWidth, 5);
    }

    public FSEnergyType GetEnergyType()
    {
        return energyType;
    }

    public void SetEnergyType(FSEnergyType sourceType)
    {
        energyType = sourceType;
        switch (energyType)
        {
            case FSEnergyType.Toilet:
                segmentFill.GetComponent<Image>().color = Color.brown;
                break;
            case FSEnergyType.Chaos:
                segmentFill.GetComponent<Image>().color = Color.purple;
                break;
            case FSEnergyType.Death:
                segmentFill.GetComponent<Image>().color = Color.darkGray;
                break;
            case FSEnergyType.Skibbidy:
                segmentFill.GetComponent<Image>().color = Color.orange;
                break;
            default:
                segmentFill.GetComponent<Image>().color = Color.black;
                break;
        }
    }
    void Update()
    {
        float sliderWidth = energySegment.GetComponent<RectTransform>().sizeDelta.x;
        float buttonWidth = sliderWidth - energyValue * (sliderWidth / energySegment.maxValue);
        hoverDetection.GetComponent<RectTransform>().offsetMin = new Vector2(buttonWidth, 5);
    }

    public void setPolarity(bool sourcePolarity)
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
    public int getValue()
    {
        return energyValue;
    }

    // Set the velue of energy in the segment
    public void setValue(int value)
    {
        energyValue = value;
        energySegment.value = energyValue;
    }
}
