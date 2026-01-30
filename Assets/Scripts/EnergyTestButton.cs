using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
public class EnergyTestButton : MonoBehaviour
{
    public UnityEngine.UIElements.Button button;
    public FSBarController FSBar;
    public bool isGood = false;
    public FSEnergyType type = FSEnergyType.Chaos;
    public int energyAmount = 10;
    void Start()
    {
        isGood = false;
        type = FSEnergyType.Chaos;
        energyAmount = 10;
    }
    public void AddEnergy()
    {
        FSBar.AddEnergy(type, energyAmount, isGood);
    }
    public void RemoveEnergy()
    {
        FSBar.RemoveEnergy(type, energyAmount, isGood);
    }
}
