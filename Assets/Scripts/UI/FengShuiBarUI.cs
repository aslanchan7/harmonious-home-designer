using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FengShuiBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private EnergySegmentController _EnergySegmentPrefab;
    public Slider sinSlider;

    [Header("Data")]
    private List<GameObject> BadEnergySegments = new();

    // Creates an instance of the energySegment prefab of the desired type
    public void CreateEnergySegment(FSEnergyType type, bool polarity, int maxE)
    {
        EnergySegmentController newEnergySegment =
            _EnergySegmentPrefab.InstantiatePrefab(sinSlider.transform, maxE);
        RectTransform rectTransform =
            newEnergySegment.gameObject.GetComponent<RectTransform>();
        newEnergySegment.SetEnergyType(type);
        newEnergySegment.SetPolarity(polarity);
        newEnergySegment.SetSize(rectTransform.sizeDelta);
        rectTransform.localPosition = Vector3.zero;
        // newEnergySegment.gameObject.SetActive(false);
        if (!polarity)
        {
            BadEnergySegments.Add(newEnergySegment.gameObject);
        }
        else
        {
            Debug.Log(
                "Not segmenting good energies at this point in development :/"
            );
        }
    }

    // Removes the sin segment of a desired type (only called if amount of that type of sin = 0)
    public void RemoveEnergySement(FSEnergyType type, bool polarity)
    {
        GameObject removedSegment;
        if (polarity)
        {
            Debug.Log(
                "Not segmenting good energies at this point in development :/"
            );
        }
        else
        {
            for (int i = 0; i < BadEnergySegments.Count; i++)
            {
                removedSegment = BadEnergySegments[i];
                if (
                    removedSegment
                        .GetComponent<EnergySegmentController>()
                        .GetEnergyType()
                        .Equals(type)
                )
                {
                    Destroy(BadEnergySegments[i]);
                    BadEnergySegments.Remove(removedSegment);
                    Debug.Log("Successfully destroyed segment");
                }
            }
        }
    }

    // Updates the value for each sin segment
    public void UpdateEnergySegmentValues(List<FSEnergy> badEnergies)
    {
        for (int i = 0; i < BadEnergySegments.Count; i++)
        {
            GameObject currentSegment = BadEnergySegments[i];
            FSEnergyType currentEnergyType = currentSegment
                .GetComponent<EnergySegmentController>()
                .GetEnergyType();
            for (int j = 0; j < badEnergies.Count; j++)
            {
                int currentSourceValue = badEnergies[j].getAmount();
                if (badEnergies[j].getType().Equals(currentEnergyType))
                {
                    currentSegment
                        .GetComponent<EnergySegmentController>()
                        .SetValue(currentSourceValue);
                }
            }
        }
    }

    // TODO: THIS JUST STRAIGHT UP DOESN'T WORK
    // Updates the position and slider graphic for each sin type
    public void UpdateEnergySegmentGraphics(
        RectTransform sinsSlider,
        List<FSEnergy> badEnergies,
        float maxEnergy
    )
    {
        float segmentOffset = 0.0f;
        float sliderPosX = sinsSlider.localPosition.x;
        for (int i = 0; i < BadEnergySegments.Count; i++)
        {
            GameObject currentSegment = BadEnergySegments[i];
            FSEnergyType currentEnergyType = BadEnergySegments[i]
                .GetComponent<EnergySegmentController>()
                .GetEnergyType();
            FSEnergy currentEnergy = null;
            float sliderWidth = sinsSlider.sizeDelta.x;
            Vector3 displacement = new(sliderPosX - segmentOffset, 0, 0);
            for (int j = 0; j < badEnergies.Count; j++)
            {
                if (currentEnergyType == badEnergies[j].getType())
                {
                    currentEnergy = badEnergies[j];
                }
            }
            Debug.Log("current displacement: " + segmentOffset);
            Debug.Log("slider width: " + sliderWidth);
            currentSegment.GetComponent<RectTransform>().localPosition =
                displacement;
            segmentOffset +=
                currentEnergy.getAmount() * (sliderWidth / maxEnergy);
        }
    }
}
