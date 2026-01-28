using NUnit.Compatibility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EnergySegmentController : MonoBehaviour
{
    
    [SerializeField] private UnityEngine.UI.Slider energySegment;

    [SerializeField] private int energyValue = 0;
    [SerializeField] private string energyType;
    [SerializeField] private bool polarity = true;
    void Awake()
    {
        energySegment.maxValue = FSBarController.maxE;
    }
    public string getEnergyType()
    {
        return energyType;
    }
    public void setEnergyType(string sourceType)
    {
        energyType = sourceType;
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
