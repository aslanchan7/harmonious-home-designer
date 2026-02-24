using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Feng Shui Energy Dictionary")]
public class FSEnergyDictionary : ScriptableObject
{
    public List<FSEnergyDictionaryItem> dictionary = new();
}

[Serializable]
public class FSEnergyDictionaryItem
{
    public FSEnergyType energyType;
    public Sprite energyIcon;
    public Sprite energyTooltip; 
    public Color color;
}

// Enumerator class for the types of energies
public enum FSEnergyType
{
    // Bad energies, when running addEnergy() or removeEnergy() input false for "polarity" field
    Toilet,
    Chaos,
    Death,
    Functional,
}
