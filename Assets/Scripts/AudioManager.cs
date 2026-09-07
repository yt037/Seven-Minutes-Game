// AudioManager.cs
// Sits on the GameManager object and follows it between scenes. Every sound
// in the game is requested by id (see GameIds.Sfx*). The clip table is empty
// until whoever owns audio fills it; unknown ids are silently ignored.
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Serializable]
    public class Entry
    {
        public string id;
        public AudioClip clip;
    }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private Entry[] clips;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Play(string id)
    {
        AudioClip clip = Find(id);
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
    }

    public void PlayMusic(string id)
    {
        AudioClip clip = Find(id);
        if (clip == null || musicSource == null) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    private AudioClip Find(string id)
    {
        if (clips == null || string.IsNullOrEmpty(id)) return null;
        foreach (Entry e in clips) if (e != null && e.id == id) return e.clip;
        return null;
    }
}
