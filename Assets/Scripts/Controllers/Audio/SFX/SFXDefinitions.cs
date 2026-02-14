using System;
using UnityEngine;

[Serializable]
public class SFXEntry
{
    public string key;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    // Pitch shift
    [Range(-12f, 12f)] public float pitchSemitones = 0f;

}

[Serializable]
public class FurnitureSFXSet
{
    public FurnitureSFXCategory category = FurnitureSFXCategory.Default;
    public SFXEntry place;
    public SFXEntry pickup;
    public SFXEntry rotate;
    public SFXEntry invalid;
}

[Serializable]
public class GlobalSFXSet
{
    public SFXEntry uiClick;
    public SFXEntry uiOpen;
    public SFXEntry uiClose;
    public SFXEntry uiPaper;
    public SFXEntry uiBoxOpen;
    public SFXEntry invalid;
}