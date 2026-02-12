using System;
using System.Collections;
using System.Numerics;
using System.Xml.Schema;
using NUnit.Compatibility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EnergySegmentController : MonoBehaviour
{
    
    [SerializeField] private UnityEngine.UI.Slider energySegment;
    [SerializeField] private UnityEngine.UI.Button hoverDetection;
    [SerializeField] private int energyValue = 0;
    [SerializeField] private FSEnergyType energyType;
    [SerializeField] private bool polarity = true;
    private Transform segmentFill;
    void Awake()
    {
        energySegment.maxValue = FSBarController.maxE;
        float sliderWidth = energySegment.GetComponent<RectTransform>().sizeDelta.x;
        segmentFill = energySegment.transform.Find("Fill Area").Find("Fill");
        float buttonWidth = sliderWidth - energyValue * (sliderWidth / energySegment.maxValue);
        hoverDetection.GetComponent<RectTransform>().offsetMin = new UnityEngine.Vector2(buttonWidth, 5);

    } 
    public FSEnergyType getEnergyType()
    {
        return energyType;
    }
    public void setEnergyType(FSEnergyType sourceType)
    {
        energyType = sourceType;
        switch (energyType)
        {
            case FSEnergyType.Toilet:
                segmentFill.GetComponent<UnityEngine.UI.Image>().color = Color.brown;
            break;
            case FSEnergyType.Chaos:
                segmentFill.GetComponent<UnityEngine.UI.Image>().color = Color.purple;
            break;
            case FSEnergyType.Death:
                segmentFill.GetComponent<UnityEngine.UI.Image>().color = Color.darkGray;
            break;
            case FSEnergyType.Skibbidy:
                segmentFill.GetComponent<UnityEngine.UI.Image>().color = Color.orange;
            break;
            default:
                segmentFill.GetComponent<UnityEngine.UI.Image>().color = Color.black;
            break;
        }
    }
    void Update()
    {
        float sliderWidth = energySegment.GetComponent<RectTransform>().sizeDelta.x;
        segmentFill = energySegment.transform.Find("Fill Area").Find("Fill");
        float buttonWidth = sliderWidth - energyValue * (sliderWidth / energySegment.maxValue);
        hoverDetection.GetComponent<RectTransform>().offsetMin = new UnityEngine.Vector2(buttonWidth, 5);
    }

    public void setPolarity(bool sourcePolarity)
    {
        polarity = sourcePolarity;
        if (polarity)
        {
            energySegment.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
        }
        else
        {
            energySegment.direction = UnityEngine.UI.Slider.Direction.RightToLeft;
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
