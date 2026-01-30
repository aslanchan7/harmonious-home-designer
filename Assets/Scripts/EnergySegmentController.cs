using System;
using System.Collections;
using NUnit.Compatibility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EnergySegmentController : MonoBehaviour
{
    
    [SerializeField] private UnityEngine.UI.Slider energySegment;

    [SerializeField] private int energyValue = 0;
    [SerializeField] private FSEnergyType energyType;
    [SerializeField] private bool polarity = true;
    void Awake()
    {
        energySegment.maxValue = FSBarController.maxE;
        
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
                energySegment.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<UnityEngine.UI.Image>().color = Color.brown;
            break;
            case FSEnergyType.Chaos:
                energySegment.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<UnityEngine.UI.Image>().color = Color.purple;
            break;
            case FSEnergyType.Death:
                energySegment.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<UnityEngine.UI.Image>().color = Color.darkGray;
            break;
            case FSEnergyType.Skibbidy:
                energySegment.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<UnityEngine.UI.Image>().color = Color.orange;
            break;
            default:
            energySegment.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<UnityEngine.UI.Image>().color = Color.black;
            break;
        }
    }
    public bool getPolarity()
    {
        return polarity;
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

    public void setValue(int value)
    {
        energyValue = value;
        energySegment.value = energyValue;
    }
    
    public void setPos(float xPos, float yPos, float zPos)
    {
        Vector3 position = new Vector3(xPos, yPos, zPos);
        energySegment.transform.position = position;
    }
    
}
