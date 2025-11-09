using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    private static MusicManager Instance;
    private AudioSource audioSource;
    private static MusicLibrary musicLibrary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            musicLibrary = GetComponent<MusicLibrary>();
            DontDestroyOnLoad(gameObject);

            if (audioSource != null)
            {
                audioSource.loop = true;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public static void PlayBGM(string musicID, bool resetSong = true)
    {
        if (Instance == null || musicLibrary == null)
        {
            Debug.LogError("MusicManager o MusicLibrary no están inicializados.");
            return;
        }

        AudioClip newClip = musicLibrary.GetClip(musicID);

        if (newClip != null)
        {
            if (Instance.audioSource.clip != newClip || resetSong)
            {
                Instance.audioSource.clip = newClip;
                Instance.audioSource.Play();
            }
        }
    }
    public static void PauseBGM()
    {
        Instance.audioSource.Pause();
    }
}
