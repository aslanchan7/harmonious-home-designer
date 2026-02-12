using System;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Furniture SFX Sets")]
    [SerializeField] private List<FurnitureSFXSet> furnitureSets = new();

    [Header("Global SFX")]
    [SerializeField] private GlobalSFXSet globalSfx = new();

    private readonly Dictionary<FurnitureSFXCategory, FurnitureSFXSet> setLookup = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        setLookup.Clear();
        foreach (var set in furnitureSets)
        {
            if (set == null) continue;
            // Last one wins if duplicates
            setLookup[set.category] = set;
        }

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();
    }

    // Main call for furniture-related sounds
    public void PlayFurnitureSFX(FurnitureSFXCategory category, SFXAction action)
    {
        var entry = ResolveFurnitureEntry(category, action);
        if (entry == null || entry.clip == null) return;

        PlayEntry(entry);
    }

    // For UI
    public void PlayGlobal(SFXAction action)
    {
        SFXEntry entry = action switch
        {
            SFXAction.UI_Click => globalSfx.uiClick,
            SFXAction.Invalid => globalSfx.invalid,
            _ => null
        };

        if (entry == null || entry.clip == null) return;
        PlayEntry(entry);
    }

    private SFXEntry ResolveFurnitureEntry(FurnitureSFXCategory category, SFXAction action)
    {
        // Try category set, then fallback to Default, then null
        if (!setLookup.TryGetValue(category, out var set))
            setLookup.TryGetValue(FurnitureSFXCategory.Default, out set);

        if (set == null) return null;

        return action switch
        {
            SFXAction.Place => set.place,
            SFXAction.Pickup => set.pickup,
            SFXAction.Rotate => set.rotate,
            SFXAction.Invalid => set.invalid,
            _ => null
        };
    }

    private void PlayEntry(SFXEntry entry)
    {
        // Optional pitch in semitones. If you don’t want pitch shift, leave at 0.
        float originalPitch = sfxSource.pitch;
        if (Mathf.Abs(entry.pitchSemitones) > 0.001f)
            sfxSource.pitch = Mathf.Pow(2f, entry.pitchSemitones / 12f);

        sfxSource.PlayOneShot(entry.clip, entry.volume);

        // Restore pitch
        sfxSource.pitch = originalPitch;
    }
}

// Old SFX Manager
//public class SFXManager : MonoBehaviour
//{
//    public static SFXManager Instance;

//    [SerializeField] List<SFXClip> audioClips = new();
//    [SerializeField] AudioSource audioSource;

//    void Awake()
//    {
//        if(Instance != null && Instance != this)
//        {
//            Destroy(this);
//        } else
//        {
//            Instance = this;
//        }
//    }

//    public void PlaySFX(SFXType sfxType)
//    {
//        SFXClip clip = audioClips.Find(c => c.sfxType == sfxType);

//        if(clip != null)
//        {
//            audioSource.clip = clip.Clip;
//            audioSource.Play();
//        }
//    }
//}

//[Serializable]
//public class SFXClip
//{
//    public AudioClip Clip;
//    public SFXType sfxType;

//}

//public enum SFXType
//{
//    Place_Wood
//}

