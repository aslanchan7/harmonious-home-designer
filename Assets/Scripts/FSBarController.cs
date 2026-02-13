using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor.Experimental;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class FSBarController : MonoBehaviour
{
    [SerializeField] private GameObject _FSBar;
    [SerializeField] private GameObject _EnergySegmentPrefab;
    [SerializeField] private UnityEngine.UI.Slider _ProgressSlider;
    [SerializeField] private UnityEngine.UI.Slider _SinsSlider;
    [SerializeField] private UnityEngine.UI.Slider _BonusSlider;
    [SerializeField] private GameObject _ParentCanvas;
    //private List<GameObject> GoodEnergySegments = new List<GameObject>();
    private List<GameObject> BadEnergySegments = new List<GameObject>();
    private List<FSEnergy> GoodEnergies = new List<FSEnergy>();
    private List<FSEnergy> BadEnergies = new List<FSEnergy>();
    private bool chopped = false;
    public int totalBadEnergy = 0;
    public int totalGoodEnergy = 0;
    public static int maxE = 100;
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        _ProgressSlider.maxValue = maxE;
        _SinsSlider.maxValue = maxE;
        _BonusSlider.maxValue = maxE;
        _ProgressSlider.value = 0;
        _SinsSlider.value = 0;
        _BonusSlider.value = 0;
       

        // Need to add sometheing here to initialize positions relative to level / rest of UI

    }

    // Update is called once per frame
    void Update()
    {
        UpdateProgressBar();
        UpdateBonusBar();
        
        // UpdateEnergySegmentGraphics();
    }

    // Adds a Feng Shui energy source to its corrosponding list if not already present, otherwise adds amount to an existign energy type
    public void AddEnergy(FSEnergyType type, int amount, bool polarity)
    {
        bool isNewType = true;
        if (polarity)
        {
            for (int i = 0; i < GoodEnergies.Count; i++)
            {
                if (GoodEnergies[i].getType() == type)
                {
                    isNewType = false;
                    GoodEnergies[i].setEnergy(GoodEnergies[i].getAmount() + amount);
                    Debug.Log("Added to existing energy of type: " + GoodEnergies[i].getType());
                }
            }
            if (isNewType)
            {
                FSEnergy newEnergyType = new FSEnergy(polarity, amount, type);
                GoodEnergies.Add(newEnergyType);
                Debug.Log("Added new energy of type: " + type);
            }
        }
        else
        {
            Debug.Log("attempting to add a sin");
            for (int i = 0; i < BadEnergies.Count; i++)
            {
                if (BadEnergies[i].getType() == type)
                {
                    Debug.Log("Added to existing energy of type: " + BadEnergies[i].getType());
                    isNewType = false;
                    BadEnergies[i].setEnergy(BadEnergies[i].getAmount() + amount);
                }
            }
            if (isNewType)
            {
                FSEnergy newEnergyType = new FSEnergy(polarity, amount, type);
                BadEnergies.Add(newEnergyType);
                Debug.Log("Added new energy of type: " + type);
                createEnergySegment(type, polarity);
            }
        }
        TallyEnergy();
        UpdateEnergySegmentValues();
        UpdateEnergySegmentGraphics();
    }

    // Lowers the amount of a specified energy type
    public void RemoveEnergy(FSEnergyType type, int amount, bool polarity)
    {
        bool exists = false;
        FSEnergy targetEnergy = null;
        Debug.Log("Attempting to remove energy of type: " + type);
        if (polarity)
        { 
            for (int i = 0; i < GoodEnergies.Count; i++)
            {
                if (GoodEnergies[i].getType() == type)
                {
                    exists = true;
                    targetEnergy = GoodEnergies[i];
                }
            }
            if (exists)
            {
                targetEnergy.setEnergy(targetEnergy.getAmount() - amount);
                Debug.Log("Sremoved from existing energy of type: " + type);
                if (targetEnergy.getAmount() <= 0)
                {
                    GoodEnergies.Remove(targetEnergy);
                    removeEnergySement(type, polarity);
                }
            }
            else
            {
                Debug.Log("Energy of type: " + type + " not found");
            }
        }else{
            for (int i = 0; i < BadEnergies.Count; i++)
            {
                if (BadEnergies[i].getType() == type)
                {
                    exists = true;
                    targetEnergy = BadEnergies[i];
                }
            }
            if (exists)
            {
                Debug.Log("Sremoved from existing energy of type: " + type);
                targetEnergy.setEnergy(targetEnergy.getAmount() - amount);
                if (targetEnergy.getAmount() <= 0)
                {
                    BadEnergies.Remove(targetEnergy);
                    removeEnergySement(type, polarity);
                }
            }
            else
            {
                Debug.Log("Energy of type: " + type + " not found");
            }
        }
        TallyEnergy();
        UpdateEnergySegmentValues();
        UpdateEnergySegmentGraphics();
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
        if (sumBadEnergy > maxE)
        {
            chopped = true;
        }
        else
        {  
            chopped = false; 
        }
    }

    // Updates the progress bar slider to visualize  the total amount of sins impeading progress
    private void UpdateProgressBar()
    {
        if (!chopped)
        {
            _SinsSlider.value = totalBadEnergy;
            _ProgressSlider.value = maxE - totalBadEnergy;
        }    
    }

    // Updates the  bonus bar slider to visualiz the mount of bonus Feng Shui energy you have accumulated
    private void UpdateBonusBar()
    {
        _BonusSlider.value = totalGoodEnergy;
    }


    // Creates an instance of the energySegment prefab of the desired type
    private void createEnergySegment(FSEnergyType type, bool polarity)
    {
        GameObject newEnergySegment = Instantiate(_EnergySegmentPrefab);
        newEnergySegment.GetComponent<EnergySegmentController>().setEnergyType(type);
        newEnergySegment.GetComponent<EnergySegmentController>().setPolarity(polarity);
        newEnergySegment.transform.SetParent(_ParentCanvas.transform);
        newEnergySegment.GetComponent<RectTransform>().localPosition = UnityEngine.Vector3.zero;
        if (!polarity)
        {
            BadEnergySegments.Add(newEnergySegment);
        }
        else
        {
            Debug.Log("Not segmenting good energies at this point in development :/");
        }

    }

    // Removes the sin segment of a desired type (only called if amount of that type of sin = 0)
    private void removeEnergySement(FSEnergyType type, bool polarity)
    {
        GameObject removedSegment;
        if (polarity)
        {
           Debug.Log("Not segmenting good energies at this point in development :/");
        }
        else
        {
            for (int i = 0; i < BadEnergySegments.Count; i++)
            {
                removedSegment = BadEnergySegments[i];
                if (removedSegment.GetComponent<EnergySegmentController>().getEnergyType().Equals(type))
                {
                    Destroy(BadEnergySegments[i]);
                    BadEnergySegments.Remove(removedSegment);
                    Debug.Log("Successfully destroyed segment");
                }
            } 
        }
        
    }

    // Updates the value for each sin segment 
    private void UpdateEnergySegmentValues()
    {
        for (int i = 0; i < BadEnergySegments.Count; i++)
        {
            GameObject currentSegment = BadEnergySegments[i];
            FSEnergyType currentEnergyType = currentSegment.GetComponent<EnergySegmentController>().getEnergyType();
            for(int j = 0; j < BadEnergies.Count; j++)
            {
                int currentSourceValue = BadEnergies[j].getAmount();
                if (BadEnergies[j].getType().Equals(currentEnergyType))
                {
                    currentSegment.GetComponent<EnergySegmentController>().setValue(currentSourceValue);
                }
            }
        }
    }
    
    // Updates the position and slider graphic for each sin type
    private void UpdateEnergySegmentGraphics()
    {
        float segmentOffset = 0.0f;
        float sliderPosX = _SinsSlider.GetComponent<RectTransform>().localPosition.x;
        for (int i = 0; i < BadEnergySegments.Count; i++)
        {
            GameObject currentSegment = BadEnergySegments[i];
            FSEnergyType currentEnergyType = BadEnergySegments[i].GetComponent<EnergySegmentController>().getEnergyType();
            FSEnergy currentEnergy = null;
            float sliderWidth = _SinsSlider.GetComponent<RectTransform>().sizeDelta.x;
            UnityEngine.Vector3 displacement = new UnityEngine.Vector3(sliderPosX - segmentOffset, 0, 0);
            for (int j = 0; j < BadEnergies.Count; j++)
            {
                if (currentEnergyType == BadEnergies[j].getType())
                {
                   currentEnergy = BadEnergies[j]; 
                }
            }
            Debug.Log("current displacement: " + segmentOffset);
            Debug.Log("slider width: " + sliderWidth);
            currentSegment.GetComponent<RectTransform>().localPosition = displacement;
            segmentOffset += currentEnergy.getAmount() * (sliderWidth / maxE);
        }
    }

}

// Class for the energies provided by furniture pieces
public class FSEnergy
{
    
    // If the energy is good or bad
    private bool isGood;
    
    // The amount of good or bad Feng Shui energy 
    private int amount;

    // The name of the energy type
    private FSEnergyType type;

    public FSEnergy(bool isGood, int amount, FSEnergyType type){
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
    public FSEnergyType getType(){
        return type;
    }
    
    // Set the amount of energy contained in a type of FSEnergy
    public void setEnergy(int inAmount)
    {
        amount = inAmount;
    }
}

// Enumerator class for the types of energies
public enum FSEnergyType
{
    // Good energies, when running addEnergy() or removeEnergy() input true for "polarity" field
    Functional,
    Luck,
    Wealth,
    Love,
    Happiness,

    // Bad energies, when running addEnergy() or removeEnergy() input false for "polarity" field
    Disfunctional,
    Toilet,
    Chaos,
    Death,
    Skibbidy
}
