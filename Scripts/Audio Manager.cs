using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        PlayMusic("Theme");
        MusicVolume(1f);
        SFXVolume(1f);
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicSounds, x => x.title == name);

        if (s == null)
            Debug.Log("Sound Not Found");
        else
        {
            musicSource.clip = s.clip;
            musicSource.Play();
        }
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.title == name);

        if (s == null)
            Debug.Log("Sound Not Found");
        else
            sfxSource.PlayOneShot(s.clip);
    }

    public void MusicVolume(float volume) => musicSource.volume = volume;
    public void SFXVolume(float volume) => sfxSource.volume = volume;
}