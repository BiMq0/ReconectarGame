// MusicLibrary.cs

using System.Collections.Generic;
using UnityEngine;

public class MusicLibrary : MonoBehaviour
{
    [System.Serializable]
    public class MusicEntry
    {
        public string id;
        public AudioClip clip;
    }

    public List<MusicEntry> musicList;
    private Dictionary<string, AudioClip> musicDictionary = new Dictionary<string, AudioClip>();

    void Awake()
    {
        foreach (var entry in musicList)
        {
            if (!musicDictionary.ContainsKey(entry.id))
            {
                musicDictionary.Add(entry.id, entry.clip);
            }
        }
    }

    public AudioClip GetClip(string id)
    {
        if (musicDictionary.ContainsKey(id))
        {
            return musicDictionary[id];
        }
        Debug.LogWarning($"Música ID '{id}' no encontrada en la librería.");
        return null;
    }
}