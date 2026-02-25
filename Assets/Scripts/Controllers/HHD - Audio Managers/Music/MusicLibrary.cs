using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SceneMusicEntry
{
    public string sceneName;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = true;
}

[CreateAssetMenu(menuName = "Audio/Music Library")]
public class MusicLibrary : ScriptableObject
{
    public List<SceneMusicEntry> entries = new();

    public bool TryGet(string sceneName, out SceneMusicEntry entry)
    {
        entry = entries.Find(e => e.sceneName == sceneName);
        return entry != null && entry.clip != null;
    }
}