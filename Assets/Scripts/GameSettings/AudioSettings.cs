using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class AudioSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public SettingsVariable settings;

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("masterVolume", volume);
        settings.masterVolume = volume;
    }

    public void SetMusicVolume(float volume)
    {
        if(!settings.isMusicMuted) audioMixer.SetFloat("musicVolume", volume);
        settings.musicVolume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        if(!settings.isSFXMuted) audioMixer.SetFloat("SFXVolume", volume);
        settings.sfxVolume = volume;
    }

    public void muteMusic(bool mute)
    {
        settings.isMusicMuted = !mute;
        audioMixer.SetFloat("musicVolume", settings.isMusicMuted ? -80f : settings.musicVolume);
    }

    public void muteSFX(bool mute)
    {
        settings.isSFXMuted = !mute;
        audioMixer.SetFloat("SFXVolume", settings.isSFXMuted ? -80f : settings.sfxVolume);
    }
}
