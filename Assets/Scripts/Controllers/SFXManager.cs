using System;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;
    
    [SerializeField] List<SFXClip> audioClips = new();
    [SerializeField] AudioSource audioSource;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        } else
        {
            Instance = this;
        }
    }

    public void PlaySFX(SFXType sfxType)
    {
        SFXClip clip = audioClips.Find(c => c.sfxType == sfxType);
        
        if(clip != null)
        {
            audioSource.clip = clip.Clip;
            audioSource.Play();
        }
    }
}

[Serializable]
public class SFXClip
{
    public AudioClip Clip;
    public SFXType sfxType;

}

public enum SFXType
{
    Place_Wood
}