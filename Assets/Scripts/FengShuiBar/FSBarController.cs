using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FSBarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Slider _ProgressSlider;

    [SerializeField]
    private Slider _SinsSlider;

    [SerializeField]
    private FengShuiBarUI fengShuiBarUI;
    [SerializeField] EndScreenUI endScreenUI;

    // [SerializeField] private UnityEngine.UI.Slider _BonusSlider;
    //private List<GameObject> GoodEnergySegments = new List<GameObject>();
    // private List<FSEnergy> GoodEnergies = new();
    [Header("Data")]
    private List<FSEnergy> BadEnergies = new();
    private bool chopped = false;
    public int totalBadEnergy = 0;
    public int totalGoodEnergy = 0;
    public int maxE = 100;

    public void SetMax(int maxE)
    {
        this.maxE = maxE;
        _ProgressSlider.maxValue = maxE;
        _SinsSlider.maxValue = maxE;
        _ProgressSlider.value = 0;
        _SinsSlider.value = maxE;

        fengShuiBarUI.SetMax(maxE);

        // Comment out Bonus Slider
        //_BonusSlider.maxValue = maxE;
        //_BonusSlider.value = 0;

        // Need to add sometheing here to initialize positions relative to level / rest of UI
    }

    // Update is called once per frame
    void Update()
    {
        UpdateProgressBar();

        CheckForEndOfGame();

        // Comment out bonus slider
        // UpdateBonusBar();

        // UpdateEnergySegmentGraphics();
    }

    private void CheckForEndOfGame()
    {
        if (totalBadEnergy <= 0 && !endScreenUI.GameContinued)
        {
            endScreenUI.gameObject.SetActive(true);
        }
    }

    // Adds a Feng Shui energy source to its corrosponding list if not already present, otherwise adds amount to an existign energy type
    public void AddEnergy(FSEnergyType type, int amount, bool polarity)
    {
        bool isNewType = true;
        if (polarity)
        {
            // for (int i = 0; i < GoodEnergies.Count; i++)
            // {
            //     if (GoodEnergies[i].getType() == type)
            //     {
            //         isNewType = false;
            //         GoodEnergies[i].setEnergy(GoodEnergies[i].getAmount() + amount);
            //         Debug.Log("Added to existing energy of type: " + GoodEnergies[i].getType());
            //     }
            // }
            // if (isNewType)
            // {
            //     FSEnergy newEnergyType = new FSEnergy(polarity, amount, type);
            //     GoodEnergies.Add(newEnergyType);
            //     Debug.Log("Added new energy of type: " + type);
            // }

            Debug.LogWarning("Do not attempt to add positive/good energy");
        }
        else
        {
            Debug.Log("attempting to add a sin");
            for (int i = 0; i < BadEnergies.Count; i++)
            {
                if (BadEnergies[i].getType() == type)
                {
                    Debug.Log(
                        "Added to existing energy of type: "
                            + BadEnergies[i].getType()
                    );
                    isNewType = false;
                    BadEnergies[i]
                        .setEnergy(BadEnergies[i].getAmount() + amount);
                }
            }
            if (isNewType)
            {
                FSEnergy newEnergyType = new FSEnergy(polarity, amount, type);
                BadEnergies.Add(newEnergyType);
                Debug.Log("Added new energy of type: " + type);
                fengShuiBarUI.CreateEnergySegment(type, polarity, maxE);
            }
        }

        TallyEnergy();
        fengShuiBarUI.UpdateEnergySegmentValues(BadEnergies);
        fengShuiBarUI.UpdateEnergySegmentGraphics(
            _SinsSlider.GetComponent<RectTransform>(),
            BadEnergies,
            maxE
        );
    }

    // Lowers the amount of a specified energy type
    public void RemoveEnergy(FSEnergyType type, int amount, bool polarity)
    {
        bool exists = false;
        FSEnergy targetEnergy = null;
        Debug.Log("Attempting to remove energy of type: " + type);
        if (polarity)
        {
            // for (int i = 0; i < GoodEnergies.Count; i++)
            // {
            //     if (GoodEnergies[i].getType() == type)
            //     {
            //         exists = true;
            //         targetEnergy = GoodEnergies[i];
            //     }
            // }
            // if (exists)
            // {
            //     targetEnergy.setEnergy(targetEnergy.getAmount() - amount);
            //     Debug.Log("Sremoved from existing energy of type: " + type);
            //     if (targetEnergy.getAmount() <= 0)
            //     {
            //         GoodEnergies.Remove(targetEnergy);
            //         removeEnergySement(type, polarity);
            //     }
            // }
            // else
            // {
            //     Debug.Log("Energy of type: " + type + " not found");
            // }

            Debug.LogWarning("Do not attempt to remove positive/good energy");
        }
        else
        {
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
                Debug.Log("Removed from existing energy of type: " + type);
                targetEnergy.setEnergy(targetEnergy.getAmount() - amount);
                if (targetEnergy.getAmount() <= 0)
                {
                    BadEnergies.Remove(targetEnergy);
                    fengShuiBarUI.RemoveEnergySement(type, polarity);
                }
            }
            else
            {
                Debug.Log("Energy of type: " + type + " not found");
            }
        }
        TallyEnergy();
        fengShuiBarUI.UpdateEnergySegmentValues(BadEnergies);
        fengShuiBarUI.UpdateEnergySegmentGraphics(
            _SinsSlider.GetComponent<RectTransform>(),
            BadEnergies,
            maxE
        );
    }

    // Calculates total good / bad energies, adjusts energy ratio accordingly
    private void TallyEnergy()
    {
        // int sumGoodEnergy = 0;
        int sumBadEnergy = 0;
        // for (int i = 0; i < GoodEnergies.Count; i++)
        // {
        //     sumGoodEnergy += GoodEnergies[i].getAmount();
        // }
        for (int i = 0; i < BadEnergies.Count; i++)
        {
            sumBadEnergy += BadEnergies[i].getAmount();
        }
        totalBadEnergy = sumBadEnergy;
        // totalGoodEnergy = sumGoodEnergy;
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

    // Comment out Bonus Slider
    // // Updates the  bonus bar slider to visualiz the mount of bonus Feng Shui energy you have accumulated
    // private void UpdateBonusBar()
    // {
    //     _BonusSlider.value = totalGoodEnergy;
    // }
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

    public FSEnergy(bool isGood, int amount, FSEnergyType type)
    {
        this.isGood = isGood;
        this.amount = amount;
        this.type = type;
    }

    // Access the energy amount
    public int getAmount()
    {
        return amount;
    }

    // Access the Polarity (good / bad)
    public bool getPolarity()
    {
        return isGood;
    }

    // Access the type of good / bad energy
    public FSEnergyType getType()
    {
        return type;
    }

    // Set the amount of energy contained in a type of FSEnergy
    public void setEnergy(int inAmount)
    {
        amount = inAmount;
    }
}