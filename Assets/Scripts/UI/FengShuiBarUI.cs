using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FengShuiBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject _EnergySegmentPrefab;
    public Slider sinSlider;
    public GameObject[] LayerParents;
    public GameObject StarParent;
    public Sprite StarEmpty;
    public Sprite StarFilled;
    public float MinValueToShowIcons;
    [SerializeField] EndScreenUI endScreenUI;

    [Header("Data")]
    [SerializeField]
    FSEnergyDictionary fSEnergyDictionary;
    private GameObject[] BadEnergySegments = new GameObject[
        Enum.GetNames(typeof(FSEnergyType)).Length
    ];

    // Creates an instance of the energySegment prefab of the desired type
    public void CreateEnergySegment(
        FSEnergyType type,
        bool polarity,
        float maxE
    )
    {
        // EnergySegmentController newEnergySegment =
        //     _EnergySegmentPrefab.InstantiatePrefab(sinSlider.transform, maxE, fSEnergyDictionary);
        // Instantiate energy segment & initialize values
        GameObject energySegmentObj = Instantiate(
            _EnergySegmentPrefab,
            sinSlider.transform
        );
        energySegmentObj.transform.SetParent(
            LayerParents[(int)type].transform,
            false
        );
        EnergySegmentController newEnergySegment =
            energySegmentObj.GetComponent<EnergySegmentController>();
        // RectTransform rectTransform = newEnergySegment.gameObject.GetComponent<RectTransform>();
        // newEnergySegment.SetPolarity(polarity);
        // newEnergySegment.SetSize(
        //     sinSlider.GetComponent<RectTransform>().sizeDelta
        // );
        // rectTransform.localPosition = Vector3.zero;

        newEnergySegment.Initialize(
            type,
            polarity,
            sinSlider.GetComponent<RectTransform>().sizeDelta,
            maxE,
            fSEnergyDictionary
        );

        if (!polarity)
        {
            BadEnergySegments[(int)type] = newEnergySegment.gameObject;
        }
        else
        {
            Debug.Log(
                "Not segmenting good energies at this point in development :/"
            );
        }
    }

    public void SetMax(float maxE)
    {
        foreach (GameObject segmentObject in BadEnergySegments)
        {
            if (segmentObject != null)
                segmentObject.GetComponent<Slider>().maxValue = maxE;
        }
    }

    // Removes the sin segment of a desired type (only called if amount of that type of sin = 0)
    public void RemoveEnergySegment(FSEnergyType type, bool polarity)
    {
        if (polarity)
        {
            Debug.Log(
                "Not segmenting good energies at this point in development :/"
            );
        }
        else
        {
            Destroy(BadEnergySegments[(int)type]);
            BadEnergySegments[(int)type] = null;
            Debug.Log("Successfully destroyed segment");
        }
    }

    // Updates the value for each sin segment
    public void UpdateEnergySegmentValues(
        List<FSEnergy> badEnergies,
        float maxEnergy
    )
    {
        float cumulativeValue = 0;
        bool capped = false;
        for (int i = BadEnergySegments.Length - 1; i >= 0; i--)
        {
            GameObject currentSegment = BadEnergySegments[i];
            if (currentSegment == null)
                continue;
            if (capped)
            {
                Destroy(currentSegment);
                BadEnergySegments[i] = null;
            }
            FSEnergyType currentEnergyType = currentSegment
                .GetComponent<EnergySegmentController>()
                .GetEnergyType();
            for (int j = 0; j < badEnergies.Count; j++)
            {
                if (badEnergies[j].getType().Equals(currentEnergyType))
                {
                    float currentValue = badEnergies[j].getAmount();
                    float totalValue = cumulativeValue + currentValue;
                    if (totalValue >= maxEnergy)
                    {
                        totalValue = maxEnergy;
                        capped = true;
                    }
                    EnergySegmentController controller =
                        currentSegment.GetComponent<EnergySegmentController>();
                    controller.SetValue(totalValue);
                    controller.SetIcon(currentValue >= MinValueToShowIcons);
                    cumulativeValue = totalValue;
                }
            }
        }
    }

    public void SetStars(int starNumber)
    {
        if (starNumber == 0)
        {
            StarParent.SetActive(false);
            return;
        }
        
        EndGame(starNumber);

        StarParent.SetActive(true);
        Image[] starImages = StarParent.GetComponentsInChildren<Image>();
        for (int i = 0; i < starImages.Length; i++)
        {
            starImages[i].sprite = i < starNumber ? StarFilled : StarEmpty;
        }
    }

    private void EndGame(int starNumber)
    {
        if(!endScreenUI.GameContinued)
        {
            endScreenUI.gameObject.SetActive(true);
        } 
    }

    // // TODO: THIS JUST STRAIGHT UP DOESN'T WORK
    // // Updates the position and slider graphic for each sin type
    // public void UpdateEnergySegmentGraphics(
    //     RectTransform sinsSlider,
    //     List<FSEnergy> badEnergies,
    //     float maxEnergy
    // )
    // {
    //     // float segmentOffset = 0.0f;
    //     for (int i = 0; i < BadEnergySegments.Count; i++)
    //     {
    //         GameObject currentSegment = BadEnergySegments[i];
    //         FSEnergyType currentEnergyType = BadEnergySegments[i]
    //             .GetComponent<EnergySegmentController>()
    //             .GetEnergyType();
    //         FSEnergy currentEnergy = null;
    //         float sliderWidth = sinsSlider.sizeDelta.x;
    //         for (int j = 0; j < badEnergies.Count; j++)
    //         {
    //             if (currentEnergyType == badEnergies[j].getType())
    //             {
    //                 currentEnergy = badEnergies[j];
    //             }
    //         }
    //         // Debug.Log("current displacement: " + segmentOffset);
    //         Debug.Log("slider width: " + sliderWidth);
    //         currentSegment.GetComponent<RectTransform>().localPosition =
    //             Vector3.zero;
    //         // segmentOffset +=
    //         //     currentEnergy.getAmount() * (sliderWidth / maxEnergy);
    //     }
    // }
}
