using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;public class EnergyTestButton : MonoBehaviour
{
    public UnityEngine.UIElements.Button button;
    public FSBarController FSBar;
    private static FSEnergy energy;
    public bool isGood = true;
    public string type = "";
    public int energyAmount = 10;
    public void AddEnergy()
    {
        energy = new FSEnergy(isGood, energyAmount, type);
        FSBar.AddEnergy(energy);
    }
    public void RemoveEnergy()
    {
        FSBar.RemoveEnergy(energy);
    }
}
