using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class FSBarController : MonoBehaviour
{
    [SerializeField] private GameObject FSBar;
    [SerializeField] private EnergySegmentController energySegmentPrefab;
    public UnityEngine.UI.Slider goodSlider;
    public UnityEngine.UI.Slider badSlider;
    public int goodLimit = 0;
    public int totalBadEnergy = 0;
    public int totalGoodEnergy = 0;
    public static int maxE = 100;
    private List<EnergySegmentController> GoodEnergySegments = new List<EnergySegmentController>();
    private List<EnergySegmentController> BadEnergySegments = new List<EnergySegmentController>();
    private List<FSEnergy> GoodEnergies = new List<FSEnergy>();
    private List<FSEnergy> BadEnergies= new List<FSEnergy>();
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        goodLimit = (maxE - totalBadEnergy);
        goodSlider.maxValue = maxE;
        badSlider.maxValue = maxE;
        goodSlider.value = 0;
        badSlider.value = 0;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEnergyLimit();
        // UpdateEnergySegmentValues();
        // UpdateEnergySegmentGraphics();
    }

    // Adds a Feng Shui energy source to its corrosponding list
    public void AddEnergy(FSEnergy source)
    {
        
        string sourceEnergyType = source.getType();
        bool sourceEnergyPolarity = source.getPolarity();
        bool newSource = true;
        if (sourceEnergyPolarity){
            int loc = GoodEnergies.IndexOf(source);
            if (loc != -1) {
                Debug.Log("GoodEnergy source already exists");
            }
            else
            {
                for(int i = 0; i < GoodEnergies.Count; i++)
                {
                    if(GoodEnergies[i].getType().Equals(sourceEnergyType))
                    {
                        newSource = false;
                    }
                }
                GoodEnergies.Add(source);
                Debug.Log("GoodEnergy Source added");
            }
        }
        else
        {
            int loc = BadEnergies.IndexOf(source);
            if (loc != -1) {
                Debug.Log("BadEnergy source already exists");
            }
            else
            {
                for(int i = 0; i < BadEnergies.Count; i++)
                {
                    if(BadEnergies[i].getType().Equals(sourceEnergyType))
                    {
                        newSource = false;
                    }
                }
                BadEnergies.Add(source);
                Debug.Log("BadEnergy Source added");
            }
        }
        TallyEnergy();
        if (newSource)
        {
            createEnergySegment(sourceEnergyType, sourceEnergyPolarity);
        }
    }

    // Removes a Feng Shui energy source from its corrosponding list
    public void RemoveEnergy(FSEnergy source)
    {
        string sourceEnergyType = source.getType();
        bool sourceEnergyPolarity = source.getPolarity();
        int totalEnergyOfType = 0;
        if (sourceEnergyPolarity){
            int loc = GoodEnergies.IndexOf(source);
            if (loc == -1) {
                Debug.Log("GoodEnergy Source not found");
            }else{
                Debug.Log("GoodEnergy Source removed");
                GoodEnergies.Remove(source);
                for(int i = 0; i < GoodEnergies.Count; i++)
                {
                    if (GoodEnergies[i].getType().Equals(sourceEnergyType)){
                        totalEnergyOfType += GoodEnergies[i].getAmount();
                    }
                }
                if (totalEnergyOfType == 0)
                {
                    removeEnergySement(sourceEnergyType, sourceEnergyPolarity);
                    Debug.Log("removeEnergySegment happenend");
                }
                else
                {
                    Debug.Log("removeEnergySegment did not happen");
                }
            }
        }else{
            int loc = BadEnergies.IndexOf(source);
            if (loc == -1) {
                Debug.Log("BadEnergy Source not found");
            }else{
                Debug.Log("BadEnergy Source removed");
                BadEnergies.Remove(source);
                for(int i = 0; i < BadEnergies.Count; i++)
                {
                    if (BadEnergies[i].getType().Equals(sourceEnergyType)){
                        totalEnergyOfType += BadEnergies[i].getAmount();
                    }
                }
                if (totalEnergyOfType == 0)
                {
                    removeEnergySement(sourceEnergyType, sourceEnergyPolarity);
                }
            }
        }
        TallyEnergy();
        
    }

    // Calculates total good / bad energies, adjusts energy ratio accordingly
    private void TallyEnergy()
    {
        int sumGoodEnergy = 0;
        int sumBadEnergy = 0;
        for(int i = 0; i < GoodEnergies.Count; i++){
            sumGoodEnergy += GoodEnergies[i].getAmount();
        }
        for(int i = 0; i < BadEnergies.Count; i++){
            sumBadEnergy += BadEnergies[i].getAmount();
        }
        totalBadEnergy = sumBadEnergy;
        totalGoodEnergy = sumGoodEnergy;
        goodLimit = maxE - sumBadEnergy;
    }

    // Updates the size of the good / bad sliders based on the total energies 
    private void UpdateEnergyLimit()
    {
        if (goodLimit < 0){
            goodLimit = 0;
            badSlider.value = maxE;
        }  else{
            goodLimit = maxE - totalBadEnergy;
            badSlider.value = totalBadEnergy;
        }
        goodSlider.value = goodLimit;
    }
    
    // Creates an instance of the energySegment prefab of the desired type
    private void createEnergySegment(string type, bool polarity)
    {
        EnergySegmentController newEnergySegment = Instantiate(energySegmentPrefab);
        
        newEnergySegment.setEnergyType(type);
        newEnergySegment.setPolarity(polarity);
        if (polarity)
        {
            GoodEnergySegments.Add(newEnergySegment);
        }
        else
        {
            //BadEnergySegments.Add(newEnergySegment);
        }

    }

    // Removes the energy segment of a desired type (only called if amount of desired energy type = 0)
    private void removeEnergySement(string type, bool polarity)
    {
        EnergySegmentController removedSegment;
        if (polarity)
        {
           for (int i = 0; i < GoodEnergySegments.Count; i++)
            {
                Debug.Log("attempting to remove good energySegment");
                removedSegment = GoodEnergySegments[i];
                Debug.Log("removedSegment type: " + removedSegment.getEnergyType());
                Debug.Log("target type: " + type);
                Debug.Log("strings are equal: " + removedSegment.getEnergyType().Equals(type));
                if (removedSegment.getEnergyType().Equals(type))
                {
                    Destroy(GoodEnergySegments[i]);
                    GoodEnergySegments.Remove(removedSegment);
                    Debug.Log("good energysegment destroyed");
                }
            }
        }
        else
        {
            for (int i = 0; i < BadEnergySegments.Count; i++)
            {
                removedSegment = BadEnergySegments[i];
                if (removedSegment.getEnergyType().Equals(type))
                {
                    Destroy(GoodEnergySegments[i]);
                    BadEnergySegments.Remove(removedSegment);
                }
            } 
        }
        
    }

    // Updates the value for each type of energy segment 
    private void UpdateEnergySegmentValues()
    {
        for (int i = 0; i < GoodEnergySegments.Count; i++)
        {
            EnergySegmentController currentSegment = GoodEnergySegments[i];
            string currentEnergyType = currentSegment.getEnergyType();
            for(int j = 0; j < GoodEnergies.Count; i++)
            {
                int currentSourceValue = GoodEnergies[j].getAmount();
                if (GoodEnergies[j].getType().Equals(currentEnergyType))
                {
                    currentSegment.setValue(currentSegment.getValue() + currentSourceValue);
                }
            }
        }
        for (int i = 0; i < BadEnergySegments.Count; i++)
        {
            EnergySegmentController currentSegment = BadEnergySegments[i];
            string currentEnergyType = currentSegment.getEnergyType();
            for(int j = 0; j < BadEnergies.Count; i++)
            {
                int currentSourceValue = BadEnergies[j].getAmount();
                if (BadEnergies[j].getType().Equals(currentEnergyType))
                {
                    currentSegment.setValue(currentSegment.getValue() + currentSourceValue);
                }
            }
        }
    }
    
    /*
        Updates the sliders for each energy type so that they appear end to end 
        Sliders will be positioned in order of creation (by order added to Good / Bad EnergySegments)
        If totalGoodEnergy is more than goodLimit, sliders passing limit will be cut short appropriately
    */
    private void UpdateEnergySegmentGraphics()
    {
        int energyAllowance = goodLimit;
        float goodEnergyOffset = 0.0f;
        float sliderWidth = goodSlider.GetComponent<RectTransform>().sizeDelta.x;
        float sliderPosX = goodSlider.GetComponent<RectTransform>().position.x;
        float sliderPosY = goodSlider.GetComponent<RectTransform>().position.y;
        float sliderPosZ = goodSlider.GetComponent<RectTransform>().position.z;
        for (int i = 0; i < GoodEnergySegments.Count; i++)
        {
            EnergySegmentController currentSegment = GoodEnergySegments[i];
            string currentEnergyType = currentSegment.getEnergyType();
            for (int j = 0; j < GoodEnergies.Count; j++)
            {
                currentSegment.setPos(goodEnergyOffset + sliderPosX, sliderPosY, sliderPosZ);
                FSEnergy currentSource = GoodEnergies[j];
                if (currentEnergyType.Equals(currentSource.getType()))
                {
                    if ((currentSource.getAmount() + currentSegment.getValue()) > energyAllowance)
                    {
                        currentSegment.setValue(currentSegment.getValue() + energyAllowance);
                    }
                    else
                    {
                        currentSegment.setValue(currentSegment.getValue() + currentSource.getAmount());
                    }
                    
                    goodEnergyOffset += (currentSegment.getValue() / maxE) * sliderWidth;
                }
            }
        }
        
        float badEnergyOffset = 0.0f;
        sliderWidth = badSlider.GetComponent<RectTransform>().sizeDelta.x;
        sliderPosX = badSlider.GetComponent<RectTransform>().position.x;
        sliderPosY = badSlider.GetComponent<RectTransform>().position.y;
        sliderPosZ = badSlider.GetComponent<RectTransform>().position.z;
        for (int i = 0; i < GoodEnergySegments.Count; i++)
        {
            EnergySegmentController currentSegment = GoodEnergySegments[i];
            string currentEnergyType = currentSegment.getEnergyType();
            for (int j = 0; j < BadEnergies.Count; j++)
            {
                currentSegment.setPos(sliderPosX - badEnergyOffset, sliderPosY, sliderPosZ);
                FSEnergy currentSource = GoodEnergies[j];
                if (currentEnergyType.Equals(currentSource.getType()))
                {
                    currentSegment.setValue(currentSegment.getValue() + currentSource.getAmount());
                    badEnergyOffset += (currentSegment.getValue() / maxE) * sliderWidth;
                }
            }
        }
    }

}

// Class for the energies provided by furniture pieces
public class FSEnergy{
    
    // If the energy is good or bad
    private bool isGood;
    
    // The amount of good or bad Feng Shui energy 
    private int amount;

    // The name of the energy type
    private string type;

    public FSEnergy(bool isGood, int amount, string type){
        this.isGood = isGood;
        this.amount = amount;
        this.type = type;
    }

    // Access the energy amount
    public int getAmount(){
        return amount;
    }

    // Access the Polarity (good / bad) 
    public bool getPolarity(){
        return isGood;
    }

    // Access the type of good / bad energy
    public string getType(){
        return type;
    }
}
