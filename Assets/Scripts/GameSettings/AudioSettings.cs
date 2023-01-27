using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
//using UnityEngine.InputSystem;

public class AudioSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    private float masterVolume;
    private float musicVolume;
    private float sfxVolume;
    private bool isMusicMuted;
    private bool isSFXMuted;

    void Start()
    {
        masterVolume = 0f;
        musicVolume = 0f;
        sfxVolume = 0f;
        isMusicMuted = false;
        isSFXMuted = false;
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("masterVolume", volume);
        masterVolume = volume;
    }

    public void SetMusicVolume(float volume)
    {
        if(!isMusicMuted) audioMixer.SetFloat("musicVolume", volume);
        musicVolume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        if(!isSFXMuted) audioMixer.SetFloat("SFXVolume", volume);
        sfxVolume = volume;
    }

    public void muteMusic(bool mute)
    {
        isMusicMuted = !mute;
        audioMixer.SetFloat("musicVolume", isMusicMuted ? -80f : musicVolume);
    }

    public void muteSFX(bool mute)
    {
        isSFXMuted = !mute;
        audioMixer.SetFloat("SFXVolume", isSFXMuted ? -80f : sfxVolume);
    }
}
