using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography.X509Certificates;
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
    public SFXEntry uiMenuHover1;
    public SFXEntry uiMenuHover2;
    public SFXEntry uiMenuHover3;
    public SFXEntry uiMenuHover4;
    public SFXEntry uiMenuHover5;
    public SFXEntry uiOpen;
    public SFXEntry uiClose;
    public SFXEntry uiCabinetOpen;
    public SFXEntry uiCabinetClose;
    public SFXEntry uiBookOpen;
    public SFXEntry uiPageForward;
    public SFXEntry uiPageBack;
    public SFXEntry uiBookClose;
    public SFXEntry uiBoxOpen;
    public SFXEntry uiBoxClose;
    public SFXEntry invalid;
    public SFXEntry deleteItem;
    public SFXEntry uiWin;
}